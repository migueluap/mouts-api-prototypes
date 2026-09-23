namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// API response model for UpdateSale operation.
/// </summary>
public class UpdateSaleResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the updated sale.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sale number.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated total amount.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the list of updated sale items.
    /// </summary>
    public List<UpdateSaleItemResponse> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// This updated value must be sent back in subsequent updates or cancel operations.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Represents a sale item in the update response.
/// </summary>
public class UpdateSaleItemResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the sale item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage.
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this line item.
    /// </summary>
    public decimal TotalAmount { get; set; }
}
