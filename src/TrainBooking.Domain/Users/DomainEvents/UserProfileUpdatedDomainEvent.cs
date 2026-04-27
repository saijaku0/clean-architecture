using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Domain.Users.DomainEvents;

public sealed record UserProfileUpdatedDomainEvent(
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
