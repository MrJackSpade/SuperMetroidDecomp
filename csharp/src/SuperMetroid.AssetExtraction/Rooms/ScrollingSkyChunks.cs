using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The cartridge's sky page each chunk down the land and ocean skies shows (#1414, #1611, #2101): the import lays each
/// sky's rows from these pages once (<see cref="SkyTilemap"/>), so the installed sky is one image repeating down its span
/// and the game never knows the pages.
/// </summary>
/// <remarks>
/// A sky is <see cref="ChunkCount"/> chunks down its span, each showing one of the sky pages, and every chunk shows one of
/// its own sky's pages. The land sky's own chunks (<see cref="LandChunks"/>) show its pages in order, and its later chunks
/// repeat them from its first. The ocean sky's own chunks (<see cref="OceanChunks"/>) show its pages in order, skipping
/// <see cref="OceanSkippedPage"/>, and its later chunks repeat them from its first.
/// </remarks>
internal static class ScrollingSkyChunks
{
    /// <summary>The chunks down the sky's span before the span repeats.</summary>
    private const int ChunkCount = 8;

    /// <summary>The land sky's own chunks, which show pages 0 through 4.</summary>
    private const int LandChunks = 5;

    /// <summary>The ocean sky's own chunks.</summary>
    private const int OceanChunks = 6;

    /// <summary>The page the ocean sky skips: its chunks from this one on show the page after.</summary>
    private const int OceanSkippedPage = 4;

    /// <summary>The page, 0 through 6, chunk <paramref name="chunk"/> of <paramref name="sky"/> shows.</summary>
    /// <param name="sky">The land or ocean sky.</param>
    /// <param name="chunk">The chunk down the sky's span, 0 through <see cref="ChunkCount"/> less one.</param>
    /// <returns>The page's position among the seven sky pages.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sky is unknown or the chunk lies outside the sky's span.</exception>
    internal static int PageOf(ScrollingSky sky, int chunk)
    {
        bool ocean = sky switch
        {
            ScrollingSky.Land => false,
            ScrollingSky.Ocean => true,
            _ => throw new ArgumentOutOfRangeException(nameof(sky), sky, "Unknown scrolling sky."),
        };
        if ((uint)chunk >= ChunkCount)
            throw new ArgumentOutOfRangeException(nameof(chunk), chunk, "The chunk lies outside the scrolling sky's span.");
        if (!ocean)
            return chunk % LandChunks;
        int own = chunk % OceanChunks;
        return own < OceanSkippedPage ? own : own + 1;
    }
}
