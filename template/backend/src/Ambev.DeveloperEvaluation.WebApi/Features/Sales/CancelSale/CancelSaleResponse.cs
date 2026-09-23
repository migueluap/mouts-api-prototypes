namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

/// <summary>
/// API response model for CancelSale operation.
/// </summary>
public class CancelSaleResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the cancelled sale.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sale number.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cancellation timestamp.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets whether the sale is cancelled (should always be true).
    /// </summary>
    public bool Cancelled { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// This updated value can be used if the sale needs to be undeleted or further modified.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
