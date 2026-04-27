using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.Trips.DomainEvent;

public sealed record TripCreatedDomainEvent(Guid TripId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
