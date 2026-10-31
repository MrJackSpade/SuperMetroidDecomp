namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Compiled gameplay route and callback identities used by the Baby Metroid after it
/// releases Mother Brain. These values control physical targets and acceleration; they
/// are engine definitions rather than editable sprite or animation data.
/// </summary>
internal static class BabyMetroidRouteDefinitions
{
    /// <summary>Number of authored route legs, <c>$A9:CA24</c> through <c>$A9:CA63</c>.</summary>
    internal const int RecordCount = 8;

    /// <summary>
    /// The eight authored waypoints. Each leg steers to its waypoint with a zero acceleration
    /// divisor index; the first five legs use the wrong-way extra-$10 callback and the final
    /// three the extra-$08 callback. Reaching the last waypoint ends the route.
    /// </summary>
    internal static readonly Position16<RoomPixels>[] Waypoints =
    [
        new(160, 120), new(304, 122), new(192, 64), new(192, 112),
        new(224, 128), new(205, 144), new(204, 160), new(203, 176),
    ];
    /// <summary>First route leg that switches from the extra-$10 to extra-$08 wrong-way correction.</summary>
    internal const int FirstExtraEightLeg = 5;

    /// <summary>The wrong-way horizontal speed addition of each native movement callback.</summary>
    internal static readonly Dictionary<BabyMetroidRouteMovementFunction, int> WrongWayOffScreenXSpeeds = new()
    {
        [BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight] = 8,
        [BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen] = 16,
    };
}
