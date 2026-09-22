using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when an individual item within a sale is cancelled.
/// This allows for partial cancellations without cancelling the entire sale.
/// </summary>
public class ItemCancelledEvent : DomainEventBase
{
    /// <summary>
    /// Gets the ID of the sale containing the cancelled item.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Gets the sale number.
    /// </summary>
    public string SaleNumber { get; }

    /// <summary>
    /// Gets the ID of the cancelled item.
    /// </summary>
    public Guid ItemId { get; }

    /// <summary>
    /// Gets the product name of the cancelled item.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the quantity that was cancelled.
    /// </summary>
    public int CancelledQuantity { get; }

    /// <summary>
    /// Gets the amount that was cancelled.
    /// </summary>
    public decimal CancelledAmount { get; }

    /// <summary>
    /// Gets the reason for item cancellation (optional).
    /// </summary>
    public string? CancellationReason { get; }

    /// <summary>
    /// Initializes a new instance of ItemCancelledEvent.
    /// </summary>
    /// <param name="sale">The sale containing the cancelled item</param>
    /// <param name="cancelledItem">The item that was cancelled</param>
    /// <param name="cancellationReason">Optional reason for the cancellation</param>
    public ItemCancelledEvent(Sale sale, SaleItem cancelledItem, string? cancellationReason = null)
    {
        SaleId = sale.Id;
        SaleNumber = sale.SaleNumber;
        ItemId = cancelledItem.Id;
        ProductName = cancelledItem.ProductName;
        CancelledQuantity = cancelledItem.Quantity;
        CancelledAmount = cancelledItem.TotalAmount;
        CancellationReason = cancellationReason;
    }
}
