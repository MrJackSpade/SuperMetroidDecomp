namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed bank-$84 header/list identities for map and resource-station access actors.</summary>
internal static class StationAccessPlmDefinitions
{
    /// <summary><c>$84:B6D7/$AD86</c>, map-station access from its right side.</summary>
    public static readonly StationAccessPlmDefinition MapRight =
        new(StationAccessBehavior.MapRight, RoomPlmHeaders.MapStationRightAccess, 0xad86);

    /// <summary><c>$84:B6DB/$ADA4</c>, map-station access from its left side.</summary>
    public static readonly StationAccessPlmDefinition MapLeft =
        new(StationAccessBehavior.MapLeft, RoomPlmHeaders.MapStationLeftAccess, 0xada4);

    /// <summary><c>$84:B6E3/$ADF1</c>, energy-station access from its right side.</summary>
    public static readonly StationAccessPlmDefinition EnergyRight =
        new(StationAccessBehavior.EnergyRight, RoomPlmHeaders.EnergyStationRightAccess, 0xadf1);

    /// <summary><c>$84:B6E7/$AE13</c>, energy-station access from its left side.</summary>
    public static readonly StationAccessPlmDefinition EnergyLeft =
        new(StationAccessBehavior.EnergyLeft, RoomPlmHeaders.EnergyStationLeftAccess, 0xae13);

    /// <summary><c>$84:B6EF/$AE7B</c>, missile-station access from its right side.</summary>
    public static readonly StationAccessPlmDefinition MissileRight =
        new(StationAccessBehavior.MissileRight, RoomPlmHeaders.MissileStationRightAccess, 0xae7b);

    /// <summary><c>$84:B6F3/$AE9D</c>, missile-station access from its left side.</summary>
    public static readonly StationAccessPlmDefinition MissileLeft =
        new(StationAccessBehavior.MissileLeft, RoomPlmHeaders.MissileStationLeftAccess, 0xae9d);

    private static readonly StationAccessPlmDefinition[] Definitions =
    [
        MapRight,
        MapLeft,
        EnergyRight,
        EnergyLeft,
        MissileRight,
        MissileLeft,
    ];

    /// <summary>Complete six-record domain used by extending station access blocks.</summary>
    public static ReadOnlySpan<StationAccessPlmDefinition> All => Definitions;

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
    ushort InstructionListPointer);
