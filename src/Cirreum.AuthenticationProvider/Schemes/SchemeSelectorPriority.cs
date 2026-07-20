namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Conventional priority values for <see cref="ISchemeSelector.Priority"/>. Selectors
/// dispatch in ascending priority order — lower runs first.
/// </summary>
/// <remarks>
/// <para>
/// Slots 100-400 are reserved for selectors backed by a Cirreum-shipped authentication
/// handler (ApiKey, SignedRequest, SessionTicket, External). Each Cirreum scheme owns
/// one slot.
/// </para>
/// <para>
/// Slot 900 is reserved for the framework-shipped <c>JwtAudienceSchemeSelector</c> — the
/// routing-to-foreign-handler bridge that dispatches JWTs to ASP.NET's <c>JwtBearer</c>
/// handler (via Microsoft Identity Web or equivalent) by matching the <c>aud</c> claim
/// against the registered <see cref="AudienceSchemeRegistration"/> set. The numeric gap
/// from 400 to 900 reflects that this selector doesn't dispatch to a Cirreum-shipped
/// handler — it routes to a foreign one.
/// </para>
/// <para>
/// Gaps in the numbering (500-800) are available for future Cirreum schemes (mTLS,
/// DPoP, etc.) or app-defined custom selectors that want to slot between the canonical
/// priorities. Apps may use any int value; the constants here are conventional.
/// </para>
/// </remarks>
public static class SchemeSelectorPriority {

	/// <summary>Conflict sentinel — runs first; detects distinct credential-carriers on the request.</summary>
	public const int Conflict = 0;

	/// <summary>ApiKey scheme selector (<c>Cirreum.Authentication.ApiKey</c>).</summary>
	public const int Key = 100;

	/// <summary>SignedRequest scheme selector (<c>Cirreum.Authentication.SignedRequest</c>).</summary>
	public const int Signed = 200;

	/// <summary>SessionTicket scheme selector (<c>Cirreum.Authentication.SessionTicket</c>).</summary>
	public const int Session = 300;

	/// <summary>External (BYOID) scheme selector (<c>Cirreum.Authentication.External</c>).</summary>
	public const int External = 400;

	/// <summary>
	/// JWT-audience routing selector — matches the <c>aud</c> claim against the
	/// registered <see cref="AudienceSchemeRegistration"/> set and routes to a foreign
	/// handler (ASP.NET <c>JwtBearer</c> via MS Identity Web). Numeric gap from 400 to
	/// 900 reflects the structural distinction between scheme-impl-backed selectors and
	/// this routing-to-foreign-handler bridge.
	/// </summary>
	public const int Audience = 900;

	/// <summary>
	/// Anonymous catch-all — runs last; always claims; the paired handler returns
	/// <see cref="Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult"/>
	/// so <c>[AllowAnonymous]</c> endpoints work cleanly.
	/// </summary>
	public const int Anonymous = 999;

}
