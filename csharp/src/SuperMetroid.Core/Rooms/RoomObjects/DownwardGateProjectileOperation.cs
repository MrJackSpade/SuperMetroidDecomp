namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Mutually exclusive work transferred from a downward-gate room object to the projectile pool.</summary>
public enum DownwardGateProjectileOperation
{
    /// <summary>Or closed-gate setup: allocates the selected gate actor and associates it with the room-object block; a full pool leaves no new actor.</summary>
    Spawn,
    /// <summary>Finds the associated actor, moves its script past its sleep, and has its next step run on the next frame, without allocating another actor.</summary>
    Wake,
}
