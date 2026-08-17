namespace Cirreum.AuthenticationProvider;

using Cirreum.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides the services, authentication builder, and configuration used to register
/// authentication schemes with Cirreum.
/// </summary>
/// <remarks>
/// Use <see cref="AddScheme{TOptions, THandler}"/> to register and declare a scheme together,
/// or <see cref="DeclareScheme"/> for schemes registered through other authentication extensions.
/// </remarks>
public interface IAuthenticationBuilder {

	/// <summary>
	/// Gets the service collection used to register authentication-related services.
	/// </summary>
	IServiceCollection Services { get; }

	/// <summary>
	/// Gets the underlying ASP.NET Core authentication builder.
	/// </summary>
	/// <remarks>
	/// Schemes registered directly through this builder are not automatically declared to Cirreum.
	/// Use <see cref="AddScheme{TOptions, THandler}"/> or call <see cref="DeclareScheme"/>
	/// after registering the scheme.
	/// </remarks>
	AuthenticationBuilder AuthBuilder { get; }

	/// <summary>
	/// Gets the application configuration used when configuring authentication providers.
	/// </summary>
	IConfiguration Configuration { get; }

	/// <summary>
	/// Declares an authentication scheme and its claim authorities to Cirreum.
	/// </summary>
	/// <param name="scheme">The authentication scheme name.</param>
	/// <param name="subjectKind">The kind of subject authenticated by the scheme.</param>
	/// <param name="profile">The authority for profile claims.</param>
	/// <param name="roles">The authority for role claims.</param>
	/// <returns>The builder, for chaining.</returns>
	/// <remarks>
	/// Use this method for schemes registered through other ASP.NET Core authentication
	/// extensions, such as <c>AddJwtBearer</c>, <c>AddOpenIdConnect</c>, or <c>AddCookie</c>.
	/// </remarks>
	IAuthenticationBuilder DeclareScheme(
		string scheme,
		SubjectKind subjectKind,
		ClaimAuthority profile = ClaimAuthority.Unspecified,
		ClaimAuthority roles = ClaimAuthority.Unspecified);

	/// <summary>
	/// Registers an authentication scheme and declares its claim authorities to Cirreum.
	/// </summary>
	/// <typeparam name="TOptions">The authentication scheme options type.</typeparam>
	/// <typeparam name="THandler">The authentication handler type.</typeparam>
	/// <param name="scheme">The authentication scheme name.</param>
	/// <param name="subjectKind">The kind of subject authenticated by the scheme.</param>
	/// <param name="profile">The authority for profile claims.</param>
	/// <param name="roles">The authority for role claims.</param>
	/// <param name="configureOptions">An optional callback used to configure the scheme.</param>
	/// <returns>The builder, for chaining.</returns>
	IAuthenticationBuilder AddScheme<TOptions, THandler>(
		string scheme,
		SubjectKind subjectKind,
		ClaimAuthority profile = ClaimAuthority.Unspecified,
		ClaimAuthority roles = ClaimAuthority.Unspecified,
		Action<TOptions>? configureOptions = null)
		where TOptions : AuthenticationSchemeOptions, new()
		where THandler : AuthenticationHandler<TOptions>;

}