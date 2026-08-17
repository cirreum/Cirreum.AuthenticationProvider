# Cirreum.AuthenticationProvider 3.0.1 — registering a scheme and declaring it become one act

## Why this release exists

3.0.0 shipped the declaration vocabulary: a provider states what it authenticates, an instance
states who owns its callers' claims, and `RegisterInstance` contributed one registration per
scheme for the runtime to aggregate. A survey of every scheme-registration site in the framework
then found the delivery didn't hold. Registration happens through three disjoint paths, and each
had a different relationship to the declaration: the registrar base filed records keyed on the
*instance key* — which is not the scheme name for multi-transport providers, so `ApiKey:Bearer`
would have resolved `Undeclared` no matter what was configured; ApiKey's actual scheme
birthplaces (including the zero-instance dynamic-resolver path) filed nothing at all; and two
providers filed correct records by hand, which held only as long as someone remembered.

Three paths drifting apart *while the pattern was being established* is the argument for a
structural fix. This release makes registering a scheme and declaring it a single act, at the
one surface every path already passes through.

## What's new

**`DeclareScheme` and a declared `AddScheme` on `IAuthenticationBuilder`.** The Cirreum
composition surface can no longer register a scheme undeclared:

```csharp
// Registers the handler scheme AND contributes its declaration:
builder.AddScheme<SessionTicketAuthenticationOptions, SessionTicketAuthenticationHandler>(
    "SessionTicket:Bearer", SubjectKind.Unknown);

// Declare-only, beside a registration an external verb performs
// (AddJwtBearer, AddOpenIdConnect, AddCookie):
builder.DeclareScheme(settings.Scheme, SubjectKind.Human,
    settings.ClaimAuthority.Profile, settings.ClaimAuthority.Roles);
```

Registering through `AuthBuilder` directly remains possible — and visible as the deliberate
exemption it is.

**The registrar contract consolidates onto the builder.** `Register`, `RegisterInstance`,
`RegisterScheme`, `AddAuthenticationHandler`, and the audience hooks now take
`IAuthenticationBuilder` in place of the `(IServiceCollection, IConfiguration,
AuthenticationBuilder)` triple — the builder carries all three. The changelog holds the full
find/replace table.

**The audience base declares in `RegisterScheme`.** Every audience instance contributes its
declaration — the registrar's `SubjectKind` plus the instance's `ClaimAuthority` block — beside
its `AudienceSchemeRegistration`, at the same moment the scheme registration is dispatched. The
mis-keyed `RegisterInstance` contribution is gone.

**`SessionTicket` carries its subject's origin.** A session ticket is a continuation — it
re-presents a subject another scheme established — so `SessionTicket` and
`SessionTicketIssueRequest` gain a `Scheme` field: the scheme that authenticated the caller the
ticket was issued to, stamped at validation as the origin scheme
(`AuthenticationContextKeys.OriginScheme`, Kernel 2.1.1) so the subject's declaration re-resolves
from the scheme that actually authenticated them. Optional; a ticket without an origin resolves
`SubjectKind.Unknown`, the fail-safe.

**Removals.** `AuthenticationSchemes` is trimmed to the schemes the umbrella itself registers
(`Dynamic`, `Ambiguous`, `Anonymous`); the `ApiKey`, `SessionTicket`, `SignedRequest`, and
`AnonymousPendingAuth` entries named schemes that don't exist, duplicated provider-owned
constants, or both — provider scheme names are owned by the provider packages (`ApiKeySchemes`,
`SessionTicketSchemes`, `SignedRequestSchemes`). `AllowPendingAuthAttribute` is removed: it
shipped as a marker and was consumed by nothing — the anonymous-pending-auth flow is carried by
endpoint-level anonymous access, per-invocation default-deny authorization, and
`connection.Promote(...)`, none of which read it.

## Why this ships as a patch

The 3.0.0 release notes said "no signature changes." This release changes those signatures, days
later — and that reversal is the point of shipping it *now*: zero released packages consume
3.0.0 (every provider release is held for this wave), so the contract can still be corrected
before it has a single adopter. A post-release, pre-adoption correction, released with the
escape hatch that exists for exactly that case. A patch has no migration-doc slot, so the
changelog carries the migration table.

## Compatibility

- **Breaking on paper for registrar and builder implementers; zero released consumers.** The
  framework's own providers and its one `IAuthenticationBuilder` implementation update in the
  same wave.
- **An application composing framework-shipped schemes has nothing to change.**
- **An application with a custom registrar** applies the parameter find/replace from the
  changelog — the builder it now receives carries everything the old parameters did.
- **An application pinning a provider scheme** by an `AuthenticationSchemes` constant was
  pinning a name that no scheme registers; the provider packages' own constants are the working
  replacements.

## See also

- `Cirreum.Kernel 2.1.1` — `AuthenticationContextKeys.OriginScheme`, the slot a ticket's origin
  is stamped into.
- `Cirreum.Runtime.Authentication` — implements the builder members and aggregates the
  declarations into the scheme claim-authority map at composition close.
- The provider packages — re-pin and route their registrations through the funnel in the same
  wave.
