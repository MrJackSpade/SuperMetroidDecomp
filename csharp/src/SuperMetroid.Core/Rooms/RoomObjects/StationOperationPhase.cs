namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a map, energy or missile station's access arm is in its use (#1554).</summary>
internal enum StationOperationPhase
{
    /// <summary>No access sequence is in progress.</summary>
    Idle,
    /// <summary>The first access draw is waiting to extend.</summary>
    Extending,
    /// <summary>The extended arm is holding before station activation.</summary>
    Extended,
    /// <summary>Station activation finished; the arm has not begun retraction.</summary>
    PostActivationHold,
    /// <summary>The arm is retracting after its sound request.</summary>
    Retracting,
    /// <summary>The short arm pose is holding before the access ends.</summary>
    FinalRetractionHold,
    /// <summary>The access has finished; leaving the map screen still owns Samus's release.</summary>
    AwaitingMapUnpause,
}
