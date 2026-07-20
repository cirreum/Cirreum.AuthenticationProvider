# Cirreum.AuthenticationProvider 1.3.0 — Container-owned audience routing data

Audience-based schemes now contribute their `audience → scheme` routing entries to the
service collection as immutable records, replacing a shared mutable map whose delivery
mechanism was defective under real compositions. This release is the contracts half of
a coordinated fix with `Cirreum.Runtime.Authentication` 1.2.0; together they repair
multi-provider audience dispatch, where only the last-registered audience instance was
actually routable.

Minor, with one deliberate deviation: two defective types are removed outright (see
Compatibility).

---

## Why this release exists

Audience-based schemes (Entra, generic OIDC) share one credential carrier —
`Authorization: Bearer` — so per-request dispatch is data-driven: a shared selector
matches the token's `aud` claim against routing data contributed by every audience
instance. That data was delivered by mutating one shared `IAudienceSchemeMap` instance
that registrars located by scanning the service collection.

Instance identity turned out to be load-bearing, and it broke: under the umbrella's
composition the reuse check never succeeded, so **every audience instance registration
silently created a fresh map**, and last-wins DI resolution kept only the final one.
In any app with more than one audience instance, every earlier audience vanished from
dispatch — those bearer tokens were rejected fail-closed as `Cirreum.Ambiguous` with
no indication why. Which instance survived depended on config-key ordering. The same
instance-identity coupling also meant an application registering its own map — by
implementation type, the idiomatic shape — could never work.

## What's new

### `AudienceSchemeRegistration`

```csharp
public sealed record AudienceSchemeRegistration(
    string Audience,
    string Scheme,
    string ProviderName);
```

The routing contribution. `AudienceAuthenticationProviderRegistrar` registers one per
enabled instance directly into the service collection — the same additive-set mechanic
as `ISchemeSelector`. There is no shared mutable state and no descriptor scanning on
the runtime path: the selector in `Cirreum.Runtime.Authentication` resolves
`IEnumerable<AudienceSchemeRegistration>` and builds its index once, at construction.

Applications integrating an IdP outside a Cirreum provider family can contribute a
mapping directly:

```csharp
services.AddSingleton(new AudienceSchemeRegistration(
    "api://my-legacy-api", "legacyBearer", "MyApp"));
```

### Collection-scoped duplicate-instance guard

The registrar base's duplicate-instance-key guard was process-global static state: a
second host composed in the same process — the integration-test norm — was rejected
for re-using an instance key its own composition had never seen. Guard state now lives
in the service collection, so hosts in one process are fully isolated.

## Compatibility

- **`IAudienceSchemeMap` and `DefaultAudienceSchemeMap` are removed.** Strictly this
  is a breaking change shipped in a minor — a deliberate, recorded deviation. The
  types were dormant-defective: the framework's own delivery path could not populate
  them correctly, and no correct program could have consumed or substituted them.
  Custom audience-routing semantics belong on the `ISchemeSelector` seam, which has
  always been the dispatch extension point.
- **Coordinated upgrade required:** `Cirreum.Runtime.Authentication` 1.2.0 consumes
  the registration set; earlier umbrella versions reference the removed map and must
  be upgraded together.
- No new dependencies.

## See also

- `Cirreum.Runtime.Authentication` 1.2.0 — the consuming selector, composition-close
  conflict validation, and the no-match diagnostics that make audience rejections
  self-explanatory
