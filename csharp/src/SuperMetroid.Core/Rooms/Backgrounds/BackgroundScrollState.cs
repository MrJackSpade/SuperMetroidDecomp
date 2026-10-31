using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Bank-$80 layer-position and PPU scroll-register state.
/// </summary>
/// <remarks>
/// This is a direct behavioral port of the scroll half of <c>$80:A2F9-$80:A527</c>. The cartridge's row and column
/// tilemap streaming is not ported: backgrounds render from level data through the offsets here (#1346).
/// No desktop camera interpolation is hidden in this class. Every native routine below
/// computes its two axes independently, so whole-value updates preserve its results.
/// </remarks>
public sealed class BackgroundScrollState
{
    /// <summary>World-space BG1/camera position (<c>$0911/$0915</c>).</summary>
    public Position16<RoomPixels> Layer1Position { get; set; }

    /// <summary>Parallax-derived BG2 position (<c>$0917/$0919</c>).</summary>
    public Position16<BackgroundPixels> Layer2Position { get; set; }

    /// <summary>
    /// The room state's per-axis layer-2 scroll. Before a room state sets it, both axes copy layer one and stream,
    /// as the cartridge's cleared mode bytes select.
    /// </summary>
    public Layer2ScrollModes Layer2Scroll { get; set; } = Layer2ScrollModes.CopiesLayer1;

    /// <summary>Wrapped delta added to <see cref="Layer1Position"/> to form BG1HOFS/BG1VOFS.</summary>
    /// <remarks>These offsets are separate values, independent of the second layer's scroll mode.</remarks>
    public Offset16<RoomPixels> Bg1Offset { get; set; }

    /// <summary>Wrapped delta added to <see cref="Layer2Position"/> to form BG2HOFS/BG2VOFS.</summary>
    public Offset16<BackgroundPixels> Bg2Offset { get; set; }

    /// <summary>Modeled BG1HOFS/BG1VOFS mirrors in direct-page words <c>$00B1/$00B3</c>.</summary>
    public Position16<RoomPixels> Bg1Scroll { get; private set; }

    /// <summary>Modeled BG2HOFS/BG2VOFS mirrors in direct-page words <c>$00B5/$00B7</c>.</summary>
    public Position16<BackgroundPixels> Bg2Scroll { get; private set; }

    /// <summary>
    /// Publishes a room actor's direct writes to the PPU BG2 scroll mirrors. Mother Brain
    /// uses fixed layer-two room modes, so the ordinary scrolling pass intentionally leaves
    /// these values untouched after the actor supplies them.
    /// </summary>
    public void SetBg2Scroll(Position16<BackgroundPixels> scroll) => Bg2Scroll = scroll;

    /// <summary>Publishes a room-main write to the BG1VOFS mirror without moving the camera or streaming origin.</summary>
    public void SetBg1VerticalScroll(int vertical) => Bg1Scroll = Bg1Scroll.WithY(vertical);

    /// <summary>
    /// Installs the door-opening offsets after a directional door setup has staged its
    /// off-screen layer-one coordinate. BG1 keeps the source room's scroll while the door
    /// transition clears BG2's; all four offsets then remain active throughout the opening scroll.
    /// </summary>
    public void ConfigureDoorOpeningOffsets(
        Position16<RoomPixels> retainedBg1Scroll,
        Position16<RoomPixels> stagedLayer1)
    {
        // State $0B preserves BG1's source-room mirrors (with the down-door one-pixel
        // adjustment already supplied by the caller) and clears both BG2 mirrors before
        // `$80:AE29` derives the four offsets. Keeping the register writes here makes the
        // returned offset state and the block snapshot one indivisible cartridge action.
        Bg1Scroll = retainedBg1Scroll;
        Bg2Scroll = Position16<BackgroundPixels>.Zero;
        Bg1Offset = retainedBg1Scroll - stagedLayer1;
        // `$80:AE3B-$80:AE4A` deliberately subtracts layer 1 for both BG2 offsets: the
        // BG2 offset words are the negated staged layer-one origin. This is not a typo:
        // layer 2's own position is combined with these words later.
        Bg2Offset = new Offset16<BackgroundPixels>(
            CoordinateAxis.AsSigned(-stagedLayer1.X),
            CoordinateAxis.AsSigned(-stagedLayer1.Y));
    }

    /// <summary>
    /// Implements the two <c>CalculateLayer2*pos</c> calls made by each directional door
    /// setup after bank $82 installs the destination layer-one coordinates. Fixed mode one
    /// deliberately retains its old layer-two word, exactly like the native early return.
    /// </summary>
    /// <param name="destination">The layer-one destination published to <see cref="Layer1Position"/>.</param>
    /// <param name="parallaxBasis">
    /// The layer-one words the parallax calculation reads. An upward door calculates BG2 from
    /// destination+$1F before the scroll's layer-one endpoint becomes destination+$20.
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
    /// Implements the gameplay entry point at <c>$80:A3AB</c>: the scroll registers and layer-two parallax follow
    /// layer one. A frozen frame changes nothing.
    /// </summary>
    public void StepScrolling(bool timeIsFrozen = false)
    {
        if (timeIsFrozen)
            return;
        CalculateScrolls();
    }

    /// <summary>
    /// Applies BG1 offsets and derives each BG2 axis from its scroll mode, preserving fixed-mode registers. Room entry
    /// runs it once before the first frame; gameplay runs it every unfrozen frame.
    /// </summary>
    public void CalculateScrolls()
    {
        Bg1Scroll = Layer1Position + Bg1Offset;

        // Mode one means "do not scroll BG2 on this axis". The 65C816 subroutine returns
        // carry set, so the caller skips both the layer-2 position write and PPU mirror.
        if (TryCalculateLayer2Position(Layer1Position.X, Layer2Scroll.Horizontal, out int layer2X))
        {
            Layer2Position = Layer2Position.WithX(layer2X);
            Bg2Scroll = Bg2Scroll.WithX(CoordinateAxis.Wrap(layer2X + Bg2Offset.X));
        }

        if (TryCalculateLayer2Position(Layer1Position.Y, Layer2Scroll.Vertical, out int layer2Y))
        {
            Layer2Position = Layer2Position.WithY(layer2Y);
            Bg2Scroll = Bg2Scroll.WithY(CoordinateAxis.Wrap(layer2Y + Bg2Offset.Y));
        }
    }

    /// <summary>
    /// Implements <c>$80:A37B</c>'s register half, used by door transitions that already own the layer positions:
    /// recomputes all four PPU scroll mirrors without changing BG2 parallax state.
    /// </summary>
    public void CalculateLayerScrolls()
    {
        Bg1Scroll = Layer1Position + Bg1Offset;
        Bg2Scroll = Layer2Position + Bg2Offset;
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
