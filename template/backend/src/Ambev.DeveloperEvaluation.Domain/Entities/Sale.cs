using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sales transaction in the system.
/// Aggregates multiple SaleItems and applies business rules for discounts.
/// Follows Domain-Driven Design principles with the Sale as an aggregate root.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique sale number for this transaction.
    /// Used for human-readable identification and tracking.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the sale was made.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the customer ID (External Identity pattern).
    /// References a customer from the Customers domain without direct foreign key.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name (denormalized for External Identity pattern).
    /// Stored locally to avoid cross-domain joins while maintaining DDD boundaries.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the branch ID where the sale was made (External Identity pattern).
    /// References a branch from the Branches domain without direct foreign key.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the branch name (denormalized for External Identity pattern).
    /// Stored locally to avoid cross-domain joins while maintaining DDD boundaries.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the total amount of the sale after all discounts.
    /// Sum of all non-cancelled SaleItem.TotalAmount values.
    /// Calculated automatically when items are added, removed, or modified.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets whether this sale has been cancelled.
    /// When a sale is cancelled, it cannot be modified further.
    /// Use Cancel() method to cancel a sale.
    /// </summary>
    public bool Cancelled { get; private set; }

    /// <summary>
    /// Gets the date and time when this sale was cancelled.
    /// Null if the sale has not been cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Gets the date and time when the sale was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time of the last update to the sale.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Private backing field for the items collection.
    /// Prevents direct external modification of the collection.
    /// </summary>
    private readonly List<SaleItem> _items = new();

    /// <summary>
    /// Gets the read-only collection of items in this sale.
    /// Items can only be added/removed through aggregate methods (AddItem, RemoveItem, etc.).
    /// This ensures the aggregate root maintains control over its invariants.
    /// EF Core writes directly to the backing field _items when loading from database.
    /// </summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the Sale class.
    /// Sets the creation timestamp to current UTC time.
    /// </summary>
    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        Date = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an item to the sale and recalculates totals.
    /// </summary>
    /// <param name="item">The sale item to add</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to add items to a cancelled sale</exception>
    public void AddItem(SaleItem item)
    {
        if (Cancelled)
            throw new InvalidOperationException("Cannot add items to a cancelled sale");

        item.SaleId = Id;
        item.CalculateTotals();
        _items.Add(item);
        CalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an item from the sale and recalculates totals.
    /// </summary>
    /// <param name="itemId">The ID of the item to remove</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to remove items from a cancelled sale</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the item is not found in the sale</exception>
    public void RemoveItem(Guid itemId)
    {
        if (Cancelled)
            throw new InvalidOperationException("Cannot remove items from a cancelled sale");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new KeyNotFoundException($"Sale item with ID {itemId} not found in this sale");

        _items.Remove(item);
        CalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the quantity of an existing item and recalculates totals.
    /// </summary>
    /// <param name="itemId">The ID of the item to update</param>
    /// <param name="newQuantity">The new quantity (must be between 1 and 20)</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to update items in a cancelled sale</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the item is not found in the sale</exception>
    /// <exception cref="ArgumentException">Thrown when quantity is invalid</exception>
    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        if (Cancelled)
            throw new InvalidOperationException("Cannot update items in a cancelled sale");

        if (newQuantity < 1 || newQuantity > 20)
            throw new ArgumentException("Quantity must be between 1 and 20", nameof(newQuantity));

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new KeyNotFoundException($"Sale item with ID {itemId} not found in this sale");

        item.UpdateQuantity(newQuantity);
        CalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels an individual item within the sale.
    /// The sale total is recalculated excluding the cancelled item.
    /// </summary>
    /// <param name="itemId">The ID of the item to cancel</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to cancel items in an already cancelled sale</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the item is not found in the sale</exception>
    public void CancelItem(Guid itemId)
    {
        if (Cancelled)
            throw new InvalidOperationException("Cannot cancel items in an already cancelled sale");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new KeyNotFoundException($"Sale item with ID {itemId} not found in this sale");

        item.Cancel();
        CalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the entire sale and all its items.
    /// Once cancelled, no further modifications are allowed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to cancel an already cancelled sale</exception>
    public void Cancel()
    {
        if (Cancelled)
            throw new InvalidOperationException("Sale is already cancelled");

        // Cancel all items
        foreach (var item in _items.Where(i => !i.Cancelled))
        {
            item.Cancel();
        }

        Cancelled = true;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the total amount of the sale.
    /// Sums all non-cancelled items' total amounts.
    /// </summary>
    private void CalculateTotalAmount()
    {
        TotalAmount = _items
            .Where(item => !item.Cancelled)
            .Sum(item => item.TotalAmount);
    }

    /// <summary>
    /// Performs validation of the sale entity using the SaleValidator rules.
    /// </summary>
    /// <returns>
    /// A <see cref="ValidationResultDetail"/> containing:
    /// - IsValid: Indicates whether all validation rules passed
    /// - Errors: Collection of validation errors if any rules failed
    /// </returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
