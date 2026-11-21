using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Core.World;

/// <summary>
/// Where the enemy projectiles the room objects throw spawn (#2514): Mother Brain's glass's shards and the Bomb Torizo
/// statue hand's fragments, each thrown request spawning itself through its family's spawner.
/// </summary>
/// <param name="GlassShards">The spawner of Mother Brain's glass's shards.</param>
/// <param name="StatueFragments">The spawner of the Bomb Torizo statue hand's fragments.</param>
internal sealed record RoomObjectThrownProjectileSpawners(
    IMotherBrainGlassShardSpawner GlassShards,
    IBombTorizoStatueFragmentSpawner StatueFragments);
