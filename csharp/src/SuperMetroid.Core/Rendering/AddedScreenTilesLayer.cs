namespace SuperMetroid.Core.Rendering;

/// <summary>Adds a frontend tile layer's colours onto the screen (#1403); an optional covering layer limits the addition to where it is opaque. Sprites stay where the screen places them.</summary>
/// <param name="Cells">The added layer's cells and the tiles they show, a 32-by-32 map.</param>
/// <param name="MainCoverage">An optional layer shown on the screen; the addition applies only where it is opaque, and everywhere when null.</param>
/// <param name="IncludeObjects">Whether the sprites are drawn into the added layer, at their depths among its tiles, before its colours are added.</param>
/// <param name="MainObjects">Whether, with a covering layer, the sprites shown on the screen, and whether they blend, also decide where the addition applies; no effect without that layer.</param>
/// <param name="VerticalScroll">Uniform vertical scroll in pixels when per-line scrolls are absent.</param>
[IndependentFlag("independent settings of the layer: whether the sprites are drawn into it, and whether they decide where it adds")]
public sealed record AddedScreenTilesLayer(ScreenTileMap Cells,
    ScreenTilesLayer? MainCoverage = null, bool IncludeObjects = false,
    bool MainObjects = false, int VerticalScroll = 0) : DisplayLayer
{
    /// <summary>The added layer's cells and the tiles they show.</summary>
    /// <exception cref="ArgumentException">The map is not one screen.</exception>
    public ScreenTileMap Cells
    {
        get;
        init => field = RequireScreen(value);
    } = RequireScreen(Cells);

    /// <summary>The largest scroll.</summary>
    public const int MaximumScroll = ushort.MaxValue;

    /// <summary>The one-screen map an added layer reads.</summary>
    public static readonly Extent16<TilemapCells> MapSize = new(32, 32);

    /// <summary>Uniform vertical scroll in pixels, 0 through <see cref="MaximumScroll"/>.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, MaximumScroll, VerticalScroll);

    /// <summary>Requires a one-screen map, the map the addition reads.</summary>
    /// <exception cref="ArgumentException">The map is not <see cref="MapSize"/>.</exception>
    private static ScreenTileMap RequireScreen(ScreenTileMap cells) => Ensure.NotNull(cells).Size == MapSize ? cells
        : throw new ArgumentException($"The layer adds a {MapSize.X}x{MapSize.Y} map, not {cells.Size.X}x{cells.Size.Y}.", nameof(cells));

    /// <summary>Copied per-line scrolls; an empty array selects the uniform scroll.</summary>
    private readonly Position16<TilePlanePixels>[] scrolls = [];
    /// <summary>Owned physical-line scrolls; empty means the uniform scroll.</summary>
    public ReadOnlySpan<Position16<TilePlanePixels>> Scrolls => scrolls;
}
