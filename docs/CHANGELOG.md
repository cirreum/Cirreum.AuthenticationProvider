# Cirreum.AuthenticationProvider Changelog

All notable changes to **Cirreum.AuthenticationProvider** are documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) — [SemVer](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

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
