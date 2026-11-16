using static SuperMetroid.Core.Enemies.Polyp.PolypLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>A Polyp's launch and cooldown, chosen by random draws.</summary>
public static class PolypLaunches
{
    /// <summary>Uses the random number divided by two, modulo 8.</summary>
    public static int Cooldown(int random) => Cooldowns[CooldownSelector.Of(random)];

    /// <summary>
    /// The place along the quadratic speed curve a rock's rise starts at: the random number divided by two, modulo 16,
    /// selects one of marks 28 through 43.
    /// </summary>
    public static QuadraticSpeedCurvePlace LaunchPlace(int random) =>
        FirstLaunchPlace + QuadraticSpeedCurvePlace.FromMarks(LaunchSelector.Of(random));

    /// <summary>The selected horizontal velocity.</summary>
    public static SignedEightEight XVelocity(int random) => XVelocities[LaunchSelector.Of(random)];
}
