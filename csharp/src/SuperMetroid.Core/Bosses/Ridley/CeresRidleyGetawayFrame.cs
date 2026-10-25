namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One authored Mode 7 getaway frame. <paramref name="Velocity"/> holds the native signed words
/// of the $A6:B00F X table (subtracted from the horizontal offset, so the image moves right) and
/// the $A6:AF2F Y table (added to the vertical offset).
/// </summary>
public readonly record struct CeresRidleyGetawayFrame(int Zoom, Offset16<FloorPixels> Velocity)
{
    /// <summary>The frame after the last, which ends the getaway and carries no zoom or motion (#1413).</summary>
    public static CeresRidleyGetawayFrame Finished { get; } = new(0, Offset16<FloorPixels>.Zero) { IsFinished = true };

    /// <summary>Whether this is <see cref="Finished"/>, the frame that ends the getaway.</summary>
    public bool IsFinished { get; private init; }

    /// <summary>The frame's Mode 7 zoom word, 0 through 65535.</summary>
    public int Zoom { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Zoom);
}
