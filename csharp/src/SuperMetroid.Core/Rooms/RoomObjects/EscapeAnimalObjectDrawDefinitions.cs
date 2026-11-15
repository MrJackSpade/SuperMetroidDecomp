using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The rescue walls' block shapes (#1862): solid three-block columns down from the wall's top block. What each frame shows
/// is its installed picture (#1557).
/// </summary>
internal static class EscapeAnimalObjectDrawDefinitions
{
    /// <summary>A solid wall three blocks tall down from its top block.</summary>
    private static readonly RoomBlockShape SolidWall = new(
    [
        new(new(0, 0), RoomCollisionType.SolidBlock), new(new(0, 1), RoomCollisionType.SolidBlock), new(new(0, 2), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The block shape each of the four rescue-wall draws draws: every frame keeps the wall solid.</summary>
    internal static readonly Dictionary<EscapeAnimalDraw, RoomBlockShape> Shapes = new()
    {
        [EscapeAnimalDraw.Cracking] = SolidWall,
        [EscapeAnimalDraw.Breaking] = SolidWall,
        [EscapeAnimalDraw.Crumbling] = SolidWall,
        [EscapeAnimalDraw.Blank] = SolidWall,
    };
}
