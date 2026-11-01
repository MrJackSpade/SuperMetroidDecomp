namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Grounded horizontal movement policy for the slope geometry families.</summary>
public static class SlopeSpeedDefinitions
{
    /// <summary>Unity coefficient for square/flat/V/overhang profiles without slope scaling.</summary>
    /// <remarks>Family membership follows the authored height profiles. The coefficients are authored tuning: cos(atan r),
    /// 1/(1+r^2) and linear-in-rise all fail to reproduce them (see residualScalarInputsReview).</remarks>
    internal const int Unscaled = 256;
    /// <summary>Four-pixel/two-pixel descending stair profiles 14/15.</summary>
    internal const int DescendingStairs = 176;
    /// <summary>Unit-rise full/clipped triangle profiles 18/20/21.</summary>
    internal const int UnitRise = 192;
    /// <summary>Lower/upper half-rise profiles 22/23.</summary>
    internal const int HalfRise = 216;
    /// <summary>Three consecutive third-rise profiles 24..26.</summary>
    internal const int ThirdRise = 240;
    /// <summary>Upper/lower double-rise profiles 27/28.</summary>
    internal const int DoubleRise = 128;
    /// <summary>Three consecutive triple-rise profiles 29..31.</summary>
    internal const int TripleRise = 80;

    /// <summary>
    /// The per-shape slope distance multiplier, adjustedDistanceMultiplier in the non-square horizontal
    /// collision consumer, by slope shape 0..31: one entry per shape, in shape order, each
    /// naming the family whose constant documents its profiles. Selects geometric families with eight fractional bits.
    /// behaviour orientation, airborne admission and signed multiplication stay in callers.
    /// Interleaved speedModifiers participate only in discarded calculations.
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
