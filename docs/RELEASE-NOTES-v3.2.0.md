# Cirreum.AuthenticationProvider 3.2.0 — `BearerCredential` is withdrawn

## Why this release exists

3.1.0 added `BearerCredential`, a reader every scheme selector and handler was to adopt so
that a query-carried bearer credential would be honoured on connection endpoints. The case it
served is real: a browser cannot set request headers on a WebSocket upgrade, so a client
connecting over WebSockets sends its token as an `access_token` query parameter, and a server
that reads only `Authorization` refuses the connection — after which SignalR falls back to
Server-Sent Events or long polling and the downgrade goes unreported.

Adopting it scheme by scheme turned out to be the wrong shape, and that became clear before
any scheme adopted it.

Every layer that reads a credential would have had to be found and changed — not only the
handlers, but the selectors that decide which scheme handles a request at all, including the
JWT audience-routing selector that lives in `Cirreum.Runtime.Authentication`, outside the
scheme packages entirely. Missing one leaves the original defect in place while appearing
fixed, and any scheme added later inherits the same obligation.

`Cirreum.Services.Server` 1.6.0 solves it in one place instead. Middleware promotes a
query-carried credential into the `Authorization` header before authentication runs, scoped to
endpoints where a client has no alternative. Every scheme and every selector then reads the
credential exactly where it always has, unchanged, and a scheme written next year inherits the
behaviour without knowing the case exists.

## What changed

`BearerCredential` is removed, with both its members:

| Member | Replacement |
| --- | --- |
| `BearerCredential.Read(HttpContext)` | Read `Authorization` as before — the middleware ensures a promoted credential is there |
| `BearerCredential.IsConnectionEndpoint(HttpContext)` | Test the endpoint for `InvocationConnectionMetadata` (`Cirreum.Contracts`) or SignalR's `HubMetadata` |

No framework code called either member, and the type was never referenced by a released
package other than this one. It is removed rather than kept as a convenience because public
surface that nothing consumes acquires consumers by accident, and is then owed compatibility
it was never designed for.

## Compatibility

* **Shipped as a minor deliberately.** Removing public API is breaking on paper; with no
  consumers, this removal cannot break a compile anywhere. The same judgement was applied to
  `AuthorizationDenial` in `Cirreum.Contracts` 4.2.0.
* Nothing else in the package changed.
* Applications wanting the behaviour need only `Cirreum.Services.Server` 1.6.0 and its
  `UseConnectionCredential()` middleware, which `Cirreum.Runtime.Server` registers by default.

## See also

* `Cirreum.Services.Server` 1.6.0 — the middleware that replaced this reader.
* `InvocationConnectionMetadata` (`Cirreum.Contracts` 4.7.0) — the endpoint marker it consults.
