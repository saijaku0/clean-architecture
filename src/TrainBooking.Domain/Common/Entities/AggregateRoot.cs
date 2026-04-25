using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.Common.Entities;

/// <summary>
/// Abstract base class for a domain aggregate that provides a unique identifier and built-in
/// support for collecting and managing domain events.
/// </summary>
/// <remarks>
/// A protected parameterless constructor generates a version 7 GUID.
/// The aggregate maintains an internal list of domain events, exposed as an IReadOnlyCollection
/// via the DomainEvents property; AddDomainEvent validates that the argument is not null and
/// adds the event; RemoveDomainEvent clears the collection.
/// </remarks>
/// <param name="id">The aggregate identifier (Guid).</param>
public abstract class AggregateRoot(Guid id) : EntityBase(id)
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot() : this(Guid.CreateVersion7()) { }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
    protected void RemoveDomainEvent() => _domainEvents.Clear();
}

