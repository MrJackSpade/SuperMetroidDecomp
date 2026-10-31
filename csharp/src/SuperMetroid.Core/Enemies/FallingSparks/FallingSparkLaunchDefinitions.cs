namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Native falling-spark horizontal 16.16 deltas, not initial X/Y positions.</summary>
public static class FallingSparkLaunchDefinitions
{
    /// <summary>The random field that chooses a launch: bits two through four.</summary>
    internal static readonly RandomSampleField LaunchSelector = new(2, 3);

    /// <summary>
    /// $86:F3D4 distance / F3D6 subdistance, seven interleaved authored records, by launch selector, as signed 16.16
    /// horizontal deltas: four leftward and three rightward launches.
    /// The eighth RNG outcome reads F3F0/F3F2 instruction words DBBD/301A; retain
    /// that cartridge overread rather than inventing a symmetric positive speed.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Launches =
    [
        SignedSixteenSixteen.FromRaw(-0x4800),
        SignedSixteenSixteen.FromRaw(-0x4000),
        SignedSixteenSixteen.FromRaw(-0x2000),
        SignedSixteenSixteen.FromRaw(-0x0100),
        SignedSixteenSixteen.FromParts(0, 0x0100),
        SignedSixteenSixteen.FromParts(0, 0x2000),
        SignedSixteenSixteen.FromParts(0, 0x4000),
        SignedSixteenSixteen.FromParts(WordWrap.Signed(0xdbbd), 0x301a),
    ];
}
