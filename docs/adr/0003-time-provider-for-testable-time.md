# ADR-0003: Use `TimeProvider` for time-dependent domain logic

## Status

Accepted — 2026-04-25

## Context

Several domain operations depend on the current time:

- `Trip.Create` rejects a departure time in the past.
- `Reservation.Create` rejects a trip that has already departed and computes `ExpiresAt = now + 15 minutes`.
- `Reservation.Confirm` rejects a confirmation attempt after `ExpiresAt` (race-safety against the expiry job).
- `Reservation.Cancel` rejects cancellation when the trip is less than 24 hours away.
- `Reservation.Expire` rejects an expiry attempt before `ExpiresAt` (job safety).
- `User.UpdateProfile` and `EntityBase.CreatedAt` set timestamp fields.

The naive implementation reads `DateTime.UtcNow` directly inside each method. That's the Domain layer doing IO — implicitly, against the operating-system clock — which makes those methods non-deterministic. Concretely, that's painful for tests:

```csharp
[Fact]
public void Cancel_LessThan24HoursBeforeDeparture_Fails()
{
    var reservation = ...; // What do we set TripDepartureTime to?
                           // DateTime.UtcNow.AddHours(23)? The test now passes
                           // or fails depending on how slow the test runner is.
}
```

We want time to be a parameter that tests can substitute deterministically.

## Decision

Domain operations that reason about time accept a `TimeProvider` parameter (from `System` namespace, BCL since .NET 8). Production code passes `TimeProvider.System`; tests substitute `FakeTimeProvider` from the `Microsoft.Extensions.TimeProvider.Testing` package.

Concretely:

```csharp
public Result Cancel(TimeProvider timeProvider)
{
    var now = timeProvider.GetUtcNow().UtcDateTime;
    var hoursUntilDeparture = (TripDepartureTime - now).TotalHours;
    if (hoursUntilDeparture < 24)
        return ReservationErrors.CannotCancelWithinDepartureWindow(...);
    // ...
}
```

Tests:

```csharp
var clock = new FakeTimeProvider(new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc));
var reservation = ...;
var result = reservation.Cancel(clock);
result.IsFailure.ShouldBeTrue();
```

`TimeProvider` is part of the BCL, so this introduces no new package reference in `TrainBooking.Domain.csproj`. The Domain layer remains free of third-party libraries (no MediatR, no EF Core, no FluentValidation).

## Consequences

### Positive

- **Deterministic tests for time-sensitive logic.** The 24-hour cancellation window, the 15-minute reservation TTL, the expiry race-safety check — all directly testable without touching system clocks or wall-clock waits.
- **Explicit dependency.** The signature `Reservation.Cancel(TimeProvider)` makes the time dependency obvious. A reader doesn't have to scan the body to discover that "this method calls `DateTime.UtcNow`."
- **No new third-party dependency.** `TimeProvider` is in `System`. The "Domain has no external libraries" rule still holds.
- **Forward-compatible with offset/timezone awareness.** `TimeProvider.GetUtcNow()` returns `DateTimeOffset`. We currently project to `DateTime` via `.UtcDateTime`, but the source-of-truth call gives us the option to handle offsets later without rewriting signatures.

### Negative

- **Slightly more parameters in factory methods.** `Trip.Create(...)` and `Reservation.Create(...)` carry `TimeProvider` as an extra argument. Mitigated by `TimeProvider` being passed once from the handler down to the aggregate; aggregates don't store it as state.
- **Inconsistency with `User` and `EntityBase`.** Those still use `DateTime.UtcNow` directly (technical debt — see backlog item in README). A future refactor will unify them.
- **Tests need the testing package.** `Microsoft.Extensions.TimeProvider.Testing` is a NuGet reference for `TrainBooking.Tests` only. Domain stays clean.

### Neutral

- **No production performance difference.** `TimeProvider.System.GetUtcNow()` resolves to the same OS call as `DateTime.UtcNow`.

## Alternatives considered

### Alternative A — `IClock` abstraction we define ourselves

Custom interface in Domain:

```csharp
public interface IClock { DateTime UtcNow { get; } }
```

with `SystemClock` implementation in Infrastructure.

**Rejected because:**

- Reinvents a wheel that the BCL provides since .NET 8.
- A custom `IClock` is a *project-specific* abstraction. `TimeProvider` is recognized across the .NET ecosystem; libraries (e.g., Polly, Quartz.NET) are migrating to consume it. Using the standard makes us interoperable.
- The `TimeProvider` API surface (timers, scheduled callbacks) is richer than a single `UtcNow` property. We may need that later (e.g., for the expiry background job).

### Alternative B — Pass `DateTime now` as a primitive parameter

Caller computes `DateTime.UtcNow` once and passes it down:

```csharp
public Result Cancel(DateTime now)
```

**Rejected because:**

- The dependency is still implicit at every level — every test setup has to remember the convention "the caller is responsible for fetching the current time."
- Doesn't compose with other time-related needs. If we later need "the current time, but advanced by 5 seconds for retry purposes," `TimeProvider` lets us implement that as a wrapper. A raw `DateTime` doesn't.
- More work to migrate to `TimeProvider` later than to start there.

### Alternative C — Keep `DateTime.UtcNow` and accept untestable time-dependent paths

Skip the abstraction entirely. Test only the parts that don't touch time, and trust manual integration testing for the rest.

**Rejected because:**

- The 24-hour cancellation window is the most subtle time-dependent invariant in the system. It's exactly the kind of logic that benefits from deterministic tests.
- `FakeTimeProvider` makes the alternative effectively free.

## Revisit trigger

Revisit if:

- A simpler abstraction emerges in the BCL or community standard practice changes.
- We need to model time zones or DST transitions (currently UTC-only — easier with the `DateTimeOffset` source on `TimeProvider` than with `DateTime.UtcNow`).
- Performance profiling reveals the abstraction matters in some hot loop (extremely unlikely — the JIT inlines `TimeProvider.System.GetUtcNow` to the same code as `DateTime.UtcNow`).

## References

- [.NET 8: New TimeProvider class](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)
- [`Microsoft.Extensions.TimeProvider.Testing`](https://www.nuget.org/packages/Microsoft.Extensions.TimeProvider.Testing) — `FakeTimeProvider`
- README → "TimeProvider for time-dependent logic" — short summary in plain language