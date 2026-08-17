# Cirreum.AuthenticationProvider

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.AuthenticationProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.AuthenticationProvider/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.AuthenticationProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.AuthenticationProvider/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.AuthenticationProvider?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.AuthenticationProvider/releases)
[![License](https://img.shields.io/badge/license-MIT-F2F2F2?style=flat-square&labelColor=1F1F1F)](https://github.com/cirreum/Cirreum.AuthenticationProvider/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Authentication track abstractions for the Cirreum framework — the contracts scheme implementations and runtime composition build on.**

## Overview

`Cirreum.AuthenticationProvider` is the abstraction layer for Cirreum's Authentication track — one of the three that separate concerns across identity: Identity, Authentication, and Authorization. It defines the contracts that:

- **Scheme packages** implement — `Cirreum.Authentication.ApiKey`, `Cirreum.Authentication.SignedRequest`, `Cirreum.Authentication.SessionTicket`, etc.
- **Runtime composition** wires — `Cirreum.Runtime.AuthenticationProvider` (the dynamic forward scheme resolver, auth-event hosted handlers, cache invalidators, boot-time analyzers)
- **The umbrella package** exposes — `Cirreum.Runtime.Authentication` (the `AddAuthentication(...)` app-facing builder)

### Contract surface

**Registrars** — the base every scheme package derives from:
- `AuthenticationProviderRegistrar<TSettings, TInstanceSettings>` — per-instance registration, scheme-name derivation (the instance key **is** the scheme name), and instance-key deduplication
- `HeaderAuthenticationProviderRegistrar<,>` / `AudienceAuthenticationProviderRegistrar<,>` — the host-type-sensitivity split, by credential transport
- `ProviderName` and `SubjectKind` — the two constants a provider declares. `SubjectKind` states whether the provider authenticates people or machines, so nothing downstream has to infer it from a token's contents

**Schemes:**
- `ISchemeSelector` + `SchemeCategory` enum — per-request scheme dispatch (open/closed)
- `CredentialTransport` enum — where the scheme reads its credential
- `ISignedRequestAlgorithm` + resolver — version-pluggable crypto for SignedRequest scheme
- `SessionTicket` family — HTTP→long-lived-connection handoff primitives

**Settings:**
- `AuthenticationProviderSettings<TInstanceSettings>` / `AuthenticationProviderInstanceSettings` — the configuration bases bound from `Cirreum:Authentication:Providers:{ProviderName}`
- `ClaimAuthoritySettings` — the optional per-instance `ClaimAuthority` block declaring who owns a scheme's callers, the identity provider or the application's own store, separately for `Profile` and `Roles`

**Patterns:**
- `IAuthenticationBuilder` — the builder surface scheme registrations extend
- `[AllowPendingAuth]` — opt-in for Two-Phase Auth's anonymous-pending-auth pattern

**Compositions:**
- `AudienceSchemeRegistration` — the audience → scheme routing contribution audience-based registrars add per instance
- `SchemeClaimAuthorityRegistration` — the claim-authority declaration contributed per registered scheme, aggregated by the runtime
- `IRevokedCredentialProvider` — app-side credential revocation hydration
- `ConfigureCoordination(...)` / `AddDefaultCoordinationScope()` — auth-track conveniences over the neutral `Cirreum.Coordination` primitive, letting the backend be chosen inside the composition callback and defaulting the scope to `{app}:{env}`

Profile enrichment (`IUserProfileEnrichmentBuilder`, `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder`, `ClaimsUserProfileEnricher`) is **not** part of this package — it's host-agnostic (any host may enrich a profile post-authentication, regardless of which — or whether any — auth scheme is active) and lives in `Cirreum.Contracts`/`Cirreum.Domain` instead. The same reasoning applies to authentication-boundary resolution (`IAuthenticationBoundaryResolver`, `AuthenticationBoundary`): the server user-state pipeline consumes it whether or not any authentication scheme is composed, so it lives in `Cirreum.Kernel`.

Attribute authority splits along the same line. The *vocabulary* — `SubjectKind`, `ClaimAuthority`, `SchemeClaimAuthority`, `ISchemeClaimAuthorityMap` — lives in `Cirreum.Kernel`, because operation authorizers read the resolved answer off `IUserState` and sit below this package. What lives here is the **declaring**: the registrar constant, the settings block, and the per-scheme contribution that carries them into composition.

## Where it fits

```
Cirreum.Kernel                  (auth event bus — IAuthenticationEventPublisher/Handler;
                                 attribute-authority vocabulary — SubjectKind, ClaimAuthority)
Cirreum.AuthenticationProvider  ← this package
Cirreum.Authentication.{Scheme} — ApiKey, SignedRequest, SessionTicket, ...
Cirreum.Runtime.AuthenticationProvider
Cirreum.Runtime.Authentication  (app-facing umbrella; AddAuthentication(...))
```

## License

MIT — see [LICENSE](LICENSE).
