namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Compiled shade-selection schedule for Ceres Ridley's eye fade. The selected
/// colours remain presentation data; this catalog owns only the fixed shade and completion
/// metadata consumed by Ceres Ridley's eye-fade step.
/// </summary>
internal static class CeresRidleyEyeFadeDefinitions
{

    /// <summary>Number of colour-producing steps in the fade.</summary>
    public const int FadeStepCount = 64;
}
