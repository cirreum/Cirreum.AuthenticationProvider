namespace Cirreum.AuthenticationProvider.Configuration;

using Cirreum.Providers.Configuration;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Abstract base class for authentication provider instance settings. Carries the
/// common configuration properties shared by every authentication scheme instance.
/// </summary>
/// <remarks>
/// Reflects the recognition of Authentication as a first-class provider pillar. The
/// type-by-type rename from the deprecated <c>Cirreum.AuthorizationProvider</c> shape
/// places this base in the Authentication pillar where it belongs.
/// </remarks>
public abstract class AuthenticationProviderInstanceSettings
	: IProviderInstanceSettings {

	/// <summary>
	/// Gets the ASP.NET Core authentication scheme name for this provider instance.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <strong>Auto-populated from the instance key</strong> during provider registration
	/// (see <see cref="AuthenticationProviderRegistrar{TSettings, TInstanceSettings}.RegisterInstance"/>).
	/// The instance key under <c>Instances:</c> in <c>appsettings.json</c> serves double duty
	/// as both the logical instance name and the ASP.NET Core authentication scheme name.
	/// </para>
	/// <para>
	/// Do <strong>not</strong> set <c>Scheme</c> in configuration. If a mismatched value is
	/// detected during registration, an <see cref="InvalidOperationException"/> is thrown.
	/// </para>
	/// <example>
	/// <code>
	/// // appsettings.json
	/// //   "Oidc": { "Instances": { "descope": { ... } } }
	/// //                           ^^^^^^^^ — this key becomes the scheme name
	/// </code>
	/// </example>
	/// </remarks>
	public string Scheme { get; set; } = "";

	/// <summary>
	/// Gets or sets a value indicating whether this provider instance is enabled.
	/// When <see langword="false"/>, the instance will be skipped during registration.
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// Gets or sets the raw <see cref="IConfigurationSection"/> used to pass
	/// provider-specific configuration to the authentication builder. This section
	/// contains the detailed configuration for this instance.
	/// </summary>
	public IConfigurationSection? Section { get; set; }

}
