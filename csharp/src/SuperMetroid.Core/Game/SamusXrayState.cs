using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible translation of X-ray's Samus-side setup, dedicated pose-input handler,
/// movement handler, beam-angle state, visor palette, and teardown.
/// </summary>
/// <remarks>
/// The retail feature crosses three banks: `$91:E16D` admits and installs the four X-ray
/// poses, `$91:FCAF` and `$90:E94F` own turning and angle-selected body art, and
/// `$88:86EF-$8AA3` widens/aims/deactivates the window beam. Keeping those words in one
/// explicit state prevents `$D5/$D6/$D9/$DA` from silently inheriting ordinary standing or
/// crouching input. The desktop compositor now consumes these exact angle/width words to
/// reproduce the moving ROM-tangent window; building the revealed-block BG2 tilemap remains
/// separate presentation work and no fake hidden tiles are generated here.
/// </remarks>
public sealed class SamusXrayState
{
    private const int VisorPaletteWords = 0x9ba3c0;
    private const int NormalSuitPalettePointerTable = 0x91d727;
    private const int SamusPaletteCgramIndex = 192;
    private const int VisorColorCgramIndex = SamusPaletteCgramIndex + 4;

    /// <summary>True while the dedicated bank-$91 X-ray input/movement handlers are installed.</summary>
    public bool IsActive { get; private set; }

    /// <summary>WRAM `$0A78`; X-ray freezes enemy/projectile/PLM/animated-tile time.</summary>
    public bool TimeIsFrozen { get; private set; }

    /// <summary>
    /// The eight setup calls in `$91:D22E-$D26F` which prepare/transfer the two BG2 screens
    /// before the bank-$88 main pre-instruction begins changing the beam state.
    /// </summary>
    public byte SetupStage { get; private set; }

    /// <summary>WRAM `$0A7A`; values zero through five index `$88:8726`.</summary>
    public XrayBeamPhase BeamPhase { get; private set; }

    /// <summary>WRAM `$0A7E`; right-facing angles occupy `$00-$7F`, left `$80-$FF`.</summary>
    public SnesAngle Angle { get; private set; }

    /// <summary>WRAM `$0A82`, the whole half-width of the beam in 8-bit angle units.</summary>
    public ushort AngularWidth { get; private set; }

    /// <summary>WRAM `$0A84`, the fractional half-width accumulator.</summary>
    public ushort AngularSubwidth { get; private set; }

    /// <summary>WRAM `$0A86`, the whole widening velocity.</summary>
    public ushort AngularWidthDelta { get; private set; }

    /// <summary>WRAM `$0A88`, the fractional widening velocity.</summary>
    public ushort AngularSubwidthDelta { get; private set; }

    /// <summary>
    /// WRAM `$0A8C`: zero while widening, one for the full beam, and `$FFFF` to make the
    /// palette handler restore normal suit colors after teardown.
    /// </summary>
    public ushort BeamSizeFlag { get; private set; }

    /// <summary>WRAM `$0A68`; eight selects the native visor palette handler.</summary>
    public ushort SpecialPaletteType { get; private set; }

    /// <summary>Typed view of the native special-palette handler index.</summary>
    public SamusSpecialPaletteType SpecialPaletteKind =>
        (SamusSpecialPaletteType)SpecialPaletteType;

    /// <summary>WRAM `$0ACE`, a byte offset into the six ROM visor-color words.</summary>
    public ushort SpecialPaletteFrame { get; private set; }

    /// <summary>WRAM `$0AD0`, the shared five-frame palette cadence timer.</summary>
    public ushort CommonPaletteTimer { get; private set; }

    /// <summary>Set by command five when sound-library-one effect nine would be queued.</summary>
    public bool ActivationSoundRequested { get; private set; }

    /// <summary>Set by state five when sound-library-one effect ten would be queued.</summary>
    public bool DeactivationSoundRequested { get; private set; }

    /// <summary>Consumes visor startup's native <c>QueueSfx1_Max6($09)</c> call.</summary>
    public bool ConsumeActivationSoundRequest()
    {
        bool requested = ActivationSoundRequested;
        ActivationSoundRequested = false;
        return requested;
    }

    /// <summary>Consumes HDMA teardown's native <c>QueueSfx1_Max6($0A)</c> call.</summary>
    public bool ConsumeDeactivationSoundRequest()
    {
        bool requested = DeactivationSoundRequested;
        DeactivationSoundRequested = false;
        return requested;
    }

    /// <summary>
    /// Ports the admission tests and pose selection in <c>XraySetup</c> at `$91:E16D`.
    /// The caller represents the already selected/equipped HUD item; item ownership is
    /// checked by the HUD selector one layer above this native routine.
    /// </summary>
    public bool TryBegin(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusMovementType previousMovementType,
        ushort gameState = 8,
        ushort powerBombExplosionStatus = 0,
        ushort projectileCooldownTimer = 0,
        ushort bombCounter = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        if (IsActive)
            return false;

        // `$91:E173-$E189` contains a very specific five-bomb/cooldown/divisor rejection.
        // Preserve the three-way conjunction; rejecting every nonzero cooldown would erase
        // a retail quirk, while ignoring it would permit the exact forbidden state.
        if (projectileCooldownTimer == 7 && bombCounter == 5 && samus.XSpeedDivisor == 2)
            return false;

        // Landing art `$A4-$A7/$E0-$E7` is explicitly excluded even though its movement
        // type is otherwise standing. The range comparisons are unsigned 16-bit compares.
        if (samus.Pose is >= SamusPoseIds.NormalLandingRightPose and
            <= SamusPoseIds.SpinLandingLeftPose or
            >= SamusPoseIds.LandingAimUpRightPose and <= SamusPoseIds.FiringLandingLeftPose)
            return false;

        if (gameState != 8 || powerBombExplosionStatus != 0 ||
            samus.Kinematics.YSpeed != 0 || samus.Kinematics.YSubspeed != 0)
        {
            return false;
        }

        SamusMovementType currentMovementType = samus.ReadMovementType(bus);
        if (ClassifyAllowedMovement(previousMovementType) == XrayPosture.Disallowed)
            return false;

        XrayPosture posture = ClassifyAllowedMovement(currentMovementType);
        if (posture == XrayPosture.Disallowed)
            return false;

        bool facingLeft = samus.IsFacingLeft(bus);
        samus.Pose = posture == XrayPosture.Crouching
            ? facingLeft ? SamusPoseIds.XrayingCrouchingLeftPose : SamusPoseIds.XrayingCrouchingRightPose
            : facingLeft ? SamusPoseIds.XrayingStandingLeftPose : SamusPoseIds.XrayingStandingRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        // Special prospective-pose command five immediately overrides the freshly selected
        // delay with frame two/timer `$3F`, installs the dedicated handler pair, starts the
        // visor palette, clears beam-flare state, and queues activation sound nine.
        samus.SetAnimationFrameFromSpecialHandler(frame: 2, timer: 0x003f);
        Angle = facingLeft ? SnesAngle.ThreeQuarterTurn : SnesAngle.QuarterTurn;
        AngularWidth = 0;
        AngularSubwidth = 0;
        AngularWidthDelta = 0;
        AngularSubwidthDelta = 0;
        BeamSizeFlag = 0;
        SpecialPaletteType = (ushort)SamusSpecialPaletteType.Xray;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = 1;
        ActivationSoundRequested = true;
        DeactivationSoundRequested = false;
        SetupStage = 1;
        BeamPhase = XrayBeamPhase.NoBeam;
        TimeIsFrozen = true;
        IsActive = true;
        return true;
    }

    /// <summary>
    /// Executes <c>XraySamusPoseInputHandler</c> at `$91:FCAF`, including the exact
    /// standing/crouching turn bodies and the frame-two/timer-one completion gate.
    /// </summary>
    public XrayPoseInputResult HandlePoseInput(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        EnsureActive();

        SamusMovementType movementType = samus.ReadMovementType(bus);
        if (movementType != SamusMovementType.TurningOnGround)
        {
            bool facingLeft = samus.IsFacingLeft(bus);
            ushort turnBinding = facingLeft
                ? (ushort)SnesButton.Right
                : (ushort)SnesButton.Left;
            if ((controllerInput & turnBinding) == 0)
                return default;

            // Mirroring through `$0100 - angle` turns right-space `$00-$7F` into its exact
            // left-space counterpart and vice versa. This happens before selecting turn art.
            Angle = SnesAngle.NormalizeRaw(-Angle.RawValue);
            bool crouching = movementType == SamusMovementType.Crouching;
            byte targetPose = (facingLeft, crouching) switch
            {
                (false, false) => SamusPoseIds.TurningRightToLeftPose,
                (false, true) => SamusPoseIds.TurningRightToLeftCrouchingPose,
                (true, false) => SamusPoseIds.TurningLeftToRightPose,
                (true, true) => SamusPoseIds.TurningLeftToRightCrouchingPose,
            };
            ApplyXrayPoseChange(bus, samus, targetPose);
            return new XrayPoseInputResult(true, false, targetPose, Angle);
        }

        // Native waits until the final visible turning frame has exactly one tick left.
        // Its beta movement handler is RTS for type `$0E`, so generic animation owns this
        // countdown without X-ray's usual per-frame timer reset.
        if (samus.AnimationFrame != 2 || samus.AnimationFrameTimer != 1)
            return default;

        bool nowFacingLeft = samus.IsFacingLeft(bus);
        byte completedPose = nowFacingLeft
            ? samus.Pose == SamusPoseIds.TurningRightToLeftPose
                ? SamusPoseIds.XrayingStandingLeftPose
                : SamusPoseIds.XrayingCrouchingLeftPose
            : samus.Pose == SamusPoseIds.TurningLeftToRightPose
                ? SamusPoseIds.XrayingStandingRightPose
                : SamusPoseIds.XrayingCrouchingRightPose;
        ApplyXrayPoseChange(bus, samus, completedPose);
        return new XrayPoseInputResult(false, true, completedPose, Angle);
    }

    /// <summary>
    /// Executes <c>SamusMovementHandler_Xray</c> at `$90:E94F`: turning is an RTS, while
    /// stable X-ray poses force timer fifteen and one of five angle-selected art frames.
    /// </summary>
    public ushort? StepMovement(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        EnsureActive();

        if (samus.ReadMovementType(bus) == SamusMovementType.TurningOnGround)
            return null;

        bool facingLeft = samus.IsFacingLeft(bus);
        ushort frame = facingLeft
            ? Angle.TableIndex < 0x99 ? (ushort)4 :
              Angle.TableIndex < 0xb2 ? (ushort)3 :
              Angle.TableIndex < 0xcb ? (ushort)2 :
              Angle.TableIndex < 0xe4 ? (ushort)1 : (ushort)0
            : Angle.TableIndex < 0x19 ? (ushort)0 :
              Angle.TableIndex < 0x32 ? (ushort)1 :
              Angle.TableIndex < 0x4b ? (ushort)2 :
              Angle.TableIndex < 0x64 ? (ushort)3 : (ushort)4;
        samus.SetAnimationFrameFromSpecialHandler(frame, timer: 15);
        return frame;
    }

    /// <summary>
    /// Advances one bank-$88 HDMA-object call. The setup-stage count represents the eight
    /// real cartridge functions. Actual BG2 copies remain intentionally outside this movement
    /// state; the renderer consumes its angle/width only after setup has completed.
    /// </summary>
    public XrayBeamStepResult StepBeam(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort dashBinding = (ushort)SnesButton.B,
        bool vramQueueHasRoom = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        EnsureActive();

        XrayBeamPhase phaseAtStart = BeamPhase;
        SnesAngle angleAtStart = Angle;
        ushort widthAtStart = AngularWidth;

        if (SetupStage != 0)
        {
            // Stages one through eight execute in order. After stage eight the instruction
            // list installs `$88:86EF`; state zero itself first runs on the following call.
            SetupStage = SetupStage >= 8 ? (byte)0 : unchecked((byte)(SetupStage + 1));
            return SnapshotBeamStep(phaseAtStart, angleAtStart, widthAtStart, completed: false);
        }

        bool holdingDash = (controllerInput & dashBinding) != 0;
        switch (BeamPhase)
        {
            case XrayBeamPhase.NoBeam:
                BeamPhase = holdingDash ? XrayBeamPhase.Widening : XrayBeamPhase.RestoreFirstHalf;
                break;

            case XrayBeamPhase.Widening:
                if (!holdingDash)
                {
                    BeamPhase = XrayBeamPhase.RestoreFirstHalf;
                    break;
                }

                // Four 16-bit words reproduce ADC carry propagation exactly. Fractional
                // widening velocity gains `$0800` per call; both velocity and width can
                // carry into their whole components before the ten-unit clamp.
                uint delta = ((uint)AngularWidthDelta << 16) | AngularSubwidthDelta;
                delta = unchecked(delta + 0x0000_0800u);
                AngularWidthDelta = unchecked((ushort)(delta >> 16));
                AngularSubwidthDelta = unchecked((ushort)delta);

                uint width = ((uint)AngularWidth << 16) | AngularSubwidth;
                width = unchecked(width + delta);
                AngularWidth = unchecked((ushort)(width >> 16));
                AngularSubwidth = unchecked((ushort)width);
                if (AngularWidth >= 11)
                {
                    AngularWidth = 10;
                    AngularSubwidth = 0;
                    BeamPhase = XrayBeamPhase.Full;
                }
                break;

            case XrayBeamPhase.Full:
                if (!holdingDash)
                {
                    BeamPhase = XrayBeamPhase.RestoreFirstHalf;
                    break;
                }

                // `$88:87C8` gives Up priority when opposite directions are both held.
                if ((controllerInput & (ushort)SnesButton.Up) != 0)
                    MoveAngleUp();
                else if ((controllerInput & (ushort)SnesButton.Down) != 0)
                    MoveAngleDown();
                break;

            case XrayBeamPhase.RestoreFirstHalf:
                // States three/four wait rather than dropping a BG2 transfer when the
                // seven-byte VRAM queue lacks room. The host renderer can expose that gate.
                if (vramQueueHasRoom)
                    BeamPhase = XrayBeamPhase.RestoreSecondHalf;
                break;

            case XrayBeamPhase.RestoreSecondHalf:
                if (vramQueueHasRoom)
                    BeamPhase = XrayBeamPhase.Finish;
                break;

            case XrayBeamPhase.Finish:
                Finish(bus, samus);
                break;

            default:
                throw new InvalidOperationException($"Unknown active X-ray beam phase {BeamPhase}.");
        }

        return SnapshotBeamStep(
            phaseAtStart,
            angleAtStart,
            widthAtStart,
            completed: !IsActive);
    }

    /// <summary>Runs `$91:DCB4-$DD30` and writes only visor color four until restoration.</summary>
    public bool UpdatePalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort equippedItems)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        if ((BeamSizeFlag & 0x8000) != 0)
        {
            // Carry clear from the native handler asks the outer dispatcher to load the
            // complete normal suit palette, then clears every X-ray palette word.
            ushort suitOffset = (equippedItems & 0x0020) != 0 ? (ushort)4 :
                (equippedItems & 0x0001) != 0 ? (ushort)2 : (ushort)0;
            ushort palettePointer = ReadWord(bus, NormalSuitPalettePointerTable + suitOffset);
            cgram.LoadFromBus(
                bus,
                0x9b0000 | palettePointer,
                colorCount: 16,
                destinationIndex: SamusPaletteCgramIndex);
            SpecialPaletteType = (ushort)SamusSpecialPaletteType.None;
            SpecialPaletteFrame = 0;
            CommonPaletteTimer = 0;
            BeamSizeFlag = 0;
            return true;
        }

        if (SpecialPaletteKind != SamusSpecialPaletteType.Xray)
            return false;

        if (BeamSizeFlag == 0 && (ushort)BeamPhase >= (ushort)XrayBeamPhase.Full)
        {
            // `$91:DCEA` enters the full-beam three-color loop at byte offset six.
            SpecialPaletteFrame = 6;
            CommonPaletteTimer = 1;
            BeamSizeFlag = 1;
        }

        NativeWordCounterStep timer = NativeWordCounter.Decrement(CommonPaletteTimer);
        CommonPaletteTimer = timer.Value;
        if (!timer.IsZero && timer.IsNonNegative)
            return false;

        CommonPaletteTimer = 5;
        cgram.SetColor(
            VisorColorCgramIndex,
            ReadWord(bus, VisorPaletteWords + SpecialPaletteFrame));

        if (BeamSizeFlag == 0)
        {
            // Widening uses offsets 0,2,4 and then pins four until state two is observed.
            if (SpecialPaletteFrame < 4)
                SpecialPaletteFrame = unchecked((ushort)(SpecialPaletteFrame + 2));
        }
        else
        {
            // Full beam cycles offsets 6,8,10,6... with the comparison after increment.
            ushort next = unchecked((ushort)(SpecialPaletteFrame + 2));
            SpecialPaletteFrame = next < 12 ? next : (ushort)6;
        }

        return true;
    }

    private void MoveAngleUp()
    {
        if (Angle.RawValue < SnesAngle.HalfTurn.RawValue)
        {
            // Right-facing upper limit is `angle - width >= 0`. Equality returns without
            // movement; crossing clamps the center to exactly the current half-width.
            int candidate = Angle.TableIndex - AngularWidth;
            if (candidate == 0)
                return;
            if (candidate < 0)
            {
                Angle = SnesAngle.FromTableIndex(checked((byte)AngularWidth));
                return;
            }

            Angle = Angle.AddTableUnits(-1);
            if (Angle.TableIndex - AngularWidth < 0)
                Angle = SnesAngle.FromTableIndex(checked((byte)AngularWidth));
            return;
        }

        // Left-facing up rotates toward `$100`; clamp the upper beam edge at exactly 256.
        int leftEdge = Angle.TableIndex + AngularWidth;
        if (leftEdge == 0x0100)
            return;
        if (leftEdge > 0x0100)
        {
            Angle = SnesAngle.NormalizeTableIndex(SnesAngle.TableUnitsPerTurn - AngularWidth);
            return;
        }

        Angle = Angle.AddTableUnits(1);
        if (Angle.TableIndex + AngularWidth > SnesAngle.TableUnitsPerTurn)
            Angle = SnesAngle.NormalizeTableIndex(SnesAngle.TableUnitsPerTurn - AngularWidth);
    }

    private void MoveAngleDown()
    {
        if (Angle.RawValue < SnesAngle.HalfTurn.RawValue)
        {
            // Right-facing down rotates toward `$80`; keep the lower edge at or below it.
            int lowerEdge = Angle.TableIndex + AngularWidth;
            if (lowerEdge == 0x0080)
                return;
            if (lowerEdge > 0x0080)
            {
                Angle = SnesAngle.NormalizeTableIndex(
                    SnesAngle.HalfTurn.TableIndex - AngularWidth);
                return;
            }

            Angle = Angle.AddTableUnits(1);
            if (Angle.TableIndex + AngularWidth > SnesAngle.HalfTurn.TableIndex)
                Angle = SnesAngle.NormalizeTableIndex(
                    SnesAngle.HalfTurn.TableIndex - AngularWidth);
            return;
        }

        // Left-facing down rotates toward `$80` from above and clamps its upper edge.
        int upperEdge = Angle.TableIndex - AngularWidth;
        if (upperEdge == 0x0080)
            return;
        if (upperEdge < 0x0080)
        {
            Angle = SnesAngle.NormalizeTableIndex(
                SnesAngle.HalfTurn.TableIndex + AngularWidth);
            return;
        }

        Angle = Angle.AddTableUnits(-1);
        if (Angle.TableIndex - AngularWidth < SnesAngle.HalfTurn.TableIndex)
            Angle = SnesAngle.NormalizeTableIndex(
                SnesAngle.HalfTurn.TableIndex + AngularWidth);
    }

    private void Finish(ISnesAddressSpace bus, SamusState samus)
    {
        // `$91:E2AD` intentionally classifies turning type `$0E` as standing. Releasing
        // X-ray during a crouched turn therefore stands Samus up—the documented retail
        // X-ray stand-up glitch—and the radius difference moves her center upward.
        SamusMovementType movementType = samus.ReadMovementType(bus);
        bool crouching = movementType == SamusMovementType.Crouching;
        bool facingLeft = samus.IsFacingLeft(bus);
        ushort oldRadius = samus.Kinematics.YRadius;
        byte targetPose = crouching
            ? facingLeft ? SamusPoseIds.CrouchingLeftPose : SamusPoseIds.CrouchingRightPose
            : facingLeft ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;

        samus.Pose = targetPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        int radiusDifference = samus.Kinematics.YRadius - oldRadius;
        if (radiusDifference >= 0)
            samus.YPosition = unchecked((ushort)(samus.YPosition - radiusDifference));

        TimeIsFrozen = false;
        IsActive = false;
        SetupStage = 0;
        BeamPhase = XrayBeamPhase.NoBeam;
        Angle = SnesAngle.Zero;
        AngularWidth = 0;
        AngularSubwidth = 0;
        AngularWidthDelta = 0;
        AngularSubwidthDelta = 0;
        BeamSizeFlag = 0xffff;
        DeactivationSoundRequested = true;
    }

    private static void ApplyXrayPoseChange(
        ISnesAddressSpace bus,
        SamusState samus,
        byte targetPose)
    {
        samus.Pose = targetPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
    }

    private static XrayPosture ClassifyAllowedMovement(SamusMovementType movementType) => movementType switch
    {
        SamusMovementType.Standing or
            SamusMovementType.Running or
            SamusMovementType.RanIntoWall => XrayPosture.Standing,
        SamusMovementType.Crouching => XrayPosture.Crouching,
        _ => XrayPosture.Disallowed,
    };

    private XrayBeamStepResult SnapshotBeamStep(
        XrayBeamPhase phaseAtStart,
        SnesAngle angleAtStart,
        ushort widthAtStart,
        bool completed) => new(
            phaseAtStart,
            BeamPhase,
            SetupStage,
            angleAtStart,
            Angle,
            widthAtStart,
            AngularWidth,
            completed);

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("X-ray has no installed Samus handler.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) | (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));

    private enum XrayPosture
    {
        Disallowed,
        Standing,
        Crouching,
    }
}

/// <summary>Exact `$0A7A` values used by the six-entry bank-$88 X-ray dispatcher.</summary>
public enum XrayBeamPhase : ushort
{
    NoBeam = 0,
    Widening = 1,
    Full = 2,
    RestoreFirstHalf = 3,
    RestoreSecondHalf = 4,
    Finish = 5,
}

/// <summary>One dedicated-input-handler call, suitable for debugger watches and assertions.</summary>
public readonly record struct XrayPoseInputResult(
    bool StartedTurn,
    bool CompletedTurn,
    byte Pose,
    SnesAngle Angle);

/// <summary>One bank-$88 beam-state call exposing the words consumed by the window renderer.</summary>
public readonly record struct XrayBeamStepResult(
    XrayBeamPhase PhaseAtStart,
    XrayBeamPhase PhaseAfterStep,
    byte SetupStage,
    SnesAngle AngleAtStart,
    SnesAngle AngleAfterStep,
    ushort WidthAtStart,
    ushort WidthAfterStep,
    bool Completed);
