using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The rescue walls' block shapes (#1862): solid columns the wall's height down from its top block. What each frame shows
/// is its installed picture (#1557).
/// </summary>
internal static class EscapeAnimalObjectDrawDefinitions
{
    /// <summary>A solid wall the wall's height (<see cref="EscapeAnimalWallDefinitions.HeightInBlocks"/>) down from its top block.</summary>
    private static readonly RoomBlockShape SolidWall = new(
        [.. Enumerable.Range(0, EscapeAnimalWallDefinitions.HeightInBlocks).Select(row => new RoomBlockShapeCell(new(0, row), RoomCollisionType.SolidBlock))]);

    /// <summary>The block shape each of the four rescue-wall draws draws: every frame keeps the wall solid.</summary>
    internal static readonly Dictionary<EscapeAnimalDraw, RoomBlockShape> Shapes = new()
    {
        [EscapeAnimalDraw.Cracking] = SolidWall,
        [EscapeAnimalDraw.Breaking] = SolidWall,
        [EscapeAnimalDraw.Crumbling] = SolidWall,
        [EscapeAnimalDraw.Blank] = SolidWall,
    };
}
