using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The station access a special-solid block's reaction selects (<see cref="BlockReactionDefinitions.StationAccesses"/>). Left/right
/// describe the side of the station containing the block.
/// </summary>
public enum StationAccessBehavior
{
    /// <summary>The map-station access block on the station's right side.</summary>
    MapRight,
    /// <summary>The map-station access block on the station's left side.</summary>
    MapLeft,
    /// <summary>The energy-recharge access block on the station's right side.</summary>
    EnergyRight,
    /// <summary>The energy-recharge access block on the station's left side.</summary>
    EnergyLeft,
    /// <summary>The missile-recharge access block on the station's right side.</summary>
    MissileRight,
    /// <summary>The missile-recharge access block on the station's left side.</summary>
    MissileLeft,
    /// <summary>The floor access block that activates the save station.</summary>
    SaveFloor,
}
