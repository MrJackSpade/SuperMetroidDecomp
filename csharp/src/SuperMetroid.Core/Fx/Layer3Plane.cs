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
            int row = firstRow + scrolled.Y / SnesPpuLayout.BackgroundTileSizePixels % rows;
            int column = scrolled.X / SnesPpuLayout.BackgroundTileSizePixels % Columns;
            sample = BgTileSample.At(plane.WordAt(row, column), scrolled);
            return true;
        }
    }
}
