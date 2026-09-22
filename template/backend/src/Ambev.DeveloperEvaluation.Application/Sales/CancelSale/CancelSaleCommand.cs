using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Command for cancelling a sale.
/// Once cancelled, a sale cannot be modified anymore.
/// </summary>
public record CancelSaleCommand : IRequest<CancelSaleResult>
{
    /// <summary>
    /// Gets the unique identifier of the sale to cancel.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Initializes a new instance of CancelSaleCommand.
    /// </summary>
    /// <param name="saleId">The ID of the sale to cancel</param>
    public CancelSaleCommand(Guid saleId)
    {
        SaleId = saleId;
    }
}
