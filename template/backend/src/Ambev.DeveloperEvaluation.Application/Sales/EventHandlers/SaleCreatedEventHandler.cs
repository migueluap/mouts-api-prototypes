using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

/// <summary>
/// Handler for SaleCreatedEvent.
/// Logs the event when a sale is created.
/// </summary>
public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of SaleCreatedEventHandler.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> _logger)
    {
        this._logger = _logger;
    }

    /// <summary>
    /// Handles the SaleCreatedEvent by logging it.
    /// </summary>
    /// <param name="notification">The event notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DOMAIN EVENT] SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}, Customer={CustomerName}, " +
            "Branch={BranchName}, TotalAmount={TotalAmount:C}, ItemCount={ItemCount}, EventId={EventId}, OccurredOn={OccurredOn}",
            notification.SaleId,
            notification.SaleNumber,
            notification.CustomerName,
            notification.BranchName,
            notification.TotalAmount,
            notification.ItemCount,
            notification.EventId,
            notification.OccurredOn);

        return Task.CompletedTask;
    }
}
