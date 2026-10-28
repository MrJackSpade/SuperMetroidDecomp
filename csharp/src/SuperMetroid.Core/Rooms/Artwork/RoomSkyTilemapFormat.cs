namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>The scrolling sky's seven tilemap pages and their installed file names.</summary>
public static class RoomSkyTilemapFormat
{
    /// <summary>One native 32x32 page of 16-bit BG tile words.</summary>
    public const int PageByteCount = RoomBackgroundTilemapFormat.BytesPerPage;
    /// <summary>The sky's pages, which the land and ocean chunk tables select among.</summary>
    public const int PageCount = 7;
    /// <summary>Total contiguous byte length of all seven compiled tilemap pages.</summary>
    public const int TotalByteCount = PageCount * PageByteCount;

    /// <summary>Rows in one 32-by-32 page.</summary>
    public const int RowsPerPage = 32;

    /// <summary>Tiles in one sky page: 32 rows of 32.</summary>
    public const int PageTiles = RowsPerPage * RowsPerPage;

    /// <summary>Bytes in one page row: 32 tile words.</summary>
    public const int RowByteCount = PageByteCount / RowsPerPage;

    /// <summary>The manifest file that orders the seven editable sky-page resources.</summary>
    public const string ManifestFileName = "scrolling-sky.json";

    /// <summary>The editable JSON file name of one scrolling-sky page, named for its place among the pages (#1414).</summary>
    /// <param name="page">Zero-based page index from zero through six.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a sky page.</exception>
    public static string FileName(int page) => $"scrolling-sky-page-{Ensure.BetweenInclusive(0, PageCount - 1, page)}.json";
}
