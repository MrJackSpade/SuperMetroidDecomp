using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Abilities;

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
    /// <summary>Host-owned visor colors, excluded from debugger-state serialization.</summary>
    [field: NonSerialized]
    public SamusVisorColorCatalog? PresentationColors { get; set; }

    /// <summary>True while the dedicated bank-$91 X-ray input/movement handlers are installed.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// True while X-Ray owns Samus's bank-$91 input and bank-$90 movement handlers.
    /// The HDMA object can remain active after automatic Reserve recovery restores the
    /// normal handler pair; that cartridge state is the persistent greyout glitch.
    /// </summary>
    public bool OwnsSamusControl { get; private set; }

    /// <summary>
    /// True when Reserve recovery cleared the shared freeze word underneath X-Ray's
    /// still-live phase-five HDMA object. The object deliberately cannot clean itself up.
    /// </summary>
    public bool IsReserveMode => IsActive && !OwnsSamusControl && !TimeIsFrozen;

    /// <summary>
    /// The four independent native enable words disabled by X-Ray setup. Unlike shared
    /// time freeze, automatic Reserve completion and a room transition do not restore
    /// these words; that mismatch is the cartridge's direct/indirect G-Mode state.
    /// </summary>
    public XraySuspendedSubsystems SuspendedSubsystems { get; internal set; }

    /// <summary>Whether X-ray's independent enemy-projectile disable remains set; gates projectile updates and drawing even if Reserve recovery has cleared shared time freeze.</summary>
    public bool AreEnemyProjectilesSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.EnemyProjectiles) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent PLM disable remains set, suppressing room-object instruction processing separately from shared time freeze.</summary>
    public bool ArePlmsSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.Plms) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent animated-tile disable remains set, preventing animated-tile object advancement even after shared time resumes.</summary>
    public bool AreAnimatedTilesSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.AnimatedTiles) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent palette-FX disable remains set; this suspends room palette effects, not Samus's separate visor palette handler.</summary>
    public bool ArePaletteFxSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.PaletteFx) != XraySuspendedSubsystems.None;

    /// <summary>Interrupted pose published by alpha, awaiting bank-$91 pose commit in beta.</summary>
    public SamusPoseId? PendingActivationPose { get; private set; }

    /// <summary>WRAM `$0A78`; X-ray freezes enemy/projectile/PLM/animated-tile time.</summary>
    public bool TimeIsFrozen { get; private set; }

    /// <summary>
    /// The eight setup calls in `$91:D22E-$D26F` which prepare/transfer the two BG2 screens
    /// before the bank-$88 main pre-instruction begins changing the beam state.
    /// </summary>
    public XraySetupStage SetupStage { get; internal set; }

    /// <summary>WRAM `$0A7A`; values zero through five index `$88:8726`.</summary>
    public XrayBeamPhase BeamPhase { get; private set; }

    /// <summary>WRAM `$0A7E`; right-facing angles occupy `$00-$7F`, left `$80-$FF`.</summary>
    public RotationAngle Angle { get; private set; }

    /// <summary>
    /// The beam's half-width as a 16.16 value in 256-per-turn angle units: zero at admission, grown by
    /// <see cref="AngularWideningSpeed"/> on each widening call, and clamped to exactly ten when its whole part
    /// would reach eleven. The widening adds carry the fraction into the whole part exactly as the cartridge does.
    /// </summary>
    public SignedSixteenSixteen AngularHalfWidth { get; private set; }

    /// <summary>The whole angle units of <see cref="AngularHalfWidth"/>, 0 through 10, which aim clamps and the window read.</summary>
    public int AngularWidth => AngularHalfWidth.WholePixels;

    /// <summary>
    /// The 16.16 widening speed in angle units per call: zero at admission and raised by the fixed widening step
    /// before each widening call adds it to <see cref="AngularHalfWidth"/>.
    /// </summary>
    public SignedSixteenSixteen AngularWideningSpeed { get; private set; }

    /// <summary>
    /// The visor-colour sequence the palette handler runs: widening at admission, full beam once the palette
    /// handler sees the full-beam phase, and a request to restore the normal suit palette after teardown.
    /// </summary>
    public XrayVisorPaletteMode VisorPaletteMode { get; private set; }

    /// <summary>
    /// The special-palette handler this owner holds: X-ray once the visor command installs it, none after
    /// teardown restores the suit palette or another handler replaces it.
    /// </summary>
    public SamusSpecialPaletteType SpecialPaletteKind { get; private set; }

    /// <summary>
    /// Byte offset of the visor colour shown next, an even value from 0 through 10: widening shows 0, 2 and 4 and
    /// holds 4; the full beam cycles 6, 8 and 10. Every advance is bounded by those comparisons.
    /// </summary>
    public int SpecialPaletteFrame { get; private set; }

    /// <summary>WRAM `$0AD0`, the shared five-frame palette cadence timer.</summary>
    public WrappingCountdown CommonPaletteTimer { get; private set; }

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
    public bool TryBegin(SamusState samus, SamusMovementType previousMovementType, XrayBeginConditions conditions, XrayActivation activation = XrayActivation.Immediate)
    {
        Ensure.NotNull(samus);

        bool cancellingDirectGMode = IsReserveMode;
        if ((IsActive && !cancellingDirectGMode) || PendingActivationPose is not null)
            return false;

        // `$91:E173-$E189` contains a very specific five-bomb/cooldown/divisor rejection.
        // Preserve the three-way conjunction; rejecting every nonzero cooldown would erase
        // a retail quirk, while ignoring it would permit the exact forbidden state.
        if (conditions.ProjectileCooldownTimer.RawValue == 7 && conditions.BombCounter == 5 && samus.XSpeedDivisor == 2)
            return false;

        // Landing art `$A4-$A7/$E0-$E7` is explicitly excluded even though its movement
        // type is otherwise standing. The range comparisons are unsigned 16-bit compares.
        if (samus.Pose is >= SamusPoseId.NormalLandingRightPose and
            <= SamusPoseId.SpinLandingLeftPose or
            >= SamusPoseId.LandingAimUpRightPose and <= SamusPoseId.FiringLandingLeftPose)
            return false;

        if (conditions.GameState != SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay || conditions.PowerBombExplosionActive ||
            samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0)
        {
            return false;
        }

        SamusMovementType currentMovementType = SamusPoseMetadata.ReadMovementType(samus);
        if (ClassifyAllowedMovement(previousMovementType) == XrayPosture.Disallowed)
            return false;

        XrayPosture posture = ClassifyAllowedMovement(currentMovementType);
        if (posture == XrayPosture.Disallowed)
            return false;

        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPoseId targetPose = posture == XrayPosture.Crouching
            ? facingLeft ? SamusPoseId.XrayingCrouchingLeftPose : SamusPoseId.XrayingCrouchingRightPose
            : facingLeft ? SamusPoseId.XrayingStandingLeftPose : SamusPoseId.XrayingStandingRightPose;

        // A second X-Ray setup under direct G-Mode makes the stranded phase-five object
        // observe a nonzero freeze word. Its `$88:8A08 -> $91:E2AD` cleanup immediately
        // restores the four subsystem-enable words. Represent the two native HDMA objects
        // at their shared observable boundary instead of inventing gate-specific state.
        if (cancellingDirectGMode)
        {
            TimeIsFrozen = true;
            Finish(samus);
            return true;
        }
        if (activation == XrayActivation.Deferred)
        {
            BeginAdmittedBeam(facingLeft);
            PendingActivationPose = targetPose;
            return true;
        }
        BeginAdmittedBeam(facingLeft);
        InstallSamusControl(samus, targetPose);
        return true;
    }

    /// <summary>Commits the setup after movement/animation; higher-priority poses can supersede it.</summary>
    public bool CommitPendingActivation(SamusState samus, bool superseded)
    {
        if (PendingActivationPose is not { } targetPose) return false;
        PendingActivationPose = null;
        if (superseded) return false;
        InstallSamusControl(samus, targetPose);
        return true;
    }

    /// <summary>
    /// Activates X-Ray's beam, frozen-time state, and subsystem suspensions during setup,
    /// even if later pose arbitration denies X-Ray control of Samus.
    /// </summary>
    private void BeginAdmittedBeam(bool facingLeft)
    {
        // `$91:E217-$E28A` runs inside XraySetup itself, before UpdateSamusPose
        // arbitrates the special command that installs X-Ray's Samus handlers. If a
        // same-frame hurt expiry wins that later arbitration, the beam HDMA lifecycle,
        // frozen-time word, and four subsystem disables still remain live: X-Mode.
        Angle = facingLeft ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularWideningSpeed = SignedSixteenSixteen.Zero;
        VisorPaletteMode = XrayVisorPaletteMode.Widening;
        SetupStage = XraySetupStage.FreezeTimeBackupBg2Registers;
        BeamPhase = XrayBeamPhase.NoBeam;
        TimeIsFrozen = true;
        SuspendedSubsystems = XraySuspendedSubsystems.All;
        IsActive = true;
        OwnsSamusControl = false;
        ActivationSoundRequested = false;
        DeactivationSoundRequested = false;
    }

    /// <summary>
    /// Installs the X-Ray pose and animation, replaces competing movement and palette owners,
    /// and requests the visor activation sound.
    /// </summary>
    private void InstallSamusControl(SamusState samus, SamusPoseId targetPose)
    {
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);

        // Special prospective-pose command five immediately overrides the freshly selected
        // delay with frame two/timer `$3F`, installs the dedicated handler pair, starts the
        // visor palette, clears beam-flare state, and queues activation sound nine.
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 2, timer: 63);
        SpecialPaletteKind = SamusSpecialPaletteType.Xray;
        samus.Shinespark.RelinquishPaletteToXray();
        samus.Shinespark.RelinquishMovementHandler();
        // `$91:EEA6` installs both X-Ray handler pointers, so an interrupted Flash's RTS
        // pose-input pointer is gone as well as its movement pointer. Keeping the host lock
        // would make Samus immobile again when X-Ray teardown restores ordinary input.
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        // The same routine replaces the shared movement/palette handlers and clears
        // `$0A68` even when an interrupted Flash, not a normal charge, owns it.
        samus.CrystalFlash.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishPaletteHandler();
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = WrappingCountdown.Start(1);
        ActivationSoundRequested = true;
        OwnsSamusControl = true;
    }

    /// <summary>
    /// Models Samus command <c>$1B</c> replacing the X-Ray handler pair when automatic
    /// Reserve recovery begins. The independent HDMA object and shared freeze word remain.
    /// </summary>
    internal void RelinquishSamusControlForReserveRecovery()
    {
        if (IsActive)
            OwnsSamusControl = false;
    }

    /// <summary>
    /// Models <c>$82:DB77</c> setting the shared <c>$0A78</c> freeze word when automatic
    /// Reserve recovery begins. A stranded phase-five X-Ray object from Reserve Mode sees
    /// this second trigger and can execute its forced-stand cleanup on the following frame.
    /// </summary>
    internal void FreezeSharedTimeForReserveRecovery()
    {
        if (IsActive)
            TimeIsFrozen = true;
    }

    /// <summary>
    /// Models state <c>$1B</c> clearing shared WRAM <c>$0A78</c> before restoring ordinary
    /// Samus handlers. X-Ray phase five observes zero and therefore leaves its HDMA object
    /// installed, producing Reserve Mode/greyout rather than normal X-Ray teardown.
    /// </summary>
    internal void ClearSharedFreezeForReserveMode()
    {
        if (IsActive)
            TimeIsFrozen = false;
    }

    /// <summary>
    /// Models the door-transition cleanup which removes direct G-Mode's stranded X-Ray
    /// HDMA object without restoring any of the four independent subsystem-enable words.
    /// </summary>
    internal void TransitionDirectGModeToIndirect()
    {
        if (!IsReserveMode)
            return;

        IsActive = false;
        OwnsSamusControl = false;
        SetupStage = XraySetupStage.Complete;
        BeamPhase = XrayBeamPhase.NoBeam;
        Angle = RotationAngle.Zero;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularWideningSpeed = SignedSixteenSixteen.Zero;
        VisorPaletteMode = XrayVisorPaletteMode.Widening;
        SpecialPaletteKind = SamusSpecialPaletteType.None;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Another special-palette handler replaces shared WRAM <c>$0A68/$0ACE/$0AD0</c>.</summary>
    internal void RelinquishPaletteHandler()
    {
        SpecialPaletteKind = SamusSpecialPaletteType.None;
        SpecialPaletteFrame = 0;
        CommonPaletteTimer = WrappingCountdown.Start(0);
        VisorPaletteMode = XrayVisorPaletteMode.Widening;
    }

    /// <summary>
    /// Executes <c>XraySamusPoseInputHandler</c> at `$91:FCAF`, including the exact
    /// standing/crouching turn bodies and the frame-two/timer-one completion gate.
    /// </summary>
    public XrayPoseInputResult HandlePoseInput(
        SamusState samus,
        ControllerButton controllerInput)
    {
        Ensure.NotNull(samus);
        EnsureActive();

        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if (movementType != SamusMovementType.TurningOnGround)
        {
            bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
            ControllerButton turnBinding = facingLeft
                ? ControllerButton.Right
                : ControllerButton.Left;
            if ((controllerInput & turnBinding) == ControllerButton.None)
                return default;

            // Mirroring through `$0100 - angle` turns right-space `$00-$7F` into its exact
            // left-space counterpart and vice versa. This happens before selecting turn art.
            Angle = RotationAngle.NormalizeRaw(-Angle.RawValue);
            bool crouching = movementType == SamusMovementType.Crouching;
            SamusPoseId targetPose = facingLeft
                ? crouching ? SamusPoseId.TurningLeftToRightCrouchingPose : SamusPoseId.TurningLeftToRightPose
                : crouching ? SamusPoseId.TurningRightToLeftCrouchingPose : SamusPoseId.TurningRightToLeftPose;
            ApplyXrayPoseChange(samus, targetPose);
            return new XrayPoseInputResult();
        }

        // Native waits until the final visible turning frame has exactly one tick left.
        // Its beta movement handler is RTS for type `$0E`, so generic animation owns this
        // countdown without X-ray's usual per-frame timer reset.
        if (samus.AnimationFrame != 2 || samus.AnimationFrameTimer.RawValue != 1)
            return default;

        bool nowFacingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPoseId completedPose = nowFacingLeft
            ? samus.Pose == SamusPoseId.TurningRightToLeftPose
                ? SamusPoseId.XrayingStandingLeftPose
                : SamusPoseId.XrayingCrouchingLeftPose
            : samus.Pose == SamusPoseId.TurningLeftToRightPose
                ? SamusPoseId.XrayingStandingRightPose
                : SamusPoseId.XrayingCrouchingRightPose;
        ApplyXrayPoseChange(samus, completedPose);
        return new XrayPoseInputResult();
    }

    /// <summary>
    /// Executes <c>SamusMovementHandler_Xray</c> at `$90:E94F`: turning is an RTS, while
    /// stable X-ray poses force timer fifteen and one of five angle-selected art frames.
    /// </summary>
    public int? StepMovement(SamusState samus)
    {
        Ensure.NotNull(samus);
        EnsureActive();

        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.TurningOnGround)
            return null;

        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        int frame = facingLeft
            ? Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.LeftFrameFour ? 4 :
              Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.LeftFrameThree ? 3 :
              Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.LeftFrameTwo ? 2 :
              Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.LeftFrameOne ? 1 : 0
            : Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.RightFrameOne ? 0 :
              Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.RightFrameTwo ? 1 :
              Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.RightFrameThree ? 2 :
              Angle.TableIndex < XrayScopeDefinitions.AnimationAngles.RightFrameFour ? 3 : 4;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame, timer: 15);
        return frame;
    }

    /// <summary>
    /// Advances one bank-$88 HDMA-object call. The setup-stage count represents the eight
    /// real cartridge functions. Actual BG2 copies remain intentionally outside this movement
    /// state; the renderer consumes its angle/width only after setup has completed.
    /// </summary>
    public XrayBeamStepResult StepBeam(
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton dashBinding = ControllerButton.B)
    {
        Ensure.NotNull(samus);
        EnsureActive();

        XrayBeamPhase phaseAtStart = BeamPhase;

        if (SetupStage != XraySetupStage.Complete)
        {
            // The eight calls execute in order. After the eighth the instruction list
            // installs `$88:86EF`; beam state zero itself first runs on the following call.
            SetupStage = SetupStage.Advance();
            return SnapshotBeamStep(phaseAtStart, completed: false);
        }

        bool holdingDash = (controllerInput & dashBinding) != ControllerButton.None;
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

                // The 16.16 adds reproduce the cartridge's carry propagation exactly: the widening
                // speed gains the fixed fractional step per call, and both speed and width can
                // carry into their whole parts before the ten-unit clamp.
                AngularWideningSpeed += SignedSixteenSixteen.FromRaw(
                    checked((int)XrayScopeDefinitions.Window.WideningStep));
                AngularHalfWidth += AngularWideningSpeed;
                if (AngularHalfWidth.WholePixels >= 11)
                {
                    AngularHalfWidth = SignedSixteenSixteen.FromParts(10, 0);
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
                if ((controllerInput & ControllerButton.Up) != ControllerButton.None)
                    MoveAngleUp();
                else if ((controllerInput & ControllerButton.Down) != ControllerButton.None)
                    MoveAngleDown();
                break;

            case XrayBeamPhase.RestoreFirstHalf:
                // Native states three/four would wait while the seven-byte VRAM queue lacks room; the managed queue
                // always has room for these BG2 transfers, so each restore half completes in one call.
                BeamPhase = XrayBeamPhase.RestoreSecondHalf;
                break;

            case XrayBeamPhase.RestoreSecondHalf:
                BeamPhase = XrayBeamPhase.Finish;
                break;

            case XrayBeamPhase.Finish:
                // `$88:8A08` performs every cleanup write only while the shared freeze
                // word is nonzero. Automatic Reserve completion clears that word first;
                // phase five then remains installed forever and produces greyout mode.
                if (TimeIsFrozen)
                    Finish(samus);
                break;

            default:
                throw new InvalidOperationException($"Unknown active X-ray beam phase {BeamPhase}.");
        }

        return SnapshotBeamStep(
            phaseAtStart,
            completed: !IsActive);
    }

    /// <summary>Runs `$91:DCB4-$DD30` and writes only visor color four until restoration.</summary>
    public bool UpdatePalette(
        SamusColours colours,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        SamusSuitColorCatalog? suitColors = null)
    {
        Ensure.NotNull(colours);

        if (VisorPaletteMode == XrayVisorPaletteMode.RestoreSuitPalette)
        {
            // Carry clear from the native handler asks the outer dispatcher to load the
            // complete normal suit palette, then clears every X-ray palette word.
            SamusNormalSuitPalette.Load(colours.Suit, equippedItems, suitColors);
            SpecialPaletteKind = SamusSpecialPaletteType.None;
            SpecialPaletteFrame = 0;
            CommonPaletteTimer = WrappingCountdown.Start(0);
            VisorPaletteMode = XrayVisorPaletteMode.Widening;
            return true;
        }

        if (SpecialPaletteKind != SamusSpecialPaletteType.Xray)
            return false;

        if (VisorPaletteMode == XrayVisorPaletteMode.Widening && BeamPhase is not (XrayBeamPhase.NoBeam or XrayBeamPhase.Widening))
        {
            // `$91:DCEA` enters the full-beam three-color loop at byte offset six.
            SpecialPaletteFrame = XrayScopeDefinitions.Palette.FullCycleFirstWordOffset;
            CommonPaletteTimer = WrappingCountdown.Start(1);
            VisorPaletteMode = XrayVisorPaletteMode.FullBeam;
        }

        CountdownStep timer = CommonPaletteTimer.Decrement();
        CommonPaletteTimer = timer.Remaining;
        if (!timer.IsZero && timer.IsNonNegative)
            return false;

        CommonPaletteTimer = WrappingCountdown.Start(XrayScopeDefinitions.Palette.FrameDelay);
        // Keep the cartridge read for non-catalog offsets reachable through externally
        // edited debugger state; ordinary widening/cycling uses installed artwork.
        if (PresentationColors is null ||
            !PresentationColors.TryResolveByteOffset(SpecialPaletteFrame, out RgbColor visorColor))
            throw new InvalidDataException(
                $"X-ray visor offset {SpecialPaletteFrame} has no installed color.");
        colours.Visor.Write(0, visorColor);

        if (VisorPaletteMode == XrayVisorPaletteMode.Widening)
        {
            // Widening uses offsets 0,2,4 and then pins four until state two is observed.
            if (SpecialPaletteFrame < XrayScopeDefinitions.Palette.WideningFinalWordOffset)
                SpecialPaletteFrame += sizeof(ushort);
        }
        else
        {
            // Full beam cycles offsets 6,8,10,6... with the comparison after increment.
            int next = SpecialPaletteFrame + sizeof(ushort);
            SpecialPaletteFrame = next < XrayScopeDefinitions.Palette.FullCycleEndWordOffset
                ? next
                : XrayScopeDefinitions.Palette.FullCycleFirstWordOffset;
        }

        return true;
    }

    /// <summary>Rotates the beam upward one angle unit while keeping its edge within the facing-dependent limit.</summary>
    private void MoveAngleUp()
    {
        if (Angle.RawValue < RotationAngle.HalfTurn.RawValue)
        {
            // Right-facing upper limit is `angle - width >= 0`. Equality returns without
            // movement; crossing clamps the center to exactly the current half-width.
            int candidate = Angle.TableIndex - AngularWidth;
            if (candidate == 0)
                return;
            if (candidate < 0)
            {
                Angle = RotationAngle.FromTableIndex(checked((byte)AngularWidth));
                return;
            }

            Angle = Angle.AddTableUnits(-1);
            if (Angle.TableIndex - AngularWidth < 0)
                Angle = RotationAngle.FromTableIndex(checked((byte)AngularWidth));
            return;
        }

        // Left-facing up rotates toward `$100`; clamp the upper beam edge at exactly 256.
        int leftEdge = Angle.TableIndex + AngularWidth;
        if (leftEdge == RotationAngle.TableUnitsPerTurn)
            return;
        if (leftEdge > RotationAngle.TableUnitsPerTurn)
        {
            Angle = RotationAngle.NormalizeTableIndex(RotationAngle.TableUnitsPerTurn - AngularWidth);
            return;
        }

        Angle = Angle.AddTableUnits(1);
        if (Angle.TableIndex + AngularWidth > RotationAngle.TableUnitsPerTurn)
            Angle = RotationAngle.NormalizeTableIndex(RotationAngle.TableUnitsPerTurn - AngularWidth);
    }

    /// <summary>Rotates the beam downward one angle unit while keeping its edge on the correct side of $80.</summary>
    private void MoveAngleDown()
    {
        if (Angle.RawValue < RotationAngle.HalfTurn.RawValue)
        {
            // Right-facing down rotates toward `$80`; keep the lower edge at or below it.
            int lowerEdge = Angle.TableIndex + AngularWidth;
            if (lowerEdge == RotationAngle.HalfTurn.TableIndex)
                return;
            if (lowerEdge > RotationAngle.HalfTurn.TableIndex)
            {
                Angle = RotationAngle.NormalizeTableIndex(
                    RotationAngle.HalfTurn.TableIndex - AngularWidth);
                return;
            }

            Angle = Angle.AddTableUnits(1);
            if (Angle.TableIndex + AngularWidth > RotationAngle.HalfTurn.TableIndex)
                Angle = RotationAngle.NormalizeTableIndex(
                    RotationAngle.HalfTurn.TableIndex - AngularWidth);
            return;
        }

        // Left-facing down rotates toward `$80` from above and clamps its upper edge.
        int upperEdge = Angle.TableIndex - AngularWidth;
        if (upperEdge == RotationAngle.HalfTurn.TableIndex)
            return;
        if (upperEdge < RotationAngle.HalfTurn.TableIndex)
        {
            Angle = RotationAngle.NormalizeTableIndex(
                RotationAngle.HalfTurn.TableIndex + AngularWidth);
            return;
        }

        Angle = Angle.AddTableUnits(-1);
        if (Angle.TableIndex - AngularWidth < RotationAngle.HalfTurn.TableIndex)
            Angle = RotationAngle.NormalizeTableIndex(
                RotationAngle.HalfTurn.TableIndex + AngularWidth);
    }

    /// <summary>Restores Samus' normal pose and handlers, subsystem enables, palette request, and beam state.</summary>
    private void Finish(SamusState samus)
    {
        // `$91:E2AD` intentionally classifies turning type `$0E` as standing. Releasing
        // X-ray during a crouched turn therefore stands Samus up—the documented retail
        // X-ray stand-up glitch—and the radius difference moves her center upward.
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        bool crouching = movementType == SamusMovementType.Crouching;
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        int oldRadius = samus.Kinematics.Radius.Y;
        SamusPoseId targetPose = crouching
            ? facingLeft ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose
            : facingLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose;

        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        int radiusDifference = samus.Kinematics.Radius.Y - oldRadius;
        // Teardown owns a direct native history shift, including the crouched-
        // turn stand-up glitch. It does not consume an ordinary input slot.
        samus.CommitPoseHistory();
        if (radiusDifference >= 0)
            samus.Position -= new Offset16<RoomPixels>(0, unchecked((short)radiusDifference));

        // `$91:E2AD` restores both normal Samus handler pointers. X-Mode can arrange for
        // an active spark to own the movement word while the old visor HDMA reaches this
        // teardown; replacing that spark while retaining `$0B3E` is the Blue Suit result.
        samus.Shinespark.RelinquishMovementHandler();
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        TimeIsFrozen = false;
        SuspendedSubsystems = XraySuspendedSubsystems.None;
        IsActive = false;
        OwnsSamusControl = false;
        SetupStage = XraySetupStage.Complete;
        BeamPhase = XrayBeamPhase.NoBeam;
        Angle = RotationAngle.Zero;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularWideningSpeed = SignedSixteenSixteen.Zero;
        VisorPaletteMode = XrayVisorPaletteMode.RestoreSuitPalette;
        DeactivationSoundRequested = true;
    }

    /// <summary>Commits a frozen X-ray turn pose with refreshed radii, animation, and native history.</summary>
    private static void ApplyXrayPoseChange(
        SamusState samus,
        SamusPoseId targetPose)
    {
        samus.Pose = targetPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Both branches of the frozen X-ray input handler publish history after
        // initializing their pose; ordinary gameplay transition dispatch is bypassed.
        samus.CommitPoseHistory();
    }

    /// <summary>Admits standing-like and crouching movement types to different visor poses.</summary>
    private static XrayPosture ClassifyAllowedMovement(SamusMovementType movementType) => movementType switch
    {
        SamusMovementType.Standing or
            SamusMovementType.Running or
            SamusMovementType.RanIntoWall => XrayPosture.Standing,
        SamusMovementType.Crouching => XrayPosture.Crouching,
        SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
            SamusMovementType.MorphBallGround or SamusMovementType.Falling or
            SamusMovementType.UnusedGlitchBall or SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or SamusMovementType.Knockback or
            SamusMovementType.Unused0B or SamusMovementType.Unused0C or SamusMovementType.Unused0D or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or
            SamusMovementType.Moonwalking or SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping or SamusMovementType.Grappling or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => XrayPosture.Disallowed,
        _ => throw new InvalidOperationException($"Undefined SamusMovementType {movementType}."),
    };

    /// <summary>Records the starting HDMA phase separately from whether this call removed the object.</summary>
    private static XrayBeamStepResult SnapshotBeamStep(
        XrayBeamPhase phaseAtStart,
        bool completed) => new(
            phaseAtStart,
            completed);

    /// <summary>Rejects movement or pose input after the X-ray handler has been removed.</summary>
    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("X-ray has no installed Samus handler.");
    }

    /// <summary>Posture categories used by visor admission and pose choice.</summary>
    private enum XrayPosture
    {
        /// <summary>Movement type outside the visor's standing and crouching entry paths.</summary>
        Disallowed,
        /// <summary>Uses the standing right or left visor pose.</summary>
        Standing,
        /// <summary>Uses the crouching right or left visor pose.</summary>
        Crouching,
    }
}
