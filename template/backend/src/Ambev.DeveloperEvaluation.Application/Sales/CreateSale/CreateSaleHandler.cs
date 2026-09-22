using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing CreateSaleCommand requests.
/// Implements the business logic for creating a new sale with items.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of CreateSaleHandler.
    /// </summary>
    /// <param name="saleRepository">The sale repository for data access</param>
    /// <param name="mapper">The AutoMapper instance for object mapping</param>
    /// <param name="publisher">The MediatR publisher for domain events</param>
    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    /// <summary>
    /// Handles the CreateSaleCommand request.
    /// </summary>
    /// <param name="command">The CreateSale command containing sale data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when sale number already exists</exception>
    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check if sale number already exists
        var existingSale = await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existingSale != null)
            throw new InvalidOperationException($"Sale with number {command.SaleNumber} already exists");

        // Create the sale entity
        var sale = new Sale
        {
            SaleNumber = command.SaleNumber,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName,
            Date = command.Date
        };

        // Add items to the sale using the aggregate root method
        foreach (var itemCommand in command.Items)
        {
            var saleItem = SaleItem.Create(
                itemCommand.ProductId,
                itemCommand.ProductName,
                itemCommand.Quantity,
                itemCommand.UnitPrice
            );

            sale.AddItem(saleItem);
        }

        // Validate the domain entity
        var domainValidation = sale.Validate();
        if (!domainValidation.IsValid)
        {
            var errors = domainValidation.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure(e.Error, e.Detail));
            throw new ValidationException(errors);
        }

        // Persist the sale
        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        // Publish domain event
        var saleCreatedEvent = new SaleCreatedEvent(createdSale);
        await _publisher.Publish(saleCreatedEvent, cancellationToken);

        // Map to result
        var result = _mapper.Map<CreateSaleResult>(createdSale);
        return result;
    }
}
