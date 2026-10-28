using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Four native two-by-two physical ceiling draws. Each list has two
/// horizontal runs, calculated without a stored layout cache. Editable visible
/// block selections remain a separate asset.
/// </summary>
internal static class SporeSpawnCeilingPlmDrawDefinitions
{
    /// <summary>
    /// The one air cell each draw fills its square with: clear fills with the blank tile; crumble advances through
    /// tiles $53..55 in its three successive sixteen-byte records.
    /// </summary>
    internal static readonly Dictionary<SporeSpawnCeilingDraw, RoomLevelWord> Cells = new()
    {
        [SporeSpawnCeilingDraw.Clear] = RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        [SporeSpawnCeilingDraw.CrumbleFirst] = RoomLevelWord.Create(0x53, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        [SporeSpawnCeilingDraw.CrumbleSecond] = RoomLevelWord.Create(0x54, LevelBlockFlipFlags.None, RoomCollisionType.Air),
        [SporeSpawnCeilingDraw.CrumbleThird] = RoomLevelWord.Create(0x55, LevelBlockFlipFlags.None, RoomCollisionType.Air),
    };

    /// <summary>The clear layout's and the three successive crumble appearances' editable artwork keys.</summary>
    internal static readonly Dictionary<SporeSpawnCeilingDraw, string> VisualIds = new()
    {
        [SporeSpawnCeilingDraw.Clear] = "clear-ceiling",
        [SporeSpawnCeilingDraw.CrumbleFirst] = "crumble-frame-0",
        [SporeSpawnCeilingDraw.CrumbleSecond] = "crumble-frame-1",
        [SporeSpawnCeilingDraw.CrumbleThird] = "crumble-frame-2",
    };
}
