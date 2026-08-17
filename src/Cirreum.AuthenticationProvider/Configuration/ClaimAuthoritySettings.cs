namespace Cirreum.AuthenticationProvider.Configuration;

using Cirreum.Security;

/// <summary>
/// Configures which authority takes precedence for claims associated with an authentication scheme.
/// </summary>
/// <remarks>
/// <para>
/// Both properties default to <see cref="ClaimAuthority.Unspecified"/>, preserving the framework's
/// default behavior.
/// </para>
/// <para>
/// Authority represents precedence, not exclusivity. When both the identity provider and application
/// store supply a value, the configured authority takes precedence.
/// </para>
/// </remarks>
public sealed class ClaimAuthoritySettings {

	/// <summary>
	/// Gets or sets the authority for identity and profile claims, such as display name,
	/// given name, family name, and nickname.
	/// </summary>
	/// <remarks>
	/// When both sources provide a claim, the configured authority's value takes precedence.
	/// </remarks>
	public ClaimAuthority Profile { get; set; } = ClaimAuthority.Unspecified;

	/// <summary>
	/// Gets or sets the authority for the caller's roles.
	/// </summary>
	/// <remarks>
	/// Use <see cref="ClaimAuthority.ApplicationStore"/> when application-managed roles are
	/// authoritative, or <see cref="ClaimAuthority.IdentityProvider"/> when roles are managed
	/// by the identity provider.
	/// </remarks>
	public ClaimAuthority Roles { get; set; } = ClaimAuthority.Unspecified;

}