namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// What one common spawner, code outside the enemy behaviours, spawns onto the shared sheet of the sets whose rooms run it
/// (#1513).
/// </summary>
/// <param name="Spawner">
/// The spawner: the Core type whose code spawns, or <c>Type.Member</c> for a type whose members serve unrelated rooms.
/// <see cref="CommonSpawnerPresenceDefinitions"/> names the rooms each one runs in.
/// </param>
/// <param name="Objects">The projectile frames and sprite objects it spawns.</param>
internal sealed record CommonSpawnedObjects(string Spawner, SpawnedObjects Objects);
