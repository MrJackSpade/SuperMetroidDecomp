using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's ceiling block draws: which room-object draws are the ceiling's and how the cells of each two-by-two square
/// collide, from <see cref="SporeSpawnCeilingObjectDrawDefinitions"/>.
/// </summary>
internal static class SporeSpawnCeilingDraws
{
    /// <summary>How every cell of a ceiling draw's square collides.</summary>
    internal static bool TryGetCollision(RoomObjectDraw request, out RoomCollisionType collision)
    {
        if (request is not RoomObjectFrameDraw<SporeSpawnCeilingDraw>)
        {
            collision = default;
            return false;
        }
        var list = RoomObjectDraw.FrameOf<SporeSpawnCeilingDraw>(request, "SporeSpawnCeilingDraw");
        collision = SporeSpawnCeilingObjectDrawDefinitions.Collisions.TryGetValue(list, out RoomCollisionType cell)
            ? cell
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {list}.");
        return true;
    }
}
