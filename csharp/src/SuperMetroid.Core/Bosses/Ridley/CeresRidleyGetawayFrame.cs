namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One authored rotating getaway frame. <paramref name="Velocity"/> holds the signed values
/// of the X table (subtracted from the horizontal offset, so the image moves right) and
/// the Y table (added to the vertical offset).
/// </summary>
/// <param name="Zoom">The frame's turning-floor zoom.</param>
/// <param name="Velocity">The frame's drift across the floor.</param>
public readonly record struct CeresRidleyGetawayFrame(FloorZoom Zoom, Offset16<FloorPixels> Velocity)
{
    /// <summary>The frame after the last, which ends the getaway and carries no zoom or motion (#1413).</summary>
    public static CeresRidleyGetawayFrame Finished { get; } = new(default, Offset16<FloorPixels>.Zero) { IsFinished = true };

    /// <summary>Whether this is <see cref="Finished"/>, the frame that ends the getaway.</summary>
    public bool IsFinished { get; private init; }
}
