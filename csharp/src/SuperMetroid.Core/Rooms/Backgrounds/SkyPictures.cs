using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The scrolling sky as BG2 shows it in a frame (#1347, #1403): its image and the pictures of the cells the room rebound
/// (#1392), the row a vertical position selects repeating every 2048 pixels down and 256 across, and nothing on a row that
/// selects no sky page.
/// </summary>
public sealed class SkyPictures
{
    /// <summary>Tile rows over the 2048-pixel span the sky covers before it repeats.</summary>
    public const int Rows = 256;

    /// <summary>Tile rows in one chunk of the sky, which shows one page or none.</summary>
    private const int RowsPerChunk = 32;

    /// <summary>Holds the sky's image and which of its rows <paramref name="variant"/>'s chunks show.</summary>
    public SkyPictures(RoomBackgroundImagePlane image, ScrollingSkyChunkTable variant)
    {
        Image = Ensure.NotNull(image);
        RowsShown = [.. Enumerable.Range(0, Rows).Select(row => ScrollingSkyChunks.TryGetPage(variant, row / RowsPerChunk, out _))];
    }

    /// <summary>The sky's image and its rebound cells' pictures.</summary>
    public RoomBackgroundImagePlane Image { get; }

    /// <summary>Whether each tile row selects a sky page; the ocean table's last chunks select none.</summary>
    public ImmutableArray<bool> RowsShown { get; }
}
