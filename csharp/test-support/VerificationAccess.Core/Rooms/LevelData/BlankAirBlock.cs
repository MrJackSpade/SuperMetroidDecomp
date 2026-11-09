namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The blank air block checks compare room-object frames with (#1580): block 255, unmirrored, as air.</summary>
internal static class BlankAirBlock
{
    /// <summary>A blank air block: block 255 of the tileset's set, unmirrored, which stock room-object frames draw to clear terrain.</summary>
    internal static readonly RoomTerrainBlock Value = RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air);
}
