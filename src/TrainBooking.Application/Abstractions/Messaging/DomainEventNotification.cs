using TrainBooking.Application.Abstractions.Messaging.Interfaces;
using TrainBooking.Domain.Common.DomainEvents;

namespace TrainBooking.Application.Abstractions.Messaging;

/// <summary>
/// Represents a domain event notification that encapsulates a domain event instance.
/// </summary>
/// <remarks>
/// Used to propagate domain events through publishing and handling mechanisms.
/// </remarks>
/// <typeparam name="TDomainEvent">
/// The specific domain event type implementing IDomainEvent.
/// </typeparam>
/// <param name="DomainEvent">
/// The encapsulated domain event.
/// </param>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent)
    : IDomainEventNotification<TDomainEvent>
    where TDomainEvent : IDomainEvent;
