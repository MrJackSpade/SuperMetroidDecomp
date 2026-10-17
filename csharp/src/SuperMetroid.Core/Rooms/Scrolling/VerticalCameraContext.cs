using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Scroller distances used by vertical target selection at <c>$90:9666</c>.</summary>
/// <param name="YDirection">Samus's vertical direction this frame.</param>
/// <param name="UpScroller">The room's up-scroller distance in pixels, 0 through 255.</param>
/// <param name="DownScroller">The room's down-scroller distance in pixels, 0 through 255.</param>
public readonly record struct VerticalCameraContext(
    SamusVerticalDirection YDirection,
    int UpScroller,
    int DownScroller)
{
    /// <summary>
    /// The room's up-scroller: how many pixels above Samus the camera's top edge sits while she is not
    /// moving up. A room-header byte, so 0 through 255; values outside that range are rejected.
    /// </summary>
    public int UpScroller { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, UpScroller);

    /// <summary>
    /// The room's down-scroller: how many pixels above Samus the camera's top edge sits while she moves up.
    /// A room-header byte, so 0 through 255; values outside that range are rejected.
    /// </summary>
    public int DownScroller { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, DownScroller);
}
