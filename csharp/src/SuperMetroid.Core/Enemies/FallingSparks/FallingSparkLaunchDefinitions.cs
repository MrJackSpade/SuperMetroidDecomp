namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Native falling-spark horizontal 16.16 deltas, not initial X/Y positions.</summary>
public static class FallingSparkLaunchDefinitions
{
    /// <summary>
    /// $86:F3D4 distance / F3D6 subdistance, seven interleaved authored records.
    /// The eighth RNG outcome reads F3F0/F3F2 instruction words DBBD/301A; retain
    /// that cartridge overread rather than inventing a symmetric positive speed.
    /// </summary>
    public static SignedSixteenSixteen FromRandom(int random) => ((random & 0x1c) >> 2) switch
    {
        0 => SignedSixteenSixteen.FromParts(-1, 0xb800),
        1 => SignedSixteenSixteen.FromParts(-1, 0xc000),
        2 => SignedSixteenSixteen.FromParts(-1, 0xe000),
        3 => SignedSixteenSixteen.FromParts(-1, 0xff00),
        4 => SignedSixteenSixteen.FromParts(0, 0x0100),
        5 => SignedSixteenSixteen.FromParts(0, 0x2000),
        6 => SignedSixteenSixteen.FromParts(0, 0x4000),
        _ => SignedSixteenSixteen.FromParts(unchecked((short)0xdbbd), 0x301a),
    };
}
