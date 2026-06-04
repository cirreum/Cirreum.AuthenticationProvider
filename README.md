# Cirreum.AuthenticationProvider

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.AuthenticationProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.AuthenticationProvider/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.AuthenticationProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.AuthenticationProvider/)
[![License](https://img.shields.io/github/license/cirreum/Cirreum.AuthenticationProvider?style=flat-square&labelColor=1F1F1F&color=F2F2F2)](https://github.com/cirreum/Cirreum.AuthenticationProvider/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Authentication track abstractions for the Cirreum framework — the contracts scheme implementations and runtime composition build on.**

## Overview

`Cirreum.AuthenticationProvider` is the abstraction layer for Cirreum's Authentication pillar (one of the three-pillar separation: Identity / Authentication / Authorization). It defines the contracts that:

- **Scheme packages** implement — `Cirreum.Authentication.ApiKey`, `Cirreum.Authentication.SignedRequest`, `Cirreum.Authentication.SessionTicket`, etc.
- **Runtime composition** wires — `Cirreum.Runtime.AuthenticationProvider` (the dynamic forward scheme resolver, auth-event hosted handlers, cache invalidators, boot-time analyzers)
- **The umbrella package** exposes — `Cirreum.Runtime.Authentication` (the `AddAuthentication(...)` app-facing builder)

### Contract surface

**Schemes:**
- `ISchemeSelector` + `SchemeCategory` enum — per-request scheme dispatch (open/closed)
- `CredentialTransport` enum — where the scheme reads its credential
- `ISignedRequestAlgorithm` + resolver — version-pluggable crypto for SignedRequest scheme
- `SessionTicket` family — HTTP→long-lived-connection handoff primitives

**Patterns:**
- `IAuthenticationBuilder` — the builder surface scheme registrations extend
- `[AllowPendingAuth]` — opt-in for Two-Phase Auth's anonymous-pending-auth pattern

**Compositions:**
- `IAuthenticationBoundaryResolver` — resolves Global vs Tenant vs None per the configured PrimaryScheme
- `ClaimsUserProfileEnricher` — default `IUserProfileEnricher` impl
- `IUserProfileEnrichmentBuilder`, `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder` — extensibility seams for claim enrichment scenarios
- `IRevokedCredentialProvider` — app-side credential revocation hydration

## Where it fits

```
Cirreum.Kernel                  (auth event bus — IAuthenticationEventPublisher/Handler)
Cirreum.AuthenticationProvider  ← this package
Cirreum.Authentication.{Scheme} — ApiKey, SignedRequest, SessionTicket, ...
Cirreum.Runtime.AuthenticationProvider
Cirreum.Runtime.Authentication  (app-facing umbrella; AddAuthentication(...))
```

## License

MIT — see [LICENSE](LICENSE).
