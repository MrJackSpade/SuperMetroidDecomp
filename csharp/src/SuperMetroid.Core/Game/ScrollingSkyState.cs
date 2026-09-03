using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Landing Site's bank-$88 scrolling-sky tilemap updater and horizontal HDMA state.
/// </summary>
/// <remarks>
/// <see cref="ProcessFrame"/> ports <c>$88:ADC2</c> and <c>$88:AFA3</c>. The original
/// materializes an indirect HDMA byte table; this model retains the same 23 fixed-point
/// data slots and exposes their final per-gameplay-scanline BG2HOFS values directly.
/// </remarks>
public sealed class ScrollingSkyState
{
    /// <summary>
    /// Bank-$8F room-main wrapper which calls bank-$88's land-sky HDMA/tilemap routine.
    /// </summary>
    public const ushort LandRoomMainCodePointer = 0xc116;

    /// <summary>
    /// Bank-$8F escape wrapper which runs the same land-sky routine before its quake work.
    /// </summary>
    public const ushort ShakingLandRoomMainCodePointer = 0xc120;

    private const ushort Bg2TilemapBaseWord = 0x4800;
    private const int LandChunkPointerTableAddress = 0x88ad9c;

    // Five declared land chunks are immediately followed by the ocean pointer table.
    // Index five therefore reads the ocean table's first word and wraps to chunk zero.
    private static readonly ushort[] LandChunkOffsets =
        [0xb180, 0xb980, 0xc180, 0xc980, 0xd180, 0xb180];

    private static readonly SkyScrollSection[] Sections =
    [
        new(0x0000, 0x8000, 0x0000, 0),
        new(0x0010, 0xc000, 0x0000, 1),
        new(0x0038, 0x8000, 0x0000, 2),
        new(0x00d0, 0xc000, 0x0000, 3),
        new(0x00e0, 0x8000, 0x0000, 4),
        new(0x0120, 0xc000, 0x0000, 5),
        new(0x01a0, 0x8000, 0x0000, 6),
        new(0x01d8, 0xc000, 0x0000, 7),
        new(0x0238, 0x8000, 0x0000, 8),
        new(0x0268, 0xc000, 0x0000, 9),
        new(0x02a0, 0x8000, 0x0000, 10),

        // This is not a typo: the $02E0 band points back to data slot eight ($9FA0), so
        // that slot receives two additions each frame and scrolls faster than either row.
        new(0x02e0, 0xc000, 0x0000, 8),
        new(0x0300, 0x8000, 0x0000, 12),
        new(0x0320, 0xc000, 0x0000, 13),
        new(0x0350, 0x8000, 0x0000, 14),
        new(0x0378, 0xc000, 0x0000, 15),
        new(0x03c8, 0x8000, 0x0000, 16),
        new(0x0440, 0x7000, 0x0000, 17),
        new(0x0460, 0xc000, 0x0000, 18),
        new(0x0480, 0x8000, 0x0000, 19),
        new(0x0490, 0x0000, 0x0000, 20),
        new(0x04a8, 0x0000, 0x0000, 21),
        new(0x04b8, 0x0000, 0x0000, 22),
    ];

    private readonly uint[] _fixedHorizontalScrolls = new uint[23];
    private readonly ISnesAddressSpace? _addressSpace;

    /// <summary>
    /// Creates sky state. A cartridge bus preserves out-of-range 65C816 table reads at the
    /// very top of the room; the parameterless form remains useful for isolated arithmetic
    /// tests whose camera positions select the declared chunk pointers.
    /// </summary>
    public ScrollingSkyState(ISnesAddressSpace? addressSpace = null)
    {
        _addressSpace = addressSpace;
    }

    /// <summary>
    /// Returns whether a room-state main pointer dispatches to the translated land-sky
    /// implementation. This is the same bank-$8F dispatch decision made by the cartridge;
    /// callers must not infer it from a room number, area, door, or camera position.
    /// </summary>
    public static bool IsLandRoomMain(ushort mainCodePointer) =>
        mainCodePointer is LandRoomMainCodePointer or ShakingLandRoomMainCodePointer;

    /// <summary>BG2VOFS mirror written from layer-1 Y by <c>$88:AFB2</c>.</summary>
    public ushort VerticalScroll { get; private set; }

    /// <summary>Whether the indirect table has not been terminated by a frozen frame.</summary>
    public bool HdmaEnabled { get; private set; } = true;

    /// <summary>Integer HDMA data words, indexed like the 23 four-byte slots at $7E:9F80.</summary>
    public ushort GetDataSlotPosition(int slot)
    {
        if ((uint)slot >= _fixedHorizontalScrolls.Length)
            throw new ArgumentOutOfRangeException(nameof(slot));
        return (ushort)(_fixedHorizontalScrolls[slot] >> 16);
    }

    /// <summary>
    /// Advances the fixed-point sky strips and appends the four ordinary VRAM queue records
    /// used to keep the circular BG2 tilemap populated around the camera.
    /// </summary>
    public void ProcessFrame(ushort layer1YPosition, bool timeIsFrozen, VramWriteQueue writes)
    {
        ArgumentNullException.ThrowIfNull(writes);
        if (timeIsFrozen)
        {
            // $88:AFA8 writes a zero first HDMA entry, terminating the table, and queues no
            // rows. The pre-instruction wrapper likewise skips fixed-point advancement.
            HdmaEnabled = false;
            return;
        }

        HdmaEnabled = true;
        AdvanceHorizontalScrolls();
        VerticalScroll = layer1YPosition;
        QueueTilemapRows(layer1YPosition, writes);
    }

    /// <summary>
    /// Resolves the indirect HDMA table into one BG2HOFS word per gameplay scanline.
    /// Index zero corresponds to physical scanline 32, immediately below the HUD.
    /// </summary>
    public ushort[] BuildGameplayHorizontalScrolls(ushort layer1YPosition, int lineCount = 192)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineCount);
        var result = new ushort[lineCount];
        if (!HdmaEnabled)
            return result;

        ReadOnlySpan<SkyScrollSection> sections = Sections;
        for (int line = 0; line < lineCount; line++)
        {
            ushort worldY = unchecked((ushort)(layer1YPosition + 32 + line));
            int sectionIndex = FindSection(worldY, sections);

            // The assembly's fallback entries point at direct-page BG2HOFS ($00B5). Its
            // ordinary value is zero for Landing Site; all positions >=$500 use that path.
            result[line] = sectionIndex >= 0
                ? GetDataSlotPosition(sections[sectionIndex].DataSlot)
                : (ushort)0;
        }
        return result;
    }

    private void AdvanceHorizontalScrolls()
    {
        foreach (SkyScrollSection section in Sections)
        {
            uint velocity = ((uint)section.Speed << 16) | section.Subspeed;
            _fixedHorizontalScrolls[section.DataSlot] = unchecked(
                _fixedHorizontalScrolls[section.DataSlot] + velocity);
        }

        // $88:ADF2-$88:ADFA clears the final fractional and integer words after all 23
        // additions, keeping the bottommost section stationary.
        _fixedHorizontalScrolls[22] = 0;
    }

    private void QueueTilemapRows(ushort layer1YPosition, VramWriteQueue writes)
    {
        // First pair: two rows immediately behind the HUD, beginning at cameraY-16 rounded
        // down to an 8-pixel boundary. All arithmetic is modular 16-bit as on the 65C816.
        ushort upperPosition = unchecked((ushort)((layer1YPosition & 0x07f8) - 0x0010));
        int upperChunk = upperPosition >> 8;
        int upperByteOffset = (upperPosition & 0x00ff) * 8;
        int upperSource = 0x8a0000 | unchecked((ushort)(ReadLandChunkPointer(upperChunk) + upperByteOffset));

        // Second pair: two rows just below the 224-line screen, based on cameraY+$F0.
        ushort lowerPosition = unchecked((ushort)((layer1YPosition & 0x07f8) + 0x00f0));
        int lowerChunk = lowerPosition >> 8;
        int lowerByteOffset = (lowerPosition & 0x00ff) * 8;
        int lowerSource = 0x8a0000 | unchecked((ushort)(ReadLandChunkPointer(lowerChunk) + lowerByteOffset));

        ushort upperDestination = unchecked((ushort)(
            Bg2TilemapBaseWord + 4 * (unchecked((ushort)(layer1YPosition - 16)) & 0x01f8)));
        ushort lowerDestination = unchecked((ushort)(
            Bg2TilemapBaseWord + 4 * (unchecked((ushort)(layer1YPosition + 240)) & 0x01f8)));

        writes.Enqueue(0x0040, upperSource, upperDestination);
        writes.Enqueue(0x0040, upperSource + 0x0040, unchecked((ushort)(upperDestination + 0x0020)));
        writes.Enqueue(0x0040, lowerSource, lowerDestination);
        writes.Enqueue(0x0040, lowerSource + 0x0040, unchecked((ushort)(lowerDestination + 0x0020)));
    }

    private ushort ReadLandChunkPointer(int index)
    {
        if (_addressSpace is not null)
        {
            // The native code derives Y from the wrapped high byte and performs
            // LDA [$00],Y without bounds checking. At camera Y=0, cameraY-16 becomes
            // $FFF0, Y becomes $01FE, and the read lands at ROM $88:AF9A—inside the nearby
            // ocean entry wrapper—not in the five-word land table. Reading through the ROM
            // bus reproduces that harmless offscreen quirk without C/C# memory unsafety.
            int offset = (LandChunkPointerTableAddress + index * 2) & 0xffff;
            int address = 0x880000 | offset;
            return (ushort)(
                _addressSpace.ReadByte(address) |
                (_addressSpace.ReadByte(0x880000 | ((offset + 1) & 0xffff)) << 8));
        }

        // The sixth value is the first adjacent ocean-table word and covers ordinary
        // bottom-of-room arithmetic in isolated tests. More distant wrapped reads require
        // the real bank bytes and therefore deliberately fail without an address space.
        if ((uint)index < LandChunkOffsets.Length)
            return LandChunkOffsets[index];
        throw new InvalidOperationException(
            "Wrapped scrolling-sky pointer reads require a cartridge address space.");
    }

    private static int FindSection(ushort worldY, ReadOnlySpan<SkyScrollSection> sections)
    {
        for (int index = 0; index < sections.Length; index++)
        {
            ushort nextTop = index + 1 < sections.Length ? sections[index + 1].TopPosition : (ushort)0x0500;
            if (worldY >= sections[index].TopPosition && worldY < nextTop)
                return index;
        }
        return -1;
    }
}

/// <summary>One eight-byte row from the ROM table at <c>$88:AEC1</c>.</summary>
public readonly record struct SkyScrollSection(
    ushort TopPosition,
    ushort Subspeed,
    ushort Speed,
    int DataSlot);
