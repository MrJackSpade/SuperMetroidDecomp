using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One shard projectile the glass's animation script spawns.</summary>
/// <param name="Kind">The projectile the glass spawns.</param>
/// <param name="Shard">Which way the shard flies, selecting its emitter on the glass.</param>
/// <param name="RoomObjectBlock">The glass PLM's block, the origin of the emitters.</param>
internal readonly record struct MotherBrainGlassProjectileRequest(
    RoomEnemyProjectileKind Kind,
    MotherBrainGlassShard Shard,
    Position16<RoomBlocks> RoomObjectBlock);
