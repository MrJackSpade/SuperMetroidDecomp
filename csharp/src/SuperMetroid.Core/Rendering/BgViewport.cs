using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>The part of a background plane a viewport shows: its scroll, screen size, tilemap dimensions and optional per-line horizontal scroll (#633).</summary>
public readonly record struct BgViewport
{
    /// <summary>Scroll.</summary>
    public required Position16<BgPlanePixels> Scroll { get; init; }

    /// <summary>Size.</summary>
    public required Extent16<ScreenPixels> Size { get; init; }

    /// <summary>Tilemap size.</summary>
    public Extent16<TilemapCells>? TilemapSize { get; init; }

    /// <summary>Horizontal scroll by line.</summary>
    public IReadOnlyList<ushort>? HorizontalScrollByLine { get; init; }
}
