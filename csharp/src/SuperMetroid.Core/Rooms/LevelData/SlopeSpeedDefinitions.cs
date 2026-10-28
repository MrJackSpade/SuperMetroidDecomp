namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Grounded horizontal movement policy for the native slope geometry families.</summary>
public static class SlopeSpeedDefinitions
{
    /// <summary>$94:8588, unity coefficient for square/flat/V/overhang profiles without slope scaling.</summary>
    /// <remarks>Family membership follows native height profiles. The coefficients are authored tuning: cos(atan r),
    /// 1/(1+r^2) and linear-in-rise all fail to reproduce them (see residualScalarInputsReview).</remarks>
    internal const ushort Unscaled = 0x100;
    /// <summary>$94:85C0/85C4, four-pixel/two-pixel descending stair profiles 14/15.</summary>
    internal const ushort DescendingStairs = 0x0b0;
    /// <summary>$94:85D0/85D8/85DC, unit-rise full/clipped triangle profiles 18/20/21.</summary>
    internal const ushort UnitRise = 0x0c0;
    /// <summary>$94:85E0/85E4, lower/upper half-rise profiles 22/23.</summary>
    internal const ushort HalfRise = 0x0d8;
    /// <summary>$94:85E8..85F0, three consecutive third-rise profiles 24..26.</summary>
    internal const ushort ThirdRise = 0x0f0;
    /// <summary>$94:85F4/85F8, upper/lower double-rise profiles 27/28.</summary>
    internal const ushort DoubleRise = 0x080;
    /// <summary>$94:85FC..8604, three consecutive triple-rise profiles 29..31.</summary>
    internal const ushort TripleRise = 0x050;

    /// <summary>
    /// $94:8588 + 4*shape, adjustedDistanceMultiplier in the non-square horizontal
    /// collision consumer, by slope shape 0..31: one entry per shape, in shape order, each
    /// naming the family whose constant documents its profiles. Selects geometric families with eight fractional bits.
    /// BTS orientation, airborne admission and native signed multiplication stay in callers.
    /// Interleaved speedModifiers participate only in discarded native calculations.
    /// </summary>
    internal static IReadOnlyList<ushort> HorizontalMultipliers { get; } =
    [
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        Unscaled,
        DescendingStairs,
        DescendingStairs,
        Unscaled,
        Unscaled,
        UnitRise,
        Unscaled,
        UnitRise,
        UnitRise,
        HalfRise,
        HalfRise,
        ThirdRise,
        ThirdRise,
        ThirdRise,
        DoubleRise,
        DoubleRise,
        TripleRise,
        TripleRise,
        TripleRise,
    ];
}
