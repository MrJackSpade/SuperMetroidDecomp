namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Exclusive meanings of behaviour $47-$4D after a special-solid block selects the station
/// access dispatcher. Left/right describe the side of the station containing the block.
/// </summary>
public enum StationAccessBehavior
{
    /// <summary>Behaviour $47: the map-station access block on the station's right side.</summary>
    MapRight = 71,
    /// <summary>Behaviour $48: the map-station access block on the station's left side.</summary>
    MapLeft = 72,
    /// <summary>Behaviour $49: the energy-recharge access block on the station's right side.</summary>
    EnergyRight = 73,
    /// <summary>Behaviour $4A: the energy-recharge access block on the station's left side.</summary>
    EnergyLeft = 74,
    /// <summary>Behaviour $4B: the missile-recharge access block on the station's right side.</summary>
    MissileRight = 75,
    /// <summary>Behaviour $4C: the missile-recharge access block on the station's left side.</summary>
    MissileLeft = 76,
    /// <summary>Behaviour $4D: the floor access block that activates the save station.</summary>
    SaveFloor = 77,
}
