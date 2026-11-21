namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The sideways drifts a falling spark can launch with.</summary>
public static partial class FallingSparkLaunchDefinitions
{
    /// <summary>
    /// The launch a spark takes, chosen from eight rolls of its random number (#2510): the seven authored sideways drifts
    /// per update in turn, four leftward and three rightward, and for the eighth roll the drift past them
    /// (<see cref="FallingSparkUnauthoredLaunch"/>).
    /// </summary>
    internal static readonly WeightedRandomChoice<FallingSparkLaunch> Launch = WeightedRandomChoice<FallingSparkLaunch>.InTurn(
        EnemyRandomRolls.FallingSparkLaunch,
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(-9, 32)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(-1, 4)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(-1, 8)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(-1, 256)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(1, 256)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(1, 8)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromPixelFraction(1, 4)),
        FallingSparkLaunch.PastAuthored.Instance);
}
