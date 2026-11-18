namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One glass shard the glass's animation spawns.</summary>
/// <param name="Shard">Which way the shard flies, selecting its emitter on the glass.</param>
/// <param name="RoomObjectBlock">The glass room object's block, the origin of the emitters.</param>
internal readonly record struct MotherBrainGlassProjectileRequest(
    MotherBrainGlassShard Shard,
    Position16<RoomBlocks> RoomObjectBlock);
