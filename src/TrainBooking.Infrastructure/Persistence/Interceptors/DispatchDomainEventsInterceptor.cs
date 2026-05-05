using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrainBooking.Application.Abstractions.Messaging;
using TrainBooking.Domain.Common.DomainEvents;
using TrainBooking.Domain.Common.Entities;

namespace TrainBooking.Infrastructure.Persistence.Interceptors;

public sealed class DispatchDomainEventsInterceptor(IPublisher publisher)
        : SaveChangesInterceptor
{
    private static readonly Action<AggregateRoot> _clearDomainEventsDelegate;

    /// <summary>
    /// Initializes a delegate for invoking the hidden domain event removal method
    /// of <see cref="AggregateRoot"/> using reflection.
    /// </summary>
    /// <remarks>
    /// Used to bypass AggregateRoot encapsulation without changing its public API.
    ///
    /// IMPORTANT:
    /// - Depends on the existence of a method named <c>RemoveDomainEvent</c>.
    /// - The method must be non-static and have a compatible signature.
    /// - Any change to the method name or signature will result in a runtime error.
    ///
    /// The static constructor executes once when the type is first loaded.
    ///
    /// If the method cannot be found, an <see cref="InvalidOperationException"/> is thrown,
    /// which is considered a critical configuration error.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <c>RemoveDomainEvent</c> method is not found or cannot be bound.
    /// </exception>
    static DispatchDomainEventsInterceptor()
    {
        MethodInfo? methodInfo = typeof(AggregateRoot).GetMethod(
            "RemoveDomainEvent",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Critical error: Cannot finde 'RemoveDomainEvent' method in AggregateRoot.");
        _clearDomainEventsDelegate = (Action<AggregateRoot>)Delegate.CreateDelegate(
            typeof(Action<AggregateRoot>),
            methodInfo);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return result;

        await DispatchDomainEventsAsync(eventData.Context, ct);

        return result;
    }

    private async Task DispatchDomainEventsAsync(DbContext dbContext, CancellationToken ct)
    {
        List<IDomainEvent> processedDomainEvents = [];
        List<IDomainEvent> unprocessedDomainEvents = GetDomainEvents(dbContext);

        while (unprocessedDomainEvents.Count != 0)
        {
            var eventsToDispatch = unprocessedDomainEvents.ToList();
            ClearDomainEvents(dbContext);
            await DispatchDomainEventsAsync(eventsToDispatch, ct);
            processedDomainEvents.AddRange(eventsToDispatch);
            unprocessedDomainEvents = [.. GetDomainEvents(dbContext).Where(e => !processedDomainEvents.Contains(e))];
        }
    }

    private List<AggregateRoot> GetTrackedAggregateRoots(DbContext dbContext) =>
        [.. dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Count != 0)
            .Select(x => x.Entity)];

    private List<IDomainEvent> GetDomainEvents(DbContext dbContext)
    {
        List<AggregateRoot> aggregatedRoots = GetTrackedAggregateRoots(dbContext);
        return [.. aggregatedRoots
            .SelectMany(x => x.DomainEvents)];
    }

    private void ClearDomainEvents(DbContext dbContext)
    {
        List<AggregateRoot> aggregateRoots = GetTrackedAggregateRoots(dbContext);
        foreach (AggregateRoot aggregate in aggregateRoots)
        {
            _clearDomainEventsDelegate(aggregate);
        }
    }

    private async Task DispatchDomainEventsAsync(
        List<IDomainEvent> domainEvents,
        CancellationToken ct)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            // Create DomainEventNotification<TConcreteEvent> at runtime
            // because the concrete event type is known only at runtime.
            Type notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await publisher.Publish(notification, ct);
        }
    }
}
