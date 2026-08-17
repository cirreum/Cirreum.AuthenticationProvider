# Migrating to Cirreum.AuthenticationProvider 3.0.0 (from 2.x)

## Why v3

Every authentication provider now declares whether it authenticates **people or machines**.

The framework used to work this out by looking at the token: an authenticated caller with no
resolvable name claim was treated as a machine, and named after the calling application. That
reads a thin token as a machine caller — which is wrong for every application that owns its
users' attributes and therefore issues tokens carrying little else. The fix is to stop inferring
and start declaring, and a provider is the only thing that reliably knows: an API-key scheme
authenticates services, an OIDC scheme authenticates people.

One abstract member carries that, which is the whole of this major.

## Breaking Changes — Find/Replace Table

| 2.x | 3.0.0 |
|---|---|
| `class MyRegistrar : AuthenticationProviderRegistrar<TSettings, TInstance>` | add `public override SubjectKind SubjectKind => SubjectKind.Human;` |
| `class MyRegistrar : HeaderAuthenticationProviderRegistrar<…>` | add `public override SubjectKind SubjectKind => …;` |
| `class MyRegistrar : AudienceAuthenticationProviderRegistrar<…>` | add `public override SubjectKind SubjectKind => …;` |

Nothing is renamed and nothing is removed. If your application composes only framework-shipped
schemes — ApiKey, SignedRequest, SessionTicket, Oidc, Entra, External — **there is nothing to
change**: those registrars answer it in the same wave, and the answer arrives with the package.

Only an application that wrote its own registrar is affected, and only by one line.

## Migration Walkthrough

1. **Build.** The compiler finds every affected type for you — an abstract member cannot be
   missed, which is why it was made abstract rather than defaulted. Each custom registrar reports
   that it "does not implement inherited abstract member `SubjectKind`".

2. **Answer from what the provider authenticates, not how it transports credentials.**

   ```csharp
   public sealed class PartnerApiKeyRegistrar
       : HeaderAuthenticationProviderRegistrar<PartnerSettings, PartnerInstanceSettings> {

       public override string ProviderName => "PartnerApi";

       // A partner service calling us on its own behalf — no person involved.
       public override SubjectKind SubjectKind => SubjectKind.Machine;
   }
   ```

   Header transport does **not** imply `Machine`. Session tickets travel in headers and carry
   people. Ask who is on the other end, not how the credential arrives.

3. **Declare attribute authority, if the default is wrong for you.** Optional, and unrelated to
   the break — omitting it preserves 2.x behaviour exactly. Per scheme instance:

   ```jsonc
   "MyProvider": { "Instances": { "customers": {
       "ClaimAuthority": { "Profile": "ApplicationStore", "Roles": "ApplicationStore" }
   } } }
   ```

   `ApplicationStore` for `Roles` means roles resolve live per request, so a revocation takes
   effect immediately rather than at the caller's next sign-in. `ApplicationStore` for `Profile`
   means claims your application minted into the token win over the identity provider's native
   ones. Each is a precedence rule — the declared side wins where both supply a value, and
   neither side is prevented from supplying one.

## What Didn't Change

- `ProviderName`, `ValidateSettings`, `Register`, `RegisterInstance`, and `RegisterScheme` keep
  their signatures and their behaviour.
- Instance configuration binding is unchanged; `ClaimAuthority` is additive and optional.
- Scheme naming is unchanged — the instance key is still the scheme name.
- No behaviour changes on its own. A registrar that answers `SubjectKind` and declares no
  `ClaimAuthority` behaves in 3.0.0 exactly as it did in 2.x; the declarations are read by
  higher-layer packages that ship alongside this one.

## Downstream Package Impact

The framework-shipped provider packages (`Cirreum.Authentication.ApiKey`, `.Oidc`, `.Entra`,
`.External`) each answer `SubjectKind` and release against this version. Applications consuming
those packages inherit the answers and need no change.
