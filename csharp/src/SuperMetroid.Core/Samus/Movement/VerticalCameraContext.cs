namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Scroller distances used by vertical target selection.</summary>
/// <param name="YDirection">Samus's vertical direction this frame.</param>
/// <param name="UpScroller">The room's up-scroller distance in pixels, zero or more.</param>
/// <param name="DownScroller">The room's down-scroller distance in pixels, zero or more.</param>
public readonly record struct VerticalCameraContext(
    SamusVerticalDirection YDirection,
    int UpScroller,
    int DownScroller)
{
    /// <summary>
    /// The room's up-scroller: how many pixels above Samus the camera's top edge sits while she is not
    /// moving up. A distance, so a negative one is rejected.
    /// </summary>
    public int UpScroller { get; } = Ensure.AtLeastZero(UpScroller);

    /// <summary>
    /// The room's down-scroller: how many pixels above Samus the camera's top edge sits while she moves up.
    /// A distance, so a negative one is rejected.
    /// </summary>
    public int DownScroller { get; } = Ensure.AtLeastZero(DownScroller);
}
