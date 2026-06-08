namespace Cirreum.Authentication;

using Cirreum.AuthenticationProvider;
using Cirreum.Coordination;

/// <summary>
/// Auth-track convenience over the neutral <c>Cirreum.Coordination</c> primitive: exposes
/// <c>services.AddCoordination(...)</c> as <c>auth.AddCoordination(...)</c> so the coordination backend can be
/// chosen inside the <c>AddAuthentication(...)</c> composition callback, alongside the schemes that consume it.
/// </summary>
public static class CoordinationAuthenticationBuilderExtensions {

	/// <summary>
	/// Selects the shared atomic-coordination backend, forwarding to the neutral
	/// <c>services.AddCoordination(configure)</c>. Schemes that need coordination pull the requirement
	/// themselves (SignedRequest strict-nonce consumes <see cref="IReplayGuard"/>; ApiKey <c>SelfContained</c>
	/// consumes <see cref="IRequestThrottle"/>); this verb just lets the application choose the backend
	/// (<c>c =&gt; c.UseInMemory()</c>, or <c>c =&gt; c.UseRedis()</c> with <c>Cirreum.Coordination.Redis</c> referenced).
	/// </summary>
	/// <param name="builder">The Cirreum authentication builder.</param>
	/// <param name="configure">Selects the backend.</param>
	/// <returns>The builder for chaining.</returns>
	public static IAuthenticationBuilder AddCoordination(
		this IAuthenticationBuilder builder,
		Action<CoordinationBuilder> configure) {

		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(configure);

		builder.Services.AddCoordination(configure);
		return builder;
	}

}
