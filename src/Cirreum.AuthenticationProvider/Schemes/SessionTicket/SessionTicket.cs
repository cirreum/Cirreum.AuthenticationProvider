namespace Cirreum.AuthenticationProvider.SessionTicket;

/// <summary>
/// Represents a session-establishment credential that binds an authenticated subject
/// to a long-lived connection.
/// </summary>
/// <remarks>
/// Session tickets are opaque credentials validated through an <see cref="ISessionStore"/>.
/// <see cref="Channel"/> and <see cref="Reference"/> are application-defined metadata and
/// are not used for authorization.
/// </remarks>
public sealed record SessionTicket {

	/// <summary>
	/// Gets the opaque ticket value used to identify and validate the session ticket.
	/// </summary>
	/// <remarks>
	/// Callers should treat this value as opaque and must not derive semantics from it.
	/// </remarks>
	public required string TicketValue { get; init; }

	/// <summary>
	/// Gets the authenticated subject associated with the ticket.
	/// </summary>
	public required string Subject { get; init; }

	/// <summary>
	/// Gets the authentication scheme that originally authenticated the subject,
	/// or <see langword="null"/> when unknown.
	/// </summary>
	/// <remarks>
	/// This identifies the subject's originating authentication scheme, not the
	/// session-ticket authentication scheme.
	/// </remarks>
	public string? Scheme { get; init; }

	/// <summary>
	/// Gets the absolute expiration time of the ticket.
	/// </summary>
	public required DateTimeOffset ExpiresAt { get; init; }

	/// <summary>
	/// Gets application-defined channel metadata associated with the session.
	/// </summary>
	public string? Channel { get; init; }

	/// <summary>
	/// Gets an application-defined correlation reference associated with the session.
	/// </summary>
	public string? Reference { get; init; }

	/// <summary>
	/// Gets additional claims to bind to the resulting principal.
	/// </summary>
	/// <remarks>
	/// The <see cref="ISessionTicketPrincipalBinder"/> determines how these values
	/// are represented on the resulting principal.
	/// </remarks>
	public IReadOnlyDictionary<string, string>? Claims { get; init; }

}