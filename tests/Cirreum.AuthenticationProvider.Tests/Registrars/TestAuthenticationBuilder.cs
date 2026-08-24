namespace Cirreum.AuthenticationProvider.Tests.Registrars;

using Cirreum.AuthenticationProvider;
using Cirreum.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Test double for <see cref="IAuthenticationBuilder"/>. The shipping implementation lives in
/// the runtime layer, which sits above this package, so the tests supply their own.
/// </summary>
internal sealed class TestAuthenticationBuilder : IAuthenticationBuilder {

	internal TestAuthenticationBuilder(IServiceCollection services, IConfiguration? configuration = null) {
		this.Services = services;
		this.Configuration = configuration ?? new ConfigurationBuilder().Build();
		this.AuthBuilder = new AuthenticationBuilder(services);
	}

	public IServiceCollection Services { get; }

	public AuthenticationBuilder AuthBuilder { get; }

	public IConfiguration Configuration { get; }

	/// <summary>Schemes declared through this builder, in declaration order.</summary>
	internal List<(string Scheme, SubjectKind SubjectKind, ClaimAuthority Profile, ClaimAuthority Roles)> Declared { get; } = [];

	public IAuthenticationBuilder DeclareScheme(
		string scheme,
		SubjectKind subjectKind,
		ClaimAuthority profile = ClaimAuthority.Unspecified,
		ClaimAuthority roles = ClaimAuthority.Unspecified) {

		this.Declared.Add((scheme, subjectKind, profile, roles));
		return this;
	}

	public IAuthenticationBuilder AddScheme<TOptions, THandler>(
		string scheme,
		SubjectKind subjectKind,
		ClaimAuthority profile = ClaimAuthority.Unspecified,
		ClaimAuthority roles = ClaimAuthority.Unspecified,
		Action<TOptions>? configureOptions = null)
		where TOptions : AuthenticationSchemeOptions, new()
		where THandler : AuthenticationHandler<TOptions> {

		this.Declared.Add((scheme, subjectKind, profile, roles));
		this.AuthBuilder.AddScheme<TOptions, THandler>(scheme, configureOptions);
		return this;
	}

}
