# Cirreum.AuthenticationProvider 1.1.0

## Summary

Adds a thin `auth.AddCoordination(...)` forwarder so auth schemes can register a
coordination backend (replay-nonce / throttle) inside the `AddAuthentication`
composition, and takes a dependency on the new standalone `Cirreum.Coordination`
primitive. Re-pins `Cirreum.Contracts` to the code-first caching foundation (1.1.1).

Strictly additive.

## Why

Some auth schemes need atomic coordination: SignedRequest's strict-nonce posture needs
a single-use claim (`IReplayGuard`) to reject replays; rate-limited schemes need a
fixed-window counter (`IRequestThrottle`). Those primitives are not auth-specific — they
were extracted into the neutral, dependency-light `Cirreum.Coordination` package so any
consumer can share them and a single chosen backend. This release wires that primitive
into the authentication builder so schemes get a natural `auth.AddCoordination(...)` verb
without `AuthenticationProvider` owning the primitives itself.

## What's new

### `auth.AddCoordination(...)`

```csharp
builder.AddAuthentication(auth => {
    auth.AddSignedRequest<MyResolver>(o => o.ConfigureValidation(v => v.RequireStrictNonce = true));
    auth.AddCoordination(c => c.UseInMemory());   // or c.UseRedis() with Cirreum.Coordination.Redis
});
```

A thin convenience on `IAuthenticationBuilder` that forwards to
`services.AddCoordination(...)` from `Cirreum.Coordination`. Registration is idempotent
and order-independent: a scheme *pulls* the requirement it needs, the app *chooses* the
backend. The primitives (`IReplayGuard`, `IRequestThrottle`, the in-memory backend, and
the posture validator) live in `Cirreum.Coordination` — see that package's release notes.

## Compatibility

- **Additive.** No changes to the existing `AuthenticationProvider` surface.
- **New dependency:** `Cirreum.Coordination 1.0.0`.
- **Re-pinned:** `Cirreum.Contracts → 1.1.1` (code-first caching foundation; no source
  impact — none of the renamed/removed cache types are referenced here).

## See also

- `Cirreum.Coordination` — the neutral atomic-coordination primitives the forwarder delegates to
- `Cirreum.Coordination.Redis` — a distributed backend for the same primitives
