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
        new(0x00a0, 0x0078), new(0x0130, 0x007a), new(0x00c0, 0x0040), new(0x00c0, 0x0070),
        new(0x00e0, 0x0080), new(0x00cd, 0x0090), new(0x00cc, 0x00a0), new(0x00cb, 0x00b0),
    ];
    /// <summary>First route leg that switches from the extra-$10 to extra-$08 wrong-way correction.</summary>
    internal const int FirstExtraEightLeg = 5;

    /// <summary>The wrong-way horizontal speed addition of each native movement callback.</summary>
    internal static readonly Dictionary<BabyMetroidRouteMovementFunction, ushort> WrongWayOffScreenXSpeeds = new()
    {
        [BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight] = 0x0008,
        [BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen] = 0x0010,
    };
}
