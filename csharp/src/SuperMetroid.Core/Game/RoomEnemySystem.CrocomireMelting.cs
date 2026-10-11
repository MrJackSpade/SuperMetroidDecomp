using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Installed melt artwork, compiled transfer scheduling, per-column erasure and BG2 distortion.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_10_Hop_3_LoadMeltingTilemap</c> ($A4:9341). Only
    /// this first melt hands the tongue its melting program and moves it onto the body.
    /// </summary>
    private void LoadFirstCrocomireMeltingTilemap(CrocomireEnemyState state)
    {
        ReadOnlySpan<ushort> tilemap = RequireCrocomireMeltingTilemap(
            CrocomireMeltingTilemapAddress.FirstTilemap);
        StartCrocomireMeltingTilemap(state, CrocomireInstructionProgramDefinitions.MeltingOneTopRow);

        if (state.Tongue is { } tongue)
        {
            InstallCrocomireInstructionList(
                tongue,
                CrocomireTongueInstructionProgramDefinitions.Melting);
            tongue.Properties = tongue.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision |
                    EnemyProperties.Invisible,
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision);
            tongue.XPosition = state.Body.XPosition;
            tongue.YPosition = unchecked((ushort)(state.Body.YPosition + 16));
        }

        // $A4:938D-93A2 clears the first $400 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 0x200);
    }

    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_2C_Hop_6_LoadMeltingTilemap</c> ($A4:93ED). It
    /// first refills the BG2 Y-scroll HDMA table ($A4:93DF) and leaves the tongue alone.
    /// </summary>
    private void LoadSecondCrocomireMeltingTilemap(CrocomireEnemyState state)
    {
        ReadOnlySpan<ushort> tilemap = RequireCrocomireMeltingTilemap(
            CrocomireMeltingTilemapAddress.SecondTilemap);
        FillCrocomireBg2ScrollTable(CrocomireBg2VerticalScroll);
        StartCrocomireMeltingTilemap(state, CrocomireInstructionProgramDefinitions.MeltingTwoTopRow);
        // $A4:940E-941D clears the first $800 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 0x400);
    }

    // Resolve the complete resource before advancing the phase or touching actors,
    // scratch buffers or VRAM. Recoverable host errors must not partially start a melt.
    private ReadOnlySpan<ushort> RequireCrocomireMeltingTilemap(CrocomireMeltingTilemapAddress tilemapAddress) =>
        (TileArtwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed tilemap artwork.")).Tilemap(tilemapAddress);

    private void StartCrocomireMeltingTilemap(CrocomireEnemyState state, ushort bodyInstructionList)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.PixelsToErasePerColumn = 48;
        death.TargetHeightOrSkeletonTileIndex = 48;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        InstallCrocomireInstructionList(state.Body, bodyInstructionList);
    }

    private void WriteCrocomireMeltingTilemap(ReadOnlySpan<ushort> tilemap, int clearedWords)
    {
        Span<ushort> working = RequireCrocomireDeath().MutableBg2WorkingTilemap;
        working[..clearedWords].Fill(CrocomireBlankBg2Tile);
        int copiedWords = tilemap.Length;
        tilemap.CopyTo(working[32..]);

        // $A4:93BE receives byte count `tilemap bytes + $0400`, so the initial upload also
        // includes 512 blank leading words and retains blank space after the compact image.
        TransferCrocomireBg2Words(0, 512 + copiedWords);
    }

    /// <summary>Ports the shipped off-by-one ROM-to-$7E:4000 copies at $A4:943D.</summary>
    private void InitializeCrocomireMeltingGraphics(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        CrocomireMeltingPass pass = CrocomireMeltingTransferDefinitions.Header(
            death.MeltingTableOffset);
        var artwork = TileArtwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed graphics artwork.");

        // Both assets have already passed geometry/padding validation during installation.
        // CopyPassTo checks scratch capacity before its first write. Only then publish the
        // new phase and distortion state; no cartridge reader is available as a fallback.
        artwork.CopyPassTo(pass.HeaderOffset, death.MutableMeltingGraphics);
        FillCrocomireBg2ScrollTable(CrocomireBg2VerticalScroll);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.DistortionStep = 0x0100;
        death.MeltingColumnCursor = 0;

        death.MaximumAdjustedDestinationY = pass.MaximumAdjustedDestinationY;
        death.AdjustedDestinationY = death.MaximumAdjustedDestinationY;
        death.DistortionEndY = pass.DistortionEndY;

        // Keep the native cursor so serialized mid-melt states resume at the same record.
        death.MeltingTableOffset = pass.TransferStartOffset;
        death.MeltingTransferOffset = 0;
        death.MutableMeltingColumnHeights.Clear();
    }

    private void UploadNextCrocomireMeltingGraphicsSlice(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
        if (!CrocomireMeltingTransferDefinitions.TryUpload(
            recordOffset, out CrocomireMeltingUpload upload))
        {
            state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
            death.MeltingTransferOffset = 0;
            return;
        }

        UploadCrocomireMeltingRecord(upload);
        death.MeltingTransferOffset += 8;
    }

    private void UploadCrocomireMeltingRecord(CrocomireMeltingUpload upload)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        int byteCount = upload.ByteCount;
        ushort destinationWord = upload.DestinationWord;
        byte sourceBank = upload.SourceBank;
        ushort source = upload.SourceWord;
        if (sourceBank != 0x7e)
        {
            throw new InvalidDataException(
                $"Crocomire melting VRAM record uses unexpected source bank ${sourceBank:X2}.");
        }

        int sourceOffset = unchecked((ushort)(source - 0x4000));
        if (sourceOffset < 0 || sourceOffset + byteCount > death.MeltingGraphics.Length)
            throw new InvalidDataException("Crocomire melting VRAM record exceeds the scratch image.");
        _vram!.LoadBytes(destinationWord * 2, death.MeltingGraphics.Slice(sourceOffset, byteCount));
    }

    private void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltingHdmaActive = true;
        int lineCount = Math.Clamp((state.Tongue?.YPosition ?? state.Body.YPosition) - 72, 0, 256);
        Span<ushort> scrolls = death.MutableBg2ScrollByScanline;
        scrolls.Slice(0, lineCount).Fill(CrocomireBg2VerticalScroll);
        if (lineCount < scrolls.Length)
            scrolls.Slice(lineCount).Fill(CrocomireBg2VerticalScroll);

        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.BodyXBeforeMelting = state.Body.XPosition;
    }

    private void RunCrocomireMelting(CrocomireEnemyState state, SamusState? samus)
    {
        SpawnCrocomireAcidSmoke(state, samus);
        CrocomireDeathState death = RequireCrocomireDeath();
        death.RumbleYOffset--;
        state.Body.XPosition = unchecked((ushort)(
            death.BodyXBeforeMelting + ((death.RumbleYOffset & 2) != 0 ? 4 : 0)));

        if (!EraseNextCrocomireMeltingColumn())
        {
            CompleteCrocomireMeltingDissolve(state);
            return;
        }

        // $A4:95D5 refreshes the BG2 scroll, which also moves the tongue onto the body.
        UpdateCrocomireBg2Scroll(state, includeVerticalPosition: true);
        ushort nextAdjustedY = unchecked((ushort)(death.AdjustedDestinationY - 3));
        if (unchecked((short)(death.AdjustedDestinationY - 19)) < 0)
        {
            if (unchecked((short)(death.DistortionStep - 0x5000)) >= 0)
            {
                CompleteCrocomireMeltingDissolve(state);
                return;
            }
            nextAdjustedY = 16;
        }
        death.AdjustedDestinationY = nextAdjustedY;
        death.DistortionStep = unchecked((ushort)Math.Min(
            death.DistortionStep + 0x0180,
            0x5000));
        BuildCrocomireMeltingScrollTable(state, death);
    }

    /// <summary>Ports the retail column-order and four-bitplane mask loop at $A4:96C8.</summary>
    private bool EraseNextCrocomireMeltingColumn()
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        Span<byte> heights = death.MutableMeltingColumnHeights;
        Span<byte> graphics = death.MutableMeltingGraphics;

        int cursor = death.MeltingColumnCursor;
        int xColumn;
        while (true)
        {
            // The retail table at $A4:9697 contains one permutation of X=0..48 and then
            // immediately falls into executable opcodes. The upstream C translation adds
            // this same `index > 48` guard to prevent those opcodes from becoming bogus X
            // coordinates while the distortion coefficient finishes its last few frames.
            // Returning success is important: it keeps the HDMA contraction running; false
            // would skip directly to the next death state several frames too early.
            if (cursor >= CrocomireMeltingDefinitions.ColumnCount)
            {
                death.MeltingColumnCursor = unchecked((ushort)cursor);
                return true;
            }
            xColumn = CrocomireMeltingDefinitions.SelectColumn(cursor);
            if (heights[xColumn] < death.TargetHeightOrSkeletonTileIndex)
                break;
            cursor++;
        }
        death.MeltingColumnCursor = unchecked((ushort)cursor);

        // The shipped routine masks by [table index & 7], not [selected X & 7]. This is one
        // of its documented indexing bugs and materially changes the dissolve silhouette.
        byte mask = CrocomireMeltingDefinitions.SelectMask(cursor);
        int remaining = death.PixelsToErasePerColumn;
        do
        {
            int y = heights[xColumn];
            int byteIndex = 2 * (y & 7) + ((y & ~7) << 6) + 4 * (xColumn & ~7);
            if (byteIndex + 17 >= graphics.Length)
                throw new InvalidDataException("Crocomire melting column addressed outside its graphics image.");
            graphics[byteIndex] &= mask;
            graphics[byteIndex + 1] &= mask;
            graphics[byteIndex + 16] &= mask;
            graphics[byteIndex + 17] &= mask;
            if (heights[xColumn] == 48)
                break;
            heights[xColumn]++;
        } while (--remaining != 0);

        int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
        if (!CrocomireMeltingTransferDefinitions.TryUpload(
            recordOffset, out CrocomireMeltingUpload upload))
        {
            death.MeltingTransferOffset = 0;
            recordOffset = death.MeltingTableOffset;
            if (!CrocomireMeltingTransferDefinitions.TryUpload(recordOffset, out upload))
                throw new InvalidDataException("Crocomire melt pass has no first transfer.");
        }
        UploadCrocomireMeltingRecord(upload);
        death.MeltingTransferOffset += 8;
        return true;
    }

    private void BuildCrocomireMeltingScrollTable(
        CrocomireEnemyState state,
        CrocomireDeathState death)
    {
        Span<ushort> output = death.MutableBg2ScrollByScanline;
        int line = Math.Clamp((state.Tongue?.YPosition ?? state.Body.YPosition) - 72, 0, 256);
        ushort adjustedY = death.AdjustedDestinationY;
        ushort currentY = adjustedY;
        uint fraction = 0;
        while (line < output.Length &&
               unchecked((short)(adjustedY - death.MaximumAdjustedDestinationY)) < 0)
        {
            output[line++] = unchecked((ushort)(
                CrocomireBg2VerticalScroll + adjustedY - currentY));
            uint sum = fraction + death.DistortionStep;
            bool carry = sum > 0xffff;
            fraction = sum & 0xffff;
            if (!carry)
                adjustedY++;
            currentY++;
        }
        if (line < output.Length)
            output.Slice(line).Fill(CrocomireBg2VerticalScroll);
    }

    private void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltingHdmaActive = false;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();

        death.MeltingTableOffset = CrocomireMeltingTransferDefinitions.Transfers(
            death.MeltingTableOffset).NextHeaderOffset;
        death.MeltingTransferOffset = 0;
    }

    private void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        state.StepCounter = 0x0800;
        ClearCrocomireBg2WorkingTilemap();
        TransferCrocomireBg2Words(0, 1024);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }
}
