using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect's part of the BG3 plane a gameplay frame shows (#1348): 32 columns by 64 rows of tiles, every cell the
/// cleared tile, with the effect's authored page from row 31 down. The HUD's images cover its top four rows (#1466): the
/// frame's plane takes those rows' pictures from the HUD, so the HUD band, liquids (the whole plane) and atmospheres (its
/// lower 32 rows) all read one plane, and nothing is written into VRAM for them.
/// </summary>
public sealed class Layer3Plane
{
    /// <summary>Tile columns; the plane repeats every 256 pixels across.</summary>
    public const int Columns = 32;

    /// <summary>Tile rows.</summary>
    public const int Rows = 64;

    /// <summary>The rows at the top of the plane the HUD's images cover.</summary>
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
    /// The plane a room shows: every cell cleared to <see cref="ClearedCell"/>, with the room effect's authored page from
    /// row <see cref="EffectPageFirstRow"/>, when the effect has one. The HUD's images cover the top
    /// <see cref="HudRows"/> rows.
    /// </summary>
    /// <param name="effectPage">The effect's authored page tiles, or empty when the effect shows none.</param>
    public static Layer3Plane Compose(ReadOnlySpan<BackgroundTile> effectPage)
    {
        int pageStart = EffectPageFirstRow * Columns;
        if (effectPage.Length > Columns * Rows - pageStart)
            throw new ArgumentException($"An effect page of {effectPage.Length} tiles runs past the plane.", nameof(effectPage));
        var plane = new BackgroundTile[Columns * Rows];
        plane.AsSpan().Fill(ClearedCell);
        effectPage.CopyTo(plane.AsSpan(pageStart));
        return new Layer3Plane(plane);
    }

    /// <summary>The plane's tiles, row-major.</summary>
    public ReadOnlySpan<BackgroundTile> Tiles => tiles;
}
