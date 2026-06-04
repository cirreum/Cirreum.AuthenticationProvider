namespace Cirreum.AuthenticationProvider.Configuration;

using Cirreum.Providers.Configuration;

/// <summary>
/// Abstract base class for an authentication provider's per-instance configuration
/// collection. Each entry under <see cref="Instances"/> is a named instance of the
/// scheme (e.g., one ApiKey configuration per partner; multiple OIDC tenants).
/// </summary>
/// <typeparam name="TInstanceSettings">The per-instance settings type for the
/// concrete scheme.</typeparam>
public abstract class AuthenticationProviderSettings<TInstanceSettings>
	: IProviderSettings<TInstanceSettings>
	where TInstanceSettings : AuthenticationProviderInstanceSettings {

	/// <summary>
	/// Gets or sets the collection of provider instance settings keyed by instance
	/// name. Each instance represents a separate configuration of the same scheme
	/// (different audiences, different keys, different headers).
	/// </summary>
	public Dictionary<string, TInstanceSettings> Instances { get; set; } = [];

}
