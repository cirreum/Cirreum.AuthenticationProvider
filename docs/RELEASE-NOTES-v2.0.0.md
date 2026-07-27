# Cirreum.AuthenticationProvider 2.0.0 — One Removal, and a Warning Worth Reading

## What breaks

**`AuthenticationDiagnostics` is removed.** That is the whole of the breaking change, and almost
certainly nothing to do — the class had **no references anywhere**, in this framework or in the one
application integrating against it.

It is removed rather than deprecated because it was worse than dead. Its single member published a
telemetry name:

```csharp
public const string DiagnosticName = "Cirreum.AuthenticationProvider";
```

That name is not among the sources and meters Kernel's `AddCirreum()` subscribes. Telemetry names in
Cirreum are a cross-package contract, and **a source whose name is never registered is silently
inert** — it records into the void, with no listener attached and nothing failing to indicate it. The
class documentation invited exactly that use, so anyone following it would have shipped telemetry
that never reached an exporter. Same defect that left identity-provisioning telemetry unobservable
until `Cirreum.Kernel` 1.3.0 registered it.

Runtime authentication telemetry has always used `Cirreum.Authentication`, which *is* registered. If
you referenced the removed constant, use `CirreumTelemetry.ActivitySources.Authentication` or
`CirreumTelemetry.Meters.Authentication` — and check whether you need it at all, since `AddCirreum()`
already subscribes those names.

## Worth reading even though it isn't breaking

`IRevokedCredentialProvider` now documents an operational constraint that was previously only
implicit: **keep your persisted revoked set bounded.**

Everything the provider yields is held in memory for the process lifetime, and the in-memory denylist
is capacity-bounded — on saturation it **fails authentication closed** rather than silently dropping
a revocation. An unbounded revoked set therefore degrades into *refused authentication*, not into
stale state. That is the correct trade, and it is not the failure mode most people would guess.

The safe pruning rule is the denylist's own: remove a revocation once the credential could not
authenticate anyway — past its expiry plus any grace window, or once deleted or rotated out of
issuance. **Never prune a live, non-expired credential's revocation** — that re-admits it.

No behavior changed here. The rule was always true; it simply wasn't written down.

## Compatibility

Breaking only through the removal above, which is a compile error rather than a silent change.

See [`MIGRATION-v2.md`](MIGRATION-v2.md).

## Coordinated downstream work

Part of the `Cirreum.Kernel` 2.0.0 wave. `Cirreum.Runtime.AuthenticationProvider` takes a major
alongside it, removing a constant that duplicated a Kernel literal and renaming an instrument —
`auth_transformations_total` → `cirreum.authn.transformations`.

## See also

- [`MIGRATION-v2.md`](MIGRATION-v2.md)
- [`CHANGELOG.md`](CHANGELOG.md)
