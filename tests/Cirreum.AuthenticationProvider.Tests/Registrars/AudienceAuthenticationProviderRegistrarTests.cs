namespace Cirreum.AuthenticationProvider.Tests.Registrars;

using Cirreum.AuthenticationProvider;
using Cirreum.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Unit tests for the audience registrar base: per-instance
/// <see cref="AudienceSchemeRegistration"/> contribution (every instance's audience
/// must be represented — multi-instance compositions previously lost all but the
/// last), scheme-name derivation, and the collection-scoped instance-key guard.
/// </summary>
public class AudienceAuthenticationProviderRegistrarTests {

	private static readonly IConfiguration EmptyConfiguration =
		new ConfigurationBuilder().Build();

	static AudienceAuthenticationProviderRegistrarTests() {
		// One-shot process-global runtime-type switch; tolerate another test class
		// having set it first — all suites in this assembly use WebApi.
		try {
			ProviderContext.SetRuntimeType(ProviderRuntimeType.WebApi);
		} catch (InvalidOperationException) {
		}
	}

	[Fact]
	public void Register_EnabledInstance_ContributesAudienceSchemeRegistration() {
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var settings = CreateSettings(("descope", "aud-project-id", true));

		registrar.Register(settings, services, EmptyConfiguration, new AuthenticationBuilder(services));

		var registration = Registrations(services).Should().ContainSingle().Subject;
		registration.Audience.Should().Be("aud-project-id");
		registration.Scheme.Should().Be("descope");
		registration.ProviderName.Should().Be("TestAudience");
	}

	[Fact]
	public void Register_MultipleInstances_ContributesEveryAudience() {
		// The wave regression: every instance's audience must survive composition —
		// not just the last-registered one.
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var settings = CreateSettings(
			("descope", "aud-descope", true),
			("entraWorkforce", "aud-workforce", true),
			("entraExternal", "aud-external", true));

		registrar.Register(settings, services, EmptyConfiguration, new AuthenticationBuilder(services));

		Registrations(services).Select(r => (r.Audience, r.Scheme)).Should().BeEquivalentTo([
			("aud-descope", "descope"),
			("aud-workforce", "entraWorkforce"),
			("aud-external", "entraExternal"),
		]);
	}

	[Fact]
	public void Register_DisabledInstance_ContributesNothing() {
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var settings = CreateSettings(("descope", "aud-descope", false));

		registrar.Register(settings, services, EmptyConfiguration, new AuthenticationBuilder(services));

		Registrations(services).Should().BeEmpty();
		registrar.WebApiSchemes.Should().BeEmpty();
	}

	[Fact]
	public void Register_WebApiRuntime_UsesWebApiRegistrationPath() {
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var settings = CreateSettings(("descope", "aud-descope", true));

		registrar.Register(settings, services, EmptyConfiguration, new AuthenticationBuilder(services));

		registrar.WebApiSchemes.Should().BeEquivalentTo(["descope"]);
		registrar.WebAppSchemes.Should().BeEmpty();
	}

	[Fact]
	public void Register_MissingAudience_Throws() {
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var settings = CreateSettings(("descope", "", true));

		var act = () => registrar.Register(settings, services, EmptyConfiguration, new AuthenticationBuilder(services));

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*Audience*");
	}

	[Fact]
	public void RegisterInstance_AutoDerivesSchemeFromKey() {
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var instance = CreateInstance("aud-descope");

		registrar.RegisterInstance("descope", instance, services, EmptyConfiguration, new AuthenticationBuilder(services));

		instance.Scheme.Should().Be("descope");
	}

	[Fact]
	public void RegisterInstance_MismatchedScheme_Throws() {
		var services = new ServiceCollection();
		var registrar = new TestAudienceRegistrar();
		var instance = CreateInstance("aud-descope");
		instance.Scheme = "other";

		var act = () => registrar.RegisterInstance("descope", instance, services, EmptyConfiguration, new AuthenticationBuilder(services));

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*Scheme*");
	}

	[Fact]
	public void RegisterInstance_DuplicateKeyInSameCollection_Throws() {
		var services = new ServiceCollection();
		var authBuilder = new AuthenticationBuilder(services);
		new TestAudienceRegistrar()
			.RegisterInstance("descope", CreateInstance("aud-1"), services, EmptyConfiguration, authBuilder);

		var act = () => new TestAudienceRegistrar()
			.RegisterInstance("descope", CreateInstance("aud-2"), services, EmptyConfiguration, authBuilder);

		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*descope*");
	}

	[Fact]
	public void RegisterInstance_SameKeyInFreshCollection_DoesNotThrow() {
		// Two hosts composed in one process must be isolated: the duplicate guard is
		// collection-scoped, not process-global.
		var first = new ServiceCollection();
		new TestAudienceRegistrar()
			.RegisterInstance("descope", CreateInstance("aud-1"), first, EmptyConfiguration, new AuthenticationBuilder(first));

		var second = new ServiceCollection();
		var act = () => new TestAudienceRegistrar()
			.RegisterInstance("descope", CreateInstance("aud-1"), second, EmptyConfiguration, new AuthenticationBuilder(second));

		act.Should().NotThrow();
	}

	private static TestAudienceInstanceSettings CreateInstance(string audience, bool enabled = true) => new() {
		Enabled = enabled,
		Audience = audience,
	};

	private static TestAudienceSettings CreateSettings(params (string Key, string Audience, bool Enabled)[] instances) {
		var settings = new TestAudienceSettings();
		foreach (var (key, audience, enabled) in instances) {
			settings.Instances[key] = CreateInstance(audience, enabled);
		}
		return settings;
	}

	private static List<AudienceSchemeRegistration> Registrations(IServiceCollection services) =>
		[.. services.Select(d => d.ImplementationInstance).OfType<AudienceSchemeRegistration>()];

}
