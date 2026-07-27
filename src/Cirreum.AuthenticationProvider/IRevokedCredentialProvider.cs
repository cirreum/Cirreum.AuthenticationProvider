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
/// No "un-revoke" signal is needed. The set re-hydrates on restart, and entries created by a live
/// <c>CredentialRevoked</c> event self-evict on the credential's own expiry. Note that entries
/// hydrated <em>here</em> cannot: this contract yields ids without expiry, so a boot-hydrated
/// revocation is retained until the next restart. That is deliberate — over-retention is safe,
/// under-revocation is not — and it is why the set wants pruning at the source.
/// </para>
/// </remarks>
public interface IRevokedCredentialProvider {

	/// <summary>
	/// Returns all credential IDs currently revoked. The shape of the credential ID
	/// is scheme-specific (API key id, JWT jti, keypair fingerprint) — the runtime
	/// consumer correlates against its own credential indexes.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	IAsyncEnumerable<string> GetRevokedCredentialIdsAsync(
		CancellationToken cancellationToken = default);

}
