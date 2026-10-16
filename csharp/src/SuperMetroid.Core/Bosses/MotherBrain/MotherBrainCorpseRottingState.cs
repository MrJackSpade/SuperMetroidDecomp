using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's specialization of the shared corpse-rotting engine at
/// <c>$A9:DB12-$DCB6/$A9:E08B-$EBAB</c>.
/// </summary>
/// <remarks>
/// This is deliberately a translation of the graphics algorithm, not a replacement fade.
/// The retail routine treats every visible pixel row as a four-byte table entry, delays the
/// rows by different amounts, and physically moves SNES 4bpp bitplane rows through a WRAM
/// staging buffer. Six VRAM transfers then upload that changing buffer. The heap keeps the
/// cartridge's byte layout, so the result is observable in exactly the representation the
/// cartridge uses.
/// </remarks>
public sealed class MotherBrainCorpseRottingState
{
    /// <summary>One rot entry per visible pixel row of the table at <see cref="EnemyStagingSurfaces.MotherBrainRotTable"/>.</summary>
    public const int EntryCount = 48;

    /// <summary>True after the head initialization equivalent at <c>$A9:8705</c>.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Number of calls made through the active rot processor.</summary>
    public uint ProcessCallCount { get; private set; }

    /// <summary>Number of row-finished hooks emitted so far.</summary>
    public uint FinishedEntryCount { get; private set; }

    /// <summary>
    /// Builds the native rot table and extracts the working corpse frame into the rotting heap.
    /// The initial pixels come exclusively from the installed, editable PNG.
    /// </summary>
    public void Initialize(EnemyStagingSurfaces scratch, RoomCharacterAtlas artwork)
    {
        Ensure.NotNull(scratch);
        Ensure.NotNull(artwork);
        ReadOnlySpan<byte> installedTiles = artwork.Transfer.Span;
        if (installedTiles.Length != MotherBrainCorpseArtworkDefinitions.ByteCount)
            throw new InvalidDataException("Installed Mother Brain corpse PNG has the wrong tile count.");

        // `$DC40` starts at height-1 and writes four bytes per entry. Y decreases from the
        // corpse's bottom row to its top while the delay grows by two calls per entry.
        CorpseRottingTableProcessor.Initialize(scratch.MotherBrainRotTable);

        // MVN copies A+1 bytes. The lengths below already include that 65816 convention,
        // so a host loop uses `< Length` without adding another byte.
        StagingByteView tiles = scratch.MotherBrainCorpseTiles;
        for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
        {
            for (int byteIndex = 0; byteIndex < MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row); byteIndex++)
            {
                int sourceAddress = MotherBrainCorpseArtworkDefinitions.InitialCopySource(row) + byteIndex;
                tiles[row * MotherBrainCorpseArtworkDefinitions.RowBytes + byteIndex] =
                    installedTiles[sourceAddress - MotherBrainCorpseArtworkDefinitions.SourceAddress];
            }
        }

        ProcessCallCount = 0;
        FinishedEntryCount = 0;
        IsInitialized = true;
    }

    /// <summary>Runs one exact call of <c>ProcessCorpseRotting</c> for Mother Brain.</summary>
    public MotherBrainCorpseRottingStepResult Step(
        EnemyStagingSurfaces scratch,
        Position16<RoomPixels> brainPosition,
        ushort randomNumberSeed,
        ushort mainEnemyExecutionCounter)
    {
        Ensure.NotNull(scratch);
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "Mother Brain corpse rotting must be initialized from installed artwork first.");
        }

        ProcessCallCount++;
        var dustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);
        StagingByteView tiles = scratch.MotherBrainCorpseTiles;

        bool stillRotting = CorpseRottingTableProcessor.Step(
            scratch.MotherBrainRotTable,
            yLimit: EntryCount - 1,
            lateMoveEntryIndex: EntryCount - 2,
            copyOrMovePixelRow: (yOffset, move) => CopyOrMovePixelRow(tiles, yOffset, move),
            entryFinished: entryIndex =>
            {
                // `$B223` samples the already-existing global seed; it does not advance
                // RNG. One dust projectile is emitted for every completed table entry.
                FinishedEntryCount++;
                dustRequests.Add(new MotherBrainCorpseDustRequest(
                    Position: brainPosition + new Offset16<RoomPixels>(
                        unchecked((short)((randomNumberSeed % 32) - 16)),
                        16),
                    Animation: MiscDustAnimation.CorpseDustCloud,
                    SoundEffectQueued: (mainEnemyExecutionCounter & 7) == 0,
                    SoundEffect: SoundEffectLibrary2Sounds.DeadMonsterDust.Value));
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

    /// <summary>Copies both 4bpp plane words to the next visible pixel row in
    /// each eligible corpse column, optionally clearing the source afterward.
    /// Rows six and seven continue in the next tile row using native offsets.</summary>
    internal static void CopyOrMovePixelRow(StagingByteView tiles, ushort yOffset, bool move)
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
            : sourceOffset + 212;

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
                ushort planes01 = tiles.ReadWord(columnOffset + sourceOffset);
                ushort planes23 = tiles.ReadWord(columnOffset + sourceOffset + 16);
                tiles.WriteWord(columnOffset + destinationOffset + 2, planes01);
                tiles.WriteWord(columnOffset + destinationOffset + 18, planes23);
            }

            if (move)
            {
                // Move clears even when Y is 46/47 and the copy was suppressed. That subtle
                // asymmetry is how the bottom rows disappear without writing past height 48.
                tiles.WriteWord(columnOffset + sourceOffset, 0);
                tiles.WriteWord(columnOffset + sourceOffset + 16, 0);
            }
        }
    }
}
