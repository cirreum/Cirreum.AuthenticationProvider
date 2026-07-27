# Backlog

Deferred work for **Cirreum.AuthenticationProvider**. Items here are tracked but not yet ready
to ship — either because the cost outweighs the benefit in isolation, or
because they're waiting on a forcing function (a related change, a consumer
upgrade, a coordinated multi-repo rollout).

## How this file works

- Each item is a `###` heading so it can be linked to and parsed.
- Each item declares **`SemVer:`** (`Patch` | `Minor` | `Major` | `Unspecified`),
  **`Trigger:`** (the human-readable condition that will make it ready), and
  **`Noted:`** (the date the item was added).
- The Cirreum DevOps release scripts (`PatchRelease`, `MinorRelease`,
  `MajorRelease`) surface items at-or-below the requested bump level so the
  operator can decide whether to fold them in before tagging.
- Items that ship: move from this file to `docs/CHANGELOG.md` under
  `[Unreleased]`. Items that grow into design discussions: promote to an ADR.

## Queued

### The host-shape branch in `AudienceAuthenticationProviderRegistrar` is untestable in-process

**SemVer:** Unspecified
**Trigger:** A change to how `RegisterScheme` chooses between the Web App and Web API paths, or a
second reason to want `ProviderContext` resettable.
**Noted:** 2026-07-27

`RegisterScheme` dispatches to `AddAuthenticationForWebApp` or `AddAuthenticationForWebApi` on
`ProviderContext.GetRuntimeType()`. That is a **write-once static**: `SetRuntimeType` throws if it has
already been configured and there is no reset, so a test assembly can select exactly one host shape
for its entire lifetime — and with xUnit's parallel classes, whichever test sets it first silently
decides the shape for every other test in the assembly.

The consequence is that no audience-based provider can cover both branches through the base
registrar. `Cirreum.Authentication.Entra` covers what it owns by calling the two public methods
directly (`EntraAuthenticationRegistrarTests`, 2026-07-27), which is the right split — the branch is
this package's code, not the scheme package's — but it means the branch itself has no coverage
anywhere.

Not worth adding a reset purely for tests: that would be test-only surface on a shipped public
static, and the write-once guard is a real invariant worth keeping (the runtime type genuinely must
not change mid-process). If this becomes worth solving, the shape to consider is an internal seam the
base registrar reads instead, with `ProviderContext` as its default source — leaving the public
guarantee intact while making the branch injectable.
