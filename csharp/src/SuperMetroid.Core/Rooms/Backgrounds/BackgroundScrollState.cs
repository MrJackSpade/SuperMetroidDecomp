using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The layers' positions and the scrolls the screen shows them at (#1555): the room pixel the foreground's view starts at
/// and the background pixel the background's starts at, both in the layer's own pixels with no offset of their own.
/// </summary>
/// <remarks>
/// This reproduces how the game derives the background's position from the camera, and when each scroll follows its
/// position: every unfrozen gameplay update, once at room entry, and at the steps a door's opening scroll recalculates
/// them. Backgrounds render from level data and installed images through these scrolls (#1346). No desktop camera
/// interpolation is hidden in this class. Every calculation below computes its two axes independently, so whole-value
/// updates preserve its results.
/// </remarks>
public sealed class BackgroundScrollState
{
    /// <summary>World-space foreground/camera position.</summary>
    public Position16<RoomPixels> ForegroundPosition { get; set; }

    /// <summary>Parallax-derived background position.</summary>
    public Position16<BackgroundPixels> BackgroundPosition { get; set; }

    /// <summary>
    /// The room state's per-axis background scroll. Before a room state sets it, both axes copy the foreground and stream,
    /// as the game's cleared mode values select.
    /// </summary>
    public BackgroundScrollModes BackgroundScrollModes { get; set; } = BackgroundScrollModes.FollowsForeground;

    /// <summary>The room pixel the foreground's view starts at: its position at the last scroll calculation, or a room main's shift.</summary>
    public Position16<RoomPixels> ForegroundScroll { get; private set; }

    /// <summary>The background pixel the background's view starts at: its position at the last scroll calculation, or a boss's placement.</summary>
    public Position16<BackgroundPixels> BackgroundScroll { get; private set; }

    /// <summary>
    /// Places the background's view where a boss that shows its body on the background puts it (Phantoon). His room's
    /// fixed background axes leave the view where he put it during the ordinary scroll calculation.
    /// </summary>
    public void PlaceBackgroundView(Position16<BackgroundPixels> placement) => BackgroundScroll = placement;

    /// <summary>Shifts the foreground's view to row <paramref name="vertical"/> without moving the camera (a room main's rumble).</summary>
    public void SetForegroundVerticalScroll(int vertical) => ForegroundScroll = ForegroundScroll.WithY(vertical);

    /// <summary>
    /// Begins a door's opening scroll with the destination room's views at <paramref name="stagedForeground"/>, the
    /// destination pixel the source room's screen will lie at, before the setup's first step: the screen shows the source
    /// room's screen there and the destination scrolls in around it.
    /// </summary>
    public void BeginDoorOpeningViews(Position16<RoomPixels> stagedForeground)
    {
        ForegroundScroll = stagedForeground;
        BackgroundScroll = new Position16<BackgroundPixels>(stagedForeground.X, stagedForeground.Y);
    }

    /// <summary>
    /// Implements the two background position calculations made by each directional door
    /// setup after the door installs the destination camera coordinates. Fixed mode one
    /// deliberately retains its old background position, exactly like the game's early return.
    /// </summary>
    /// <param name="destination">The foreground destination published to <see cref="ForegroundPosition"/>.</param>
    /// <param name="parallaxBasis">
    /// The foreground position the parallax calculation reads. An upward door calculates background from
    /// destination+31 before the scroll's foreground endpoint becomes destination+32.
    /// </param>
    public void PrepareDoorOpeningDestination(Position16<RoomPixels> destination, Position16<RoomPixels> parallaxBasis)
    {
        if (TryCalculateBackgroundPosition(parallaxBasis.X, BackgroundScrollModes.Horizontal, out int backgroundX))
            BackgroundPosition = BackgroundPosition.WithX(backgroundX);
        if (TryCalculateBackgroundPosition(parallaxBasis.Y, BackgroundScrollModes.Vertical, out int backgroundY))
            BackgroundPosition = BackgroundPosition.WithY(backgroundY);
        ForegroundPosition = destination;
    }

    /// <summary>
    /// Implements the gameplay entry point: the layer scrolls and the background parallax follow
    /// the foreground. A frozen frame changes nothing.
    /// </summary>
    public void StepScrolling(bool timeIsFrozen = false)
    {
        if (timeIsFrozen)
            return;
        CalculateScrolls();
    }

    /// <summary>
    /// The foreground's view follows its position, and each background axis its scroll mode, keeping a fixed axis's view.
    /// Room entry runs it once before the first frame; gameplay runs it every unfrozen frame.
    /// </summary>
    public void CalculateScrolls()
    {
        ForegroundScroll = ForegroundPosition;

        // Mode one means "do not scroll the background on this axis": the caller skips both the
        // background position write and the background's view.
        if (TryCalculateBackgroundPosition(ForegroundPosition.X, BackgroundScrollModes.Horizontal, out int backgroundX))
        {
            BackgroundPosition = BackgroundPosition.WithX(backgroundX);
            BackgroundScroll = BackgroundScroll.WithX(backgroundX);
        }

        if (TryCalculateBackgroundPosition(ForegroundPosition.Y, BackgroundScrollModes.Vertical, out int backgroundY))
        {
            BackgroundPosition = BackgroundPosition.WithY(backgroundY);
            BackgroundScroll = BackgroundScroll.WithY(backgroundY);
        }
    }

    /// <summary>
    /// Both views follow their layers' positions, for door transitions that already own the layer positions, without
    /// changing background parallax state.
    /// </summary>
    public void CalculateLayerScrolls()
    {
        ForegroundScroll = ForegroundPosition;
        BackgroundScroll = BackgroundPosition;
    }

    /// <summary>
    /// Copies, freezes, or parallax-scales one background axis as the room state's scroll selects. The foreground
    /// coordinate wraps past 65535 to 0 and so does the result: the parallax product keeps its own wrap.
    /// </summary>
    private static bool TryCalculateBackgroundPosition(int foregroundPosition, BackgroundAxisScroll scroll, out int result)
    {
        switch (scroll.Kind)
        {
            case BackgroundScrollKind.Fixed:
                result = 0;
                return false;
            case BackgroundScrollKind.FollowsForeground:
                result = foregroundPosition;
                return true;
            case BackgroundScrollKind.Parallax:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BackgroundScrollKind)} {scroll.Kind}.");
        }

        // The parallax position is floor(position * factor / 256), wrapped to the coordinate's sixteen bits.
        uint factor = (uint)scroll.Factor;
        result = CoordinateAxis.Wrap(EightEight.WholeOf(checked((uint)foregroundPosition) * factor));
        return true;
    }

}
