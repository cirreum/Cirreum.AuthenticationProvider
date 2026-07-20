# Cirreum.AuthenticationProvider 1.4.0 — Boundary resolution moves to the Kernel

`IAuthenticationBoundaryResolver` and `DefaultAuthenticationBoundaryResolver` are
removed from this package; they now live in `Cirreum.Kernel` 1.2.0, namespace
`Cirreum.Security`, beside the `AuthenticationBoundary` enum, `IUserState`, and
`UserStateBase` they operate on. The never-called `AddAuthenticationBoundaryResolver`
extension is deleted.

## Why this release exists

The boundary seam is spine infrastructure: the server user-state pipeline resolves it
per invocation and grant providers consume the stamped classification, whether or not
any authentication scheme is composed. Hosting it here forced the services spine to
reference this package for exactly one interface, and left the seam's registration
orphaned — nothing called the "spine registration" extension, so every caller
classified as `None`. Registration is now owned by the packages that need it:
`Cirreum.Services.Server` registers the default alongside its user-state accessor,
and `Cirreum.Runtime.Authentication` registers the scheme-aware primary-scheme
resolver it restores in 1.2.0.

## Migrating

Implementers of a custom resolver change one line:

```csharp
// before
using Cirreum.AuthenticationProvider.Security;
// after
using Cirreum.Security;   // Cirreum.Kernel
```

## Compatibility

- Type removal in a minor — the same deliberate, recorded SemVer deviation as 1.3.0,
  while the authentication rewrite completes: the removed types were unreachable in
  practice (no registration path existed), so no correct program consumed them from
  here.
- **Coordinated upgrade:** `Cirreum.Kernel` 1.2.0 provides the relocated types;
  `Cirreum.Services.Server` and `Cirreum.Runtime.Authentication` pick up the new
  registrations in their next releases.

## See also

- `Cirreum.Kernel` 1.2.0 — the seam's new home
- `Cirreum.Runtime.Authentication` 1.2.0 — restored scheme-aware classification
