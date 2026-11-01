namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling-spark horizontal 16.16 deltas, not initial X/Y positions.</summary>
public static class FallingSparkLaunchDefinitions
{
    /// <summary>The random field that chooses a launch: bits two through four.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(2, 3);

    /// <summary>
    /// Distance and subdistance: the seven authored records, by launch selector, as signed 16.16 horizontal deltas:
    /// four leftward and three rightward launches. The eighth selection lies past them and drifts by
    /// <see cref="FallingSparkUnauthoredLaunch"/>.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Launches =
    [
        SignedSixteenSixteen.FromRaw(-18432),
        SignedSixteenSixteen.FromRaw(-16384),
        SignedSixteenSixteen.FromRaw(-8192),
        SignedSixteenSixteen.FromRaw(-256),
        SignedSixteenSixteen.FromParts(0, 256),
        SignedSixteenSixteen.FromParts(0, 8192),
        SignedSixteenSixteen.FromParts(0, 16384),
    ];
}
