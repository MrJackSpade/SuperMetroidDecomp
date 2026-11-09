namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What the room object Spore Spawn spawns at its ceiling does: the only spawns Spore Spawn's ceiling spawner performs.</summary>
public enum SporeSpawnCeilingMutation
{
    /// <summary>Crumbles the ceiling on Spore Spawn's death, then clears it.</summary>
    Crumble,
    /// <summary>Clears the ceiling of a room whose Spore Spawn is already defeated.</summary>
    Clear,
}
