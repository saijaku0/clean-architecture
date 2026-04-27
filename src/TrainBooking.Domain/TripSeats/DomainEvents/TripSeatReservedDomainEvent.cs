using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.TripSeats.DomainEvents;

public sealed record TripSeatReservedDomainEvent(Guid TripSeatId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
