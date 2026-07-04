# Cirreum.AuthenticationProvider Changelog

All notable changes to **Cirreum.AuthenticationProvider** are documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) — [SemVer](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

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
