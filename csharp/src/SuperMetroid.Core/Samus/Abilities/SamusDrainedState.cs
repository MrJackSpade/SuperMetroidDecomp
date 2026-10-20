using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Literal Samus-side translation of the drained controller at <c>$91:E4AD</c> and its
/// installed falling handler at <c>$90:94CB</c>.
/// </summary>
/// <remarks>
/// Mother Brain and the Baby Metroid own when controller functions zero through four are
/// called. This class deliberately does not invent that enemy AI. It owns only the WRAM
/// mutations made by those calls and the movement pointer installed later by animation
/// command <c>$F7</c>, giving the debugger an exact seam for the future actor producer.
/// </remarks>
public sealed class SamusDrainedState
{
    /// <summary>Host-owned Hyper Beam palette artwork, excluded from debugger-state serialization.</summary>
    [field: NonSerialized]
    public SamusHyperBeamColorCatalog? PresentationColors { get; set; }

    /// <summary>
    /// Latches the immediate suit-palette restore from controller zero or command $17
    /// until the same frame's software-CGRAM pass runs.
    /// </summary>
    private bool _suitPaletteRestoreRequested;

    /// <summary>Host-readable substitute for the active native pose/handler combination.</summary>
    public DrainedSamusPhase Phase { get; private set; }

    /// <summary>
    /// Live translation of object <c>$8D:E1F0</c>. It animates OBJ palette-six colors
    /// one through eight independently of the earlier full-body rainbow palette handler.
    /// </summary>
    public HyperBeamPaletteFxState HyperBeamPaletteFx { get; } = new();

    /// <summary>
    /// Host-readable form of super-special-palette flag `$8000`, written by Samus command
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
    public NativeCountdown CommonPaletteTimer { get; private set; }

    /// <summary>
    /// Index of the Hyper Beam palette shown by the rainbow cycle, 0 through the palette count minus one: cleared by
    /// both rainbow commands and advanced by one on each timer expiry, returning to zero after the last palette.
    /// </summary>
    public int ChargePaletteIndex { get; private set; }

    /// <summary>
    /// Host-readable identity of the timer/hack function installed by Samus command five
    /// or <c>$18</c>. It deliberately survives the later `$91:E4AD` pose changes, just as
    /// native WRAM's function pointer does.
    /// </summary>
    public DrainedGetUpHandler GetUpHandler { get; internal set; }

    /// <summary>
    /// Another owner replaced the physical movement pointer and pose before drained art
    /// installed its falling callback. The actor may continue its own AI, but this Samus
    /// handler marker must not shadow the replacement after that owner completes.
    /// </summary>
    internal void RelinquishMovementHandler() => Phase = DrainedSamusPhase.Inactive;

    /// <summary>
    /// Samus command $0F ($90:F2D8) replaces the shared hack-handler word with timer
    /// processing. The runtime owns that timer; the old drained callback must stop.
    /// </summary>
    internal void RelinquishTimerHackHandler() => GetUpHandler = DrainedGetUpHandler.Inactive;

    /// <summary>
    /// Ports Samus command five at <c>$90:F38E</c>: install the able-to-stand timer handler,
    /// then enter the shared rainbow-beam setup at <c>$90:F394</c>.
    /// </summary>
    public void SetupForRainbowBeamAbleToStand(SamusState samus)
    {
        GetUpHandler = DrainedGetUpHandler.AbleToStand;
        SetupForRainbowBeam(samus);
    }

    /// <summary>
    /// Ports Samus command <c>$18</c> at <c>$90:F3C0</c>: install the failed-stand timer
    /// handler, then enter the same locked left-knockback pose as command five.
    /// </summary>
    public void SetupForRainbowBeamUnableToStand(SamusState samus)
    {
        GetUpHandler = DrainedGetUpHandler.UnableToStand;
        SetupForRainbowBeam(samus);
    }

    /// <summary>
    /// Runs one call of the installed <c>$90:E09B/$E0C5</c> timer/hack handler. Native calls
    /// it after movement and immediately before animation, so a written timer of one is
    /// decremented and the selected art begins advancing in that same beta pass.
    /// </summary>
    public bool StepGetUpHandler(SamusState samus, SnesButton newlyPressedInput)
    {
        Ensure.NotNull(samus);
        SnesButton newlyPressed = newlyPressedInput;
        if (!newlyPressed.HasAny(SnesButton.Up))
            return false;

        if (GetUpHandler == DrainedGetUpHandler.AbleToStand &&
            samus.Pose == SamusPoseId.DrainedCrouchingLeftPose &&
            samus.AnimationFrame >= 8)
        {
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 13, timer: 1);

            // `$90:E0BE` replaces the hack pointer with an RTS after the first accepted Up
            // edge. Further presses cannot restart the stand-up stream.
            GetUpHandler = DrainedGetUpHandler.Inactive;
            return true;
        }

        if (GetUpHandler == DrainedGetUpHandler.UnableToStand &&
            samus.AnimationFrame >= 8 &&
            samus.AnimationFrame < 12)
        {
            // Unlike the able branch, `$90:E0C5` retains its own function pointer. A later
            // visit to frames 8..11 can therefore react to another fresh Up edge.
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 18, timer: 1);
            return true;
        }

        return false;
    }

    /// <summary>Ports Samus command <c>$19</c> at <c>$90:F3FB</c>.</summary>
    /// <remarks>The command returns carry set, which cancels the pending pose transition.</remarks>
    public static void FreezeForHyperBeamAcquisition(SamusState samus)
    {
        Ensure.NotNull(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 28, timer: 1);
        samus.CancelPendingPoseTransition();
    }

    /// <summary>Ports Samus command <c>$16</c> at <c>$90:F3C9</c>.</summary>
    public void EnableRainbow(SamusState samus)
    {
        Ensure.NotNull(samus);
        RainbowPaletteEnabled = true;
        SpecialPaletteFrame = 1;
        CommonPaletteTimer = NativeCountdown.Start(1);
        ChargePaletteIndex = 0;
        _suitPaletteRestoreRequested = false;
    }

    /// <summary>
    /// Applies `$A9:CD59-$CD65`'s frame increment after the Baby's fractional `$0300`
    /// accumulator carries. The native comparison clamps at ten rather than wrapping.
    /// </summary>
    /// <param name="maximumFrame">The largest hold the cycle may reach; the Baby passes ten.</param>
    public void IncrementRainbowPaletteFrame(int maximumFrame)
    {
        // The frame never exceeds the caller's small maximum, so the increment cannot reach a wrap.
        int incremented = SpecialPaletteFrame + 1;
        SpecialPaletteFrame = incremented < maximumFrame ? incremented : maximumFrame;
    }

    /// <summary>
    /// Ports Samus command <c>$17</c> at <c>$90:F3DD</c>: clear every rainbow/charge palette
    /// word and begin the standing portion of the drained animation at frame thirteen.
    /// </summary>
    public void DisableRainbowAndStartStandingAnimation(SamusState samus)
    {
        Ensure.NotNull(samus);
        RainbowPaletteEnabled = false;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = NativeCountdown.Start(0);
        ChargePaletteIndex = 0;
        _suitPaletteRestoreRequested = true;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 13, timer: 1);
    }

    /// <summary>
    /// Runs the negative-super-special branch of `$91:D6F7/$91:D954-$D997`.
    /// </summary>
    /// <remarks>
    /// While command `$16`'s `$8000` flag is set, this handler has absolute priority over
    /// charge, Speed Booster, shinespark, Crystal Flash, and X-ray palettes. It loads one
    /// complete 16-color Hyper Beam palette before touching either timer. Installed
    /// artwork supplies those colors; a missing catalog is an error, not a native-byte
    /// fallback. The Baby
    /// independently raises <see cref="SpecialPaletteFrame"/> from one through ten; that
    /// value becomes the delay between advances of the ten-entry palette index.
    /// </remarks>
    /// <returns>True when this state owned the current frame's Samus palette dispatch.</returns>
    public bool UpdatePalette(IPaletteColors cgram, SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        SamusSuitColorCatalog? suitColors = null)
    {
        Ensure.NotNull(cgram);

        if (_suitPaletteRestoreRequested)
        {
            // `$90:F3E9` and `$91:E56B` choose Gravity before Varia, then Power. The table
            // stores byte offsets 0/2/4 rather than a host enum, matching every other Samus
            // palette restoration path in bank `$91`.
            SamusNormalSuitPalette.Load(cgram, equippedItems, suitColors);
            _suitPaletteRestoreRequested = false;
            return true;
        }

        if (!RainbowPaletteEnabled)
            return false;

        // `$91:D96F` doubles the palette number, selects one of ten complete palette
        // frames, and copies all sixteen colors before decrementing the shared timer.
        int paletteFrame = ChargePaletteIndex %
            SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteCount;
        if (PresentationColors is null)
            throw new InvalidOperationException("Drained Samus requires installed Hyper Beam colors.");
        for (int index = 0; index < SamusHyperBeamColorFormat.ColorsPerFrame; index++)
            new SamusPalettes(cgram).Suit.Write(index,
                PresentationColors.Resolve(paletteFrame, index));

        NativeWordCounterStep timer = CommonPaletteTimer.Decrement();
        CommonPaletteTimer = timer.Remaining;
        if (!timer.IsZero && timer.IsNonNegative)
            return true;

        CommonPaletteTimer = NativeCountdown.Start(SpecialPaletteFrame);
        ChargePaletteIndex++;
        if (ChargePaletteIndex >= SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteCount)
            ChargePaletteIndex = 0;
        return true;
    }

    /// <summary>
    /// Ports controller function zero, “let drained Samus fall,” at <c>$91:E4F8</c>.
    /// </summary>
    public void LetFall(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Native computes 21-oldRadius and subtracts it from center Y before changing the
        // pose. This preserves the previous bottom boundary even when the source body did
        // not already have the drained pose's radius of 21 pixels.
        ushort sourceRadius = samus.Kinematics.Radius.Y;
        samus.Position = samus.Position.WithWholeY(unchecked((ushort)(
            samus.Position.Y.Whole - unchecked((ushort)(21 - sourceRadius)))));

        // Direction byte four is left; every other byte chooses the right-facing record.
        // Read it before replacing the pose, exactly as `$91:E50D` does.
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.DrainedCrouchingLeftPose
            : SamusPoseId.DrainedCrouchingRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Special ||
            samus.Kinematics.Radius.Y != 21)
            throw new InvalidDataException("ROM drained pose $E8/$E9 metadata changed unexpectedly.");

        // NewPoseSamusAnimationFrame is literally two. The animation is initialized here,
        // but movement remains the normal RTS type-$1B handler until command `$F7` appears.
        SamusPresentation.InitializeAnimation(samus, initialFrame: 2);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        _suitPaletteRestoreRequested = true;
        Phase = DrainedSamusPhase.WaitingForFallingCommand;
        // Native controller commands 0/1/2/4 return carry set; their dispatcher
        // then shifts history even for a same-pose command. Hyper (3) does not.
        samus.CommitPoseHistory();
    }

    /// <summary>
    /// Called only by animation command <c>$F7</c> at <c>$90:8360</c>; the generic delay
    /// interpreter performs the command's separate frame increment.
    /// </summary>
    internal void InstallFallingMovementHandler(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.Pose is not (
            SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose))
        {
            throw new InvalidOperationException(
                $"Drained falling command requires pose $E8/$E9, not ${(int)samus.Pose:X2}.");
        }

        // Command `$F7` writes `$90:94CB` into the single physical movement-handler word.
        // It therefore replaces either kind of special owner: an ordinary uncrashed spark
        // retains its boost words as Blue Suit, while Crystal Flash retains its independent
        // palette/input state as Shinespark Suit. Neither old movement routine may resume.
        samus.Shinespark.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishMovementHandler();
        Phase = DrainedSamusPhase.Falling;
    }

    /// <summary>
    /// Runs one call of <c>SamusMovementHandler_SamusDrained_Falling</c> at
    /// <c>$90:94CB</c>, using the shared native Y-speed calculation and block collision.
    /// </summary>
    public DrainedSamusMovementResult StepFalling(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (Phase != DrainedSamusPhase.Falling)
            throw new InvalidOperationException("The drained falling movement handler is not installed.");

        BlockMoveResult vertical = SamusAerialMovement.StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level }, samus, nmiFrameCounter, out _, out bool downwardDisplacement);

        // Generic Y movement has already applied gravity when it reports the collision.
        // Capture the exact magnitude before `$90:94E5-$94E8` erases it for on-floor art.
        SnesUnsignedSixteenSixteen impactYSpeed = samus.Kinematics.YSpeed;

        if (vertical.Collided)
        {
            // `$90:94D3-$94E8` restores the normal movement pointer, jumps directly to art
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

    /// <summary>Ports controller function one at <c>$91:E571</c>.</summary>
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

    /// <summary>Ports controller function four at <c>$91:E60C</c>.</summary>
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

    /// <summary>Ports controller function two at <c>$91:E59B</c>.</summary>
    public void Release(SamusState samus)
    {
        Ensure.NotNull(samus);

        // These are literal byte indices into the asymmetrical delay programs, not visual
        // frame counts. In particular, left `$E9` index 13 names an operand byte; native
        // intentionally draws that index until the next animation decrement advances it.
        if (samus.Pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 13, timer: 1);
        else if (samus.Pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 4, timer: 1);

        // The merge path executes even for an unexpected pose. Preserve that surprisingly
        // broad native behavior instead of rejecting a caller that the 65816 accepts.
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        Phase = DrainedSamusPhase.Releasing;
        samus.CommitPoseHistory();
    }

    /// <summary>Ports controller function three at <c>$91:E5F0</c>.</summary>
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
        // Shared rainbow setup calls the native previous-pose helper before lock.
        samus.CommitPoseHistory();
        // Shared `$90:F394` installs the locked current/new state handlers after changing art.
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
