using SuperMetroid.Core.Assets;
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

    /// <summary>True after the head initialization equivalent at <c>$A9:8705</c>.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Number of calls made through the active rot processor.</summary>
    public uint ProcessCallCount { get; private set; }

    /// <summary>Number of row-finished hooks emitted so far.</summary>
    public uint FinishedEntryCount { get; private set; }

    /// <summary>
    /// Builds the native rot table and extracts the working corpse frame into WRAM.
    /// The initial pixels come exclusively from the installed, editable PNG.
    /// </summary>
    public void Initialize(ISnesAddressSpace bus, RoomCharacterAtlas artwork)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(artwork);
        ReadOnlySpan<byte> installedTiles = artwork.Transfer.Span;
        if (installedTiles.Length != MotherBrainCorpseArtworkDefinitions.ByteCount)
            throw new InvalidDataException("Installed Mother Brain corpse PNG has the wrong tile count.");

        // `$DC40` starts at height-1 and writes four bytes per entry. Y decreases from the
        // corpse's bottom row to its top while the delay grows by two calls per entry.
        CorpseRottingTableProcessor.Initialize(bus, RotTableAddress, EntryCount);

        // MVN copies A+1 bytes. The lengths below already include that 65816 convention,
        // so a host loop uses `< Length` without adding another byte.
        for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
        {
            for (int byteIndex = 0; byteIndex < MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row); byteIndex++)
            {
                int sourceAddress = MotherBrainCorpseArtworkDefinitions.InitialCopySource(row) + byteIndex;
                bus.WriteByte(
                    GraphicsBufferAddress + row * MotherBrainCorpseArtworkDefinitions.RowBytes + byteIndex,
                    installedTiles[sourceAddress - MotherBrainCorpseArtworkDefinitions.SourceAddress]);
            }
        }

        ProcessCallCount = 0;
        FinishedEntryCount = 0;
        IsInitialized = true;
    }

    /// <summary>Runs one exact call of <c>ProcessCorpseRotting</c> for Mother Brain.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        ISnesAddressSpace bus,
        ISnesMutableMemory memory,
        ushort brainXPosition,
        ushort brainYPosition,
        ushort randomNumberSeed,
        ushort mainEnemyExecutionCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(memory);
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "Mother Brain corpse rotting must be initialized from installed artwork first.");
        }

        ProcessCallCount++;
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);

        bool stillRotting = CorpseRottingTableProcessor.Step(
            bus,
            memory,
            RotTableAddress,
            EntryCount,
            yLimit: EntryCount - 1,
            lateMoveEntryIndex: EntryCount - 2,
            copyOrMovePixelRow: (yOffset, move) => CopyOrMovePixelRow(bus, memory, yOffset, move),
            entryFinished: entryIndex =>
            {
                // `$B223` samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every completed table entry.
                FinishedEntryCount++;
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    XPosition: unchecked((ushort)(
                        brainXPosition + (randomNumberSeed & 0x001f) - 0x0010)),
                    YPosition: unchecked((ushort)(brainYPosition + 0x0010)),
                    ProjectileParameter: 0x000a,
                    SoundEffectQueued: (mainEnemyExecutionCounter & 7) == 0,
                    SoundEffect: 0x0010));
            });

        // The final entry returns carry clear before the enemy-specific VRAM transfer
        // callback, whereas every still-active call queues the complete six-record list.
        if (!stillRotting)
        {
            return new MotherBrainCorpseRottingStepResult(
                StillRotting: false,
                VramTransfers: [],
                DustRequests: dustRequests);
        }

        // Carry set from `$DBDF` reaches `$B1DD`, which queues all six records every time.
        return new MotherBrainCorpseRottingStepResult(
            StillRotting: true,
            MotherBrainCorpseArtworkDefinitions.RotTransfers,
            DustRequests: dustRequests);
    }

    private static void CopyOrMovePixelRow(ISnesAddressSpace bus, ISnesMutableMemory memory, ushort yOffset, bool move)
    {
        // Divide by eight to choose the tile row, then multiply the remaining pixel index
        // by two because each 4bpp bitplane row stores a 16-bit planes-0/1 word.
        int tileRowIndex = yOffset >> 3;
        int pixelRowIndex = yOffset & 7;
        int sourceOffset = MotherBrainCorpseArtworkDefinitions.TileRowOffset(tileRowIndex) + pixelRowIndex * 2;

        // Pixel rows 6/7 cannot be written at +2/+4 without leaving their current tile.
        // `$DC01` adds `$E0-$0C = $D4`; the callee's own +2 then lands at pixel rows 7/0
        // of the next tile row exactly as the native address arithmetic intends.
        int destinationOffset = pixelRowIndex < 6
            ? sourceOffset
            : sourceOffset + 0x00d4;

        for (int columnIndex = 0; columnIndex < MotherBrainCorpseArtworkDefinitions.ColumnCount; columnIndex++)
        {
            if (yOffset < MotherBrainCorpseArtworkDefinitions.ColumnMinimumY(columnIndex))
                continue;

            int columnOffset = columnIndex * MotherBrainCorpseArtworkDefinitions.TileBytes;
            bool sourceCanBeCopied = yOffset < EntryCount - 2;
            if (sourceCanBeCopied)
            {
                // Each tile pixel row consists of two independent 16-bit words: planes
                // 0/1 at +0 and planes 2/3 at +$10. Read before any source clear so move
                // retains the exact load/store ordering of `$EA40-$EB07`.
                ushort planes01 = SnesWorkRam.ReadWord(
                    memory,
                    GraphicsBufferAddress + columnOffset + sourceOffset);
                ushort planes23 = SnesWorkRam.ReadWord(
                    memory,
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

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }

}

/// <summary>One MiscDust projectile emitted by Mother Brain's row-finished hook.</summary>
public readonly record struct MotherBrainCorpseDustRequest(
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
