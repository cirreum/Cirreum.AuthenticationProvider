namespace Cirreum.Authentication;

using Cirreum.Coordination;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Registers the canonical coordination scope for hosts that compose authentication.
/// </summary>
/// <remarks>
/// <see cref="CoordinationScope"/> is deliberately opaque to <c>Cirreum.Coordination</c> — the
/// primitive holds no opinion about what an application or an environment is. Supplying the
/// default therefore belongs to a composition surface that knows <c>IDomainEnvironment</c>, and
/// this is that surface: every auth-track verb that pulls coordination routes through here rather
/// than repeating the registration.
/// </remarks>
public static class CoordinationScopeServiceCollectionExtensions {

	/// <summary>
	/// Registers the canonical <c>{applicationName}:{environmentName}</c> coordination scope,
	/// unless one has already been registered.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	/// <remarks>
	/// Applications and environments sharing one distributed backend never share coordination
	/// state. Registered with <c>TryAdd</c> while <c>WithScope(...)</c> replaces, so an explicit
	/// scope wins in any call order. The in-memory backend ignores the scope entirely.
	/// </remarks>
	public static IServiceCollection AddDefaultCoordinationScope(this IServiceCollection services) {

		ArgumentNullException.ThrowIfNull(services);

		services.TryAddSingleton(static sp => {
			var environment = sp.GetService<IDomainEnvironment>()
				?? throw new InvalidOperationException(
					"The default CoordinationScope derives {app}:{env} from IDomainEnvironment, " +
					"which is not registered. Host via DomainApplication.CreateBuilder, or register " +
					"an explicit scope: auth.ConfigureCoordination(c => c.WithScope(...)).");
			return CoordinationScope.For(environment.ApplicationName, environment.EnvironmentName);
		});

		return services;
	}

}
