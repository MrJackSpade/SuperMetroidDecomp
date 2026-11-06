using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Layer positions and the background scroll offsets derived from them.
/// </summary>
/// <remarks>
/// This reproduces how the game derives background scroll from layer positions. The original row and column
/// tilemap streaming is not ported: backgrounds render from level data through the offsets here (#1346).
/// No desktop camera interpolation is hidden in this class. Every calculation below
/// computes its two axes independently, so whole-value updates preserve its results.
/// </remarks>
public sealed class BackgroundScrollState
{
    /// <summary>World-space foreground/camera position.</summary>
    public Position16<RoomPixels> ForegroundPosition { get; set; }

    /// <summary>Parallax-derived background position.</summary>
    public Position16<BackgroundPixels> BackgroundPosition { get; set; }

    /// <summary>
    /// The room state's per-axis background scroll. Before a room state sets it, both axes copy the foreground and stream,
    /// as the game's cleared mode bytes select.
    /// </summary>
    public BackgroundScrollModes BackgroundScrollModes { get; set; } = BackgroundScrollModes.FollowsForeground;

    /// <summary>Wrapped delta added to <see cref="ForegroundPosition"/> to form the foreground's scroll.</summary>
    /// <remarks>These offsets are separate values, independent of the second layer's scroll mode.</remarks>
    public Offset16<RoomPixels> ForegroundOffset { get; set; }

    /// <summary>Wrapped delta added to <see cref="BackgroundPosition"/> to form the background's scroll.</summary>
    public Offset16<BackgroundPixels> BackgroundOffset { get; set; }

    /// <summary>Background 1's scroll position.</summary>
    public Position16<RoomPixels> ForegroundScroll { get; private set; }

    /// <summary>Background 2's scroll position.</summary>
    public Position16<BackgroundPixels> BackgroundScroll { get; private set; }

    /// <summary>
    /// Publishes a room actor's direct writes to the background 2 scroll. Mother Brain
    /// uses fixed background room modes, so the ordinary scrolling pass intentionally leaves
    /// these values untouched after the actor supplies them.
    /// </summary>
    public void SetBackgroundScroll(Position16<BackgroundPixels> scroll) => BackgroundScroll = scroll;

    /// <summary>Publishes a room-main write to the foreground's vertical scroll mirror without moving the camera or streaming origin.</summary>
    public void SetForegroundVerticalScroll(int vertical) => ForegroundScroll = ForegroundScroll.WithY(vertical);

    /// <summary>
    /// Installs the door-opening offsets after a directional door setup has staged its
    /// off-screen foreground coordinate. The foreground keeps the source room's scroll while the door
    /// transition clears background's; all four offsets then remain active throughout the opening scroll.
    /// </summary>
    public void ConfigureDoorOpeningOffsets(
        Position16<RoomPixels> retainedForegroundScroll,
        Position16<RoomPixels> stagedForeground)
    {
        // State $0B preserves foreground's source-room mirrors (with the down-door one-pixel
        // adjustment already supplied by the caller) and clears both background mirrors before
        // the four offsets are derived. Keeping the scroll writes here makes the
        // returned offset state and the block snapshot one indivisible action.
        ForegroundScroll = retainedForegroundScroll;
        BackgroundScroll = Position16<BackgroundPixels>.Zero;
        ForegroundOffset = retainedForegroundScroll - stagedForeground;
        // The derivation deliberately subtracts the foreground for both background offsets: the
        // background offset words are the negated staged camera origin. This is not a typo:
        // the background's own position is combined with these words later.
        BackgroundOffset = new Offset16<BackgroundPixels>(
            CoordinateAxis.AsSigned(-stagedForeground.X),
            CoordinateAxis.AsSigned(-stagedForeground.Y));
    }

    /// <summary>
    /// Implements the two background position calculations made by each directional door
    /// setup after the door installs the destination camera coordinates. Fixed mode one
    /// deliberately retains its old background position, exactly like the original early return.
    /// </summary>
    /// <param name="destination">The foreground destination published to <see cref="ForegroundPosition"/>.</param>
    /// <param name="parallaxBasis">
    /// The foreground words the parallax calculation reads. An upward door calculates background from
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
    /// Implements the gameplay entry point: the scroll registers and the background parallax follow
    /// the foreground. A frozen frame changes nothing.
    /// </summary>
    public void StepScrolling(bool timeIsFrozen = false)
    {
        if (timeIsFrozen)
            return;
        CalculateScrolls();
    }

    /// <summary>
    /// Applies foreground offsets and derives each background axis from its scroll mode, preserving fixed-mode registers. Room entry
    /// runs it once before the first frame; gameplay runs it every unfrozen frame.
    /// </summary>
    public void CalculateScrolls()
    {
        ForegroundScroll = ForegroundPosition + ForegroundOffset;

        // Mode one means "do not scroll the background on this axis": the caller skips both the
        // background position write and the background scroll.
        if (TryCalculateBackgroundPosition(ForegroundPosition.X, BackgroundScrollModes.Horizontal, out int backgroundX))
        {
            BackgroundPosition = BackgroundPosition.WithX(backgroundX);
            BackgroundScroll = BackgroundScroll.WithX(CoordinateAxis.Wrap(backgroundX + BackgroundOffset.X));
        }

        if (TryCalculateBackgroundPosition(ForegroundPosition.Y, BackgroundScrollModes.Vertical, out int backgroundY))
        {
            BackgroundPosition = BackgroundPosition.WithY(backgroundY);
            BackgroundScroll = BackgroundScroll.WithY(CoordinateAxis.Wrap(backgroundY + BackgroundOffset.Y));
        }
    }

    /// <summary>
    /// Recomputes all four background scroll positions, for door transitions that already own the layer positions,
    /// without changing background parallax state.
    /// </summary>
    public void CalculateLayerScrolls()
    {
        ForegroundScroll = ForegroundPosition + ForegroundOffset;
        BackgroundScroll = BackgroundPosition + BackgroundOffset;
    }

    /// <summary>
    /// Copies, freezes, or parallax-scales one background axis as the room state's scroll selects. The foreground
    /// coordinate is a sixteen-bit word and so is the result: the parallax product keeps its own wrap.
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

        // The hardware implementation performs two 8x8 multiplies: factor*low contributes
        // its high product byte, and factor*high contributes its low product word. Together
        // they equal floor(position * factor / 256), modulo 16 bits.
        uint factor = (uint)scroll.Factor;
        result = CoordinateAxis.Wrap(EightEight.WholeOf(checked((uint)foregroundPosition) * factor));
        return true;
    }

}
