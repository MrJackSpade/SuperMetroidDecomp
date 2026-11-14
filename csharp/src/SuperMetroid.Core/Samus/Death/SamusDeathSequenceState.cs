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
/// the death-sequence poses advertise the knockback movement type: the death game states stop Samus's normal input
/// and movement phases and run this independent state machine instead.
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

    /// <summary>Calls left before the flash changes its death shade.</summary>
    public int FlashShadeTimer { get; private set; }

    /// <summary>The death shade the flash shows, zero or one.</summary>
    public int FlashShade { get; private set; }

    /// <summary>Flashing calls made, up to <see cref="SamusDeathSequenceDefinitions.FlashFrameCount"/>, when the suit explodes.</summary>
    public int FlashCalls { get; private set; }

    /// <summary>Calls left on the shown suit-explosion step.</summary>
    public int ExplosionTimer { get; private set; }

    /// <summary>
    /// The suit-explosion step, from zero up to <see cref="SamusDeathExplosionTimingDefinitions.StepCount"/>, where the
    /// sequence completes.
    /// </summary>
    public int ExplosionStep { get; private set; }

    /// <summary>
    /// The whiteout shade the explosion shows next, which stops at
    /// <see cref="SamusDeathSequenceDefinitions.BrightestExplosionWhiteoutShade"/>; the last call shows
    /// <see cref="SamusDeathSequenceDefinitions.FinalWhiteoutShade"/>.
    /// </summary>
    public int WhiteoutShade { get; private set; }

    /// <summary>
    /// The colour the explosion's whiteout shows every picture and sprite in but Samus's body, or null before the
    /// whiteout's first shade. Each whiteout step selects the installed shade of its index; the last saturates.
    /// </summary>
    public RgbColor? Whiteout { get; private set; }

    /// <summary>Screen-space position captured at death-pose setup.</summary>
    public Position16<ScreenPixels> ScreenPosition { get; private set; }


    /// <summary>The suit-explosion frame shown, or null outside the explosion.</summary>
    public int? ExplosionFrame =>
        Phase == SamusDeathSequencePhase.SuitExplosion && ExplosionStep < SamusDeathExplosionTimingDefinitions.StepCount
            ? ExplosionStep : null;

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
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, initialFrame, timer: 2);

        // The game subtracts the foreground directly from Samus's live position because its death
        // renderer adds the foreground back before drawing. Retain world coordinates in Samus
        // for the shared camera/minimap model and capture the mathematically identical
        // screen-space pair here for the eventual explosion-frame path.
        ScreenPosition = samus.Position.Whole.ToScreen(cameraPosition);

        PreFlashingTimer = SamusDeathSequenceDefinitions.PreFlashFrameCount;
        FlashShadeTimer = SamusDeathSequenceDefinitions.FirstFlashShadeCalls;
        FlashShade = 0;
        FlashCalls = 0;
        ExplosionTimer = 0;
        ExplosionStep = 0;
        WhiteoutShade = 0;
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
                FlashCalls++;
                if (FlashCalls >= SamusDeathSequenceDefinitions.FlashFrameCount)
                {
                    // The flash's last call shows death shade zero, starts the explosion at its first step and whiteout
                    // shade, and immediately draws explosion frame zero in this same call.
                    ShowDeathShades(samus, samusColours, deathShade: 0);
                    paletteChanged = true;
                    ExplosionTimer =
                        SamusDeathExplosionTiming.DurationForIndex(index: 0);
                    ExplosionStep = 0;
                    WhiteoutShade = 0;
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
                    FlashShadeTimer--;
                    if (FlashShadeTimer <= 0)
                    {
                        if (FlashShade == 0)
                        {
                            FlashShade = 1;
                            FlashShadeTimer = SamusDeathSequenceDefinitions.SecondFlashShadeCalls;
                        }
                        else
                        {
                            FlashShade = 0;
                            FlashShadeTimer = SamusDeathSequenceDefinitions.FirstFlashShadeCalls;
                        }
                        ShowDeathShades(samus, samusColours, FlashShade);
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
        ExplosionTimer--;
        if (ExplosionTimer > 0)
            return true;

        ExplosionStep++;
        if (ExplosionStep >= SamusDeathExplosionTimingDefinitions.StepCount)
        {
            // The last call shows the full-white whiteout over everything but Samus's body and draws no
            // explosion frame after the last.
            WhiteoutShade = SamusDeathSequenceDefinitions.FinalWhiteoutShade;
            whiteoutChanged = ApplyWhiteout(samusColours, artwork);
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }

        ExplosionTimer =
            SamusDeathExplosionTiming.DurationForIndex(ExplosionStep);
        int deathShade = (artwork ?? throw new InvalidOperationException(
            "Samus death requires installed palette artwork.")).ExplosionStep(ExplosionStep);
        ShowDeathShades(samus, samusColours, deathShade);
        paletteChanged = true;
        return true;
    }

    /// <summary>Advances the room whiteout shade while preserving the first explosion frame's original background.</summary>
    private bool ApplyWhiteout(SamusColours samusColours, SamusDeathSteps? artwork)
    {
        // The first explosion step shows no whiteout, so the first explosion frame stays over the room as it was
        // even though the suit explosion has begun.
        if (ExplosionStep == 0)
            return false;

        // Everything but Samus's body whitens: the room, the HUD, every other sprite, and her beams and grapple flare,
        // which show the whiteout's shade (#1512). Her body's suit and suitless colours are her own shades and never
        // whiten; the sprites that shared their sets with her no longer keep her colours through the whiteout.
        Whiteout = (artwork ?? throw new InvalidOperationException(
            "Samus death whiteout requires installed palette artwork.")).WhiteoutColour(WhiteoutShade);
        samusColours.ShowBeams(SamusShades.DeathWhiteout(WhiteoutShade));

        if (WhiteoutShade < SamusDeathSequenceDefinitions.BrightestExplosionWhiteoutShade)
            WhiteoutShade++;
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
