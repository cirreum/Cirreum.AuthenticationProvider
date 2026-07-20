# Cirreum.AuthenticationProvider Changelog

All notable changes to **Cirreum.AuthenticationProvider** are documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) — [SemVer](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

## [1.4.0] - 2026-07-20

### Changed

- **Authentication-boundary resolution relocated to `Cirreum.Kernel` (ADR-0032).**
  `IAuthenticationBoundaryResolver` and `DefaultAuthenticationBoundaryResolver` move
  to the Kernel's `Cirreum.Security` namespace, beside the `AuthenticationBoundary`
  enum, `IUserState`, and `UserStateBase` they operate on; the never-called
  `AddAuthenticationBoundaryResolver` extension is deleted. The seam is spine
  infrastructure — the server user-state pipeline consumes it whether or not any
  authentication scheme is composed — and its placement here forced the services
  spine to reference this package for one interface. Removal ships in a minor under
  the same recorded rewrite-completion deviation as 1.3.0; implementers change
  `using Cirreum.AuthenticationProvider.Security` to `using Cirreum.Security`.
  Registration is now consumer-owned: `Cirreum.Services.Server` registers the
  default, and `Cirreum.Runtime.Authentication` registers the scheme-aware
  primary-scheme resolver restored under the same ADR.

## [1.3.0] - 2026-07-20

### Added

- `AudienceSchemeRegistration` — an immutable `audience → scheme` routing contribution
  record. Audience-based registrars now register one per enabled instance directly into
  the service collection; the runtime's audience selector aggregates the set at
  construction and the umbrella validates it at composition close (ADR-0031).

### Changed

- **Audience dispatch data is container-owned (ADR-0031).** The mutable
  `IAudienceSchemeMap` / `DefaultAudienceSchemeMap` pair is removed outright — under the
  umbrella composition its find-or-create reuse check could never succeed, so each
  audience instance silently created a fresh map and only the last-registered audience
  remained routable (every other audience-based scheme 401'd as `Cirreum.Ambiguous`).
  The types were unshippable in practice and no correct program could have consumed
  them; removal ships in this minor as a deliberate, recorded SemVer deviation while
  the authentication rewrite completes. Custom audience routing belongs on the
  `ISchemeSelector` seam; manual mappings are contributed via
  `services.AddSingleton(new AudienceSchemeRegistration(...))`.

### Fixed

- The registrar base's duplicate-instance-key guard was process-global static state:
  a second host composed in the same process (the integration-test norm) rejecting a
  legitimately re-used instance key with "already been registered". Guard state now
  lives in the service collection, so hosts are fully isolated (ADR-0028 principle).

## [1.2.2] - 2026-07-18

### Updated

- Updated NuGet packages.

## [1.2.0] - 2026-07-06

### Added

- `ConfigureCoordination(...)` now defaults the `CoordinationScope` to the canonical `{applicationName}:{environmentName}` (from `IDomainEnvironment`) when none is registered, so applications and environments sharing a distributed coordination backend never share replay/throttle/signal state. An explicit `configure(c => c.WithScope(...))` wins in any order (the default is `TryAdd`; `WithScope` replaces); the in-memory backend ignores the scope. Matches the same default `auth.AddEventCoordination()` applies in `Cirreum.Runtime.Authentication`.

### Fixed

- `[AllowPendingAuth]` docs named the removed `TwoPhaseAuth.Promote` static helper; the promotion path is now the `connection.Promote(principal)` extension in `Cirreum.Runtime.AuthenticationProvider`.

## [1.1.3] - 2026-07-05

### Fixed

- Renamed `AddCoordination` to `ConfigureCoordination` on `IAuthenticationBuilder`, matching the framework's `Configure*` convention (adjusting an already-implied capability — see `ConfigureCors`/`ConfigureConductor`) rather than `Add*` (registering a new one). No functional change — same signature, same forward to `services.AddCoordination(...)`. Source-breaking: update any call site from `auth.AddCoordination(...)` to `auth.ConfigureCoordination(...)`. Ships as a patch — no known external consumers of the verb yet, no `[Obsolete]` shim, no migration doc. Also fixes a stale doc/comment reference to ApiKey's `SelfContained` profile, dropped in the 2026-06-08 redesign.

## [1.1.2] - 2026-07-04

### Fixed

- **Relocated the profile-enrichment builder family to `Cirreum.Contracts`/`Cirreum.Domain`.** `IUserProfileEnrichmentBuilder`, `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder`, and `ClaimsUserProfileEnricher` are removed from this package. They're host-agnostic — any host may enrich a user's profile after authentication, regardless of which (or whether any) auth scheme is active — so they belong in the spine, not the Authentication feature track. Landing them here meant `IUserProfileEnrichmentBuilder` inherited this package's `IAuthenticationBuilder`, which carries server-only `AuthBuilder`/`Configuration` members for the `AddAuthentication()` composition surface — silently breaking every Blazor WebAssembly implementer (there's no server-side `AuthenticationBuilder` on a WASM client). `IGraphEnabledBuilder`/`IExternalGraphEnabledBuilder` only shipped here in `v1.1.1`, hours before this fix; `IUserProfileEnrichmentBuilder`/`ClaimsUserProfileEnricher` shipped since `v1.0.0`. Now in `Cirreum.Contracts 1.2.0` (interfaces) and `Cirreum.Domain 1.2.0` (default enricher impl).

## [1.1.1] - 2026-07-04

### Fixed

- **`IGraphEnabledBuilder` / `IExternalGraphEnabledBuilder` actually ship now.** The `v1.0.0` changelog and README both documented these as relocated here from `Cirreum.Core 5.x` alongside `IAuthenticationBuilder`/`IUserProfileEnrichmentBuilder`, but the two Graph interfaces were never actually ported — only claimed. They remained real only in the (now-archived) legacy `Cirreum.Core` source, silently blocking `Cirreum.Graph.Provider`'s foundation cutover. Ported verbatim (same shape, root `Cirreum` namespace) so existing `Cirreum.Core`-based consumers see no source break when they switch their reference.

## [1.1.0] - 2026-07-03

### Added

- **`auth.AddCoordination(...)` forwarder** — a thin auth-track convenience on `IAuthenticationBuilder` over the neutral `Cirreum.Coordination` primitive, exposing `services.AddCoordination(...)` as `auth.AddCoordination(c => c.UseInMemory())` (or `c => c.UseRedis()` with `Cirreum.Coordination.Redis` referenced) so auth schemes can register a coordination backend inside the `AddAuthentication` composition. Schemes pull the requirement they need — SignedRequest's strict-nonce posture consumes `IReplayGuard`; a fixed-window `IRequestThrottle` is available for rate-limited schemes. The coordination primitives themselves live in the standalone, dependency-light `Cirreum.Coordination` package (usable outside authentication), not here — atomic coordination is a reusable primitive, not an auth-only concern.

### Changed

- New dependency: `Cirreum.Coordination 1.0.0` (the coordination primitives the `AddCoordination` forwarder delegates to).
- Re-pinned `Cirreum.Contracts` → 1.1.1 (code-first caching foundation; no source impact — none of the renamed/removed cache types are referenced here).

## [1.0.0] - 2026-06-05

### Added

- Initial release. Cirreum.AuthenticationProvider is the abstractions layer for the Authentication pillar of the Cirreum framework, established as part of the **Cirreum 1.0 Foundation Reset** wave.
- Relocated from former `Cirreum.Core 5.x`:
  - `IAuthenticationBuilder` — builder surface for scheme registrations; carries `Services`, the ASP.NET `AuthenticationBuilder`, and the host `IConfiguration` so app-facing composition verbs like `AddApiKey(...)` can bind their provider's appsettings section
  - `IUserProfileEnrichmentBuilder` — extensibility for claim-based profile enrichment
  - `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder` — msgraph-driven enrichment scenarios
  - `IAuthenticationBoundaryResolver` — Global vs Tenant vs None boundary resolution
  - `ClaimsUserProfileEnricher` — default `IUserProfileEnricher` implementation
- New contracts:
  - `ISchemeSelector` + `SchemeCategory` — per-request scheme dispatch (open/closed model)
  - `CredentialTransport` — where the scheme reads its credential
  - `[AllowPendingAuth]` — Two-Phase Auth anonymous-pending-auth opt-in marker
  - `AuthenticationSchemes` static constants — well-known scheme name centralization
  - `IRevokedCredentialProvider` — app-side credential revocation hydration
  - `ISignedRequestAlgorithm` + `ISignedRequestAlgorithmResolver` — RFC 9421 version-pluggable crypto consumed by the SignedRequest scheme
  - `SessionTicket` record + `SessionTicketIssueRequest` + `ISessionTicketIssuer` + `ISessionTicketValidator` + `ISessionTicketPrincipalBinder` — HTTP→long-lived-connection handoff primitives consumed by the SessionTicket scheme
  - `ISessionStore` — session-ticket persistence; exposes an atomic single-use `ConsumeAsync` (retrieve-and-remove in one operation, so concurrent handshakes can't both redeem a ticket) alongside `StoreAsync` / `RetrieveAsync` / `RemoveAsync` / `RemoveBySubjectAsync`

  See [`MIGRATION-v1.md`](MIGRATION-v1.md) for migration from `Cirreum.Core 5.x`.
