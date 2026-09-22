using AutoMapper;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of UpdateSaleHandler.
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="publisher">The MediatR publisher for domain events</param>
    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    /// <summary>
    /// Handles the UpdateSaleCommand request.
    /// </summary>
    /// <param name="command">The update command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sale details</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the sale is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when trying to update a cancelled sale</exception>
    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Retrieve the existing sale
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found");

        if (sale.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale");

        // Get existing item IDs
        var existingItemIds = sale.Items.Select(i => i.Id).ToHashSet();
        var commandItemIds = command.Items.Where(i => i.ItemId != Guid.Empty).Select(i => i.ItemId).ToHashSet();

        // Remove items that are not in the command and publish events
        var itemsToRemove = existingItemIds.Except(commandItemIds).ToList();
        foreach (var itemId in itemsToRemove)
        {
            var itemToRemove = sale.Items.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                // Publish ItemCancelledEvent before removing
                var itemCancelledEvent = new ItemCancelledEvent(sale, itemToRemove, "Item removed during sale update");
                await _publisher.Publish(itemCancelledEvent, cancellationToken);
            }

            sale.RemoveItem(itemId);
        }

        // Update or add items
        foreach (var itemCommand in command.Items)
        {
            if (itemCommand.ItemId == Guid.Empty)
            {
                // Add new item
                var newItem = SaleItem.Create(
                    itemCommand.ProductId,
                    itemCommand.ProductName,
                    itemCommand.Quantity,
                    itemCommand.UnitPrice
                );
                sale.AddItem(newItem);
            }
            else
            {
                // Update existing item quantity
                sale.UpdateItemQuantity(itemCommand.ItemId, itemCommand.Quantity);
            }
        }

        // Validate the domain entity
        var domainValidation = sale.Validate();
        if (!domainValidation.IsValid)
        {
            var errors = domainValidation.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure(e.Error, e.Detail));
            throw new ValidationException(errors);
        }

        // Persist the changes with optimistic concurrency control
        Sale updatedSale;
        try
        {
            // Set the RowVersion from the command to enable concurrency check
            sale.RowVersion = command.RowVersion;
            updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "The sale was modified by another user. Please reload the sale and try again.");
        }

        // Publish domain event
        var modificationsDescription = BuildModificationDescription(itemsToRemove.Count, command.Items.Count);
        var saleModifiedEvent = new SaleModifiedEvent(updatedSale, modificationsDescription);
        await _publisher.Publish(saleModifiedEvent, cancellationToken);

        // Map to result
        var result = _mapper.Map<UpdateSaleResult>(updatedSale);
        return result;
    }

    /// <summary>
    /// Builds a description of the modifications made to the sale.
    /// </summary>
    private string BuildModificationDescription(int removedCount, int totalItemsInCommand)
    {
        var parts = new List<string>();

        if (removedCount > 0)
            parts.Add($"{removedCount} item(s) removed");

        parts.Add($"Updated to {totalItemsInCommand} item(s)");

        return string.Join(", ", parts);
    }
}
