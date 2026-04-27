# ADR-0001: Materialize TripSeats per trip

## Status

Accepted — 2026-04-23

## Context

A core question in the domain model is where to store the **availability and price of a specific seat on a specific trip**.

The physical layer (`Trains` → `Wagons` → `Seats`) is stable: a train has the same physical seats for years. But each run of that train — each `Trip` — has its own independent availability state. Seat 15 in wagon 3 may be `Available` on the 25 April trip, `Sold` on the 26 April trip, and `Reserved` on the 27 April trip, all at the same time.

There are three realistic ways to model this:

1. **Denormalized per-trip snapshot (materialized bridge).** When a `Trip` is created, insert one row per physical seat into a `TripSeats` table. Each row owns `(TripId, SeatId, Status, Price)` for that one trip. This is what we chose.

2. **Thin bridge with on-demand rows.** A `TripSeats` table exists, but rows are created only when a seat transitions out of `Available` (e.g. first reservation touches it). Availability is derived: "no row = available, row exists = whatever status says."

3. **Fully derived availability.** No `TripSeats` table at all. A seat's status for a trip is computed by joining `Reservations` and `ReservationSeats` and checking for active reservations on that `(TripId, SeatId)` pair.

## Decision

We materialize a `TripSeats` row for every seat of the train at the moment a `Trip` is created.

Concretely: the `CreateTripCommand` handler (or seed script) reads all `Seats` belonging to the train's `Wagons` and inserts one `TripSeats` row per seat with `Status = 'Available'` and `Price = BasePrice`.

## Consequences

### Positive

- **Pessimistic locking is straightforward.** `SELECT ... WITH (UPDLOCK, ROWLOCK, HOLDLOCK) FROM TripSeats WHERE TripId = @t AND SeatId IN (...)` locks exactly the rows we want. With approach (2) or (3) the locked object is conceptually a range or a derived view, which is harder to get right in SQL Server.
- **Availability queries are one indexed scan.** `GET /trips/{id}/seats` reads `TripSeats` filtered by `TripId` and `Status = 'Available'`. No joins through reservation history, no computed columns. The `IX_TripSeats_TripId_Status` index makes this fast.
- **Per-trip price is a single column.** An operator editing the price for one trip writes one cell. With derived approaches, per-trip pricing needs its own side table anyway, which erodes the benefit of normalization.
- **Simple mental model.** A reader of the schema sees `TripSeats` and immediately knows where seat state lives.

### Negative

- **Row count grows as `trips × seats`.** A fleet of 100 trains with ~500 seats each, running 1 trip per day over a 1-year booking horizon, produces ~18 million `TripSeats` rows. SQL Server handles that size with proper indexing, but it is not small.
- **Trip creation is a bulk insert.** Creating one trip inserts ~500 rows. Not a problem at MVP scale; something to batch or move off the request path at high volume.
- **Historical data accumulates.** Trips that departed last year still have all their `TripSeats` rows. A retention policy (archive or delete trips older than N months) will eventually be needed.
- **Schema changes to seat state ripple across many rows.** Adding a new column to `TripSeats` touches every historical row in migration time.

### Neutral

- Price changes propagating to existing reservations are prevented by `ReservationSeats.PriceSnapshot`, which is orthogonal to this decision.

## Alternatives considered

### Alternative A — thin bridge with on-demand rows

Store rows in `TripSeats` only for seats that are no longer `Available` on that trip. Availability check becomes: *if no row exists, the seat is available.*

**Rejected because:**

- Pessimistic locking across a "row may not yet exist" set is painful. Common patterns (upsert with `MERGE`, insert-or-update under lock) all have footguns on SQL Server, and the locking semantics of a non-existent row require range locks — which is exactly the isolation level we wanted to avoid.
- The "seat is available" case is the hot path (most seats on most trips are available). Making it a "row doesn't exist" query inverts the cost: reads are cheap in normal form but lock acquisition is more expensive.
- Savings are modest: a popular trip may have 40–60% of its seats booked close to departure, so we're not saving a large fraction of rows.

### Alternative B — fully derived availability from `Reservations`

Never materialize any seat state. Compute it from active reservations.

**Rejected because:**

- Every availability read becomes a 3-way join (`Reservations` × `ReservationSeats` × `Seats`) with date filtering on reservation expiry. That join runs for every `GET /trips/{id}/seats`, which is the single most frequent read in the system.
- The 15-minute TTL makes "active reservation" a time-dependent predicate, so availability depends on wall clock. This prevents simple caching and makes reasoning about consistency harder.
- Per-trip pricing has no natural home. It would need a side table that looks suspiciously like `TripSeats` without the status column.

## Revisit trigger

Revisit this decision if any of the following become true:

- `TripSeats` row count exceeds ~100M and either query performance or storage cost becomes a concern
- The booking horizon extends to multiple years across a large route network
- An archive/partitioning strategy on `TripSeats` becomes operationally burdensome
- A new feature requires richer seat state (assignments, holds, VIP tags) that would fit better in a normalized model

## References

- `docs/database-schema.md` — the schema as currently implemented
- `README.md` → Concurrency strategy — the locking contract that this decision enables
- Martin Fowler, "Event Sourcing" — a relevant alternative if we ever want to reconstruct seat state from a log of booking events rather than materialize it