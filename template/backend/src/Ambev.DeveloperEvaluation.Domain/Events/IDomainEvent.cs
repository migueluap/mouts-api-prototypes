using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// </summary>
/// <remarks>
/// Domain events are implemented using MediatR notifications for in-process event handling.
/// This allows decoupling of event publishers from event subscribers.
/// </remarks>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; }
}
