using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The block shape each mutation of the Shitroid's invisible wall gives its wall column (#1608).</summary>
internal static class ShitroidWallMutationDefinitions
{
    /// <summary>The opened wall: the ten blocks of the wall column, downward from the spawned block, made air.</summary>
    private static readonly RoomBlockShape OpenedWall = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air), new(new(0, 3), RoomCollisionType.Air),
        new(new(0, 4), RoomCollisionType.Air), new(new(0, 5), RoomCollisionType.Air),
        new(new(0, 6), RoomCollisionType.Air), new(new(0, 7), RoomCollisionType.Air),
        new(new(0, 8), RoomCollisionType.Air), new(new(0, 9), RoomCollisionType.Air),
    ]);

    /// <summary>The raised wall: the ten blocks of the wall column, downward from the spawned block, made solid.</summary>
    private static readonly RoomBlockShape RaisedWall = new(
    [
        new(new(0, 0), RoomCollisionType.SolidBlock), new(new(0, 1), RoomCollisionType.SolidBlock),
        new(new(0, 2), RoomCollisionType.SolidBlock), new(new(0, 3), RoomCollisionType.SolidBlock),
        new(new(0, 4), RoomCollisionType.SolidBlock), new(new(0, 5), RoomCollisionType.SolidBlock),
        new(new(0, 6), RoomCollisionType.SolidBlock), new(new(0, 7), RoomCollisionType.SolidBlock),
        new(new(0, 8), RoomCollisionType.SolidBlock), new(new(0, 9), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>
    /// The shape each mutation gives the wall column: every covered block takes the cell's collision and keeps its
    /// appearance.
    /// </summary>
    internal static readonly Dictionary<ShitroidWallMutation, RoomBlockShape> Shapes = new()
    {
        [ShitroidWallMutation.Clear] = OpenedWall,
        [ShitroidWallMutation.Create] = RaisedWall,
    };
}
