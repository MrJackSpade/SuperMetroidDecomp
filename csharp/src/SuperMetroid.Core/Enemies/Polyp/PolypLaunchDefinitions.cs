namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Fixed launch/cooldown mechanics selected by three independent RNG draws, how near Samus brings a launch and how its
/// rock falls (#2156).
/// </summary>
internal static class PolypLaunchDefinitions
{
    /// <summary>
    /// The random roll, one of sixteen, whose value is how many marks past <see cref="FirstLaunchPlace"/> a rock's rise
    /// starts; a horizontal velocity's draw reads the same roll of its own number (<see cref="XVelocity"/>).
    /// </summary>
    internal static readonly RandomSampleField LaunchSelector = new(1, 4);

    /// <summary>
    /// The cooldown a launch's draw chooses, in updates, the last the one the Polyp rearms on (#2281, #2510): the eight rolls
    /// of its random number take seventeen updates and eight longer for each roll in turn.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> Cooldown = WeightedRandomChoice<int>.InTurn(
        new RandomSampleField(1, 3), 17, 25, 33, 41, 49, 57, 65, 73);

    /// <summary>
    /// The place along the quadratic speed curve, mark 28, the first launch selector starts a rock's rise at; each
    /// following selector starts it a mark further along.
    /// </summary>
    internal static QuadraticSpeedCurvePlace FirstLaunchPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(28);

    /// <summary>
    /// The horizontal velocity a launch's draw chooses (#2510): the sixteen rolls of <see cref="LaunchSelector"/> in turn, the
    /// first eight pushing right and the second eight left by the same magnitudes, each a sixteenth of a pixel per update
    /// faster than the last from 96 256ths.
    /// </summary>
    internal static readonly WeightedRandomChoice<SignedEightEight> XVelocity = WeightedRandomChoice<SignedEightEight>.InTurn(
        LaunchSelector,
        SignedEightEight.FromParts(0, 96), SignedEightEight.FromParts(0, 112), SignedEightEight.FromParts(0, 128), SignedEightEight.FromParts(0, 144),
        SignedEightEight.FromParts(0, 160), SignedEightEight.FromParts(0, 176), SignedEightEight.FromParts(0, 192), SignedEightEight.FromParts(0, 208),
        -SignedEightEight.FromParts(0, 96), -SignedEightEight.FromParts(0, 112), -SignedEightEight.FromParts(0, 128), -SignedEightEight.FromParts(0, 144),
        -SignedEightEight.FromParts(0, 160), -SignedEightEight.FromParts(0, 176), -SignedEightEight.FromParts(0, 192), -SignedEightEight.FromParts(0, 208));

    /// <summary>How close on each axis Samus comes, in pixels, for a Polyp to launch a rock.</summary>
    internal const int LaunchReach = 64;

    /// <summary>How far each update moves a rock's place along the quadratic speed curve, two marks.</summary>
    internal static QuadraticSpeedCurvePlace RockFallStep { get; } = QuadraticSpeedCurvePlace.FromMarks(2);

    /// <summary>The place along the quadratic speed curve, mark 64, a falling rock stops accelerating at.</summary>
    internal static QuadraticSpeedCurvePlace RockFastestFallPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(64);
}
