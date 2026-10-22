using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire melting in the acid: the melting tilemaps and graphics and the dissolve passes.</summary>
[RoomLifetime]
internal sealed class CrocomireMelting
{
    /// <summary>Owns Crocomire body, skeleton, and melting tile uploads.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Crocomire's BG2 body: its scroll, scroll table, tilemap transfers and the acid smoke drawn with it.</summary>
    private readonly CrocomireBg2 _bg2;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireMelting(EnemyPresentation presentation, CrocomireBg2 bg2, CrocomireEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _presentation = presentation;
        _bg2 = bg2;
        _access = access;
    }

    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_10_Hop_3_LoadMeltingTilemap</c> ($A4:9341). Only
    /// this first melt hands the tongue its melting program and moves it onto the body.
    /// </summary>
    internal void LoadFirstCrocomireMeltingTilemap(CrocomireEnemyState state)
    {
        ReadOnlySpan<ushort> tilemap = RequireCrocomireMeltingTilemap(
            CrocomireMeltingTilemapAddress.FirstTilemap);
        StartCrocomireMeltingTilemap(state, CrocomireBodyProgram.MeltingOneTopRow);

        if (state.Tongue is { } tongue)
        {
            CrocomireBg2.StartCrocomireTongueProgram(state, CrocomireTongueProgram.Melting, WrappingCountdown.Start(1));
            tongue.Properties = tongue.Properties.Replace(
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision |
                    EnemyProperties.Invisible,
                EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision);
            tongue.Position = tongue.Position.WithWhole(new(state.Body.Position.X.Whole, unchecked((ushort)(state.Body.Position.Y.Whole + 16))));
        }

        // $A4:938D-93A2 clears the first $400 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 512);
    }

    /// <summary>
    /// <c>MainAI_Crocomire_DeathSequence_2C_Hop_6_LoadMeltingTilemap</c> ($A4:93ED). It
    /// first refills the BG2 Y-scroll HDMA table ($A4:93DF) and leaves the tongue alone.
    /// </summary>
    internal void LoadSecondCrocomireMeltingTilemap(CrocomireEnemyState state)
    {
        ReadOnlySpan<ushort> tilemap = RequireCrocomireMeltingTilemap(
            CrocomireMeltingTilemapAddress.SecondTilemap);
        _bg2.FillCrocomireBg2ScrollTable(_bg2.CrocomireBg2Scroll.Y);
        StartCrocomireMeltingTilemap(state, CrocomireBodyProgram.MeltingTwoTopRow);
        // $A4:940E-941D clears the first $800 bytes.
        WriteCrocomireMeltingTilemap(tilemap, clearedWords: 1024);
    }

    /// <summary>Requires a complete installed melting map before phase, actor, scratch, or VRAM state changes.</summary>
    internal ReadOnlySpan<ushort> RequireCrocomireMeltingTilemap(CrocomireMeltingTilemapAddress tilemapAddress) =>
        (_bossArtwork.Artwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed tilemap artwork.")).Tilemap(tilemapAddress);

    /// <summary>Starts the selected body melting list and synchronizes actor state with the new BG2 map.</summary>
    private void StartCrocomireMeltingTilemap(CrocomireEnemyState state, CrocomireBodyProgram bodyProgram)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.PixelsToErasePerColumn = 48;
        death.MeltTargetHeight = 48;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, bodyProgram);
    }

    /// <summary>Stages the melting map with its native blank lead-in and uploads the bounded BG2 span.</summary>
    private void WriteCrocomireMeltingTilemap(ReadOnlySpan<ushort> tilemap, int clearedWords)
    {
        Span<ushort> working = _access.RequireCrocomireDeath().MutableBg2WorkingTilemap;
        working[..clearedWords].Fill(CrocomireBg2.CrocomireBlankBg2Tile);
        int copiedWords = tilemap.Length;
        tilemap.CopyTo(working[32..]);

        // $A4:93BE receives byte count `tilemap bytes + $0400`, so the initial upload also
        // includes 512 blank leading words and retains blank space after the compact image.
        _bg2.TransferCrocomireBg2Words(0, 512 + copiedWords);
    }

    /// <summary>Ports the shipped off-by-one ROM-to-$7E:4000 copies at $A4:943D.</summary>
    internal void InitializeCrocomireMeltingGraphics(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        CrocomireMeltingPass pass = CrocomireMeltingTransferDefinitions.Header(
            death.MeltingTableOffset);
        var artwork = _bossArtwork.Artwork?.CrocomireMelting ?? throw new InvalidDataException(
            "Crocomire melting requires installed graphics artwork.");

        // Both assets have already passed geometry/padding validation during installation.
        // CopyPassTo checks scratch capacity before its first write. Only then publish the
        // new phase and distortion state; no cartridge reader is available as a fallback.
        artwork.CopyPassTo(pass.HeaderOffset, death.MutableMeltingGraphics);
        _bg2.FillCrocomireBg2ScrollTable(_bg2.CrocomireBg2Scroll.Y);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.DistortionStep = 256;
        death.MeltingColumnCursor = 0;

        death.MaximumAdjustedDestinationY = pass.MaximumAdjustedDestinationY;
        death.AdjustedDestinationY = death.MaximumAdjustedDestinationY;
        death.DistortionEndY = pass.DistortionEndY;

        // Keep the native cursor so serialized mid-melt states resume at the same record.
        death.MeltingTableOffset = pass.TransferStartOffset;
        death.MeltingTransferOffset = 0;
        death.MutableMeltingColumnHeights.Clear();
    }

    /// <summary>Transfers the next installed melting record and advances the serializable upload cursor.</summary>
    internal void UploadNextCrocomireMeltingGraphicsSlice(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
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

    /// <summary>
    /// Binds one compiled melting graphics record's slice of the masked image at its native VRAM destination (#1355):
    /// the melt shows through background artwork bindings, not VRAM writes.
    /// </summary>
    [PpuLayoutBoundary("codec: Crocomire melting VRAM transfer records' native destinations")]
    private void UploadCrocomireMeltingRecord(CrocomireMeltingUpload upload)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        int byteCount = upload.ByteCount;
        int destinationWord = upload.DestinationWord;
        int sourceBank = upload.SourceBank;
        int source = upload.SourceWord;
        if (sourceBank != CrocomireMeltingTransferDefinitions.ScratchBank)
        {
            throw new InvalidDataException(
                $"Crocomire melting VRAM record uses unexpected source bank ${sourceBank:X2}.");
        }

        int sourceOffset = unchecked((ushort)(source - CrocomireMeltingTransferDefinitions.ScratchStart));
        if (sourceOffset < 0 || sourceOffset + byteCount > death.MeltingGraphics.Length)
            throw new InvalidDataException("Crocomire melting VRAM record exceeds the scratch image.");
        _presentation.Backgrounds.Bind(MeltingCharacters(destinationWord), destinationWord, death.MeltingGraphics.Slice(sourceOffset, byteCount));
    }

    /// <summary>The name every artwork the melt binds starts with.</summary>
    private const string MeltingCharactersName = "Crocomire melting characters";

    /// <summary>The artwork the melt binds over the characters from <paramref name="destinationWord"/>.</summary>
    private static BgArtworkIdentity MeltingCharacters(int destinationWord) => new($"{MeltingCharactersName} ${destinationWord:X4}");

    /// <summary>Whether <paramref name="artwork"/> is characters the melt rewrote.</summary>
    internal static bool IsMeltingCharacters(BgArtworkIdentity artwork) =>
        artwork.Name.StartsWith(MeltingCharactersName, StringComparison.Ordinal);

    /// <summary>Starts Crocomire's dissolve after installed graphics and tilemap data are ready.</summary>
    internal void BeginCrocomireMelting(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.MeltingHdmaActive = true;
        int lineCount = Math.Clamp((state.Tongue?.Position.Y.Whole ?? state.Body.Position.Y.Whole) - 72, 0, 256);
        Span<ushort> scrolls = death.MutableBg2ScrollByScanline;
        scrolls.Slice(0, lineCount).Fill(_bg2.CrocomireBg2Scroll.Y);
        if (lineCount < scrolls.Length)
            scrolls.Slice(lineCount).Fill(_bg2.CrocomireBg2Scroll.Y);

        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
        death.BodyXBeforeMelting = state.Body.Position.X.Whole;
    }

    /// <summary>Updates the acid dissolve, BG2 distortion, tongue placement, and scheduled melting graphics.</summary>
    internal void RunCrocomireMelting(CrocomireEnemyState state, SamusState? samus)
    {
        _bg2.SpawnCrocomireAcidSmoke(state, samus);
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.MeltShakeCounter = death.MeltShakeCounter.Decrement().Remaining;
        state.Body.Position = state.Body.Position.WithWholeX(unchecked((ushort)(
            death.BodyXBeforeMelting + ((death.MeltShakeCounter.RawValue & 2) != 0 ? 4 : 0))));

        if (!EraseNextCrocomireMeltingColumn())
        {
            CompleteCrocomireMeltingDissolve(state);
            return;
        }

        // $A4:95D5 refreshes the BG2 scroll, which also moves the tongue onto the body.
        _bg2.UpdateCrocomireBg2Scroll(state);
        int nextAdjustedY = death.AdjustedDestinationY - 3;
        if (death.AdjustedDestinationY < 19)
        {
            if (death.DistortionStep >= CrocomireBehavior.MaximumCrocomireDistortionStep)
            {
                CompleteCrocomireMeltingDissolve(state);
                return;
            }
            nextAdjustedY = 16;
        }
        death.AdjustedDestinationY = nextAdjustedY;
        death.DistortionStep = Math.Min(
            death.DistortionStep + 384,
            CrocomireBehavior.MaximumCrocomireDistortionStep);
        BuildCrocomireMeltingScrollTable(state, death);
    }

    /// <summary>Ports the retail column-order and four-bitplane mask loop at $A4:96C8.</summary>
    internal bool EraseNextCrocomireMeltingColumn()
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
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
                death.MeltingColumnCursor = cursor;
                return true;
            }
            xColumn = CrocomireMeltingDefinitions.SelectColumn(cursor);
            if (heights[xColumn] < death.MeltTargetHeight)
                break;
            cursor++;
        }
        death.MeltingColumnCursor = cursor;

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

    /// <summary>Builds scanline BG2 offsets that distort Crocomire's body through the melting pass.</summary>
    private void BuildCrocomireMeltingScrollTable(
        CrocomireEnemyState state,
        CrocomireDeathState death)
    {
        Span<ushort> output = death.MutableBg2ScrollByScanline;
        int line = Math.Clamp((state.Tongue?.Position.Y.Whole ?? state.Body.Position.Y.Whole) - 72, 0, 256);
        int adjustedY = death.AdjustedDestinationY;
        int currentY = adjustedY;
        ushort fraction = 0;
        while (line < output.Length &&
               adjustedY < death.MaximumAdjustedDestinationY)
        {
            output[line++] = unchecked((ushort)(
                _bg2.CrocomireBg2Scroll.Y + adjustedY - currentY));
            bool carry = false;
            fraction = SnesCarry.AddWithCarry(fraction, checked((ushort)death.DistortionStep), ref carry);
            if (!carry)
                adjustedY++;
            currentY++;
        }
        if (line < output.Length)
            output.Slice(line).Fill(_bg2.CrocomireBg2Scroll.Y);
    }

    /// <summary>Ends the current dissolve and selects the next death-scene instruction phase.</summary>
    internal void CompleteCrocomireMeltingDissolve(CrocomireEnemyState state)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.MeltingHdmaActive = false;
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();

        death.MeltingTableOffset = CrocomireMeltingTransferDefinitions.Transfers(
            death.MeltingTableOffset).NextHeaderOffset;
        death.MeltingTransferOffset = 0;
    }

    /// <summary>Restores the BG2 presentation and actor state after the final melting pass.</summary>
    internal void FinishCrocomireMeltingPass(CrocomireEnemyState state)
    {
        state.StartVerticalMotion(SnesEightEight.FromWhole(CrocomireBehavior.SinkStartingAccelerationWhole));
        _bg2.ClearCrocomireBg2WorkingTilemap();
        _bg2.TransferCrocomireBg2Words(0, 1024);
        state.DeathSequenceIndex = state.DeathSequenceIndex.Next();
    }
}
