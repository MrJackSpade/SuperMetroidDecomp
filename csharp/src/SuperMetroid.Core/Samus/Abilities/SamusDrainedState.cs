using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
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
    /// Latches the immediate suit-palette restore from letting drained Samus fall or the rainbow stop
    /// until the same frame's palette pass runs.
    /// </summary>
    private bool _suitPaletteRestoreRequested;

    /// <summary>The drained phase: the drained pose Samus holds, or her fall.</summary>
    public DrainedSamusPhase Phase { get; private set; }

    /// <summary>
    /// The drained Samus's Hyper Beam glow colouring. It animates the glow's colours independently of the earlier
    /// full-body rainbow colouring.
    /// </summary>
    public HyperBeamColourEffectState HyperBeamColourEffect { get; } = new();

    /// <summary>
    /// The super-special-palette flag, set by the rainbow start and cleared
    /// by the rainbow stop during the Baby Metroid death sequence.
    /// </summary>
    public bool RainbowPaletteEnabled { get; private set; }

    /// <summary>
    /// Calls each Hyper Beam palette is held for during the cutscene rainbow cycle, 0 through the Baby's maximum of
    /// ten: the rainbow start sets it to one, the Baby raises it one at a time and never past its maximum,
    /// and the rainbow stop clears it. Each palette advance reloads the common palette timer from it.
    /// </summary>
    public int SpecialPaletteFrame { get; private set; }

    /// <summary>Common palette timer initialized to one by the rainbow start.</summary>
    public int CommonPaletteTimer { get; private set; }

    /// <summary>
    /// The step of the Hyper Beam glow the rainbow cycle shows, 0 through the glow's step count minus one: cleared by
    /// the rainbow start and stop and advanced by one on each timer expiry, returning to zero after the last step.
    /// </summary>
    public int HyperBeamGlowStep { get; private set; }

    /// <summary>
    /// How Samus answers Up after each movement, which either drain setup chooses: able or unable to stand. It
    /// deliberately survives the later drained pose changes, just as it does in the game.
    /// </summary>
    public DrainedGetUpCheck GetUpCheck { get; internal set; }

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
    /// it after movement and immediately before animation, so a written timer of one is
    /// decremented and the selected art begins advancing in that same movement phase.
    /// </summary>
    public bool RunGetUpCheck(SamusState samus, ControllerButton newlyPressedInput)
    {
        Ensure.NotNull(samus);
        ControllerButton newlyPressed = newlyPressedInput;
        if (!newlyPressed.HasAny(ControllerButton.Up))
            return false;

        if (GetUpCheck == DrainedGetUpCheck.AbleToStand &&
            samus.Pose == SamusPoseId.DrainedCrouchingLeftPose &&
            samus.AnimationFrame >= SamusDrainedAnimationDefinitions.AbleToStandFirstFrame)
        {
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.AbleToStandFrame, timer: 1);

            // The able branch ends its get-up after the first accepted Up
            // edge. Further presses cannot restart the stand-up stream.
            GetUpCheck = DrainedGetUpCheck.Inactive;
            return true;
        }

        if (GetUpCheck == DrainedGetUpCheck.UnableToStand &&
            samus.AnimationFrame >= SamusDrainedAnimationDefinitions.UnableToStandFirstFrame &&
            samus.AnimationFrame <= SamusDrainedAnimationDefinitions.UnableToStandLastFrame)
        {
            // Unlike the able branch, the failed-stand branch keeps its get-up. A later
            // visit to frames 8..11 can therefore react to another fresh Up edge.
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.UnableToStandFrame, timer: 1);
            return true;
        }

        return false;
    }

    /// <summary>Freezes the drained animation while the Hyper Beam is acquired.</summary>
    /// <remarks>The freeze reports itself handled, which cancels the pending pose transition.</remarks>
    public static void FreezeForHyperBeamAcquisition(SamusState samus)
    {
        Ensure.NotNull(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame, timer: 1);
        samus.CancelPendingPoseTransition();
    }

    /// <summary>Starts Samus's rainbow colouring.</summary>
    public void EnableRainbow(SamusState samus)
    {
        Ensure.NotNull(samus);
        RainbowPaletteEnabled = true;
        SpecialPaletteFrame = 1;
        CommonPaletteTimer = 1;
        HyperBeamGlowStep = 0;
        _suitPaletteRestoreRequested = false;
    }

    /// <summary>
    /// Applies the Baby's frame increment after the Baby's fractional 768
    /// counter overflows a whole unit. The comparison clamps at ten rather than wrapping.
    /// </summary>
    /// <param name="maximumFrame">The largest hold the cycle may reach; the Baby passes ten.</param>
    public void IncrementRainbowPaletteFrame(int maximumFrame)
    {
        // The frame never exceeds the caller's small maximum, so the increment cannot reach a wrap.
        int incremented = SpecialPaletteFrame + 1;
        SpecialPaletteFrame = incremented < maximumFrame ? incremented : maximumFrame;
    }

    /// <summary>
    /// Stops Samus's rainbow colouring: clear every rainbow/charge palette
    /// value and begin the standing portion of the drained animation at frame thirteen.
    /// </summary>
    public void DisableRainbowAndStartStandingAnimation(SamusState samus)
    {
        Ensure.NotNull(samus);
        RainbowPaletteEnabled = false;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = 0;
        HyperBeamGlowStep = 0;
        _suitPaletteRestoreRequested = true;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.RainbowStopFrame, timer: 1);
    }

    /// <summary>
    /// Runs Samus's rainbow suit colouring after the Baby drains her, while the rainbow palette is enabled.
    /// </summary>
    /// <remarks>
    /// While the rainbow palette is enabled, this colouring has absolute priority over
    /// charge, Speed Booster, shinespark, Crystal Flash, and X-ray palettes. It loads one
    /// complete Hyper Beam glow shade before touching either timer; the import pre-colours Samus in each of its frames
    /// (#1512). The Baby
    /// independently raises <see cref="SpecialPaletteFrame"/> from one through ten; that
    /// value becomes the delay between steps through the glow's ten shades.
    /// </remarks>
    /// <returns>True when this state owned the current frame's Samus palette dispatch.</returns>
    public bool UpdatePalette(SamusColours colours, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);

        if (_suitPaletteRestoreRequested)
        {
            // The drained palette and the rainbow stop choose Gravity before Varia, then Power, as every
            // other Samus palette restoration path does.
            SamusNormalSuitPalette.Load(colours, equippedItems);
            _suitPaletteRestoreRequested = false;
            return true;
        }

        if (!RainbowPaletteEnabled)
            return false;

        // The colouring shows the glow's current step before counting down the shared timer.
        colours.ShowSuit(SamusShades.HyperBeamSuit(HyperBeamGlowStep));

        if (--CommonPaletteTimer > 0)
            return true;

        CommonPaletteTimer = SpecialPaletteFrame;
        HyperBeamGlowStep++;
        if (HyperBeamGlowStep >= SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount)
            HyperBeamGlowStep = 0;
        return true;
    }

    /// <summary>
    /// Lets drained Samus fall.
    /// </summary>
    public void LetFall(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The game computes 21-oldRadius and subtracts it from center Y before changing the
        // pose. This preserves the previous bottom boundary even when the source body did
        // not already have the drained pose's radius of 21 pixels.
        int sourceRadius = samus.Kinematics.Radius.Y;
        samus.Position = samus.Position.WithWrappedWholeY(
            samus.Position.Y.Whole - CoordinateAxis.Wrap(21 - sourceRadius));

        // A left-facing pose chooses the left-facing record; every other facing chooses the right-facing one.
        // Read it before replacing the pose, exactly as the drained setup does.
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.DrainedCrouchingLeftPose
            : SamusPoseId.DrainedCrouchingRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Special ||
            samus.Kinematics.Radius.Y != 21)
            throw new InvalidDataException("The drained crouching poses' metadata changed unexpectedly.");

        // The new pose's animation frame is literally two. The animation is initialized here,
        // but the special movement type keeps her still until the drained-fall
        // animation step appears.
        SamusPresentation.InitializeAnimation(samus, initialFrame: SamusDrainedAnimationDefinitions.FallInitialFrame);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        _suitPaletteRestoreRequested = true;
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
        // while Crystal Flash retains its independent palette/input state as Shinespark Suit.
        // Neither old movement behaviour may resume.
        SamusMovementHandOver.To(samus, SamusMovementOwner.Drained);
        Phase = DrainedSamusPhase.Falling;
    }

    /// <summary>
    /// Runs one call of drained Samus's fall, using
    /// the shared Y-speed calculation and block collision.
    /// </summary>
    public DrainedSamusMovementResult StepFalling(

        RoomLevelData level,
        SamusState samus,
        int frameCounter)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (Phase != DrainedSamusPhase.Falling)
            throw new InvalidOperationException("Drained Samus is not falling.");

        BlockMoveResult vertical = SamusAerialMovement.StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level }, samus, frameCounter, out _, out bool downwardDisplacement);

        // Generic Y movement has already applied gravity when it reports the collision.
        // Capture the exact magnitude before the landing erases it for on-floor art.
        UnsignedSixteenSixteen impactYSpeed = samus.Kinematics.YSpeed;

        if (vertical.Collided)
        {
            // The landing ends the fall, jumps directly to art
            // frame seven for eight ticks, and clears both velocity halves. It does not
            // clear Y direction and it does not select a new pose.
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.LandingFrame,
                timer: SamusDrainedAnimationDefinitions.LandingTimer);
            samus.Kinematics.YSpeed = default;
            Phase = DrainedSamusPhase.OnFloor;
        }

        return new DrainedSamusMovementResult(
            downwardDisplacement && vertical.Collided,
            impactYSpeed);
    }

    /// <summary>Stands drained Samus up.</summary>
    public void PutStanding(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPresentation.SetPoseAndAnimationFromScriptedController(samus, 
            facingLeft ? SamusPoseId.DrainedStandingLeftPose : SamusPoseId.DrainedStandingRightPose,
            frame: 0,
            timer: 16);
        Phase = DrainedSamusPhase.Standing;
        samus.CommitPoseHistory();
    }

    /// <summary>Puts drained Samus back into the drained crouch.</summary>
    public void PutCrouchingOrFalling(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPresentation.SetPoseAndAnimationFromScriptedController(samus, 
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

        // These are literal indices into the asymmetrical delay animations, not visual
        // frame counts. In particular, the left-facing drained crouch's index 13 lands inside a
        // cue's argument rather than on a frame; the game
        // intentionally draws that index until the next animation decrement advances it.
        if (samus.Pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.ReleaseCrouchingFrame, timer: 1);
        else if (samus.Pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: SamusDrainedAnimationDefinitions.ReleaseStandingFrame, timer: 1);

        // The merge path executes even for an unexpected pose. Preserve that surprisingly
        // broad behavior instead of rejecting a caller that the game accepts.
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        Phase = DrainedSamusPhase.Releasing;
        samus.CommitPoseHistory();
    }

    /// <summary>
    /// Equips the Hyper Beam: Charge, Wave and Plasma, the rainbow palette effect, and no charging sound left to resume.
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
        HyperBeamColourEffect.Spawn();
        samus.ResumeChargingBeamSoundPending = false;
    }

    /// <summary>Ends the Samus-side drain lock after release art reaches its transition to the facing standing pose.</summary>
    internal void CompleteRelease() => Phase = DrainedSamusPhase.Inactive;

    /// <summary>Starts the shared left-knockback rainbow setup and locks Samus input.</summary>
    internal void SetupForRainbowBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The rainbow setup unconditionally chooses left-facing knockback KnockbackLeftPose, even when
        // Samus had faced right. This is why letting her fall later selects DrainedCrouchingLeftPose.
        samus.Pose = SamusPoseId.KnockbackLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Shared rainbow setup calls the previous-pose helper before lock.
        samus.CommitPoseHistory();
        // The shared rainbow setup installs the locked input mode and the rainbow beam's movement mode after changing art.
        samus.Control.LockForRainbowBeam();
        Phase = DrainedSamusPhase.RainbowBeamLocked;
    }

    /// <summary>Erases horizontal base speed and both halves of vertical speed during drained pose changes.</summary>
    private static void ClearBaseAndVerticalSpeed(SamusState samus)
    {
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;
    }

}
