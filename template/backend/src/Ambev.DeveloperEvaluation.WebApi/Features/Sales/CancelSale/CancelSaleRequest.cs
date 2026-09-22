namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

/// <summary>
/// Request model for cancelling a sale.
/// </summary>
public class CancelSaleRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the sale to cancel.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// Must match the current version in the database to prevent concurrent modifications.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
