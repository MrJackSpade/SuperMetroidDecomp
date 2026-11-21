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
/// explosion update.Sequence. It is apart from knockback, though the death-sequence poses have the knockback movement type:
/// the death stops Samus's normal input and movement and runs this sequence instead.
/// </summary>
public sealed class SamusDeathSequenceState : IGameplayShadingSource
{
    /// <summary>The game-state-owned phase corresponding to the pre-flash, flashing and suit-explosion states.</summary>
    public SamusDeathSequencePhase Phase => stage.Phase;

    /// <summary>
    /// The sequence's stage, holding what it plays: not dying until the death pose begins it, the explosion its own
    /// pictures from the update the suit explodes.
    /// </summary>
    private DeathStage stage = NotDyingStage.Instance;

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
        stage is ExplosionStage { Explosion.HasStopped: false } exploding ? exploding.Explosion.Shown : null;

    /// <summary>The facing captured before pose metadata is replaced by the death-sequence pose: left, or right otherwise.</summary>
    private HorizontalDirection Facing { get; set; }

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

        Facing = SamusPoseMetadata.ReadFacingDirection(samus) is SamusFacingDirection.Left ? HorizontalDirection.Left : HorizontalDirection.Right;
        SamusPoseId deathPose = Facing switch
        {
            HorizontalDirection.Left => SamusPoseId.DeathSequenceLeftPose,
            HorizontalDirection.Right => SamusPoseId.DeathSequenceRightPose,
            _ => throw new InvalidOperationException($"Undefined {nameof(HorizontalDirection)} {Facing}."),
        };
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
        WhiteoutBrightening = null;
        Whiteout = null;
        // Dying while spin jumping ends the spin's sound at normal priority.
        if (SamusMovementTypeDefinitions.Movements[sourceMovementType].SpinSoundEndsOnDeath)
            samus.AudioRequests.QueueSound(SamusSound.SpinJumpEnd, SoundRequestPriority.Normal);
        stage = PreFlashStage.Instance;
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

        // The completed sequence advances to the room fade with no Samus draw. The outer game-state owner is outside this
        // sequence, so it stays locked.
        SamusDeathDraw draws = stage is SteppingDeathStage stepping
            ? stepping.Step(new DeathUpdate(this, samus, samusColours, artwork))
            : SamusDeathDraw.Nothing;

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
            death.Frame(SamusDeathSheetDefinitions.ExplosionFrame(Facing, frame)),
            ScreenPosition));
    }

    /// <summary>Plays the explosion on, its death shades and terminal whiteout, returning whether to draw this frame.</summary>
    private bool StepSuitExplosion(SamusState samus, SamusColours samusColours, SamusDeathSteps artwork,
        AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue> explosion)
    {
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
            stage = CompleteStage.Instance;
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

        // The whiteout shows a new shade on every update of the explosion; one that showed none would keep its last.
        if (whiteout.StepToNewFrame() is { } shade)
            ShowWhiteout(samusColours, artwork, shade);
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

    /// <summary>A stage of the death sequence, named by the phase it is.</summary>
    private abstract class DeathStage
    {
        /// <summary>The phase the stage is.</summary>
        internal abstract SamusDeathSequencePhase Phase { get; }
    }

    /// <summary>A stage that plays on each update: the wait before the flash, the flash and the explosion.</summary>
    private abstract class SteppingDeathStage : DeathStage
    {
        /// <summary>Runs one update of the stage, returning what it draws of Samus.</summary>
        /// <param name="update">The sequence, Samus, her colours and the death's shades this update.</param>
        internal abstract SamusDeathDraw Step(DeathUpdate update);
    }

    /// <summary>One update of the death sequence, as a stage plays it.</summary>
    /// <param name="Sequence">The sequence the stage is of.</param>
    /// <param name="Samus">Samus, dying.</param>
    /// <param name="Colours">Her colours, which the stage shades.</param>
    /// <param name="Artwork">The death's shades and whiteout colours.</param>
    private readonly record struct DeathUpdate(SamusDeathSequenceState Sequence, SamusState Samus, SamusColours Colours, SamusDeathSteps Artwork);

    /// <summary>Samus is not dying.</summary>
    private sealed class NotDyingStage : DeathStage
    {
        /// <summary>The stage's only value.</summary>
        internal static NotDyingStage Instance { get; } = new();

        /// <inheritdoc/>
        internal override SamusDeathSequencePhase Phase => SamusDeathSequencePhase.Inactive;
    }

    /// <summary>The wait before the flash, while the death pose animates.</summary>
    private sealed class PreFlashStage : SteppingDeathStage
    {
        /// <summary>The stage's only value.</summary>
        internal static PreFlashStage Instance { get; } = new();

        /// <inheritdoc/>
        internal override SamusDeathSequencePhase Phase => SamusDeathSequencePhase.PreFlashing;

        /// <inheritdoc/>
        internal override SamusDeathDraw Step(DeathUpdate update)
        {
            // The death animation advances the death pose's frame delays only during the pre-flash state.
            // The flashing state freezes that final body frame while its shades alternate.
            SamusPresentation.AnimateDeathFrame(update.Samus);
            if (update.Sequence.PreFlashingTimer.StepEnds())
                update.Sequence.stage = FlashStage.Instance;
            return SamusDeathDraw.Pose;
        }
    }

    /// <summary>The flash, alternating the death shades until the suit explodes.</summary>
    private sealed class FlashStage : SteppingDeathStage
    {
        /// <summary>The stage's only value.</summary>
        internal static FlashStage Instance { get; } = new();

        /// <inheritdoc/>
        internal override SamusDeathSequencePhase Phase => SamusDeathSequencePhase.Flashing;

        /// <inheritdoc/>
        internal override SamusDeathDraw Step(DeathUpdate update)
        {
            if (update.Sequence.Flashing.Step())
            {
                // The flash's last update shows the first death shade, starts the explosion on its first picture, and
                // immediately plays and draws it in this same update.
                ShowDeathShades(update.Samus, update.Colours, SamusDeathShade.First);
                var exploding = new ExplosionStage(SamusDeathAnimations.ExplosionStarting());
                update.Sequence.WhiteoutBrightening = null;
                update.Sequence.stage = exploding;
                return update.Sequence.StepSuitExplosion(update.Samus, update.Colours, update.Artwork, exploding.Explosion)
                    ? SamusDeathDraw.Explosion
                    : SamusDeathDraw.Nothing;
            }
            if (update.Sequence.Flash.StepToNewFrame() is { } flashShade)
                ShowDeathShades(update.Samus, update.Colours, flashShade);
            return SamusDeathDraw.Pose;
        }
    }

    /// <summary>
    /// The suit's explosion (#2388), started on the flash's last update: its pictures played once for their dwell times,
    /// the sequence completing once its last picture has shown.
    /// </summary>
    /// <param name="explosion">The explosion's pictures, from its first.</param>
    private sealed class ExplosionStage(AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue> explosion)
        : SteppingDeathStage
    {
        /// <summary>The explosion's pictures as they play.</summary>
        internal AnimationPlayer<SamusDeathExplosionSegment, SamusDeathExplosionFrame, NoAnimationCue> Explosion { get; } = explosion;

        /// <inheritdoc/>
        internal override SamusDeathSequencePhase Phase => SamusDeathSequencePhase.SuitExplosion;

        /// <inheritdoc/>
        internal override SamusDeathDraw Step(DeathUpdate update)
        {
            update.Sequence.ApplyWhiteout(update.Colours, update.Artwork);
            return update.Sequence.StepSuitExplosion(update.Samus, update.Colours, update.Artwork, Explosion)
                ? SamusDeathDraw.Explosion
                : SamusDeathDraw.Nothing;
        }
    }

    /// <summary>The terminal white frame has shown; the sequence stays the owner until the room fades.</summary>
    private sealed class CompleteStage : DeathStage
    {
        /// <summary>The stage's only value.</summary>
        internal static CompleteStage Instance { get; } = new();

        /// <inheritdoc/>
        internal override SamusDeathSequencePhase Phase => SamusDeathSequencePhase.Complete;
    }
}
