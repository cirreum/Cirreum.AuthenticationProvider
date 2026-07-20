namespace Cirreum.AuthenticationProvider;

/// <summary>
/// An immutable <c>audience → scheme</c> routing contribution, registered into the
/// service collection by audience-based authentication registrars — one per enabled
/// provider instance.
/// </summary>
/// <remarks>
/// <para>
/// Audience-based schemes share a single credential carrier (<c>Authorization: Bearer</c>),
/// so per-request dispatch is data-driven: the framework-shipped
/// <c>JwtAudienceSchemeSelector</c> (in <c>Cirreum.Runtime.Authentication</c>) resolves
/// <see cref="IEnumerable{T}"/> of this record from the container and builds its lookup
/// index once, at construction. Contributions are additive service registrations —
/// registration order never affects routing.
/// </para>
/// <para>
/// The complete set is validated at composition close by the umbrella package: one
/// audience claimed by two different schemes fails the host with every collision
/// reported. Applications integrating an IdP outside a Cirreum provider family can
/// contribute a mapping directly:
/// <c>services.AddSingleton(new AudienceSchemeRegistration(audience, scheme, "MyApp"))</c>.
/// </para>
/// </remarks>
/// <param name="Audience">The token <c>aud</c> claim value this entry routes. Compared case-insensitively.</param>
/// <param name="Scheme">The ASP.NET Core authentication scheme name that owns the audience.</param>
/// <param name="ProviderName">The contributing provider's name (e.g. <c>"Entra"</c>, <c>"Oidc"</c>) — used in diagnostics and conflict reporting.</param>
public sealed record AudienceSchemeRegistration(
	string Audience,
	string Scheme,
	string ProviderName);
