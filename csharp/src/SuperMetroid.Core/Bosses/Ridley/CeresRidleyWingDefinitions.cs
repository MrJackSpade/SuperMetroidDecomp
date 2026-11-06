namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How fast Ceres Ridley's wings beat for the speed he flies at.</summary>
internal static class CeresRidleyWingDefinitions
{
    /// <summary>
    /// The frames each update takes off the wing animation timer, slowest flight first; a descending or level flight
    /// halves the selected step.
    /// </summary>
    internal static readonly int[] AnimationTimerSteps = [12, 14, 16, 18, 28, 32, 40, 48];
}
