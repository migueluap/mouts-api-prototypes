using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Command for physically deleting a sale from the database.
/// </summary>
/// <remarks>
/// Note: For business purposes, consider using CancelSale instead of DeleteSale
/// to maintain audit trail and historical data.
/// </remarks>
public record DeleteSaleCommand : IRequest
{
    /// <summary>
    /// Gets the unique identifier of the sale to delete.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Initializes a new instance of DeleteSaleCommand.
    /// </summary>
    /// <param name="saleId">The ID of the sale to delete</param>
    public DeleteSaleCommand(Guid saleId)
    {
        SaleId = saleId;
    }
}
