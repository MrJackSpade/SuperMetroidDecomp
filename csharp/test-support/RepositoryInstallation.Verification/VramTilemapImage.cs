using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// What one background shows of a laid-out tilemap over a memory image's characters (#1467), decoded on the CPU as the
/// cartridge's screen draws it, for checks that a screen's pictures draw what its tilemaps draw: each screen pixel shows
/// the cell its scrolled position lands on, the map repeating at its edges, and that cell's character pixel, mirrored as
/// the cell mirrors it, in the colour its palette row and ink name; the backdrop shows where that pixel is colour zero or
/// the cell lies in a priority tier the layer does not draw.
/// </summary>
internal static class VramTilemapImage
{
    /// <summary>Cells along each side of one screen of a tilemap; a larger map keeps whole screens one after another.</summary>
    private const int ScreenCells = ScreenGeometry.TilemapScreenWidthInTiles;

    /// <summary>Pixels along a cell's side.</summary>
    private const int CellPixels = ScreenGeometry.BackgroundTileSizePixels;

    /// <summary>
    /// The cells of <paramref name="screens"/>, a <paramref name="size"/> tilemap laid out as the cartridge lays one out,
    /// whole 32-cell screens one after another, left to right then top to bottom, as rows from the top-left cell.
    /// </summary>
    /// <exception cref="ArgumentException">The tilemap does not hold every cell of the map, or the map is not whole screens.</exception>
    internal static BackgroundTile[] Rows(ReadOnlySpan<BackgroundTile> screens, Extent16<TilemapCells> size)
    {
        Ensure.LengthEqual(screens, size.X * size.Y);
        if (size.X % ScreenCells != 0 || size.Y % ScreenCells != 0)
            throw new ArgumentException($"A {size.X}x{size.Y} map is not whole {ScreenCells}-cell screens.", nameof(size));
        int screensAcross = size.X / ScreenCells;
        var cells = new BackgroundTile[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int screen = y / ScreenCells * screensAcross + x / ScreenCells;
                cells[y * size.X + x] = screens[screen * ScreenCells * ScreenCells + y % ScreenCells * ScreenCells + x % ScreenCells];
            }
        }
        return cells;
    }

    /// <summary>
    /// The screen a background shows of <paramref name="rows"/>, a <paramref name="size"/> tilemap row by row, scrolled by
    /// <paramref name="scroll"/>, over the <paramref name="depth"/> characters from <paramref name="characterWord"/> in
    /// <paramref name="image"/>, in <paramref name="colours"/>, a native 256-colour image; only the cells in the
    /// <paramref name="priority"/> tier draw, or every cell when it is null, and <paramref name="backdrop"/> shows elsewhere.
    /// </summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image, or the tilemap does not fill the map.</exception>
    internal static RgbaPixel[] Draw(ReadOnlySpan<BackgroundTile> rows, Extent16<TilemapCells> size, ReadOnlySpan<byte> image, int characterWord,
        TileInkDepth depth, ReadOnlySpan<RgbColor> colours, Position16<TilePlanePixels> scroll, bool? priority, RgbColor backdrop)
    {
        Ensure.LengthEqual(image, SnesVram.ByteCount);
        Ensure.LengthEqual(rows, size.X * size.Y);
        bool sixteen = depth == TileInkDepth.SixteenColours;
        int characterWords = sixteen ? 2 * CellPixels : CellPixels;
        int rowInks = sixteen ? 16 : 4;
        int width = size.X * CellPixels, height = size.Y * CellPixels;
        var screen = new RgbaPixel[ScreenGeometry.ScreenWidthPixels * ScreenGeometry.ScreenHeightPixels];
        for (int y = 0; y < ScreenGeometry.ScreenHeightPixels; y++)
        {
            for (int x = 0; x < ScreenGeometry.ScreenWidthPixels; x++)
            {
                int planeX = ((x + scroll.X) & ushort.MaxValue) % width, planeY = ((y + scroll.Y) & ushort.MaxValue) % height;
                BackgroundTile cell = rows[planeY / CellPixels * size.X + planeX / CellPixels];
                int ink = 0;
                if (priority is not { } tier || cell.HighPriority == tier)
                {
                    int column = cell.FlipHorizontally ? CellPixels - 1 - planeX % CellPixels : planeX % CellPixels;
                    int row = cell.FlipVertically ? CellPixels - 1 - planeY % CellPixels : planeY % CellPixels;
                    int first = characterWord + cell.Tile * characterWords;
                    ink = Plane(image, first + row, column) | Plane(image, first + row, column, 1) << 1;
                    if (sixteen)
                        ink |= Plane(image, first + CellPixels + row, column) << 2 | Plane(image, first + CellPixels + row, column, 1) << 3;
                }
                screen[y * ScreenGeometry.ScreenWidthPixels + x] = (ink == 0 ? backdrop
                    : colours[GameplayColourPositionDefinitions.BackgroundSetsFirst + cell.ColourVariant * rowInks + ink]).ToRgbaPixel();
            }
        }
        return screen;
    }

    /// <summary>Bit <paramref name="column"/> from the left of byte <paramref name="half"/> of character word <paramref name="word"/>, VRAM addresses wrapping.</summary>
    private static int Plane(ReadOnlySpan<byte> image, int word, int column, int half = 0) =>
        image[SnesVramAddressingConstants.ByteOffsetOfWord(word) + half] >> (CellPixels - 1 - column) & 1;
}
