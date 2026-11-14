namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A scrolled frontend background drawn from placed pictures (#1467, #1555): every picture's pixels at the layer's place in
/// its scene's order (#1514), clear pixels showing what lies behind unless the layer <see cref="DrawsOpaque"/>. The
/// plane does not repeat: a screen pixel no picture covers shows what lies behind.
/// </summary>
/// <param name="Plane">The plane's pictures.</param>
/// <param name="Scroll">The plane pixel shown at the screen's top-left corner: screen pixel (x, y) shows plane pixel (x + X, y + Y).</param>
public sealed record PicturedPlaneLayer(PicturedPlane Plane, Offset16<PlanePixels> Scroll) : DisplayLayer
{
    /// <summary>The plane's pictures.</summary>
    public PicturedPlane Plane
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Plane);

    /// <summary>
    /// Whether the layer is drawn opaque (#1544, #1633): its pictures' clear pixels show the colour each keeps under its
    /// alpha rather than what lies behind. A place no picture covers still shows what lies behind.
    /// </summary>
    public bool DrawsOpaque { get; init; }

    /// <summary>
    /// How the plane is pixelated (#1774): each square block of the screen shows the plane as its top-left pixel shows it,
    /// a clear pixel there leaving the whole block showing what lies behind. The default leaves the plane unpixelated.
    /// </summary>
    public BackgroundPixelation Pixelation { get; init; }
}
