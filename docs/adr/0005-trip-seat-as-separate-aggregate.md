# ADR-0005: `TripSeat` is a separate aggregate root, not an inner entity of `Trip`

## Status

Accepted — 2026-04-25

## Context

In the conceptual model, a `Trip` "has" `TripSeat`s — every seat on every trip is a particular row in `TripSeats`, scoped to that trip. A textbook DDD modeler would naturally make `TripSeat` an inner entity of the `Trip` aggregate, similar to how `Wagon` and `Seat` are inner entities of the `Train` aggregate.

Specifically, the canonical DDD rule says:

> All access to inner entities goes through the aggregate root. The unit of transactional consistency is the entire aggregate.

If we followed that rule for `Trip` + `TripSeats`, every reservation flow would have to:

1. Load `Trip` plus all its `TripSeat`s (potentially 500 rows for a long train) into memory.
2. Mutate the relevant `TripSeat`s through `Trip` methods.
3. Save the entire `Trip` aggregate back through `ITripRepository`.

This conflicts with the *primary* technical concern of the project: pessimistic locking on individual `TripSeat` rows during reservation.

## Decision

`TripSeat` is its own aggregate root. It has:

- An identifier (`TripSeatId`, Guid).
- Its own repository — `ITripSeatRepository` (to be implemented in Application/Infrastructure).
- Its own status transitions and invariants — `Reserve()`, `Release()`, `MarkAsSold()`.
- Its own domain events — `TripSeatReservedDomainEvent`, `TripSeatReleasedDomainEvent`.

`Trip` knows nothing about `TripSeats`. The relationship is database-level only — the FK `TripSeats.TripId → Trips.Id`. There is no `Trip.TripSeats` collection in the Domain.

The tension this creates with classic DDD is acknowledged. We accept it deliberately because the primary system concern — pessimistic locking — pushes for the granularity that this decision provides.

## Consequences

### Positive

- **Pessimistic locking targets exactly the right rows.** A reservation handler does:

  ```sql
  SELECT * FROM TripSeats WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
  WHERE TripId = @t AND Id IN (...)
  ORDER BY Id
  ```

  This works because `TripSeat` is its own row in its own aggregate and we have a repository that can issue this query. If `TripSeat` were inner to `Trip`, we'd be locking the entire trip's row footprint or working around through raw SQL, both ugly.

- **Cache invalidation is granular.** When a single `TripSeat` flips from `Available` to `Reserved`, we invalidate the relevant Redis key for that trip's seat list — driven by `TripSeatReservedDomainEvent`. We don't have to detect "the trip aggregate was saved" and figure out which seats changed.

- **Hot-path queries are simple.** `GET /trips/{id}/seats` is `SELECT * FROM TripSeats WHERE TripId = @t AND Status = 'Available'`. No need to load `Trip` first to access an inner collection.

- **Reservation flow is clean.** `Reservation.Create` accepts an `IReadOnlyCollection<TripSeat>` directly — no awkward parent-child loading from `Trip`.

- **The `Trip` aggregate stays small.** It has six fields and one method (`Create`). It's simple to reason about precisely because the seat-level state belongs elsewhere.

### Negative

- **DDD-orthodox reviewers will flag this.** A purist reading of Eric Evans says "TripSeat exists only inside a Trip context, therefore it should be inside the Trip aggregate." The justification has to be on hand.

- **Cross-aggregate consistency.** When `Reservation.Confirm` runs, it raises `ReservationConfirmedDomainEvent`; a handler then calls `tripSeat.MarkAsSold()` on each affected `TripSeat`. This crosses two aggregate boundaries within the same business transaction. We accept eventual consistency between these two via domain events:

  ```text
  Reservation.Confirm  →  Reservation status = Confirmed (saved)
                       →  ReservationConfirmedDomainEvent (raised)
                       →  handler invokes TripSeat.MarkAsSold per seat
                       →  TripSeats updated (saved)
  ```

  If the second save fails, the system is briefly inconsistent. The 15-minute reservation TTL plus deterministic locking constrains the blast radius. A future Outbox makes it durably consistent (see README backlog item 1).

- **No physical seat-context lookup through Trip.** Code that wants "all seats on this trip" goes through `ITripSeatRepository.GetByTripIdAsync(...)`, not `trip.Seats`. Slightly more verbose, but the explicit repository call is honest about what's happening (a query, not a navigation).

- **Trip and TripSeat must agree on schema.** Adding a per-seat field that depends on trip-level data requires both sides to coordinate. Acceptable price for the decoupling.

### Neutral

- **The aggregate boundary is database-aligned, not domain-aligned.** Per row → its own aggregate. This works because the rows are *meaningfully* independent from a transactional standpoint (one user reserves seat 42, another reserves seat 17, those are independent operations).

## Alternatives considered

### Alternative A — `TripSeat` as inner entity of `Trip` (the textbook DDD answer)

Trip aggregate root contains `List<TripSeat>`. Reservation handlers go through `Trip.ReserveSeats(...)`.

**Rejected because:**

- **Loading 500 rows for a single seat reservation is wasteful.** Even with EF Core's per-collection lazy loading, the locking semantics blur — we'd be locking conceptually at the seat level but loading at the trip level. SQL Server isn't naturally aligned with this view.
- **`SELECT WITH (UPDLOCK)` against a child collection** through EF Core requires raw SQL anyway. The clean repository abstraction we get with `ITripSeatRepository` evaporates.
- **Cache invalidation becomes "the trip changed, invalidate everything trip-related"** instead of "this specific seat changed, invalidate the trip's seat list." The granular path is much better for hot-path queries.

### Alternative B — `TripSeat` is a *value object* on `Trip`

Even more aggressive — no identity, just (Trip, Seat) → status mapping.

**Rejected because:**

- Value objects can't be locked at row level by definition.
- Domain events on individual seat state changes become impossible (no identity to attach the event to).
- The `ReservationSeats` join table needs an FK to `TripSeat`, which requires identity.

### Alternative C — `Trip` and `TripSeat` are two aggregates, but `Trip.Seats` is a navigation property loaded on demand

Compromise: keep separate roots, but expose a convenient navigation in code.

**Rejected because:**

- It re-creates the same temptation that pure-DDD would create: "I'll just load `trip.Seats` and mutate them" — bypassing the locking-aware repository pattern.
- EF Core navigation properties from one aggregate root to another are a known anti-pattern. Once exposed, they're hard to take back.

## Revisit trigger

Revisit if:

- We adopt **optimistic concurrency** instead of pessimistic locking. The original argument goes away — `Trip + TripSeats` becomes a reasonable single-aggregate model.
- We never actually need to lock individual seats (e.g., the system pivots to a queue-based "request a seat → batch assigns" model). The aggregate boundary follows the locking strategy, so the answer changes.
- A future `TripCancelledDomainEvent` handler needs to release every `TripSeat` of the trip atomically. Currently it would do so per row in a loop with retries; if that becomes a hotspot, we may need a more aggregate-aware design.

## References

- ADR-0001 — TripSeats denormalization (related: why TripSeat exists per-trip in the first place)
- ADR-0004 — Domain events without PII (related: how cross-aggregate consistency is achieved)
- README → 🔒 Concurrency strategy — describes the locking model that drives this decision
- Vaughn Vernon, *Implementing Domain-Driven Design*, Chapter 10 — discussion of when "small aggregates" is preferable to "true conceptual aggregates"