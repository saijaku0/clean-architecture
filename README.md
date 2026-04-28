# 🚂 Train Seat Reservation API

Welcome to the **Train Seat Reservation API**!

This is a learning project that demonstrates modern approaches to building backend applications on the .NET platform. The goal is to deliver a reliable, testable, and easily scalable train seat reservation service using the principles of **Clean Architecture** and **Domain-Driven Design (DDD)**.

The project is a good fit for studying design patterns, CQRS, domain events, concurrent access, and microservice infrastructure.

## 🎯 About the project and business rules (Core Domain)

The problem domain is modeled in two layers.

**Physical layer** — long-lived rolling stock that rarely changes:

* **Train** — a physical trainset; reused across many trips. Aggregate root.
* **Wagon** — a car of a train, with a class (`FirstClass` / `SecondClass` / `Bistro`). Internal entity of the Train aggregate.
* **Seat** — a physical seat in a wagon, identified by its number within the wagon. Internal entity of the Train aggregate.

**Logical / booking layer** — per-trip availability and bookings:

* **Trip** — a specific run of a train on a route at a given date and time. Aggregate root.
* **TripSeat** — the availability and price of a specific seat on a specific trip (status: `Available` / `Reserved` / `Sold`). A separate aggregate root, isolated from `Trip` so that pessimistic locking can target it independently — see [ADR-0005](./docs/adr/0005-trip-seat-as-separate-aggregate.md).
* **Reservation** — a booking entity linking a passenger to one to four trip seats. Aggregate root.
* **ReservationSeat** — internal entity of a Reservation, with a `PriceSnapshot` field that fixes the price at booking time.

**Identity:**

* **User** — a thin local cache of Auth0 identity (email, full name). Keyed by an internal Guid `Id` with a unique `Auth0Sub` column. See [ADR-0002](./docs/adr/0002-user-internal-guid-with-auth0-sub-unique.md).

Full ERD and column-level details live in [`docs/database-schema.md`](./docs/database-schema.md).

**Key business rules enforced in the domain:**

* **Idempotency and concurrency:** A trip seat cannot be booked if its status is already `Reserved` or `Sold`. Implementation details are covered in the [🔒 Concurrency strategy](#-concurrency-strategy) section.
* **Reservation TTL:** A reservation lives for exactly 15 minutes. If its status has not moved to `Confirmed`, it is cancelled automatically by a background job, and the status of its trip seats flips back to `Available` via a domain event.
* **Limits:** A reservation must include between 1 and 4 trip seats.
* **Time validation:** A reservation cannot be created for a trip that has already departed. A reservation cannot be confirmed after its 15-minute window has expired (race-safety against the expiry job). A reservation cannot be cancelled less than 24 hours before the trip's departure.
* **Pricing:** The seat class (the `SeatClass` value object) encapsulates the business logic for final price calculation. Class is attached at the wagon level and inherited by seats. The `Reservation.TotalPrice` and `ReservationSeat.PriceSnapshot` are denormalized at booking time so later price changes don't retroactively rewrite existing bookings.

## 🛠 Tech stack and patterns

### Architecture
* **Clean Architecture:** Strict separation into layers. The Domain has no external dependencies — no MediatR, no EF Core, no FluentValidation, only the .NET BCL.
* **CQRS via MediatR:** Commands are isolated from queries. Repetitive infrastructure plumbing (UnitOfWork, uniform handling of domain exceptions) is extracted into base classes `CommandHandler<T>` and `QueryHandler<T>`, so concrete handlers contain only pure business logic. An alternative approach using Pipeline Behaviors is discussed in the backlog.
* **Domain Events:** `IDomainEvent` lives in Domain as a marker interface; `DomainEventNotification<T>` in Application wraps it for MediatR. Events are dispatched by overriding `SaveChangesAsync` in EF Core (see [ADR-0004](./docs/adr/0004-domain-events-without-pii.md) for the no-PII rule).
* **Result pattern:** No exceptions for business logic. The domain uses `Result<T>` and `Error` (with `ErrorType` for HTTP-status mapping at the API layer). Exceptions remain only for programmatic-contract violations (null parameters, invalid Guids), thrown by `Guard.Against.*` clauses.
* **`TimeProvider` for time-dependent logic:** Aggregates that reason about time (`Trip`, `Reservation`) accept `TimeProvider` as a parameter on factory methods and operations. Tests substitute `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`. See [ADR-0003](./docs/adr/0003-time-provider-for-testable-time.md).

### Infrastructure
* **Database:** Microsoft SQL Server 2022, with migrations extracted into a separate console project.
* **Caching:** Redis for frequent queries (for example, `GetTripSeatsQuery`). The cache is invalidated by domain events when seat state changes.
* **Background jobs:** An `IHostedService` that automatically checks for expired reservations every minute.
* **Authentication:** Auth0 (cloud-hosted JWT validation via JWKS). A thin local `User` table caches `email` and `full name`, keyed by an internal Guid `Id` with a unique `Auth0Sub` column. Populated lazily by middleware on the first authenticated request. See [🔑 Auth0 setup](#-auth0-setup).
* **Logging:** Serilog (structured logging).

## 🔒 Concurrency strategy

Seat booking is the central contention point in the system: multiple users click on the same seats of a popular trip at the same time. For that load profile, we use **pessimistic row-level locking** in SQL Server on the `TripSeats` table:

* **Lock hints:** `WITH (UPDLOCK, ROWLOCK, HOLDLOCK)` on the `SELECT` inside the reservation transaction.
    * `UPDLOCK` — an intent-update lock: it blocks anyone else who wants to modify the row, while still allowing plain SELECTs.
    * `ROWLOCK` — forces row-level locking instead of page/table level.
    * `HOLDLOCK` — holds the lock until the transaction ends (the equivalent of SERIALIZABLE range locks, but scoped to the selected rows).
* **Isolation level:** `ReadCommitted` (the default) together with the hints above. We do not raise the global isolation level to SERIALIZABLE, to avoid blocking anything unnecessary.
* **Deterministic lock order:** trip seats are locked with `ORDER BY TripSeatId` — this is critical to prevent deadlocks when two requests compete for overlapping sets of seats (recall the 4-seat limit per reservation).
* **Deadlock policy:** we catch SQL Server error 1205 and retry up to 3 times with exponential backoff via Polly. If the conflict persists, the client gets a 409 Conflict.
* **Lock timeout:** `SET LOCK_TIMEOUT 3000` at the command level, so a connection is not held indefinitely.
* **Supporting index:** `IX_TripSeats_TripId_Status` backs both the hot-path availability query (`GET /trips/{id}/seats`) and the locked `SELECT` inside the reservation command.

This approach gives strict correctness under the moderate contention expected at the level of individual trips. Horizontal scaling is possible through sharding by `TripId` if the need arises.

## 📁 Solution structure

The project is split into logical modules to keep coupling loose:

```text
📦 TrainBooking.sln
 ┣ 📂 src
 ┃  ┣ 📂 TrainBooking.Domain         # Entities, Value Objects, IDomainEvent, Domain Errors/Results, Guard clauses
 ┃  ┣ 📂 TrainBooking.Application    # Commands, Queries, Handlers, FluentValidators, IRepository interfaces
 ┃  ┣ 📂 TrainBooking.Infrastructure # EF Core DataContext, Redis integration, MediatR adapters, Serilog
 ┃  ┣ 📂 TrainBooking.Api            # Endpoints, Global Exception Handler, Result -> HTTP extensions
 ┃  ┗ 📂 TrainBooking.Migrations     # EF Core Migrations runner (Console App)
 ┣ 📂 tests
 ┃  ┗ 📂 TrainBooking.Tests          # Architecture Tests (NetArchTest), Unit Tests, Integration Tests (Testcontainers)
 ┗ 📂 docs                           # ERD, ADRs, supporting documentation
```

## 🚧 Project scope (MVP boundaries)

The scope is deliberately fixed. We are not adding payment gateways, user registration, or email notifications at this stage. All development revolves around 5 endpoints and 1 background job:

**Write (Commands):**
* `POST /reservations` — create a new reservation.
* `PUT /reservations/{id}/confirm` — confirm payment/reservation.
* `PUT /reservations/{id}/cancel` — cancel a reservation on the user's behalf.

**Read (Queries):**
* `GET /trips/{id}/seats` — get the list of available seats (cached in Redis).
* `GET /reservations/{id}` — get the status of a specific reservation.

**Background:**
* A job that checks for and expires reservations (TTL > 15 min).

*Ideas for extending the functionality are welcome, but they first go into the backlog so they do not block the MVP release.*

## 🐳 Quick start (local development)

The project is fully containerized. Infrastructure is brought up via Docker Compose.

**Requirements**
* .NET 10 SDK
* Docker Desktop
* A dev tenant on Auth0 (free up to 25k MAU)

**Steps**
1. Clone the repository.
2. Configure Auth0 (see [🔑 Auth0 setup](#-auth0-setup)).
3. In the root directory, run: `docker compose up -d` (this starts SQL Server and Redis).
4. Run the `TrainBooking.Migrations` project to apply the database schema.
5. Run `TrainBooking.Api`. The Scalar UI will be available at `https://localhost:5001/scalar`.

## 🔑 Auth0 setup

1. Create a dev tenant on Auth0.
2. Under **Applications → Applications**, create an Application of type *Regular Web Application* (or *Machine-to-Machine* for service-to-service tests).
3. Under **Applications → APIs**, create an API with an identifier (audience), for example `https://train-booking-api`. In Permissions, define the scopes: `reservations:read`, `reservations:write`.
4. In `appsettings.Development.json`, configure:
   ```json
   "Auth0": {
     "Domain": "your-tenant.eu.auth0.com",
     "Audience": "https://train-booking-api"
   }
   ```

* The JWT Bearer pipeline pulls the JWKS automatically from `https://{Domain}/.well-known/openid-configuration` — signature validation does not require manual key setup.
* Scopes from the token are mapped to policies via `[Authorize(Policy = "reservations:write")]`.

**Local user cache.** Auth0 is the source of truth for identity. We keep a thin `User` table keyed by an internal Guid `Id` with a unique `Auth0Sub` column, caching `email` and `full name` so reservation reads don't need to call the Auth0 Management API on every request. On the first authenticated request, an `EnsureUserCache` middleware looks up the user by the `sub` claim from the JWT, inserts a new row with a fresh Guid if missing, and exposes the resulting internal `User.Id` to handlers. A `LastSyncedAt` column allows periodic refresh if profile data drifts. Decoupling the internal `Id` from `Auth0Sub` means changing the identity provider later does not require migrating every foreign key.

## 🧪 Testing

* **Unit tests:** pure tests of the domain and handlers, with no infrastructure. `xUnit` + `Shouldly`. Tests substitute `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` for any time-dependent logic.
* **Architecture tests:** `NetArchTest` — verifies that Domain does not reference Infrastructure, that every handler inherits from the base class, that domain events implement `IDomainEvent`, and so on.
* **Integration tests:** `WebApplicationFactory` + `Testcontainers` (`Testcontainers.MsSql`, `Testcontainers.Redis`) — a real SQL Server and Redis are spun up in Docker on every run. No in-memory database substitutes.

*Before submitting a Pull Request, please make sure all tests (Unit, Integration, Architecture) pass.*

## 🗂 Conscious trade-offs and backlog

This section honestly documents the MVP's compromises. Every item here is a deliberate decision, not an oversight.

1.  **Domain events are dispatched inside `SaveChangesAsync`, without an Outbox.** If a handler fails after the commit, the event is lost (for example, Redis invalidation does not happen → stale cache until the TTL expires). For production-grade reliability, moving to a Transactional Outbox + a dedicated worker is on the backlog.
2.  **The Redis cache is eventually consistent.** The baseline defense is a short TTL (60 sec) as a fallback in case event-driven invalidation does not fire.
3.  **Idempotency for `POST /reservations` via an `Idempotency-Key` header** (+ Redis with a 24-hour TTL) — not in the MVP, but on the backlog. Without it, client retries on a network timeout will produce duplicate reservations.
4.  **Base handler classes vs. Pipeline Behaviors.** The current approach with `CommandHandler<T>` / `QueryHandler<T>` reduces boilerplate through inheritance. The alternative — a set of `IPipelineBehavior<TRequest, TResponse>` implementations (Logging / Validation / Transaction) — is the more idiomatic MediatR path and gives better composition of cross-cutting behavior. A likely refactor once the domain stabilizes.
5.  **The retry policy is limited to deadlocks (error 1205).** Other transient SQL Server failures are not retried automatically — for production this should be extended via Polly combined with the `Microsoft.Data.SqlClient` transient error detector.
6.  **`TripSeats` are generated per trip (denormalization).** Creating a new `Trip` materializes a `TripSeat` row for every seat of the linked train. This simplifies locking and availability queries but duplicates rows across trips. For a longer booking horizon (years of trips across many routes), revisiting with table partitioning or a lighter-weight availability model is the next step. See [ADR-0001](./docs/adr/0001-trip-seats-denormalization.md).
7.  **Local `User` cache is lazy and eventually consistent with Auth0.** Profile changes made in Auth0 (email, name) don't propagate to our DB until the next refresh window. Acceptable for MVP; a webhook-driven sync is the next step if it matters.
8.  **`User` and `EntityBase` audit fields use `DateTime.UtcNow` directly** instead of `TimeProvider`. Doesn't affect correctness — only test determinism. Migration to TimeProvider is on the backlog.
9.  **No strongly-typed IDs (yet).** All identifiers are raw `Guid`. The generic `Entity<TId>` keeps the door open for migration later via a source generator like `StronglyTypedId`. See the trade-off discussion in the foundation PR.

This is a learning project that evolves iteratively. Its goal is to reinforce the patterns of Clean Architecture, DDD, CQRS, domain events, and concurrent access in SQL Server.
