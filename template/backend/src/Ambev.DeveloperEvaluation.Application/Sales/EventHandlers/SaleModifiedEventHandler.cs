using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

/// <summary>
/// Handler for SaleModifiedEvent.
/// Logs the event when a sale is modified.
/// </summary>
public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaleModifiedEventHandler.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the SaleModifiedEvent by logging it.
    /// </summary>
    /// <param name="notification">The event notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DOMAIN EVENT] SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}, " +
            "Modification={ModificationDescription}, NewTotalAmount={NewTotalAmount:C}, NewItemCount={NewItemCount}, " +
            "EventId={EventId}, OccurredOn={OccurredOn}",
            notification.SaleId,
            notification.SaleNumber,
            notification.ModificationDescription,
            notification.NewTotalAmount,
            notification.NewItemCount,
            notification.EventId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
