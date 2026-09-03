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
        Phase == SamusDeathSequencePhase.SuitExplosion && AnimationIndex < 9
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
        ReadOnlySpan<byte> initialFrames =
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

        return new SamusDeathSequenceStartResult(
            sourceMovementType,
            deathPose,
            initialFrame,
            ScreenX,
            ScreenY,
            SpinJumpSoundRequested);
    }

    /// <summary>
    /// Executes one game-state `$16/$17/$18` call. Palette writes target the same CGRAM
    /// slots as native, and tile segments enter the ordinary NMI-drained VRAM queue.
    /// </summary>
    public SamusDeathSequenceStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        SnesCgram cgram,
        VramWriteQueue vramWrites)
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
                    WritePalettePair(bus, samus, cgram, paletteIndex: 0);
                    paletteChanged = true;
                    QueueSegment(vramWrites, segmentIndex: 4);
                    AnimationTimer = ReadExplosionTimer(bus, index: 0);
                    AnimationIndex = 0;
                    AnimationCounter = 0;
                    Phase = SamusDeathSequencePhase.SuitExplosion;
                    drawExplosion = StepSuitExplosion(
                        bus,
                        samus,
                        cgram,
                        applyWhiteoutFirst: false,
                        ref paletteChanged,
                        ref whiteoutChanged);
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
                        WritePalettePair(bus, samus, cgram, AnimationIndex);
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
                    ref whiteoutChanged);
                break;

            case SamusDeathSequencePhase.Complete:
                // Native advances to the room fade with no Samus draw. The outer game-state
                // owner has not been translated into this gameplay host, so remain locked.
                break;

            default:
                throw new InvalidOperationException($"Unknown death phase {Phase}.");
        }

        return new SamusDeathSequenceStepResult(
            phaseAtStart,
            Phase,
            indexAtStart,
            AnimationIndex,
            AnimationTimer,
            AnimationCounter,
            LastQueuedSegment,
            paletteChanged,
            whiteoutChanged,
            drawPose,
            drawExplosion,
            ExplosionSpritemapIndex,
            Phase == SamusDeathSequencePhase.Complete);
    }

    /// <summary>Emits `$92:EDBE`'s one right/left explosion spritemap at captured screen position.</summary>
    public void DrawExplosion(ISnesAddressSpace bus, OamBuffer oam)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        if (ExplosionSpritemapIndex is not ushort spritemap)
            return;
        oam.AddSamusSpritemap(bus, spritemap, ScreenX, ScreenY);
    }

    private bool StepSuitExplosion(
        ISnesAddressSpace bus,
        SamusState samus,
        SnesCgram cgram,
        bool applyWhiteoutFirst,
        ref bool paletteChanged,
        ref bool whiteoutChanged)
    {
        if (applyWhiteoutFirst)
            whiteoutChanged = ApplyWhiteout(bus, cgram);

        AnimationTimer = unchecked((ushort)(AnimationTimer - 1));
        if (AnimationTimer != 0 && (AnimationTimer & 0x8000) == 0)
            return true;

        AnimationIndex = unchecked((ushort)(AnimationIndex + 1));
        if (AnimationIndex >= 9)
        {
            // The terminal call writes shade 21 (`$7FFF`) across every non-Samus/non-
            // suitless palette and returns one without drawing a tenth explosion frame.
            AnimationCounter = 0x0015;
            whiteoutChanged = ApplyWhiteout(bus, cgram);
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }

        AnimationTimer = ReadExplosionTimer(bus, AnimationIndex);
        ushort paletteIndex = ReadExplosionPaletteIndex(bus, AnimationIndex);
        WritePalettePair(bus, samus, cgram, paletteIndex);
        paletteChanged = true;
        return true;
    }

    private bool ApplyWhiteout(ISnesAddressSpace bus, SnesCgram cgram)
    {
        // Index zero suppresses whiteout completely. This is why the first 21-tick explosion
        // frame remains over the original room even though game state `$18` has begun.
        if (AnimationIndex == 0)
            return false;

        ushort shade = ReadWord(
            bus,
            SamusPaletteRomData.Death.WhiteoutShades + AnimationCounter * sizeof(ushort));
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

    private static void WritePalettePair(
        ISnesAddressSpace bus,
        SamusState samus,
        SnesCgram cgram,
        ushort paletteIndex)
    {
        // SuitPaletteIndex is 0/2/4. Multiplying it by ten skips each twenty-byte pointer
        // family (ten little-endian pointers) and exactly reproduces `$9B:B5D1-$B5E6`.
        ushort suitIndex = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? (ushort)4 :
            samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? (ushort)2 : (ushort)0;
        ushort suitPointer = ReadWord(
            bus,
            SamusPaletteRomData.Death.SuitPointers +
                suitIndex * SamusPaletteRomData.Death.PaletteCount + paletteIndex * sizeof(ushort));
        ushort suitlessPointer = ReadWord(
            bus,
            SamusPaletteRomData.Death.SuitlessPointers + paletteIndex * sizeof(ushort));

        LoadPalette(
            bus,
            cgram,
            SamusPaletteRomData.Banks.Palette | suitPointer,
            SamusPaletteRomData.Common.SamusObjPaletteStart);
        LoadPalette(
            bus,
            cgram,
            SamusPaletteRomData.Banks.Palette | suitlessPointer,
            SamusPaletteRomData.Common.SuitlessObjPaletteStart);
    }

    private static void LoadPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        int address,
        int destinationColor)
    {
        for (int color = 0; color < SamusPaletteRomData.Common.ColorsPerObjPalette; color++)
        {
            cgram.SetColor(
                destinationColor + color,
                ReadWord(bus, address + color * sizeof(ushort)));
        }
    }

    private void QueueSegment(VramWriteQueue vramWrites, byte segmentIndex)
    {
        ReadOnlySpan<SamusDeathTileSegment> segments =
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

    private static ushort ReadExplosionTimer(ISnesAddressSpace bus, ushort index) =>
        bus.ReadByte(
            SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices + index * 2);

    private static ushort ReadExplosionPaletteIndex(ISnesAddressSpace bus, ushort index) =>
        bus.ReadByte(
            SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices + index * 2 + 1);

    private static ushort ReadWord(ISnesAddressSpace bus, int address)
    {
        SnesAddress source = SnesAddress.FromBusAddress(address);
        return unchecked((ushort)(
            bus.ReadByte((int)source) |
            (bus.ReadByte((int)source.AddWithinBank(1)) << 8)));
    }
}

/// <summary>Game-state phases that own Samus after fatal damage has reached bank `$9B`.</summary>
public enum SamusDeathSequencePhase
{
    Inactive,
    PreFlashing,
    Flashing,
    SuitExplosion,
    Complete,
}

/// <summary>Inspectable output from `$9B:B3A7`'s pose-selection boundary.</summary>
public readonly record struct SamusDeathSequenceStartResult(
    SamusMovementType SourceMovementType,
    byte DeathPose,
    ushort InitialFrame,
    ushort ScreenX,
    ushort ScreenY,
    bool SpinJumpSoundRequested);

/// <summary>One native death game-state call, including draw and NMI-transfer requests.</summary>
public readonly record struct SamusDeathSequenceStepResult(
    SamusDeathSequencePhase PhaseAtStart,
    SamusDeathSequencePhase PhaseAfterStep,
    ushort IndexAtStart,
    ushort IndexAfterStep,
    ushort TimerAfterStep,
    ushort CounterAfterStep,
    byte? QueuedSegment,
    bool PaletteChanged,
    bool WhiteoutChanged,
    bool DrawPose,
    bool DrawExplosion,
    ushort? ExplosionSpritemapIndex,
    bool Completed);
