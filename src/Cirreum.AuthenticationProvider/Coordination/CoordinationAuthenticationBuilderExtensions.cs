namespace Cirreum.Authentication;

using Cirreum.AuthenticationProvider;
using Cirreum.AuthenticationProvider.Coordination;

/// <summary>
/// The <c>AddCoordination(...)</c> composition verb, available inside the <c>configure</c> callback of the
/// umbrella <c>AddAuthentication(...)</c>. Registers the atomic-coordination backend
/// (<see cref="IReplayGuard"/> + <see cref="IRequestThrottle"/>) ONCE, shared by every scheme that needs it.
/// </summary>
public static class CoordinationAuthenticationBuilderExtensions {

	/// <summary>
	/// Configures the shared atomic-coordination backend. Call once during composition; schemes that need
	/// it (ApiKey <c>SelfContained</c> consumes <see cref="IRequestThrottle"/>; SignedRequest strict-nonce
	/// consumes <see cref="IReplayGuard"/>) resolve the registered interface. Omit entirely when no scheme
	/// needs in-app coordination (ApiKey <c>EdgeThrottled</c>/<c>Baseline</c>, SignedRequest window-only).
	/// </summary>
	/// <param name="builder">The Cirreum authentication builder.</param>
	/// <param name="configure">Selects the backend, e.g. <c>c =&gt; c.UseInMemory()</c> or, with the Redis
	/// adapter package referenced, <c>c =&gt; c.UseRedis()</c>.</param>
	/// <returns>The builder for chaining.</returns>
	public static IAuthenticationBuilder AddCoordination(
		this IAuthenticationBuilder builder,
		Action<CoordinationBuilder> configure) {

		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(configure);

		configure(new CoordinationBuilder(builder.Services));
		return builder;
	}

}
