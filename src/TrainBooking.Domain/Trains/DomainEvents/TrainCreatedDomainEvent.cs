using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.Trains.DomainEvents;

public sealed record TrainCreatedDomainEvent(
    Guid TrainId,
    string Name) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
