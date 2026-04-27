using MediatR;
using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Application.Abstractions.Messaging.Interfaces;

/// <summary>
/// Domain event notification that implements INotification and contains a domain event instance.
/// </summary>
/// <remarks>
/// Supports covariance to safely pass more specific domain event types
/// to notification handlers.
/// </remarks>
/// <typeparam name="TDomainEvent">
/// The type of the domain event, implementing IDomainEvent.
/// </typeparam>
public interface IDomainEventNotification<out TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    TDomainEvent DomainEvent { get; }
}
