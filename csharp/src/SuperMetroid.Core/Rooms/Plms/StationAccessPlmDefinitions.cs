namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Named station/side dispatch for the six station-access behaviours.
/// Each case selects its retracted/extended arm draws.
/// Energy and missile stations share access artwork on each side. Other behaviour
/// values, including the save-floor trigger, are outside this extending domain.
/// </summary>
internal static class StationAccessPlmDefinitions
{
    /// <summary>Map-station access from its right side.</summary>
    public static StationAccessPlmDefinition MapRight =>
        new(StationDraw.MapRightRetracted, StationDraw.MapRightExtended);

    /// <summary>Map-station access from its left side.</summary>
    public static StationAccessPlmDefinition MapLeft =>
        new(StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended);

    /// <summary>Energy-station access from its right side.</summary>
    public static StationAccessPlmDefinition EnergyRight =>
        new(StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended);

    /// <summary>Energy-station access from its left side.</summary>
    public static StationAccessPlmDefinition EnergyLeft =>
        new(StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended);

    /// <summary>Missile-station access from its right side.</summary>
    public static StationAccessPlmDefinition MissileRight =>
        new(StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended);

    /// <summary>Missile-station access from its left side.</summary>
    public static StationAccessPlmDefinition MissileLeft =>
        new(StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended);
}
