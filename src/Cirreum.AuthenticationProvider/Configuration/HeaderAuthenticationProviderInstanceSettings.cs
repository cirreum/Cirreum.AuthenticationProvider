namespace Cirreum.AuthenticationProvider.Configuration;

/// <summary>
/// Abstract base class for header-based authentication provider instance settings.
/// Used by schemes that read a credential from an HTTP header — API keys, the
/// future RFC 9421 SignedRequest signatures, etc.
/// </summary>
public abstract class HeaderAuthenticationProviderInstanceSettings
	: AuthenticationProviderInstanceSettings {

	/// <summary>
	/// Gets or sets the HTTP header name where the credential is expected.
	/// Defaults to <c>X-Api-Key</c>; ApiKey instances opting into Bearer transport
	/// override at the scheme level.
	/// </summary>
	public string HeaderName { get; set; } = "X-Api-Key";

	/// <summary>
	/// Gets or sets the unique client identifier assigned to authenticated requests.
	/// Surfaces as <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>.
	/// </summary>
	public string ClientId { get; set; } = "";

	/// <summary>
	/// Gets or sets the display name for the client. Surfaces as
	/// <see cref="System.Security.Claims.ClaimTypes.Name"/>.
	/// </summary>
	public string ClientName { get; set; } = "";

	/// <summary>
	/// Gets or sets the roles to assign to the authenticated principal. Surfaces as
	/// <see cref="System.Security.Claims.ClaimTypes.Role"/> claims.
	/// </summary>
	public List<string> Roles { get; set; } = [];

}
