using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Four two-by-two physical ceiling draws. Each list has two
/// horizontal runs, calculated without a stored layout cache. Editable visible
/// block selections remain a separate asset.
/// </summary>
internal static class SporeSpawnCeilingPlmDrawDefinitions
{
    /// <summary>
    /// The one air cell each draw fills its square with: clear fills with the blank tile; crumble advances through
    /// tiles $53..55 in its three successive sixteen-byte records.
    /// </summary>
    internal static readonly Dictionary<SporeSpawnCeilingDraw, RoomTerrainBlock> Cells = new()
    {
        [SporeSpawnCeilingDraw.Clear] = RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air),
        [SporeSpawnCeilingDraw.CrumbleFirst] = RoomTerrainBlock.Of(83, BlockMirroring.None, RoomCollisionType.Air),
        [SporeSpawnCeilingDraw.CrumbleSecond] = RoomTerrainBlock.Of(84, BlockMirroring.None, RoomCollisionType.Air),
        [SporeSpawnCeilingDraw.CrumbleThird] = RoomTerrainBlock.Of(85, BlockMirroring.None, RoomCollisionType.Air),
    };
}
