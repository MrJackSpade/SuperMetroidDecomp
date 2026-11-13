namespace SuperMetroid.Core.Assets;

/// <summary>
/// One whole-frame sprite entry of a published frame: an actor or display element shown at an origin. Backends draw it
/// as one whole picture, assembling nothing.
/// </summary>
public abstract record SpriteDraw
{
    /// <summary>Keeps what every kind of draw has.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="origin">The frame origin in screen pixels; each coordinate lies from -32768 through 32767.</param>
    private protected SpriteDraw(SpriteSource source, Position16<ScreenPixels> origin)
    {
        Source = source;
        Origin = origin;
    }

    /// <summary>The actor or display element shown.</summary>
    public SpriteSource Source { get; }
    /// <summary>The frame origin in screen pixels; each coordinate lies from -32768 through 32767.</summary>
    public Position16<ScreenPixels> Origin { get; }
}
