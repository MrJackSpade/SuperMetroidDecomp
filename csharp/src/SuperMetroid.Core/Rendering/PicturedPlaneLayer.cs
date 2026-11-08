namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A scrolled frontend background drawn from placed pictures (#1467): every picture's pixels at the layer's place in its
/// scene's order (#1514), clear pixels showing what lies behind unless <see cref="ShowsClearPixelColours"/> is set.
/// </summary>
/// <param name="Plane">The plane's pictures.</param>
/// <param name="Scroll">The plane's scroll in pixels, before the plane repeats.</param>
public sealed record PicturedPlaneLayer(PicturedPlane Plane, Position16<PlanePixels> Scroll) : DisplayLayer
{
    /// <summary>The plane's pictures.</summary>
    public PicturedPlane Plane
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Plane);

    /// <summary>
    /// Whether the pictures' clear pixels are drawn in the colour each picture gives them rather than showing what lies
    /// behind, as an opaque plane draws them (#1544); a clear pixel with no colour, and a place no picture covers, still
    /// show nothing.
    /// </summary>
    public bool ShowsClearPixelColours { get; init; }
}
