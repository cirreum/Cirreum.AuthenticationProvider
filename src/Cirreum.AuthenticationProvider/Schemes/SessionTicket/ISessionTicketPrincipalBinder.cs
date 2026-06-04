namespace Cirreum.AuthenticationProvider.SessionTicket;

using System.Security.Claims;

/// <summary>
/// Builds the <see cref="ClaimsPrincipal"/> bound to the connection on successful
/// session-ticket validation. The principal flows into
/// <c>IInvocationConnection.User</c> and from there into per-invocation
/// <c>IUserState</c> for the lifetime of the connection.
/// </summary>
/// <remarks>
/// <para>
/// Default implementation in
/// <c>Cirreum.Authentication.SessionTicket</c> handles the common case (sub + name +
/// claims pass-through); apps override for app-specific claim shapes (e.g., adding
/// tenant identifiers, mapping role claims from custom names).
/// </para>
/// <para>
/// Implementations are stateless and registered as singletons. The binder is called
/// once per accepted ticket — the resulting principal is cached for the lifetime of
/// the connection.
/// </para>
/// </remarks>
public interface ISessionTicketPrincipalBinder {

	/// <summary>
	/// Builds the <see cref="ClaimsPrincipal"/> for <paramref name="ticket"/>.
	/// Implementations control claim shape, identity type, and any framework-specific
	/// claim mapping.
	/// </summary>
	ClaimsPrincipal BuildPrincipal(SessionTicket ticket);

}
