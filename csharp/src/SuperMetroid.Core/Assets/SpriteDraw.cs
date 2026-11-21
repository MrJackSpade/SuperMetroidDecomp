namespace SuperMetroid.Core.Assets;

/// <summary>
/// One whole-frame sprite entry of a published frame: an actor or display element shown at an origin. Backends draw it
/// as one whole picture, assembling nothing.
/// </summary>
public abstract partial record SpriteDraw
{
    /// <summary>Keeps what every kind of draw has.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="origin">Where on the screen the frame's origin lies; left of or above the screen is negative.</param>
    private protected SpriteDraw(SpriteSource source, Offset16<ScreenPixels> origin)
    {
        Source = source;
        Origin = origin;
    }

    /// <summary>The actor or display element shown.</summary>
    public SpriteSource Source { get; }
    /// <summary>
    /// Where on the screen the frame's origin lies, in pixels from the screen's top-left corner (#2000): left of or above
    /// the screen is negative.
    /// </summary>
    public Offset16<ScreenPixels> Origin { get; }

    /// <summary>
    /// The draw as a frame shows it (#2505): a draw that names its shading as it is, and a draw shown in Samus's colours
    /// in <paramref name="samusShading"/>, the shading her colours show as the frame publishes.
    /// </summary>
    /// <param name="samusShading">The shading Samus's colours show as the frame publishes.</param>
    internal abstract ImageSpriteDraw Published(PictureShading samusShading);
}
