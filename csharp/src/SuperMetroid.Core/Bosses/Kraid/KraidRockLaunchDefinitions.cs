namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The horizontal launch speeds of rocks spat by Kraid.</summary>
public static class KraidRockLaunchDefinitions
{
    /// <summary>$A7:BC67/BC73: leftward 3.75-pixel/frame launch.</summary>
    private static SnesSignedEightEight SlowLeft { get; } = SnesSignedEightEight.FromSignedRaw(-15 * 64);
    /// <summary>$A7:BC65/BC6F: leftward 4-pixel/frame launch.</summary>
    private static SnesSignedEightEight NormalLeft { get; } = SnesSignedEightEight.FromSignedRaw(-4 * 256);
    /// <summary>$A7:BC6B/BC71: leftward 4.5-pixel/frame launch.</summary>
    private static SnesSignedEightEight FastLeft { get; } = SnesSignedEightEight.FromSignedRaw(-18 * 64);
    /// <summary>$A7:BC69/BC6D: leftward 4.75-pixel/frame launch.</summary>
    private static SnesSignedEightEight FastestLeft { get; } = SnesSignedEightEight.FromSignedRaw(-19 * 64);

    /// <summary>
    /// Selects one of four launch speeds, each assigned two of the eight native
    /// RNG-bit choices. Preserves every ushort input and never advances RNG.
    /// </summary>
    /// <remarks>
    /// $A7:BC0D-BC14 selects a launch category from RNG bits1..3, not a motion
    /// sample or chronological random-number generator. Preserve the original
    /// branch assignment even though each speed has the same one-quarter weight.
    /// </remarks>
    public static SnesSignedEightEight FromRandom(int random) => (random & 0x000e) switch
    {
        0 or 10 => NormalLeft,
        4 or 8 => FastestLeft,
        6 or 12 => FastLeft,
        _ => SlowLeft,
    };
}
