namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling-spark horizontal 16.16 deltas, not initial X/Y positions.</summary>
public static class FallingSparkLaunchDefinitions
{
    /// <summary>The random field that chooses a launch: the sample divided by 4, modulo 8.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(2, 3);

    /// <summary>
    /// The seven authored horizontal drifts per update, by launch selector, in 256ths of a pixel: four leftward and three
    /// rightward launches. The eighth selection lies past them and drifts by <see cref="FallingSparkUnauthoredLaunch"/>.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Launches =
    [
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-72),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-64),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-32),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(1),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(32),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(64),
    ];
}
