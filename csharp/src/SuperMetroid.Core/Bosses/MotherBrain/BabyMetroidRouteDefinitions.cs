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
    private static readonly Position16<RoomPixels>[] Waypoints =
    [
        new(0x00a0, 0x0078), new(0x0130, 0x007a), new(0x00c0, 0x0040), new(0x00c0, 0x0070),
        new(0x00e0, 0x0080), new(0x00cd, 0x0090), new(0x00cc, 0x00a0), new(0x00cb, 0x00b0),
    ];
    /// <summary>First route leg that switches from the extra-$10 to extra-$08 wrong-way correction.</summary>
    private const int FirstExtraEightLeg = 5;

    /// <summary>Combines an authored waypoint with its callback and whether it is the final leg.</summary>
    private static BabyMetroidRouteRecord Record(int index) => new(
        Waypoints[index],
        0,
        index < FirstExtraEightLeg
            ? BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen
            : BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight,
        EndsWithLatchOntoSamus: index + 1 == RecordCount);

    /// <summary>Returns the authored route leg <paramref name="leg"/>, counted from zero.</summary>
    internal static BabyMetroidRouteRecord GetRecord(int leg) =>
        (uint)leg < RecordCount
            ? Record(leg)
            : throw new InvalidDataException($"Baby route leg {leg} is not one of the {RecordCount} authored legs.");

    /// <summary>Resolves a native movement callback to its wrong-way horizontal speed addition.</summary>
    internal static ushort GetWrongWayOffScreenXSpeed(BabyMetroidRouteMovementFunction movementFunction) =>
        movementFunction switch
        {
            BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight => 0x0008,
            BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen => 0x0010,
            _ => throw new InvalidDataException(
                $"Baby route names unknown movement function ${(int)movementFunction:X4}."),
        };

}
