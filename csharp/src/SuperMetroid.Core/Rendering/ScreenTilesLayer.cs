namespace SuperMetroid.Core.Rendering;

/// <summary>A scrolled frontend tile layer (#1403): its cells, drawn from their tiles' pre-coloured pictures.</summary>
/// <param name="Cells">The layer's cells and the tiles they show.</param>
/// <param name="Scroll">The layer's scroll in pixels, before the map wraps.</param>
/// <param name="Priority">Selects only cells of one priority tier, or null for both.</param>
public sealed record ScreenTilesLayer(ScreenTileMap Cells, Position16<TilePlanePixels> Scroll, bool? Priority) : DisplayLayer
{
    /// <summary>The layer's cells and the tiles they show.</summary>
    public ScreenTileMap Cells
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Cells);

    /// <summary>The map's size in eight-pixel cells.</summary>
    public Extent16<TilemapCells> MapSize => Cells.Size;
}
