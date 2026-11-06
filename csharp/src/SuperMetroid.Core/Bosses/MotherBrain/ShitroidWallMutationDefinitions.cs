using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The height of the Shitroid's invisible wall, and the room PLM and collision of each wall mutation.</summary>
internal static class ShitroidWallMutationDefinitions
{
    /// <summary>Blocks in the wall column, downward from the spawned block.</summary>
    internal const int WallHeight = 10;

    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<ShitroidWallMutation, PlmHeaderId> Plms =
        new Dictionary<ShitroidWallMutation, PlmHeaderId>
        {
            [ShitroidWallMutation.Clear] = PlmHeaderId.ClearBabyMetroidInvisibleWall,
            [ShitroidWallMutation.Create] = PlmHeaderId.CreateBabyMetroidInvisibleWall,
        };

    /// <summary>The collision each mutation gives every block of the wall column, keeping its appearance.</summary>
    internal static readonly IReadOnlyDictionary<ShitroidWallMutation, RoomCollisionType> Collisions =
        new Dictionary<ShitroidWallMutation, RoomCollisionType>
        {
            [ShitroidWallMutation.Clear] = RoomCollisionType.Air,
            [ShitroidWallMutation.Create] = RoomCollisionType.SolidBlock,
        };
}
