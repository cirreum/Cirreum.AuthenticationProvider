namespace Cirreum.AuthenticationProvider.Security;

using Cirreum.Security;

/// <summary>
/// Default <see cref="IAuthenticationBoundaryResolver"/> — treats every authenticated caller as
/// <see cref="AuthenticationBoundary.Global"/> and every unauthenticated caller as
/// <see cref="AuthenticationBoundary.None"/>.
/// </summary>
/// <remarks>
/// The correct default for single-scheme applications where all authenticated users belong to the
/// operator's own IdP. Multi-tenant applications replace this with a scheme-aware resolver that
/// distinguishes the operator's primary scheme from tenant IdP schemes; the registration uses
/// <c>TryAdd</c> so a custom resolver registered first wins. Lives in the spine-reachable
/// <c>Cirreum.AuthenticationProvider</c> (not the opt-in Authentication track) so that
/// <c>IUserState</c> / <c>UserStateAccessor</c> always have a working boundary resolver, even in
/// apps that never reference the Authentication packages.
/// </remarks>
internal sealed class DefaultAuthenticationBoundaryResolver : IAuthenticationBoundaryResolver {

	/// <inheritdoc/>
	public AuthenticationBoundary Resolve(IUserState userState, string? authenticationScheme) =>
		userState.IsAuthenticated ? AuthenticationBoundary.Global : AuthenticationBoundary.None;

}
