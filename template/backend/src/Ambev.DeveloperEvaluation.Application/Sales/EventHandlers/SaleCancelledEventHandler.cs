using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

/// <summary>
/// Handler for SaleCancelledEvent.
/// Logs the event when a sale is cancelled.
/// </summary>
public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaleCancelledEventHandler.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the SaleCancelledEvent by logging it.
    /// </summary>
    /// <param name="notification">The event notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "[DOMAIN EVENT] SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}, " +
            "CancelledAmount={CancelledAmount:C}, Reason={CancellationReason}, " +
            "EventId={EventId}, OccurredOn={OccurredOn}",
            notification.SaleId,
            notification.SaleNumber,
            notification.CancelledAmount,
            notification.CancellationReason ?? "Not specified",
            notification.EventId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
