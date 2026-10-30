namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A scrolled frontend background drawn from placed pictures (#1467): each picture's pixels in the priority tier its
/// picture was drawn in, colour zero showing what lies behind unless <see cref="ShowsColourZero"/> is set.
/// </summary>
/// <param name="Plane">The plane's pictures.</param>
/// <param name="Scroll">The plane's scroll in pixels, before the plane repeats.</param>
/// <param name="Priority">Draws only the pixels in front (true) or at the back (false), or null for both.</param>
public sealed record PicturedPlaneLayer(PicturedPlane Plane, Position16<TilePlanePixels> Scroll, bool? Priority) : DisplayLayer
{
    /// <summary>The plane's pictures.</summary>
    public PicturedPlane Plane
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Plane);

    /// <summary>
    /// Whether the pictures' colour-zero pixels are drawn in their colour rather than showing what lies behind, as an
    /// opaque plane draws them; where no picture lies the plane still shows nothing.
    /// </summary>
    public bool ShowsColourZero { get; init; }
}
