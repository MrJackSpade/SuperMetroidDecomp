namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Fixed launch/cooldown mechanics selected by three independent RNG draws, how near Samus brings a launch and how its
/// rock falls (#2156).
/// </summary>
internal static class PolypLaunchDefinitions
{

    /// <summary>
    /// The cooldown a launch's draw chooses, in updates, the last the one the Polyp rearms on (#2281, #2510): the eight rolls
    /// of its random number take seventeen updates and eight longer for each roll in turn.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> Cooldown = WeightedRandomChoice<int>.InTurn(
        EnemyRandomRolls.PolypCooldown, 17, 25, 33, 41, 49, 57, 65, 73);

    /// <summary>
    /// The place along the quadratic speed curve, mark 28, the first launch selector starts a rock's rise at; each
    /// following selector starts it a mark further along.
    /// </summary>
    internal static QuadraticSpeedCurvePlace FirstLaunchPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(28);

    /// <summary>
    /// The horizontal velocity a launch's draw chooses (#2510): the sixteen rolls of <see cref="EnemyRandomRolls.PolypLaunchPlace"/> in turn, the
    /// first eight pushing right and the second eight left by the same magnitudes, the slowest three eighths of a pixel per
    /// update and each a sixteenth of a pixel per update faster than the last.
    /// </summary>
    internal static readonly WeightedRandomChoice<SignedEightEight> XVelocity = WeightedRandomChoice<SignedEightEight>.InTurn(
        EnemyRandomRolls.PolypLaunchPlace,
        SignedEightEight.FromPixelFraction(3, 8), SignedEightEight.FromPixelFraction(7, 16), SignedEightEight.FromPixelFraction(1, 2), SignedEightEight.FromPixelFraction(9, 16),
        SignedEightEight.FromPixelFraction(5, 8), SignedEightEight.FromPixelFraction(11, 16), SignedEightEight.FromPixelFraction(3, 4), SignedEightEight.FromPixelFraction(13, 16),
        -SignedEightEight.FromPixelFraction(3, 8), -SignedEightEight.FromPixelFraction(7, 16), -SignedEightEight.FromPixelFraction(1, 2), -SignedEightEight.FromPixelFraction(9, 16),
        -SignedEightEight.FromPixelFraction(5, 8), -SignedEightEight.FromPixelFraction(11, 16), -SignedEightEight.FromPixelFraction(3, 4), -SignedEightEight.FromPixelFraction(13, 16));

    /// <summary>How close on each axis Samus comes, in pixels, for a Polyp to launch a rock.</summary>
    internal const int LaunchReach = 64;

    /// <summary>How far each update moves a rock's place along the quadratic speed curve, two marks.</summary>
    internal static QuadraticSpeedCurvePlace RockFallStep { get; } = QuadraticSpeedCurvePlace.FromMarks(2);

    /// <summary>The place along the quadratic speed curve, mark 64, a falling rock stops accelerating at.</summary>
    internal static QuadraticSpeedCurvePlace RockFastestFallPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(64);
}
