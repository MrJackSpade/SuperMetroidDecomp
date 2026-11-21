using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One link of Spore Spawn's stalk as he spawns it: where it starts from his body and where along the stalk it hangs.</summary>
/// <param name="SpawnOffset">Where the link starts, relative to his body.</param>
/// <param name="Place">Where along the stalk it hangs.</param>
internal sealed record SporeSpawnStalkLinkDefinition(Offset16<RoomPixels> SpawnOffset, SporeSpawnStalkPlace Place);
