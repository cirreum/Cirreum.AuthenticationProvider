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

### Enrich `IRevokedCredentialProvider` for denylist hygiene

- **SemVer:** Major
- **Trigger:** Boot-hydrated revocation memory becomes a real concern (very large or long-lived
  revoked-credential populations), or the hydrated denylist needs to evict expired entries without a
  restart — or a coordinated foundation major bundles the contract change.
- **Noted:** 2026-07-05
- Today `GetRevokedCredentialIdsAsync` yields `IAsyncEnumerable<string>` — credential ids only, no
  expiry. So the scheme hydrators (`ApiKeyRevocationHydrator`, and the SignedRequest equivalent) record
  boot-hydrated revocations with a `null` expiry ("retain until restart"). That is **safe**
  (over-retention, never under-revocation) and self-limiting (the set re-hydrates each boot), but a
  hydrated entry cannot self-evict on the credential's own expiry the way an event-driven
  `CredentialRevoked.ExpiresAt` entry now does (that expiry threading shipped in `Cirreum.Authentication.ApiKey`).
  Enriching the contract to yield a revocation record (id + optional expiry, possibly credential-type)
  would let the hydrators thread expiry into `IApiKeyDenylist.Revoke(id, expiresAt)` for symmetry with
  the live event path. **Breaking** — changes the public return type of an app-implemented interface —
  hence Major, gated to a foundation major or a real forcing function.
- **App-side guidance (independent, doc-only / Patch-able now):** document on `IRevokedCredentialProvider`
  that apps must keep their persisted revoked set bounded so the in-memory denylist (capacity-bounded;
  fails auth closed on saturation) does not fill up. Safe pruning rule = the same as the denylist's own
  eviction: remove a revocation once the credential can no longer authenticate anyway — past its own
  expiry (plus any validation grace window), or once deleted/rotated out of issuance entirely. **Never**
  prune a live, non-expired credential's revocation (re-admits it); non-expiring credentials stay until
  removed from issuance. The framework needs no "un-revoke" signal — the set re-hydrates on restart and
  live in-memory entries self-evict on the credential's expiry.
