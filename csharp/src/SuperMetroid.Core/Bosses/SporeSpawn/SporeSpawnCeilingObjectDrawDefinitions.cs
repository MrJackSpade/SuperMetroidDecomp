using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Four two-by-two ceiling draws, each two horizontal runs of two cells. What each frame shows is its installed picture
/// (#1557).
/// </summary>
internal static class SporeSpawnCeilingObjectDrawDefinitions
{
    /// <summary>How every cell of each draw's square collides: the clear and the three crumble frames all leave air.</summary>
    internal static readonly Dictionary<SporeSpawnCeilingDraw, RoomCollisionType> Collisions = new()
    {
        [SporeSpawnCeilingDraw.Clear] = RoomCollisionType.Air,
        [SporeSpawnCeilingDraw.CrumbleFirst] = RoomCollisionType.Air,
        [SporeSpawnCeilingDraw.CrumbleSecond] = RoomCollisionType.Air,
        [SporeSpawnCeilingDraw.CrumbleThird] = RoomCollisionType.Air,
    };
}
