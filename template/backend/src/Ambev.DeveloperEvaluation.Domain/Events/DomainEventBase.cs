namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Base class for domain events with common properties.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Initializes a new instance of the domain event.
    /// </summary>
    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
