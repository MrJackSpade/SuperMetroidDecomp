namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Native falling-spark horizontal 16.16 deltas, not initial X/Y positions.</summary>
public static class FallingSparkLaunchDefinitions
{
    /// <summary>The random bits that choose a launch: bits two through four.</summary>
    internal const int LaunchSelectorMask = 0x1c;

    /// <summary>
    /// $86:F3D4 distance / F3D6 subdistance, seven interleaved authored records, by launch selector.
    /// The eighth RNG outcome reads F3F0/F3F2 instruction words DBBD/301A; retain
    /// that cartridge overread rather than inventing a symmetric positive speed.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Launches =
    [
        SignedSixteenSixteen.FromParts(-1, 0xb800),
        SignedSixteenSixteen.FromParts(-1, 0xc000),
        SignedSixteenSixteen.FromParts(-1, 0xe000),
        SignedSixteenSixteen.FromParts(-1, 0xff00),
        SignedSixteenSixteen.FromParts(0, 0x0100),
        SignedSixteenSixteen.FromParts(0, 0x2000),
        SignedSixteenSixteen.FromParts(0, 0x4000),
        SignedSixteenSixteen.FromParts(unchecked((short)0xdbbd), 0x301a),
    ];
}
