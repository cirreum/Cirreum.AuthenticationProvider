namespace Cirreum.AuthenticationProvider.Coordination;

/// <summary>
/// Single-use claim primitive for replay / nonce protection — consumed by the SignedRequest scheme's
/// strict-nonce posture (ADR-0021). Backed by any store with an atomic conditional-insert (set-if-absent):
/// the built-in single-instance <see cref="InMemoryReplayGuard"/>, or a distributed adapter
/// (e.g. <c>Cirreum.Authentication.Coordination.Redis</c>) selected via <c>AddCoordination</c>.
/// </summary>
/// <remarks>
/// Split from <see cref="IRequestThrottle"/> because the two capabilities are asymmetric: atomic
/// set-if-absent is near-universal across stores, whereas an atomic windowed counter is the discriminator.
/// A backend can implement one without the other.
/// </remarks>
public interface IReplayGuard {

	/// <summary>
	/// Atomically claims <paramref name="token"/> for the window <paramref name="ttl"/>. Returns
	/// <see langword="true"/> exactly once per token while the claim is live (the first caller) and
	/// <see langword="false"/> for every subsequent caller until the claim expires — so a replayed
	/// credential is rejected. Fail-closed: a non-positive <paramref name="ttl"/> is rejected so a token
	/// can never be claimed into an immortal, never-expiring slot.
	/// </summary>
	/// <param name="token">The opaque single-use value (e.g. a nonce or signature digest). Must be non-empty.</param>
	/// <param name="ttl">How long the claim is held; must be positive.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns><see langword="true"/> if this call claimed the token; <see langword="false"/> if it was already claimed and still live.</returns>
	ValueTask<bool> TryClaimAsync(string token, TimeSpan ttl, CancellationToken cancellationToken = default);

}
