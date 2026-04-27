using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.Users.DomainEvents;

public sealed record UserCreatedDomainEvent(
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
