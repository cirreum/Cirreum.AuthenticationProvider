# Cirreum.AuthenticationProvider 3.1.0 — one reader for the inbound bearer credential

## Why this release exists

A browser cannot set request headers on a WebSocket upgrade. A client connecting to a hub over
WebSockets therefore sends its bearer token as an `access_token` query parameter — the
convention SignalR's own clients follow, and the only one available to them.

No Cirreum scheme read it. The upgrade arrived with no credential the server recognized and was
refused, and SignalR responded by falling back to Server-Sent Events or long polling. The
application kept working, never used WebSockets, and nothing reported the downgrade. The
failure is invisible precisely because the fallback succeeds.

Reading the credential from the query is straightforward. Reading it *only where it belongs* is
the part that needs a home: on an ordinary API route a query-carried token has no justification
and every drawback, so acceptance must be scoped to the endpoints that leave a client no
alternative.

Two layers need that answer, and both had their own partial view of it. An `IBearerSchemeSelector`
decides which scheme handles a request, before any handler runs; the handler then validates the
credential. Teaching handlers alone would not have worked — the selector declines first, and in
an application with several Bearer schemes, which is the usual shape, dispatch falls through to
the wrong scheme or to none.

## What's new

### `BearerCredential`

```csharp
var token = BearerCredential.Read(context);
```

One reader, used by selectors and handlers alike:

| Request | Result |
| --- | --- |
| `Authorization: Bearer <token>` | the token |
| No header, connection endpoint, `?access_token=<token>` | the token |
| No header, ordinary endpoint, `?access_token=…` | `null` — a query parameter outside a connection endpoint carries no authority |
| `Authorization: <other scheme> …` | `null` — an explicit header of another scheme is the caller's choice, not a reason to consult the query |
| Both header and query present | the header — it always wins |

The value is returned exactly as it arrived. A scheme prefix such as `st_prod_…` is part of the
opaque secret its issuer minted and stored, not a wrapper to be peeled off, so prefix-based
dispatch continues to work unchanged.

Connection endpoints are recognized two ways: a SignalR hub by SignalR's own hub metadata, and
everything else by `InvocationConnectionMetadata` from `Cirreum.Contracts` 4.7.0, which the
server spine stamps on the connection endpoints it maps. Applications need configure nothing.

## Also in this release

The package's test suite compiles again. Its registrar doubles and call sites still used the
signatures that preceded the `IAuthenticationBuilder` consolidation, so the project had not
built since. Release runs skip test solutions by default, which is why no release caught it.

## Compatibility

* **Purely additive** — one new type, no changes to existing members.
* Existing selectors and handlers are unaffected until they adopt the reader; each is updated
  in its own release.
* Requires `Cirreum.Contracts` 4.7.0.

## See also

* `IBearerSchemeSelector` — the selector contract whose implementations read the credential to
  decide dispatch.
* `InvocationConnectionMetadata` (`Cirreum.Contracts`) — the endpoint marker this reader consults.
