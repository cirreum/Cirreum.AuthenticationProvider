namespace Cirreum.AuthenticationProvider;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Cirreum authentication builder passed to the <c>configure</c> callback of the
/// umbrella package's <c>AddAuthentication(...)</c>. Carries the DI service collection, the
/// ASP.NET Core <see cref="AuthenticationBuilder"/>, and the host
/// <see cref="IConfiguration"/> so scheme extension methods can register schemes +
/// handlers + selectors and bind their own appsettings sections against the same
/// composition surface the framework-shipped registrars used.
/// </summary>
public interface IAuthenticationBuilder {

	/// <summary>
	/// The DI service collection — used for scoped/singleton service registration
	/// (resolvers, selectors, configuration).
	/// </summary>
	IServiceCollection Services { get; }

	/// <summary>
	/// The ASP.NET Core authentication builder — used by scheme extension methods to
	/// register <c>AddScheme&lt;TOptions, THandler&gt;</c> for dynamically-driven
	/// scheme registrations that bypass the appsettings-driven registrar flow.
	/// </summary>
	AuthenticationBuilder AuthBuilder { get; }

	/// <summary>
	/// The host configuration — used by app-facing composition verbs
	/// (e.g. <c>AddApiKey(...)</c>) to bind their provider's
	/// <c>Cirreum:Authentication:Providers:{Provider}</c> section at composition
	/// time, the same way the runtime's <c>RegisterAuthenticationProvider</c> helper does
	/// for auto-registered providers.
	/// </summary>
	IConfiguration Configuration { get; }

}
