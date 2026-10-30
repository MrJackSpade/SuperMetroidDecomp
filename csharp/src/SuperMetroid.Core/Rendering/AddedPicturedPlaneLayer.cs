namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Adds a one-screen pictured plane's colours onto the screen (#1467); an optional covering plane limits the addition to
/// where it is opaque. Sprites stay where the screen places them.
/// </summary>
/// <param name="Plane">The added plane's pictures, a 32-by-32-cell plane.</param>
/// <param name="MainCoverage">An optional plane shown on the screen; the addition applies only where it is opaque, and everywhere when null.</param>
/// <param name="IncludeObjects">Whether the sprites are drawn into the added plane, at their depths among its pictures, before its colours are added.</param>
/// <param name="MainObjects">Whether, with a covering plane, the sprites shown on the screen, and whether they blend, also decide where the addition applies; no effect without that plane.</param>
/// <param name="VerticalScroll">Uniform vertical scroll in pixels when per-line scrolls are absent.</param>
[IndependentFlag("independent settings of the layer: whether the sprites are drawn into it, and whether they decide where it adds")]
public sealed record AddedPicturedPlaneLayer(PicturedPlane Plane,
    PicturedPlaneLayer? MainCoverage = null, bool IncludeObjects = false,
    bool MainObjects = false, int VerticalScroll = 0) : DisplayLayer
{
    /// <summary>The added plane's pictures.</summary>
    /// <exception cref="ArgumentException">The plane is not one screen.</exception>
    public PicturedPlane Plane
    {
        get;
        init => field = RequireScreen(value);
    } = RequireScreen(Plane);

    /// <summary>The largest scroll.</summary>
    public const int MaximumScroll = ushort.MaxValue;

    /// <summary>The one-screen plane, in cells, an added layer reads.</summary>
    public static readonly Extent16<TilemapCells> MapSize = new(32, 32);

    /// <summary>Uniform vertical scroll in pixels, 0 through <see cref="MaximumScroll"/>.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, MaximumScroll, VerticalScroll);

    /// <summary>Copied per-line scrolls; an empty array selects the uniform scroll.</summary>
    private Position16<TilePlanePixels>[] scrolls = [];
    /// <summary>Owned physical-line scrolls; empty means the uniform scroll.</summary>
    public ReadOnlySpan<Position16<TilePlanePixels>> Scrolls => scrolls;

    /// <summary>Returns a copied layer with independently owned per-line scrolls; these replace the uniform scroll and do not move the sprites.</summary>
    /// <param name="value">Exactly 224 scrolls, one per screen line from the top; the plane repeats within its 256-by-256 pixels.</param>
    /// <returns>A record copy retaining the other layer settings and a new scroll array.</returns>
    /// <exception cref="ArgumentException">The scroll count differs from the visible screen height.</exception>
    public AddedPicturedPlaneLayer WithScrolls(ReadOnlySpan<Position16<TilePlanePixels>> value)
    {
        Ensure.LengthEqual(value, ScreenGeometry.ScreenHeightPixels);
        var copy = this with { };
        copy.scrolls = value.ToArray();
        return copy;
    }

    /// <summary>Requires a one-screen plane, the plane the addition reads.</summary>
    /// <exception cref="ArgumentException">The plane is not <see cref="MapSize"/>.</exception>
    private static PicturedPlane RequireScreen(PicturedPlane plane) => Ensure.NotNull(plane).Size == MapSize ? plane
        : throw new ArgumentException($"The layer adds a {MapSize.X}x{MapSize.Y}-cell plane, not {plane.Size.X}x{plane.Size.Y}.", nameof(plane));
}
