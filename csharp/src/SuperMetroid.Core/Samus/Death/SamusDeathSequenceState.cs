using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// Owns Samus's fatal-damage pose, picture, palette, flashing, whiteout, and suit-
/// explosion sequence. This is deliberately not folded into knockback merely because poses
/// the death-sequence poses advertise the knockback movement type: the death game states stop the normal Samus frame
/// handlers and call this independent state machine instead.
/// </summary>
public sealed class SamusDeathSequenceState : IGameplayShadingSource
{
    /// <summary>The game-state-owned phase corresponding to the pre-flash, flashing and suit-explosion states.</summary>
    public SamusDeathSequencePhase Phase { get; private set; }

    /// <summary>
    /// True from the death sequence's setup through the terminal white frame. A completed sequence
    /// remains an owner until the outer game-state fade replaces the room, preventing normal
    /// knockback movement code from accidentally resuming on <see cref="SamusPoseId.DeathSequenceRightPose"/> and <see cref="SamusPoseId.DeathSequenceLeftPose"/>.
    /// </summary>
    public bool IsActive => Phase != SamusDeathSequencePhase.Inactive;

    /// <summary>Sixteen calls precede the alternating yellow flash.</summary>
    public int PreFlashingTimer { get; private set; }

    /// <summary>Reused by flash and suit-explosion palette timing.</summary>
    public int AnimationTimer { get; private set; }

    /// <summary>
    /// Flash palette selector (zero or one) while flashing, then the suit-explosion frame index from zero up to
    /// <see cref="SamusDeathExplosionTimingDefinitions.StepCount"/>, where the sequence completes.
    /// </summary>
    public int AnimationIndex { get; private set; }

    /// <summary>Flashing-call count up to the 60-call flash, then the whiteout shade index, which saturates at 20 and ends at 21.</summary>
    public int AnimationCounter { get; private set; }

    /// <summary>
    /// The colour the explosion's whiteout shows every picture and sprite in but Samus's body, or null before the
    /// whiteout's first shade. Each whiteout step selects the installed shade of its index; the last saturates.
    /// </summary>
    public RgbColor? Whiteout { get; private set; }

    /// <summary>Screen-space position captured at death-pose setup.</summary>
    public Position16<ScreenPixels> ScreenPosition { get; private set; }


    /// <summary>The suit-explosion frame shown, or null outside the explosion.</summary>
    public int? ExplosionFrame =>
        Phase == SamusDeathSequencePhase.SuitExplosion && AnimationIndex < SamusDeathExplosionTimingDefinitions.StepCount
            ? AnimationIndex : null;

    /// <summary>The facing bit captured before pose metadata is replaced by the death-sequence pose.</summary>
    public bool FacingLeft { get; private set; }

    /// <summary>
    /// Sets Samus's death-sequence pose. The outer caller represents
    /// the death's first game state having observed an empty music queue; fatal-damage acquisition and
    /// the music wait are distinct producers and are not guessed here.
    /// </summary>
    public SamusDeathSequenceStartResult Begin(
        SamusState samus,
        Position16<RoomPixels> cameraPosition)
    {
        Ensure.NotNull(samus);
        if (IsActive)
            throw new InvalidOperationException("Samus death sequence is already running.");

        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);

        FacingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPoseId deathPose = FacingLeft
            ? SamusPoseId.DeathSequenceLeftPose
            : SamusPoseId.DeathSequenceRightPose;
        int initialFrame = SamusDeathSequenceFrames.InitialFrame(sourceMovementType);

        // The game initializes DeathSequenceRightPose and DeathSequenceLeftPose normally, then overwrites only the visible frame
        // with the movement-type table result. All six delays are two, so the timer produced
        // by frame-zero initialization remains the correct literal value for every start.
        samus.Pose = deathPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Death entry publishes history itself, before overriding the starting
        // animation frame; the ordinary gameplay pose step is not running.
        samus.CommitPoseHistory();
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, initialFrame, timer: 2);

        // The game subtracts the foreground directly from Samus's live position because its death
        // renderer adds the foreground back before drawing. Retain world coordinates in Samus
        // for the shared camera/minimap model and capture the mathematically identical
        // screen-space pair here for the eventual explosion-frame path.
        ScreenPosition = samus.Position.Whole.ToScreen(cameraPosition);

        PreFlashingTimer = SamusDeathSequenceDefinitions.PreFlashFrameCount;
        AnimationTimer = 3;
        AnimationIndex = 0;
        AnimationCounter = 0;
        Whiteout = null;
        // Dying while spin jumping ends the spin's sound at normal priority.
        if (sourceMovementType == SamusMovementType.SpinJumping)
            samus.AudioRequests.QueueSound(SamusSound.SpinJumpEnd, SoundRequestPriority.Normal);
        Phase = SamusDeathSequencePhase.PreFlashing;

        return new SamusDeathSequenceStartResult();
    }

    /// <summary>
    /// Executes one call of the pre-flash, flashing or suit-explosion game state. Samus's suit and suitless steps are her shades, and the whiteout is
    /// the <see cref="Whiteout"/> colour the display shows (#1512), on the same calls the game wrote them. The explosion
    /// is shown by selecting its frame (<see cref="ExplosionFrame"/>) on the call the game drew it; the five
    /// artwork loads that precede it in the game have no counterpart, because each frame is a pre-coloured picture.
    /// </summary>
    public SamusDeathSequenceStepResult Step(
        SamusState samus,
        SamusColours samusColours,
        SamusDeathSteps? artwork = null)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(samusColours);
        if (!IsActive)
            throw new InvalidOperationException("Samus death sequence has not begun.");

        bool paletteChanged = false;
        bool whiteoutChanged = false;
        bool drawPose = false;
        bool drawExplosion = false;

        switch (Phase)
        {
            case SamusDeathSequencePhase.PreFlashing:
                // The death animation advances the death pose's delay stream only during the pre-flash state.
                // The flashing state freezes that final body frame while its palettes alternate.
                SamusPresentation.AnimateDeathFrame(samus);
                drawPose = true;
                PreFlashingTimer--;
                if (PreFlashingTimer <= 0)
                    Phase = SamusDeathSequencePhase.Flashing;
                break;

            case SamusDeathSequencePhase.Flashing:
                AnimationCounter++;
                if (AnimationCounter >= SamusDeathSequenceDefinitions.FlashFrameCount)
                {
                    // The explosion step shows death shade zero, resets the reused counters, and immediately
                    // draws explosion frame zero in this very same death-flashing call.
                    ShowDeathShades(samus, samusColours, deathShade: 0);
                    paletteChanged = true;
                    AnimationTimer =
                        SamusDeathExplosionTiming.DurationForIndex(index: 0);
                    AnimationIndex = 0;
                    AnimationCounter = 0;
                    Phase = SamusDeathSequencePhase.SuitExplosion;
                    drawExplosion = StepSuitExplosion(
                        samus,
                        samusColours,
                        ref paletteChanged,
                        ref whiteoutChanged,
                        artwork);
                }
                else
                {
                    AnimationTimer--;
                    if (AnimationTimer <= 0)
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
                        ShowDeathShades(samus, samusColours, AnimationIndex);
                        paletteChanged = true;
                    }
                    drawPose = true;
                }
                break;

            case SamusDeathSequencePhase.SuitExplosion:
                whiteoutChanged = ApplyWhiteout(samusColours, artwork);
                drawExplosion = StepSuitExplosion(
                    samus,
                    samusColours,
                    ref paletteChanged,
                    ref whiteoutChanged,
                    artwork);
                break;

            case SamusDeathSequencePhase.Complete:
                // The game advances to the room fade with no Samus draw. The outer game-state
                // owner is outside this sequence, so remain locked.
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

    /// <summary>Publishes the one right/left explosion frame at the captured screen position.</summary>
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

    /// <summary>Advances the explosion timer, death shade and terminal whiteout, returning whether to draw this frame.</summary>
    private bool StepSuitExplosion(
        SamusState samus,
        SamusColours samusColours,
        ref bool paletteChanged,
        ref bool whiteoutChanged,
        SamusDeathSteps? artwork)
    {
        AnimationTimer--;
        if (AnimationTimer > 0)
            return true;

        AnimationIndex++;
        if (AnimationIndex >= SamusDeathExplosionTimingDefinitions.StepCount)
        {
            // The terminal call writes shade 21 across every non-Samus/non-
            // suitless palette and returns one without drawing a tenth explosion frame.
            AnimationCounter = 21;
            whiteoutChanged = ApplyWhiteout(samusColours, artwork);
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }

        AnimationTimer =
            SamusDeathExplosionTiming.DurationForIndex(AnimationIndex);
        int deathShade = (artwork ?? throw new InvalidOperationException(
            "Samus death requires installed palette artwork.")).ExplosionStep(AnimationIndex);
        ShowDeathShades(samus, samusColours, deathShade);
        paletteChanged = true;
        return true;
    }

    /// <summary>Advances the room whiteout shade while preserving the first explosion frame's original background.</summary>
    private bool ApplyWhiteout(SamusColours samusColours, SamusDeathSteps? artwork)
    {
        // Index zero suppresses whiteout completely. This is why the first 21-tick explosion
        // frame remains over the room as it was even though the suit-explosion game state has begun.
        if (AnimationIndex == 0)
            return false;

        // Everything but Samus's body whitens: the room, the HUD, every other sprite, and her beams and grapple flare,
        // which show the whiteout's shade (#1512). Her body's suit and suitless colours are her own shades and never
        // whiten; the sprites that shared their sets with her no longer keep her colours through the whiteout.
        Whiteout = (artwork ?? throw new InvalidOperationException(
            "Samus death whiteout requires installed palette artwork.")).WhiteoutColour(AnimationCounter);
        samusColours.ShowBeams(SamusShades.DeathWhiteout(AnimationCounter));

        if (AnimationCounter < 20)
            AnimationCounter++;
        return true;
    }

    /// <summary>
    /// Shows the whiteout over the whole display but Samus once its first shade is written (#1512): every picture, the
    /// HUD's among them, and every sprite but hers shows the whiteout's colour.
    /// </summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (Whiteout is { } colour)
            frame.Fade(new GameplayDisplayFade(new ColourFlash(colour), GameplayFadeSpares.Samus));
    }

    /// <summary>Shows the suited and suitless death shades the shown suit and the animation step select (#1512).</summary>
    private static void ShowDeathShades(SamusState samus, SamusColours samusColours, int deathShade)
    {
        samusColours.ShowSuit(SamusShades.DeathSuit(samus.EquippedItems.ShownSuit(), deathShade));
        samusColours.ShowSuitless(SamusShades.DeathSuitless(deathShade));
    }
}
