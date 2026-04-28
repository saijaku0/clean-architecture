# Architecture Decision Records (ADRs)

An ADR captures a significant architectural decision made along with its context and consequences. The format is based on [Michael Nygard's template](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions.html).

We write an ADR when a decision:

- Has meaningful trade-offs (we'd plausibly have chosen differently)
- Shapes how the system is put together (not just how a single class is written)
- Would be re-asked by future contributors if it weren't written down

ADRs are immutable once accepted. To change a decision, write a new ADR that **supersedes** the old one.

## Index

| # | Title | Status |
|---|---|---|
| [0001](./0001-trip-seats-denormalization.md) | Materialize TripSeats per trip | Accepted |
| [0002](./0002-user-internal-guid-with-auth0-sub-unique.md) | User cache keyed by internal Guid `Id` with unique `Auth0Sub` | Accepted |
| [0003](./0003-time-provider-for-testable-time.md) | Use `TimeProvider` for time-dependent domain logic | Accepted |
| [0004](./0004-domain-events-without-pii.md) | Domain events carry identifiers only, never PII or rich payload | Accepted |
| [0005](./0005-trip-seat-as-separate-aggregate.md) | `TripSeat` is a separate aggregate root, not an inner entity of `Trip` | Accepted |

## Writing a new ADR

1. Copy `0001-trip-seats-denormalization.md` as a starting point.
2. Increment the number. Use the next free integer, zero-padded to 4 digits.
3. Kebab-case the title in the filename: `NNNN-short-descriptive-title.md`.
4. Fill the sections: Context → Decision → Consequences → Alternatives → Revisit trigger.
5. Open a PR. ADRs are reviewed like code; disagreement is the point.
6. On merge, add the new entry to the index above.
