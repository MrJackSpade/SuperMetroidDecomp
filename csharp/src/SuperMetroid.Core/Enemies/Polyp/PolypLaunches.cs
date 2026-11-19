using static SuperMetroid.Core.Enemies.Polyp.PolypLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Where a Polyp's rock starts its rise, from a random draw.</summary>
internal static class PolypLaunches
{
    /// <summary>
    /// The place along the quadratic speed curve a rock's rise starts at,
    /// selects one of marks 28 through 43.
    /// </summary>
    internal static QuadraticSpeedCurvePlace LaunchPlace(int random) =>
        FirstLaunchPlace + QuadraticSpeedCurvePlace.FromMarks(LaunchSelector.Of(random));
}
