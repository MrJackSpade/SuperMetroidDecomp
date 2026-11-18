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

    /// <summary>Sixteen updates precede the alternating yellow flash.</summary>
    public int PreFlashingTimer { get; private set; }

    /// <summary>
    /// The flash's shades (#2387): after three updates it shows her second death shade for one update and her first for
    /// three, over and over, until the suit explodes. The death pose starts it again.
    /// </summary>
    internal AnimationPlayer<SamusDeathFlashSegment, SamusDeathShade, NoAnimationCue> Flash { get; private set; } =
        SamusDeathAnimations.FlashStarting();

    /// <summary>Flashing updates made, up to <see cref="SamusDeathSequenceDefinitions.FlashFrameCount"/>, when the suit explodes.</summary>
    public int FlashUpdates { get; private set; }

    /// <summary>
    /// The suit's explosion (#2388), its nine pictures played once for their dwell times, or null before the suit explodes;
    /// the sequence completes once its last picture has shown.
    /// </summary>
    internal AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue>? Explosion { get; private set; }

    /// <summary>
    /// The whiteout's shades (#2387): one update each from the update after the explosion's second picture first shows,
    /// brightening to <see cref="SamusWhiteoutShade.Brightest"/> and resting there; null until the explosion reaches its
    /// second picture, so its first picture stays over the room as it was.
    /// </summary>
    internal AnimationPlayer<SamusWhiteoutSegment, SamusWhiteoutShade, NoAnimationCue>? WhiteoutBrightening { get; private set; }

    /// <summary>
    /// The whiteout shade the explosion shows, or null before its first: the explosion's last update shows
    /// <see cref="SamusWhiteoutShade.Final"/>.
    /// </summary>
    public SamusWhiteoutShade? WhiteoutShade { get; private set; }

    /// <summary>
    /// The colour the explosion's whiteout shows every picture and sprite in but Samus's body, or null before the
    /// whiteout's first shade. Each whiteout step selects the installed shade of its index; the last saturates.
    /// </summary>
    public RgbColor? Whiteout { get; private set; }

    /// <summary>The signed screen place captured at death-pose setup: left of or above the screen is negative.</summary>
    public Offset16<ScreenPixels> ScreenPosition { get; private set; }


    /// <summary>The suit-explosion picture shown, or null outside the explosion.</summary>
    public SamusDeathExplosionFrame? ExplosionFrame =>
        Phase == SamusDeathSequencePhase.SuitExplosion && Explosion is { HasStopped: false } explosion ? explosion.Shown : null;

    /// <summary>The facing captured before pose metadata is replaced by the death-sequence pose.</summary>
    public bool FacingLeft { get; private set; }

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
        SamusAnimationTarget initialFrame = SamusDeathSequenceFrames.InitialFrame(sourceMovementType);

        // The game initializes DeathSequenceRightPose and DeathSequenceLeftPose normally, then overwrites only the visible frame
        // with the movement-type table result. All six delays are two, so the timer produced
        // by frame-zero initialization remains the correct literal value for every start.
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

        PreFlashingTimer = SamusDeathSequenceDefinitions.PreFlashFrameCount;
        Flash = SamusDeathAnimations.FlashStarting();
        FlashUpdates = 0;
        Explosion = null;
        WhiteoutBrightening = null;
        WhiteoutShade = null;
        Whiteout = null;
        // Dying while spin jumping ends the spin's sound at normal priority.
        if (sourceMovementType == SamusMovementType.SpinJumping)
            samus.AudioRequests.QueueSound(SamusSound.SpinJumpEnd, SoundRequestPriority.Normal);
        Phase = SamusDeathSequencePhase.PreFlashing;
    }

    /// <summary>
    /// Executes one update of the pre-flash, flashing or suit-explosion game state. Samus's suit and suitless steps are her shades, and the whiteout is
    /// the <see cref="Whiteout"/> colour the display shows (#1512), on the same updates the game shows them. The explosion
    /// is shown by selecting its frame (<see cref="ExplosionFrame"/>) on the update the game draws it; the five
    /// artwork loads that precede it in the game have no counterpart, because each frame is a pre-coloured picture.
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

        bool coloursChanged = false;
        bool whiteoutChanged = false;
        bool drawPose = false;
        bool drawExplosion = false;

        switch (Phase)
        {
            case SamusDeathSequencePhase.PreFlashing:
                // The death animation advances the death pose's frame delays only during the pre-flash state.
                // The flashing state freezes that final body frame while its shades alternate.
                SamusPresentation.AnimateDeathFrame(samus);
                drawPose = true;
                PreFlashingTimer--;
                if (PreFlashingTimer <= 0)
                    Phase = SamusDeathSequencePhase.Flashing;
                break;

            case SamusDeathSequencePhase.Flashing:
                FlashUpdates++;
                if (FlashUpdates >= SamusDeathSequenceDefinitions.FlashFrameCount)
                {
                    // The flash's last update shows the first death shade, starts the explosion on its first picture, and
                    // immediately plays and draws it in this same update.
                    ShowDeathShades(samus, samusColours, SamusDeathShade.First);
                    coloursChanged = true;
                    Explosion = SamusDeathAnimations.ExplosionStarting();
                    WhiteoutBrightening = null;
                    Phase = SamusDeathSequencePhase.SuitExplosion;
                    drawExplosion = StepSuitExplosion(
                        samus,
                        samusColours,
                        ref coloursChanged,
                        ref whiteoutChanged,
                        artwork);
                }
                else
                {
                    if (Flash.StepToNewFrame() is { } flashShade)
                    {
                        ShowDeathShades(samus, samusColours, flashShade);
                        coloursChanged = true;
                    }
                    drawPose = true;
                }
                break;

            case SamusDeathSequencePhase.SuitExplosion:
                whiteoutChanged = ApplyWhiteout(samusColours, artwork);
                drawExplosion = StepSuitExplosion(
                    samus,
                    samusColours,
                    ref coloursChanged,
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

    /// <summary>Publishes the one right/left explosion frame at the captured screen position, from <paramref name="sheets"/>.</summary>
    public void DrawExplosion(SpriteFrameBuffer sprites, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(sheets);
        if (ExplosionFrame is not { } frame)
            return;
        SpriteSheet death = sheets.Get(SpriteSheetId.SamusDeath);
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusDeathExplosion),
            death.Frame(SamusDeathSheetDefinitions.ExplosionFrame(FacingLeft ? HorizontalDirection.Left : HorizontalDirection.Right, frame)),
            ScreenPosition, SpriteMirror.None));
    }

    /// <summary>Plays the explosion on, its death shades and terminal whiteout, returning whether to draw this frame.</summary>
    private bool StepSuitExplosion(
        SamusState samus,
        SamusColours samusColours,
        ref bool coloursChanged,
        ref bool whiteoutChanged,
        SamusDeathSteps artwork)
    {
        AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue> explosion =
            Explosion ?? throw new InvalidOperationException("The suit's explosion has not started.");
        if (explosion.StepToNewFrame() is { } picture)
        {
            // The whiteout starts on the update after the explosion's second picture first shows.
            WhiteoutBrightening ??= SamusDeathAnimations.WhiteoutStarting();
            ShowDeathShades(samus, samusColours, artwork.ExplosionShade(picture));
            coloursChanged = true;
            return true;
        }
        if (explosion.HasStopped)
        {
            // The last update shows the full-white whiteout over everything but Samus's body and draws no
            // explosion frame after the last.
            ShowWhiteout(samusColours, artwork, SamusWhiteoutShade.Final);
            whiteoutChanged = true;
            Phase = SamusDeathSequencePhase.Complete;
            return false;
        }
        return true;
    }

    /// <summary>Advances the room whiteout shade while preserving the first explosion frame's original background.</summary>
    private bool ApplyWhiteout(SamusColours samusColours, SamusDeathSteps artwork)
    {
        // The explosion's first picture shows no whiteout, so it stays over the room as it was even though the suit
        // explosion has begun.
        if (WhiteoutBrightening is not { } whiteout)
            return false;

        ShowWhiteout(samusColours, artwork, whiteout.StepToNewFrame()
            ?? throw new InvalidOperationException("The whiteout shows a shade on every update of the explosion."));
        return true;
    }

    /// <summary>
    /// Shows <paramref name="shade"/> of the whiteout: everything but Samus's body whitens, the room, the HUD, every other
    /// sprite, and her beams and grapple flare, which show the whiteout's shade (#1512). Her body's suit and suitless
    /// colours are her own shades and never whiten; the sprites that shared their sets with her no longer keep her colours
    /// through the whiteout.
    /// </summary>
    private void ShowWhiteout(SamusColours samusColours, SamusDeathSteps artwork, SamusWhiteoutShade shade)
    {
        WhiteoutShade = shade;
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
