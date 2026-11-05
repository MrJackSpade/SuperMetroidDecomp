namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>A common spawner the generator found, with the room data that runs it.</summary>
/// <param name="Spawned">The spawner and the objects it spawns.</param>
/// <param name="Presence">The rooms and enemies under which it runs.</param>
internal sealed record PlacedCommonSpawner(CommonSpawnedObjects Spawned, CommonSpawnerPresence Presence);
