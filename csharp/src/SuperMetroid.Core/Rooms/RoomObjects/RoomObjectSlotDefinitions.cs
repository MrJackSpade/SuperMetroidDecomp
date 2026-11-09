using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Values a room-object slot starts from.</summary>
internal static class RoomObjectSlotDefinitions
{
    /// <summary>
    /// The restore terrain of a cleared slot, and of a room object whose setup clears it: block zero of the tileset, unmirrored,
    /// air.
    /// </summary>
    internal static readonly RoomTerrainBlock ClearedRestoreTerrain = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);
}
