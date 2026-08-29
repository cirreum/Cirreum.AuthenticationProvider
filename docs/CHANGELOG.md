# Cirreum.AuthenticationProvider Changelog

All notable changes to **Cirreum.AuthenticationProvider** are documented in this file.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) — [SemVer](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Updated

- **`IRevokedCredentialProvider` documents what its result set must cover.** The remarks stated
  how long a revocation must be retained, but not which credentials have to appear in the set to
  begin with: it must cover every credential a scheme's credential resolver can still return.

## [3.2.0] - 2026-08-24

### Changed

* **Removed the dormant `BearerCredential` reader.** Shipped in 3.1.0 to let every scheme
  selector and handler read a query-carried credential, for the case a browser cannot avoid:
  it can set no headers on a WebSocket upgrade, so its bearer token arrives as an
  `access_token` query parameter.

  That approach was replaced before anything adopted it. `Cirreum.Services.Server` 1.6.0
  promotes such a credential into the `Authorization` header before authentication runs, so
  every scheme and selector reads it where it always has and none of them need to know the
  case exists — including schemes added later, and the JWT audience-routing selector in
  `Cirreum.Runtime.Authentication`, which the per-scheme approach would have missed.

  Deleted under the dormant-surface rule: no framework code called either member, and public
  surface nothing consumes is surface that accumulates consumers by accident. Shipped as a
  Minor deliberately — with zero consumers, the removal cannot break a compile anywhere.


## [3.1.0] - 2026-08-23

### Added

* **`BearerCredential`** — the single reader for an inbound bearer credential, used by both
  scheme selectors, which choose the handling scheme, and scheme handlers, which validate it.
  Reads `Authorization: Bearer`, and otherwise the `access_token` query parameter when the
  request targets a connection endpoint. A browser cannot set
  headers on a WebSocket upgrade, so a query-carried token is the only credential such a
  client can present, and it is the convention SignalR's own clients follow. The header always
  wins, and the query is never consulted for an ordinary endpoint, so a query parameter
  outside a connection upgrade carries no authority.
  Connection endpoints are recognized two ways: a SignalR hub by SignalR's own hub metadata,
  and everything else by `InvocationConnectionMetadata`, which ships in `Cirreum.Contracts`
  4.7.0 and which the server spine stamps on the connection endpoints it maps.

### Fixed

* **The test suite compiles again.** Its registrar doubles and call sites still used the
  pre-consolidation signatures — `AuthenticationBuilder` in place of `IAuthenticationBuilder`,
  and `Register` / `RegisterInstance` taking the service collection and configuration
  separately rather than the composed builder — so the project had not built since that
  consolidation. Release runs skip test solutions by default, which is why no release caught
  it. The doubles are updated and a `TestAuthenticationBuilder` supplies the interface, whose
  shipping implementation lives above this layer.

## [3.0.5] - 2026-08-23

### Updated

- Updated NuGet packages.

## [3.0.4] - 2026-08-20

### Updated

- Updated NuGet packages.

## [3.0.3] - 2026-08-18

### Updated

- Updated NuGet packages.

## [3.0.2] - 2026-08-17

### Updated

- Updated NuGet packages.

## [3.0.1] - 2026-08-17

### Added

- **`IAuthenticationBuilder` gains the registration funnel.** Two members make registering a
  scheme and declaring what it authenticates one act:
  - `DeclareScheme(scheme, subjectKind, profile, roles)` — contributes the scheme's
    `SchemeClaimAuthorityRegistration` for the map the runtime aggregates at composition close.
    Used beside scheme registrations an external verb performs (`AddJwtBearer`,
    `AddOpenIdConnect`, `AddCookie`, `AddMicrosoftIdentityWebApi`).
  - `AddScheme<TOptions, THandler>(scheme, subjectKind, profile, roles, configureOptions)` —
    registers the ASP.NET handler scheme *and* contributes the declaration. The Cirreum
    composition surface cannot register a scheme undeclared; registering through `AuthBuilder`
    directly is the visible, deliberate exemption.

  New abstract members on a shipped interface are breaking for implementers; the framework's
  one implementer (`CirreumAuthenticationBuilder`) updates in the same wave. Part of the
  pre-adoption reshape below.
- **`AudienceAuthenticationProviderRegistrar` declares in `RegisterScheme`.** Every audience
  instance now contributes its declaration — the registrar's `SubjectKind` plus the instance's
  `ClaimAuthority` block — beside its `AudienceSchemeRegistration`, at the same moment the
  scheme registration is dispatched.
- **`Scheme` on `SessionTicket` and `SessionTicketIssueRequest`** — the authentication scheme
  that established the ticket's subject. A session ticket is a continuation; validators stamp
  this as the origin scheme (`AuthenticationContextKeys.OriginScheme`, Kernel 2.1.1) so the
  subject's declaration re-resolves from the scheme that authenticated them. Optional — a
  ticket without an origin resolves `SubjectKind.Unknown`, the fail-safe.

### Changed

- **The registrar contract consolidates onto `IAuthenticationBuilder`.** Breaking on paper and
  shipped as a patch deliberately: a post-release, pre-adoption reshape (no released package
  consumes 3.0.0 — all six providers are held), released with `-AllowBreakingPatch`. The builder
  carries everything the old parameter triple did (`Services`, `Configuration`, `AuthBuilder`).
  Find/replace for registrar implementations:

  | Before | After |
  |---|---|
  | `Register(TSettings, IServiceCollection, IConfiguration, AuthenticationBuilder)` | `Register(TSettings, IAuthenticationBuilder)` |
  | `RegisterInstance(string, TInstanceSettings, IServiceCollection, IConfiguration, AuthenticationBuilder)` | `RegisterInstance(string, TInstanceSettings, IAuthenticationBuilder)` |
  | `RegisterScheme(string, TInstanceSettings, IServiceCollection, IConfiguration, AuthenticationBuilder)` | `RegisterScheme(string, TInstanceSettings, IAuthenticationBuilder)` |
  | `AddAuthenticationHandler(string, TInstanceSettings, IServiceCollection, IConfiguration, AuthenticationBuilder)` | `AddAuthenticationHandler(string, TInstanceSettings, IAuthenticationBuilder)` |
  | `AddAuthenticationForWebApi(IConfigurationSection, TInstanceSettings, AuthenticationBuilder)` | `AddAuthenticationForWebApi(IConfigurationSection, TInstanceSettings, IAuthenticationBuilder)` |
  | `AddAuthenticationForWebApp(IConfigurationSection, TInstanceSettings, AuthenticationBuilder)` | `AddAuthenticationForWebApp(IConfigurationSection, TInstanceSettings, IAuthenticationBuilder)` |

- `SessionTicket` record docs no longer describe a JWT variant or enumerate transports — the
  opaque store-validated ticket is the product, and transports are the scheme package's concern.
- **`RegisterInstance` no longer contributes `SchemeClaimAuthorityRegistration`.** The 3.0.0
  contribution was keyed on the instance key, which is not the scheme name for multi-transport
  providers (`ApiKey:{transport}`) — a record no lookup would ever find. The declaration now
  anchors where the scheme is actually registered: the audience base for audience providers,
  each provider's own registration path otherwise.

### Removed

- **`AuthenticationSchemes.ApiKey`, `.SessionTicket`, `.SignedRequest`, and
  `.AnonymousPendingAuth`.** The first two named schemes that no longer exist (real names:
  `ApiKey:Bearer` / `ApiKey:{header}`, `SessionTicket:Bearer`), the third duplicated
  `SignedRequestSchemes.Default`, and the fourth named a scheme that was never registered.
  Zero consumers verified framework-wide. The class now holds only the schemes the umbrella
  itself registers (`Dynamic`, `Ambiguous`, `Anonymous`); provider scheme names are owned by
  their provider packages (`ApiKeySchemes`, `SessionTicketSchemes`, `SignedRequestSchemes`).
- **`AllowPendingAuthAttribute`.** Shipped in 1.0.0 as a marker and never consumed — no
  pipeline code exempted decorated endpoints, and the boot-time pairing validation its docs
  described was never built. The anonymous-pending-auth *flow* is unaffected: it is carried by
  endpoint-level anonymous access, per-invocation default-deny operation authorization, and
  `connection.Promote(...)`, none of which read the attribute. A future revival gets real
  enforcement designed against `IUserState.SubjectKind`, where the pending window is
  `SubjectKind.Unknown` with no origin scheme stamped.

## [3.0.0] - 2026-08-16

### Updated

- Updated NuGet packages.

### Breaking

- **`AuthenticationProviderRegistrar<,>` gains an abstract `SubjectKind`.** Every registrar now
  declares whether it authenticates people or machines. Framework-shipped registrars answer it in
  the same wave; an application with its own custom registrar must add one line
  (`public override SubjectKind SubjectKind => SubjectKind.Human;` for a scheme carrying real
  users). Abstract rather than defaulted on purpose: a provider that never answers is one whose
  callers cannot be classified, and that belongs at compile time rather than surfacing as an
  unexplained `Unknown` in production. Answer it from what the provider authenticates, never from
  its transport — header-based does not imply machine, since session tickets carry people.

### Added

- **`ClaimAuthoritySettings` on every authentication instance, as an optional `ClaimAuthority`
  block.** Declares which side owns a scheme's callers — the identity provider or the
  application's own store — separately for `Profile` and `Roles`, because the two genuinely
  differ: an identity provider federating an external account may supply the profile while the
  application still owns the roles. Each is a *precedence* rule, not exclusivity — the declared
  side wins where both supply a value, and neither is blocked from supplying one, which is what
  lets one scheme serve both a federated account carrying full claims and a local account
  arriving thin. Omitting the block declares nothing and preserves existing behavior: roles come
  from the application store when a resolver is registered for the scheme, and from the identity
  provider otherwise.
- **`AddDefaultCoordinationScope()`** — registers the canonical `{app}:{env}` coordination scope,
  extracted from `ConfigureCoordination` so it has one definition. `AddEventCoordination` in
  `Cirreum.Runtime.Authentication` carried a verbatim copy — identical registration, identical
  exception text — and can now call this instead. The scope stays deliberately opaque to
  `Cirreum.Coordination`, which holds no opinion about what an application or environment is;
  supplying the default belongs to a composition surface that knows `IDomainEnvironment`, and
  this is that surface.
- **Per-scheme declaration published at composition.** `RegisterInstance` contributes a
  `SchemeClaimAuthorityRegistration` for each registered scheme, after validation so a failing
  instance declares nothing. The runtime aggregates these into the map its claims transformation
  and user-state accessor read, replacing three separate inferences drawn from token contents.

## [2.0.5] - 2026-08-04

### Updated

- Updated NuGet packages.

## [2.0.4] - 2026-08-03

### Updated

- Updated NuGet packages.

## [2.0.3] - 2026-07-31

### Updated

- Re-pinned `Cirreum.Contracts` `4.0.0` → `4.0.1` (carries `Cirreum.Kernel` 2.0.1, completing
  the wave bottom-up).

## [2.0.2] - 2026-07-31

### Updated

- Re-pinned `Cirreum.Contracts` `2.0.0` → `4.0.0`, converging on the current major (this repo
  had been skipped by the 3.0.0 and 4.0.0 repin waves; no consumed surface changed).

## [2.0.1] - 2026-07-29

### Updated

- Updated NuGet packages.

## [2.0.0] - 2026-07-26

### Changed

- **`IRevokedCredentialProvider` yields revocation records instead of bare identifiers.**
  `GetRevokedCredentialIdsAsync` → `GetRevokedCredentialsAsync`, returning
  `IAsyncEnumerable<RevokedCredential>` where `RevokedCredential` is a new
  `readonly record struct` carrying the credential id and its optional expiry.

  Previously the contract could only express *which* credentials were revoked, so a boot-hydrated
  revocation was recorded with no expiry and retained until the process restarted — while an entry
  created by a live `CredentialRevoked` event self-evicted on the credential's own expiry, because
  `IApiKeyDenylist.Revoke(id, expiresAt)` has always accepted one. The hydration path was the only
  place that couldn't supply it. It now can.

  Behavior is unchanged for a provider that supplies no expiry: `null` means "retain until restart",
  which is safe — over-retention costs memory, under-revocation would re-admit a credential.

  A `readonly record struct` rather than a class because this streams through `IAsyncEnumerable<T>`
  on the boot path, and the populations that make expiry worth carrying are exactly the large ones.

  The method is renamed rather than overloaded so an implementer gets a clean compile error instead
  of a type mismatch on a member whose name still says "Ids". See `MIGRATION-v2.md`.

### Added

- `IRevokedCredentialProvider` now documents the operational constraint that was previously only
  implicit: **keep the persisted revoked set bounded.** Everything it yields is held in memory for
  the process lifetime, and the in-memory denylist is capacity-bounded — on saturation it *fails
  authentication closed* rather than silently dropping a revocation, so an unbounded set degrades
  into refused authentication rather than stale state. Records the safe pruning rule (remove once
  the credential could not authenticate anyway; never prune a live, non-expired credential's
  revocation) and why boot-hydrated entries cannot self-evict the way event-driven ones do.
  Documentation only — no behavior change.

### Removed

- **`AuthenticationDiagnostics`** — a public static class whose sole member,
  `DiagnosticName = "Cirreum.AuthenticationProvider"`, had **no references anywhere in the
  framework**. Its documentation claimed the `ActivitySource` and `Meter` were "created in the
  runtime composition" and that the constant existed "so both agree on the identifier"; neither was
  true. The runtime's authentication telemetry uses `Cirreum.Authentication`, and that name is what
  `CirreumTelemetry` registers.

  Worse than dead: the name it published is not among the sources or meters Kernel's `AddCirreum()`
  subscribes, so a future author who reached for it — exactly as its documentation invited — would
  have created a source with no listener and shipped telemetry that records into the void with
  nothing failing to say so. The same defect that left identity-provisioning telemetry unobservable
  until `Cirreum.Kernel` 1.3.0.

  Telemetry names belong to `CirreumTelemetry` in `Cirreum.Kernel`, which is both the single
  literal and the registration. See `MIGRATION-v2.md`.

## [1.4.2] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.4.1] - 2026-07-20

### Updated

- Updated NuGet packages.

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
