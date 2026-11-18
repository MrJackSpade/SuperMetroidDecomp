namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The stations Samus uses through an access arm (<see cref="AccessStationObject"/>): each has its access rule
/// (<see cref="StationAccessRuleDefinitions"/>). The save station is a room object of its own (<see cref="SaveStationObject"/>).
/// </summary>
public enum StationKind
{
    /// <summary>Acquires the current area's map and keeps Samus input locked until map-screen unpause releases it.</summary>
    Map,
    /// <summary>Refills Samus health to its current maximum after the access arm's extension hold.</summary>
    Energy,
    /// <summary>Refills ordinary Missiles to their current maximum after the access arm's extension hold.</summary>
    Missile,
}
