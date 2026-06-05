# Migration to Cirreum.AuthenticationProvider v1.0.0

## Context

This is the **initial release** of `Cirreum.AuthenticationProvider`. There is no
prior version of this package to migrate from. It is the abstractions layer for the
Authentication pillar of the Cirreum framework, established as part of the
**Cirreum 1.0 Foundation Reset** wave.

## Migrating from `Cirreum.Core 5.x`

Apps that consumed the Authentication-pillar abstractions from `Cirreum.Core 5.x`
migrate by installing `Cirreum.AuthenticationProvider`:

```xml
<PackageReference Include="Cirreum.AuthenticationProvider" Version="1.0.0" />
```

The `Cirreum.*` and `Cirreum.Security.*` namespaces are **preserved** for the
relocated types, so existing `using` directives and type references continue to
resolve once the package reference is in place. No source changes are required
beyond adding the package.

### What relocated from `Cirreum.Core 5.x`

- `IAuthenticationBuilder`, `IUserProfileEnrichmentBuilder`,
  `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder`
- `IAuthenticationBoundaryResolver` (Global / Tenant / None boundary resolution)
- `ClaimsUserProfileEnricher` (default `IUserProfileEnricher`)

### New contracts in 1.0.0

- `ISchemeSelector` + `SchemeCategory`, `CredentialTransport`, `[AllowPendingAuth]`,
  `AuthenticationSchemes`, `IRevokedCredentialProvider`
- `ISignedRequestAlgorithm` + `ISignedRequestAlgorithmResolver` (RFC 9421)
- `SessionTicket` primitives (`SessionTicketIssueRequest`, `ISessionTicketIssuer`,
  `ISessionTicketValidator`, `ISessionTicketPrincipalBinder`) and `ISessionStore`
  (with the atomic single-use `ConsumeAsync`)

## Dependencies

`Cirreum.AuthenticationProvider` consumes the Cirreum foundation entirely as
published NuGet packages:

- `Cirreum.Contracts` `1.1.0` — brings `Cirreum.Kernel` `1.0.1` and
  `Cirreum.Result` `2.0.0` transitively (the auth event bus and
  `AuthenticationContextKeys` live in `Cirreum.Kernel`).
- `Cirreum.Providers` `1.2.1` — registrar base contracts +
  `ProviderType.Authentication`.
- `FrameworkReference Microsoft.AspNetCore.App` — `HttpContext` / scheme dispatch.
