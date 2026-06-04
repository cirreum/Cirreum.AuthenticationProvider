namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Where a scheme reads its credential from on the inbound request. Drives the
/// scheme implementation's reading logic — Bearer vs custom-header for ApiKey,
/// RFC 9421-aligned signature headers for SignedRequest, cookie vs header for
/// SessionTicket, etc.
/// </summary>
/// <remarks>
/// <para>
/// The transport is part of the scheme's metadata and is consumed
/// by the dynamic forward resolver to disambiguate when multiple schemes could
/// match a request. Modeled as <see cref="FlagsAttribute"/> so credentials can opt
/// into multiple transports (e.g., ApiKey accepting both Bearer and a custom
/// header on a transition window).
/// </para>
/// </remarks>
[Flags]
public enum CredentialTransport {

	/// <summary>
	/// Sentinel — no transport. Configurations with this value are invalid; schemes
	/// reject them at registration time.
	/// </summary>
	None = 0,

	/// <summary>
	/// HTTP <c>Authorization: Bearer &lt;token&gt;</c> header (RFC 6750).
	/// </summary>
	BearerAuthorizationHeader = 1,

	/// <summary>
	/// Custom HTTP header (e.g., <c>X-Api-Key</c>, <c>X-Cirreum-Signature</c>). The
	/// scheme registration carries the header name.
	/// </summary>
	CustomHeader = 1 << 1,

	/// <summary>
	/// Multiple HTTP request headers compose the credential (e.g., RFC 9421 HTTP
	/// Message Signatures: <c>Signature</c> + <c>Signature-Input</c> + content
	/// digest). The scheme reads all of them per its specification.
	/// </summary>
	HeaderComposition = 1 << 2,

	/// <summary>
	/// Transport-level credential (mTLS client certificate, SSL_CLIENT_S_DN). Read
	/// from the connection context rather than the HTTP request body/headers.
	/// </summary>
	TransportLayer = 1 << 3,

	/// <summary>
	/// Query string parameter. Reserved for narrow use cases (e.g., signed download
	/// URLs); discouraged for primary authentication due to log exposure.
	/// </summary>
	QueryString = 1 << 4,

	/// <summary>
	/// HTTP cookie (typically session tickets bound to a browser).
	/// </summary>
	Cookie = 1 << 5

}