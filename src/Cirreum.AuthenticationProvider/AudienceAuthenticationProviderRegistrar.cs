namespace Cirreum.AuthenticationProvider;

using Cirreum.AuthenticationProvider.Configuration;
using Cirreum.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Abstract base class for audience-based authentication provider registrars. Used
/// by schemes that route by JWT audience claim (Entra, Okta, Ping, generic OIDC).
/// </summary>
/// <typeparam name="TSettings">The provider's settings type.</typeparam>
/// <typeparam name="TInstanceSettings">The provider's per-instance settings type.</typeparam>
/// <remarks>
/// <para>
/// The base validates the <c>Audience</c> property and dispatches to one of two
/// abstract methods depending on host runtime type — Web App (interactive cookie+OIDC
/// flow) versus Web API (JWT bearer). Concrete scheme packages implement both methods
/// for the schemes they support.
/// </para>
/// <para>
/// Audience-to-scheme dispatch at request time is handled by
/// <see cref="ISchemeSelector"/> implementations in runtime composition — the
/// registrar is concerned only with registering the ASP.NET Core scheme handlers,
/// not with how inbound requests get routed to them.
/// </para>
/// </remarks>
public abstract class AudienceAuthenticationProviderRegistrar<TSettings, TInstanceSettings>
	: AuthenticationProviderRegistrar<TSettings, TInstanceSettings>
	where TInstanceSettings : AudienceAuthenticationProviderInstanceSettings
	where TSettings : AuthenticationProviderSettings<TInstanceSettings> {

	/// <inheritdoc/>
	protected override void RegisterScheme(
		string key,
		TInstanceSettings settings,
		IServiceCollection services,
		IConfiguration configuration,
		AuthenticationBuilder authBuilder) {

		if (string.IsNullOrWhiteSpace(settings.Audience)) {
			throw new InvalidOperationException(
				$"Audience-based provider instance '{key}' requires an Audience.");
		}

		// Contribute this instance's audience → scheme routing entry. The
		// framework-shipped JwtAudienceSchemeSelector aggregates the full set from the
		// container at construction; the umbrella validates it at composition close.
		// Every audience-based scheme picks this up through the base — no per-scheme
		// wiring required.
		services.AddSingleton(new AudienceSchemeRegistration(
			settings.Audience,
			settings.Scheme,
			this.ProviderName));

		var instanceSection = configuration.GetSection(this.GetInstanceSectionPath(key));

		if (ProviderContext.GetRuntimeType() == ProviderRuntimeType.WebApp) {
			this.AddAuthenticationForWebApp(instanceSection, settings, authBuilder);
		} else {
			this.AddAuthenticationForWebApi(instanceSection, settings, authBuilder);
		}
	}

	/// <summary>
	/// Adds the authentication scheme configuration for Web API applications
	/// (JWT bearer flow).
	/// </summary>
	public abstract void AddAuthenticationForWebApi(
		IConfigurationSection instanceSection,
		TInstanceSettings providerSettings,
		AuthenticationBuilder authBuilder);

	/// <summary>
	/// Adds the authentication scheme configuration for Web App applications
	/// (interactive cookie + OIDC flow).
	/// </summary>
	public abstract void AddAuthenticationForWebApp(
		IConfigurationSection instanceSection,
		TInstanceSettings providerSettings,
		AuthenticationBuilder authBuilder);

}
