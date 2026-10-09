using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Owns Samus's bank-$9B fatal-damage pose, tile, palette, flashing, whiteout, and suit-
/// explosion sequence. This is deliberately not folded into knockback merely because poses
/// `$D7/$D8` advertise movement type `$0A`: game states `$15-$18` stop the normal Samus frame
/// handlers and call this independent state machine instead.
/// </summary>
public sealed class SamusDeathSequenceState
{
    /// <summary>The game-state-owned phase corresponding to native states `$16-$18`.</summary>
    public SamusDeathSequencePhase Phase { get; private set; }

    /// <summary>
    /// True from `$9B:B3A7` setup through the terminal white frame. A completed sequence
    /// remains an owner until the outer game-state fade replaces the room, preventing normal
    /// type-`$0A` knockback code from accidentally resuming on pose `$D7/$D8`.
    /// </summary>
    public bool IsActive => Phase != SamusDeathSequencePhase.Inactive;

    /// <summary>WRAM `$0A9A`; sixteen calls precede the alternating yellow flash.</summary>
    public ushort PreFlashingTimer { get; private set; }

    /// <summary>WRAM `$0A9C`; reused by flash and suit-explosion palette timing.</summary>
    public ushort AnimationTimer { get; private set; }

    /// <summary>WRAM `$0A9E`; flash selector zero/one, then explosion index zero through nine.</summary>
    public ushort AnimationIndex { get; private set; }

    /// <summary>WRAM `$0AA0`; 60-call flashing counter, then whiteout shade index.</summary>
    public ushort AnimationCounter { get; private set; }

    /// <summary>Screen-space X captured by `$9B:B409-$B410` at death-pose setup.</summary>
    public ushort ScreenX { get; private set; }

    /// <summary>Screen-space Y captured by `$9B:B413-$B41A` at death-pose setup.</summary>
    public ushort ScreenY { get; private set; }

    /// <summary>Set only when the source movement type was spin jumping and SFX `$32` is queued.</summary>
    public bool SpinJumpSoundRequested { get; private set; }

    /// <summary>Consumes death setup's native <c>QueueSfx1_Max6($32)</c> publication.</summary>
    public bool ConsumeSpinJumpSoundRequest()
    {
        bool requested = SpinJumpSoundRequested;
        SpinJumpSoundRequested = false;
        return requested;
    }

    /// <summary>Most recently queued zero-based `$400` graphics segment, or null.</summary>
    public byte? LastQueuedSegment { get; private set; }

    /// <summary>Current `$92:808D` spritemap-table index used by `$92:EDBE`, or null.</summary>
    public ushort? ExplosionSpritemapIndex =>
        Phase == SamusDeathSequencePhase.SuitExplosion &&
            AnimationIndex < SamusDeathExplosionTimingDefinitions.RecordCount
            ? unchecked((ushort)((FacingLeft
                ? SamusSpecialSequenceRomData.Death.LeftExplosionSpritemap
                : SamusSpecialSequenceRomData.Death.RightExplosionSpritemap) + AnimationIndex))
            : null;

    /// <summary>The facing bit captured before pose metadata is replaced by `$D7/$D8`.</summary>
    public bool FacingLeft { get; private set; }

    /// <summary>
    /// Ports <c>SetSamusDeathSequencePose</c> at `$9B:B3A7`. The outer caller represents
    /// game state `$15` having observed an empty music queue; fatal-damage acquisition and
    /// the music wait are distinct producers and are not guessed here.
    /// </summary>
    public SamusDeathSequenceStartResult Begin(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (IsActive)
            throw new InvalidOperationException("Samus death sequence is already owned by bank $9B.");

        SamusMovementType sourceMovementType = samus.ReadMovementType(bus);
        var initialFrames =
            SamusSpecialSequenceRomData.Death.InitialFramesByMovementType;
        if ((byte)sourceMovementType >= initialFrames.Length)
        {
            throw new InvalidDataException(
                $"Death-pose frame table has no movement type ${(byte)sourceMovementType:X2}.");
        }

        FacingLeft = samus.IsFacingLeft(bus);
        byte deathPose = FacingLeft
            ? SamusPoseIds.DeathSequenceLeftPose
            : SamusPoseIds.DeathSequenceRightPose;
        ushort initialFrame = initialFrames[(byte)sourceMovementType];

        // Native initializes pose `$D7/$D8` normally, then overwrites only the visible frame
        // with the movement-type table result. All six delays are two, so the timer produced
        // by frame-zero initialization remains the correct literal value for every start.
        samus.Pose = deathPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        // Death entry publishes history itself, before overriding the starting
        // animation frame; the ordinary gameplay pose dispatcher is not running.
        samus.CommitPoseHistory(bus);
        samus.SetAnimationFrameFromSpecialHandler(initialFrame, timer: 2);

        // The cartridge subtracts layer 1 directly from Samus's live words because its death
        // renderer adds layer 1 back before emitting OAM. Retain world coordinates in Samus
        // for this host's shared camera/minimap model and capture the mathematically identical
        // screen-space pair here for the eventual explosion-spritemap path.
        ScreenX = unchecked((ushort)(samus.XPosition - layer1X));
        ScreenY = unchecked((ushort)(samus.YPosition - layer1Y));

        PreFlashingTimer = SamusSpecialSequenceRomData.Death.PreFlashFrameCount;
        AnimationTimer = 3;
        AnimationIndex = 0;
        AnimationCounter = 0;
        SpinJumpSoundRequested = sourceMovementType == SamusMovementType.SpinJumping;
        LastQueuedSegment = null;
        Phase = SamusDeathSequencePhase.PreFlashing;

        return new SamusDeathSequenceStartResult();
    }

    /// <summary>
    /// Executes one game-state `$16/$17/$18` call. Palette writes target the same CGRAM
    /// slots as native, and tile segments enter the ordinary NMI-drained VRAM queue.
    /// </summary>
    public SamusDeathSequenceStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        SnesCgram cgram,
        VramWriteQueue vramWrites,
        SamusDeathPaletteArtworkCatalog? artwork = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(vramWrites);
        if (!IsActive)
            throw new InvalidOperationException("Samus death sequence has not begun.");

        LastQueuedSegment = null;
        SamusDeathSequencePhase phaseAtStart = Phase;
        ushort indexAtStart = AnimationIndex;
        bool paletteChanged = false;
        bool whiteoutChanged = false;
        bool drawPose = false;
        bool drawExplosion = false;

        switch (Phase)
        {
            case SamusDeathSequencePhase.PreFlashing:
                // `$90:8976` advances the `$D7/$D8` delay stream only during state `$16`.
                // State `$17` freezes that final body frame while its palettes alternate.
                samus.AnimateDeathFrame(bus);
                drawPose = true;
                PreFlashingTimer = unchecked((ushort)(PreFlashingTimer - 1));
                if (PreFlashingTimer == 0 || (PreFlashingTimer & 0x8000) != 0)
                    Phase = SamusDeathSequencePhase.Flashing;
                break;

            case SamusDeathSequencePhase.Flashing:
                if (AnimationCounter < 4)
                    QueueSegment(vramWrites, unchecked((byte)AnimationCounter));

                AnimationCounter = unchecked((ushort)(AnimationCounter + 1));
                if (AnimationCounter >= SamusSpecialSequenceRomData.Death.FlashFrameCount)
                {
                    // `$9B:B498` loads palette pair zero, queues the fifth/final graphics
                    // segment, resets the reused counters, and immediately draws explosion
                    // spritemap zero in this very same game-state-17 call.
                    WritePalettePair(bus, samus, cgram, paletteIndex: 0, artwork);
                    paletteChanged = true;
                    QueueSegment(vramWrites, segmentIndex: 4);
                    AnimationTimer =
                        SamusDeathExplosionTimingDefinitions.DurationForIndex(index: 0);
                    AnimationIndex = 0;
                    AnimationCounter = 0;
                    Phase = SamusDeathSequencePhase.SuitExplosion;
                    drawExplosion = StepSuitExplosion(
                        bus,
                        samus,
                        cgram,
                        applyWhiteoutFirst: false,
                        ref paletteChanged,
                        ref whiteoutChanged,
                        artwork);
                }
                else
                {
                    AnimationTimer = unchecked((ushort)(AnimationTimer - 1));
                    if (AnimationTimer == 0 || (AnimationTimer & 0x8000) != 0)
                    {
                        if (AnimationIndex == 0)
                        {
                            AnimationIndex = 1;
                            AnimationTimer = 1;
                        }
                        else
                        {
                            AnimationIndex = 0;
                            AnimationTimer = 3;
                        }
                        WritePalettePair(bus, samus, cgram, AnimationIndex, artwork);
                        paletteChanged = true;
                    }
                    drawPose = true;
                }
                break;

            case SamusDeathSequencePhase.SuitExplosion:
                drawExplosion = StepSuitExplosion(
                    bus,
                    samus,
                    cgram,
                    applyWhiteoutFirst: true,
                    ref paletteChanged,
                    ref whiteoutChanged,
                    artwork);
                break;

            case SamusDeathSequencePhase.Complete:
                // Native advances to the room fade with no Samus draw. The outer game-state
                // owner has not been translated into this gameplay host, so remain locked.
                break;

            default:
                throw new InvalidOperationException($"Unknown death phase {Phase}.");
        }

        return new SamusDeathSequenceStepResult(
            Phase,
            drawPose,
            drawExplosion,
            Phase == SamusDeathSequencePhase.Complete);
    }

    /// <summary>Emits `$92:EDBE`'s one right/left explosion spritemap at captured screen position.</summary>
    public void DrawExplosion(ISnesAddressSpace bus, OamBuffer oam,
        SamusSpritemapArtworkCatalog? artwork = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        if (ExplosionSpritemapIndex is not ushort spritemap)
            return;
        oam.AddSamusSpritemap(bus as ISnesMutableMemory ?? throw new InvalidOperationException(
            "Samus death spritemap requires WRAM."),
            spritemap, ScreenX, ScreenY, artwork ?? throw new InvalidOperationException(
                "Samus death spritemap requires installed artwork."));
    }

    /// <summary>Advances explosion timing, palette changes, and the optional room whiteout for one game-state call.</summary>
    /// <param name="bus">Address space supplied by the game-state caller; palette changes are written through <paramref name="cgram"/>.</param>
    /// <param name="samus">Samus state used to select the equipped-suit palette.</param>
    /// <param name="cgram">CGRAM receiving explosion palette and whiteout colors.</param>
    /// <param name="applyWhiteoutFirst">Whether to apply the current whiteout shade before advancing the explosion timer.</param>
    /// <param name="paletteChanged">Set to <see langword="true"/> when this call writes an explosion palette pair.</param>
    /// <param name="whiteoutChanged">Set to <see langword="true"/> when this call changes room palette colors.</param>
    /// <param name="artwork">Palette data required for explosion frames and whiteout shades.</param>
    /// <returns><see langword="true"/> when the caller should draw the current explosion frame.</returns>
    private bool StepSuitExplosion(
        ISnesAddressSpace bus,
        SamusState samus,
        SnesCgram cgram,
        bool applyWhiteoutFirst,
        ref bool paletteChanged,
        ref bool whiteoutChanged,
        SamusDeathPaletteArtworkCatalog? artwork)
    {
        if (applyWhiteoutFirst)
            whiteoutChanged = ApplyWhiteout(bus, cgram, artwork);

        AnimationTimer = unchecked((ushort)(AnimationTimer - 1));
        if (AnimationTimer != 0 && (AnimationTimer & 0x8000) == 0)
            return true;

        AnimationIndex = unchecked((ushort)(AnimationIndex + 1));
        if (AnimationIndex >= SamusDeathExplosionTimingDefinitions.RecordCount)
        {
            // The terminal call writes shade 21 (`$7FFF`) across every non-Samus/non-
            // suitless palette and returns one without drawing a tenth explosion frame.
            AnimationCounter = 0x0015;
            whiteoutChanged = ApplyWhiteout(bus, cgram, artwork);
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }

        AnimationTimer =
            SamusDeathExplosionTimingDefinitions.DurationForIndex(AnimationIndex);
        ushort paletteIndex = (artwork ?? throw new InvalidOperationException(
            "Samus death requires installed palette artwork.")).ExplosionPaletteIndex(AnimationIndex);
        WritePalettePair(bus, samus, cgram, paletteIndex, artwork);
        paletteChanged = true;
        return true;
    }

    /// <summary>Applies the current whiteout shade to room palettes while preserving Samus and suitless colors.</summary>
    /// <param name="bus">Address-space context supplied by the death-sequence caller; this operation updates <paramref name="cgram"/> directly.</param>
    /// <param name="cgram">Palette memory whose eligible colors are replaced.</param>
    /// <param name="artwork">Table supplying the shade for the current whiteout index.</param>
    /// <returns><see langword="true"/> when a shade was written; index zero leaves CGRAM unchanged.</returns>
    private bool ApplyWhiteout(ISnesAddressSpace bus, SnesCgram cgram,
        SamusDeathPaletteArtworkCatalog? artwork)
    {
        // Index zero suppresses whiteout completely. This is why the first 21-tick explosion
        // frame remains over the original room even though game state `$18` has begun.
        if (AnimationIndex == 0)
            return false;

        ushort shade = (artwork ?? throw new InvalidOperationException(
            "Samus death whiteout requires installed palette artwork.")).WhiteoutColor(AnimationCounter);
        for (int color = 0; color < SamusPaletteRomData.Common.SamusObjPaletteStart; color++)
            cgram.SetColor(color, shade);
        for (int color =
                SamusPaletteRomData.Common.SamusObjPaletteStart +
                    SamusPaletteRomData.Common.ColorsPerObjPalette;
            color < SamusPaletteRomData.Common.SuitlessObjPaletteStart;
            color++)
        {
            cgram.SetColor(color, shade);
        }

        if (AnimationCounter < 0x0014)
            AnimationCounter = unchecked((ushort)(AnimationCounter + 1));
        return true;
    }

    /// <summary>Writes one death-flash or explosion palette pair for the equipped suit and the suitless Samus palette.</summary>
    /// <param name="bus">Address-space context supplied by the death-sequence caller; colors are written through <paramref name="cgram"/>.</param>
    /// <param name="samus">Samus equipment state used to choose the suited palette family.</param>
    /// <param name="cgram">CGRAM receiving the selected palette colors.</param>
    /// <param name="paletteIndex">Palette frame selected by the death sequence.</param>
    /// <param name="artwork">Artwork tables containing suited and suitless color values.</param>
    private static void WritePalettePair(
        ISnesAddressSpace bus,
        SamusState samus,
        SnesCgram cgram,
        ushort paletteIndex,
        SamusDeathPaletteArtworkCatalog? artwork)
    {
        // SuitPaletteIndex is 0/2/4. Multiplying it by ten skips each twenty-byte pointer
        // family (ten little-endian pointers) and exactly reproduces `$9B:B5D1-$B5E6`.
        ushort suitIndex = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? (ushort)4 :
            samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? (ushort)2 : (ushort)0;
        if (artwork is not null)
        {
            for (int color = 0; color < SamusDeathPaletteArtworkCatalog.ColorCount; color++)
            {
                cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + color,
                    artwork.SuitedColor(suitIndex / 2, paletteIndex, color));
                cgram.SetColor(SamusPaletteRomData.Common.SuitlessObjPaletteStart + color,
                    artwork.SuitlessColor(paletteIndex, color));
            }
            return;
        }
        throw new InvalidOperationException("Samus death requires installed palette artwork.");
    }

    /// <summary>Adds one death-animation tile segment to the NMI-drained VRAM queue and records its index.</summary>
    /// <param name="vramWrites">Queue that receives the segment's source and VRAM destination.</param>
    /// <param name="segmentIndex">Zero-based index into the death sequence's tile-segment table.</param>
    private void QueueSegment(VramWriteQueue vramWrites, byte segmentIndex)
    {
        var segments =
            SamusSpecialSequenceRomData.Death.TileSegments;
        if (segmentIndex >= segments.Length)
            throw new ArgumentOutOfRangeException(nameof(segmentIndex));
        SamusDeathTileSegment segment = segments[segmentIndex];
        vramWrites.Enqueue(
            sizeInBytes: SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
            sourceAddress: segment.SourceAddress,
            encodedVramDestination: segment.EncodedVramDestination);
        LastQueuedSegment = segmentIndex;
    }

}

/// <summary>Game-state phases that own Samus after fatal damage has reached bank `$9B`.</summary>
public enum SamusDeathSequencePhase
{
    /// <summary>Normal gameplay owns Samus; no bank-$9B death sequence is active.</summary>
    Inactive,
    /// <summary>Game state $16 advances the fatal pose for sixteen calls before flashing.</summary>
    PreFlashing,
    /// <summary>Game state $17 alternates death palettes and queues the five graphics segments.</summary>
    Flashing,
    /// <summary>Game state $18 advances explosion spritemaps, palettes, and room whiteout.</summary>
    SuitExplosion,
    /// <summary>The terminal white frame is installed and the outer room fade may take ownership.</summary>
    Complete,
}

/// <summary>Inspectable output from `$9B:B3A7`'s pose-selection boundary.</summary>
public readonly record struct SamusDeathSequenceStartResult();

/// <summary>One native death game-state call, including draw and NMI-transfer requests.</summary>
/// <param name="PhaseAfterStep">Phase that owns the sequence after this call has completed.</param>
/// <param name="DrawPose">Whether the normal death-pose sprite should be drawn for this call.</param>
/// <param name="DrawExplosion">Whether the explosion spritemap should be drawn for this call.</param>
/// <param name="Completed">Whether the terminal whiteout frame has transferred ownership to the room fade.</param>
public readonly record struct SamusDeathSequenceStepResult(
    SamusDeathSequencePhase PhaseAfterStep,
    bool DrawPose,
    bool DrawExplosion,
    bool Completed);
