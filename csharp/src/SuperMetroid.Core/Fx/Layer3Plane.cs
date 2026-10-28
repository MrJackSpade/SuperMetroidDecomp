using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The BG3 plane a gameplay frame shows (#1348): 32 columns by 64 rows of tiles. The HUD's four rows sit at the top;
/// below them the room effect's area, every cell the cleared tile, with the effect's authored page from row 31 down. The
/// HUD band, liquids (the whole plane) and atmospheres (its lower 32 rows) all read it, so nothing is written into VRAM
/// for them.
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

    /// <summary>The cell the effect area is cleared to before its page is drawn: the blank character 78 in colour variant 6.</summary>
    public static BackgroundTile ClearedCell { get; } = new(78, 6, highPriority: false);

    /// <summary>The plane's tiles, row-major.</summary>
    private readonly BackgroundTile[] tiles;

    /// <summary>Takes every tile of the plane, row-major.</summary>
    /// <param name="tiles">Exactly <see cref="Columns"/> times <see cref="Rows"/> tiles.</param>
    public Layer3Plane(ReadOnlySpan<BackgroundTile> tiles)
    {
        Ensure.LengthEqual(tiles, Columns * Rows);
        this.tiles = tiles.ToArray();
    }

    /// <summary>
    /// The plane a room shows: the HUD's rows, then the effect area cleared to <see cref="ClearedCell"/> with the
    /// room effect's authored page from row <see cref="EffectPageFirstRow"/>, when the effect has one.
    /// </summary>
    /// <param name="hudRows">The HUD's four rows: its static top row, then the three rows <see cref="Hud.HudState"/> owns.</param>
    /// <param name="effectPage">The effect's authored page tiles, or empty when the effect shows none.</param>
    public static Layer3Plane Compose(ReadOnlySpan<BackgroundTile> hudRows, ReadOnlySpan<BackgroundTile> effectPage)
    {
        Ensure.LengthEqual(hudRows, HudRows * Columns);
        int pageStart = EffectPageFirstRow * Columns;
        if (effectPage.Length > Columns * Rows - pageStart)
            throw new ArgumentException($"An effect page of {effectPage.Length} tiles runs past the plane.", nameof(effectPage));
        var plane = new BackgroundTile[Columns * Rows];
        hudRows.CopyTo(plane);
        plane.AsSpan(hudRows.Length).Fill(ClearedCell);
        effectPage.CopyTo(plane.AsSpan(pageStart));
        return new Layer3Plane(plane);
    }

    /// <summary>The plane's tiles, row-major.</summary>
    public ReadOnlySpan<BackgroundTile> Tiles => tiles;
}
