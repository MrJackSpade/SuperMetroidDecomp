using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Death;

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
    public WrappingCountdown PreFlashingTimer { get; private set; }

    /// <summary>WRAM `$0A9C`; reused by flash and suit-explosion palette timing.</summary>
    public WrappingCountdown AnimationTimer { get; private set; }

    /// <summary>
    /// Flash palette selector (zero or one) while flashing, then the suit-explosion frame index from zero up to
    /// <see cref="SamusDeathExplosionTimingDefinitions.RecordCount"/>, where the sequence completes.
    /// </summary>
    public int AnimationIndex { get; private set; }

    /// <summary>Flashing-call count up to the 60-call flash, then the whiteout shade index, which saturates at 20 and ends at 21.</summary>
    public int AnimationCounter { get; private set; }

    /// <summary>Screen-space position captured by `$9B:B409-$B41A` at death-pose setup.</summary>
    public Position16<ScreenPixels> ScreenPosition { get; private set; }

    /// <summary>Set only when the source movement type was spin jumping and SFX `$32` is queued.</summary>
    public bool SpinJumpSoundRequested { get; private set; }

    /// <summary>Consumes death setup's native <c>QueueSfx1_Max6($32)</c> publication.</summary>
    public bool ConsumeSpinJumpSoundRequest()
    {
        bool requested = SpinJumpSoundRequested;
        SpinJumpSoundRequested = false;
        return requested;
    }

    /// <summary>The suit-explosion frame shown, or null outside the explosion.</summary>
    public int? ExplosionFrame =>
        Phase == SamusDeathSequencePhase.SuitExplosion && AnimationIndex < SamusDeathExplosionTimingDefinitions.RecordCount
            ? AnimationIndex : null;

    /// <summary>The facing bit captured before pose metadata is replaced by `$D7/$D8`.</summary>
    public bool FacingLeft { get; private set; }

    /// <summary>
    /// Ports <c>SetSamusDeathSequencePose</c> at `$9B:B3A7`. The outer caller represents
    /// game state `$15` having observed an empty music queue; fatal-damage acquisition and
    /// the music wait are distinct producers and are not guessed here.
    /// </summary>
    public SamusDeathSequenceStartResult Begin(
        SamusState samus,
        Position16<RoomPixels> layer1)
    {
        Ensure.NotNull(samus);
        if (IsActive)
            throw new InvalidOperationException("Samus death sequence is already owned by bank $9B.");

        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        var initialFrames =
            SamusSpecialSequenceRomData.Death.InitialFramesByMovementType;
        if ((byte)sourceMovementType >= initialFrames.Length)
        {
            throw new InvalidDataException(
                $"Death-pose frame table has no movement type ${(byte)sourceMovementType:X2}.");
        }

        FacingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPoseId deathPose = FacingLeft
            ? SamusPoseId.DeathSequenceLeftPose
            : SamusPoseId.DeathSequenceRightPose;
        ushort initialFrame = initialFrames[(byte)sourceMovementType];

        // Native initializes pose `$D7/$D8` normally, then overwrites only the visible frame
        // with the movement-type table result. All six delays are two, so the timer produced
        // by frame-zero initialization remains the correct literal value for every start.
        samus.Pose = deathPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Death entry publishes history itself, before overriding the starting
        // animation frame; the ordinary gameplay pose dispatcher is not running.
        samus.CommitPoseHistory();
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, initialFrame, timer: 2);

        // The cartridge subtracts layer 1 directly from Samus's live words because its death
        // renderer adds layer 1 back before emitting OAM. Retain world coordinates in Samus
        // for this host's shared camera/minimap model and capture the mathematically identical
        // screen-space pair here for the eventual explosion-spritemap path.
        ScreenPosition = samus.Position.Whole.ToScreen(layer1);

        PreFlashingTimer = WrappingCountdown.Start(SamusSpecialSequenceRomData.Death.PreFlashFrameCount);
        AnimationTimer = WrappingCountdown.Start(3);
        AnimationIndex = 0;
        AnimationCounter = 0;
        SpinJumpSoundRequested = sourceMovementType == SamusMovementType.SpinJumping;
        Phase = SamusDeathSequencePhase.PreFlashing;

        return new SamusDeathSequenceStartResult();
    }

    /// <summary>
    /// Executes one game-state `$16/$17/$18` call. Palette writes target the same CGRAM slots as native. The
    /// five death-character uploads that natively precede the explosion are not needed: its draws bind the
    /// complete death characters.
    /// </summary>
    public SamusDeathSequenceStepResult Step(
        SamusState samus,
        IPaletteColors cgram,
        SamusDeathPaletteArtworkCatalog? artwork = null)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(cgram);
        if (!IsActive)
            throw new InvalidOperationException("Samus death sequence has not begun.");

        bool paletteChanged = false;
        bool whiteoutChanged = false;
        bool drawPose = false;
        bool drawExplosion = false;

        switch (Phase)
        {
            case SamusDeathSequencePhase.PreFlashing:
                // `$90:8976` advances the `$D7/$D8` delay stream only during state `$16`.
                // State `$17` freezes that final body frame while its palettes alternate.
                SamusPresentation.AnimateDeathFrame(samus);
                drawPose = true;
                PreFlashingTimer = PreFlashingTimer.Decrement().Remaining;
                if (PreFlashingTimer.IsZero || PreFlashingTimer.IsNegative)
                    Phase = SamusDeathSequencePhase.Flashing;
                break;

            case SamusDeathSequencePhase.Flashing:
                AnimationCounter++;
                if (AnimationCounter >= SamusSpecialSequenceRomData.Death.FlashFrameCount)
                {
                    // `$9B:B498` loads palette pair zero, resets the reused counters, and immediately
                    // draws explosion spritemap zero in this very same game-state-17 call.
                    WritePalettePair(samus, cgram, paletteIndex: 0, artwork);
                    paletteChanged = true;
                    AnimationTimer =
                        WrappingCountdown.FromRaw(SamusDeathExplosionTimingDefinitions.DurationForIndex(index: 0));
                    AnimationIndex = 0;
                    AnimationCounter = 0;
                    Phase = SamusDeathSequencePhase.SuitExplosion;
                    drawExplosion = StepSuitExplosion(
                        samus,
                        cgram,
                        ref paletteChanged,
                        ref whiteoutChanged,
                        artwork);
                }
                else
                {
                    AnimationTimer = AnimationTimer.Decrement().Remaining;
                    if (AnimationTimer.IsZero || AnimationTimer.IsNegative)
                    {
                        if (AnimationIndex == 0)
                        {
                            AnimationIndex = 1;
                            AnimationTimer = WrappingCountdown.Start(1);
                        }
                        else
                        {
                            AnimationIndex = 0;
                            AnimationTimer = WrappingCountdown.Start(3);
                        }
                        WritePalettePair(samus, cgram, AnimationIndex, artwork);
                        paletteChanged = true;
                    }
                    drawPose = true;
                }
                break;

            case SamusDeathSequencePhase.SuitExplosion:
                whiteoutChanged = ApplyWhiteout(cgram, artwork);
                drawExplosion = StepSuitExplosion(
                    samus,
                    cgram,
                    ref paletteChanged,
                    ref whiteoutChanged,
                    artwork);
                break;

            case SamusDeathSequencePhase.Complete:
                // Native advances to the room fade with no Samus draw. The outer game-state
                // owner has not been translated into this gameplay host, so remain locked.
                break;

            case SamusDeathSequencePhase.Inactive:
            default:
                throw new InvalidOperationException($"Unknown death phase {Phase}.");
        }

        return new SamusDeathSequenceStepResult(
            Phase,
            drawPose,
            drawExplosion,
            Phase == SamusDeathSequencePhase.Complete);
    }

    /// <summary>Publishes `$92:EDBE`'s one right/left explosion frame at the captured screen position.</summary>
    public void DrawExplosion(SpriteFrameBuffer sprites, InstalledSpriteSheets? sheets = null)
    {
        Ensure.NotNull(sprites);
        if (ExplosionFrame is not int frame)
            return;
        SpriteSheet death = (sheets ?? throw new InvalidOperationException(
            "Samus death explosion requires the installed sprite sheets.")).Get(SpriteSheetId.SamusDeath);
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusDeathExplosion),
            death.Frame(SamusDeathSheetDefinitions.ExplosionFrame(FacingLeft ? HorizontalDirection.Left : HorizontalDirection.Right, frame)),
            ScreenPosition, SpriteMirror.None));
    }

    /// <summary>Advances the explosion timer, palette and terminal whiteout, returning whether to draw this frame.</summary>
    private bool StepSuitExplosion(
        SamusState samus,
        IPaletteColors cgram,
        ref bool paletteChanged,
        ref bool whiteoutChanged,
        SamusDeathPaletteArtworkCatalog? artwork)
    {
        AnimationTimer = AnimationTimer.Decrement().Remaining;
        if (!AnimationTimer.IsZero && !AnimationTimer.IsNegative)
            return true;

        AnimationIndex++;
        if (AnimationIndex >= SamusDeathExplosionTimingDefinitions.RecordCount)
        {
            // The terminal call writes shade 21 (`$7FFF`) across every non-Samus/non-
            // suitless palette and returns one without drawing a tenth explosion frame.
            AnimationCounter = 21;
            whiteoutChanged = ApplyWhiteout(cgram, artwork);
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }

        AnimationTimer =
            WrappingCountdown.FromRaw(SamusDeathExplosionTimingDefinitions.DurationForIndex(AnimationIndex));
        int paletteIndex = (artwork ?? throw new InvalidOperationException(
            "Samus death requires installed palette artwork.")).ExplosionPaletteIndex(AnimationIndex);
        WritePalettePair(samus, cgram, paletteIndex, artwork);
        paletteChanged = true;
        return true;
    }

    /// <summary>Advances the room whiteout shade while preserving the first explosion frame's original background.</summary>
    private bool ApplyWhiteout(IPaletteColors cgram,
        SamusDeathPaletteArtworkCatalog? artwork)
    {
        // Index zero suppresses whiteout completely. This is why the first 21-tick explosion
        // frame remains over the original room even though game state `$18` has begun.
        if (AnimationIndex == 0)
            return false;

        Bgr555 shade = (artwork ?? throw new InvalidOperationException(
            "Samus death whiteout requires installed palette artwork.")).WhiteoutColor(AnimationCounter);
        var palettes = new SamusPalettes(cgram);
        palettes.DeathWhiteoutBelowSuit.Fill(shade);
        palettes.DeathWhiteoutBetweenSuits.Fill(shade);

        if (AnimationCounter < 20)
            AnimationCounter++;
        return true;
    }

    /// <summary>Loads suited and suitless death colors selected by equipment and animation frame.</summary>
    private static void WritePalettePair(
        SamusState samus,
        IPaletteColors cgram,
        int paletteIndex,
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
                new SamusPalettes(cgram).Suit.Write(color,
                    artwork.SuitedColor(suitIndex / 2, paletteIndex, color));
                new SamusPalettes(cgram).Suitless.Write(color,
                    artwork.SuitlessColor(paletteIndex, color));
            }
            return;
        }
        throw new InvalidOperationException("Samus death requires installed palette artwork.");
    }
}
