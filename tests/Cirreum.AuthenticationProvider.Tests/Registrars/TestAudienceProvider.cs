namespace Cirreum.AuthenticationProvider.Tests.Registrars;

using Cirreum.AuthenticationProvider;
using Cirreum.AuthenticationProvider.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Concrete test doubles for exercising
/// <see cref="AudienceAuthenticationProviderRegistrar{TSettings, TInstanceSettings}"/>
/// (and, through it, the registrar base's instance-key guard and scheme derivation).
/// </summary>
internal sealed class TestAudienceInstanceSettings : AudienceAuthenticationProviderInstanceSettings {
}

internal sealed class TestAudienceSettings : AuthenticationProviderSettings<TestAudienceInstanceSettings> {
}

internal sealed class TestAudienceRegistrar : AudienceAuthenticationProviderRegistrar<TestAudienceSettings, TestAudienceInstanceSettings> {

	public override string ProviderName => "TestAudience";

	public List<string> WebApiSchemes { get; } = [];

	public List<string> WebAppSchemes { get; } = [];

	public override void AddAuthenticationForWebApi(
		IConfigurationSection instanceSection,
		TestAudienceInstanceSettings providerSettings,
		AuthenticationBuilder authBuilder) {
		this.WebApiSchemes.Add(providerSettings.Scheme);
	}

	public override void AddAuthenticationForWebApp(
		IConfigurationSection instanceSection,
		TestAudienceInstanceSettings providerSettings,
		AuthenticationBuilder authBuilder) {
		this.WebAppSchemes.Add(providerSettings.Scheme);
	}

}
