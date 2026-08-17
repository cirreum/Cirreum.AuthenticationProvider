namespace Cirreum.AuthenticationProvider.Configuration;

using Cirreum.Security;

/// <summary>
/// Declares which side owns a scheme's callers — the identity provider, or the application's
/// own store — for each class of attribute.
/// </summary>
/// <remarks>
/// <para>
/// Both properties default to <see cref="ClaimAuthority.Unspecified"/>, and an instance that
/// omits the block entirely keeps existing behavior: roles resolve from the application store
/// when a resolver is registered for the scheme, and from the identity provider otherwise.
/// Declaring is opt-in; nothing changes for an application that never does.
/// </para>
/// <para>
/// Each is a <b>precedence</b> rule, not an exclusivity one. The declared side wins where both
/// supply a value; neither is prevented from supplying one. That matters where availability
/// varies per user on a single scheme — a federated account may arrive carrying full profile
/// claims while a local account on the same scheme arrives thin.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // appsettings.json — an application that owns its users' profiles and roles
/// //   "Oidc": { "Instances": { "descope": {
/// //       "Authority": "https://…", "Audience": "…",
/// //       "ClaimAuthority": { "Profile": "ApplicationStore", "Roles": "ApplicationStore" }
/// //   } } }
/// </code>
/// </example>
public sealed class ClaimAuthoritySettings {

	/// <summary>
	/// Who owns the caller's identity and profile claims — display name, given and family name,
	/// nickname.
	/// </summary>
	/// <remarks>
	/// <see cref="ClaimAuthority.ApplicationStore"/> means claims the application minted into the
	/// token win over the identity provider's native ones. There is no per-request lookup for
	/// profile: the application's values travel in the token.
	/// </remarks>
	public ClaimAuthority Profile { get; set; } = ClaimAuthority.Unspecified;

	/// <summary>
	/// Who owns the caller's roles.
	/// </summary>
	/// <remarks>
	/// <see cref="ClaimAuthority.ApplicationStore"/> means a live per-request resolve, so a
	/// revoked role takes effect immediately rather than at the caller's next sign-in — the
	/// reason this is worth declaring rather than reading off a token whose values were captured
	/// once and ride the whole refresh chain.
	/// </remarks>
	public ClaimAuthority Roles { get; set; } = ClaimAuthority.Unspecified;

}
