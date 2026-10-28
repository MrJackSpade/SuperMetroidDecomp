namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The horizontal launch speeds of rocks spat by Kraid.</summary>
public static class KraidRockLaunchDefinitions
{
    /// <summary>$A7:BC67/BC73: leftward 3.75-pixel/frame launch.</summary>
    private static SignedEightEight SlowLeft { get; } = SignedEightEight.FromSignedRaw(-15 * 64);
    /// <summary>$A7:BC65/BC6F: leftward 4-pixel/frame launch.</summary>
    private static SignedEightEight NormalLeft { get; } = SignedEightEight.FromSignedRaw(-4 * 256);
    /// <summary>$A7:BC6B/BC71: leftward 4.5-pixel/frame launch.</summary>
    private static SignedEightEight FastLeft { get; } = SignedEightEight.FromSignedRaw(-18 * 64);
    /// <summary>$A7:BC69/BC6D: leftward 4.75-pixel/frame launch.</summary>
    private static SignedEightEight FastestLeft { get; } = SignedEightEight.FromSignedRaw(-19 * 64);

    /// <summary>
    /// Selects one of four launch speeds, each assigned two of the eight native
    /// RNG-bit choices. Preserves every ushort input and never advances RNG.
    /// </summary>
    /// <remarks>
    /// $A7:BC0D-BC14 selects a launch category from RNG bits1..3, not a motion
    /// sample or chronological random-number generator. Preserve the original
    /// branch assignment even though each speed has the same one-quarter weight.
    /// </remarks>
    public static SignedEightEight FromRandom(int random) => Speeds[random & ChoiceMask];

    /// <summary>Keeps RNG bits 1..3, the eight even launch choices.</summary>
    private const int ChoiceMask = 0x000e;

    /// <summary>The launch speed of each of the eight even RNG choices.</summary>
    private static readonly Dictionary<int, SignedEightEight> Speeds = new()
    {
        [0] = NormalLeft,
        [2] = SlowLeft,
        [4] = FastestLeft,
        [6] = FastLeft,
        [8] = FastestLeft,
        [10] = NormalLeft,
        [12] = FastLeft,
        [14] = SlowLeft,
    };
}
