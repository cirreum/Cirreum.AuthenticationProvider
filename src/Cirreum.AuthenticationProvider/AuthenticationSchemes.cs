namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Scheme names for the framework-shipped schemes the umbrella composition itself
/// registers (implemented in <c>Cirreum.Runtime.AuthenticationProvider</c>, composed by
/// <c>Cirreum.Runtime.Authentication</c>).
/// </summary>
/// <remarks>
/// This class holds only the schemes the framework spine owns. Provider scheme names are
/// owned by the packages that register them — pin a provider scheme from that package's
/// own constants (<c>ApiKeySchemes</c>, <c>SessionTicketSchemes</c>,
/// <c>SignedRequestSchemes</c>), never from a name restated here.
/// </remarks>
public static class AuthenticationSchemes {

	/// <summary>
	/// Dynamic forward scheme — ASP.NET <c>PolicyScheme</c> that iterates registered
	/// <see cref="ISchemeSelector"/> instances by <see cref="ISchemeSelector.Priority"/>
	/// ascending and forwards to the first claimant's scheme. The configured default
	/// scheme for Cirreum-hosted apps.
	/// </summary>
	public const string Dynamic = "Cirreum.Dynamic";

	/// <summary>
	/// Ambiguous-request scheme — claimed by the framework-shipped conflict sentinel
	/// (<see cref="SchemeSelectorPriority.Conflict"/>) when a request carries distinct
	/// credential-carriers (for example, a custom header AND <c>Authorization: Bearer</c>).
	/// The handler fails closed with 401.
	/// </summary>
	public const string Ambiguous = "Cirreum.Ambiguous";

	/// <summary>
	/// Anonymous fallback scheme — claimed when no other selector matches; the handler
	/// returns <see cref="Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult"/>
	/// so <c>[AllowAnonymous]</c> endpoints continue to work.
	/// </summary>
	public const string Anonymous = "Cirreum.Anonymous";

}
