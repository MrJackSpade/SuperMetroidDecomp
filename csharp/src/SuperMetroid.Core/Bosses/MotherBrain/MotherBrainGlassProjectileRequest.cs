using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One <c>SpawnEprojWithRoomGfx($CEFC, parameter)</c> emitted by glass bytecode.</summary>
/// <param name="Kind">The projectile the glass spawns.</param>
/// <param name="Shard">Which way the shard flies, selecting its emitter on the glass.</param>
/// <param name="PlmBlock">The glass PLM's block, the origin of the emitters.</param>
internal readonly record struct MotherBrainGlassProjectileRequest(
    RoomEnemyProjectileKind Kind,
    MotherBrainGlassShard Shard,
    Position16<RoomBlocks> PlmBlock);
