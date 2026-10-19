using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One published background layer: source S showing a grid of cells read from owned artwork, placed at a screen
/// origin and shown on a band of scanlines. Backends draw it without VRAM; later layer kinds (room, sky, HUD, Mode 7,
/// boss bodies) publish through the same model.
/// </summary>
public sealed class BgLayer
{
    /// <summary>The cells, row by row.</summary>
    private readonly BgCell[] cells;

    /// <summary>Checks the grid against the artwork, so a bad binding fails where it is made.</summary>
    /// <param name="source">The element shown.</param>
    /// <param name="artwork">The characters the cells read.</param>
    /// <param name="width">Cells per row.</param>
    /// <param name="cells">The cells, row by row.</param>
    /// <param name="origin">Screen position of the top-left cell's top-left pixel.</param>
    /// <param name="firstScanline">First scanline the layer shows.</param>
    /// <param name="endScanline">Scanline after the last one the layer shows.</param>
    /// <exception cref="ArgumentException">The grid is ragged, or a cell reads past the artwork.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A palette or the scanline band is out of range.</exception>
    public BgLayer(BgLayerSource source, BgArtwork artwork, int width, ImmutableArray<BgCell> cells,
        Position16<ScreenPixels> origin, int firstScanline, int endScanline)
    {
        Ensure.NotNull(artwork);
        if (cells.IsDefaultOrEmpty || width <= 0 || cells.Length % width != 0)
            throw new ArgumentException($"Background layer {source} needs whole rows of {width} cells.", nameof(cells));
        if (firstScanline < 0 || endScanline < firstScanline)
            throw new ArgumentOutOfRangeException(nameof(firstScanline), firstScanline, $"Scanlines {firstScanline}..{endScanline} are not a band.");
        foreach (BgCell cell in cells)
        {
            if ((uint)cell.Character >= (uint)artwork.CharacterCount)
                throw new ArgumentException($"Background layer {source} reads character {cell.Character}; artwork {artwork.Id} has {artwork.CharacterCount}.", nameof(cells));
            if (cell.Palette is < 0 or > 7)
                throw new ArgumentOutOfRangeException(nameof(cells), cell.Palette, "Background palette rows are 0..7.");
        }
        Source = source;
        Artwork = artwork;
        Size = new Extent16<TilemapCells>(checked((ushort)width), checked((ushort)(cells.Length / width)));
        this.cells = System.Runtime.InteropServices.ImmutableCollectionsMarshal.AsArray(cells)
            ?? throw new ArgumentException("A background layer needs cells.", nameof(cells));
        Origin = origin;
        FirstScanline = firstScanline;
        EndScanline = endScanline;
    }

    /// <summary>The element shown.</summary>
    public BgLayerSource Source { get; }
    /// <summary>The characters the cells read.</summary>
    public BgArtwork Artwork { get; }
    /// <summary>Cells per row (X) and rows of cells (Y).</summary>
    public Extent16<TilemapCells> Size { get; }
    /// <summary>The cells, row by row.</summary>
    public ImmutableArray<BgCell> Cells => System.Runtime.InteropServices.ImmutableCollectionsMarshal.AsImmutableArray(cells);
    /// <summary>Screen position of the top-left cell's top-left pixel.</summary>
    public Position16<ScreenPixels> Origin { get; }
    /// <summary>First scanline the layer shows.</summary>
    public int FirstScanline { get; }
    /// <summary>Scanline after the last one the layer shows.</summary>
    public int EndScanline { get; }

    /// <summary>The cell at a grid position.</summary>
    /// <param name="cell">Cell column (X) and row (Y).</param>
    public BgCell CellAt(Position16<TilemapCells> cell) => cells[cell.Y * Size.X + cell.X];

    /// <summary>
    /// The palette color index (0 transparent) the layer shows at a screen pixel: the cell's palette row times the
    /// row size plus the character's color, or 0 outside the grid or band.
    /// </summary>
    /// <param name="pixel">The screen pixel.</param>
    public int PaletteIndexAt(Position16<ScreenPixels> pixel)
    {
        if (pixel.Y < FirstScanline || pixel.Y >= EndScanline)
            return 0;
        int layerX = pixel.X - unchecked((short)Origin.X), layerY = pixel.Y - unchecked((short)Origin.Y);
        if (layerX < 0 || layerY < 0 || layerX >= Size.X * BgArtwork.CharacterPixels || layerY >= Size.Y * BgArtwork.CharacterPixels)
            return 0;
        BgCell cell = CellAt(new Position16<TilemapCells>(
            (ushort)(layerX / BgArtwork.CharacterPixels), (ushort)(layerY / BgArtwork.CharacterPixels)));
        int px = layerX % BgArtwork.CharacterPixels, py = layerY % BgArtwork.CharacterPixels;
        if ((cell.Flip & SnesTileFlipFlags.Horizontal) != 0) px = BgArtwork.CharacterPixels - 1 - px;
        if ((cell.Flip & SnesTileFlipFlags.Vertical) != 0) py = BgArtwork.CharacterPixels - 1 - py;
        byte color = Artwork.ColorIndex(cell.Character, new Position16<CharacterPixels>((ushort)px, (ushort)py));
        return color == 0 ? 0 : cell.Palette * (1 << Artwork.BitsPerPixel) + color;
    }
}
