using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Literal Samus-side translation of the drained controller and its
/// installed falling handler.
/// </summary>
/// <remarks>
/// Mother Brain and the Baby Metroid own when controller functions zero through four are
/// called. This class deliberately does not invent that enemy AI. It owns only the state
/// mutations made by those calls and the movement pointer installed later by animation
/// command <c>$F7</c>, giving the debugger an exact seam for the future actor producer.
/// </remarks>
public sealed class SamusDrainedState
{
    /// <summary>Host-owned Hyper Beam palette artwork, excluded from debugger-state serialization.</summary>
    [field: NonSerialized]
    public SamusHyperBeamColours? PresentationColors { get; set; }

    /// <summary>
    /// Latches the immediate suit-palette restore from controller zero or command $17
    /// until the same frame's palette pass runs.
    /// </summary>
    private bool _suitPaletteRestoreRequested;

    /// <summary>Host-readable substitute for the active pose/handler combination.</summary>
    public DrainedSamusPhase Phase { get; private set; }

    /// <summary>
    /// Live translation of the drained palette object. It animates sprite palette-six colors
    /// one through eight independently of the earlier full-body rainbow palette handler.
    /// </summary>
    public HyperBeamPaletteFxState HyperBeamPaletteFx { get; } = new();

    /// <summary>
    /// Host-readable form of the super-special-palette flag, written by Samus command
    /// `$16` and cleared by command `$17` during the Baby Metroid death sequence.
    /// </summary>
    public bool RainbowPaletteEnabled { get; private set; }

    /// <summary>
    /// Calls each Hyper Beam palette is held for during the cutscene rainbow cycle, 0 through the Baby's maximum of
    /// ten: command <c>$16</c> starts it at one, the Baby raises it one at a time and never past its maximum, and
    /// command <c>$17</c> clears it. Each palette advance reloads the common palette timer from it.
    /// </summary>
    public int SpecialPaletteFrame { get; private set; }

    /// <summary>Common palette timer initialized to one by command `$16`.</summary>
    public WrappingCountdown CommonPaletteTimer { get; private set; }

    /// <summary>
    /// Index of the Hyper Beam palette shown by the rainbow cycle, 0 through the palette count minus one: cleared by
    /// both rainbow commands and advanced by one on each timer expiry, returning to zero after the last palette.
    /// </summary>
    public int ChargePaletteIndex { get; private set; }

    /// <summary>
    /// Host-readable identity of the timer/hack function installed by Samus command five
    /// or <c>$18</c>. It deliberately survives the later drained pose changes, just as
    /// the game's function pointer does.
    /// </summary>
    public DrainedGetUpHandler GetUpHandler { get; internal set; }

    /// <summary>
    /// Another owner replaced the physical movement pointer and pose before drained art
    /// installed its falling callback. The actor may continue its own AI, but this Samus
    /// handler marker must not shadow the replacement after that owner completes.
    /// </summary>
    internal void RelinquishMovementHandler() => Phase = DrainedSamusPhase.Inactive;

    /// <summary>
    /// Samus command $0F replaces the shared hack-handler word with timer
    /// processing. The runtime owns that timer; the old drained callback must stop.
    /// </summary>
    internal void RelinquishTimerHackHandler() => GetUpHandler = DrainedGetUpHandler.Inactive;

    /// <summary>
    /// Ports Samus command five: install the able-to-stand timer handler,
    /// then enter the shared rainbow-beam setup.
    /// </summary>
    public void SetupForRainbowBeamAbleToStand(SamusState samus)
    {
        GetUpHandler = DrainedGetUpHandler.AbleToStand;
        SetupForRainbowBeam(samus);
    }

    /// <summary>
    /// Ports Samus command <c>$18</c>: install the failed-stand timer
    /// handler, then enter the same locked left-knockback pose as command five.
    /// </summary>
    public void SetupForRainbowBeamUnableToStand(SamusState samus)
    {
        GetUpHandler = DrainedGetUpHandler.UnableToStand;
        SetupForRainbowBeam(samus);
    }

    /// <summary>
    /// Runs one call of the installed timer/hack handler. The game calls
    /// it after movement and immediately before animation, so a written timer of one is
    /// decremented and the selected art begins advancing in that same beta pass.
    /// </summary>
    public bool StepGetUpHandler(SamusState samus, ControllerButton newlyPressedInput)
    {
        Ensure.NotNull(samus);
        ControllerButton newlyPressed = newlyPressedInput;
        if (!newlyPressed.HasAny(ControllerButton.Up))
            return false;

        if (GetUpHandler == DrainedGetUpHandler.AbleToStand &&
            samus.Pose == SamusPoseId.DrainedCrouchingLeftPose &&
            samus.AnimationFrame >= 8)
        {
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 13, timer: 1);

            // The able branch replaces the hack pointer with a no-op after the first accepted Up
            // edge. Further presses cannot restart the stand-up stream.
            GetUpHandler = DrainedGetUpHandler.Inactive;
            return true;
        }

        if (GetUpHandler == DrainedGetUpHandler.UnableToStand &&
            samus.AnimationFrame >= 8 &&
            samus.AnimationFrame < 12)
        {
            // Unlike the able branch, the failed-stand branch retains its own function pointer. A later
            // visit to frames 8..11 can therefore react to another fresh Up edge.
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 18, timer: 1);
            return true;
        }

        return false;
    }

    /// <summary>Ports Samus command <c>$19</c>.</summary>
    /// <remarks>The command returns carry set, which cancels the pending pose transition.</remarks>
    public static void FreezeForHyperBeamAcquisition(SamusState samus)
    {
        Ensure.NotNull(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 28, timer: 1);
        samus.CancelPendingPoseTransition();
    }

    /// <summary>Ports Samus command <c>$16</c>.</summary>
    public void EnableRainbow(SamusState samus)
    {
        Ensure.NotNull(samus);
        RainbowPaletteEnabled = true;
        SpecialPaletteFrame = 1;
        CommonPaletteTimer = WrappingCountdown.Start(1);
        ChargePaletteIndex = 0;
        _suitPaletteRestoreRequested = false;
    }

    /// <summary>
    /// Applies the Baby's frame increment after the Baby's fractional 768
    /// accumulator carries. The comparison clamps at ten rather than wrapping.
    /// </summary>
    /// <param name="maximumFrame">The largest hold the cycle may reach; the Baby passes ten.</param>
    public void IncrementRainbowPaletteFrame(int maximumFrame)
    {
        // The frame never exceeds the caller's small maximum, so the increment cannot reach a wrap.
        int incremented = SpecialPaletteFrame + 1;
        SpecialPaletteFrame = incremented < maximumFrame ? incremented : maximumFrame;
    }

    /// <summary>
    /// Ports Samus command <c>$17</c>: clear every rainbow/charge palette
    /// word and begin the standing portion of the drained animation at frame thirteen.
    /// </summary>
    public void DisableRainbowAndStartStandingAnimation(SamusState samus)
    {
        Ensure.NotNull(samus);
        RainbowPaletteEnabled = false;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = WrappingCountdown.Start(0);
        ChargePaletteIndex = 0;
        _suitPaletteRestoreRequested = true;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 13, timer: 1);
    }

    /// <summary>
    /// Runs the negative-super-special branch of Samus's palette handler.
    /// </summary>
    /// <remarks>
    /// While command `$16`'s flag is set, this handler has absolute priority over
    /// charge, Speed Booster, shinespark, Crystal Flash, and X-ray palettes. It loads one
    /// complete 16-color Hyper Beam palette before touching either timer. Installed
    /// artwork supplies those colors; a missing catalog is an error, not a fallback to the original data.
    /// fallback. The Baby
    /// independently raises <see cref="SpecialPaletteFrame"/> from one through ten; that
    /// value becomes the delay between advances of the ten-entry palette index.
    /// </remarks>
    /// <returns>True when this state owned the current frame's Samus palette dispatch.</returns>
    public bool UpdatePalette(SamusColours colours, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        InstalledSamusSuitColors? suitColors = null)
    {
        Ensure.NotNull(colours);

        if (_suitPaletteRestoreRequested)
        {
            // The drained palette and command `$17` choose Gravity before Varia, then Power. The table
            // stores byte offsets 0/2/4 rather than a host enum, matching every other Samus
            // palette restoration path.
            SamusNormalSuitPalette.Load(colours.Suit, equippedItems, suitColors);
            _suitPaletteRestoreRequested = false;
            return true;
        }

        if (!RainbowPaletteEnabled)
            return false;

        // The handler doubles the palette number, selects one of ten complete palette
        // frames, and copies all sixteen colors before decrementing the shared timer.
        int paletteFrame = ChargePaletteIndex %
            SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount;
        if (PresentationColors is null)
            throw new InvalidOperationException("Drained Samus requires installed Hyper Beam colors.");
        PresentationColors.Apply(colours, paletteFrame);

        CountdownStep timer = CommonPaletteTimer.Decrement();
        CommonPaletteTimer = timer.Remaining;
        if (!timer.IsZero && timer.IsNonNegative)
            return true;

        CommonPaletteTimer = WrappingCountdown.Start(SpecialPaletteFrame);
        ChargePaletteIndex++;
        if (ChargePaletteIndex >= SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount)
            ChargePaletteIndex = 0;
        return true;
    }

    /// <summary>
    /// Ports controller function zero, “let drained Samus fall,”.
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

        // Direction byte four is left; every other byte chooses the right-facing record.
        // Read it before replacing the pose, exactly as the drained setup does.
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.DrainedCrouchingLeftPose
            : SamusPoseId.DrainedCrouchingRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Special ||
            samus.Kinematics.Radius.Y != 21)
            throw new InvalidDataException("Drained pose $E8/$E9 metadata changed unexpectedly.");

        // NewPoseSamusAnimationFrame is literally two. The animation is initialized here,
        // but movement remains the normal empty type-$1B handler until command `$F7` appears.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 2);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        _suitPaletteRestoreRequested = true;
        Phase = DrainedSamusPhase.WaitingForFallingCommand;
        // Controller commands 0/1/2/4 return carry set; their dispatcher
        // then shifts history even for a same-pose command. Hyper (3) does not.
        samus.CommitPoseHistory();
    }

    /// <summary>
    /// Called only by animation command <c>$F7</c>; the generic delay
    /// interpreter performs the command's separate frame increment.
    /// </summary>
    internal void InstallFallingMovementHandler(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.Pose is not (
            SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose))
        {
            throw new InvalidOperationException(
                $"Drained falling command requires pose $E8/$E9, not {samus.Pose}.");
        }

        // Command `$F7` writes the drained falling handler into the single physical movement-handler word.
        // It therefore replaces either kind of special owner: an ordinary uncrashed spark
        // retains its boost words as Blue Suit, while Crystal Flash retains its independent
        // palette/input state as Shinespark Suit. Neither old movement routine may resume.
        samus.Shinespark.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishMovementHandler();
        Phase = DrainedSamusPhase.Falling;
    }

    /// <summary>
    /// Runs one call of <c>SamusMovementHandler_SamusDrained_Falling</c>, using
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
            throw new InvalidOperationException("The drained falling movement handler is not installed.");

        BlockMoveResult vertical = SamusAerialMovement.StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level }, samus, frameCounter, out _, out bool downwardDisplacement);

        // Generic Y movement has already applied gravity when it reports the collision.
        // Capture the exact magnitude before the landing erases it for on-floor art.
        UnsignedSixteenSixteen impactYSpeed = samus.Kinematics.YSpeed;

        if (vertical.Collided)
        {
            // The landing restores the normal movement pointer, jumps directly to art
            // frame seven for eight ticks, and clears both velocity halves. It does not
            // clear Y direction and it does not select a new pose.
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 7, timer: 8);
            samus.Kinematics.YSpeed = default;
            Phase = DrainedSamusPhase.OnFloor;
        }

        return new DrainedSamusMovementResult(
            downwardDisplacement && vertical.Collided,
            impactYSpeed);
    }

    /// <summary>Ports controller function one.</summary>
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

    /// <summary>Ports controller function four.</summary>
    public void PutCrouchingOrFalling(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPresentation.SetPoseAndAnimationFromScriptedController(samus, 
            facingLeft ? SamusPoseId.DrainedCrouchingLeftPose : SamusPoseId.DrainedCrouchingRightPose,
            frame: 8,
            timer: 16);
        Phase = DrainedSamusPhase.Crouching;
        samus.CommitPoseHistory();
    }

    /// <summary>Ports controller function two.</summary>
    public void Release(SamusState samus)
    {
        Ensure.NotNull(samus);

        // These are literal byte indices into the asymmetrical delay programs, not visual
        // frame counts. In particular, left `$E9` index 13 names an operand byte; the game
        // intentionally draws that index until the next animation decrement advances it.
        if (samus.Pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 13, timer: 1);
        else if (samus.Pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 4, timer: 1);

        // The merge path executes even for an unexpected pose. Preserve that surprisingly
        // broad behavior instead of rejecting a caller that the game accepts.
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        Phase = DrainedSamusPhase.Releasing;
        samus.CommitPoseHistory();
    }

    /// <summary>Ports controller function three.</summary>
    public void EnableHyperBeam(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.EquippedBeams =
            SamusBeamFlags.Charge |
            SamusBeamFlags.Wave |
            SamusBeamFlags.Plasma;
        samus.HyperBeamEnabled = true;
        HyperBeamPaletteFx.Spawn();
    }

    /// <summary>Ends the Samus-side drain lock after release art reaches `$FD,$01/$02`.</summary>
    internal void CompleteRelease() => Phase = DrainedSamusPhase.Inactive;

    /// <summary>Starts the shared left-knockback rainbow setup and locks Samus input.</summary>
    internal void SetupForRainbowBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Both commands unconditionally choose left-facing knockback pose `$54`, even when
        // Samus had faced right. This is why controller zero later selects drained pose `$E9`.
        samus.Pose = SamusPoseId.KnockbackLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Shared rainbow setup calls the previous-pose helper before lock.
        samus.CommitPoseHistory();
        // The shared rainbow setup installs the locked current/new state handlers after changing art.
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
