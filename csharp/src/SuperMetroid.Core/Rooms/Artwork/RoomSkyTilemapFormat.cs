namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Native bank-$8A scrolling-sky visual pages; pointer arithmetic remains engine-owned.</summary>
public static class RoomSkyTilemapFormat
{
    /// <summary>$8A:B180, first of seven contiguous 32x32 scrolling-sky tilemap pages.</summary>
    public const int FirstSourceAddress = SuperMetroid.Core.Rooms.RoomAssetRomData.ScrollingSkyTilemapPages;
    /// <summary>One native 32x32 page of 16-bit BG tile words.</summary>
    public const int PageByteCount = RoomBackgroundTilemapFormat.BytesPerPage;
    /// <summary>Seven pages through $8A:E97F, including land and ocean sources.</summary>
    public const int PageCount = 7;
    /// <summary>Total contiguous byte length of all seven compiled tilemap pages.</summary>
    public const int TotalByteCount = PageCount * PageByteCount;

    /// <summary>The manifest file that orders the seven editable sky-page resources.</summary>
    public const string ManifestFileName = "scrolling-sky.json";

    /// <summary>Calculates one page's full 24-bit native source identity.</summary>
    /// <param name="page">Zero-based page index from zero through six.</param>
    /// <returns>The page-aligned address in the contiguous bank-$8A range.</returns>
    public static int SourceAddress(int page) => (uint)page < PageCount
        ? FirstSourceAddress + page * PageByteCount
        : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>Builds the editable JSON file name for one scrolling-sky page.</summary>
    /// <param name="page">Zero-based page index from zero through six.</param>
    /// <returns>A file name containing the page's six-digit native source address.</returns>
    public static string FileName(int page) => $"scrolling-sky-{SourceAddress(page):X6}.json";
}
