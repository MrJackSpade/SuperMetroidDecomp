namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The sideways drifts a falling spark can launch with.</summary>
public static class FallingSparkLaunchDefinitions
{
    /// <summary>
    /// The launch a spark takes, chosen from eight rolls of its random number (#2510): the seven authored horizontal drifts
    /// per update in turn, in 256ths of a pixel, four leftward and three rightward, and for the eighth roll the drift past
    /// them (<see cref="FallingSparkUnauthoredLaunch"/>).
    /// </summary>
    internal static readonly WeightedRandomChoice<FallingSparkLaunch> Launch = WeightedRandomChoice<FallingSparkLaunch>.InTurn(
        new RandomSampleField(2, 3),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-72)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-64)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-32)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(1)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(32)),
        new FallingSparkLaunch.Authored(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(64)),
        FallingSparkLaunch.PastAuthored.Instance);
}
