using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's specialization of the shared corpse-rotting engine at
/// <c>$A9:DB12-$DCB6/$A9:E08B-$EBAB</c>.
/// </summary>
/// <remarks>
/// This is deliberately a translation of the graphics algorithm, not a replacement fade.
/// The retail routine treats every visible pixel row as a four-byte table entry, delays the
/// rows by different amounts, and physically moves SNES 4bpp bitplane rows through a WRAM
/// staging buffer. Six ordinary VRAM queue records then upload that changing buffer.
/// Keeping the native WRAM addresses makes the result observable by the normal DMA path and
/// by a debugger in exactly the same representation used by the cartridge.
/// </remarks>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>First byte of the six-row corpse graphics staging buffer.</summary>
    public const int GraphicsBufferAddress = 0x7e9000;

    /// <summary>Native 48-entry rot table at <c>$7E:9700-$97BF</c>.</summary>
    public const int RotTableAddress = 0x7e9700;

    /// <summary>Six tile rows times <c>$E0</c> bytes per row.</summary>
    public const int GraphicsBufferSize = 0x0540;

    /// <summary>One rot entry per visible pixel row.</summary>
    public const int EntryCount = 0x0030;

    // `$A9:E262` maps each group of eight pixel rows to its row of seven 4bpp tiles.
    // The final two offsets are retained even though the 48-pixel corpse normally indexes
    // only rows zero through five: the native table itself contains all eight words.
    private static ReadOnlySpan<ushort> TileRowOffsets =>
        [0x0000, 0x00e0, 0x01c0, 0x02a0, 0x0380, 0x0460, 0x0540, 0x0620];

    // The Mother Brain corpse is seven tiles wide. Each value is the start of one tile
    // column inside a `$E0`-byte row: two adjacent `$10`-byte bitplane halves make a tile.
    private static ReadOnlySpan<ushort> ColumnOffsets =>
        [0x0000, 0x0020, 0x0040, 0x0060, 0x0080, 0x00a0, 0x00c0];

    // Transparent gaps in the right-hand corpse frame mean not every column exists at every
    // Y. These lower bounds are the literal CMP/BCC gates in `$EA40/$EB0B`; applying them is
    // essential because clearing a nonexistent column would corrupt an adjacent tile row.
    private static ReadOnlySpan<ushort> ColumnMinimumY =>
        [0x0010, 0x0008, 0x0000, 0x0000, 0x0000, 0x0008, 0x0020];

    // `$A9:E08B` extracts only the right-hand frame from two side-by-side corpse frames.
    // The first four rows omit their empty final tile; the bottom two copy all seven tiles.
    private static readonly MotherBrainCorpseGraphicsCopy[] InitialGraphicsCopies =
    [
        new(0xb7cec0, 0x0000, 0x00c0),
        new(0xb7d0c0, 0x00e0, 0x00c0),
        new(0xb7d2c0, 0x01c0, 0x00c0),
        new(0xb7d4c0, 0x02a0, 0x00c0),
        new(0xb7d6c0, 0x0380, 0x00e0),
        new(0xb7d8c0, 0x0460, 0x00e0),
    ];

    // `$A9:E1F4-$E225` is not a frame-spread sprite-transfer list. The rotting body function
    // walks the entire definition on every active call and appends all six records.
    private static readonly MotherBrainSpriteTileTransferRequest[] VramTransfers =
    [
        new(0, 0x0060, 0x7e9040, 0x7a80),
        new(1, 0x00a0, 0x7e9100, 0x7b70),
        new(2, 0x00c0, 0x7e91c0, 0x7c60),
        new(3, 0x00c0, 0x7e92a0, 0x7d60),
        new(4, 0x00e0, 0x7e9380, 0x7e60),
        new(5, 0x00e0, 0x7e9460, 0x7f60),
    ];

    /// <summary>True after the head initialization equivalent at <c>$A9:8705</c>.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Number of calls made through the active rot processor.</summary>
    public uint ProcessCallCount { get; private set; }

    /// <summary>Number of row-finished hooks emitted so far.</summary>
    public uint FinishedEntryCount { get; private set; }

    /// <summary>
    /// Builds the native rot table and extracts the working corpse frame from ROM into WRAM.
    /// </summary>
    public void Initialize(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // `$DC40` starts at height-1 and writes four bytes per entry. Y decreases from the
        // corpse's bottom row to its top while the delay grows by two calls per entry.
        for (int entryIndex = 0; entryIndex < EntryCount; entryIndex++)
        {
            int entryAddress = RotTableAddress + entryIndex * 4;
            WriteWord(bus, entryAddress, unchecked((ushort)(EntryCount - 1 - entryIndex)));
            WriteWord(bus, entryAddress + 2, unchecked((ushort)(entryIndex * 2)));
        }

        // MVN copies A+1 bytes. The lengths below already include that 65816 convention,
        // so a host loop uses `< Length` without adding another byte.
        foreach (MotherBrainCorpseGraphicsCopy copy in InitialGraphicsCopies)
        {
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                bus.WriteByte(
                    GraphicsBufferAddress + copy.DestinationOffset + byteIndex,
                    bus.ReadByte(checked((int)copy.SourceAddress) + byteIndex));
            }
        }

        ProcessCallCount = 0;
        FinishedEntryCount = 0;
        IsInitialized = true;
    }

    /// <summary>Reads one native four-byte table entry for debugger and verification use.</summary>
    public MotherBrainCorpseRotEntry ReadEntry(ISnesAddressSpace bus, int entryIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)entryIndex >= EntryCount)
            throw new ArgumentOutOfRangeException(nameof(entryIndex));

        int entryAddress = RotTableAddress + entryIndex * 4;
        return new MotherBrainCorpseRotEntry(
            unchecked((short)ReadWord(bus, entryAddress)),
            ReadWord(bus, entryAddress + 2));
    }

    /// <summary>Runs one exact call of <c>ProcessCorpseRotting</c> for Mother Brain.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        ISnesAddressSpace bus,
        ushort brainXPosition,
        ushort brainYPosition,
        ushort randomNumberSeed,
        ushort mainEnemyExecutionCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "Mother Brain corpse rotting must be initialized from the ROM-backed bus first.");
        }

        ProcessCallCount++;
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        for (int entryIndex = 0; entryIndex < EntryCount; entryIndex++)
        {
            int entryAddress = RotTableAddress + entryIndex * 4;
            short yOffset = unchecked((short)ReadWord(bus, entryAddress));

            // `$FFFF` marks a completed non-final entry. BMI skips every negative value,
            // so preserve the signed 16-bit test rather than inventing a separate active bit.
            if (yOffset < 0)
                continue;

            ushort timer = ReadWord(bus, entryAddress + 2);
            if (timer != 0)
            {
                // Delayed entries begin copying only for timer values 3, 2, and 1. Entry
                // indices 46/47 use move instead, preventing rows below the corpse height
                // from being populated by those final two delayed passes.
                timer = unchecked((ushort)(timer - 1));
                WriteWord(bus, entryAddress + 2, timer);
                if (timer >= 4)
                    continue;

                CopyOrMovePixelRow(
                    bus,
                    unchecked((ushort)yOffset),
                    move: entryIndex >= EntryCount - 2);
                continue;
            }

            // Once its delay is zero an entry moves, rather than copies, its source row.
            // The source is cleared after the destination writes, creating the dissolving
            // downward trail visible in the original effect.
            CopyOrMovePixelRow(bus, unchecked((ushort)yOffset), move: true);

            ushort nextYOffset = unchecked((ushort)(yOffset + 2));
            if (nextYOffset < EntryCount - 1)
            {
                WriteWord(bus, entryAddress, nextYOffset);
                continue;
            }

            // `$B223` samples the already-existing global seed; it does not advance RNG.
            // One dust projectile is emitted for every completed entry. Sound library two
            // effect `$10` is requested only on main-enemy execution counts divisible by 8.
            FinishedEntryCount++;
            dustRequests.Add(new MotherBrainCorpseDustRequest(
                EntryIndex: unchecked((ushort)entryIndex),
                XPosition: unchecked((ushort)(brainXPosition + (randomNumberSeed & 0x001f) - 0x0010)),
                YPosition: unchecked((ushort)(brainYPosition + 0x0010)),
                ProjectileParameter: 0x000a,
                SoundEffectQueued: (mainEnemyExecutionCounter & 7) == 0,
                SoundEffect: 0x0010));

            // The last table entry returns carry clear immediately. Native `$B1D5` then
            // skips the VRAM-transfer function on this completion call.
            if (entryIndex >= EntryCount - 1)
            {
                return new MotherBrainCorpseRottingStepResult(
                    StillRotting: false,
                    VramTransfers: [],
                    DustRequests: dustRequests);
            }

            // Earlier finished rows remain in the table as signed `$FFFF` and are skipped
            // by every later call while the delayed rows above them continue falling.
            WriteWord(bus, entryAddress, 0xffff);
        }

        // Carry set from `$DBDF` reaches `$B1DD`, which queues all six records every time.
        return new MotherBrainCorpseRottingStepResult(
            StillRotting: true,
            VramTransfers,
            DustRequests: dustRequests);
    }

    private static void CopyOrMovePixelRow(ISnesAddressSpace bus, ushort yOffset, bool move)
    {
        // Divide by eight to choose the tile row, then multiply the remaining pixel index
        // by two because each 4bpp bitplane row stores a 16-bit planes-0/1 word.
        int tileRowIndex = yOffset >> 3;
        int pixelRowIndex = yOffset & 7;
        int sourceOffset = TileRowOffsets[tileRowIndex] + pixelRowIndex * 2;

        // Pixel rows 6/7 cannot be written at +2/+4 without leaving their current tile.
        // `$DC01` adds `$E0-$0C = $D4`; the callee's own +2 then lands at pixel rows 7/0
        // of the next tile row exactly as the native address arithmetic intends.
        int destinationOffset = pixelRowIndex < 6
            ? sourceOffset
            : sourceOffset + 0x00d4;

        for (int columnIndex = 0; columnIndex < ColumnOffsets.Length; columnIndex++)
        {
            if (yOffset < ColumnMinimumY[columnIndex])
                continue;

            int columnOffset = ColumnOffsets[columnIndex];
            bool sourceCanBeCopied = yOffset < EntryCount - 2;
            if (sourceCanBeCopied)
            {
                // Each tile pixel row consists of two independent 16-bit words: planes
                // 0/1 at +0 and planes 2/3 at +$10. Read before any source clear so move
                // retains the exact load/store ordering of `$EA40-$EB07`.
                ushort planes01 = ReadWord(
                    bus,
                    GraphicsBufferAddress + columnOffset + sourceOffset);
                ushort planes23 = ReadWord(
                    bus,
                    GraphicsBufferAddress + columnOffset + sourceOffset + 0x10);
                WriteWord(
                    bus,
                    GraphicsBufferAddress + columnOffset + destinationOffset + 2,
                    planes01);
                WriteWord(
                    bus,
                    GraphicsBufferAddress + columnOffset + destinationOffset + 0x12,
                    planes23);
            }

            if (move)
            {
                // Move clears even when Y is 46/47 and the copy was suppressed. That subtle
                // asymmetry is how the bottom rows disappear without writing past height 48.
                WriteWord(bus, GraphicsBufferAddress + columnOffset + sourceOffset, 0);
                WriteWord(bus, GraphicsBufferAddress + columnOffset + sourceOffset + 0x10, 0);
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }

    private readonly record struct MotherBrainCorpseGraphicsCopy(
        uint SourceAddress,
        int DestinationOffset,
        int Length);
}

/// <summary>One native <c>(signed Y offset, timer)</c> corpse-rotting table record.</summary>
public readonly record struct MotherBrainCorpseRotEntry(short YOffset, ushort Timer);

/// <summary>One MiscDust projectile emitted by Mother Brain's row-finished hook.</summary>
public readonly record struct MotherBrainCorpseDustRequest(
    ushort EntryIndex,
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter,
    bool SoundEffectQueued,
    ushort SoundEffect);

/// <summary>Observable work produced by one shared corpse-rotting processor call.</summary>
public readonly record struct MotherBrainCorpseRottingStepResult(
    bool StillRotting,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> VramTransfers,
    IReadOnlyList<MotherBrainCorpseDustRequest> DustRequests);
