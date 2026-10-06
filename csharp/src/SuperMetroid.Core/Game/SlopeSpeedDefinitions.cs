namespace SuperMetroid.Core.Game;

/// <summary>Grounded horizontal movement policy for the native slope geometry families.</summary>
public static class SlopeSpeedDefinitions
{
    // REQUIRED: these selected movement coefficients are not derived from geometry.
    // Family membership follows native height profiles; no trigonometric rounding is claimed.
    /// <summary>$94:8588, unity coefficient for square/flat/V/overhang profiles without slope scaling.</summary>
    private const ushort Unscaled = 0x100;
    /// <summary>$94:85C0/85C4, four-pixel/two-pixel descending stair profiles 14/15.</summary>
    private const ushort DescendingStairs = 0x0b0;
    /// <summary>$94:85D0/85D8/85DC, unit-rise full/clipped triangle profiles 18/20/21.</summary>
    private const ushort UnitRise = 0x0c0;
    /// <summary>$94:85E0/85E4, lower/upper half-rise profiles 22/23.</summary>
    private const ushort HalfRise = 0x0d8;
    /// <summary>$94:85E8..85F0, three consecutive third-rise profiles 24..26.</summary>
    private const ushort ThirdRise = 0x0f0;
    /// <summary>$94:85F4/85F8, upper/lower double-rise profiles 27/28.</summary>
    private const ushort DoubleRise = 0x080;
    /// <summary>$94:85FC..8604, three consecutive triple-rise profiles 29..31.</summary>
    private const ushort TripleRise = 0x050;

    /// <summary>
    /// $94:8588 + 4*shape, adjustedDistanceMultiplier in the non-square horizontal
    /// collision consumer. Selects geometric families with eight fractional bits.
    /// BTS orientation, airborne admission and native signed multiplication stay in callers.
    /// Interleaved speedModifiers participate only in discarded native calculations.
    /// </summary>
    public static ushort HorizontalMultiplier(int shape) => shape switch
    {
        >= 0 and <= 13 or 16 or 17 or 19 => Unscaled,
        14 or 15 => DescendingStairs,
        18 or 20 or 21 => UnitRise,
        22 or 23 => HalfRise,
        >= 24 and <= 26 => ThirdRise,
        27 or 28 => DoubleRise,
        >= 29 and <= 31 => TripleRise,
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };
}
