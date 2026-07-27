# Cirreum.AuthenticationProvider v1 → v2 Migration

v2 carries two breaking changes: `IRevokedCredentialProvider` now yields revocation records rather
than bare identifiers, and the dead `AuthenticationDiagnostics` class is removed.

---

## 1. `IRevokedCredentialProvider` yields records

| Before | After |
|---|---|
| `IAsyncEnumerable<string> GetRevokedCredentialIdsAsync(…)` | `IAsyncEnumerable<RevokedCredential> GetRevokedCredentialsAsync(…)` |

```csharp
public readonly record struct RevokedCredential(
	string CredentialId,
	DateTimeOffset? ExpiresAt = null);
```

### Why

The contract could only say *which* credentials were revoked. So a revocation hydrated at boot was
recorded with no expiry and retained until the process restarted — while an entry created by a live
`CredentialRevoked` event self-evicted on the credential's own expiry, because
`IApiKeyDenylist.Revoke(id, expiresAt)` has always accepted one.

The plumbing existed at both ends. The contract in the middle was the only thing that couldn't carry
expiry, so the two paths behaved differently for no reason a consumer could see.

### Migration

Applications implementing the interface change the member name and wrap each identifier:

```csharp
// Before
public async IAsyncEnumerable<string> GetRevokedCredentialIdsAsync(
	[EnumeratorCancellation] CancellationToken cancellationToken = default) {

	await foreach (var row in _db.RevokedCredentials.AsAsyncEnumerable()) {
		yield return row.CredentialId;
	}
}

// After — minimum change, behavior identical
public async IAsyncEnumerable<RevokedCredential> GetRevokedCredentialsAsync(
	[EnumeratorCancellation] CancellationToken cancellationToken = default) {

	await foreach (var row in _db.RevokedCredentials.AsAsyncEnumerable()) {
		yield return new RevokedCredential(row.CredentialId);
	}
}

// Better — supply the credential's own expiry where the store knows it
		yield return new RevokedCredential(row.CredentialId, row.CredentialExpiresAt);
```

**Supplying `ExpiresAt` is optional and additive.** Omit it and you get exactly the previous
behavior: the entry is retained until restart. Supply it and the entry self-evicts once the
credential could no longer authenticate anyway, which is the point of the change — it directly
relieves the memory pressure that a large or long-lived revoked population creates.

Note this is the **credential's** expiry, not the revocation's. A revocation never expires early;
the entry is dropped only when the credential itself is already unusable.

The member is renamed rather than overloaded so this surfaces as a clean compile error, rather than
a type mismatch on a method whose name still says "Ids".

---

## 2. `AuthenticationDiagnostics` is removed

## Why it existed, and why removal rather than deprecation

One public class is removed: `AuthenticationDiagnostics`. It had no references anywhere in the
framework, and the name it published was a trap for whoever used it next.

Its single member was:

```csharp
public const string DiagnosticName = "Cirreum.AuthenticationProvider";
```

The class documentation said the `ActivitySource` and `Meter` "are created in the runtime
composition" and that the constant existed "so both agree on the identifier." Neither statement was
true. The runtime's authentication telemetry has always used `Cirreum.Authentication`, and that is
the name `CirreumTelemetry` registers.

That mismatch is the real reason for removal rather than deprecation. Telemetry names in Cirreum are
a **cross-package contract**: Kernel's `AddCirreum()` subscribes a fixed set of `ActivitySource` and
`Meter` names, and a source whose name is not among them is *silently inert* — it records into the
void, with no listener attached and nothing failing to indicate it. `"Cirreum.AuthenticationProvider"`
was not in that set. An author following this class's own documentation would have created a source
under an unregistered name and shipped telemetry that never reached an exporter. That is precisely
how identity-provisioning telemetry shipped unobservable before `Cirreum.Kernel` 1.3.0 registered it.

## Breaking Changes — Find/Replace Table

| Removed | Replace with |
|---|---|
| `AuthenticationDiagnostics` | — (no equivalent; the class had no working use) |
| `AuthenticationDiagnostics.DiagnosticName` | `CirreumTelemetry.ActivitySources.Authentication` or `CirreumTelemetry.Meters.Authentication` |

## Migration Walkthrough

Almost certainly nothing to do — a tree-wide search found no consumer, in this framework or in the
one application integrating against it.

If your code did reference it, what you meant depends on which side you were on:

### Subscribing to authentication telemetry

```csharp
// Before
builder.Services.AddOpenTelemetry()
	.WithTracing(t => t.AddSource(AuthenticationDiagnostics.DiagnosticName));

// After — usually unnecessary; AddCirreum() already registers this name
builder.Services.AddOpenTelemetry()
	.WithTracing(t => t.AddSource(CirreumTelemetry.ActivitySources.Authentication));
```

Check whether you need it at all. `AddCirreum()` registers the Conductor, remote-services,
authentication, authorization, and identity-provisioning names, so an application already calling it
collects authentication telemetry with no further configuration.

### Emitting authentication telemetry

Take the name from `CirreumTelemetry` and pass the version:

```csharp
// After
private static readonly ActivitySource _source =
	new(CirreumTelemetry.ActivitySources.Authentication, CirreumTelemetry.Version);

private static readonly Meter _meter =
	new(CirreumTelemetry.Meters.Authentication, CirreumTelemetry.Version);
```

**Never introduce a local literal for a source or meter name.** If a track genuinely needs a new
name, add the constant to `CirreumTelemetry` in `Cirreum.Kernel` — that is the one place where the
name and its registration live together, and `CirreumTelemetryTests` pins each constant to its
literal so a rename on one side without the other fails the build rather than going quiet.

## What Didn't Change

- Every authentication composition verb, registrar, and settings type
- `IApplicationUserResolver`, scheme registration, and per-scheme dispatch
- Authentication boundary resolution and the audience-scheme dispatch surface
- The authentication telemetry actually emitted at runtime, which lives in
  `Cirreum.Runtime.AuthenticationProvider` and was never named by the removed constant

## Downstream Package Impact

None expected. `Cirreum.Runtime.AuthenticationProvider` takes its own major in the same wave for a
related cleanup — see its `MIGRATION-v2.md`.
