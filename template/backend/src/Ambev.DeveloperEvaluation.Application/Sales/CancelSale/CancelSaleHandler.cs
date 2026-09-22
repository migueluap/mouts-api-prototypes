using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Handler for processing CancelSaleCommand requests.
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of CancelSaleHandler.
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="publisher">The MediatR publisher for domain events</param>
    public CancelSaleHandler(ISaleRepository saleRepository, IMapper mapper, IPublisher publisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    /// <summary>
    /// Handles the CancelSaleCommand request.
    /// </summary>
    /// <param name="command">The cancel command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cancellation result</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the sale is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when the sale is already cancelled</exception>
    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);

        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found");

        // Cancel the sale using the domain method (this throws if already cancelled)
        sale.Cancel();

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
        var saleCancelledEvent = new SaleCancelledEvent(updatedSale, "Sale cancelled by user request");
        await _publisher.Publish(saleCancelledEvent, cancellationToken);

        // Map to result
        return _mapper.Map<CancelSaleResult>(updatedSale);
    }
}
