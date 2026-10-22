namespace SuperMetroid.Core.Assets;

/// <summary>
/// One whole-frame sprite entry of a published frame: an actor or display element shown at an origin, on a layer
/// against the backgrounds. Backends draw it without OAM or VRAM.
/// </summary>
public abstract record SpriteDraw
{
    /// <summary>Checks the layer, so a bad draw fails where it is made.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="origin">The frame origin in screen pixels; each coordinate is a signed 16-bit value.</param>
    /// <param name="priority">Priority against the backgrounds 0..3.</param>
    /// <exception cref="ArgumentOutOfRangeException">The priority is out of range.</exception>
    private protected SpriteDraw(SpriteSource source, Position16<ScreenPixels> origin, int priority)
    {
        if (priority is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(priority), priority, "OBJ priorities are 0..3.");
        Source = source;
        Origin = origin;
        Priority = priority;
    }

    /// <summary>The actor or display element shown.</summary>
    public SpriteSource Source { get; }
    /// <summary>The frame origin in screen pixels; each coordinate is a signed 16-bit value.</summary>
    public Position16<ScreenPixels> Origin { get; }
    /// <summary>Priority against the backgrounds 0..3; validated by the constructor.</summary>
    public int Priority { get; }
}
