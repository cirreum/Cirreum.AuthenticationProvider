namespace Cirreum.AuthenticationProvider.SessionTicket;

/// <summary>
/// Parameters for minting a <see cref="SessionTicket"/> via <see cref="ISessionTicketIssuer"/>.
/// </summary>
/// <remarks>
/// App code (negotiate endpoints, webhook handlers) constructs an
/// <see cref="SessionTicketIssueRequest"/> from its already-authenticated context and
/// passes it to the issuer. The issuer is responsible for ticket-value generation
/// (opaque vs JWT), persistence (opaque-variant), and signing (JWT-variant).
/// </remarks>
public sealed record SessionTicketIssueRequest {

	/// <summary>
	/// The authenticated subject the ticket binds to. The caller has already
	/// authenticated this subject upstream — the issuer does not re-authenticate.
	/// </summary>
	public required string Subject { get; init; }

	/// <summary>
	/// How long the ticket should remain valid from issuance. Issuers compute
	/// <c>SessionTicket.ExpiresAt = now + Lifetime</c>. Short lifetimes are the v1
	/// posture.
	/// </summary>
	public required TimeSpan Lifetime { get; init; }

	/// <summary>
	/// Optional app-defined channel annotation flowing into <c>SessionTicket.Channel</c>.
	/// </summary>
	public string? Channel { get; init; }

	/// <summary>
	/// Optional app-defined correlation reference flowing into
	/// <c>SessionTicket.Reference</c>.
	/// </summary>
	public string? Reference { get; init; }

	/// <summary>
	/// Optional additional claims to bind onto the issued ticket. Flowed into
	/// <c>SessionTicket.Claims</c>; the issuer is free to add framework-managed claims
	/// alongside (e.g., issuance timestamp for audit).
	/// </summary>
	public IReadOnlyDictionary<string, string>? Claims { get; init; }

}
