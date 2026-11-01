namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Mutually exclusive work transferred from a downward-gate PLM to the projectile pool.</summary>
public enum DownwardGateProjectileOperation
{
    /// <summary>Or closed-gate setup: allocates the selected gate actor and associates it with the PLM block; a full pool leaves no new actor.</summary>
    Spawn,
    /// <summary>Finds the associated actor, advances its instruction pointer two bytes past sleep, and sets its instruction timer to one without allocating another actor.</summary>
    Wake,
}
