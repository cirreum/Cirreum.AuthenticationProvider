# Cirreum.AuthenticationProvider 3.0.0 — a provider declares who it authenticates

## Why this release exists

The framework used to work out whether a caller was a person or a machine by looking at their
token. An authenticated caller with no resolvable name claim was treated as a machine and named
after the calling application.

That reads a *thin* token as a *machine* token, and the two are not the same thing. An
application that owns its users' attributes issues tokens carrying little else — the name lives
in the application's own store, or arrives under a provisioned claim the server never aliased.
Those callers are people, and they were being classified as software.

Nothing about a token can settle the question reliably. A provider can: an API-key scheme
authenticates services, an OIDC scheme authenticates people, and the scheme package knows which
it is at the moment it is written. This release moves the answer from inference to declaration.

## What's new

**`SubjectKind` on the registrar base.** One abstract member, answered once per provider:

```csharp
public sealed class PartnerApiKeyRegistrar
    : HeaderAuthenticationProviderRegistrar<PartnerSettings, PartnerInstanceSettings> {

    public override string ProviderName => "PartnerApi";
    public override SubjectKind SubjectKind => SubjectKind.Machine;
}
```

It sits beside `ProviderName` because it is the same species of fact: a constant of the provider,
not of any instance.

**Answer it from what the provider authenticates, never from how it carries credentials.**
Header-based does not imply `Machine` — session tickets travel in headers and carry people. This
is the one place the declaration can still be got wrong, so it is stated on the member itself.

**`ClaimAuthority`, an optional per-instance block.** Declares who owns a scheme's callers — the
identity provider, or the application's own store — separately for profile and roles:

```jsonc
"Oidc": { "Instances": { "descope": {
    "Authority": "https://…", "Audience": "…",
    "ClaimAuthority": { "Profile": "ApplicationStore", "Roles": "ApplicationStore" }
} } }
```

The two axes are answered separately because applications genuinely differ. An identity provider
federating an external account may supply the whole profile while the application still owns the
roles — that combination is the common case, not an exotic one.

Each is a **precedence** rule, not exclusivity: the declared side wins where both supply a value,
and neither side is prevented from supplying one. That distinction is what lets a single scheme
serve a federated account arriving with full profile claims *and* a local account on the same
scheme arriving thin. Availability varies per user; the declaration is per scheme; precedence is
what reconciles them.

Omitting the block declares nothing and preserves 2.x behaviour exactly.

**Per-scheme publication at composition.** `RegisterInstance` contributes a
`SchemeClaimAuthorityRegistration` for each registered scheme — after validation, so an instance
that fails to validate declares nothing. The runtime aggregates these into the map its claims
transformation and user-state accessor read.

The contribution is anchored on the *registered scheme* rather than the configured instance. Those
are usually the same, but not always: a provider whose credentials resolve dynamically can
register a scheme with no configured instances at all, and contributes from its own composition
verb instead. Anchoring on the instance would have left exactly those schemes undeclared.

**`AddDefaultCoordinationScope()`.** The canonical `{app}:{env}` coordination scope, extracted
from `ConfigureCoordination` so it has one definition. `AddEventCoordination` in
`Cirreum.Runtime.Authentication` carried a verbatim copy — identical registration, identical
exception text, across a layer boundary where neither could see the other drift.

It was deliberately *not* pushed down into `Cirreum.Coordination`. The scope is opaque there by
design: the primitive holds no opinion about what an application or an environment is. Supplying
the default belongs to a composition surface that knows `IDomainEnvironment`, so one such surface
now owns it.

## Compatibility

**Breaking, by the letter of SemVer rather than the weight of the change.**

- An application composing only framework-shipped schemes — ApiKey, SignedRequest, SessionTicket,
  Oidc, Entra, External — has **nothing to change**. Those registrars answer `SubjectKind` in the
  same wave, and the answers arrive with the packages.
- An application with its own custom registrar adds **one line**. The compiler finds every case;
  an abstract member cannot be missed, which is why it is abstract rather than defaulted. A
  provider that never answers is a provider whose callers cannot be classified, and that belongs
  at compile time rather than surfacing as an unexplained `Unknown` in production.
- No renames, no removals, no signature changes. `ProviderName`, `ValidateSettings`, `Register`,
  `RegisterInstance`, and `RegisterScheme` are untouched, as is scheme-name derivation.
- **No behaviour changes on its own.** A registrar that answers `SubjectKind` and declares no
  `ClaimAuthority` behaves exactly as it did in 2.x. The declarations are read by higher-layer
  packages releasing alongside this one.

See [`MIGRATION-v3.md`](MIGRATION-v3.md) for the walkthrough.

## See also

- `Cirreum.Kernel 2.1.0` — the vocabulary this package declares against: `SubjectKind`,
  `ClaimAuthority`, `SchemeClaimAuthority`, and `ISchemeClaimAuthorityMap`, plus
  `IUserState.SubjectKind` where operation authorizers read the resolved answer.
- `Cirreum.Runtime.AuthenticationProvider` — aggregates the per-scheme registrations into the map,
  and reads the declaration in place of the roles-claim inference it used before.
- `Cirreum.Runtime.Authentication` — takes `AddDefaultCoordinationScope()` in place of its own
  copy.
