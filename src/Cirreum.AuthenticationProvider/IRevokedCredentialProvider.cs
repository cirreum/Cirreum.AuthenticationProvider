namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Apps implement this contract to expose "what credentials have been revoked?" to the
/// framework. Consumed by the cache-invalidator handler at bootstrap (to hydrate the
/// in-memory denylist after head restart) and by ApiKey / SignedRequest resolver chains
/// (to reject credentials known to be revoked even if their cache entries haven't been
/// invalidated yet).
/// </summary>
/// <remarks>
/// <para>
/// Cirreum does not own credential admin (no admin schema, no
/// admin UI, no parallel revocation table). Apps administer credentials in their own
/// data stores. This provider lets apps expose their revocation state for framework
/// consumption without coupling framework infrastructure to app-specific admin
/// schemas.
/// </para>
/// <para>
/// Pairs with <c>Events.CredentialRevoked</c> auth events (from
/// <c>Cirreum.Kernel</c>): apps publish the event on revocation; the
/// framework consumes via handlers to update in-memory state; this provider
/// hydrates that state at boot.
/// </para>
/// <para>
/// <b>Keep the persisted revoked set bounded.</b> Everything this provider yields is held in
/// memory for the lifetime of the process, and the in-memory denylist is capacity-bounded — on
/// saturation it <em>fails authentication closed</em> rather than silently forgetting a
/// revocation. An unbounded revoked set therefore degrades into refused authentication, not into
/// stale state.
/// </para>
/// <para>
/// The safe pruning rule is the same one the denylist applies to itself: remove a revocation once
/// the credential could no longer authenticate anyway — past its own expiry plus any validation
/// grace window, or once it has been deleted or rotated out of issuance entirely. <b>Never prune a
/// live, non-expired credential's revocation</b>; doing so re-admits it. A non-expiring credential's
/// revocation stays until that credential leaves issuance.
/// </para>
/// <para>
/// No "un-revoke" signal is needed. The set re-hydrates on restart, and an entry self-evicts once
/// the credential's own expiry passes — for entries hydrated here as well as for those created by a
/// live <c>CredentialRevoked</c> event, provided <see cref="RevokedCredential.ExpiresAt"/> is
/// supplied. Omit it and the entry is retained until restart, which is safe but keeps memory that
/// pruning at the source would release.
/// </para>
/// </remarks>
public interface IRevokedCredentialProvider {

	/// <summary>
	/// Returns every currently revoked credential — its identifier, and when it expires on its own.
	/// </summary>
	/// <remarks>
	/// Supplying <see cref="RevokedCredential.ExpiresAt"/> lets a hydrated revocation self-evict once
	/// the credential could no longer authenticate anyway, matching what a live
	/// <c>CredentialRevoked</c> event already achieves. Leaving it <see langword="null"/> is safe and
	/// means "retain until restart" — the trade is memory, never a re-admitted credential.
	/// </remarks>
	/// <param name="cancellationToken">Cancellation token.</param>
	IAsyncEnumerable<RevokedCredential> GetRevokedCredentialsAsync(
		CancellationToken cancellationToken = default);

}
