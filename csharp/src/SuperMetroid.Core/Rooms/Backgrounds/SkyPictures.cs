using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The scrolling sky as the background shows it in a frame (#1347, #1403, #1514): its image and the pictures of the cells the
/// room rebound (#1392), repeating every <see cref="SpanPixels"/> down and 256 across, and nothing on a band of the span that
/// shows no sky page.
/// </summary>
public sealed class SkyPictures
{
    /// <summary>Pixels down the span the sky covers before it repeats.</summary>
    public const int SpanPixels = 2048;

    /// <summary>Pixels down one band of the sky, which shows one page or none.</summary>
    public const int BandPixels = 256;

    /// <summary>The bands down the sky's span.</summary>
    public const int Bands = SpanPixels / BandPixels;

    /// <summary>Holds the sky's image and which of its bands <paramref name="variant"/>'s chunks show.</summary>
    public SkyPictures(RoomBackgroundImagePlane image, ScrollingSkyChunkTable variant)
    {
        Image = Ensure.NotNull(image);
        BandsShown = [.. Enumerable.Range(0, Bands).Select(band => ScrollingSkyChunks.TryGetPage(variant, band, out _))];
    }

    /// <summary>The sky's image and its rebound cells' pictures.</summary>
    public RoomBackgroundImagePlane Image { get; }

    /// <summary>Whether each band of the span, from its top, shows a sky page; the ocean table's last chunks show none.</summary>
    public ImmutableArray<bool> BandsShown { get; }
}
