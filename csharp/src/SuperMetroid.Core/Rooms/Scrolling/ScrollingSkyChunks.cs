namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The sky page each chunk down the land and ocean skies shows (#1414, #1611).</summary>
/// <remarks>
/// A sky is <see cref="ChunkCount"/> chunks down its span, each showing one of the seven sky pages, and every chunk shows
/// one of its own sky's pages. The land sky has five chunks, pages 0 through 4, and repeats them from its first: chunks 5
/// through 7 show pages 0 through 2. The ocean sky has six chunks, pages 0 through 3 and then 5 and 6, skipping page 4,
/// and repeats them from its first: chunks 6 and 7 show pages 0 and 1.
/// </remarks>
public static class ScrollingSkyChunks
{
    /// <summary>The chunks down the sky's span before the span repeats.</summary>
    public const int ChunkCount = 8;

    /// <summary>The land sky's own chunks, which show pages 0 through 4.</summary>
    private const int LandChunks = 5;

    /// <summary>The ocean sky's own chunks.</summary>
    private const int OceanChunks = 6;

    /// <summary>The page the ocean sky skips: its chunks from this one on show the page after.</summary>
    private const int OceanSkippedPage = 4;

    /// <summary>The page, 0 through 6, chunk <paramref name="chunk"/> of <paramref name="table"/>'s sky shows.</summary>
    /// <param name="table">The land or ocean sky.</param>
    /// <param name="chunk">The chunk down the sky's span, 0 through <see cref="ChunkCount"/> less one.</param>
    /// <returns>The page's position among the seven sky pages.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The table is unknown or the chunk lies outside the sky's span.</exception>
    public static int PageOf(ScrollingSkyChunkTable table, int chunk)
    {
        bool ocean = table switch
        {
            ScrollingSkyChunkTable.Land => false,
            ScrollingSkyChunkTable.Ocean => true,
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown scrolling-sky chunk table."),
        };
        if ((uint)chunk >= ChunkCount)
            throw new ArgumentOutOfRangeException(nameof(chunk), chunk, "The chunk lies outside the scrolling sky's span.");
        if (!ocean)
            return chunk % LandChunks;
        int own = chunk % OceanChunks;
        return own < OceanSkippedPage ? own : own + 1;
    }
}
