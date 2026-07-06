namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Opt-in marker on an endpoint, controller, or handler indicating that the framework's
/// "anonymous-pending-auth" pattern is allowed — the request may proceed
/// without an authenticated principal up to the boundary defined by the endpoint, with
/// the expectation that a Two-Phase Auth <c>Promote</c> call will upgrade the connection
/// to an authenticated principal before privileged operations occur.
/// </summary>
/// <remarks>
/// <para>
/// Used for flows like cold-IVA (a phone call connects anonymously, the caller is
/// identified mid-conversation), browser AI chat warm sessions (the chat opens before
/// sign-in, the user signs in mid-session), and webhook-driven partner integrations.
/// </para>
/// <para>
/// Without this attribute, the framework's standard authentication pipeline rejects
/// unauthenticated requests with 401. The runtime composition validates at boot time
/// that <c>[AllowPendingAuth]</c>-decorated endpoints are paired with a configured
/// promotion path (typically a <c>connection.Promote(principal)</c> usage — the
/// Two-Phase Auth extension in <c>Cirreum.Runtime.AuthenticationProvider</c>).
/// </para>
/// </remarks>
[AttributeUsage(
	AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate,
	Inherited = true,
	AllowMultiple = false)]
public sealed class AllowPendingAuthAttribute : Attribute {

	/// <summary>
	/// Optional descriptive tag — surfaced in audit and operator diagnostics to
	/// indicate which pending-auth scenario this endpoint belongs to (e.g.,
	/// <c>"cold-iva"</c>, <c>"webhook-handoff"</c>, <c>"chat-warm-session"</c>).
	/// </summary>
	public string? Scenario { get; init; }

}
