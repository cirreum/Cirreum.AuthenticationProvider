namespace Cirreum.AuthenticationProvider.SignedRequest;

/// <summary>
/// The neutral request components a signature base is built from (ADR-0021 §8). This type carries no
/// ASP.NET (<c>HttpRequest</c>) or <c>HttpClient</c> (<c>HttpRequestMessage</c>) dependency — the
/// per-side adapters in the server and client packages project their host request type onto it, so
/// signer and verifier construct the byte-identical signature base from the same builder.
/// </summary>
public sealed class SignatureBaseComponents {

	/// <summary>The HTTP method, uppercased (the <c>@method</c> value).</summary>
	public required string Method { get; init; }

	/// <summary>The absolute request path (the <c>@path</c> value), e.g. <c>/api/orders</c>.</summary>
	public required string Path { get; init; }

	/// <summary>
	/// The query string for the <c>@query</c> value, including the leading <c>?</c>. Per RFC 9421 this
	/// is <c>?</c> when there is no query.
	/// </summary>
	public string Query { get; init; } = "?";

	/// <summary>
	/// The request authority (host[:port]), lowercased, for the opt-in <c>@authority</c> value; or
	/// <see langword="null"/> when authority is not available. Required only if <c>@authority</c> is covered.
	/// </summary>
	public string? Authority { get; init; }

	/// <summary>
	/// The covered HTTP field values, keyed by lowercased field name (e.g. <c>content-digest</c>).
	/// Used for any non-derived covered component.
	/// </summary>
	public IReadOnlyDictionary<string, string> Fields { get; init; } =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

}
