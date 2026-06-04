namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Well-known authentication scheme names registered by Cirreum's scheme
/// implementations and consumed by the dynamic forward resolver, app composition,
/// and audit code paths.
/// </summary>
/// <remarks>
/// Scheme names are stable string constants centralized here to prevent
/// drift between registration and consumption. Scheme packages reference
/// these constants when calling <c>AddAuthentication().AddScheme(...)</c>; app code
/// references them in <c>[Authorize(AuthenticationSchemes = ...)]</c> attributes when
/// pinning specific schemes is desired.
/// </remarks>
public static class AuthenticationSchemes {

	/// <summary>
	/// ApiKey scheme — header/Bearer transport for M2M credentials.
	/// Implemented in <c>Cirreum.Authentication.ApiKey</c>.
	/// </summary>
	public const string ApiKey = "ApiKey";

	/// <summary>
	/// SignedRequest scheme — RFC 9421-aligned HTTP message signatures for M2M
	/// credentials with key-pair authentication.
	/// Implemented in <c>Cirreum.Authentication.SignedRequest</c>.
	/// </summary>
	public const string SignedRequest = "SignedRequest";

	/// <summary>
	/// SessionTicket scheme — bridges HTTP-authenticated callers to long-lived
	/// connection establishment (WebSocket, SignalR, gRPC streaming).
	/// Implemented in <c>Cirreum.Authentication.SessionTicket</c>.
	/// </summary>
	public const string SessionTicket = "SessionTicket";

	/// <summary>
	/// Anonymous-pending-auth pseudo-scheme — accepts unauthenticated callers on
	/// endpoints marked with <see cref="AllowPendingAuthAttribute"/>.
	/// </summary>
	public const string AnonymousPendingAuth = "AnonymousPendingAuth";

	// Framework-shipped schemes (implemented in Cirreum.Runtime.AuthenticationProvider).
	// Apps reference these constants in [Authorize(AuthenticationSchemes = ...)] when pinning.

	/// <summary>
	/// Dynamic forward scheme — ASP.NET <c>PolicyScheme</c> that iterates registered
	/// <see cref="ISchemeSelector"/> instances by <see cref="ISchemeSelector.Priority"/>
	/// ascending and forwards to the first claimant's scheme. The configured default
	/// scheme for Cirreum-hosted apps.
	/// </summary>
	public const string Dynamic = "Cirreum.Dynamic";

	/// <summary>
	/// Ambiguous-request scheme — claimed by the framework-shipped conflict sentinel
	/// (<see cref="SchemeSelectorPriority.Conflict"/>) when a request carries distinct
	/// credential-carriers (for example, a custom header AND <c>Authorization: Bearer</c>).
	/// The handler fails closed with 401.
	/// </summary>
	public const string Ambiguous = "Cirreum.Ambiguous";

	/// <summary>
	/// Anonymous fallback scheme — claimed when no other selector matches; the handler
	/// returns <see cref="Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult"/>
	/// so <c>[AllowAnonymous]</c> endpoints continue to work.
	/// </summary>
	public const string Anonymous = "Cirreum.Anonymous";

}
