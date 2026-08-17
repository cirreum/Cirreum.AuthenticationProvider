namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Provides persisted credential revocations to the authentication framework.
/// </summary>
/// <remarks>
/// Applications remain responsible for credential storage and administration. This provider
/// exposes application-managed revocation state so it can be restored when authentication
/// infrastructure initializes.
/// <para>
/// Implementations should retain a revocation only while the credential could otherwise
/// authenticate. For expiring credentials, this generally means until the credential's
/// expiration and any applicable validation grace period have passed.
/// </para>
/// <para>
/// A revocation must not be removed while its credential could still authenticate, as doing
/// so would make the credential valid again.
/// </para>
/// </remarks>
public interface IRevokedCredentialProvider {

	/// <summary>
	/// Gets the credentials that are currently revoked.
	/// </summary>
	/// <remarks>
	/// Supply <see cref="RevokedCredential.ExpiresAt"/> when known so the framework can discard
	/// the revocation once the credential can no longer authenticate. A <see langword="null"/>
	/// expiration causes the revocation to remain for the lifetime of the process.
	/// </remarks>
	/// <param name="cancellationToken">The token used to cancel the operation.</param>
	/// <returns>The currently revoked credentials.</returns>
	IAsyncEnumerable<RevokedCredential> GetRevokedCredentialsAsync(
		CancellationToken cancellationToken = default);

}