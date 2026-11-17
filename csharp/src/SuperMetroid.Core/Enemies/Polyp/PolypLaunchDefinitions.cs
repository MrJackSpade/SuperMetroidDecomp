namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Fixed launch/cooldown mechanics selected by three independent RNG draws, how near Samus brings a launch and how its
/// rock falls (#2156).
/// </summary>
internal static class PolypLaunchDefinitions
{
    /// <summary>The random number divided by two, modulo 8, selects the cooldown.</summary>
    internal static readonly RandomSampleField CooldownSelector = new(1, 3);

    /// <summary>The random number divided by two, modulo 16, selects the launch speed and the horizontal velocity.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(1, 4);

    /// <summary>
    /// The cooldown each RNG selector chooses, in updates, the last the one the Polyp rearms on: seventeen, eight longer
    /// per selector (#2281).
    /// </summary>
    internal static readonly int[] Cooldowns = [17, 25, 33, 41, 49, 57, 65, 73];

    /// <summary>
    /// The place along the quadratic speed curve, mark 28, the first launch selector starts a rock's rise at; each
    /// following selector starts it a mark further along.
    /// </summary>
    internal static QuadraticSpeedCurvePlace FirstLaunchPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(28);

    /// <summary>
    /// The horizontal velocity each launch selector chooses: the first eight push right, the second eight push left by
    /// the same magnitudes, each a sixteenth of a pixel per update faster than the last from 96 256ths.
    /// </summary>
    internal static readonly SignedEightEight[] XVelocities =
    [
        SignedEightEight.FromParts(0, 96), SignedEightEight.FromParts(0, 112), SignedEightEight.FromParts(0, 128), SignedEightEight.FromParts(0, 144),
        SignedEightEight.FromParts(0, 160), SignedEightEight.FromParts(0, 176), SignedEightEight.FromParts(0, 192), SignedEightEight.FromParts(0, 208),
        -SignedEightEight.FromParts(0, 96), -SignedEightEight.FromParts(0, 112), -SignedEightEight.FromParts(0, 128), -SignedEightEight.FromParts(0, 144),
        -SignedEightEight.FromParts(0, 160), -SignedEightEight.FromParts(0, 176), -SignedEightEight.FromParts(0, 192), -SignedEightEight.FromParts(0, 208),
    ];

    /// <summary>How close on each axis Samus comes, in pixels, for a Polyp to launch a rock.</summary>
    internal const int LaunchReach = 64;

    /// <summary>The seed every Polyp's initialization resets the enemies' shared random numbers to.</summary>
    internal const int RandomSeed = 17;

    /// <summary>How far each update moves a rock's place along the quadratic speed curve, two marks.</summary>
    internal static QuadraticSpeedCurvePlace RockFallStep { get; } = QuadraticSpeedCurvePlace.FromMarks(2);

    /// <summary>The place along the quadratic speed curve, mark 64, a falling rock stops accelerating at.</summary>
    internal static QuadraticSpeedCurvePlace RockFastestFallPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(64);
}
