namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$80 layer-position, PPU scroll-register, and 16-pixel streaming-block state.
/// </summary>
/// <remarks>
/// This is a direct behavioral port of <c>$80:A2F9-$80:A527</c>. It deliberately stops
/// at the four row/column update calls: each call is returned as a typed request so the
/// following level-data-to-VRAM translation can be implemented and debugged independently.
/// No desktop camera interpolation is hidden in this class.
/// </remarks>
public sealed class BackgroundScrollState
{
    /// <summary>World-space BG1/camera X word (<c>$0911</c>).</summary>
    public ushort Layer1XPosition { get; set; }

    /// <summary>World-space BG1/camera Y word (<c>$0915</c>).</summary>
    public ushort Layer1YPosition { get; set; }

    /// <summary>Parallax-derived BG2 X word (<c>$0917</c>).</summary>
    public ushort Layer2XPosition { get; set; }

    /// <summary>Parallax-derived BG2 Y word (<c>$0919</c>).</summary>
    public ushort Layer2YPosition { get; set; }

    /// <summary>
    /// Room header's layer-2 X mode. Zero copies BG1; one freezes the axis; every other
    /// value uses its even portion as a fixed-point parallax multiplier over 128.
    /// </summary>
    public byte Layer2ScrollX { get; set; }

    /// <summary>Vertical counterpart of <see cref="Layer2ScrollX"/>.</summary>
    public byte Layer2ScrollY { get; set; }

    // These four offsets are separate native variables despite the confusing adjacency
    // of BG1 X offset and Layer2ScrollY in WRAM. Keeping descriptive names prevents a C#
    // reader from needing to remember that $091F is addressed as Layer2ScrollY+1.
    public ushort Bg1XOffset { get; set; }
    public ushort Bg1YOffset { get; set; }
    public ushort Bg2XOffset { get; set; }
    public ushort Bg2YOffset { get; set; }

    /// <summary>Modeled BG1HOFS mirror in direct-page word <c>$00B1</c>.</summary>
    public ushort Bg1HorizontalScroll { get; private set; }

    /// <summary>Modeled BG1VOFS mirror in direct-page word <c>$00B3</c>.</summary>
    public ushort Bg1VerticalScroll { get; private set; }

    /// <summary>Modeled BG2HOFS mirror in direct-page word <c>$00B5</c>.</summary>
    public ushort Bg2HorizontalScroll { get; private set; }

    /// <summary>Modeled BG2VOFS mirror in direct-page word <c>$00B7</c>.</summary>
    public ushort Bg2VerticalScroll { get; private set; }

    // "Block" means one 16x16 level block here, not one 256x256 room-scroll cell.
    public ushort Bg1XBlock { get; private set; }
    public ushort Bg1YBlock { get; private set; }
    public ushort Bg2XBlock { get; private set; }
    public ushort Bg2YBlock { get; private set; }
    public ushort Layer1XBlock { get; private set; }
    public ushort Layer1YBlock { get; private set; }
    public ushort Layer2XBlock { get; private set; }
    public ushort Layer2YBlock { get; private set; }

    public ushort PreviousLayer1XBlock { get; private set; }
    public ushort PreviousLayer1YBlock { get; private set; }
    public ushort PreviousLayer2XBlock { get; private set; }
    public ushort PreviousLayer2YBlock { get; private set; }

    /// <summary>
    /// Publishes a room actor's direct writes to the PPU BG2 scroll mirrors. Mother Brain
    /// uses fixed layer-two room modes, so the ordinary scrolling pass intentionally leaves
    /// these values untouched after the actor supplies them.
    /// </summary>
    public void SetBg2ScrollRegisters(ushort horizontal, ushort vertical)
    {
        Bg2HorizontalScroll = horizontal;
        Bg2VerticalScroll = vertical;
    }

    /// <summary>
    /// Implements <c>$80:AE29</c> after a directional door setup has staged its off-screen
    /// layer-one coordinate. BG1 retains the source-room PPU scroll while state $0B clears
    /// both BG2 registers; all four offsets then remain active throughout the opening IRQ.
    /// </summary>
    public void ConfigureDoorOpeningOffsets(
        ushort retainedBg1Horizontal,
        ushort retainedBg1Vertical,
        ushort stagedLayer1X,
        ushort stagedLayer1Y)
    {
        Bg1XOffset = unchecked((ushort)(retainedBg1Horizontal - stagedLayer1X));
        Bg1YOffset = unchecked((ushort)(retainedBg1Vertical - stagedLayer1Y));
        Bg2XOffset = unchecked((ushort)(0 - stagedLayer1X));
        Bg2YOffset = unchecked((ushort)(0 - stagedLayer1Y));
    }

    /// <summary>
    /// Implements the two <c>CalculateLayer2*pos</c> calls made by each directional door
    /// setup after bank $82 installs the destination layer-one coordinates. Fixed mode one
    /// deliberately retains its old layer-two word, exactly like the native early return.
    /// </summary>
    public void PrepareDoorOpeningDestination(ushort layer1X, ushort layer1Y)
    {
        Layer1XPosition = layer1X;
        Layer1YPosition = layer1Y;
        if (TryCalculateLayer2Position(layer1X, Layer2ScrollX, out ushort layer2X))
            Layer2XPosition = layer2X;
        if (TryCalculateLayer2Position(layer1Y, Layer2ScrollY, out ushort layer2Y))
            Layer2YPosition = layer2Y;
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
        Bg1HorizontalScroll = unchecked((ushort)(Layer1XPosition + Bg1XOffset));
        Bg1VerticalScroll = unchecked((ushort)(Layer1YPosition + Bg1YOffset));
        Bg2HorizontalScroll = unchecked((ushort)(Layer2XPosition + Bg2XOffset));
        Bg2VerticalScroll = unchecked((ushort)(Layer2YPosition + Bg2YOffset));
        return CalculateBlocksAndUpdates();
    }

    /// <summary>
    /// Seeds the four "previous" words after room/door setup so the first stationary
    /// gameplay frame does not request four phantom rows and columns.
    /// </summary>
    public void PrimePreviousBlocks()
    {
        // Room setup reaches this logical state through several native entry points. Doing
        // the gameplay calculation here makes the C# initialization operation atomic: the
        // saved layer-2 blocks correspond to the parallax position, not constructor zeroes.
        CalculateGameplayScrolls();
        CalculateBlockCoordinates();
        PreviousLayer1XBlock = Layer1XBlock;
        PreviousLayer1YBlock = Layer1YBlock;
        PreviousLayer2XBlock = Layer2XBlock;
        PreviousLayer2YBlock = Layer2YBlock;
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
    public void PrimeHorizontalDoorOpeningBlocks(byte orientation)
    {
        int direction = orientation & 3;
        if (direction is not 0 and not 1)
            throw new ArgumentOutOfRangeException(
                nameof(orientation), orientation, "Horizontal door orientation must be 0 or 1.");

        Bg1HorizontalScroll = unchecked((ushort)(Layer1XPosition + Bg1XOffset));
        Bg1VerticalScroll = unchecked((ushort)(Layer1YPosition + Bg1YOffset));
        Bg2HorizontalScroll = unchecked((ushort)(Layer2XPosition + Bg2XOffset));
        Bg2VerticalScroll = unchecked((ushort)(Layer2YPosition + Bg2YOffset));
        CalculateBlockCoordinates();

        PreviousLayer1XBlock = direction == 0
            ? unchecked((ushort)(Layer1XBlock - 1))
            : unchecked((ushort)(Layer1XBlock + 2));
        PreviousLayer2XBlock = direction == 0
            ? unchecked((ushort)(Layer2XBlock - 1))
            : unchecked((ushort)(Layer2XBlock + 2));
        PreviousLayer1YBlock = Layer1YBlock;
        PreviousLayer2YBlock = Layer2YBlock;
    }

    /// <summary>
    /// Executes <c>$80:AD1D</c>'s source-room row repair before an upward door replaces
    /// the active level data. The routine deliberately restores its layer positions and
    /// PPU mirrors but retains the row written by its temporary 16-pixel probe.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> FixDoorsMovingUp()
    {
        ushort savedLayer1Y = Layer1YPosition;
        ushort savedLayer2Y = Layer2YPosition;
        ushort savedBg1Horizontal = Bg1HorizontalScroll;
        ushort savedBg1Vertical = Bg1VerticalScroll;
        ushort savedBg2Horizontal = Bg2HorizontalScroll;
        ushort savedBg2Vertical = Bg2VerticalScroll;

        Layer1YPosition = unchecked((ushort)(Layer1YPosition - 16));
        Layer2YPosition = unchecked((ushort)(Layer2YPosition - 16));
        CalculateScrollRegisters();
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();
        PreviousLayer1YBlock = unchecked((ushort)(PreviousLayer1YBlock + 1));
        PreviousLayer2YBlock = unchecked((ushort)(PreviousLayer2YBlock + 1));
        IReadOnlyList<BackgroundUpdateRequest> requests = CalculateBlocksAndUpdates();

        Layer1YPosition = savedLayer1Y;
        Layer2YPosition = savedLayer2Y;
        Bg1HorizontalScroll = savedBg1Horizontal;
        Bg1VerticalScroll = savedBg1Vertical;
        Bg2HorizontalScroll = savedBg2Horizontal;
        Bg2VerticalScroll = savedBg2Vertical;
        return requests;
    }

    /// <summary>
    /// Reconstructs the previous-block words left by the two vertical setup routines.
    /// Down also returns frame zero's hidden row transfer; up carries frame counter one
    /// from <c>FixDoorsMovingUp</c> and therefore performs no destination transfer yet.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> PrimeVerticalDoorOpeningBlocks(
        byte orientation,
        ushort stagedLayer1Y,
        ushort stagedLayer2Y)
    {
        int direction = orientation & 3;
        if (direction is not 2 and not 3)
            throw new ArgumentOutOfRangeException(
                nameof(orientation), orientation, "Vertical door orientation must be 2 or 3.");

        ushort actualLayer1Y = Layer1YPosition;
        ushort actualLayer2Y = Layer2YPosition;
        Layer1YPosition = stagedLayer1Y;
        Layer2YPosition = stagedLayer2Y;
        CalculateScrollRegisters();
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();

        if (direction == 3)
        {
            PreviousLayer1YBlock = unchecked((ushort)(PreviousLayer1YBlock + 1));
            PreviousLayer2YBlock = unchecked((ushort)(PreviousLayer2YBlock + 1));
            Layer1YPosition = actualLayer1Y;
            Layer2YPosition = actualLayer2Y;
            CalculateScrollRegisters();
            return Array.Empty<BackgroundUpdateRequest>();
        }

        // DoorTransition_Down frame zero probes fifteen pixels above the staged viewport,
        // publishes its newly exposed bottom row, and then restores the visible registers.
        ushort savedBg1Horizontal = Bg1HorizontalScroll;
        ushort savedBg1Vertical = Bg1VerticalScroll;
        ushort savedBg2Horizontal = Bg2HorizontalScroll;
        ushort savedBg2Vertical = Bg2VerticalScroll;
        Layer1YPosition = unchecked((ushort)(stagedLayer1Y - 15));
        Layer2YPosition = unchecked((ushort)(stagedLayer2Y - 15));
        CalculateScrollRegisters();
        CalculateBlockCoordinates();
        CopyCurrentBlocksToPrevious();
        PreviousLayer1YBlock = unchecked((ushort)(PreviousLayer1YBlock - 1));
        PreviousLayer2YBlock = unchecked((ushort)(PreviousLayer2YBlock - 1));
        IReadOnlyList<BackgroundUpdateRequest> requests = CalculateBlocksAndUpdates();
        Layer1YPosition = actualLayer1Y;
        Layer2YPosition = actualLayer2Y;
        Bg1HorizontalScroll = savedBg1Horizontal;
        Bg1VerticalScroll = savedBg1Vertical;
        Bg2HorizontalScroll = savedBg2Horizontal;
        Bg2VerticalScroll = savedBg2Vertical;
        return requests;
    }

    /// <summary>
    /// Ports the 17-column force-blank fill at <c>$80:A176-$80:A210</c>.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> BuildInitialViewportRequests()
    {
        CalculateGameplayScrolls();
        CalculateBlockCoordinates();
        bool streamsLayer2 = (Layer2ScrollX & 1) == 0;
        var requests = new List<BackgroundUpdateRequest>(streamsLayer2 ? 34 : 17);

        // The loop condition tests the old X value after pushing it, so values 0..16 run:
        // seventeen columns. Each column is staged and DMAed before these four block words
        // increment in native code; returning ordered requests preserves that sequencing.
        for (int column = 0; column < 17; column++)
        {
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Level,
                BackgroundUpdateAxis.Column,
                Layer1XBlock,
                Layer1YBlock,
                Bg1XBlock,
                Bg1YBlock));

            if (streamsLayer2)
            {
                requests.Add(new BackgroundUpdateRequest(
                    BackgroundLayer.Background,
                    BackgroundUpdateAxis.Column,
                    Layer2XBlock,
                    Layer2YBlock,
                    Bg2XBlock,
                    Bg2YBlock));
            }

            Layer1XBlock = unchecked((ushort)(Layer1XBlock + 1));
            Bg1XBlock = unchecked((ushort)(Bg1XBlock + 1));
            Layer2XBlock = unchecked((ushort)(Layer2XBlock + 1));
            Bg2XBlock = unchecked((ushort)(Bg2XBlock + 1));
        }

        return requests;
    }

    private void CalculateGameplayScrolls()
    {
        Bg1HorizontalScroll = unchecked((ushort)(Layer1XPosition + Bg1XOffset));
        Bg1VerticalScroll = unchecked((ushort)(Layer1YPosition + Bg1YOffset));

        // Mode one means "do not scroll BG2 on this axis". The 65C816 subroutine returns
        // carry set, so the caller skips both the layer-2 position write and PPU mirror.
        if (TryCalculateLayer2Position(Layer1XPosition, Layer2ScrollX, out ushort layer2X))
        {
            Layer2XPosition = layer2X;
            Bg2HorizontalScroll = unchecked((ushort)(Layer2XPosition + Bg2XOffset));
        }

        if (TryCalculateLayer2Position(Layer1YPosition, Layer2ScrollY, out ushort layer2Y))
        {
            Layer2YPosition = layer2Y;
            Bg2VerticalScroll = unchecked((ushort)(Layer2YPosition + Bg2YOffset));
        }
    }

    private void CalculateScrollRegisters()
    {
        Bg1HorizontalScroll = unchecked((ushort)(Layer1XPosition + Bg1XOffset));
        Bg1VerticalScroll = unchecked((ushort)(Layer1YPosition + Bg1YOffset));
        Bg2HorizontalScroll = unchecked((ushort)(Layer2XPosition + Bg2XOffset));
        Bg2VerticalScroll = unchecked((ushort)(Layer2YPosition + Bg2YOffset));
    }

    private void CopyCurrentBlocksToPrevious()
    {
        PreviousLayer1XBlock = Layer1XBlock;
        PreviousLayer1YBlock = Layer1YBlock;
        PreviousLayer2XBlock = Layer2XBlock;
        PreviousLayer2YBlock = Layer2YBlock;
    }

    private IReadOnlyList<BackgroundUpdateRequest> CalculateBlocksAndUpdates()
    {
        CalculateBlockCoordinates();
        var requests = new List<BackgroundUpdateRequest>(capacity: 4);

        // $80:A3E4-$80:A413. Crossing right selects the column 16 blocks beyond the
        // viewport's left edge; crossing left selects the new leftmost column itself.
        if (Layer1XBlock != PreviousLayer1XBlock)
        {
            ushort distance = SignedDifference(Layer1XBlock, PreviousLayer1XBlock) < 0
                ? (ushort)0
                : (ushort)16;
            PreviousLayer1XBlock = Layer1XBlock;
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Level,
                BackgroundUpdateAxis.Column,
                unchecked((ushort)(Layer1XBlock + distance)),
                Layer1YBlock,
                unchecked((ushort)(Bg1XBlock + distance)),
                Bg1YBlock));
        }

        // Odd layer-2 scroll modes suppress dynamic tilemap updates on that axis. Mode one
        // is fixed; other odd values still calculate parallax with bit zero masked away.
        if ((Layer2ScrollX & 1) == 0 && Layer2XBlock != PreviousLayer2XBlock)
        {
            ushort distance = SignedDifference(Layer2XBlock, PreviousLayer2XBlock) < 0
                ? (ushort)0
                : (ushort)16;
            PreviousLayer2XBlock = Layer2XBlock;
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Background,
                BackgroundUpdateAxis.Column,
                unchecked((ushort)(Layer2XBlock + distance)),
                Layer2YBlock,
                unchecked((ushort)(Bg2XBlock + distance)),
                Bg2YBlock));
        }

        // The visible playfield is 15 blocks tall (240 pixels in the tilemap staging
        // scheme). Downward crossings upload row +15; upward crossings upload row +1.
        if (Layer1YBlock != PreviousLayer1YBlock)
        {
            ushort distance = SignedDifference(Layer1YBlock, PreviousLayer1YBlock) < 0
                ? (ushort)1
                : (ushort)15;
            PreviousLayer1YBlock = Layer1YBlock;
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Level,
                BackgroundUpdateAxis.Row,
                Layer1XBlock,
                unchecked((ushort)(Layer1YBlock + distance)),
                Bg1XBlock,
                unchecked((ushort)(Bg1YBlock + distance))));
        }

        if ((Layer2ScrollY & 1) == 0 && Layer2YBlock != PreviousLayer2YBlock)
        {
            ushort distance = SignedDifference(Layer2YBlock, PreviousLayer2YBlock) < 0
                ? (ushort)1
                : (ushort)15;
            PreviousLayer2YBlock = Layer2YBlock;
            requests.Add(new BackgroundUpdateRequest(
                BackgroundLayer.Background,
                BackgroundUpdateAxis.Row,
                Layer2XBlock,
                unchecked((ushort)(Layer2YBlock + distance)),
                Bg2XBlock,
                unchecked((ushort)(Bg2YBlock + distance))));
        }

        return requests;
    }

    /// <summary>Implements the eight four-LSR sequences at <c>$80:A4BB</c>.</summary>
    private void CalculateBlockCoordinates()
    {
        // PPU scroll mirrors are treated as unsigned words by four logical shifts.
        Bg1XBlock = (ushort)(Bg1HorizontalScroll >> 4);
        Bg2XBlock = (ushort)(Bg2HorizontalScroll >> 4);
        Bg1YBlock = (ushort)(Bg1VerticalScroll >> 4);
        Bg2YBlock = (ushort)(Bg2VerticalScroll >> 4);

        // World positions receive an explicit 12-to-16-bit sign extension after the same
        // logical shifts. An arithmetic cast expresses the result, but the comments retain
        // the ROM's actual LSR/BIT #$0800/ORA #$F000 mechanism.
        Layer1XBlock = ArithmeticBlock(Layer1XPosition);
        Layer2XBlock = ArithmeticBlock(Layer2XPosition);
        Layer1YBlock = ArithmeticBlock(Layer1YPosition);
        Layer2YBlock = ArithmeticBlock(Layer2YPosition);
    }

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
        result = unchecked((ushort)(((uint)layer1Position * factor) >> 8));
        return true;
    }

    private static ushort ArithmeticBlock(ushort position) => unchecked((ushort)((short)position >> 4));

    private static int SignedDifference(ushort left, ushort right) => unchecked((short)(left - right));
}

/// <summary>Which native tilemap producer must satisfy a streaming request.</summary>
public enum BackgroundLayer
{
    Level,
    Background,
}

/// <summary>Whether the native updater requested a 16-block column or row.</summary>
public enum BackgroundUpdateAxis
{
    Column,
    Row,
}

/// <summary>
/// The four coordinate words passed through WRAM <c>$16-$1C</c> immediately before one of
/// the update-level/background-data row/column calls at <c>$80:A413-$80:A4B5</c>.
/// </summary>
public readonly record struct BackgroundUpdateRequest(
    BackgroundLayer Layer,
    BackgroundUpdateAxis Axis,
    ushort SourceXBlock,
    ushort SourceYBlock,
    ushort VramXBlock,
    ushort VramYBlock);
