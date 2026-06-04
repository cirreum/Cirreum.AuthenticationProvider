# Cirreum.AuthenticationProvider 1.0.0 — The Authentication pillar's contract surface

`Cirreum.AuthenticationProvider` is the abstraction layer for Cirreum's Authentication pillar (one of the three-pillar separation: Identity / Authentication / Authorization). It defines the contracts that scheme packages implement, that runtime composition wires, and that the app-facing umbrella exposes — so any scheme plugs into one open/closed dispatch model without depending on the others.

**Strictly additive — initial release.** No predecessor `Cirreum.AuthenticationProvider` package. Targets .NET 10.0.

---

## Why this release exists

The **Cirreum 1.0 Foundation Reset** separates the framework's security concerns into three pillars (Identity / Authentication / Authorization). This package is the Authentication pillar's *abstractions* — the single surface that `Cirreum.Authentication.{Scheme}` packages implement, `Cirreum.Runtime.AuthenticationProvider` composes, and `Cirreum.Runtime.Authentication` surfaces.

Isolating the contracts here is what makes scheme dispatch open/closed: a new scheme (ApiKey, SignedRequest, SessionTicket, an OIDC variant, a future mTLS) ships by registering a selector against these contracts — no edits to a central resolver, and no scheme depending on another. It builds on `Cirreum.Kernel`'s authentication-event bus and holds only contracts; the behavior lives in the scheme packages and the runtime.

---

## What's new

### Per-request scheme dispatch

```csharp
public interface ISchemeSelector {
    int Priority { get; }
    (bool Matches, string? SchemeName) TrySelect(HttpContext context);
}
```

The open/closed model: each scheme registers a cheap `ISchemeSelector`; the runtime iterates them by `Priority` and dispatches to the first claimant. `CredentialTransport` names *where* a scheme reads its credential, `SchemeCategory` classifies it, and `AuthenticationSchemes` centralizes the well-known scheme-name constants.

### SignedRequest crypto contracts

`ISignedRequestAlgorithm` + `ISignedRequestAlgorithmResolver` — the version-pluggable signing/verification seam the SignedRequest scheme dispatches through, keyed by algorithm identity so new algorithms drop in additively.

### SessionTicket handoff primitives

```csharp
public interface ISessionStore {
    ValueTask StoreAsync(SessionTicket ticket, CancellationToken ct);
    ValueTask<SessionTicket?> ConsumeAsync(string ticketValue, CancellationToken ct);   // atomic single-use
    ValueTask<SessionTicket?> RetrieveAsync(string ticketValue, CancellationToken ct);
    ValueTask RemoveAsync(string ticketValue, CancellationToken ct);
    ValueTask RemoveBySubjectAsync(string subject, CancellationToken ct);
}
```

The `SessionTicket` record + `SessionTicketIssueRequest`, the `ISessionTicketIssuer` / `ISessionTicketValidator` / `ISessionTicketPrincipalBinder` triad, and `ISessionStore` — the credentials that bridge an authenticated HTTP context to a long-lived connection. `ISessionStore.ConsumeAsync` is the **atomic single-use** consume (retrieve-and-remove in one operation), so two concurrent handshakes presenting the same ticket cannot both redeem it.

### Composition & extensibility seams

```csharp
public interface IRevokedCredentialProvider {
    IAsyncEnumerable<string> GetRevokedCredentialIdsAsync(CancellationToken ct = default);
}
```

`IAuthenticationBuilder` — the registration surface scheme verbs extend (it carries `Services`, the ASP.NET `AuthenticationBuilder`, and the host `IConfiguration`, so a verb like `AddApiKey(...)` can also bind its provider's configuration section). `[AllowPendingAuth]` — the Two-Phase Auth anonymous-pending-auth opt-in. `IAuthenticationBoundaryResolver` — Global vs Tenant vs None resolution. The claim-enrichment seams (`IUserProfileEnrichmentBuilder`, `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder`, and the default `ClaimsUserProfileEnricher`). And `IRevokedCredentialProvider`, through which an app exposes its revocation state for the framework to honor — without coupling framework infrastructure to an app-specific admin schema.

---

## Why this lives in Cirreum.AuthenticationProvider

These are contracts, not implementations — so scheme packages and runtime composition depend on the *same* surface without depending on each other, which is exactly what keeps scheme dispatch open/closed. The package builds on `Cirreum.Kernel` (the authentication-event bus); the concretes live in the `Cirreum.Authentication.{Scheme}` packages and the runtime, never here.

---

## Coordinated downstream work

`Cirreum.Authentication.{Scheme}` packages (ApiKey, SignedRequest, SessionTicket, …) implement these contracts; `Cirreum.Runtime.AuthenticationProvider` composes them (the dynamic forward scheme resolver, the authentication-event hosted handlers and cache invalidators, the boot-time analyzers); `Cirreum.Runtime.Authentication` is the app-facing `AddAuthentication(...)` umbrella. This package publishes after `Cirreum.Kernel`.

---

## Compatibility

- **Additive.** Initial release.
- **.NET 10.0.**
- References `Cirreum.Kernel` (and the foundation peers as needed).
- **Migration from `Cirreum.Core 5.x`:** install `Cirreum.AuthenticationProvider`. Namespaces `Cirreum.*` and `Cirreum.Security.*` are preserved for the relocated types.

---

## See also

- `CHANGELOG.md` — condensed change list for `1.0.0`.
