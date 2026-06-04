namespace Cirreum.AuthenticationProvider;

using System.Collections.Concurrent;

/// <summary>
/// Audience → scheme registry consumed by the framework-shipped audience-routing
/// selector to dispatch a JWT bearer request to the appropriate ASP.NET authentication
/// scheme based on the token's <c>aud</c> claim.
/// </summary>
/// <remarks>
/// <para>
/// Audience-based registrars populate this map by calling <see cref="Register"/>
/// from their <c>RegisterScheme</c> implementation. The framework-shipped
/// <c>JwtAudienceSchemeSelector</c> (in <c>Cirreum.Runtime.Authentication</c>)
/// consumes the populated map at request time.
/// </para>
/// <para>
/// This is the focused replacement for the legacy <c>AuthorizationSchemeRegistry</c> —
/// only the audience-mapping side survives, because header → scheme dispatch is now
/// handled directly by each per-scheme header-based selector probing its own configured
/// headers.
/// </para>
/// </remarks>
public interface IAudienceSchemeMap {

	/// <summary>
	/// Register an audience → scheme mapping. Called by audience-based registrars
	/// at startup. Throws <see cref="InvalidOperationException"/> when the same
	/// <paramref name="audience"/> is already registered with a different scheme name
	/// (a real configuration conflict — two audience-based registrars claiming the
	/// same <c>aud</c> for different schemes). Idempotent re-registration with the
	/// same scheme name is allowed.
	/// </summary>
	void Register(string audience, string scheme);

	/// <summary>Look up the scheme name for a given audience; returns null when unmapped.</summary>
	string? GetSchemeForAudience(string audience);

	/// <summary>All registered mappings, for diagnostics and boot-time validation.</summary>
	IReadOnlyDictionary<string, string> Mappings { get; }

}

/// <summary>
/// Default <see cref="IAudienceSchemeMap"/> implementation. Case-insensitive registry
/// backed by a <see cref="ConcurrentDictionary{TKey, TValue}"/> so registration from
/// multiple registrars during startup is safe.
/// </summary>
public sealed class DefaultAudienceSchemeMap : IAudienceSchemeMap {

	private readonly ConcurrentDictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc />
	public void Register(string audience, string scheme) {
		if (string.IsNullOrWhiteSpace(audience)) {
			throw new ArgumentException("Audience must be non-empty.", nameof(audience));
		}

		if (string.IsNullOrWhiteSpace(scheme)) {
			throw new ArgumentException("Scheme must be non-empty.", nameof(scheme));
		}

		_map.AddOrUpdate(
			audience,
			scheme,
			(_, existing) => existing.Equals(scheme, StringComparison.Ordinal)
				? existing
				: throw new InvalidOperationException(
					$"Audience '{audience}' is already registered to scheme '{existing}'; " +
					$"cannot re-register to scheme '{scheme}'. Two audience-based registrars " +
					$"are claiming the same audience for different schemes — fix the configuration."));
	}

	/// <inheritdoc />
	public string? GetSchemeForAudience(string audience) =>
		_map.TryGetValue(audience, out var scheme) ? scheme : null;

	/// <inheritdoc />
	public IReadOnlyDictionary<string, string> Mappings => _map;

}
