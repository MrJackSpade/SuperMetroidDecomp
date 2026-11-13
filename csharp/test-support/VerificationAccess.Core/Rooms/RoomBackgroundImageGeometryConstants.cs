namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomBackgroundImageGeometry"/> that only verification reads.</summary>
internal static class RoomBackgroundImageGeometryConstants
{
    /// <summary>Pixels along a tilemap page's side: the width of every background picture, one page wide.</summary>
    internal const int PagePixels = RoomBackgroundImageGeometry.PageTiles * 8;
}
