namespace SuperMetroid.Core.Rendering;

/// <summary>A 256-pixel-wide window into a wrapping 32-row frontend tile layer (#1403), drawn from its tiles' pre-coloured pictures.</summary>
/// <param name="Cells">The layer's cells and the tiles they show, a 32-by-32 map.</param>
/// <param name="VerticalScroll">Vertical scroll in pixels, before the map wraps.</param>
/// <param name="TransparentColorZero">Whether pen zero is transparent.</param>
/// <param name="Priority">Draws only the cells placed in front (true) or at the back (false), or null for both.</param>
[IndependentFlag("the layer's colour-zero transparency, an independent setting")]
public sealed record ScreenTilesViewportLayer(ScreenTileMap Cells, int VerticalScroll, bool TransparentColorZero, bool? Priority) : DisplayLayer
{
    /// <summary>The largest scroll.</summary>
    public const int MaximumScroll = ushort.MaxValue;

    /// <summary>The map the window looks into.</summary>
    public static readonly Extent16<TilemapCells> MapSize = new(32, 32);

    /// <summary>The layer's cells and the tiles they show.</summary>
    /// <exception cref="ArgumentException">The map is not <see cref="MapSize"/>.</exception>
    public ScreenTileMap Cells
    {
        get;
        init => field = RequireScreen(value);
    } = RequireScreen(Cells);

    /// <summary>Vertical scroll in pixels, 0 through <see cref="MaximumScroll"/>; the sampler wraps it at the map height.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, MaximumScroll, VerticalScroll);

    /// <summary>Requires a one-screen map, the map the layer looks into.</summary>
    /// <exception cref="ArgumentException">The map is not <see cref="MapSize"/>.</exception>
    internal static ScreenTileMap RequireScreen(ScreenTileMap cells) => Ensure.NotNull(cells).Size == MapSize ? cells
        : throw new ArgumentException($"The layer looks into a {MapSize.X}x{MapSize.Y} map, not {cells.Size.X}x{cells.Size.Y}.", nameof(cells));
}
