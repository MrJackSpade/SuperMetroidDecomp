namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky as an accepted frame shows it: the background's vertical scroll, which follows the camera
/// (it copies the foreground's). The bands' horizontal scrolls are read while the frame is drawn and are not part of this latch.
/// </summary>
/// <param name="BackgroundScroll">
/// The background scroll the sky latches. Only <see cref="Position16{TSpace}.Y"/> is the sky's: the
/// camera Y it copies, wrapping past row 65535 to row 0. X is zero because the sky never sets a
/// uniform horizontal scroll; its bands scroll instead.
/// </param>
public readonly record struct LatchedScrollingSky(Position16<BackgroundPixels> BackgroundScroll)
{
    /// <summary>The sky as it stands, or null when the room has no scrolling sky.</summary>
    public static LatchedScrollingSky? Capture(ScrollingSkyState? sky) =>
        sky is null ? null : new(sky.BackgroundScroll);
}
