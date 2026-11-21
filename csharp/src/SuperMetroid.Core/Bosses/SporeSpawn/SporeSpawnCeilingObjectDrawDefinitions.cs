using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The block shape of each of Spore Spawn's four ceiling frames (#1608): the two-by-two square from the ceiling's
/// top-left block, which the three crumble frames and the clear all leave open. What each frame shows is its installed
/// picture (#1557).
/// </summary>
internal static class SporeSpawnCeilingObjectDrawDefinitions
{
    /// <summary>The opened ceiling: the two-by-two square right of and below the ceiling's top-left block, every block open.</summary>
    private static readonly RoomBlockShape OpenedCeiling = RoomBlockShapes.Rectangle(new(2, 2), RoomCollisionType.Air);

    /// <summary>The block shape each of the four ceiling frames draws: every one opens the same ceiling.</summary>
    internal static readonly Dictionary<SporeSpawnCeilingDraw, RoomBlockShape> Shapes =
        Enum.GetValues<SporeSpawnCeilingDraw>().ToDictionary(draw => draw, _ => OpenedCeiling);
}
