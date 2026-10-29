namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomSkyTilemapFormat"/> that only verification reads.</summary>
internal static class RoomSkyTilemapFormatConstants
{
    /// <summary>Total contiguous byte length of all seven compiled tilemap pages.</summary>
    public const int TotalByteCount = RoomSkyTilemapFormat.PageCount * RoomSkyTilemapFormat.PageByteCount;
}
