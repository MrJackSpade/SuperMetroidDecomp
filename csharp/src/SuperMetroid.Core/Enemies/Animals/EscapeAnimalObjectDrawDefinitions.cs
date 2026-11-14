using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The rescue walls' geometry: three-cell vertical columns. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class EscapeAnimalObjectDrawDefinitions
{
    /// <summary>How every cell of each of the four rescue-wall draws collides, repeated down its three-cell column.</summary>
    internal static readonly Dictionary<EscapeAnimalDraw, RoomCollisionType> Collisions = new()
    {
        [EscapeAnimalDraw.Cracking] = RoomCollisionType.SolidBlock,
        [EscapeAnimalDraw.Breaking] = RoomCollisionType.SolidBlock,
        [EscapeAnimalDraw.Crumbling] = RoomCollisionType.SolidBlock,
        [EscapeAnimalDraw.Blank] = RoomCollisionType.SolidBlock,
    };

    /// <summary>The blocks in each rescue-wall column.</summary>
    internal const int WallHeight = 3;
}
