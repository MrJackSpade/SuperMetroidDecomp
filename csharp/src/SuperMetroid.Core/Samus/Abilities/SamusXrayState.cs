using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// X-ray's Samus-side setup, dedicated pose-input handler,
/// movement handler, beam-angle state, visor palette, and teardown.
/// </summary>
/// <remarks>
/// The retail feature crosses three parts of the game: X-ray setup admits and installs the four X-ray
/// poses, the X-ray pose-input and movement handlers own turning and angle-selected body art, and
/// the cone effect widens/aims/deactivates the window beam. Keeping that state in one
/// explicit state prevents the standing and crouching X-ray poses from silently inheriting ordinary standing or
/// crouching input. The desktop compositor now consumes these exact angle and width values to
/// reproduce the moving tangent-table window; building the revealed blocks' picture remains
/// separate presentation work and no fake hidden tiles are generated here.
/// </remarks>
public sealed class SamusXrayState
{
    /// <summary>True while the dedicated X-ray input/movement handlers are installed.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// True while X-Ray owns Samus's input and movement handlers.
    /// The cone effect can remain active after automatic Reserve recovery restores the
    /// normal handler pair; that game state is the persistent greyout glitch.
    /// </summary>
    public bool OwnsSamusControl { get; private set; }

    /// <summary>
    /// True when Reserve recovery cleared the shared freeze flag underneath X-Ray's
    /// still-live phase-five cone effect. The effect deliberately cannot clean itself up.
    /// </summary>
    public bool IsReserveMode => IsActive && !OwnsSamusControl && !TimeIsFrozen;

    /// <summary>
    /// The four independent subsystems X-Ray setup disables. Unlike shared
    /// time freeze, automatic Reserve completion and a room transition do not restore
    /// these subsystems; that mismatch is the game's direct/indirect G-Mode state.
    /// </summary>
    public XraySuspendedSubsystems SuspendedSubsystems { get; internal set; }

    /// <summary>Whether X-ray's independent enemy-projectile disable remains set; gates projectile updates and drawing even if Reserve recovery has cleared shared time freeze.</summary>
    public bool AreEnemyProjectilesSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.EnemyProjectiles) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent room-object disable remains set, suppressing room-object program processing separately from shared time freeze.</summary>
    public bool AreObjectsSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.RoomObjects) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent animated-tile disable remains set, preventing animated-tile object advancement even after shared time resumes.</summary>
    public bool AreAnimatedTilesSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.AnimatedTiles) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent palette-FX disable remains set; this suspends the room's colour effects, not the separate colouring of Samus's visor.</summary>
    public bool ArePaletteFxSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.PaletteFx) != XraySuspendedSubsystems.None;

    /// <summary>Interrupted pose published by alpha, awaiting pose commit in beta.</summary>
    public SamusPoseId? PendingActivationPose { get; private set; }

    /// <summary>X-ray freezes enemy/projectile/room-object/animated-tile time.</summary>
    public bool TimeIsFrozen { get; private set; }

    /// <summary>
    /// The beam's warm-up: the updates of frozen time it spends after X-ray starts before its own step begins changing
    /// the beam's phase; over whenever no beam is warming up.
    /// </summary>
    public XrayWarmUp WarmUp { get; internal set; }

    /// <summary>Values zero through five index the beam dispatcher.</summary>
    public XrayBeamPhase BeamPhase { get; private set; }

    /// <summary>Right-facing angles occupy 0-127, left 128-255.</summary>
    public RotationAngle Angle { get; private set; }

    /// <summary>
    /// The beam's half-width as a 16.16 value in 256-per-turn angle units: zero at admission, grown by
    /// <see cref="AngularWideningSpeed"/> on each widening call, and clamped to exactly ten when its whole part
    /// would reach eleven. The widening adds move the fraction's overflow into the whole part exactly as the game does.
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
    /// The visor-colour sequence X-ray's suit colouring runs: widening at admission, full beam once the colouring
    /// sees the full-beam phase, and a request to restore the normal suit colours after teardown.
    /// </summary>
    public XrayVisorPaletteMode VisorPaletteMode { get; private set; }

    /// <summary>
    /// The special suit colouring X-ray runs: X-ray's once the visor command starts it, none after teardown
    /// restores the suit colours or another colouring takes over.
    /// </summary>
    public SamusSpecialPaletteType SpecialPaletteKind { get; private set; }

    /// <summary>
    /// The visor shade shown next, 0 through 5: widening shows 0, 1 and 2 and holds 2; the full beam cycles 3, 4
    /// and 5. Every advance is bounded by those comparisons.
    /// </summary>
    public int VisorShade { get; private set; }

    /// <summary>The shared five-frame palette cadence timer.</summary>
    public int CommonPaletteTimer { get; private set; }


    /// <summary>
    /// Runs the admission tests and pose selection of the X-ray setup.
    /// The caller represents the already selected/equipped HUD item; item ownership is
    /// checked by the HUD selector one layer above this.
    /// </summary>
    public bool TryBegin(SamusState samus, SamusMovementType previousMovementType, XrayBeginConditions conditions, XrayActivation activation = XrayActivation.Immediate)
    {
        Ensure.NotNull(samus);

        bool cancellingDirectGMode = IsReserveMode;
        if ((IsActive && !cancellingDirectGMode) || PendingActivationPose is not null)
            return false;

        // X-ray setup contains a very specific five-bomb/cooldown/divisor rejection.
        // Preserve the three-way conjunction; rejecting every nonzero cooldown would erase
        // a retail quirk, while ignoring it would permit the exact forbidden state.
        if (conditions.ProjectileCooldownTimer.IsAt(7) && conditions.BombCounter == 5 && samus.XSpeedDivisor == 2)
            return false;

        // Landing art (the normal and spin landings and the aimed and firing landings) is explicitly
        // excluded even though its movement type is otherwise standing.
        if (SamusPoseMetadata.IsLandingPose(samus.Pose))
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
        // observe a nonzero freeze flag. Its phase-five cleanup immediately
        // restores the four suspended subsystems. Represent the two cone effects
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
        // The cone setup runs inside the X-ray setup itself, before Samus's pose update
        // arbitrates the special command that installs X-Ray's Samus handlers. If a
        // same-frame hurt expiry wins that later arbitration, the beam effect's lifecycle,
        // frozen-time flag, and four subsystem disables still remain live: X-Mode.
        Angle = facingLeft ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularWideningSpeed = SignedSixteenSixteen.Zero;
        VisorPaletteMode = XrayVisorPaletteMode.Widening;
        WarmUp = XrayWarmUp.Started;
        BeamPhase = XrayBeamPhase.NoBeam;
        TimeIsFrozen = true;
        SuspendedSubsystems = XraySuspendedSubsystems.All;
        IsActive = true;
        OwnsSamusControl = false;
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
        // delay with frame two and a 63-tick timer, installs the dedicated handler pair, starts the
        // visor palette, clears beam-flare state, and queues activation sound nine.
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 2, timer: 63);
        SpecialPaletteKind = SamusSpecialPaletteType.Xray;
        samus.Shinespark.RelinquishPaletteToXray();
        samus.Shinespark.RelinquishMovementHandler();
        // The X-ray command installs both X-Ray handlers, so an interrupted Flash's no-op
        // pose-input handler is gone as well as its movement handler. Keeping the host lock
        // would make Samus immobile again when X-Ray teardown restores ordinary input.
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        // The same step takes over Samus's movement and suit colouring and clears
        // the special-palette timer even when an interrupted Flash, not a normal charge, owns it.
        samus.CrystalFlash.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishPaletteHandler();
        VisorShade = 0;
        CommonPaletteTimer = 1;
        samus.AudioRequests.QueueSound(SamusSound.XrayStart, SoundRequestPriority.Normal);
        OwnsSamusControl = true;
    }

    /// <summary>
    /// Models the Samus command that replaces the X-Ray handler pair when automatic
    /// Reserve recovery begins. The independent cone effect and shared freeze flag remain.
    /// </summary>
    internal void RelinquishSamusControlForReserveRecovery()
    {
        if (IsActive)
            OwnsSamusControl = false;
    }

    /// <summary>
    /// Models the shared freeze flag being set when automatic
    /// Reserve recovery begins. A stranded phase-five X-Ray object from Reserve Mode sees
    /// this second trigger and can execute its forced-stand cleanup on the following frame.
    /// </summary>
    internal void FreezeSharedTimeForReserveRecovery()
    {
        if (IsActive)
            TimeIsFrozen = true;
    }

    /// <summary>
    /// Models the Reserve recovery state clearing the shared freeze flag before restoring ordinary
    /// Samus handlers. X-Ray phase five observes zero and therefore leaves its cone effect
    /// installed, producing Reserve Mode/greyout rather than normal X-Ray teardown.
    /// </summary>
    internal void ClearSharedFreezeForReserveMode()
    {
        if (IsActive)
            TimeIsFrozen = false;
    }

    /// <summary>
    /// Models the door-transition cleanup which removes direct G-Mode's stranded X-Ray
    /// cone effect without restoring any of the four suspended subsystems.
    /// </summary>
    internal void TransitionDirectGModeToIndirect()
    {
        if (!IsReserveMode)
            return;

        IsActive = false;
        OwnsSamusControl = false;
        WarmUp = XrayWarmUp.Over;
        BeamPhase = XrayBeamPhase.NoBeam;
        Angle = RotationAngle.Zero;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularWideningSpeed = SignedSixteenSixteen.Zero;
        VisorPaletteMode = XrayVisorPaletteMode.Widening;
        SpecialPaletteKind = SamusSpecialPaletteType.None;
        VisorShade = 0;
        CommonPaletteTimer = 0;
    }

    /// <summary>Another special suit colouring takes over from X-ray's.</summary>
    internal void RelinquishPaletteHandler()
    {
        SpecialPaletteKind = SamusSpecialPaletteType.None;
        VisorShade = 0;
        CommonPaletteTimer = 0;
        VisorPaletteMode = XrayVisorPaletteMode.Widening;
    }

    /// <summary>
    /// Runs X-ray's pose input handler, including the exact
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

            // Mirroring through `256 - angle` turns right-space 0-127 into its exact
            // left-space counterpart and vice versa. This happens before selecting turn art.
            Angle = RotationAngle.Zero - Angle;
            bool crouching = movementType == SamusMovementType.Crouching;
            SamusPoseId targetPose = facingLeft
                ? crouching ? SamusPoseId.TurningLeftToRightCrouchingPose : SamusPoseId.TurningLeftToRightPose
                : crouching ? SamusPoseId.TurningRightToLeftCrouchingPose : SamusPoseId.TurningRightToLeftPose;
            ApplyXrayPoseChange(samus, targetPose);
            return new XrayPoseInputResult();
        }

        // The game waits until the final visible turning frame has exactly one tick left.
        // Its beta movement handler does nothing for a grounded turn, so generic animation owns this
        // countdown without X-ray's usual per-frame timer reset.
        if (samus.AnimationFrame != 2 || !samus.AnimationFrameTimer.IsAt(1))
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
    /// Executes the X-ray scope's movement: turning does nothing, while
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
            ? Angle < XrayScopeDefinitions.AnimationAngles.LeftFrameFour ? 4 :
              Angle < XrayScopeDefinitions.AnimationAngles.LeftFrameThree ? 3 :
              Angle < XrayScopeDefinitions.AnimationAngles.LeftFrameTwo ? 2 :
              Angle < XrayScopeDefinitions.AnimationAngles.LeftFrameOne ? 1 : 0
            : Angle < XrayScopeDefinitions.AnimationAngles.RightFrameOne ? 0 :
              Angle < XrayScopeDefinitions.AnimationAngles.RightFrameTwo ? 1 :
              Angle < XrayScopeDefinitions.AnimationAngles.RightFrameThree ? 2 :
              Angle < XrayScopeDefinitions.AnimationAngles.RightFrameFour ? 3 : 4;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame, timer: 15);
        return frame;
    }

    /// <summary>
    /// Advances the cone effect by one update: an update of the beam's warm-up, until that is over, and then the beam's own
    /// step. The reveal itself remains outside this movement state; the display reads the beam's angle and width only
    /// once the warm-up is over.
    /// </summary>
    public XrayBeamStepResult StepBeam(
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton dashBinding = ControllerButton.B)
    {
        Ensure.NotNull(samus);
        EnsureActive();

        XrayBeamPhase phaseAtStart = BeamPhase;

        if (!WarmUp.IsOver)
        {
            // The warm-up's last update hands over to the beam's own step, whose phase zero first runs on the
            // following update.
            WarmUp = WarmUp.Spend();
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

                // The 16.16 adds reproduce the game's fraction overflow exactly: the widening
                // speed gains the fixed fractional step per call, and both speed and width can
                // overflow into their whole parts before the ten-unit clamp.
                AngularWideningSpeed += XrayScopeDefinitions.Window.WideningStep;
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

                // Aiming gives Up priority when opposite directions are both held.
                if ((controllerInput & ControllerButton.Up) != ControllerButton.None)
                    MoveAngleUp();
                else if ((controllerInput & ControllerButton.Down) != ControllerButton.None)
                    MoveAngleDown();
                break;

            case XrayBeamPhase.RestoreFirstHalf:
                // The game's states three/four would wait while the artwork queue lacks room; the managed queue
                // always has room for these restorations, so each restore half completes in one call.
                BeamPhase = XrayBeamPhase.RestoreSecondHalf;
                break;

            case XrayBeamPhase.RestoreSecondHalf:
                BeamPhase = XrayBeamPhase.Finish;
                break;

            case XrayBeamPhase.Finish:
                // The phase-five cleanup performs every cleanup write only while the shared freeze
                // flag is set. Automatic Reserve completion clears that flag first;
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

    /// <summary>Runs the X-ray visor's colouring, which changes only the visor's shade until restoration.</summary>
    public bool UpdatePalette(
        SamusColours colours,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);

        if (VisorPaletteMode == XrayVisorPaletteMode.RestoreSuitPalette)
        {
            // A handler that reports the palette unhandled asks the outer dispatcher to load the
            // complete normal suit palette, then clears every X-ray palette value.
            SamusNormalSuitPalette.Load(colours, equippedItems);
            SpecialPaletteKind = SamusSpecialPaletteType.None;
            VisorShade = 0;
            CommonPaletteTimer = 0;
            VisorPaletteMode = XrayVisorPaletteMode.Widening;
            return true;
        }

        if (SpecialPaletteKind != SamusSpecialPaletteType.Xray)
            return false;

        if (VisorPaletteMode == XrayVisorPaletteMode.Widening && BeamPhase is not (XrayBeamPhase.NoBeam or XrayBeamPhase.Widening))
        {
            // The visor enters the full-beam three-color loop at its first shade.
            VisorShade = XrayScopeDefinitions.Palette.FullCycleFirstShade;
            CommonPaletteTimer = 1;
            VisorPaletteMode = XrayVisorPaletteMode.FullBeam;
        }

        if (--CommonPaletteTimer > 0)
            return false;

        CommonPaletteTimer = XrayScopeDefinitions.Palette.FrameDelay;
        colours.ShowVisor(SamusShades.Visor(VisorShade));

        if (VisorPaletteMode == XrayVisorPaletteMode.Widening)
        {
            // Widening shows shades 0,1,2 and then pins the last until state two is observed.
            if (VisorShade < XrayScopeDefinitions.Palette.WideningFinalShade)
                VisorShade++;
        }
        else
        {
            // Full beam cycles shades 3,4,5,3... with the comparison after increment.
            int next = VisorShade + 1;
            VisorShade = next < XrayScopeDefinitions.Palette.FullCycleEndShade
                ? next
                : XrayScopeDefinitions.Palette.FullCycleFirstShade;
        }

        return true;
    }

    /// <summary>
    /// The whole steps of the beam's centre, from which its edges lie <see cref="AngularWidth"/> steps either side. The
    /// clamps compare those edges unwrapped, so an edge may lie below zero or past a full turn.
    /// </summary>
    private int CenterSteps => Angle.ToTrigonometryAngle().RawValue;

    /// <summary>The whole steps of straight down, which the beam's lower or upper edge may not cross.</summary>
    private const int HalfTurnSteps = RotationAngle.StepsPerTurn / 2;

    /// <summary>Rotates the beam upward one angle unit while keeping its edge within the facing-dependent limit.</summary>
    private void MoveAngleUp()
    {
        if (Angle < RotationAngle.HalfTurn)
        {
            // Right-facing upper limit is `angle - width >= 0`. Equality returns without
            // movement; crossing clamps the center to exactly the current half-width.
            int candidate = CenterSteps - AngularWidth;
            if (candidate == 0)
                return;
            if (candidate < 0)
            {
                Angle = RotationAngle.FromSteps(AngularWidth);
                return;
            }

            Angle = Angle.TurnedBySteps(-1);
            if (CenterSteps - AngularWidth < 0)
                Angle = RotationAngle.FromSteps(AngularWidth);
            return;
        }

        // Left-facing up rotates toward 256; clamp the upper beam edge at exactly 256.
        int leftEdge = CenterSteps + AngularWidth;
        if (leftEdge == RotationAngle.StepsPerTurn)
            return;
        if (leftEdge > RotationAngle.StepsPerTurn)
        {
            Angle = RotationAngle.WrappedSteps(RotationAngle.StepsPerTurn - AngularWidth);
            return;
        }

        Angle = Angle.TurnedBySteps(1);
        if (CenterSteps + AngularWidth > RotationAngle.StepsPerTurn)
            Angle = RotationAngle.WrappedSteps(RotationAngle.StepsPerTurn - AngularWidth);
    }

    /// <summary>Rotates the beam downward one angle unit while keeping its edge on the correct side of straight down.</summary>
    private void MoveAngleDown()
    {
        if (Angle < RotationAngle.HalfTurn)
        {
            // Right-facing down rotates toward straight down; keep the lower edge at or below it.
            int lowerEdge = CenterSteps + AngularWidth;
            if (lowerEdge == HalfTurnSteps)
                return;
            if (lowerEdge > HalfTurnSteps)
            {
                Angle = RotationAngle.WrappedSteps(
                    HalfTurnSteps - AngularWidth);
                return;
            }

            Angle = Angle.TurnedBySteps(1);
            if (CenterSteps + AngularWidth > HalfTurnSteps)
                Angle = RotationAngle.WrappedSteps(
                    HalfTurnSteps - AngularWidth);
            return;
        }

        // Left-facing down rotates toward straight down from above and clamps its upper edge.
        int upperEdge = CenterSteps - AngularWidth;
        if (upperEdge == HalfTurnSteps)
            return;
        if (upperEdge < HalfTurnSteps)
        {
            Angle = RotationAngle.WrappedSteps(
                HalfTurnSteps + AngularWidth);
            return;
        }

        Angle = Angle.TurnedBySteps(-1);
        if (CenterSteps - AngularWidth < HalfTurnSteps)
            Angle = RotationAngle.WrappedSteps(
                HalfTurnSteps + AngularWidth);
    }

    /// <summary>Restores Samus' normal pose and handlers, subsystem enables, palette request, and beam state.</summary>
    private void Finish(SamusState samus)
    {
        // The X-ray teardown intentionally classifies a grounded turn as standing. Releasing
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
        // Teardown owns a direct history shift, including the crouched-
        // turn stand-up glitch. It does not consume an ordinary input slot.
        samus.CommitPoseHistory();
        if (radiusDifference >= 0)
            samus.Position -= new Offset16<RoomPixels>(0, radiusDifference);

        // The teardown restores both normal Samus handlers. X-Mode can arrange for
        // an active spark to own the movement handler while the old visor effect reaches this
        // teardown; replacing that spark while retaining the spark's palette timer is the Blue Suit result.
        samus.Shinespark.RelinquishMovementHandler();
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        TimeIsFrozen = false;
        SuspendedSubsystems = XraySuspendedSubsystems.None;
        IsActive = false;
        OwnsSamusControl = false;
        WarmUp = XrayWarmUp.Over;
        BeamPhase = XrayBeamPhase.NoBeam;
        Angle = RotationAngle.Zero;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularWideningSpeed = SignedSixteenSixteen.Zero;
        VisorPaletteMode = XrayVisorPaletteMode.RestoreSuitPalette;
        samus.AudioRequests.QueueSound(SamusSound.XrayEnd, SoundRequestPriority.Normal);
    }

    /// <summary>Commits a frozen X-ray turn pose with refreshed radii, animation, and history.</summary>
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
            SamusMovementType.CompactAirborne or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or
            SamusMovementType.Moonwalking or SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping or SamusMovementType.Grappling or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => XrayPosture.Disallowed,
        _ => throw new InvalidOperationException($"Undefined SamusMovementType {movementType}."),
    };

    /// <summary>Records the starting beam phase separately from whether this call removed the effect.</summary>
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
