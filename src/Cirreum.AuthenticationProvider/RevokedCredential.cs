namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Describes a credential that has been revoked.
/// </summary>
/// <param name="CredentialId">
/// The exact credential identifier used by the authentication scheme for lookup and revocation matching.
/// </param>
/// <param name="ExpiresAt">
/// The credential's expiration time, or <see langword="null"/> if the credential does not expire
/// or its expiration is unknown.
/// </param>
/// <remarks>
/// <see cref="ExpiresAt"/> represents the credential's own expiration, not the expiration of the
/// revocation. When supplied, it allows the framework to discard the revocation after the
/// credential can no longer authenticate.
/// </remarks>
public readonly record struct RevokedCredential(
	string CredentialId,
	DateTimeOffset? ExpiresAt = null);