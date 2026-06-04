namespace Cirreum.AuthenticationProvider;

using Cirreum.AuthenticationProvider.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Abstract base class for header-based authentication provider registrars. Validates
/// the common header-credential properties (<c>HeaderName</c>, <c>ClientId</c>) and
/// delegates the ASP.NET Core scheme handler registration to the derived class.
/// </summary>
/// <typeparam name="TSettings">The provider's settings type.</typeparam>
/// <typeparam name="TInstanceSettings">The provider's per-instance settings type.</typeparam>
/// <remarks>
/// <para>
/// Scheme-specific credential resolution (e.g., ApiKey client registry lookup,
/// SignedRequest signature verification) is the derived registrar's responsibility —
/// the base only handles cross-scheme header-validation concerns. This keeps the
/// abstraction free of scheme-specific knowledge that belongs in scheme packages.
/// </para>
/// </remarks>
public abstract class HeaderAuthenticationProviderRegistrar<TSettings, TInstanceSettings>
	: AuthenticationProviderRegistrar<TSettings, TInstanceSettings>
	where TInstanceSettings : HeaderAuthenticationProviderInstanceSettings
	where TSettings : AuthenticationProviderSettings<TInstanceSettings> {

	/// <inheritdoc/>
	protected override void RegisterScheme(
		string key,
		TInstanceSettings settings,
		IServiceCollection services,
		IConfiguration configuration,
		AuthenticationBuilder authBuilder) {

		if (string.IsNullOrWhiteSpace(settings.HeaderName)) {
			throw new InvalidOperationException(
				$"Header-based provider instance '{key}' requires a HeaderName.");
		}

		if (string.IsNullOrWhiteSpace(settings.ClientId)) {
			throw new InvalidOperationException(
				$"Header-based provider instance '{key}' requires a ClientId.");
		}

		this.AddAuthenticationHandler(key, settings, services, configuration, authBuilder);
	}

	/// <summary>
	/// Registers the ASP.NET Core authentication handler and any scheme-specific
	/// state (client registries, key stores, dispatch selectors) for this instance.
	/// </summary>
	/// <param name="key">The instance key (also the ASP.NET Core scheme name).</param>
	/// <param name="settings">The instance settings.</param>
	/// <param name="services">The DI service collection.</param>
	/// <param name="configuration">The root configuration.</param>
	/// <param name="authBuilder">The ASP.NET Core authentication builder.</param>
	protected abstract void AddAuthenticationHandler(
		string key,
		TInstanceSettings settings,
		IServiceCollection services,
		IConfiguration configuration,
		AuthenticationBuilder authBuilder);

}
