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
