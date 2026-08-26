using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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
    /// <summary>Host-readable substitute for the active native pose/handler combination.</summary>
    public DrainedSamusPhase Phase { get; private set; }

    /// <summary>
    /// True after controller function three writes equipped beams <c>$1009</c> and hyper
    /// beam flag <c>$8000</c>. Palette-FX object creation remains presentation work.
    /// </summary>
    public bool HyperBeamPaletteFxRequested { get; private set; }

    /// <summary>
    /// Host-readable form of super-special-palette flag `$8000`, written by Samus command
    /// `$16` and cleared by command `$17` during the Baby Metroid death sequence.
    /// </summary>
    public bool RainbowPaletteEnabled { get; private set; }

    /// <summary>Global special-Samus-palette frame used by the cutscene rainbow cycle.</summary>
    public ushort SpecialPaletteFrame { get; private set; }

    /// <summary>Common palette timer initialized to one by command `$16`.</summary>
    public ushort CommonPaletteTimer { get; private set; }

    /// <summary>Charge-palette index cleared by both rainbow commands.</summary>
    public ushort ChargePaletteIndex { get; private set; }

    /// <summary>
    /// Host-readable identity of the timer/hack function installed by Samus command five
    /// or <c>$18</c>. It deliberately survives the later `$91:E4AD` pose changes, just as
    /// native WRAM's function pointer does.
    /// </summary>
    public DrainedGetUpHandler GetUpHandler { get; private set; }

    /// <summary>
    /// Ports Samus command five at <c>$90:F38E</c>: install the able-to-stand timer handler,
    /// then enter the shared rainbow-beam setup at <c>$90:F394</c>.
    /// </summary>
    public void SetupForRainbowBeamAbleToStand(ISnesAddressSpace bus, SamusState samus)
    {
        GetUpHandler = DrainedGetUpHandler.AbleToStand;
        SetupForRainbowBeam(bus, samus);
    }

    /// <summary>
    /// Ports Samus command <c>$18</c> at <c>$90:F3C0</c>: install the failed-stand timer
    /// handler, then enter the same locked left-knockback pose as command five.
    /// </summary>
    public void SetupForRainbowBeamUnableToStand(ISnesAddressSpace bus, SamusState samus)
    {
        GetUpHandler = DrainedGetUpHandler.UnableToStand;
        SetupForRainbowBeam(bus, samus);
    }

    /// <summary>
    /// Runs one call of the installed <c>$90:E09B/$E0C5</c> timer/hack handler. Native calls
    /// it after movement and immediately before animation, so a written timer of one is
    /// decremented and the selected art begins advancing in that same beta pass.
    /// </summary>
    public bool StepGetUpHandler(SamusState samus, ushort newlyPressedInput)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if ((newlyPressedInput & 0x0800) == 0)
            return false;

        if (GetUpHandler == DrainedGetUpHandler.AbleToStand &&
            samus.Pose == SamusState.DrainedCrouchingLeftPose &&
            samus.AnimationFrame >= 8)
        {
            samus.SetAnimationFrameFromSpecialHandler(frame: 13, timer: 1);

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
            samus.SetAnimationFrameFromSpecialHandler(frame: 18, timer: 1);
            return true;
        }

        return false;
    }

    /// <summary>Ports Samus command <c>$19</c> at <c>$90:F3FB</c>.</summary>
    public void FreezeForHyperBeamAcquisition(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        samus.SetAnimationFrameFromSpecialHandler(frame: 28, timer: 1);
    }

    /// <summary>Ports Samus command <c>$16</c> at <c>$90:F3C9</c>.</summary>
    public void EnableRainbow(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        RainbowPaletteEnabled = true;
        SpecialPaletteFrame = 1;
        CommonPaletteTimer = 1;
        ChargePaletteIndex = 0;
    }

    /// <summary>
    /// Applies `$A9:CD59-$CD65`'s frame increment after the Baby's fractional `$0300`
    /// accumulator carries. The native comparison clamps at ten rather than wrapping.
    /// </summary>
    public void IncrementRainbowPaletteFrame(ushort maximumFrame)
    {
        ushort incremented = unchecked((ushort)(SpecialPaletteFrame + 1));
        SpecialPaletteFrame = incremented < maximumFrame ? incremented : maximumFrame;
    }

    /// <summary>
    /// Ports Samus command <c>$17</c> at <c>$90:F3DD</c>: clear every rainbow/charge palette
    /// word and begin the standing portion of the drained animation at frame thirteen.
    /// </summary>
    public void DisableRainbowAndStartStandingAnimation(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        RainbowPaletteEnabled = false;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = 0;
        ChargePaletteIndex = 0;
        samus.SetAnimationFrameFromSpecialHandler(frame: 13, timer: 1);
    }

    /// <summary>
    /// Ports controller function zero, “let drained Samus fall,” at <c>$91:E4F8</c>.
    /// </summary>
    public void LetFall(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // Native computes 21-oldRadius and subtracts it from center Y before changing the
        // pose. This preserves the previous bottom boundary even when the source body did
        // not already have the drained pose's radius of 21 pixels.
        ushort sourceRadius = samus.Kinematics.YRadius;
        samus.YPosition = unchecked((ushort)(
            samus.YPosition - unchecked((ushort)(21 - sourceRadius))));

        // Direction byte four is left; every other byte chooses the right-facing record.
        // Read it before replacing the pose, exactly as `$91:E50D` does.
        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        samus.Pose = facingLeft
            ? SamusState.DrainedCrouchingLeftPose
            : SamusState.DrainedCrouchingRightPose;
        samus.RefreshCollisionRadii(bus);
        if (samus.ReadMovementType(bus) != 0x1b || samus.Kinematics.YRadius != 21)
            throw new InvalidDataException("ROM drained pose $E8/$E9 metadata changed unexpectedly.");

        // NewPoseSamusAnimationFrame is literally two. The animation is initialized here,
        // but movement remains the normal RTS type-$1B handler until command `$F7` appears.
        samus.InitializeAnimation(bus, initialFrame: 2);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = 2;
        Phase = DrainedSamusPhase.WaitingForFallingCommand;
    }

    /// <summary>
    /// Called only by animation command <c>$F7</c> at <c>$90:8360</c>; the generic delay
    /// interpreter performs the command's separate frame increment.
    /// </summary>
    internal void InstallFallingMovementHandler(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (samus.Pose is not (
            SamusState.DrainedCrouchingRightPose or SamusState.DrainedCrouchingLeftPose))
        {
            throw new InvalidOperationException(
                $"Drained falling command requires pose $E8/$E9, not ${samus.Pose:X2}.");
        }

        Phase = DrainedSamusPhase.Falling;
    }

    /// <summary>
    /// Runs one call of <c>SamusMovementHandler_SamusDrained_Falling</c> at
    /// <c>$90:94CB</c>, using the shared native Y-speed calculation and block collision.
    /// </summary>
    public DrainedSamusMovementResult StepFalling(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (Phase != DrainedSamusPhase.Falling)
            throw new InvalidOperationException("The drained falling movement handler is not installed.");

        BlockMoveResult vertical = SamusAerialMovement.StepVerticalWithSpeedCalculations(
            bus,
            level,
            samus,
            nmiFrameCounter,
            out bool hitCeiling,
            out bool downwardDisplacement);

        if (vertical.Collided)
        {
            // `$90:94D3-$94E8` restores the normal movement pointer, jumps directly to art
            // frame seven for eight ticks, and clears both velocity halves. It does not
            // clear Y direction and it does not select a new pose.
            samus.SetAnimationFrameFromSpecialHandler(frame: 7, timer: 8);
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            Phase = DrainedSamusPhase.OnFloor;
        }

        return new DrainedSamusMovementResult(
            vertical,
            hitCeiling,
            downwardDisplacement && vertical.Collided,
            Phase);
    }

    /// <summary>Ports controller function one at <c>$91:E571</c>.</summary>
    public void PutStanding(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        samus.SetPoseAndAnimationFromScriptedController(
            bus,
            facingLeft ? SamusState.DrainedStandingLeftPose : SamusState.DrainedStandingRightPose,
            frame: 0,
            timer: 16,
            refreshRadius: false);
        Phase = DrainedSamusPhase.Standing;
    }

    /// <summary>Ports controller function four at <c>$91:E60C</c>.</summary>
    public void PutCrouchingOrFalling(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        samus.SetPoseAndAnimationFromScriptedController(
            bus,
            facingLeft ? SamusState.DrainedCrouchingLeftPose : SamusState.DrainedCrouchingRightPose,
            frame: 8,
            timer: 16,
            refreshRadius: false);
        Phase = DrainedSamusPhase.Crouching;
    }

    /// <summary>Ports controller function two at <c>$91:E59B</c>.</summary>
    public void Release(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // These are literal byte indices into the asymmetrical delay programs, not visual
        // frame counts. In particular, left `$E9` index 13 names an operand byte; native
        // intentionally draws that index until the next animation decrement advances it.
        if (samus.Pose is SamusState.DrainedCrouchingRightPose or SamusState.DrainedCrouchingLeftPose)
            samus.SetAnimationFrameFromSpecialHandler(frame: 13, timer: 1);
        else if (samus.Pose is SamusState.DrainedStandingRightPose or SamusState.DrainedStandingLeftPose)
            samus.SetAnimationFrameFromSpecialHandler(frame: 4, timer: 1);

        // The merge path executes even for an unexpected pose. Preserve that surprisingly
        // broad native behavior instead of rejecting a caller that the 65816 accepts.
        samus.RefreshCollisionRadii(bus);
        ClearBaseAndVerticalSpeed(samus);
        samus.Kinematics.YDirection = 2;
        Phase = DrainedSamusPhase.Releasing;
    }

    /// <summary>Ports controller function three at <c>$91:E5F0</c>.</summary>
    public void EnableHyperBeam(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        samus.EquippedBeams = 0x1009;
        samus.HyperBeam = 0x8000;
        HyperBeamPaletteFxRequested = true;
    }

    /// <summary>Ends the Samus-side drain lock after release art reaches `$FD,$01/$02`.</summary>
    internal void CompleteRelease() => Phase = DrainedSamusPhase.Inactive;

    private void SetupForRainbowBeam(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // Both commands unconditionally choose left-facing knockback pose `$54`, even when
        // Samus had faced right. This is why controller zero later selects drained pose `$E9`.
        samus.Pose = SamusState.KnockbackLeftPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        // Shared `$90:F394` installs the locked current/new state handlers after changing art.
        samus.InputLocked = true;
        Phase = DrainedSamusPhase.RainbowBeamLocked;
    }

    private static void ClearBaseAndVerticalSpeed(SamusState samus)
    {
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
    }
}

/// <summary>Named host equivalents for the otherwise opaque drain pose/handler state.</summary>
public enum DrainedSamusPhase
{
    Inactive,
    RainbowBeamLocked,
    WaitingForFallingCommand,
    Falling,
    OnFloor,
    Standing,
    Crouching,
    Releasing,
}

/// <summary>Named equivalents of the three timer/hack pointer values relevant to draining.</summary>
public enum DrainedGetUpHandler
{
    Inactive,
    AbleToStand,
    UnableToStand,
}

/// <summary>One-frame debugger witness from the translated `$90:94CB` handler.</summary>
public readonly record struct DrainedSamusMovementResult(
    BlockMoveResult Vertical,
    bool HitCeiling,
    bool Landed,
    DrainedSamusPhase PhaseAfterStep);
