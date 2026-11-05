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
    public Position16<RoomPixels> Layer1Position { get; set; }

    /// <summary>Parallax-derived background position.</summary>
    public Position16<BackgroundPixels> Layer2Position { get; set; }

    /// <summary>
    /// The room state's per-axis layer-2 scroll. Before a room state sets it, both axes copy layer one and stream,
    /// as the game's cleared mode bytes select.
    /// </summary>
    public Layer2ScrollModes Layer2Scroll { get; set; } = Layer2ScrollModes.CopiesLayer1;

    /// <summary>Wrapped delta added to <see cref="Layer1Position"/> to form the foreground's scroll.</summary>
    /// <remarks>These offsets are separate values, independent of the second layer's scroll mode.</remarks>
    public Offset16<RoomPixels> ForegroundOffset { get; set; }

    /// <summary>Wrapped delta added to <see cref="Layer2Position"/> to form the background's scroll.</summary>
    public Offset16<BackgroundPixels> BackgroundOffset { get; set; }

    /// <summary>Background 1's scroll position.</summary>
    public Position16<RoomPixels> ForegroundScroll { get; private set; }

    /// <summary>Background 2's scroll position.</summary>
    public Position16<BackgroundPixels> BackgroundScroll { get; private set; }

    /// <summary>
    /// Publishes a room actor's direct writes to the background 2 scroll. Mother Brain
    /// uses fixed layer-two room modes, so the ordinary scrolling pass intentionally leaves
    /// these values untouched after the actor supplies them.
    /// </summary>
    public void SetBackgroundScroll(Position16<BackgroundPixels> scroll) => BackgroundScroll = scroll;

    /// <summary>Publishes a room-main write to the foreground's vertical scroll mirror without moving the camera or streaming origin.</summary>
    public void SetForegroundVerticalScroll(int vertical) => ForegroundScroll = ForegroundScroll.WithY(vertical);

    /// <summary>
    /// Installs the door-opening offsets after a directional door setup has staged its
    /// off-screen layer-one coordinate. The foreground keeps the source room's scroll while the door
    /// transition clears background's; all four offsets then remain active throughout the opening scroll.
    /// </summary>
    public void ConfigureDoorOpeningOffsets(
        Position16<RoomPixels> retainedForegroundScroll,
        Position16<RoomPixels> stagedLayer1)
    {
        // State $0B preserves foreground's source-room mirrors (with the down-door one-pixel
        // adjustment already supplied by the caller) and clears both background mirrors before
        // the four offsets are derived. Keeping the scroll writes here makes the
        // returned offset state and the block snapshot one indivisible action.
        ForegroundScroll = retainedForegroundScroll;
        BackgroundScroll = Position16<BackgroundPixels>.Zero;
        ForegroundOffset = retainedForegroundScroll - stagedLayer1;
        // The derivation deliberately subtracts layer 1 for both background offsets: the
        // The background offset words are the negated staged layer-one origin. This is not a typo:
        // layer 2's own position is combined with these words later.
        BackgroundOffset = new Offset16<BackgroundPixels>(
            CoordinateAxis.AsSigned(-stagedLayer1.X),
            CoordinateAxis.AsSigned(-stagedLayer1.Y));
    }

    /// <summary>
    /// Implements the two layer-two position calculations made by each directional door
    /// setup after the door installs the destination layer-one coordinates. Fixed mode one
    /// deliberately retains its old layer-two position, exactly like the original early return.
    /// </summary>
    /// <param name="destination">The layer-one destination published to <see cref="Layer1Position"/>.</param>
    /// <param name="parallaxBasis">
    /// The layer-one words the parallax calculation reads. An upward door calculates background from
    /// destination+31 before the scroll's layer-one endpoint becomes destination+32.
    /// </param>
    public void PrepareDoorOpeningDestination(Position16<RoomPixels> destination, Position16<RoomPixels> parallaxBasis)
    {
        if (TryCalculateLayer2Position(parallaxBasis.X, Layer2Scroll.Horizontal, out int layer2X))
            Layer2Position = Layer2Position.WithX(layer2X);
        if (TryCalculateLayer2Position(parallaxBasis.Y, Layer2Scroll.Vertical, out int layer2Y))
            Layer2Position = Layer2Position.WithY(layer2Y);
        Layer1Position = destination;
    }

    /// <summary>
    /// Implements the gameplay entry point: the scroll registers and layer-two parallax follow
    /// layer one. A frozen frame changes nothing.
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
        ForegroundScroll = Layer1Position + ForegroundOffset;

        // Mode one means "do not scroll layer 2 on this axis": the caller skips both the
        // layer-2 position write and the layer-2 scroll.
        if (TryCalculateLayer2Position(Layer1Position.X, Layer2Scroll.Horizontal, out int layer2X))
        {
            Layer2Position = Layer2Position.WithX(layer2X);
            BackgroundScroll = BackgroundScroll.WithX(CoordinateAxis.Wrap(layer2X + BackgroundOffset.X));
        }

        if (TryCalculateLayer2Position(Layer1Position.Y, Layer2Scroll.Vertical, out int layer2Y))
        {
            Layer2Position = Layer2Position.WithY(layer2Y);
            BackgroundScroll = BackgroundScroll.WithY(CoordinateAxis.Wrap(layer2Y + BackgroundOffset.Y));
        }
    }

    /// <summary>
    /// Recomputes all four background scroll positions, for door transitions that already own the layer positions,
    /// without changing background parallax state.
    /// </summary>
    public void CalculateLayerScrolls()
    {
        ForegroundScroll = Layer1Position + ForegroundOffset;
        BackgroundScroll = Layer2Position + BackgroundOffset;
    }

    /// <summary>
    /// Copies, freezes, or parallax-scales one layer-two axis as the room state's scroll selects. The layer-one
    /// coordinate is a sixteen-bit word and so is the result: the parallax product keeps its own wrap.
    /// </summary>
    private static bool TryCalculateLayer2Position(int layer1Position, Layer2AxisScroll scroll, out int result)
    {
        switch (scroll.Kind)
        {
            case Layer2ScrollKind.Fixed:
                result = 0;
                return false;
            case Layer2ScrollKind.CopiesLayer1:
                result = layer1Position;
                return true;
            case Layer2ScrollKind.Parallax:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(Layer2ScrollKind)} {scroll.Kind}.");
        }

        // The hardware implementation performs two 8x8 multiplies: factor*low contributes
        // its high product byte, and factor*high contributes its low product word. Together
        // they equal floor(position * factor / 256), modulo 16 bits.
        uint factor = (uint)scroll.Factor;
        result = CoordinateAxis.Wrap(EightEight.WholeOf(checked((uint)layer1Position) * factor));
        return true;
    }

}
