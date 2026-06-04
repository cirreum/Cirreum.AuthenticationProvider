namespace Cirreum.AuthenticationProvider.SessionTicket;

/// <summary>
/// Persistence abstraction for session tickets. Used by the opaque-variant
/// <see cref="ISessionTicketIssuer"/> and <see cref="ISessionTicketValidator"/>
/// implementations; the JWT variant may use a no-op or minimal store since validation
/// is self-contained.
/// </summary>
/// <remarks>
/// <para>
/// The <c>Cirreum.Authentication.SessionTicket</c> package ships a
/// default in-memory store suitable for development and single-head apps; multi-head
/// production apps plug in a distributed store (Redis, Cosmos DB, etc.) via their
/// persistence track of choice.
/// </para>
/// <para>
/// <see cref="RemoveBySubjectAsync"/> exists to support the connection-terminator flow
/// — when an admin revokes a user, the framework walks all active
/// tickets for that subject and removes them, then aborts any live connections.
/// </para>
/// </remarks>
public interface ISessionStore {

	/// <summary>
	/// Persists <paramref name="ticket"/>. Called by the opaque-variant issuer at
	/// issuance time. Implementations are responsible for honoring
	/// <see cref="SessionTicket.ExpiresAt"/> (e.g., via storage-level TTL).
	/// </summary>
	ValueTask StoreAsync(SessionTicket ticket, CancellationToken cancellationToken);

	/// <summary>
	/// Atomically retrieves <em>and removes</em> the ticket stored under
	/// <paramref name="ticketValue"/> in a single operation, returning it on hit or
	/// <see langword="null"/> when no matching ticket exists or it has expired. This is
	/// the single-use consume primitive: it MUST be race-free so two concurrent
	/// handshakes presenting the same ticket cannot both observe a hit (in-memory:
	/// <c>ConcurrentDictionary.TryRemove(key, out value)</c>; Redis: <c>GETDEL</c>;
	/// Cosmos: a delete that returns the prior document). A non-atomic
	/// retrieve-then-remove is <b>not</b> an acceptable implementation — it reintroduces
	/// the replay window single-use exists to close.
	/// </summary>
	ValueTask<SessionTicket?> ConsumeAsync(string ticketValue, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieves the ticket previously stored under <paramref name="ticketValue"/> without
	/// consuming it; returns <see langword="null"/> when no matching ticket exists or has
	/// expired. Use this for reusable-ticket validators; single-use validators use
	/// <see cref="ConsumeAsync"/> instead.
	/// </summary>
	ValueTask<SessionTicket?> RetrieveAsync(string ticketValue, CancellationToken cancellationToken);

	/// <summary>
	/// Removes the ticket stored under <paramref name="ticketValue"/>. Idempotent;
	/// returns without error when no matching ticket exists.
	/// </summary>
	ValueTask RemoveAsync(string ticketValue, CancellationToken cancellationToken);

	/// <summary>
	/// Removes all tickets bound to <paramref name="subject"/>. Used by the framework's
	/// connection-terminator flow when admin actions revoke a user.
	/// </summary>
	ValueTask RemoveBySubjectAsync(string subject, CancellationToken cancellationToken);

}
