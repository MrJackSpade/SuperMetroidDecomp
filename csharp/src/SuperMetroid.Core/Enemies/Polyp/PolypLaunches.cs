using static SuperMetroid.Core.Enemies.Polyp.PolypLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Where a Polyp's rock starts its rise, from a random draw.</summary>
internal static partial class PolypLaunches
{
    /// <summary>
    /// The place along the quadratic speed curve a rock's rise starts at: <see cref="PolypLaunchDefinitions.FirstLaunchPlace"/>
    /// on by what <see cref="EnemyRandomRolls.PolypLaunchPlace"/> selects.
    /// </summary>
    internal static QuadraticSpeedCurvePlace LaunchPlace(int random) =>
        FirstLaunchPlace + QuadraticSpeedCurvePlace.FromMarks(EnemyRandomRolls.PolypLaunchPlace.Of(random));
}
