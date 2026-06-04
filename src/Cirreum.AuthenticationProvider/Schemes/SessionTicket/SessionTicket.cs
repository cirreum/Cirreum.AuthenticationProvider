namespace Cirreum.AuthenticationProvider.SessionTicket;

/// <summary>
/// A session-establishment credential minted by app code (negotiate endpoints, webhook
/// handlers) and validated at WebSocket / SignalR / gRPC-streaming handshake. Binds an
/// authenticated subject to a long-lived connection without re-running the upstream
/// authentication flow on every reconnect.
/// </summary>
/// <remarks>
/// <para>
/// SessionTicket has two variants implemented in
/// <c>Cirreum.Authentication.SessionTicket</c>: an opaque variant (random bytes + DB
/// lookup via <see cref="ISessionStore"/>) and a JWT variant (RFC 7519 / RFC 8725 /
/// RFC 9068 conventions; self-contained validation). The record here is variant-
/// agnostic — both variants surface as <see cref="SessionTicket"/> after validation.
/// </para>
/// <para>
/// <see cref="Channel"/> and <see cref="Reference"/> are app-defined annotations that
/// flow into <c>IRequestOrigin</c> on binding. They are <em>not security-relevant</em> —
/// the authorization pipeline does not branch on them.
/// </para>
/// </remarks>
public sealed record SessionTicket {

	/// <summary>
	/// The opaque ticket value as carried on the wire (subprotocol, cookie, query, or
	/// JWT bearer). Validation contract: the value uniquely identifies the ticket;
	/// callers MUST NOT parse semantics out of it directly.
	/// </summary>
	public required string TicketValue { get; init; }

	/// <summary>
	/// The authenticated subject the ticket is bound to. Populated from the upstream
	/// authentication that minted the ticket; this is what becomes the connection's
	/// principal name after binding.
	/// </summary>
	public required string Subject { get; init; }

	/// <summary>
	/// Absolute expiry. Validators reject tickets after this instant; short TTLs
	/// (minutes, not days) are the v1 hardening posture (no DPoP / sender-constrained
	/// tokens today).
	/// </summary>
	public required DateTimeOffset ExpiresAt { get; init; }

	/// <summary>
	/// App-defined channel annotation flowing into <c>IRequestOrigin.Channel</c> on
	/// binding (e.g., <c>"TwilioIVA"</c>, <c>"WebChat"</c>, <c>"LLMToolCall"</c>).
	/// </summary>
	public string? Channel { get; init; }

	/// <summary>
	/// App-defined correlation reference flowing into <c>IRequestOrigin.Reference</c>
	/// on binding (e.g., call SID, conversation ID).
	/// </summary>
	public string? Reference { get; init; }

	/// <summary>
	/// Additional claims to bind onto the resulting principal. The
	/// <see cref="ISessionTicketPrincipalBinder"/> implementation decides how these
	/// map onto the produced <c>ClaimsPrincipal</c>.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Claims { get; init; }

}
