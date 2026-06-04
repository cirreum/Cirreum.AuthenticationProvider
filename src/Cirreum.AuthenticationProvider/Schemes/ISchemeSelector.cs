namespace Cirreum.AuthenticationProvider;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Per-request scheme selector. The dynamic forward scheme resolver in
/// <c>Cirreum.Runtime.Authentication</c> iterates registered selectors sorted by
/// <see cref="Priority"/> ascending and dispatches to the first claimant — open/closed
/// composition without hardcoded resolver chains.
/// </summary>
/// <remarks>
/// <para>
/// Each scheme package registers its own
/// <see cref="ISchemeSelector"/> implementation(s) in DI. The resolver builds the iteration set
/// from DI — adding a new scheme means registering a new selector; no resolver edits.
/// </para>
/// <para>
/// Selectors are stateless and registered as singletons. They observe the
/// <see cref="HttpContext"/> (headers, path, cookies, claims pre-populated by other
/// middleware) and return a tuple indicating match + scheme name. Implementations MUST
/// keep <see cref="TrySelect"/> cheap — no JWT signature verification, no DB lookup,
/// no async work.
/// </para>
/// <para>
/// Use the conventional priority values in <see cref="SchemeSelectorPriority"/>;
/// apps may use any int value.
/// </para>
/// </remarks>
public interface ISchemeSelector {

	/// <summary>
	/// Dispatch order; lower runs first. Use <see cref="SchemeSelectorPriority"/>
	/// constants for conventional slots; apps may use any int value.
	/// </summary>
	int Priority { get; }

	/// <summary>
	/// Cheap probe — does this selector claim the request, and which scheme name does
	/// it resolve to? Implementations MUST NOT do expensive work (no JWT signature
	/// verification, no DB lookup) — that's the handler's job after dispatch. May parse
	/// JWT structure for shape/audience detection; that's bounded and cheap.
	/// </summary>
	/// <param name="context">The current HTTP context. Selectors that target non-HTTP
	/// transports (e.g., gRPC, SignalR) inspect equivalent state via the dispatch
	/// adapter.</param>
	/// <returns>A tuple: <c>Matches</c> indicates the selector applies; <c>SchemeName</c>
	/// names the registered authentication scheme to dispatch to (<see langword="null"/>
	/// when <c>Matches</c> is false).</returns>
	(bool Matches, string? SchemeName) TrySelect(HttpContext context);

}
