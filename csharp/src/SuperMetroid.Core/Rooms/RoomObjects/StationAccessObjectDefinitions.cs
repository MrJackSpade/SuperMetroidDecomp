namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Named station/side dispatch for the six station-access behaviours.
/// Each case selects its retracted/extended arm draws.
/// Energy and missile stations share access artwork on each side. Other behaviour
/// values, including the save-floor trigger, are outside this extending domain.
/// </summary>
internal static class StationAccessObjectDefinitions
{
    /// <summary>Map-station access from its right side.</summary>
    public static StationAccessObjectDefinition MapRight =>
        new(StationDraw.MapRightRetracted, StationDraw.MapRightExtended);

    /// <summary>Map-station access from its left side.</summary>
    public static StationAccessObjectDefinition MapLeft =>
        new(StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended);

    /// <summary>Energy-station access from its right side.</summary>
    public static StationAccessObjectDefinition EnergyRight =>
        new(StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended);

    /// <summary>Energy-station access from its left side.</summary>
    public static StationAccessObjectDefinition EnergyLeft =>
        new(StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended);

    /// <summary>Missile-station access from its right side.</summary>
    public static StationAccessObjectDefinition MissileRight =>
        new(StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended);

    /// <summary>Missile-station access from its left side.</summary>
    public static StationAccessObjectDefinition MissileLeft =>
        new(StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended);
}
