using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The tilemap geometry the import pictures a library room background from (#1502): the two BG2 pages it reads and the
/// tiles of one changing cell. The game only shows the installed picture.
/// </summary>
internal static class RoomBackgroundImageGeometry
{
    /// <summary>The tilemap pages a library background pictures.</summary>
    internal const int Pages = 2;

    /// <summary>Tiles in a library background's two pages.</summary>
    internal const int Words = Pages * RoomBackgroundImage.PageTiles * RoomBackgroundImage.PageTiles;

    /// <summary>Tiles in one 16x16 cell of an image.</summary>
    internal const int CellTiles = 4;
}
