namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Mutually exclusive work transferred from a downward-gate room object to the projectile pool.</summary>
public enum DownwardGateProjectileOperation
{
    /// <summary>Places the gate's actor at the gate's block, as a gate starts or sets up closed; a full pool places none.</summary>
    Spawn,
    /// <summary>Finds the associated actor, moves its animation past its rest, and has its next step run on the next frame, without allocating another actor.</summary>
    Wake,
}
