namespace Cirreum.AuthenticationProvider;

/// <summary>
/// A revoked credential as reported by an <see cref="IRevokedCredentialProvider"/> — the identifier,
/// and when the credential expires on its own.
/// </summary>
/// <param name="CredentialId">
/// The credential identifier. Scheme-specific in shape (API key id, JWT <c>jti</c>, keypair
/// fingerprint) and must be the exact value the runtime consumer indexes credentials by.
/// </param>
/// <param name="ExpiresAt">
/// When the credential expires by its own terms, or <see langword="null"/> when it does not expire —
/// or when the application cannot determine it.
/// <para>
/// This is the credential's expiry, <b>not</b> the revocation's. A revocation never expires early;
/// carrying the credential's expiry simply lets the in-memory denylist drop an entry once the
/// credential could no longer authenticate anyway, rather than retaining it until the process
/// restarts. <see langword="null"/> means "retain until restart" — safe, because over-retention only
/// costs memory while under-revocation would re-admit a revoked credential.
/// </para>
/// </param>
/// <remarks>
/// A <see langword="readonly record struct" /> rather than a class: this streams through
/// <see cref="IAsyncEnumerable{T}"/> on the boot path, and the populations that motivate carrying
/// expiry at all are the large ones — where an allocation per revocation is the cost worth avoiding.
/// </remarks>
public readonly record struct RevokedCredential(
	string CredentialId,
	DateTimeOffset? ExpiresAt = null);
