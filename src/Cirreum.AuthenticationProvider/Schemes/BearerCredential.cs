namespace Cirreum.AuthenticationProvider;

using Cirreum.Invocation.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Reads the inbound bearer credential. The single reader used by both scheme selectors,
/// which decide which scheme handles a request, and scheme handlers, which validate it.
/// </summary>
/// <remarks>
/// <para>
/// The credential is taken from <c>Authorization: Bearer</c>, and otherwise from the
/// <see cref="QueryParameterName"/> query parameter when the request targets an endpoint
/// carrying <see cref="InvocationConnectionMetadata"/>. A browser cannot set headers on a
/// WebSocket upgrade, so a query-carried token is the only credential such a client can
/// present; SignalR's own clients follow the same convention.
/// </para>
/// <para>
/// The header always wins, and the query is never consulted for an endpoint that is not a
/// connection upgrade, so a query parameter on an ordinary route carries no authority.
/// </para>
/// </remarks>
public static class BearerCredential {

	/// <summary>The query parameter carrying the credential where a header cannot be sent.</summary>
	public const string QueryParameterName = "access_token";

	private const string BearerPrefixToken = "Bearer ";

	/// <summary>
	/// Read the bearer credential from <paramref name="context"/>, or <see langword="null"/>
	/// when the request carries none that this reader will honor.
	/// </summary>
	/// <param name="context">The inbound request.</param>
	public static string? Read(HttpContext? context) {
		if (context is null) {
			return null;
		}

		var header = context.Request.Headers[HeaderNames.Authorization].ToString();
		if (!string.IsNullOrEmpty(header)) {

			if (!header.StartsWith(BearerPrefixToken, StringComparison.OrdinalIgnoreCase)) {
				// An Authorization header of another scheme is a deliberate choice by the
				// caller; falling through to the query would override it.
				return null;
			}

			var headerToken = header[BearerPrefixToken.Length..].Trim();
			return string.IsNullOrEmpty(headerToken) ? null : headerToken;
		}

		if (!IsConnectionEndpoint(context)) {
			return null;
		}

		var queryToken = context.Request.Query[QueryParameterName].ToString();
		return string.IsNullOrEmpty(queryToken) ? null : queryToken;
	}

	/// <summary>
	/// Whether the request targets an endpoint that upgrades to a long-lived connection.
	/// </summary>
	/// <remarks>
	/// Recognized by SignalR's own hub metadata, or by <see cref="InvocationConnectionMetadata"/>,
	/// which the framework stamps on the connection endpoints it maps and which an application
	/// may stamp on a transport of its own.
	/// </remarks>
	/// <param name="context">The inbound request.</param>
	public static bool IsConnectionEndpoint(HttpContext? context) {
		var metadata = context?.GetEndpoint()?.Metadata;
		if (metadata is null) {
			return false;
		}

		return metadata.GetMetadata<HubMetadata>() is not null
			|| metadata.GetMetadata<InvocationConnectionMetadata>() is not null;
	}

}
