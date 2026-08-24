namespace Cirreum.AuthenticationProvider.Tests.Schemes;

using Cirreum.AuthenticationProvider;
using Cirreum.Invocation.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

public class BearerCredentialTests {

	private sealed class DummyHub : Hub {
	}

	private static HttpContext Request(
		string? authorization = null,
		string? accessTokenQuery = null,
		bool connectionEndpoint = false,
		bool hubEndpoint = false) {

		var context = new DefaultHttpContext();

		if (authorization is not null) {
			context.Request.Headers["Authorization"] = authorization;
		}

		if (accessTokenQuery is not null) {
			context.Request.QueryString = new QueryString($"?access_token={accessTokenQuery}");
		}

		var items = new List<object>();
		if (connectionEndpoint) {
			items.Add(InvocationConnectionMetadata.Instance);
		}
		if (hubEndpoint) {
			items.Add(new HubMetadata(typeof(DummyHub)));
		}
		var metadata = new EndpointMetadataCollection(items);

		context.Features.Set<IEndpointFeature>(new Endpoint(_ => Task.CompletedTask, metadata, "test") is var endpoint
			? new TestEndpointFeature(endpoint)
			: null);

		return context;
	}

	private sealed class TestEndpointFeature(Endpoint endpoint) : IEndpointFeature {
		public Endpoint? Endpoint { get; set; } = endpoint;
	}

	// Header ————————————————————————————————————————————————————

	[Fact]
	public void BearerHeader_IsRead() {
		BearerCredential.Read(Request(authorization: "Bearer abc123")).Should().Be("abc123");
	}

	[Fact]
	public void BearerHeader_IsCaseInsensitiveOnTheScheme() {
		BearerCredential.Read(Request(authorization: "bearer abc123")).Should().Be("abc123");
	}

	[Fact]
	public void BearerHeader_PreservesAPrefixedTokenVerbatim() {
		// Prefixes are part of the opaque secret; dispatch matches on them.
		BearerCredential.Read(Request(authorization: "Bearer st_prod_abc123")).Should().Be("st_prod_abc123");
	}

	[Fact]
	public void EmptyBearerHeader_ReadsAsNoCredential() {
		BearerCredential.Read(Request(authorization: "Bearer   ")).Should().BeNull();
	}

	[Fact]
	public void NonBearerHeader_IsNotRead() {
		BearerCredential.Read(Request(authorization: "ApiKey abc123")).Should().BeNull();
	}

	[Fact]
	public void NonBearerHeader_DoesNotFallThroughToTheQuery() {
		var context = Request(authorization: "ApiKey abc123", accessTokenQuery: "from-query", connectionEndpoint: true);

		BearerCredential.Read(context).Should()
			.BeNull("an explicit header of another scheme is the caller's choice");
	}

	// Query, on connection endpoints only ————————————————————————

	[Fact]
	public void QueryToken_IsReadOnAConnectionEndpoint() {
		BearerCredential.Read(Request(accessTokenQuery: "abc123", connectionEndpoint: true))
			.Should().Be("abc123");
	}

	[Fact]
	public void QueryToken_IsIgnoredOnAnOrdinaryEndpoint() {
		BearerCredential.Read(Request(accessTokenQuery: "abc123"))
			.Should().BeNull("a query parameter on an ordinary route carries no authority");
	}

	[Fact]
	public void QueryToken_IsIgnoredWhenNoEndpointWasResolved() {
		var context = new DefaultHttpContext();
		context.Request.QueryString = new QueryString("?access_token=abc123");

		BearerCredential.Read(context).Should().BeNull();
	}

	[Fact]
	public void HeaderWins_OverTheQuery() {
		var context = Request(
			authorization: "Bearer from-header",
			accessTokenQuery: "from-query",
			connectionEndpoint: true);

		BearerCredential.Read(context).Should().Be("from-header");
	}

	[Fact]
	public void EmptyQueryToken_ReadsAsNoCredential() {
		BearerCredential.Read(Request(accessTokenQuery: "", connectionEndpoint: true)).Should().BeNull();
	}

	// Absent ————————————————————————————————————————————————————

	[Fact]
	public void NoCredentialAtAll_ReadsAsNull() {
		BearerCredential.Read(Request(connectionEndpoint: true)).Should().BeNull();
	}

	[Fact]
	public void NullContext_ReadsAsNull() {
		BearerCredential.Read(null).Should().BeNull();
	}

	// Endpoint classification ————————————————————————————————————

	[Fact]
	public void IsConnectionEndpoint_ReflectsTheMetadata() {
		BearerCredential.IsConnectionEndpoint(Request(connectionEndpoint: true)).Should().BeTrue();
		BearerCredential.IsConnectionEndpoint(Request()).Should().BeFalse();
		BearerCredential.IsConnectionEndpoint(null).Should().BeFalse();
	}


	// SignalR hubs are mapped by the application, so they are recognized by SignalR's
	// own metadata rather than by anything the framework stamps.

	[Fact]
	public void QueryToken_IsReadOnASignalRHubEndpoint() {
		BearerCredential.Read(Request(accessTokenQuery: "abc123", hubEndpoint: true))
			.Should().Be("abc123");
	}

	[Fact]
	public void IsConnectionEndpoint_RecognizesASignalRHub() {
		BearerCredential.IsConnectionEndpoint(Request(hubEndpoint: true)).Should().BeTrue();
	}

	[Fact]
	public void HeaderStillWins_OnAHubEndpoint() {
		var context = Request(authorization: "Bearer from-header", accessTokenQuery: "from-query", hubEndpoint: true);

		BearerCredential.Read(context).Should().Be("from-header");
	}

}
