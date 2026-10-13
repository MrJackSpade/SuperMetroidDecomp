using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$80 layer-position, PPU scroll-register, and 16-pixel streaming-block state.
/// </summary>
/// <remarks>
/// This is a direct behavioral port of <c>$80:A2F9-$80:A527</c>. It deliberately stops
/// at the four row/column update calls: each call is returned as a typed request so the
/// following level-data-to-VRAM translation can be implemented and debugged independently.
/// No desktop camera interpolation is hidden in this class. Every native routine below
/// computes its two axes independently, so whole-value updates preserve its results.
/// </remarks>
public sealed class BackgroundScrollState
{
    /// <summary>World-space BG1/camera position (<c>$0911/$0915</c>).</summary>
    public Position16<RoomPixels> Layer1Position { get; set; }

    /// <summary>Parallax-derived BG2 position (<c>$0917/$0919</c>).</summary>
    public Position16<Bg2Pixels> Layer2Position { get; set; }

    /// <summary>
    /// The room header's per-axis layer-2 scroll modes. Zero copies BG1; one freezes the axis;
    /// every other value uses its even portion as a fixed-point parallax multiplier over 128.
    /// </summary>
    public Layer2ScrollModes Layer2Scroll { get; set; }

    // These offsets are separate native variables despite the confusing adjacency of the
    // BG1 X offset and Layer2ScrollY in WRAM ($091F is addressed as Layer2ScrollY+1).
    /// <summary>Wrapped delta added to <see cref="Layer1Position"/> to form BG1HOFS/BG1VOFS.</summary>
    public Offset16<RoomPixels> Bg1Offset { get; set; }

    /// <summary>Wrapped delta added to <see cref="Layer2Position"/> to form BG2HOFS/BG2VOFS.</summary>
    public Offset16<Bg2Pixels> Bg2Offset { get; set; }

    /// <summary>Modeled BG1HOFS/BG1VOFS mirrors in direct-page words <c>$00B1/$00B3</c>.</summary>
    public Position16<RoomPixels> Bg1Scroll { get; private set; }

    /// <summary>Modeled BG2HOFS/BG2VOFS mirrors in direct-page words <c>$00B5/$00B7</c>.</summary>
    public Position16<Bg2Pixels> Bg2Scroll { get; private set; }

    // "Block" means one 16x16 level block here, not one 256x256 room-scroll cell.
    /// <summary>BG1 scroll converted from pixels to the 16-pixel circular-VRAM block coordinate.</summary>
    public Position16<TilemapBlocks> Bg1Block { get; private set; }

    /// <summary>BG2 scroll converted from pixels to the 16-pixel circular-VRAM block coordinate.</summary>
    public Position16<TilemapBlocks> Bg2Block { get; private set; }

    /// <summary>Layer-one world position converted to a 16-pixel source-data block coordinate.</summary>
    public Position16<RoomBlocks> Layer1Block { get; private set; }

    /// <summary>Layer-two world position converted to a 16-pixel source-data block coordinate.</summary>
    public Position16<RoomBlocks> Layer2Block { get; private set; }

    /// <summary>Prior layer-one source block used to detect a newly exposed tilemap column or row.</summary>
    public Position16<RoomBlocks> PreviousLayer1Block { get; private set; }

    /// <summary>Prior layer-two source block used to detect a newly exposed background column or row.</summary>
    public Position16<RoomBlocks> PreviousLayer2Block { get; private set; }

    /// <summary>Stores $FFFF to the previous layer-one X block, as Kraid's dead-room tail does.</summary>
    internal void ResetPreviousLayer1XBlock() => PreviousLayer1Block = PreviousLayer1Block.WithX(0xffff);

    /// <summary>
    /// Publishes a room actor's direct writes to the PPU BG2 scroll mirrors. Mother Brain
    /// uses fixed layer-two room modes, so the ordinary scrolling pass intentionally leaves
    /// these values untouched after the actor supplies them.
    /// </summary>
    public void SetBg2ScrollRegisters(Position16<Bg2Pixels> scroll) => Bg2Scroll = scroll;

    /// <summary>Publishes a room-main write to the BG1VOFS mirror without moving the camera or streaming origin.</summary>
    public void SetBg1VerticalScrollRegister(ushort vertical) => Bg1Scroll = Bg1Scroll.WithY(vertical);

    /// <summary>
    /// Implements <c>$80:AE29</c> after a directional door setup has staged its off-screen
    /// layer-one coordinate. BG1 retains the source-room PPU scroll while state $0B clears
    /// both BG2 registers; all four offsets then remain active throughout the opening IRQ.
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
        Bg2Scroll = Position16<Bg2Pixels>.Zero;
        Bg1Offset = retainedBg1Scroll - stagedLayer1;
        // `$80:AE3B-$80:AE4A` deliberately subtracts layer 1 for both BG2 offsets: the
        // BG2 offset words are the negated staged layer-one origin. This is not a typo:
        // layer 2's own position is combined with these words later.
        Bg2Offset = new Offset16<Bg2Pixels>(
            unchecked((short)-stagedLayer1.X),
            unchecked((short)-stagedLayer1.Y));
    }

    /// <summary>
    /// Implements the two <c>CalculateLayer2*pos</c> calls made by each directional door
    /// setup after bank $82 installs the destination layer-one coordinates. Fixed mode one
    /// deliberately retains its old layer-two word, exactly like the native early return.
    /// </summary>
    /// <param name="destination">The layer-one destination published to <see cref="Layer1Position"/>.</param>
    /// <param name="parallaxBasis">
    /// The layer-one words the parallax calculation reads. An upward door calculates BG2 from
    /// destination+$1F before the IRQ's layer-one endpoint becomes destination+$20.
    /// </param>
    public void PrepareDoorOpeningDestination(Position16<RoomPixels> destination, Position16<RoomPixels> parallaxBasis)
    {
        if (TryCalculateLayer2Position(parallaxBasis.X, Layer2Scroll.Horizontal, out ushort layer2X))
            Layer2Position = Layer2Position.WithX(layer2X);
        if (TryCalculateLayer2Position(parallaxBasis.Y, Layer2Scroll.Vertical, out ushort layer2Y))
            Layer2Position = Layer2Position.WithY(layer2Y);
        Layer1Position = destination;
    }

    /// <summary>
    /// Implements the gameplay entry point at <c>$80:A3AB</c>. A frozen frame performs no
    /// register, parallax, previous-block, or streaming-request changes.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> StepScrolling(bool timeIsFrozen = false)
    {
        if (timeIsFrozen)
            return Array.Empty<BackgroundUpdateRequest>();

        CalculateGameplayScrolls();
        return CalculateBlocksAndUpdates();
    }

    /// <summary>
    /// Implements <c>$80:A37B</c>, used by door transitions that already own layer-2
    /// positions and merely need all four PPU scroll mirrors recomputed.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> CalculateScrollsAndUpdates()
    {
        CalculateScrollRegisters();
        return CalculateBlocksAndUpdates();
    }

    /// <summary>
    /// Seeds the "previous" blocks after room/door setup so the first stationary
    /// gameplay frame does not request four phantom rows and columns.
    /// </summary>
    public void PrimePreviousBlocks()
    {
        // Room setup reaches this logical state through several native entry points. Doing
        // the gameplay calculation here makes the C# initialization operation atomic: the
        // saved layer-2 blocks correspond to the parallax position, not constructor zeroes.
        CalculateGameplayScrolls();
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();
    }

    /// <summary>
    /// Recreates the horizontal previous-block words left by
    /// <c>DoorTransitionScrollingSetup_Right/Left</c> after their built-in first
    /// four-pixel transition call.
    /// </summary>
    /// <remarks>
    /// The runtime stores the already-advanced +/-$FC coordinate atomically. Native first
    /// primes the +/-$100 coordinate, biases its previous-X word, and only then advances.
    /// Reconstructing those previous words is what makes the first streaming request select
    /// room column zero/last instead of wrapping to an unrelated word before level data.
    /// </remarks>
    public void PrimeHorizontalDoorOpeningBlocks(DoorDirection direction)
    {
        if (direction is not (DoorDirection.Right or DoorDirection.Left))
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Horizontal door direction must be right or left.");

        CalculateScrollRegisters();
        CalculateBlockCoordinates();

        var bias = new Offset16<RoomBlocks>(direction == DoorDirection.Right ? (short)-1 : (short)2, 0);
        PreviousLayer1Block = Layer1Block + bias;
        PreviousLayer2Block = Layer2Block + bias;
    }

    /// <summary>
    /// Executes <c>$80:AD1D</c>'s source-room row repair before an upward door replaces
    /// the active level data. The routine deliberately restores its layer positions and
    /// PPU mirrors but retains the row written by its temporary 16-pixel probe.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> FixDoorsMovingUp()
    {
        Position16<RoomPixels> savedLayer1 = Layer1Position;
        Position16<Bg2Pixels> savedLayer2 = Layer2Position;
        Position16<RoomPixels> savedBg1Scroll = Bg1Scroll;
        Position16<Bg2Pixels> savedBg2Scroll = Bg2Scroll;

        Layer1Position = Layer1Position - new Offset16<RoomPixels>(0, 16);
        Layer2Position = Layer2Position - new Offset16<Bg2Pixels>(0, 16);
        // `$80:AFAB` calls only CalculateBlocks before it copies the previous
        // coordinates. At this instant the BG block words still come from the displayed
        // PPU scroll registers, while the layer blocks come from this temporary probe.
        // CalculateBGPositionAndUpdates at $AFB7 recomputes the registers afterwards.
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();
        AdvancePreviousRows(1);
        CalculateScrollRegisters();
        IReadOnlyList<BackgroundUpdateRequest> requests = CalculateBlocksAndUpdates();

        Layer1Position = savedLayer1;
        Layer2Position = savedLayer2;
        Bg1Scroll = savedBg1Scroll;
        Bg2Scroll = savedBg2Scroll;
        return requests;
    }

    /// <summary>
    /// Reconstructs the previous-block words left by the two vertical setup routines.
    /// Down also returns frame zero's hidden row transfer; up carries frame counter one
    /// from <c>FixDoorsMovingUp</c> and therefore performs no destination transfer yet.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> PrimeVerticalDoorOpeningBlocks(
        DoorDirection direction,
        ushort stagedLayer1Y,
        ushort stagedLayer2Y)
    {
        if (direction is not (DoorDirection.Down or DoorDirection.Up))
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Vertical door direction must be down or up.");

        Position16<RoomPixels> actualLayer1 = Layer1Position;
        Position16<Bg2Pixels> actualLayer2 = Layer2Position;
        Layer1Position = Layer1Position.WithY(stagedLayer1Y);
        Layer2Position = Layer2Position.WithY(stagedLayer2Y);
        // Both `$80:AF24` (down) and `$80:AFAB` (up) calculate block coordinates before
        // their temporary layer position is reflected into the PPU scroll mirrors. This
        // mixed old-register/new-layer snapshot is intentional circular-buffer state.
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();

        if (direction == DoorDirection.Up)
        {
            AdvancePreviousRows(1);
            Layer1Position = actualLayer1;
            Layer2Position = actualLayer2;
            CalculateScrollRegisters();
            return Array.Empty<BackgroundUpdateRequest>();
        }

        // DoorTransition_Down frame zero probes fifteen pixels above the staged viewport,
        // publishes its newly exposed bottom row, and then restores the visible registers.
        Position16<RoomPixels> savedBg1Scroll = Bg1Scroll;
        Position16<Bg2Pixels> savedBg2Scroll = Bg2Scroll;
        Layer1Position = Layer1Position.WithY(unchecked((ushort)(stagedLayer1Y - 15)));
        Layer2Position = Layer2Position.WithY(unchecked((ushort)(stagedLayer2Y - 15)));
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();
        AdvancePreviousRows(-1);
        CalculateScrollRegisters();
        IReadOnlyList<BackgroundUpdateRequest> requests = CalculateBlocksAndUpdates();
        Layer1Position = actualLayer1;
        Layer2Position = actualLayer2;
        Bg1Scroll = savedBg1Scroll;
        Bg2Scroll = savedBg2Scroll;
        return requests;
    }

    /// <summary>
    /// Ports the 17-column force-blank fill at <c>$80:A176-$80:A210</c>.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> BuildInitialViewportRequests()
    {
        CalculateGameplayScrolls();
        CalculateBlockCoordinates();
        bool streamsLayer2 = (Layer2Scroll.Horizontal & 1) == 0;
        var requests = new List<BackgroundUpdateRequest>(streamsLayer2 ? 34 : 17);
        var nextColumn = new Offset16<RoomBlocks>(1, 0);
        var nextTilemapColumn = new Offset16<TilemapBlocks>(1, 0);

        // The loop condition tests the old X value after pushing it, so values 0..16 run:
        // seventeen columns. Each column is staged and DMAed before these four block words
        // increment in native code; returning ordered requests preserves that sequencing.
        for (int column = 0; column < 17; column++)
        {
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Level,
                BackgroundUpdateAxis.Column,
                Layer1Block,
                Bg1Block));

            if (streamsLayer2)
            {
                requests.Add(new BackgroundUpdateRequest(
                    BackgroundLayer.Background,
                    BackgroundUpdateAxis.Column,
                    Layer2Block,
                    Bg2Block));
            }

            Layer1Block += nextColumn;
            Bg1Block += nextTilemapColumn;
            Layer2Block += nextColumn;
            Bg2Block += nextTilemapColumn;
        }

        return requests;
    }

    /// <summary>Applies BG1 offsets and derives each BG2 axis from its scroll mode, preserving fixed-mode registers.</summary>
    private void CalculateGameplayScrolls()
    {
        Bg1Scroll = Layer1Position + Bg1Offset;

        // Mode one means "do not scroll BG2 on this axis". The 65C816 subroutine returns
        // carry set, so the caller skips both the layer-2 position write and PPU mirror.
        if (TryCalculateLayer2Position(Layer1Position.X, Layer2Scroll.Horizontal, out ushort layer2X))
        {
            Layer2Position = Layer2Position.WithX(layer2X);
            Bg2Scroll = Bg2Scroll.WithX(unchecked((ushort)(layer2X + Bg2Offset.X)));
        }

        if (TryCalculateLayer2Position(Layer1Position.Y, Layer2Scroll.Vertical, out ushort layer2Y))
        {
            Layer2Position = Layer2Position.WithY(layer2Y);
            Bg2Scroll = Bg2Scroll.WithY(unchecked((ushort)(layer2Y + Bg2Offset.Y)));
        }
    }

    /// <summary>Recomputes PPU scroll mirrors from existing layer positions without changing BG2 parallax state.</summary>
    private void CalculateScrollRegisters()
    {
        Bg1Scroll = Layer1Position + Bg1Offset;
        Bg2Scroll = Layer2Position + Bg2Offset;
    }

    /// <summary>Primes the row and column crossing detector with the current source blocks.</summary>
    private void CopyCurrentBlocksToPrevious()
    {
        PreviousLayer1Block = Layer1Block;
        PreviousLayer2Block = Layer2Block;
    }

    /// <summary>Biases both previous row words, as the vertical door setups do.</summary>
    private void AdvancePreviousRows(short rows)
    {
        var bias = new Offset16<RoomBlocks>(0, rows);
        PreviousLayer1Block += bias;
        PreviousLayer2Block += bias;
    }

    /// <summary>Emits up to four newly exposed level or background strips when 16-pixel boundaries are crossed.</summary>
    private List<BackgroundUpdateRequest> CalculateBlocksAndUpdates()
    {
        CalculateBlockCoordinates();
        var requests = new List<BackgroundUpdateRequest>(capacity: 4);

        // $80:A3E4-$80:A413. Crossing right selects the column 16 blocks beyond the
        // viewport's left edge; crossing left selects the new leftmost column itself.
        if (Layer1Block.X != PreviousLayer1Block.X)
        {
            short distance = SignedDifference(Layer1Block.X, PreviousLayer1Block.X) < 0 ? (short)0 : (short)16;
            PreviousLayer1Block = PreviousLayer1Block.WithX(Layer1Block.X);
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Level,
                BackgroundUpdateAxis.Column,
                Layer1Block + new Offset16<RoomBlocks>(distance, 0),
                Bg1Block + new Offset16<TilemapBlocks>(distance, 0)));
        }

        // Odd layer-2 scroll modes suppress dynamic tilemap updates on that axis. Mode one
        // is fixed; other odd values still calculate parallax with bit zero masked away.
        if ((Layer2Scroll.Horizontal & 1) == 0 && Layer2Block.X != PreviousLayer2Block.X)
        {
            short distance = SignedDifference(Layer2Block.X, PreviousLayer2Block.X) < 0 ? (short)0 : (short)16;
            PreviousLayer2Block = PreviousLayer2Block.WithX(Layer2Block.X);
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Background,
                BackgroundUpdateAxis.Column,
                Layer2Block + new Offset16<RoomBlocks>(distance, 0),
                Bg2Block + new Offset16<TilemapBlocks>(distance, 0)));
        }

        // The visible playfield is 15 blocks tall (240 pixels in the tilemap staging
        // scheme). Downward crossings upload row +15; upward crossings upload row +1.
        if (Layer1Block.Y != PreviousLayer1Block.Y)
        {
            short distance = SignedDifference(Layer1Block.Y, PreviousLayer1Block.Y) < 0 ? (short)1 : (short)15;
            PreviousLayer1Block = PreviousLayer1Block.WithY(Layer1Block.Y);
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Level,
                BackgroundUpdateAxis.Row,
                Layer1Block + new Offset16<RoomBlocks>(0, distance),
                Bg1Block + new Offset16<TilemapBlocks>(0, distance)));
        }

        if ((Layer2Scroll.Vertical & 1) == 0 && Layer2Block.Y != PreviousLayer2Block.Y)
        {
            short distance = SignedDifference(Layer2Block.Y, PreviousLayer2Block.Y) < 0 ? (short)1 : (short)15;
            PreviousLayer2Block = PreviousLayer2Block.WithY(Layer2Block.Y);
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Background,
                BackgroundUpdateAxis.Row,
                Layer2Block + new Offset16<RoomBlocks>(0, distance),
                Bg2Block + new Offset16<TilemapBlocks>(0, distance)));
        }

        return requests;
    }

    /// <summary>Implements the eight four-LSR sequences at <c>$80:A4BB</c>.</summary>
    private void CalculateBlockCoordinates()
    {
        // PPU scroll mirrors are treated as unsigned words by four logical shifts; world
        // positions receive the ROM's explicit 12-to-16-bit sign extension.
        Bg1Block = Bg1Scroll.ToTilemapBlocks();
        Bg2Block = Bg2Scroll.ToTilemapBlocks();
        Layer1Block = Layer1Position.ToSourceBlocks();
        Layer2Block = Layer2Position.ToSourceBlocks();
    }

    /// <summary>Copies, freezes, or parallax-scales one layer-two axis using the room header's native mode byte.</summary>
    private static bool TryCalculateLayer2Position(ushort layer1Position, byte scrollMode, out ushort result)
    {
        if (scrollMode == 1)
        {
            result = 0;
            return false;
        }

        if (scrollMode == 0)
        {
            result = layer1Position;
            return true;
        }

        // The hardware implementation performs two 8x8 multiplies: factor*low contributes
        // its high product byte, and factor*high contributes its low product word. Together
        // they equal floor(position * (mode & $FE) / $100), modulo 16 bits.
        uint factor = (uint)(scrollMode & 0xfe);
        result = unchecked((ushort)SnesEightEight.WholeOf((uint)layer1Position * factor));
        return true;
    }

    /// <summary>Interprets wrapped source-block subtraction as a signed 16-bit movement direction.</summary>
    private static int SignedDifference(ushort left, ushort right) => unchecked((short)(left - right));
}

/// <summary>
/// The room state header's per-axis layer-2 scroll modes: zero copies
/// layer one, one fixes the axis, and other values are parallax multipliers over 128 whose
/// odd bit suppresses dynamic background streaming on that axis.
/// </summary>
/// <param name="Horizontal">The horizontal-axis mode.</param>
/// <param name="Vertical">The vertical-axis mode.</param>
[SpatialComponents("Spatial value type: one layer-2 parallax mode per axis, as the room header stores them.")]
public readonly record struct Layer2ScrollModes(byte Horizontal, byte Vertical);

/// <summary>Which native tilemap producer must satisfy a streaming request.</summary>
public enum BackgroundLayer
{
    /// <summary>The room's collision-backed level tilemap rendered on BG1.</summary>
    Level,

    /// <summary>The independently streamed room-background tilemap rendered on BG2.</summary>
    Background,
}

/// <summary>Whether the native updater requested a 16-block column or row.</summary>
public enum BackgroundUpdateAxis
{
    /// <summary>Transfers one vertical strip when horizontal scrolling crosses a 16-pixel boundary.</summary>
    Column,

    /// <summary>Transfers one horizontal strip when vertical scrolling crosses a 16-pixel boundary.</summary>
    Row,
}

/// <summary>
/// The four coordinate words passed through WRAM <c>$16-$1C</c> immediately before one of
/// the update-level/background-data row/column calls at <c>$80:A413-$80:A4B5</c>.
/// </summary>
/// <param name="Layer">The tilemap producer that must satisfy the request.</param>
/// <param name="Axis">Whether a column or a row is transferred.</param>
/// <param name="SourceBlock">The first source level/background-data block of the strip.</param>
/// <param name="VramBlock">The circular-tilemap block the strip begins at.</param>
public readonly record struct BackgroundUpdateRequest(
    BackgroundLayer Layer,
    BackgroundUpdateAxis Axis,
    Position16<RoomBlocks> SourceBlock,
    Position16<TilemapBlocks> VramBlock);
