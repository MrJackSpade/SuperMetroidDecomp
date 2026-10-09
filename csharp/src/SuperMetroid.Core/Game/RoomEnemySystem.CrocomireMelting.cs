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
            CrocomireMeltingArtworkAddresses.FirstTilemap);
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
            CrocomireMeltingArtworkAddresses.SecondTilemap);
        FillCrocomireBg2ScrollTable(CrocomireBg2VerticalScroll);
        StartCrocomireMeltingTilemap(state, CrocomireInstructionProgramDefinitions.MeltingTwoTopRow);
        // $A4:940E-941D clears the first $800 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 0x400);
    }

    // Resolve the complete resource before advancing the phase or touching actors,
    // scratch buffers or VRAM. Recoverable host errors must not partially start a melt.
    /// <summary>Requires installed tilemap artwork for a native Crocomire melt address.</summary>
    /// <param name="tilemapAddress">Native address identifying the first or second melt tilemap.</param>
    /// <returns>The complete tilemap words used to initialize the BG2 working image.</returns>
    /// <exception cref="InvalidDataException">The installation has no Crocomire melting artwork.</exception>
    private ReadOnlySpan<ushort> RequireCrocomireMeltingTilemap(int tilemapAddress) =>
        (TileArtwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed tilemap artwork.")).Tilemap(tilemapAddress);

    /// <summary>Initializes the per-column dissolve height and redirects the body to the selected melt list.</summary>
    /// <param name="state">Crocomire encounter state whose death phase and body instruction pointer are updated.</param>
    /// <param name="bodyInstructionList">Compiled body list for the current melt sequence.</param>
    private void StartCrocomireMeltingTilemap(CrocomireEnemyState state, ushort bodyInstructionList)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.PixelsToErasePerColumn = 48;
        death.TargetHeightOrSkeletonTileIndex = 48;
        state.DeathSequenceIndex += 2;
        InstallCrocomireInstructionList(state.Body, bodyInstructionList);
    }

    /// <summary>Clears the requested BG2 words, places the compact image after its blank lead-in, and starts the initial transfer.</summary>
    /// <param name="tilemap">Tilemap words copied into the mutable BG2 working image.</param>
    /// <param name="clearedWords">Number of leading working words cleared for this melt phase.</param>
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
        state.DeathSequenceIndex += 2;
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

    /// <summary>Uploads the next compiled melt graphics slice or advances the phase after the transfer list ends.</summary>
    /// <param name="state">Encounter state whose death-sequence phase tracks transfer-list completion.</param>
    private void UploadNextCrocomireMeltingGraphicsSlice(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        int recordOffset = death.MeltingTableOffset + death.MeltingTransferOffset;
        if (!CrocomireMeltingTransferDefinitions.TryUpload(
            recordOffset, out CrocomireMeltingUpload upload))
        {
            state.DeathSequenceIndex += 2;
            death.MeltingTransferOffset = 0;
            return;
        }

        UploadCrocomireMeltingRecord(upload);
        death.MeltingTransferOffset += 8;
    }

    /// <summary>Copies one validated bank-$7E melt graphics record from the scratch image into VRAM.</summary>
    /// <param name="upload">Compiled transfer descriptor containing source, size, and VRAM destination.</param>
    /// <exception cref="InvalidDataException">The record uses an unexpected source bank or extends beyond the scratch image.</exception>
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

    /// <summary>Enables BG2 melt HDMA, initializes its scanline scroll values, and captures the body's starting X position.</summary>
    /// <param name="state">Encounter state containing the tongue/body position and melt distortion state.</param>
    private void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltingHdmaActive = true;
        int lineCount = Math.Clamp((state.Tongue?.YPosition ?? state.Body.YPosition) - 72, 0, 256);
        Span<ushort> scrolls = death.MutableBg2ScrollByScanline;
        scrolls.Slice(0, lineCount).Fill(CrocomireBg2VerticalScroll);
        if (lineCount < scrolls.Length)
            scrolls.Slice(lineCount).Fill(CrocomireBg2VerticalScroll);

        state.DeathSequenceIndex += 2;
        death.BodyXBeforeMelting = state.Body.XPosition;
    }

    /// <summary>Advances acid effects, column erasure, body rumble, and the changing BG2 distortion during a melt frame.</summary>
    /// <param name="state">Encounter state updated by the active dissolve.</param>
    /// <param name="samus">Optional player state used by the acid-smoke effect.</param>
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

    /// <summary>Builds per-scanline BG2 offsets from the current melt distortion fraction and vertical bounds.</summary>
    /// <param name="state">Encounter state supplying the tongue/body screen position.</param>
    /// <param name="death">Melt state containing adjusted Y values and the distortion step.</param>
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

    /// <summary>Stops dissolve HDMA and moves the cursor to the next compiled melt-transfer pass.</summary>
    /// <param name="state">Encounter state whose death-sequence phase advances past the completed dissolve.</param>
    private void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = RequireCrocomireDeath();
        death.MeltingHdmaActive = false;
        state.DeathSequenceIndex += 2;

        death.MeltingTableOffset = CrocomireMeltingTransferDefinitions.Transfers(
            death.MeltingTableOffset).NextHeaderOffset;
        death.MeltingTransferOffset = 0;
    }

    /// <summary>Resets Crocomire's reaction counters and clears/uploads the full BG2 working tilemap between melt passes.</summary>
    /// <param name="state">Encounter state whose melt pacing counters and death phase are reset or advanced.</param>
    private void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.ReactionTimer = 0;
        state.ProjectileCounter = 0;
        state.StepCounter = 0x0800;
        ClearCrocomireBg2WorkingTilemap();
        TransferCrocomireBg2Words(0, 1024);
        state.DeathSequenceIndex += 2;
    }
}
