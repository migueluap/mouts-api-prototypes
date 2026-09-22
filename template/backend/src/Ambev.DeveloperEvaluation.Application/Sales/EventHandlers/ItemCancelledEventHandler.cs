using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

/// <summary>
/// Handler for ItemCancelledEvent.
/// Logs the event when an individual item within a sale is cancelled.
/// </summary>
public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of ItemCancelledEventHandler.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the ItemCancelledEvent by logging it.
    /// </summary>
    /// <param name="notification">The event notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "[DOMAIN EVENT] ItemCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}, ItemId={ItemId}, " +
            "Product={ProductName}, CancelledQuantity={CancelledQuantity}, CancelledAmount={CancelledAmount:C}, " +
            "Reason={CancellationReason}, EventId={EventId}, OccurredOn={OccurredOn}",
            notification.SaleId,
            notification.SaleNumber,
            notification.ItemId,
            notification.ProductName,
            notification.CancelledQuantity,
            notification.CancelledAmount,
            notification.CancellationReason ?? "Not specified",
            notification.EventId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
