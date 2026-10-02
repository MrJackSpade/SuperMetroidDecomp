namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named station/side dispatch for BTS $47..$4C, matching $94:91C7..91D2.
/// Each case selects its bank-$84 header/list and retracted/extended draw operands.
/// Energy and missile stations share access artwork on each side. Other BTS
/// values, including the save-floor trigger, are outside this extending domain.
/// </summary>
internal static class StationAccessPlmDefinitions
{
    /// <summary><c>$84:B6D7/$AD86</c>, map-station access from its right side.</summary>
    public static StationAccessPlmDefinition MapRight =>
        new(StationAccessBehavior.MapRight, RoomPlmHeaders.MapStationRightAccess,
            0xad86, 0x9f49, 0x9f55);

    /// <summary><c>$84:B6DB/$ADA4</c>, map-station access from its left side.</summary>
    public static StationAccessPlmDefinition MapLeft =>
        new(StationAccessBehavior.MapLeft, RoomPlmHeaders.MapStationLeftAccess,
            0xada4, 0x9f5b, 0x9f67);

    /// <summary><c>$84:B6E3/$ADF1</c>, energy-station access from its right side.</summary>
    public static StationAccessPlmDefinition EnergyRight =>
        new(StationAccessBehavior.EnergyRight, RoomPlmHeaders.EnergyStationRightAccess,
            0xadf1, 0x9fb5, 0x9fbb);

    /// <summary><c>$84:B6E7/$AE13</c>, energy-station access from its left side.</summary>
    public static StationAccessPlmDefinition EnergyLeft =>
        new(StationAccessBehavior.EnergyLeft, RoomPlmHeaders.EnergyStationLeftAccess,
            0xae13, 0x9fc1, 0x9fc7);

    /// <summary><c>$84:B6EF/$AE7B</c>, missile-station access from its right side.</summary>
    public static StationAccessPlmDefinition MissileRight =>
        new(StationAccessBehavior.MissileRight, RoomPlmHeaders.MissileStationRightAccess,
            0xae7b, 0x9fb5, 0x9fbb);

    /// <summary><c>$84:B6F3/$AE9D</c>, missile-station access from its left side.</summary>
    public static StationAccessPlmDefinition MissileLeft =>
        new(StationAccessBehavior.MissileLeft, RoomPlmHeaders.MissileStationLeftAccess,
            0xae9d, 0x9fc1, 0x9fc7);

    /// <summary>Enumerate the six named station/side cases in their native BTS order.</summary>
    public static IEnumerable<StationAccessPlmDefinition> All
    {
        get
        {
            yield return MapRight;
            yield return MapLeft;
            yield return EnergyRight;
            yield return EnergyLeft;
            yield return MissileRight;
            yield return MissileLeft;
        }
    }

    /// <summary>Resolves the access actor selected by one station-special-block BTS.</summary>
    public static StationAccessPlmDefinition Resolve(StationAccessBehavior behavior) => behavior switch
    {
        StationAccessBehavior.MapRight => MapRight,
        StationAccessBehavior.MapLeft => MapLeft,
        StationAccessBehavior.EnergyRight => EnergyRight,
        StationAccessBehavior.EnergyLeft => EnergyLeft,
        StationAccessBehavior.MissileRight => MissileRight,
        StationAccessBehavior.MissileLeft => MissileLeft,
        _ => throw new InvalidDataException($"Station access has invalid BTS ${(byte)behavior:X2}.")
    };
}

/// <summary>One station-access BTS paired with its PLM header and initial instruction list.</summary>
internal readonly record struct StationAccessPlmDefinition(
    StationAccessBehavior Behavior,
    ushort HeaderPointer,
    ushort InstructionListPointer,
    ushort RetractedDrawPointer,
    ushort ExtendedDrawPointer)
{
    internal ushort DrawPointer(bool extended) =>
        extended ? ExtendedDrawPointer : RetractedDrawPointer;
}
