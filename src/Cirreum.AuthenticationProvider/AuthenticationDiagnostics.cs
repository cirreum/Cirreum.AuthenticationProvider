namespace Cirreum.AuthenticationProvider;

/// <summary>
/// Diagnostic constants for the Authentication pillar telemetry. The
/// <see cref="System.Diagnostics.ActivitySource"/> and
/// <see cref="System.Diagnostics.Metrics.Meter"/> are created in the runtime
/// composition; this class exposes the shared name so both agree on the identifier.
/// </summary>
public static class AuthenticationDiagnostics {

	/// <summary>
	/// Diagnostic name for the Authentication pillar's <c>ActivitySource</c> and
	/// <c>Meter</c>. Referenced by runtime composition to subscribe to telemetry.
	/// </summary>
	public const string DiagnosticName = "Cirreum.AuthenticationProvider";

}
