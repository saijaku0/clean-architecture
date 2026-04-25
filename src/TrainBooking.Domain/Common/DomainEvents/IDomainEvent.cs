namespace TrainBooking.Domain.Common.DomainEvents;

/// <summary>
/// Marker interface for domain events.
/// </summary>
/// <remarks>
/// Intended for marking and discovering domain events; implementations are used
/// to publish and handle domain events within the application.
/// </remarks>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
