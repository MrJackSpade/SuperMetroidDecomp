using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's fight in his room: his state once his body has initialized. He and his projectiles share it: the ceiling
/// spawners hold their spores while he suppresses them, and his spores take his body's colours.
/// </summary>
[RoomLifetime]
internal sealed class SporeSpawnEncounter
{
    /// <summary>Single active boss state, absent outside a room containing Spore Spawn.</summary>
    internal SporeSpawnEnemyState? SporeSpawn { get; set; }
}
