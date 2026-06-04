namespace Cirreum.AuthenticationProvider.SessionTicket;

/// <summary>
/// Mints session tickets. Called by app code (negotiate endpoints, webhook handlers)
/// after the upstream authentication has produced an authenticated subject.
/// </summary>
/// <remarks>
/// The implementation in <c>Cirreum.Authentication.SessionTicket</c>
/// ships opaque-ticket and JWT-ticket variants; apps can register custom issuers for
/// app-specific ticket shapes. Issuer implementations handle ticket-value generation,
/// persistence (opaque), signing (JWT), and expiry semantics.
/// </remarks>
public interface ISessionTicketIssuer {

	/// <summary>
	/// Mints a session ticket from the supplied <paramref name="request"/>.
	/// </summary>
	/// <param name="request">The issuance parameters; the caller has already
	/// authenticated <c>request.Subject</c> upstream.</param>
	/// <param name="cancellationToken">Cancellation token for the issuance operation
	/// (relevant when the issuer persists the ticket).</param>
	/// <returns>The newly minted ticket. <see cref="SessionTicket.TicketValue"/> is the
	/// value the caller returns to the partner / client for use at handshake.</returns>
	ValueTask<SessionTicket> IssueAsync(SessionTicketIssueRequest request, CancellationToken cancellationToken);

}
