using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a line item within a sale transaction.
/// Each SaleItem contains product information, quantity, pricing, and discount details.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the sale this item belongs to.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the parent Sale.
    /// </summary>
    public Sale Sale { get; set; } = null!;

    /// <summary>
    /// Gets or sets the product ID (External Identity pattern).
    /// References a product from the Products domain without direct foreign key.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name (denormalized for External Identity pattern).
    /// Stored locally to avoid cross-domain joins while maintaining DDD boundaries.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the quantity of items purchased.
    /// Must be between 1 and 20 (business rule: maximum 20 identical items).
    /// Use UpdateQuantity() method to modify this value.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price of the product at the time of sale.
    /// Captures historical pricing for audit purposes.
    /// Set only during item creation.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the discount percentage applied to this item (0-100).
    /// Calculated automatically based on quantity:
    /// - 0% for quantities less than 4
    /// - 10% for quantities 4-9
    /// - 20% for quantities 10-20
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Gets the total amount for this line item after discount.
    /// Calculated automatically as: (UnitPrice * Quantity) * (1 - Discount/100)
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets whether this item has been cancelled.
    /// Allows individual item cancellation without cancelling the entire sale.
    /// Use Cancel() method to cancel an item.
    /// </summary>
    public bool Cancelled { get; private set; }

    /// <summary>
    /// Gets the date and time when this item was cancelled.
    /// Null if the item has not been cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// This is automatically managed by the database and EF Core.
    /// Used to detect concurrent modifications to prevent lost updates.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Calculates the appropriate discount percentage based on quantity.
    /// Business Rules:
    /// - Less than 4 items: 0% discount
    /// - 4 to 9 items: 10% discount
    /// - 10 to 20 items: 20% discount
    /// - More than 20 items: Not allowed (validation will fail)
    /// </summary>
    /// <returns>The discount percentage (0, 10, or 20)</returns>
    public decimal CalculateDiscount()
    {
        if (Quantity < 4)
            return 0;

        if (Quantity >= 4 && Quantity <= 9)
            return 10;

        if (Quantity >= 10 && Quantity <= 20)
            return 20;

        // This shouldn't happen due to validation, but return 0 as safe default
        return 0;
    }

    /// <summary>
    /// Initializes a new instance of SaleItem with required values.
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="productName">The product name</param>
    /// <param name="quantity">The quantity (1-20)</param>
    /// <param name="unitPrice">The unit price</param>
    public static SaleItem Create(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity < 1 || quantity > 20)
            throw new ArgumentException("Quantity must be between 1 and 20", nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than 0", nameof(unitPrice));

        var item = new SaleItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        item.CalculateTotals();
        return item;
    }

    /// <summary>
    /// Updates the quantity of this item and recalculates totals.
    /// </summary>
    /// <param name="newQuantity">The new quantity (must be between 1 and 20)</param>
    /// <exception cref="ArgumentException">Thrown when quantity is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when trying to update a cancelled item</exception>
    public void UpdateQuantity(int newQuantity)
    {
        if (Cancelled)
            throw new InvalidOperationException("Cannot update quantity of a cancelled item");

        if (newQuantity < 1 || newQuantity > 20)
            throw new ArgumentException("Quantity must be between 1 and 20", nameof(newQuantity));

        Quantity = newQuantity;
        CalculateTotals();
    }

    /// <summary>
    /// Calculates and updates the discount and total amount for this item.
    /// Called internally whenever quantity changes.
    /// </summary>
    internal void CalculateTotals()
    {
        Discount = CalculateDiscount();
        var subtotal = UnitPrice * Quantity;
        TotalAmount = subtotal * (1 - Discount / 100);
    }

    /// <summary>
    /// Cancels this sale item.
    /// Sets the Cancelled flag and records the cancellation timestamp.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to cancel an already cancelled item</exception>
    public void Cancel()
    {
        if (Cancelled)
            throw new InvalidOperationException("Item is already cancelled");

        Cancelled = true;
        CancelledAt = DateTime.UtcNow;
    }
}
