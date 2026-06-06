namespace Cirreum.AuthenticationProvider.SignedRequest;

using System.Security.Cryptography;

/// <summary>
/// Computes and verifies the RFC 9530 <c>Content-Digest</c> field (SHA-256), used to bind the request
/// body into the signature regardless of method (resolves the unsigned-DELETE-body class). Pure.
/// </summary>
public static class ContentDigest {

	/// <summary>The SHA-256 digest algorithm key in a Content-Digest dictionary field.</summary>
	public const string Sha256Key = "sha-256";

	/// <summary>
	/// Computes the <c>Content-Digest</c> field value for a body: <c>sha-256=:&lt;base64&gt;:</c>.
	/// </summary>
	public static string Compute(ReadOnlySpan<byte> body) {
		Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
		SHA256.HashData(body, hash);
		return $"{Sha256Key}=:{Convert.ToBase64String(hash)}:";
	}

	/// <summary>
	/// Verifies that a received <c>Content-Digest</c> field value contains a SHA-256 digest matching
	/// <paramref name="body"/>, in constant time. Returns <see langword="false"/> for a malformed value
	/// or a missing SHA-256 entry.
	/// </summary>
	public static bool Verify(string? headerValue, ReadOnlySpan<byte> body) {
		if (string.IsNullOrWhiteSpace(headerValue)) {
			return false;
		}

		// Content-Digest is an RFC 8941 dictionary; base64 contains no comma, so member-splitting is safe.
		foreach (var member in headerValue.Split(',')) {
			var entry = member.Trim();
			var eq = entry.IndexOf('=');
			if (eq <= 0) {
				continue;
			}

			var key = entry[..eq].Trim();
			if (!key.Equals(Sha256Key, StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			var value = entry[(eq + 1)..].Trim();
			if (value.Length < 2 || value[0] != ':' || value[^1] != ':') {
				return false;
			}

			if (!TryFromBase64(value[1..^1], out var expected) || expected.Length != SHA256.HashSizeInBytes) {
				return false;
			}

			Span<byte> actual = stackalloc byte[SHA256.HashSizeInBytes];
			SHA256.HashData(body, actual);
			return CryptographicOperations.FixedTimeEquals(actual, expected);
		}

		return false;
	}

	private static bool TryFromBase64(string value, out byte[] bytes) {
		if (value.Length is 0 or > 256) {
			bytes = [];
			return false;
		}

		var buffer = new byte[((value.Length + 3) / 4) * 3];
		if (Convert.TryFromBase64String(value, buffer, out var written)) {
			bytes = buffer[..written];
			return true;
		}

		bytes = [];
		return false;
	}

}
