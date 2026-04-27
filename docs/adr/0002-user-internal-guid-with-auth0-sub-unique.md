# ADR-0002: User cache keyed by internal Guid `Id` with unique `Auth0Sub`

## Status

Accepted — 2026-04-25
Supersedes the original schema sketch where `Auth0Sub` was the primary key.

## Context

Authentication is delegated to Auth0. Each authenticated user is identified externally by an Auth0 `sub` claim — a string of the form `auth0|6534abc123` or `google-oauth2|10293...`. We need a local `User` table to cache profile fields (email, full name) so that reservation reads don't have to call the Auth0 Management API on every request.

The question: what is the primary key of the `User` table, and what foreign key do `Reservations` and any future user-referencing rows use?

There are two natural options:

1. **`Auth0Sub` as PK.** The local table mirrors Auth0's identifier directly. Reservations FK to a `nvarchar` column.
2. **Internal Guid `Id` as PK with `Auth0Sub` as a separate unique column.** The Auth0 sub is *cached data*, not identity, and the local `Id` is what every other table references.

Initially we sketched option 1, then switched to option 2.

## Decision

We use **internal Guid `Id` as the primary key of `User`**, with `Auth0Sub` as a separate `nvarchar` column constrained by a unique index (`IX_User_Auth0Sub_Unique`).

Foreign keys from other tables (currently `Reservations.UserId`, possibly more later) point at `User.Id`.

The flow on each authenticated request:

1. Middleware reads the `sub` claim from the validated JWT.
2. Looks up `User` by `Auth0Sub`. If missing, inserts a new row with `Id = Guid.CreateVersion7()`, `Auth0Sub = sub`, profile fields from claims, and `LastSyncedAt = now`.
3. Exposes the resulting internal `User.Id` to handlers via the request context.

Handlers and aggregates only ever see the internal `Id`. The Auth0 sub is an implementation detail of the identity bridge, not part of the domain language.

## Consequences

### Positive

- **Identity-provider independence.** If we replace Auth0 with Keycloak, Cognito, or a self-hosted IdP later, the change is local: rename `Auth0Sub` to whatever, re-sync values. Foreign keys throughout the schema don't move.
- **All FKs use the same primitive type.** `Reservations.UserId` is `uniqueidentifier`, just like `Reservations.TripId` and every other FK in the database. SQL plans, EF Core mappings, indexing strategies all stay uniform.
- **Composite locality.** Anything that references a user does so by Guid. Joins are 16 bytes, not variable-length strings of 30–40 characters.
- **Domain stays clean.** The `User` aggregate's `Auth0Sub` field is just data. The Domain layer doesn't know it's special; no Domain code looks up users by sub. Lookup by sub lives entirely in the Application/Infrastructure boundary (the user-cache middleware).

### Negative

- **One extra hop per authenticated request to translate sub → Id.** A `SELECT Id FROM Users WHERE Auth0Sub = @sub` per request. Mitigated by:
  - Cheap query on a unique-indexed column.
  - The middleware can cache the mapping in `IMemoryCache` for the lifetime of the JWT.
  - On a busy request the user is already in the EF `ChangeTracker` after first lookup.
- **Slightly more code in middleware.** Has to do lookup + insert-if-missing dance instead of just using sub directly.

### Neutral

- The `User` row count equals "distinct authenticated users that have ever hit the API," which is the same in both options. No storage difference.

## Alternatives considered

### Option 1 — `Auth0Sub` as primary key

Reservations FK to `User.Auth0Sub` (string). Middleware does no lookup beyond verifying the user exists.

**Rejected because:**

- **Identity-provider lock-in.** Auth0 sub strings are baked into every foreign key, every domain reference, every audit log. Migrating to another IdP becomes a schema-wide rewrite.
- **String FKs are noisy.** `nvarchar(255)` foreign keys eat more storage and fragment indexes more than `uniqueidentifier`. Not catastrophic, but unnecessary tax.
- **Domain leakage.** Aggregates would carry around an Auth0-specific identifier as part of their identity, encoding "we use Auth0" into the domain language.

### Option 3 — User has both, with a generated foreign-key shim

Two columns, both queryable, FK from somewhere external could go either way. Considered briefly and rejected as ambiguous: when there are two valid keys, every reader has to ask "which one am I using?" — and that question costs more than the migration insurance is worth.

## Revisit trigger

Revisit this decision if:

- We add a second source of identity (e.g., service accounts that don't come from Auth0). At that point the lookup model shifts and the schema may need a discriminator column.
- The middleware's sub-to-Id lookup ever shows up as a hot spot in production profiling. Unlikely with a unique index, but possible with extreme RPS.
- We adopt strongly-typed IDs project-wide. `User.Id` would become `UserId` (record struct around Guid), but the structural decision in this ADR doesn't change.

## References

- `docs/database-schema.md` — current schema
- `README.md` → 🔑 Auth0 setup — middleware flow described in plain language
- ADR-0003 — TimeProvider, which interacts with `User.LastSyncedAt`