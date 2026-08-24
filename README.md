# Cirreum.AuthenticationProvider

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.AuthenticationProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.AuthenticationProvider/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.AuthenticationProvider.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.AuthenticationProvider/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.AuthenticationProvider?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.AuthenticationProvider/releases)
[![License](https://img.shields.io/badge/license-MIT-F2F2F2?style=flat-square&labelColor=1F1F1F)](https://github.com/cirreum/Cirreum.AuthenticationProvider/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Authentication track abstractions for the Cirreum framework — the contracts scheme implementations and runtime composition build on.**

## Overview

`Cirreum.AuthenticationProvider` provides the shared contracts for Cirreum authentication providers.

It sits between individual authentication scheme packages and the runtime that composes them:

- **Scheme packages** implement these contracts — `Cirreum.Authentication.ApiKey`,
  `Cirreum.Authentication.SignedRequest`, `Cirreum.Authentication.SessionTicket`, etc.
- **Runtime composition** consumes them — `Cirreum.Runtime.AuthenticationProvider`
- **Applications** access them through — `Cirreum.Runtime.Authentication`

## Contract surface

### Provider registration

- `AuthenticationProviderRegistrar<TSettings, TInstanceSettings>` — base registrar for
  provider instances, scheme naming, and instance deduplication
- `HeaderAuthenticationProviderRegistrar<,>` / `AudienceAuthenticationProviderRegistrar<,>` —
  registrar specializations by credential routing model
- `ProviderName` and `SubjectKind` — provider identity and authenticated subject classification

### Scheme contracts

- `ISchemeSelector` / `SchemeCategory` — per-request authentication scheme selection
- `CredentialTransport` — identifies where a scheme receives its credential
- `BearerCredential` — reads the inbound bearer credential for both selectors and handlers: `Authorization: Bearer`, or the `access_token` query parameter on a connection endpoint, which is the only credential a browser can present on a WebSocket upgrade
- `ISignedRequestAlgorithm` and resolver — pluggable SignedRequest algorithms
- `SessionTicket` contracts — HTTP-to-long-lived-connection authentication handoff

### Configuration

- `AuthenticationProviderSettings<TInstanceSettings>` /
  `HeaderAuthenticationProviderInstanceSettings`, `AudienceAuthenticationProviderInstanceSettings` — base provider configuration bound from
  `Cirreum:Authentication:Providers:{ProviderName}`
- `ClaimAuthoritySettings` — declares whether profile and role claims are authoritative
  from the identity provider or application store

### Composition

- `IAuthenticationBuilder` — builder surface used by scheme packages to register and
  declare authentication schemes
- `AudienceSchemeRegistration` — audience-to-scheme routing metadata
- `SchemeClaimAuthorityRegistration` — per-scheme claim-authority metadata
- `IRevokedCredentialProvider` — application-provided credential revocation state
- `ConfigureCoordination(...)` / `AddDefaultCoordinationScope()` — authentication-specific
  coordination helpers

This package contains authentication-provider composition contracts only. Host-independent
identity/profile enrichment and authorization primitives live in lower-level Cirreum packages.


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
