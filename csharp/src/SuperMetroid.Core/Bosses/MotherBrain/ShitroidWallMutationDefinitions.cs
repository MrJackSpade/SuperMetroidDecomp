using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The height of the Shitroid's invisible wall, and the room PLM and collision of each wall mutation.</summary>
internal static class ShitroidWallMutationDefinitions
{
    /// <summary>Blocks in the wall column, downward from the spawned block.</summary>
    internal const int WallHeight = 10;

    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<ShitroidWallMutation, RoomObjectHeaderId> RoomObjects =
        new Dictionary<ShitroidWallMutation, RoomObjectHeaderId>
        {
            [ShitroidWallMutation.Clear] = RoomObjectHeaderId.ClearBabyMetroidInvisibleWall,
            [ShitroidWallMutation.Create] = RoomObjectHeaderId.CreateBabyMetroidInvisibleWall,
        };

    /// <summary>The collision each mutation gives every block of the wall column, keeping its appearance.</summary>
    internal static readonly IReadOnlyDictionary<ShitroidWallMutation, RoomCollisionType> Collisions =
        new Dictionary<ShitroidWallMutation, RoomCollisionType>
        {
            [ShitroidWallMutation.Clear] = RoomCollisionType.Air,
            [ShitroidWallMutation.Create] = RoomCollisionType.SolidBlock,
        };
}
