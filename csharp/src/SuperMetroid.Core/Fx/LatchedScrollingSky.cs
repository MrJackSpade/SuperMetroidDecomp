namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky as an accepted frame shows it: layer 2's vertical scroll, which follows the camera
/// (<c>$88:AFB2</c>), and whether the sky shows its rows in bands on the tall layer-2 map. The bands'
/// horizontal scrolls are read while the frame is drawn and are not part of this latch.
/// </summary>
/// <param name="ShowsBands">Whether the sky shows its rows in horizontally scrolled bands on the tall layer-2 map.</param>
/// <param name="Bg2Scroll">
/// The layer-2 scroll the sky latches. Only <see cref="Position16{TSpace}.Y"/> is the sky's: the
/// layer-one Y it copies, kept with its sixteen-bit wrap. X is zero because the sky never sets a
/// uniform horizontal scroll; its bands scroll instead.
/// </param>
public readonly record struct LatchedScrollingSky(bool ShowsBands, Position16<BackgroundPixels> Bg2Scroll)
{
    /// <summary>The sky as it stands, or null when the room has no scrolling sky.</summary>
    public static LatchedScrollingSky? Capture(ScrollingSkyState? sky) =>
        sky is null ? null : new(sky.HasBands, sky.Bg2Scroll);
}
