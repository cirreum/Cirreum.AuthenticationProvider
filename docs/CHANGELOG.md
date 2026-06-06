# Cirreum.AuthenticationProvider Changelog

All notable changes to **Cirreum.AuthenticationProvider** are documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) — [SemVer](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- **Atomic coordination primitives** (`Cirreum.AuthenticationProvider.Coordination`) — `IReplayGuard` (single-use claim for the SignedRequest strict-nonce posture / replay protection) and `IRequestThrottle` (fixed-window rate limit for the ApiKey `SelfContained` conformance profile). Two interfaces, not one, because store capability is asymmetric: atomic set-if-absent is near-universal, an atomic windowed counter is the discriminator. Ships built-in single-instance implementations — `InMemoryReplayGuard` (lock-free compare-and-swap set-if-absent) and `InMemoryRequestThrottle` (`Interlocked` fixed-window counter) — with monotonic `Environment.TickCount64` deadlines (clock-step-immune), single-sweeper opportunistic eviction (bounds memory under high-cardinality keys), and fail-closed rejection of non-positive windows/limits. Register the backend once via the `AddCoordination(c => c.UseInMemory())` verb on `IAuthenticationBuilder` (a public `CoordinationBuilder` that distributed adapters such as `Cirreum.Authentication.Coordination.Redis` extend with `UseXxx()`); schemes consume only the interface they need. The pure cache-aside `ICacheService` is intentionally NOT the home for these — atomic coordination is an Auth-vertical concern, not a caching one.

### Changed

- Re-pinned `Cirreum.Contracts` 1.1.0 → 1.1.1 (code-first caching foundation; no source impact — none of the renamed/removed cache types are referenced here).

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
