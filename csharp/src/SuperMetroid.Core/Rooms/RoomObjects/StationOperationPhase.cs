namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a map, energy or missile station's access arm is in its use (#1554).</summary>
internal enum StationOperationPhase
{
    /// <summary>No access sequence is in progress.</summary>
    Idle,
    /// <summary>The arm is in use, from the access's first draw to its end (<see cref="StationAccessTimingDefinitions.Use"/>).</summary>
    InUse,
    /// <summary>The access has finished; leaving the map screen still owns Samus's release.</summary>
    AwaitingMapUnpause,
}
