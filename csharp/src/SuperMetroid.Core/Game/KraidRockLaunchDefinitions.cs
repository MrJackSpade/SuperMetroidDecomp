namespace SuperMetroid.Core.Game;

/// <summary>Native signed 8.8 velocities for rocks spat by Kraid.</summary>
public static class KraidRockLaunchDefinitions
{
    /// <summary>$A7:BC67/BC73: leftward 3.75-pixel/frame launch, signed8.8.</summary>
    private const ushort SlowLeft = unchecked((ushort)(-15 * 64));
    /// <summary>$A7:BC65/BC6F: leftward 4-pixel/frame launch, signed8.8.</summary>
    private const ushort NormalLeft = unchecked((ushort)(-4 * 256));
    /// <summary>$A7:BC6B/BC71: leftward 4.5-pixel/frame launch, signed8.8.</summary>
    private const ushort FastLeft = unchecked((ushort)(-18 * 64));
    /// <summary>$A7:BC69/BC6D: leftward 4.75-pixel/frame launch, signed8.8.</summary>
    private const ushort FastestLeft = unchecked((ushort)(-19 * 64));

    /// <summary>
    /// Selects one of four launch speeds, each assigned two of the eight native
    /// RNG-bit choices. Preserves every ushort input and never advances RNG.
    /// </summary>
    /// <remarks>
    /// $A7:BC0D-BC14 selects a launch category from RNG bits1..3, not a motion
    /// sample or chronological random-number generator. Preserve the original
    /// branch assignment even though each speed has the same one-quarter weight.
    /// </remarks>
    public static ushort FromRandom(ushort random) => (random & 0x000e) switch
    {
        0 or 10 => NormalLeft,
        4 or 8 => FastestLeft,
        6 or 12 => FastLeft,
        _ => SlowLeft,
    };
}
