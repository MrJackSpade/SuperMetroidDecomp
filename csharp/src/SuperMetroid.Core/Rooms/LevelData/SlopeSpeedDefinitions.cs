namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Grounded horizontal movement policy for the slope geometry families.</summary>
public static class SlopeSpeedDefinitions
{
    /// <summary>The whole distance, 256/256, for square/flat/V/overhang profiles without slope scaling.</summary>
    /// <remarks>Family membership follows the authored height profiles. The coefficients are authored tuning: cos(atan r),
    /// 1/(1+r^2) and linear-in-rise all fail to reproduce them (see the review inventory's residual scalar inputs).</remarks>
    internal const int Unscaled = 256;
    /// <summary>Eleven sixteenths of the distance, 176/256, for the four-pixel/two-pixel descending stair profiles 14/15.</summary>
    internal const int DescendingStairs = 176;
    /// <summary>Three quarters of the distance, 192/256, for the unit-rise full/clipped triangle profiles 18/20/21.</summary>
    internal const int UnitRise = 192;
    /// <summary>Twenty-seven thirty-seconds of the distance, 216/256, for the lower/upper half-rise profiles 22/23.</summary>
    internal const int HalfRise = 216;
    /// <summary>Fifteen sixteenths of the distance, 240/256, for the three consecutive third-rise profiles 24..26.</summary>
    internal const int ThirdRise = 240;
    /// <summary>Half the distance, 128/256, for the upper/lower double-rise profiles 27/28.</summary>
    internal const int DoubleRise = 128;
    /// <summary>Five sixteenths of the distance, 80/256, for the three consecutive triple-rise profiles 29..31.</summary>
    internal const int TripleRise = 80;

    /// <summary>
    /// The share of a grounded horizontal move that crosses each slope shape, in 256ths of the move, by slope shape
    /// 0..31: one entry per shape, in shape order, each naming the family whose constant documents its profiles.
    /// Behaviour orientation, airborne admission and signed scaling stay in callers.
    /// </summary>
    internal static IReadOnlyList<int> HorizontalMultipliers { get; } =
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
