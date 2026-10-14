namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive work transferred from a downward-gate PLM to the bank-$86 projectile pool.</summary>
public enum DownwardGateProjectileOperation
{
    /// <summary>$84:BBE1, Instruction_PLM_SpawnEnemyProjectileY, or closed-gate setup: allocates the selected gate actor and associates it with the PLM block; a full pool leaves no new actor.</summary>
    Spawn,
    /// <summary>$84:BBF0, Instruction_PLM_WakeEnemyProjectileAtPLMsPosition: finds the associated actor, advances its instruction pointer two bytes past sleep, and sets its instruction timer to one without allocating another actor.</summary>
    Wake,
}
