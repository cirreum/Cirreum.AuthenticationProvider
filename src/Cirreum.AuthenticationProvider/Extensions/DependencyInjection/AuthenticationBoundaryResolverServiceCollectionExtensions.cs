namespace Cirreum;

using Cirreum.AuthenticationProvider.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Spine registration for the default <see cref="IAuthenticationBoundaryResolver"/>.
/// </summary>
public static class AuthenticationBoundaryResolverServiceCollectionExtensions {

	/// <summary>
	/// Registers <see cref="DefaultAuthenticationBoundaryResolver"/> as the
	/// <see cref="IAuthenticationBoundaryResolver"/> if one is not already registered (TryAdd —
	/// an app- or tenancy-supplied resolver registered first wins). Called by the server spine
	/// (<c>DomainApplicationBuilder.BuildDomainCore</c>) so every Cirreum server has a working
	/// boundary default independent of whether the opt-in Authentication track is referenced.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddDefaultAuthenticationBoundaryResolver(this IServiceCollection services) {
		ArgumentNullException.ThrowIfNull(services);
		services.TryAddSingleton<IAuthenticationBoundaryResolver, DefaultAuthenticationBoundaryResolver>();
		return services;
	}

}
