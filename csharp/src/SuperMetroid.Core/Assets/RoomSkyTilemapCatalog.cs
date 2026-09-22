using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Seven contiguous native scrolling-sky tilemap pages, selected by host artwork.</summary>
public sealed class RoomSkyTilemapCatalog : IRomArtworkSource
{
    private readonly byte[] pages;

    public RoomSkyTilemapCatalog(IReadOnlyList<RoomBackgroundTilemapAtlas> selectedPages)
    {
        ArgumentNullException.ThrowIfNull(selectedPages);
        if (selectedPages.Count != RoomSkyTilemapFormat.PageCount)
            throw new InvalidDataException(
                $"Scrolling sky requires {RoomSkyTilemapFormat.PageCount} ordered pages.");
        pages = new byte[RoomSkyTilemapFormat.TotalByteCount];
        for (int index = 0; index < selectedPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas atlas = selectedPages[index]
                ?? throw new InvalidDataException($"Scrolling sky page {index} is missing.");
            if (atlas.Transfer.Length != RoomSkyTilemapFormat.PageByteCount)
                throw new InvalidDataException($"Scrolling sky page {index} has the wrong length.");
            atlas.Transfer.Span.CopyTo(pages.AsSpan(index * RoomSkyTilemapFormat.PageByteCount));
        }
    }

    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        int offset = sourceAddress - RoomSkyTilemapFormat.FirstSourceAddress;
        if ((uint)offset >= pages.Length)
        {
            data = default;
            return false;
        }
        if (byteCount < 0 || byteCount > pages.Length - offset)
            throw new InvalidDataException(
                $"Scrolling-sky transfer ${sourceAddress:X6}+${byteCount:X} crosses the installed pages.");
        data = pages.AsMemory(offset, byteCount);
        return true;
    }
}

/// <summary>Native bank-$8A scrolling-sky visual pages; pointer arithmetic remains engine-owned.</summary>
public static class RoomSkyTilemapFormat
{
    /// <summary>$8A:B180, first of seven contiguous 32x32 scrolling-sky tilemap pages.</summary>
    public const int FirstSourceAddress = 0x8ab180;
    /// <summary>One native 32x32 page of 16-bit BG tile words.</summary>
    public const int PageByteCount = RoomBackgroundTilemapFormat.BytesPerPage;
    /// <summary>Seven pages through $8A:E97F, including land and ocean sources.</summary>
    public const int PageCount = 7;
    public const int TotalByteCount = PageCount * PageByteCount;
    public const string ManifestFileName = "scrolling-sky.json";

    public static int SourceAddress(int page) => (uint)page < PageCount
        ? FirstSourceAddress + page * PageByteCount
        : throw new ArgumentOutOfRangeException(nameof(page));

    public static string FileName(int page) => $"scrolling-sky-{SourceAddress(page):X6}.json";
}
