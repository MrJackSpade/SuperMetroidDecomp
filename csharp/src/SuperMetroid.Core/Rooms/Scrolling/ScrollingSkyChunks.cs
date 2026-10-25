namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The sky page each chunk of the land and ocean sky tables selects (#1414).</summary>
/// <remarks>
/// The camera admits chunk indices 0 through 8 and 255. The land table's five pages repeat: indices 5 through 8 show
/// pages 0 through 3 again. The ocean table shows pages 0 through 3, skips page 4, then shows pages 5 and 6; its
/// indices 6 through 8 read the code beside the table, which names no page, so those chunks show nothing. Index 255
/// reads an instruction operand on either table and shows nothing either. Every other index is rejected. Each
/// selection is verified against the pinned cartridge's tables.
/// </remarks>
public static class ScrollingSkyChunks
{
    /// <summary>Chunk indices from zero the camera can select; 255 is the only other.</summary>
    private const int ChunkCount = 9;

    /// <summary>The land table's pages before they repeat.</summary>
    private const int LandPageCount = 5;

    /// <summary>The ocean table's pages before it skips one.</summary>
    private const int OceanLeadingPageCount = 4;

    /// <summary>The ocean table's last chunk that names a page.</summary>
    private const int OceanLastPageChunk = 5;

    /// <summary>The page, 0 through 6, chunk <paramref name="index"/> of <paramref name="table"/> shows; false for a chunk that names no page.</summary>
    /// <param name="table">The land or ocean table.</param>
    /// <param name="index">The chunk index the camera selects: 0 through 8, or 255.</param>
    /// <param name="page">The page's position among the seven sky pages when the chunk selects one, otherwise -1.</param>
    /// <exception cref="ArgumentOutOfRangeException">The table is unknown or the camera cannot select the index.</exception>
    public static bool TryGetPage(ScrollingSkyChunkTable table, int index, out int page)
    {
        bool ocean = table switch
        {
            ScrollingSkyChunkTable.Land => false,
            ScrollingSkyChunkTable.Ocean => true,
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown scrolling-sky chunk table."),
        };
        page = -1;
        if (index == byte.MaxValue)
            return false;
        if ((uint)index >= ChunkCount)
            throw new ArgumentOutOfRangeException(nameof(index), index,
                "Camera row selected an unreachable scrolling-sky chunk index.");
        if (!ocean)
            page = index % LandPageCount;
        else if (index < OceanLeadingPageCount)
            page = index;
        else if (index <= OceanLastPageChunk)
            page = index + 1;
        return page >= 0;
    }
}
