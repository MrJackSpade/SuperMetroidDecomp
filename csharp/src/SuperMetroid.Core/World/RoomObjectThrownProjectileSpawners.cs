using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Core.World;

/// <summary>
/// Where the enemy projectiles the room objects throw spawn (#2514): Mother Brain's glass's shards and the Bomb Torizo
/// statue hand's fragments. The room's population hands the glass and the hand their spawner as they set up, after the
/// room's enemies are built, and each request they throw carries it.
/// </summary>
/// <param name="GlassShards">The spawner of Mother Brain's glass's shards.</param>
/// <param name="StatueFragments">The spawner of the Bomb Torizo statue hand's fragments.</param>
internal sealed partial record RoomObjectThrownProjectileSpawners(
    IMotherBrainGlassShardSpawner GlassShards,
    IBombTorizoStatueFragmentSpawner StatueFragments);
