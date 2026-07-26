# Cirreum.AuthenticationProvider v1 → v2 Migration

## Why v2

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
