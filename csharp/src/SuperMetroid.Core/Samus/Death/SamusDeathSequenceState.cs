using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// Owns Samus's fatal-damage pose, picture, shade, flashing, whiteout, and suit-
/// explosion sequence. It is apart from knockback, though the death-sequence poses have the knockback movement type:
/// the death stops Samus's normal input and movement and runs this sequence instead.
/// </summary>
public sealed class SamusDeathSequenceState : IGameplayShadingSource
{
    /// <summary>The game-state-owned phase corresponding to the pre-flash, flashing and suit-explosion states.</summary>
    public SamusDeathSequencePhase Phase { get; private set; }

    /// <summary>
    /// True from the death sequence's setup through the terminal white frame. A completed sequence
    /// remains an owner until the outer game-state fade replaces the room, so normal knockback movement never resumes
    /// on <see cref="SamusPoseId.DeathSequenceRightPose"/> and <see cref="SamusPoseId.DeathSequenceLeftPose"/>.
    /// </summary>
    public bool IsActive => Phase != SamusDeathSequencePhase.Inactive;

    /// <summary>
    /// The wait before the alternating yellow flash, the last update included: it lasts
    /// <see cref="SamusDeathSequenceDefinitions.PreFlashFrameCount"/> from when the death sequence begins.
    /// </summary>
    public UpdateCountdown PreFlashingTimer { get; private set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The flash's shades (#2387): after an opening wait it shows her second death shade and then her first, each for its
    /// own hold, over and over, until the suit explodes. The death pose starts it again.
    /// </summary>
    private AnimationPlayer<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue> Flash { get; set; } =
        SamusDeathAnimations.FlashStarting();

    /// <summary>The flashing, lasting <see cref="SamusDeathSequenceDefinitions.FlashFrameCount"/>: the suit explodes on its last update.</summary>
    private UpdateCountdown Flashing { get; set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The suit's explosion (#2388), its pictures played once for their dwell times, or null before the suit explodes;
    /// the sequence completes once its last picture has shown.
    /// </summary>
    private AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue>? Explosion { get; set; }

    /// <summary>
    /// The whiteout's shades (#2387): one update each from the update after the explosion's second picture first shows,
    /// brightening to <see cref="SamusWhiteoutShade.Brightest"/> and resting there; null until the explosion reaches its
    /// second picture, so its first picture stays over the room as it was.
    /// </summary>
    internal AnimationPlayer<SamusWhiteoutSegment, SamusWhiteoutShade, NoAnimationCue>? WhiteoutBrightening { get; private set; }

    /// <summary>
    /// The colour the explosion's whiteout shows every picture and sprite in but Samus's body, or null before the
    /// whiteout's first shade. Each whiteout step shows the next installed shade, and the last one stays.
    /// </summary>
    public RgbColor? Whiteout { get; private set; }

    /// <summary>The signed screen place captured at death-pose setup: left of or above the screen is negative.</summary>
    private Offset16<ScreenPixels> ScreenPosition { get; set; }

/// <summary>The suit-explosion picture shown, or null outside the explosion.</summary>
    private SamusDeathExplosionFrame? ExplosionFrame =>
        Phase == SamusDeathSequencePhase.SuitExplosion && Explosion is { HasStopped: false } explosion ? explosion.Shown : null;

    /// <summary>The facing captured before pose metadata is replaced by the death-sequence pose.</summary>
    private bool FacingLeft { get; set; }

    /// <summary>
    /// Sets Samus's death-sequence pose. It runs once
    /// the death's first game state has observed an empty music queue; fatal-damage acquisition and
    /// the music wait are distinct producers and are not guessed here.
    /// </summary>
    public void Begin(
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
        SamusAnimationFrame initialFrame = SamusDeathSequenceFrames.InitialFrame(sourceMovementType);

        // The death pose is set up as any pose is, then shows the frame the movement type it starts from begins it
        // on. Every frame of the death pose lasts as long as the others, so the timer its setup starts holds for whichever frame
        // it shows.
        samus.Pose = deathPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // Death entry publishes history itself, before overriding the starting
        // animation frame; the ordinary gameplay pose step is not running.
        samus.CommitPoseHistory();
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, initialFrame, timer: SamusDeathSequenceDefinitions.InitialFrameHold);

        // Samus keeps her room position for the camera and the map; the death sequence draws her at her place
        // on the screen, taken here for the explosion.
        ScreenPosition = samus.Position.Whole.RelativeToCamera(cameraPosition);

        PreFlashingTimer = UpdateCountdown.Lasting(SamusDeathSequenceDefinitions.PreFlashFrameCount);
        Flash = SamusDeathAnimations.FlashStarting();
        Flashing = UpdateCountdown.Lasting(SamusDeathSequenceDefinitions.FlashFrameCount);
        Explosion = null;
        WhiteoutBrightening = null;
        Whiteout = null;
        // Dying while spin jumping ends the spin's sound at normal priority.
        if (sourceMovementType == SamusMovementType.SpinJumping)
            samus.AudioRequests.QueueSound(SamusSound.SpinJumpEnd, SoundRequestPriority.Normal);
        Phase = SamusDeathSequencePhase.PreFlashing;
    }

    /// <summary>
    /// Executes one update of the pre-flash, flashing or suit-explosion game state. Samus's suit and suitless steps are her shades, and the whiteout is
    /// the <see cref="Whiteout"/> colour the display shows (#1512), on the same updates the game shows them. The explosion
    /// is shown by selecting its frame (<see cref="ExplosionFrame"/>), a whole coloured picture, on the update the game
    /// shows it.
    /// </summary>
    public SamusDeathSequenceStepResult Step(
        SamusState samus,
        SamusColours samusColours,
        SamusDeathSteps artwork)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(samusColours);
        Ensure.NotNull(artwork);
        if (!IsActive)
            throw new InvalidOperationException("Samus death sequence has not begun.");

        SamusDeathDraw draws = SamusDeathDraw.Nothing;

        switch (Phase)
        {
            case SamusDeathSequencePhase.PreFlashing:
                // The death animation advances the death pose's frame delays only during the pre-flash state.
                // The flashing state freezes that final body frame while its shades alternate.
                SamusPresentation.AnimateDeathFrame(samus);
                draws = SamusDeathDraw.Pose;
                if (PreFlashingTimer.StepEnds())
                    Phase = SamusDeathSequencePhase.Flashing;
                break;

            case SamusDeathSequencePhase.Flashing:
                if (Flashing.Step())
                {
                    // The flash's last update shows the first death shade, starts the explosion on its first picture, and
                    // immediately plays and draws it in this same update.
                    ShowDeathShades(samus, samusColours, SamusDeathShade.First);
                    Explosion = SamusDeathAnimations.ExplosionStarting();
                    WhiteoutBrightening = null;
                    Phase = SamusDeathSequencePhase.SuitExplosion;
                    draws = StepSuitExplosion(samus, samusColours, artwork) ? SamusDeathDraw.Explosion : SamusDeathDraw.Nothing;
                }
                else
                {
                    if (Flash.StepToNewFrame() is { } flashShade)
                    {
                        ShowDeathShades(samus, samusColours, flashShade);
                    }
                    draws = SamusDeathDraw.Pose;
                }
                break;

            case SamusDeathSequencePhase.SuitExplosion:
                ApplyWhiteout(samusColours, artwork);
                draws = StepSuitExplosion(samus, samusColours, artwork) ? SamusDeathDraw.Explosion : SamusDeathDraw.Nothing;
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
            draws,
            Phase == SamusDeathSequencePhase.Complete);
    }

    /// <summary>
    /// Draws what a death update shows of Samus (<paramref name="draws"/>): her death pose with no invincibility flicker,
    /// the suit's explosion on its current picture, or nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="draws"/> names no draw.</exception>
    public void Draw(SamusDeathDraw draws, SamusState samus, SpriteFrameBuffer sprites, Position16<RoomPixels> cameraPosition,
        TurningFloorTransform? turningFloor)
    {
        Ensure.NotNull(samus);
        switch (draws)
        {
            case SamusDeathDraw.Pose:
                samus.Presentation.DrawWithoutFlicker(samus, sprites, cameraPosition, turningFloor);
                break;
            case SamusDeathDraw.Explosion:
                DrawExplosion(sprites, samus.Artwork.Sheets);
                break;
            case SamusDeathDraw.Nothing:
                break;
            default:
                throw new InvalidOperationException($"Undefined death draw {draws}.");
        }
    }

    /// <summary>Publishes the one right/left explosion frame at the captured screen position, from <paramref name="sheets"/>.</summary>
    private void DrawExplosion(SpriteFrameBuffer sprites, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(sheets);
        if (ExplosionFrame is not { } frame)
            return;
        SpriteSheet death = sheets.Get(SpriteSheetId.SamusDeath);
        sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusDeathExplosion),
            death.Frame(SamusDeathSheetDefinitions.ExplosionFrame(FacingLeft ? HorizontalDirection.Left : HorizontalDirection.Right, frame)),
            ScreenPosition));
    }

    /// <summary>Plays the explosion on, its death shades and terminal whiteout, returning whether to draw this frame.</summary>
    private bool StepSuitExplosion(SamusState samus, SamusColours samusColours, SamusDeathSteps artwork)
    {
        AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue> explosion =
            Explosion ?? throw new InvalidOperationException("The suit's explosion has not started.");
        if (explosion.StepToNewFrame() is { } picture)
        {
            // The whiteout starts on the update after the explosion's second picture first shows.
            WhiteoutBrightening ??= SamusDeathAnimations.WhiteoutStarting();
            ShowDeathShades(samus, samusColours, artwork.ExplosionShade(picture));
            return true;
        }
        if (explosion.HasStopped)
        {
            // The last update shows the full-white whiteout over everything but Samus's body and draws no
            // explosion frame after the last.
            ShowWhiteout(samusColours, artwork, SamusWhiteoutShade.Final);
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }
        return true;
    }

    /// <summary>Advances the room whiteout shade while preserving the first explosion frame's original background.</summary>
    private void ApplyWhiteout(SamusColours samusColours, SamusDeathSteps artwork)
    {
        // The explosion's first picture shows no whiteout, so it stays over the room as it was even though the suit
        // explosion has begun.
        if (WhiteoutBrightening is not { } whiteout)
            return;

        ShowWhiteout(samusColours, artwork, whiteout.StepToNewFrame()
            ?? throw new InvalidOperationException("The whiteout shows a shade on every update of the explosion."));
    }

    /// <summary>
    /// Shows <paramref name="shade"/> of the whiteout: everything but Samus's body whitens, the room, the HUD, every other
    /// sprite, and her beams and grapple flare, which show the whiteout's shade (#1512). Her body's suit and suitless
    /// colours are her own shades and never whiten; the sprites that shared their sets with her no longer keep her colours
    /// through the whiteout.
    /// </summary>
    private void ShowWhiteout(SamusColours samusColours, SamusDeathSteps artwork, SamusWhiteoutShade shade)
    {
        Whiteout = artwork.WhiteoutColour(shade);
        samusColours.ShowBeams(SamusShades.DeathWhiteout(shade));
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
    private static void ShowDeathShades(SamusState samus, SamusColours samusColours, SamusDeathShade deathShade)
    {
        samusColours.ShowSuit(SamusShades.DeathSuit(samus.EquippedItems.ShownSuit(), deathShade));
        samusColours.ShowSuitless(SamusShades.DeathSuitless(deathShade));
    }
}
