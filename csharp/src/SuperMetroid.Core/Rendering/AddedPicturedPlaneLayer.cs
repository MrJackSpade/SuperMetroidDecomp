namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A pictured plane that glows over the screen (#1467, #1514, #1555): its colours are added to what the layers behind it
/// show, each channel clamped at full. Unless the layer names what it glows over, the glow covers the whole screen, the
/// backdrop included; when it names a plane, it covers only where that plane is opaque, and when it names a sprite layer,
/// where one of those sprites shows, the glow covers it if the sprite takes part in blends and leaves it otherwise. The
/// plane does not repeat: a screen pixel none of its pictures covers adds nothing.
/// </summary>
/// <param name="Plane">The glowing plane's pictures.</param>
/// <param name="GlowsOver">The plane the glow covers where it is opaque, or null for a glow that names no plane.</param>
/// <param name="GlowsOverSprites">
/// The sprite layer, one of the scene's layers, whose sprites the glow covers where they show, or null for none.
/// </param>
/// <param name="VerticalScroll">The plane line shown on the screen's top line, every line's.</param>
public sealed record AddedPicturedPlaneLayer(PicturedPlane Plane, PicturedPlaneLayer? GlowsOver = null,
    SpritesLayer? GlowsOverSprites = null, int VerticalScroll = 0) : DisplayLayer
{
    /// <summary>The glowing plane's pictures.</summary>
    public PicturedPlane Plane
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Plane);

    /// <summary>
    /// The plane line shown on the screen's top line, every line's: a displacement in the plane's space, which holds it as
    /// it holds every picture's corner and every plane layer's scroll (<see cref="Offset16{TSpace}"/>, #1627).
    /// </summary>
    public int VerticalScroll
    {
        get;
        init => field = PlaneLine(value);
    } = PlaneLine(VerticalScroll);

    /// <summary>A vertical displacement in the plane's space, as the plane's offsets hold it.</summary>
    private static int PlaneLine(int line) => Offset16<PlanePixels>.Of(0, line).Y;

    /// <summary>
    /// The sine wave that ripples the glowing plane sideways (#1468), or null for a horizontal scroll of zero on every line;
    /// a line pushed sideways shows the plane's pixels at the pushed place, and the wave does not move the sprites.
    /// </summary>
    public ScrollWave? HorizontalWave { get; init; }
}
