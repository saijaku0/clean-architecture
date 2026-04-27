# ADR-0004: Domain events carry identifiers only, never PII or rich payload

## Status

Accepted — 2026-04-25

## Context

Each aggregate raises domain events when significant state changes happen — a user registers, a reservation is confirmed, a trip seat is reserved. Subscribers (cache invalidators, audit loggers, notification senders) react to those events.

There's a design choice for what each event carries:

- **Rich events** include the data the subscriber will need: emails, names, prices, status changes, full state snapshots. Subscribers act directly on the payload.
- **Thin events** carry only identifiers. Subscribers fetch enriched data from repositories themselves.

The first option looks more efficient — one less DB round trip per subscriber. The second looks more disciplined but introduces a "useless" extra query.

In practice, the trade-off cuts the other way once you account for where events end up.

## Decision

**Domain events carry identifiers only.** No emails. No full names. No payment amounts. No status enums. No detailed payload of any kind.

For example, our `UserCreatedDomainEvent` and `ReservationConfirmedDomainEvent` look like:

```csharp
public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record ReservationConfirmedDomainEvent(Guid ReservationId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

A subscriber that needs the user's email retrieves it via `IUserRepository.GetByIdAsync(notification.DomainEvent.UserId)`.

This applies to every domain event in the system without exception.

## Consequences

### Positive

- **No accidental PII in logs, traces, or event stores.** Every cross-cutting concern that touches events — `LoggingBehavior`, OpenTelemetry spans, Outbox tables, audit pipelines — automatically inherits this safety. We don't have to remember to scrub fields per log target.
- **GDPR-by-default.** Email addresses, names, and other personal data exist in one place (the database, behind controlled access). Events are a transport concern; transports should not duplicate persistent data.
- **Subscribers fetch the freshest version of the data.** If between event raise and event handle the underlying entity changed, the subscriber sees the new state. With rich events, the subscriber would act on stale snapshot data.
- **Smaller event payloads.** Event records serialize cheaply, fit in single in-memory MediatR notifications, transmit cleanly over a future Outbox/message bus without excess bytes.
- **Single point of contract.** Every event is a `record` with `(Guid SomeId)` and an `OccurredAt`. Reviewers don't have to re-evaluate the privacy posture of every new event — the rule is "no payload, period."

### Negative

- **One extra database read per subscriber that needs entity data.** A subscriber that wants the user's email reads the user back. Mitigated by:
  - The entity is usually still in the EF `ChangeTracker` from the operation that raised the event, so the read may not even hit the database.
  - Most subscribers don't need entity data at all — cache invalidators, metric counters, simple "fact happened" handlers operate on identifiers alone.
  - Where the cost matters (e.g., bulk welcome emails after a sign-up storm), batched repository reads keep the overhead manageable.

### Neutral

- **Doesn't affect domain rules.** This is purely about the event-transport surface. The domain inside the aggregate works the same regardless of what the event carries.
- **Compatible with any future Outbox / integration-event pipeline.** When events cross a service boundary, the rule already prevents the worst leakage.

## Alternatives considered

### Alternative A — Rich events carrying full state

Each event carries the relevant fields directly:

```csharp
public sealed record UserCreatedDomainEvent(
    Guid UserId,
    string Auth0Sub,
    string Email,
    string? FullName) : IDomainEvent;
```

**Rejected because:**

- **PII leakage** to every observability and persistence path that handles events. Concretely: a `LoggingBehavior` that logs every notification will write `john@example.com` to Serilog → log files → log aggregators (Seq, CloudWatch, Datadog) → log retention systems. This is a GDPR fact, not a hypothetical.
- **Stale data risk.** The event is raised at a specific moment. By the time it's handled (especially with Outbox or async pipelines), the underlying entity may have moved on. Handlers that act on rich-event snapshots act on a frozen view.
- **Event versioning becomes harder.** Every payload field is a contract. Adding, removing, or reshaping a field is a breaking change for subscribers. With identifier-only events, the contract is essentially eternal (a Guid is always a Guid).

### Alternative B — Hybrid: rich events for "internal" handlers, thin events crossing boundaries

Mark certain events as "internal-only" and let those carry rich data; only events that cross context boundaries get the strict no-PII treatment.

**Rejected because:**

- **The boundary moves.** What's internal today (a logging behavior in the same process) may go through an Outbox to a different process tomorrow when we operationalize for production. Being strict from day one removes the rewrite later.
- **Two-tier rules are harder to enforce.** "When is an event allowed to carry PII?" is a question every code reviewer would have to relitigate. "Never" is a one-line policy.
- **The cost of strict events is small.** One repository read per subscriber that needs data, for a small subset of subscribers. Not worth the policy complexity.

### Alternative C — Rich events with explicit scrubbing in cross-cutting handlers

Keep rich events, but have `LoggingBehavior` and the Outbox dispatcher strip PII fields by attribute:

```csharp
public sealed record UserCreatedDomainEvent(
    Guid UserId,
    [Sensitive] string Email) : IDomainEvent;
```

**Rejected because:**

- **One-by-one annotations are forget-prone.** Adding a new field tomorrow without `[Sensitive]` introduces a leak silently.
- **The scrubbing logic is custom code, with bugs.** "Strip everything, keep nothing" is bug-proof. "Strip the things marked sensitive" is bug-prone.
- **Doesn't help if the event lands in a queue/store** that doesn't run our scrubbing logic.

## Revisit trigger

Revisit if:

- A specific subscriber proves itself unable to function without payload data, *and* the cost of the repository round trip is shown to dominate the operation. We've never observed this.
- The domain ever needs **integration events** that cross service boundaries with rich payload by design. That's a different layer of events from these (domain events stay identifier-only; integration events would be a separate type with explicit, reviewed payload). When we add them, this ADR doesn't change — it just doesn't apply to that other category.

## References

- README → 🗂 Conscious trade-offs and backlog → item 1 (events without Outbox)
- ADR-0002 — User identity model, also a "no leakage of internal identifiers" decision
- [GDPR Article 5(1)(c) — data minimisation principle](https://gdpr-info.eu/art-5-gdpr/) — the legal underpinning of "don't pass PII through systems that don't need it"