using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The BG3 plane a gameplay frame shows (#1348): 32 columns by 64 rows of tilemap words. The HUD's four rows sit at
/// the top; below them the room effect's area, every word the cleared tile, with the effect's authored page from row
/// 31 down. The HUD band, liquids (the whole plane) and atmospheres (its lower 32 rows) all read it, so nothing is
/// written into VRAM for them.
/// </summary>
public sealed class Layer3Plane
{
    /// <summary>Tile columns; the plane repeats every 256 pixels across.</summary>
    public const int Columns = 32;

    /// <summary>Tile rows.</summary>
    public const int Rows = 64;

    /// <summary>The HUD's rows at the top of the plane.</summary>
    public const int HudRows = 4;

    /// <summary>The row the effect's authored page begins on (<c>$5BE0</c>).</summary>
    public const int EffectPageFirstRow = 31;

    /// <summary>The plane's tilemap words, row-major.</summary>
    private readonly ushort[] words;

    /// <summary>Takes every tilemap word of the plane, row-major.</summary>
    /// <param name="words">Exactly <see cref="Columns"/> times <see cref="Rows"/> words.</param>
    public Layer3Plane(ReadOnlySpan<ushort> words)
    {
        Ensure.LengthEqual(words, Columns * Rows);
        this.words = words.ToArray();
    }

    /// <summary>
    /// The plane a room shows: the HUD's rows, then the effect area cleared to <c>$184E</c> (<c>$82:E566</c>) with the
    /// room effect's authored page from row <see cref="EffectPageFirstRow"/>, when the effect has one.
    /// </summary>
    /// <param name="hudRows">The HUD's four rows: its static top row, then the three rows <see cref="Hud.HudState"/> owns.</param>
    /// <param name="effectPage">The effect's authored page words, or empty when the effect shows none.</param>
    public static Layer3Plane Compose(ReadOnlySpan<ushort> hudRows, ReadOnlySpan<ushort> effectPage)
    {
        Ensure.LengthEqual(hudRows, HudRows * Columns);
        int pageStart = EffectPageFirstRow * Columns;
        if (effectPage.Length > Columns * Rows - pageStart)
            throw new ArgumentException($"An effect page of {effectPage.Length} words runs past the plane.", nameof(effectPage));
        var plane = new ushort[Columns * Rows];
        hudRows.CopyTo(plane);
        plane.AsSpan(hudRows.Length).Fill(RoomFxRomData.Layer3.ClearTilemapWord);
        effectPage.CopyTo(plane.AsSpan(pageStart));
        return new Layer3Plane(plane);
    }

    /// <summary>The plane's tilemap words, row-major.</summary>
    public ReadOnlySpan<ushort> Words => words;

    /// <summary>
    /// Each word's picture in the room's BG3 tile word sheet, row-major, null where a word draws from its character;
    /// default before the plane's pictures are resolved (#1392).
    /// </summary>
    public ImmutableArray<BgTilePicture?> Pictures { get; private init; }

    /// <summary>The plane drawn from <paramref name="pictures"/>, one for each word.</summary>
    /// <exception cref="ArgumentException">There is not one picture slot for each word.</exception>
    public Layer3Plane WithPictures(ImmutableArray<BgTilePicture?> pictures)
    {
        if (pictures.IsDefault || pictures.Length != Columns * Rows)
            throw new ArgumentException($"The BG3 plane needs a picture slot for each of its {Columns * Rows} words.", nameof(pictures));
        return new Layer3Plane(words) { Pictures = pictures };
    }

    /// <summary>
    /// A window of the plane as BG3 shows it: from its words' pictures once resolved, otherwise from their characters; as
    /// <see cref="Window"/> lays it out.
    /// </summary>
    public IBgSource Shown(int firstRow, int rows) =>
        Pictures.IsDefault ? new BgTileLayer(Window(firstRow, rows)) : new PicturedWindow(this, Window(firstRow, rows), firstRow, rows);

    /// <summary>The word at a cell.</summary>
    /// <param name="row">Row, 0 through 63.</param>
    /// <param name="column">Column, 0 through 31.</param>
    public SnesBgTilemapWord WordAt(int row, int column) => new(words[row * Columns + column]);

    /// <summary>
    /// The plane as a background of <paramref name="rows"/> rows from <paramref name="firstRow"/>, repeating every 256
    /// pixels across and every <paramref name="rows"/> rows down.
    /// </summary>
    /// <param name="firstRow">The first row: 0 for the whole plane or the HUD, 32 for an atmosphere's page.</param>
    /// <param name="rows">32 or 64.</param>
    public IBgTileSource Window(int firstRow, int rows)
    {
        if (rows is not (32 or 64) || firstRow < 0 || firstRow + rows > Rows)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, $"Rows {firstRow}..{firstRow + rows - 1} are not a window of the plane.");
        return new PlaneWindow(this, firstRow, rows);
    }

    /// <summary>A window of the plane as a tile source.</summary>
    private sealed class PlaneWindow(Layer3Plane plane, int firstRow, int rows) : IBgTileSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
        {
            sample = BgTileSample.At(plane.WordAt(RowAt(scrolled, firstRow, rows), ColumnAt(scrolled)), scrolled);
            return true;
        }
    }

    /// <summary>A window of the plane drawn from its pictures, a word without one from its character.</summary>
    private sealed class PicturedWindow(Layer3Plane plane, IBgTileSource tiles, int firstRow, int rows) : IBgSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            if (plane.Pictures[RowAt(scrolled, firstRow, rows) * Columns + ColumnAt(scrolled)] is { } picture)
            {
                sample = BgSample.OfPicture(picture.Picture, new Position16<ImagePixels>(
                    (ushort)(picture.Corner.X + scrolled.X % SnesPpuLayout.BackgroundTileSizePixels),
                    (ushort)(picture.Corner.Y + scrolled.Y % SnesPpuLayout.BackgroundTileSizePixels)));
                return true;
            }
            tiles.TrySample(scrolled, out BgTileSample tile);
            sample = BgSample.OfTile(tile);
            return true;
        }
    }

    /// <summary>The plane row a window of <paramref name="rows"/> rows from <paramref name="firstRow"/> shows at a scrolled position.</summary>
    private static int RowAt(Position16<BgPlanePixels> scrolled, int firstRow, int rows) =>
        firstRow + scrolled.Y / SnesPpuLayout.BackgroundTileSizePixels % rows;

    /// <summary>The plane column a window shows at a scrolled position.</summary>
    private static int ColumnAt(Position16<BgPlanePixels> scrolled) => scrolled.X / SnesPpuLayout.BackgroundTileSizePixels % Columns;
}
