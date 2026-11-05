namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A one-screen pictured plane that glows over the screen (#1467, #1514): its colours are added to what the layers behind
/// it show, each channel clamped at full. Unless the layer names what it glows over, the glow covers the whole screen, the
/// backdrop included; when it names a plane, it covers only where that plane is opaque, and when it names a sprite layer,
/// where one of those sprites shows, the glow covers it if the sprite takes part in blends and leaves it otherwise.
/// </summary>
/// <param name="Plane">The glowing plane's pictures, a 32-by-32-cell plane.</param>
/// <param name="GlowsOver">The plane the glow covers where it is opaque, or null for a glow that names no plane.</param>
/// <param name="GlowsOverSprites">
/// The sprite layer, one of the scene's layers, whose sprites the glow covers where they show, or null for none.
/// </param>
/// <param name="VerticalScroll">Vertical scroll in pixels, every line's.</param>
public sealed record AddedPicturedPlaneLayer(PicturedPlane Plane, PicturedPlaneLayer? GlowsOver = null,
    SpritesLayer? GlowsOverSprites = null, int VerticalScroll = 0) : DisplayLayer
{
    /// <summary>The glowing plane's pictures.</summary>
    /// <exception cref="ArgumentException">The plane is not one screen.</exception>
    public PicturedPlane Plane
    {
        get;
        init => field = RequireScreen(value);
    } = RequireScreen(Plane);

    /// <summary>The largest scroll.</summary>
    public const int MaximumScroll = ushort.MaxValue;

    /// <summary>The one-screen plane, in cells, a glowing layer shows.</summary>
    public static readonly Extent16<TilemapCells> MapSize = new(32, 32);

    /// <summary>Vertical scroll in pixels, every line's, 0 through <see cref="MaximumScroll"/>.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, MaximumScroll, VerticalScroll);

    /// <summary>
    /// The sine wave that ripples the glowing plane sideways (#1468), or null for a horizontal scroll of zero on every line;
    /// the plane repeats within its 256-by-256 pixels, and the wave does not move the sprites.
    /// </summary>
    public ScrollWave? HorizontalWave { get; init; }

    /// <summary>Requires a one-screen plane, the plane the glow shows.</summary>
    /// <exception cref="ArgumentException">The plane is not <see cref="MapSize"/>.</exception>
    private static PicturedPlane RequireScreen(PicturedPlane plane) => Ensure.NotNull(plane).Size == MapSize ? plane
        : throw new ArgumentException($"The layer adds a {MapSize.X}x{MapSize.Y}-cell plane, not {plane.Size.X}x{plane.Size.Y}.", nameof(plane));
}
