namespace Cirreum.AuthenticationProvider.Configuration;

/// <summary>
/// Abstract base class for audience-based authentication provider instance settings.
/// Used by schemes that route by JWT audience claim — Entra, Okta, Ping, generic
/// OIDC bearer.
/// </summary>
public abstract class AudienceAuthenticationProviderInstanceSettings
	: AuthenticationProviderInstanceSettings {

	/// <summary>
	/// Gets or sets the JWT audience claim value bound to this scheme instance.
	/// Inbound bearer tokens whose <c>aud</c> claim matches dispatch to this scheme.
	/// </summary>
	public string Audience { get; set; } = "";

}
