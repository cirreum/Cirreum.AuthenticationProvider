namespace Cirreum.Authentication;

using Cirreum.AuthenticationProvider;
using Cirreum.Coordination;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Auth-track convenience over the neutral <c>Cirreum.Coordination</c> primitive: exposes
/// <c>services.AddCoordination(...)</c> as <c>auth.ConfigureCoordination(...)</c> so the coordination backend
/// can be chosen inside the <c>AddAuthentication(...)</c> composition callback, alongside the schemes that
/// consume it.
/// </summary>
public static class CoordinationAuthenticationBuilderExtensions {

	/// <summary>
	/// Selects the shared coordination backend, forwarding to the neutral
	/// <c>services.AddCoordination(configure)</c>. Schemes that need coordination pull the requirement
	/// themselves (SignedRequest strict-nonce consumes <see cref="IReplayGuard"/>); this verb just lets the
	/// application choose the backend (<c>c =&gt; c.UseInMemory()</c>, or <c>c =&gt; c.UseRedis()</c> with
	/// <c>Cirreum.Coordination.Redis</c> referenced).
	/// </summary>
	/// <param name="builder">The Cirreum authentication builder.</param>
	/// <param name="configure">Selects the backend.</param>
	/// <returns>The builder for chaining.</returns>
	/// <remarks>
	/// When no <see cref="CoordinationScope"/> has been registered, this call defaults it
	/// to the canonical <c>{applicationName}:{environmentName}</c> (from
	/// <c>IDomainEnvironment</c>), so applications and environments sharing a distributed
	/// backend never share coordination state. An explicit
	/// <c>configure(c =&gt; c.WithScope(...))</c> always wins, in any order — the default
	/// registers via <c>TryAdd</c> while <c>WithScope</c> replaces. The in-memory backend
	/// ignores the scope entirely.
	/// </remarks>
	public static IAuthenticationBuilder ConfigureCoordination(
		this IAuthenticationBuilder builder,
		Action<CoordinationBuilder> configure) {

		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(configure);

		builder.Services.AddCoordination(configure);

		// Default the coordination scope to the canonical {app}:{env}. TryAdd, and
		// WithScope(...) uses Replace — so an explicit scope wins in any order.
		builder.Services.TryAddSingleton<CoordinationScope>(static sp => {
			var environment = sp.GetRequiredService<IDomainEnvironment>();
			return CoordinationScope.For(environment.ApplicationName, environment.EnvironmentName);
		});

		return builder;
	}

}
