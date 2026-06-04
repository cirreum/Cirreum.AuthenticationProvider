namespace Cirreum.AuthenticationProvider.SessionTicket;

/// <summary>
/// Validates an inbound session-ticket value at handshake. Returns the validated
/// <see cref="SessionTicket"/> when the ticket is accepted, or <see langword="null"/>
/// when rejected.
/// </summary>
/// <remarks>
/// The SessionTicket scheme handler calls
/// <see cref="ValidateAsync"/> from its <c>AuthenticationHandler&lt;TOptions&gt;</c>
/// at WebSocket / SignalR / gRPC-streaming handshake. Single-use semantics (where
/// applicable) are enforced inside the validator — typically by atomically removing
/// the ticket from <see cref="ISessionStore"/> on first successful validation.
/// </remarks>
public interface ISessionTicketValidator {

	/// <summary>
	/// Validates <paramref name="ticketValue"/> and returns the bound ticket on success.
	/// Returns <see langword="null"/> for unknown, expired, malformed, or revoked tickets.
	/// </summary>
	/// <param name="ticketValue">The ticket value as carried on the inbound request
	/// (subprotocol, cookie, query, or JWT bearer).</param>
	/// <param name="cancellationToken">Cancellation token for the validation operation
	/// (relevant when the validator consults a store).</param>
	ValueTask<SessionTicket?> ValidateAsync(string ticketValue, CancellationToken cancellationToken);

}
