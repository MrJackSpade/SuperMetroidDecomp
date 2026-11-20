using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The Samus side of Mother Brain's drain: the drained poses and the drained fall, while the drain holds her movement
/// (<see cref="SamusMovementOwner.Drained"/>).
/// </summary>
/// <remarks>
/// Mother Brain and the Baby Metroid own when the drain's operations below are
/// called. This class deliberately does not invent that enemy AI. It owns only the state
/// mutations made by those calls and the fall the drained-fall animation cue starts later.
/// </remarks>
public sealed class SamusDrainedState
{
    /// <summary>
    /// Where the rainbow's colouring stands (#2562): showing, or ended by letting drained Samus fall or the rainbow's stop,
    /// the same update's colour pass then showing her suit's colours again.
    /// </summary>
    private SpecialColouringStage _colouringStage;

    /// <summary>The drained phase: the drained pose Samus holds, or her fall.</summary>
    public DrainedSamusPhase Phase { get; private set; }

    /// <summary>
    /// The drained Samus's Hyper Beam glow colouring, or null until she receives the Hyper Beam (<see cref="EnableHyperBeam"/>).
    /// It animates the glow's colours independently of the earlier full-body rainbow colouring.
    /// </summary>
    public HyperBeamColourEffectState? HyperBeamColourEffect { get; private set; }

    /// <summary>
    /// Whether the rainbow glow colours Samus, ahead of every other colouring: the rainbow start sets it and the rainbow
    /// stop clears it during the Baby Metroid's death.
    /// </summary>
    public bool RainbowGlowing => rainbowHold is not null;

    /// <summary>
    /// How long the rainbow holds each of its shades while it glows (#2387, #2560): one update as the rainbow starts,
    /// lengthened by the Baby; none while it does not glow. Each shade the rainbow moves on to shows for the hold as it
    /// stands then.
    /// </summary>
    private RainbowGlowHold? rainbowHold;

    /// <summary>
    /// The rainbow's play of the Hyper Beam's full-body glow (#2387, #2447): the shade it shows, none outside the rainbow,
    /// and the updates that shade still shows for. The rainbow's start shows its first shade for the starting hold, so the
    /// first colouring moves it on at once, and each shade it moves on to shows for <see cref="RainbowGlowHold"/> as it
    /// stands then, not for its own beat.
    /// </summary>
    private AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> Rainbow { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.HyperBeamRainbow);

    /// <summary>
    /// How Samus answers Up after each movement, which either drain setup chooses: able or unable to stand. It
    /// deliberately survives the later drained pose changes, just as it does in the game.
    /// </summary>
    public DrainedGetUpCheck GetUpCheck { get; private set; }

    /// <summary>
    /// Ends the drain's hold on Samus's movement when another owner takes her movement and pose before the drain's
    /// art starts her fall (<see cref="SamusMovementHandOver"/>). The actor may continue its own AI, but the drain must
    /// not hold her again after that owner lets go.
    /// </summary>
    internal void RelinquishMovement() => Phase = DrainedSamusPhase.Inactive;

    /// <summary>
    /// Ends the drained get-up when the escape countdown starts: the countdown's display takes the place of the check
    /// that runs after her movement. The runtime owns that timer; the old drained behaviour must stop.
    /// </summary>
    internal void RelinquishGetUp() => GetUpCheck = DrainedGetUpCheck.Inactive;

    /// <summary>
    /// Runs the able-to-stand drain setup: Up can stand her once she is crouched,
    /// then enter the shared rainbow-beam setup.
    /// </summary>
    public void SetupForRainbowBeamAbleToStand(SamusState samus)
    {
        GetUpCheck = DrainedGetUpCheck.AbleToStand;
        SetupForRainbowBeam(samus);
    }

    /// <summary>
    /// Runs the unable-to-stand drain setup: Up only starts a failed stand,
    /// then enter the same locked left-knockback pose as the able-to-stand setup.
    /// </summary>
    public void SetupForRainbowBeamUnableToStand(SamusState samus)
    {
        GetUpCheck = DrainedGetUpCheck.UnableToStand;
        SetupForRainbowBeam(samus);
    }

    /// <summary>
    /// Runs one drained get-up check. The game runs
    /// it after movement and immediately before animation, so a timer set to expire at once is
    /// decremented and the selected art begins advancing in that same movement phase.
    /// </summary>
    public bool RunGetUpCheck(SamusState samus, GameplayAction newlyPressedInput)
    {
        Ensure.NotNull(samus);
        GameplayAction newlyPressed = newlyPressedInput;
        if (!newlyPressed.HasAny(GameplayAction.Up))
            return false;

        if (GetUpCheck == DrainedGetUpCheck.AbleToStand &&
            samus.Pose == SamusPoseId.DrainedCrouchingLeftPose &&
            samus.AnimationHasReached(SamusDrainedAnimationDefinitions.AbleToStandFirstFrame))
        {
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.AbleToStandFrame, timer: SamusDrainedAnimationDefinitions.SetFrameTimer);

            // Able to stand, she ends her get-up after the first accepted Up press. Further presses cannot restart the
            // stand-up.
            GetUpCheck = DrainedGetUpCheck.Inactive;
            return true;
        }

        if (GetUpCheck == DrainedGetUpCheck.UnableToStand &&
            samus.AnimationHasReached(SamusDrainedAnimationDefinitions.UnableToStandFirstFrame) &&
            !samus.AnimationHasPassed(SamusDrainedAnimationDefinitions.UnableToStandLastFrame))
        {
            // Unable to stand, she keeps her get-up. A later visit to the held crouch's frames that accept Up can
            // therefore react to another fresh Up press.
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.UnableToStandFrame, timer: SamusDrainedAnimationDefinitions.SetFrameTimer);
            return true;
        }

        return false;
    }

    /// <summary>Freezes the drained animation while the Hyper Beam is acquired.</summary>
    /// <remarks>The freeze reports itself handled, which cancels the pending pose transition.</remarks>
    public static void FreezeForHyperBeamAcquisition(SamusState samus)
    {
        Ensure.NotNull(samus);
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame, timer: SamusDrainedAnimationDefinitions.SetFrameTimer);
        samus.PoseTransitionCancellation.Cancel();
    }

    /// <summary>Starts Samus's rainbow colouring.</summary>
    public void EnableRainbow(SamusState samus)
    {
        Ensure.NotNull(samus);
        rainbowHold = RainbowGlowHold.Starting;
        SamusGlowPlay.RestartShowing(Rainbow, SamusGlowSegment.HyperBeamRainbow, RainbowGlowHold.Starting.Updates);
        _colouringStage = SpecialColouringStage.Showing;
    }

    /// <summary>
    /// Holds each shade of the rainbow one update longer, each time the Baby's slowing of it completes a step
    /// (RainbowFramePart); the hold stops at <paramref name="longestHold"/>.
    /// </summary>
    /// <param name="longestHold">The longest hold the glow may reach, which the Baby passes.</param>
    /// <remarks>A rainbow that does not glow holds nothing: its start holds each shade one update again.</remarks>
    public void HoldRainbowGlowLonger(int longestHold)
    {
        if (rainbowHold is { } hold)
            rainbowHold = hold.Lengthened(longestHold);
    }

    /// <summary>
    /// Stops Samus's rainbow colouring: clear every rainbow/charge colouring
    /// value and begin the standing portion of the drained animation at its rainbow-stop frame.
    /// </summary>
    public void DisableRainbowAndStartStandingAnimation(SamusState samus)
    {
        Ensure.NotNull(samus);
        rainbowHold = null;
        SamusGlowPlay.Restart(Rainbow, SamusGlowSegment.HyperBeamRainbow);
        _colouringStage = SpecialColouringStage.Ending;
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.RainbowStopFrame, timer: SamusDrainedAnimationDefinitions.SetFrameTimer);
    }

    /// <summary>
    /// Runs Samus's rainbow suit colouring after the Baby drains her, while the rainbow colouring is enabled.
    /// </summary>
    /// <remarks>
    /// While the rainbow colouring is enabled, this colouring has absolute priority over
    /// charge, Speed Booster, shinespark, Crystal Flash, and X-ray colourings. It shows its shade of the Hyper Beam's
    /// full-body glow on every update before counting down its hold; the import pre-colours Samus in each of its frames
    /// (#1512). The Baby independently lengthens <see cref="RainbowGlowHold"/> from one update up to its longest; that hold becomes
    /// the delay before the rainbow moves on to its next shade (#2387).
    /// </remarks>
    /// <returns>True when this state owned the current frame's Samus colours.</returns>
    public bool UpdateColouring(SamusColours colours, SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);

        if (_colouringStage == SpecialColouringStage.Ending)
        {
            // The drained shade and the rainbow stop choose Gravity before Varia, then Power, as every
            // other path restoring Samus's suit shade does.
            SamusSuitShade.Show(colours, equippedItems);
            _colouringStage = SpecialColouringStage.Showing;
            return true;
        }

        if (rainbowHold is not { } hold)
            return false;

        // The colouring shows the rainbow's shade before counting down its hold.
        colours.ShowSuit(Rainbow.Showing);

        // Each shade shows for the rainbow's hold as it stands when the rainbow moves on to it, not for its own beat.
        if (Rainbow.StepToNewFrame() is not null)
            Rainbow.HoldShownFrameFor(hold.Updates);
        return true;
    }

    /// <summary>
    /// Lets drained Samus fall.
    /// </summary>
    public void LetFall(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The game raises her center by the drained pose's radius less her previous one before changing the
        // pose. This preserves the previous bottom boundary even when the source body did
        // not already have the drained pose's radius.
        int sourceRadius = samus.Kinematics.Radius.Y;
        samus.Position = samus.Position.WithWholeY(
            samus.Position.Y.Whole - (SamusDrainedAnimationDefinitions.CrouchingYRadius - sourceRadius));

        // A left-facing pose chooses the left-facing drained pose; every other facing chooses the right-facing one.
        // Read it before replacing the pose, exactly as the drained setup does.
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.DrainedCrouchingLeftPose
            : SamusPoseId.DrainedCrouchingRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Special ||
            samus.Kinematics.Radius.Y != SamusDrainedAnimationDefinitions.CrouchingYRadius)
            throw new InvalidDataException("The drained crouching poses' metadata changed unexpectedly.");

        // The new pose starts on the collapse's third frame. The animation is initialized here,
        // but the special movement type keeps her still until the drained-fall
        // animation step appears.
        SamusPresentation.InitializeAnimation(samus, SamusDrainedAnimationDefinitions.FallInitialFrame);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.TurnedDownward();
        _colouringStage = SpecialColouringStage.Ending;
        Phase = DrainedSamusPhase.WaitingForFallingCue;
        // Letting her fall, standing her, crouching her and releasing her report themselves handled; their step
        // then shifts history even when the pose is unchanged. Enabling the Hyper Beam does not.
        samus.CommitPoseHistory();
    }

    /// <summary>
    /// Called only by the drained-fall animation cue; the generic delay
    /// step performs the cue's separate frame increment. The cue does not look at the pose: the drained
    /// crouches' animations hold it, and a landing pose whose frame the Baby's Hyper Beam freeze or rainbow stop sets reads
    /// into the drained crouch's animation and reaches it too, so that pose falls as a drained Samus does (#1535).
    /// </summary>
    internal void StartFalling(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The cue gives the drain Samus's movement for her fall. It therefore takes it from
        // either special owner: an ordinary uncrashed spark retains its boost state as Blue Suit,
        // while Crystal Flash retains its independent colour/input state as Shinespark Suit.
        // Neither old movement behaviour may resume.
        SamusMovementHandOver.To(samus, SamusMovementOwner.Drained);
        Phase = DrainedSamusPhase.Falling;
    }

    /// <summary>
    /// Runs one update of drained Samus's fall, using
    /// the shared Y-speed calculation and block collision.
    /// </summary>
    internal DrainedSamusMovementResult StepFalling(
        RoomLevelData level,
        ISamusRoomObjects roomObjects,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (Phase != DrainedSamusPhase.Falling)
            throw new InvalidOperationException("Drained Samus is not falling.");

        BlockMoveResult vertical = SamusAerialMovement.StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, frameCounter, out _, out bool downwardDisplacement);

        // Generic Y movement has already applied gravity when it reports the collision.
        // Capture the exact velocity before the landing erases its speed for on-floor art.
        SamusVerticalVelocity impact = samus.Kinematics.VerticalVelocity;

        if (vertical.Collided)
        {
            // The landing ends the fall, shows the held crouch's landing frame for its landing timer
            // (SamusDrainedAnimationDefinitions.LandingTimer) and stops her vertical speed. It keeps her vertical
            // direction and does not select a new pose.
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.LandingFrame,
                timer: SamusDrainedAnimationDefinitions.LandingTimer);
            samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.Stopped();
            Phase = DrainedSamusPhase.OnFloor;
        }

        return new DrainedSamusMovementResult(
            downwardDisplacement && vertical.Collided,
            impact);
    }

    /// <summary>Stands drained Samus up.</summary>
    public void PutStanding(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPoseId standing = facingLeft ? SamusPoseId.DrainedStandingLeftPose : SamusPoseId.DrainedStandingRightPose;
        SamusPresentation.SetPoseAndAnimationForScriptedSequence(samus,
            standing,
            frame: SamusAnimationDelays.AnimationForPose(standing).Start,
            timer: SamusDrainedAnimationDefinitions.PutStandingTimer);
        Phase = DrainedSamusPhase.Standing;
        samus.CommitPoseHistory();
    }

    /// <summary>Puts drained Samus back into the drained crouch.</summary>
    public void CrouchDrained(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPresentation.SetPoseAndAnimationForScriptedSequence(samus,
            facingLeft ? SamusPoseId.DrainedCrouchingLeftPose : SamusPoseId.DrainedCrouchingRightPose,
            frame: SamusDrainedAnimationDefinitions.PutCrouchingFrame,
            timer: SamusDrainedAnimationDefinitions.PutCrouchingTimer);
        Phase = DrainedSamusPhase.Crouching;
        samus.CommitPoseHistory();
    }

    /// <summary>Releases drained Samus.</summary>
    public void Release(SamusState samus)
    {
        Ensure.NotNull(samus);

        // These are named places in the left-facing drained animations; where each lands in the animation Samus plays is an
        // installed outcome (#2048). The crouch's release place is the one its held crouch's repeat carries rather than a
        // frame of its own, and it is drawn until the next animation step advances it.
        if (samus.Pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.ReleaseCrouchingFrame, timer: SamusDrainedAnimationDefinitions.SetFrameTimer);
        else if (samus.Pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusDrainedAnimationDefinitions.ReleaseStandingFrame, timer: SamusDrainedAnimationDefinitions.SetFrameTimer);

        // The shared step runs for any pose, an unexpected one included: the game accepts
        // every pose here.
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.TurnedDownward();
        Phase = DrainedSamusPhase.Releasing;
        samus.CommitPoseHistory();
    }

    /// <summary>
    /// Equips the Hyper Beam: Charge, Wave and Plasma, the rainbow colour effect, and no charging sound left to resume.
    /// The enemy pass that runs this loads the equipped beams' artwork, as the game does here (#1561).
    /// </summary>
    public void EnableHyperBeam(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.EquippedBeams =
            SamusBeamFlags.Charge |
            SamusBeamFlags.Wave |
            SamusBeamFlags.Plasma;
        samus.HyperBeamEnabled = true;
        HyperBeamColourEffect = new();
        samus.ChargingSound.Cancel();
    }

    /// <summary>Ends the Samus-side drain lock after release art reaches its transition to the facing standing pose.</summary>
    internal void CompleteRelease() => Phase = DrainedSamusPhase.Inactive;

    /// <summary>Starts the shared left-knockback rainbow setup and locks Samus input.</summary>
    private void SetupForRainbowBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The rainbow setup unconditionally chooses left-facing knockback KnockbackLeftPose, even when
        // Samus had faced right. This is why letting her fall later selects DrainedCrouchingLeftPose.
        samus.Pose = SamusPoseId.KnockbackLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // The shared rainbow setup records the pose history before the lock.
        samus.CommitPoseHistory();
        // The shared rainbow setup locks Samus's input and movement after changing art.
        samus.Control.LockForRainbowBeam();
        Phase = DrainedSamusPhase.RainbowBeamLocked;
    }

    /// <summary>Stops Samus's horizontal base speed and her vertical speed during drained pose changes.</summary>
    private static void ClearBaseAndVerticalSpeed(SamusState samus)
    {
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.Stopped();
    }

}
