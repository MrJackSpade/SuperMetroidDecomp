using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Values a PLM slot starts from.</summary>
internal static class PlmSlotDefinitions
{
    /// <summary>
    /// The restore terrain of a cleared slot, and of a PLM whose setup clears it: block zero of the tileset, unmirrored,
    /// air.
    /// </summary>
    internal static readonly RoomTerrainBlock ClearedRestoreTerrain = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);
}
