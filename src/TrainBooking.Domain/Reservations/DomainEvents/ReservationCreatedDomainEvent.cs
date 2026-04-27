using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.Reservations.DomainEvents;

public sealed record ReservationCreatedDomainEvent(Guid ReservationId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
