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
    /// Gets the row version for optimistic concurrency control.
    /// Must match the current version in the database to prevent concurrent modifications.
    /// </summary>
    public byte[] RowVersion { get; }

    /// <summary>
    /// Initializes a new instance of CancelSaleCommand.
    /// </summary>
    /// <param name="saleId">The ID of the sale to cancel</param>
    /// <param name="rowVersion">The row version for concurrency control</param>
    public CancelSaleCommand(Guid saleId, byte[] rowVersion)
    {
        SaleId = saleId;
        RowVersion = rowVersion ?? Array.Empty<byte>();
    }
}
