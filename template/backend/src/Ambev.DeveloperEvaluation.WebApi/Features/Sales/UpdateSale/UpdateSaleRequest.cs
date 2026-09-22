namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// API request model for updating a sale.
/// </summary>
public class UpdateSaleRequest
{
    /// <summary>
    /// Gets or sets the ID of the sale to update.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// Must match the current version in the database to prevent lost updates.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the list of items to update.
    /// </summary>
    public List<UpdateSaleItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Represents an item update within a sale.
/// </summary>
public class UpdateSaleItemRequest
{
    /// <summary>
    /// Gets or sets the ID of the item to update (empty GUID for new items).
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the product ID (required for new items).
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name (required for new items).
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price (required for new items).
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the discount percentage.
    /// </summary>
    public decimal Discount { get; set; }
}
