using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The block shape each mutation of the Baby Metroid's invisible wall gives its wall column (#1608).</summary>
internal static partial class BabyMetroidWallMutationDefinitions
{
    /// <summary>How many blocks tall the wall column is.</summary>
    private const int WallHeight = 10;

    /// <summary>The opened wall: the <see cref="WallHeight"/> blocks of the wall column, downward from the spawned block, made air.</summary>
    private static readonly RoomBlockShape OpenedWall = RoomBlockShapes.Column(WallHeight, RoomCollisionType.Air);

    /// <summary>The raised wall: the <see cref="WallHeight"/> blocks of the wall column, downward from the spawned block, made solid.</summary>
    private static readonly RoomBlockShape RaisedWall = RoomBlockShapes.Column(WallHeight, RoomCollisionType.SolidBlock);

    /// <summary>
    /// The shape each mutation gives the wall column: every covered block takes the cell's collision and keeps its
    /// appearance.
    /// </summary>
    internal static readonly Dictionary<BabyMetroidWallMutation, RoomBlockShape> Shapes = new()
    {
        [BabyMetroidWallMutation.Clear] = OpenedWall,
        [BabyMetroidWallMutation.Create] = RaisedWall,
    };
}
