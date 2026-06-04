namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Marker interface for selectors that probe <c>Authorization: Bearer</c>. Used by the
/// framework's boot-time validator (in <c>Cirreum.Runtime.Authentication</c>) to
/// enforce the prefix-uniqueness invariant: when multiple Bearer-probing selectors are
/// registered, each must have a unique <see cref="BearerPrefix"/> so the token's leading
/// bytes disambiguate dispatch (<c>ak_prod_…</c> routes to one scheme,
/// <c>st_prod_…</c> to another, etc.).
/// </summary>
/// <remarks>
/// <para>
/// The framework-recommended prefix shape is <c>{scheme}_{env}_{raw}</c> — modeled on
/// Stripe/GitHub/Slack conventions. When prefix is the claim signal, JWT-shape is
/// irrelevant — <c>ak_prod_eyJ...</c> is unambiguously ApiKey because the prefix has
/// already committed dispatch.
/// </para>
/// <para>
/// Selectors that have no configured <see cref="BearerPrefix"/> fall back to JWT-shape
/// disambiguation: claim only when the Bearer value is *not* JWT-shaped. JWT-shaped
/// values are left for the framework-shipped audience-routing selector to handle.
/// </para>
/// </remarks>
public interface IBearerSchemeSelector : ISchemeSelector {

	/// <summary>
	/// The configured per-provider Bearer prefix (e.g. <c>"ak_prod_"</c>); null when
	/// no prefix is configured. Boot-time validation enforces uniqueness across all
	/// registered <see cref="IBearerSchemeSelector"/> instances and requires a prefix
	/// when multiple Bearer-probing selectors are registered.
	/// </summary>
	string? BearerPrefix { get; }

}
