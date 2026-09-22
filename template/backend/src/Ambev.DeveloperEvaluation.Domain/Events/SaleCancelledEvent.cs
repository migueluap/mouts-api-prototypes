using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is cancelled.
/// This event can be used for triggering refund processes, notifications, or inventory adjustments.
/// </summary>
public class SaleCancelledEvent : DomainEventBase
{
    /// <summary>
    /// Gets the ID of the cancelled sale.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Gets the sale number.
    /// </summary>
    public string SaleNumber { get; }

    /// <summary>
    /// Gets the total amount that was cancelled.
    /// </summary>
    public decimal CancelledAmount { get; }

    /// <summary>
    /// Gets the reason for cancellation (optional).
    /// </summary>
    public string? CancellationReason { get; }

    /// <summary>
    /// Initializes a new instance of SaleCancelledEvent.
    /// </summary>
    /// <param name="sale">The sale that was cancelled</param>
    /// <param name="cancellationReason">Optional reason for the cancellation</param>
    public SaleCancelledEvent(Sale sale, string? cancellationReason = null)
    {
        SaleId = sale.Id;
        SaleNumber = sale.SaleNumber;
        CancelledAmount = sale.TotalAmount;
        CancellationReason = cancellationReason;
    }
}
