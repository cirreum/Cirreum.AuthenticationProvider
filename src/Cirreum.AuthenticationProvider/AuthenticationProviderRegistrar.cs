namespace Cirreum.AuthenticationProvider;

using Cirreum.AuthenticationProvider.Configuration;
using Cirreum.Providers;
using Cirreum.Providers.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Abstract base class for authentication provider registrars. Handles per-instance
/// registration, scheme-name derivation, and instance-key deduplication; concrete
/// scheme registrars (ApiKey, SignedRequest, SessionTicket, audience-based OIDC)
/// derive from this or one of its specialized subclasses.
/// </summary>
/// <typeparam name="TSettings">The provider's settings type (collection of instances).</typeparam>
/// <typeparam name="TInstanceSettings">The provider's per-instance settings type.</typeparam>
/// <remarks>
/// <para>
/// Following the provider track packaging convention, this base lives in the
/// Authentication track's contracts package and reports
/// <see cref="ProviderType.Authentication"/>. Concrete scheme registrars derive from
/// <see cref="HeaderAuthenticationProviderRegistrar{TSettings, TInstanceSettings}"/> or
/// <see cref="AudienceAuthenticationProviderRegistrar{TSettings, TInstanceSettings}"/>
/// per the host-type-sensitivity split.
/// </para>
/// <para>
/// Composition is explicit, not discovered polymorphically. The umbrella package
/// (<c>Cirreum.Runtime.Authentication</c>) composes the framework-shipped scheme registrars
/// via explicit typed calls to the runtime's <c>RegisterAuthenticationProvider&lt;TRegistrar,
/// TSettings, TInstance&gt;(...)</c> extension method — that helper reads this
/// provider's configuration section, binds to <typeparamref name="TSettings"/>, and
/// calls <see cref="Register(TSettings, IServiceCollection, IConfiguration, AuthenticationBuilder)"/>.
/// Apps that add custom schemes call the same typed extension method themselves.
/// </para>
/// </remarks>
public abstract class AuthenticationProviderRegistrar<TSettings, TInstanceSettings>
	: IProviderRegistrar<TSettings, TInstanceSettings>
	where TInstanceSettings : AuthenticationProviderInstanceSettings
	where TSettings : AuthenticationProviderSettings<TInstanceSettings> {

	/// <inheritdoc/>
	public ProviderType ProviderType => ProviderType.Authentication;

	/// <summary>
	/// The provider name — used by the runtime's typed registration extension method to locate
	/// the configuration section at <c>Cirreum:Authentication:Providers:{ProviderName}</c>.
	/// </summary>
	public abstract string ProviderName { get; }

	/// <summary>
	/// Validates an instance's settings. The base implementation is a no-op; concrete
	/// registrars override with scheme-specific checks.
	/// </summary>
	public virtual void ValidateSettings(TInstanceSettings settings) {
	}

	/// <summary>
	/// Registers every enabled instance under <paramref name="providerSettings"/>.
	/// </summary>
	public virtual void Register(
		TSettings providerSettings,
		IServiceCollection services,
		IConfiguration configuration,
		AuthenticationBuilder authBuilder) {

		if (providerSettings is null || providerSettings.Instances.Count == 0) {
			return;
		}

		foreach (var (key, settings) in providerSettings.Instances) {
			if (!settings.Enabled) {
				continue;
			}

			this.RegisterInstance(key, settings, services, configuration, authBuilder);
		}
	}

	/// <summary>
	/// Registers a single provider instance. Enforces unique instance keys, validates
	/// the scheme-name footgun (instance key IS the scheme name), runs scheme-specific
	/// validation, then delegates to <see cref="RegisterScheme"/>.
	/// </summary>
	public virtual void RegisterInstance(
		string key,
		TInstanceSettings settings,
		IServiceCollection services,
		IConfiguration configuration,
		AuthenticationBuilder authBuilder) {

		// Duplicate-registration guard, scoped to this service collection — state lives
		// in the composition, not the process, so multiple hosts in one process are
		// fully isolated.
		var providerRegistrationKey = $"Cirreum.{this.ProviderType}.{this.ProviderName}::{key}";
		if (services.Any(d => d.ImplementationInstance is ProcessedInstanceKey processed
			&& processed.Value == providerRegistrationKey)) {
			throw new InvalidOperationException($"A service with the key of '{key}' has already been registered.");
		}
		services.AddSingleton(new ProcessedInstanceKey(providerRegistrationKey));

		if (settings is null) {
			throw new InvalidOperationException($"Missing required settings for the service '{key}'");
		}

		// The instance key IS the scheme name. Apps explicitly setting Scheme to a
		// mismatched value would silently overwrite — fail loudly instead.
		if (!string.IsNullOrWhiteSpace(settings.Scheme) && settings.Scheme != key) {
			throw new InvalidOperationException(
				$"Provider instance '{key}' has Scheme='{settings.Scheme}' configured, but the " +
				$"scheme name is auto-derived from the instance key. Remove the 'Scheme' value " +
				$"from configuration — the instance key IS the scheme name.");
		}

		settings.Scheme = key;

		this.ValidateSettings(settings);

		this.RegisterScheme(key, settings, services, configuration, authBuilder);
	}

	/// <summary>
	/// Gets the configuration section path for a specific instance under
	/// <c>Cirreum:Authentication:Providers:{ProviderName}:Instances:{instanceKey}</c>.
	/// </summary>
	protected string GetInstanceSectionPath(string instanceKey) =>
		$"Cirreum:{this.ProviderType}:Providers:{this.ProviderName}:Instances:{instanceKey}";

	/// <summary>
	/// Registers the ASP.NET Core authentication scheme for this instance. Derived
	/// classes implement this to handle audience-based or header-based registration.
	/// </summary>
	protected abstract void RegisterScheme(
		string key,
		TInstanceSettings settings,
		IServiceCollection services,
		IConfiguration configuration,
		AuthenticationBuilder authBuilder);

}
