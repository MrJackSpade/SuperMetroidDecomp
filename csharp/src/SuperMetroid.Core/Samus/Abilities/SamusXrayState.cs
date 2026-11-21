using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// X-ray's Samus-side setup, its own pose input and its hold on her movement
/// (<see cref="SamusMovementOwner.Xray"/>), beam-angle state, visor shade, and teardown.
/// </summary>
/// <remarks>
/// The scope crosses three parts of the game: X-ray setup admits and installs the four X-ray
/// poses, X-ray's own pose input and movement own turning and angle-selected body art, and
/// the cone effect widens, aims and ends the beam. Keeping that state in one
/// explicit state prevents the standing and crouching X-ray poses from silently inheriting ordinary standing or
/// crouching input. The cone's angle and width reach the display through <see cref="SamusXrayDisplaySnapshot"/>, from
/// which the display draws the cone and the blocks it reveals.
/// </remarks>
public sealed class SamusXrayState
{
    /// <summary>The special suit colouring holding Samus's suit, which the X-ray visor takes.</summary>
    private readonly SamusSpecialColouring _colouring;

    /// <summary>Creates an X-ray owner whose visor colours take Samus's special suit colouring.</summary>
    /// <param name="colouring">Samus's special suit colouring, which the visor takes.</param>
    internal SamusXrayState(SamusSpecialColouring colouring)
    {
        _colouring = Ensure.NotNull(colouring);
        _scope = new LiveScope(this);
        _control = new SamusControl(this);
    }

    /// <summary>The live scope's steps, reached through <see cref="Scope"/>.</summary>
    private readonly LiveScope _scope;

    /// <summary>The X-ray's hold on Samus's control, reached through <see cref="Control"/>.</summary>
    private readonly SamusControl _control;

    /// <summary>The live scope, whose beam steps: from its activation until its teardown, including Reserve Mode; none while it is off.</summary>
    internal LiveScope? Scope => IsActive ? _scope : null;

    /// <summary>The X-ray's hold on Samus's pose input and movement while it has them (<see cref="OwnsSamusControl"/>); none otherwise.</summary>
    internal SamusControl? Control => OwnsSamusControl ? _control : null;

    /// <summary>True while the X-ray scope is live: from its activation until its teardown, including Reserve Mode.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// True while X-Ray owns Samus's pose input and her movement.
    /// The cone effect can remain active after automatic Reserve recovery restores the
    /// normal input and movement modes; that game state is the persistent greyout glitch.
    /// </summary>
    public bool OwnsSamusControl { get; private set; }

    /// <summary>
    /// True when Reserve recovery let time run under X-Ray's still-live finishing beam, which then cannot end itself.
    /// </summary>
    private bool IsReserveMode => IsActive && !OwnsSamusControl && !TimeIsFrozen;

    /// <summary>
    /// The four independent subsystems X-Ray setup disables. Unlike shared
    /// time freeze, automatic Reserve completion and a room transition do not restore
    /// these subsystems; that mismatch is the game's direct/indirect G-Mode state.
    /// </summary>
    private XraySuspendedSubsystems SuspendedSubsystems { get; set; }

    /// <summary>Whether X-ray's independent enemy-projectile disable remains set; gates projectile updates and drawing even if Reserve recovery has cleared shared time freeze.</summary>
    public bool AreEnemyProjectilesSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.EnemyProjectiles) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent room-object disable remains set, suppressing room-object animation processing separately from shared time freeze.</summary>
    public bool AreObjectsSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.RoomObjects) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent block-animation disable remains set, preventing block animations from advancing even after shared time resumes.</summary>
    public bool AreBlockAnimationsSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.BlockAnimations) != XraySuspendedSubsystems.None;

    /// <summary>Whether X-ray's independent colour effect disable remains set; this suspends the room's colour effects, not the separate colouring of Samus's visor.</summary>
    public bool AreColourEffectsSuspended =>
        (SuspendedSubsystems & XraySuspendedSubsystems.ColourEffects) != XraySuspendedSubsystems.None;

    /// <summary>X-ray freezes enemy/projectile/room-object/block-animation time.</summary>
    public bool TimeIsFrozen { get; private set; }

    /// <summary>
    /// The beam's warm-up: the updates of frozen time it spends after X-ray starts before its own step begins changing
    /// the beam's phase; over whenever no beam is warming up.
    /// </summary>
    public XrayWarmUp WarmUp { get; private set; }

    /// <summary>The beam's phase once its warm-up is over.</summary>
    public XrayBeamPhase BeamPhase { get; private set; }

    /// <summary>
    /// The beam's closing: the updates of frozen time it spends after Run's release before its last step releases Samus;
    /// over whenever the beam is not closing.
    /// </summary>
    public XrayClosing Closing { get; private set; }

    /// <summary>Right-facing aims lie in the turn's first half and left-facing ones in its second
    /// (<see cref="RotationAngle.HalfTurn"/>).</summary>
    public RotationAngle Angle { get; private set; }

    /// <summary>
    /// The angle from the beam's centre to each of its edges, with its fraction of a step: zero at admission, grown by
    /// <see cref="AngularWideningSpeed"/> on each widening update, and clamped to exactly
    /// <see cref="XrayScopeDefinitions.Cone.WidestHalfWidth"/> once it would reach a step beyond.
    /// </summary>
    private RotationAngle AngularHalfWidth { get; set; }

    /// <summary>The whole steps of <see cref="AngularHalfWidth"/>, up to <see cref="XrayScopeDefinitions.Cone.WidestHalfWidth"/>, which aim clamps and the cone read.</summary>
    public TrigonometryAngle AngularWidth => AngularHalfWidth.ToTrigonometryAngle();

    /// <summary>
    /// The widening speed, the turn the half-width grows by each update: zero at admission and raised by the fixed
    /// widening step before each widening update adds it to <see cref="AngularHalfWidth"/>.
    /// </summary>
    private RotationAngle AngularWideningSpeed { get; set; }

    /// <summary>
    /// The sequence of colours X-ray's suit colouring shows on the visor: widening at admission, full beam once the colouring
    /// sees the full-beam phase, and a request to restore the normal suit colours after teardown.
    /// </summary>
    private XrayVisorShadeMode VisorShadeMode { get; set; }

    /// <summary>
    /// The visor's shade sequence for <see cref="VisorShadeMode"/> (#2285, #2387), each shade shown for the same colouring
    /// updates: widening shows the widening glow's shades and rests on its widest, showing it again each time it runs out;
    /// the full beam cycles the steady visor cycle's. Its first shade shows on the colouring update after it starts.
    /// </summary>
    private AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> Visor { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.Of(SamusGlow.XrayVisorWidening));

/// <summary>
    /// Runs the admission tests and pose selection of the X-ray setup.
    /// The X-ray scope is already the selected and equipped HUD item; item ownership is
    /// checked by the HUD selector one layer above this. An admitted beam begins at once; its pose and Samus's control
    /// wait for <see cref="CommitActivation"/>, which the frame's pose update calls with the pose this setup returns once
    /// higher-priority transitions have had their turn (#2562).
    /// </summary>
    /// <param name="samus">Samus.</param>
    /// <param name="previousMovementType">The movement type of her previous update.</param>
    /// <param name="conditions">What else the setup reads of the update.</param>
    /// <param name="activationPose">
    /// The pose the admitted setup installs as it activates, which the frame's pose update commits; null when the setup
    /// refuses, or when it ends a stranded beam at once.
    /// </param>
    public bool TryBegin(SamusState samus, SamusMovementType previousMovementType, XrayBeginConditions conditions,
        out SamusPoseId? activationPose)
    {
        Ensure.NotNull(samus);
        activationPose = null;

        bool cancellingDirectGMode = IsReserveMode;
        if (IsActive && !cancellingDirectGMode)
            return false;

        if (XrayRefusedAsDrained.Refuses(conditions.ProjectileCooldownTimer, conditions.BombCounter, samus.SlowdownLevel))
            return false;

        // Landing art (the normal and spin landings and the aimed and firing landings) is explicitly
        // excluded even though its movement type is otherwise standing.
        if (SamusPoseMetadata.IsLandingPose(samus.Pose))
            return false;

        if (conditions.GameState != SuperMetroidGameState.MainGameplay || conditions.PowerBombExplosionActive ||
            samus.Kinematics.VerticalVelocity.IsMoving)
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

        // A second X-Ray setup under direct G-Mode makes the stranded finishing beam find time frozen, and its last step
        // at once restores the four suspended subsystems: on this update time freezes and the beam finishes together.
        if (cancellingDirectGMode)
        {
            TimeIsFrozen = true;
            Finish(samus);
            return true;
        }
        BeginAdmittedBeam(facingLeft);
        activationPose = targetPose;
        return true;
    }

    /// <summary>
    /// Commits the setup's <paramref name="activationPose"/> after movement and animation; a higher-priority pose
    /// supersedes it.
    /// </summary>
    public bool CommitActivation(SamusState samus, SamusPoseId activationPose, bool superseded)
    {
        if (superseded) return false;
        InstallSamusControl(samus, activationPose);
        return true;
    }

    /// <summary>
    /// Activates X-Ray's beam, frozen-time state, and subsystem suspensions during setup,
    /// even if later pose arbitration denies X-Ray control of Samus.
    /// </summary>
    private void BeginAdmittedBeam(bool facingLeft)
    {
        // The cone setup runs inside the X-ray setup itself, before Samus's pose update
        // arbitrates X-ray's activation, which gives X-Ray Samus's pose input and movement. If a
        // same-frame hurt expiry wins that later arbitration, the beam effect's lifecycle,
        // frozen-time flag, and four subsystem disables still remain live: X-Mode.
        Angle = facingLeft ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn;
        AngularHalfWidth = RotationAngle.Zero;
        AngularWideningSpeed = RotationAngle.Zero;
        VisorShadeMode = XrayVisorShadeMode.Widening;
        WarmUp = XrayWarmUp.Started;
        BeamPhase = XrayBeamPhase.NoBeam;
        Closing = XrayClosing.Over;
        TimeIsFrozen = true;
        SuspendedSubsystems = XraySuspendedSubsystems.All;
        IsActive = true;
        OwnsSamusControl = false;
    }

    /// <summary>
    /// Gives Samus the X-Ray pose and animation, replaces competing movement and colouring owners,
    /// and requests the visor activation sound.
    /// </summary>
    private void InstallSamusControl(SamusState samus, SamusPoseId targetPose)
    {
        samus.ChangePose(targetPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);

        // X-ray's activation pose immediately overrides the freshly selected
        // delay with its level aim and the activation's hold, takes her pose input and movement, starts the
        // visor shade, clears beam-flare state, and plays the X-ray's starting sound.
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationFrames.XrayActivation, timer: XrayScopeDefinitions.Holds.Activation);
        _colouring.Take(SamusSpecialColouringKind.Xray);
        samus.Shinespark.RelinquishColoursToXray();
        // X-ray's activation takes both Samus's pose input and her movement, so an interrupted Flash's
        // lock on her pose input is gone as well as its hold on her movement. Keeping the Crystal Flash input lock
        // would make Samus immobile again when X-Ray teardown restores ordinary input.
        SamusMovementHandOver.ToXray(samus);
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        // The same step takes over Samus's suit colouring and clears
        // the special colouring timer even when an interrupted Flash, not a normal charge, owns it.
        samus.CrystalFlash.RelinquishSuitColouring();
        SamusGlowPlay.Restart(Visor, SamusGlowSegment.Of(SamusGlow.XrayVisorWidening));
        samus.AudioRequests.QueueSound(SamusSound.XrayStart, SoundRequestPriority.Normal);
        OwnsSamusControl = true;
    }

    /// <summary>
    /// Automatic reserve recovery takes Samus's pose input and movement from X-Ray; the cone and the frozen time stay.
    /// </summary>
    internal void RelinquishSamusControlForReserveRecovery()
    {
        if (IsActive)
            OwnsSamusControl = false;
    }

    /// <summary>
    /// Automatic reserve recovery freezes time, which lets a stranded finishing X-Ray beam from Reserve Mode finish its
    /// forced stand on the next update.
    /// </summary>
    internal void FreezeSharedTimeForReserveRecovery()
    {
        if (IsActive)
            TimeIsFrozen = true;
    }

    /// <summary>
    /// Reserve recovery lets time run again before returning Samus's pose input and movement to her pose; a finishing
    /// X-Ray beam then finds time running and leaves its cone in place, so Reserve Mode greys the screen rather than
    /// X-Ray ending.
    /// </summary>
    internal void ClearSharedFreezeForReserveMode()
    {
        if (IsActive)
            TimeIsFrozen = false;
    }

    /// <summary>
    /// A door transition removes direct G-Mode's stranded X-Ray cone without restoring any of the four suspended
    /// subsystems.
    /// </summary>
    internal void TransitionDirectGModeToIndirect()
    {
        if (!IsReserveMode)
            return;

        IsActive = false;
        OwnsSamusControl = false;
        WarmUp = XrayWarmUp.Over;
        BeamPhase = XrayBeamPhase.NoBeam;
        Closing = XrayClosing.Over;
        Angle = RotationAngle.Zero;
        AngularHalfWidth = RotationAngle.Zero;
        AngularWideningSpeed = RotationAngle.Zero;
        VisorShadeMode = XrayVisorShadeMode.Widening;
        _colouring.Release(SamusSpecialColouringKind.Xray);
        SamusGlowPlay.Restart(Visor, SamusGlowSegment.Of(SamusGlow.XrayVisorWidening));
    }

    /// <summary>Another special suit colouring takes over from X-ray's.</summary>
    internal void RelinquishSuitColouring()
    {
        SamusGlowPlay.Restart(Visor, SamusGlowSegment.Of(SamusGlow.XrayVisorWidening));
        VisorShadeMode = XrayVisorShadeMode.Widening;
    }

    /// <summary>
    /// Runs X-ray's own pose input, including the exact standing and crouching turns and their completion on the
    /// update the turn's last frame has one update left to show.
    /// </summary>
    private void HandlePoseInput(
        SamusState samus,
        GameplayAction controllerInput)
    {
        Ensure.NotNull(samus);

        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if (movementType != SamusMovementType.TurningOnGround)
        {
            bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
            GameplayAction turnBinding = facingLeft
                ? GameplayAction.Right
                : GameplayAction.Left;
            if ((controllerInput & turnBinding) == GameplayAction.None)
                return;

            // The turn mirrors the aim across the vertical, before the turn's picture is chosen.
            Angle = RotationAngle.Zero - Angle;
            bool crouching = movementType == SamusMovementType.Crouching;
            SamusPoseId targetPose = facingLeft
                ? crouching ? SamusPoseId.TurningLeftToRightCrouchingPose : SamusPoseId.TurningLeftToRightPose
                : crouching ? SamusPoseId.TurningRightToLeftCrouchingPose : SamusPoseId.TurningRightToLeftPose;
            ApplyXrayPoseChange(samus, targetPose);
            return;
        }

        // The game waits until the final visible turning frame has exactly one tick left.
        // Its movement does nothing for a grounded turn, so generic animation owns this
        // countdown without X-ray's usual per-frame timer reset.
        if (!samus.AnimationIsAt(SamusAnimationFrames.TurnEnd) || !samus.AnimationFrameTimer.IsAt(1))
            return;

        bool nowFacingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        SamusPoseId completedPose = nowFacingLeft
            ? samus.Pose == SamusPoseId.TurningRightToLeftPose
                ? SamusPoseId.XrayingStandingLeftPose
                : SamusPoseId.XrayingCrouchingLeftPose
            : samus.Pose == SamusPoseId.TurningLeftToRightPose
                ? SamusPoseId.XrayingStandingRightPose
                : SamusPoseId.XrayingCrouchingRightPose;
        ApplyXrayPoseChange(samus, completedPose);
    }

    /// <summary>
    /// Executes the X-ray scope's movement: turning does nothing, while
    /// stable X-ray poses force the aim's hold and one of the five aims the angle selects.
    /// </summary>
    private void StepMovement(SamusState samus)
    {
        Ensure.NotNull(samus);

        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.TurningOnGround)
            return;

        // Any pose not facing left aims as a right-facing scope does.
        SamusFacingDirection facing = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? SamusFacingDirection.Left : SamusFacingDirection.Right;
        SamusAnimationFrame aim = XrayScopeDefinitions.AimBands[facing].AimAt(Angle);
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, aim, timer: XrayScopeDefinitions.Holds.Aim);
    }

    /// <summary>
    /// Advances the cone effect by one update: an update of the beam's warm-up, until that is over, and then the beam's own
    /// step. The reveal itself remains outside this movement state; the display reads the beam's angle and width only
    /// once the warm-up is over.
    /// </summary>
    private XrayBeamStepResult StepBeam(
        SamusState samus,
        GameplayAction controllerInput)
    {
        Ensure.NotNull(samus);

        XrayBeamPhase phaseAtStart = BeamPhase;

        if (!WarmUp.IsOver)
        {
            // The warm-up's last update hands over to the beam's own step, whose no-beam phase first runs on the
            // following update.
            WarmUp = WarmUp.Spend();
            return SnapshotBeamStep(phaseAtStart, completed: false);
        }

        bool holdingDash = (controllerInput & GameplayAction.Dash) != GameplayAction.None;
        StepBeamPhase();

        void StepBeamPhase()
        {
            if (BeamPhase == XrayBeamPhase.NoBeam)
            {
                if (holdingDash)
                    BeamPhase = XrayBeamPhase.Widening;
                else
                    StartClosing();
            }
            else if (BeamPhase == XrayBeamPhase.Widening)
            {
                if (!holdingDash)
                {
                    StartClosing();
                    return;
                }

                // The widening speed grows each update, and the width grows by it until it passes the widest
                // half-width, where it is clamped.
                AngularWideningSpeed += XrayScopeDefinitions.Cone.WideningStep;
                AngularHalfWidth += AngularWideningSpeed;
                if (AngularHalfWidth.WholeStep > XrayScopeDefinitions.Cone.WidestHalfWidth)
                {
                    AngularHalfWidth = XrayScopeDefinitions.Cone.WidestHalfWidth;
                    BeamPhase = XrayBeamPhase.Full;
                }
            }
            else if (BeamPhase == XrayBeamPhase.Full)
            {
                if (!holdingDash)
                {
                    StartClosing();
                    return;
                }

                // Aiming gives Up priority when opposite directions are both held, turning the beam one angle unit
                // while keeping its edge within the facing-dependent limit.
                if ((controllerInput & GameplayAction.Up) != GameplayAction.None)
                    Angle = AimedUp(Angle, AngularWidth);
                else if ((controllerInput & GameplayAction.Down) != GameplayAction.None)
                    Angle = AimedDown(Angle, AngularWidth);
            }
            else if (BeamPhase == XrayBeamPhase.Closing)
            {
                // Each closing update keeps time frozen and Samus held; the update after the last releases her.
                Closing = Closing.Spend();
                if (Closing.IsOver)
                    BeamPhase = XrayBeamPhase.Finish;
            }
            else
            {
                // The remaining member, XrayBeamPhase.Finish.
                // The beam's last step ends the scope only while time is frozen. Automatic Reserve completion lets time
                // run first, so the finishing beam then stays forever and the screen greys out.
                if (TimeIsFrozen)
                    Finish(samus);
            }
        }

        return SnapshotBeamStep(
            phaseAtStart,
            completed: !IsActive);
    }

    /// <summary>Runs the X-ray visor's colouring, which changes only the visor's shade until restoration.</summary>
    public bool UpdateColouring(
        SamusColours colours,
        SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);

        if (VisorShadeMode == XrayVisorShadeMode.TornDown)
        {
            // The first colouring update after X-ray ends shows Samus's complete normal suit shade again and clears
            // every X-ray shade value; the visor's glow restarts at its widening for the next X-ray.
            SamusSuitShade.Show(colours, equippedItems);
            _colouring.Release(SamusSpecialColouringKind.Xray);
            SamusGlowPlay.Restart(Visor, SamusGlowSegment.Of(SamusGlow.XrayVisorWidening));
            VisorShadeMode = XrayVisorShadeMode.Widening;
            return true;
        }

        if (!_colouring.Holds(SamusSpecialColouringKind.Xray))
            return false;

        if (VisorShadeMode == XrayVisorShadeMode.Widening && BeamPhase is not (XrayBeamPhase.NoBeam or XrayBeamPhase.Widening))
        {
            // The visor enters the steady cycle, whose first shade shows on this update.
            SamusGlowPlay.Restart(Visor, SamusGlowSegment.Of(SamusGlow.VisorCycle));
            VisorShadeMode = XrayVisorShadeMode.FullBeam;
        }

        if (Visor.StepToNewFrame() is not { } shade)
            return false;
        colours.ShowVisor(shade);
        return true;
    }

    /// <summary>
    /// The beam's centre after one update of aiming up from <paramref name="center"/> with edges
    /// <paramref name="halfWidth"/> steps either side. Facing right (a centre before the half turn), up turns back
    /// toward the angle zero, which the earlier edge may not cross; facing left, up turns on toward the full turn,
    /// which the later edge may not cross.
    /// </summary>
    /// <param name="center">The beam's centre.</param>
    /// <param name="halfWidth">The whole steps from the centre to each edge.</param>
    private static RotationAngle AimedUp(RotationAngle center, TrigonometryAngle halfWidth) => center < RotationAngle.HalfTurn
        ? TurnedBackToEarlierEdgeLimit(center, halfWidth, RotationAngle.Zero)
        : TurnedOnToLaterEdgeLimit(center, halfWidth, RotationAngle.Zero);

    /// <summary>
    /// The beam's centre after one update of aiming down from <paramref name="center"/> with edges
    /// <paramref name="halfWidth"/> steps either side. Facing right, down turns on toward straight down, which the
    /// later edge may not cross; facing left, down turns back toward straight down, which the earlier edge may not cross.
    /// </summary>
    /// <param name="center">The beam's centre.</param>
    /// <param name="halfWidth">The whole steps from the centre to each edge.</param>
    private static RotationAngle AimedDown(RotationAngle center, TrigonometryAngle halfWidth) => center < RotationAngle.HalfTurn
        ? TurnedOnToLaterEdgeLimit(center, halfWidth, RotationAngle.HalfTurn)
        : TurnedBackToEarlierEdgeLimit(center, halfWidth, RotationAngle.HalfTurn);

    /// <summary>
    /// Turns <paramref name="center"/> back one step unless its earlier edge already rests on <paramref name="limit"/>;
    /// an edge that lies, or would lie, beyond the limit is clamped exactly onto it.
    /// </summary>
    private static RotationAngle TurnedBackToEarlierEdgeLimit(RotationAngle center, TrigonometryAngle halfWidth, RotationAngle limit)
    {
        int edge = EarlierEdgeStepsFrom(center, halfWidth, limit);
        if (edge == 0)
            return center;
        if (edge < 0)
            return limit + halfWidth.ToAngle();

        RotationAngle turned = center.TurnedBySteps(-1);
        return EarlierEdgeStepsFrom(turned, halfWidth, limit) < 0 ? limit + halfWidth.ToAngle() : turned;
    }

    /// <summary>
    /// Turns <paramref name="center"/> on one step unless its later edge already rests on <paramref name="limit"/>; an
    /// edge that lies, or would lie, beyond the limit is clamped exactly onto it.
    /// </summary>
    private static RotationAngle TurnedOnToLaterEdgeLimit(RotationAngle center, TrigonometryAngle halfWidth, RotationAngle limit)
    {
        int edge = LaterEdgeStepsFrom(center, halfWidth, limit);
        if (edge == 0)
            return center;
        if (edge > 0)
            return limit - halfWidth.ToAngle();

        RotationAngle turned = center.TurnedBySteps(1);
        return LaterEdgeStepsFrom(turned, halfWidth, limit) > 0 ? limit - halfWidth.ToAngle() : turned;
    }

    /// <summary>
    /// How far the edge <paramref name="halfWidth"/> steps earlier in the turn than <paramref name="center"/> lies on
    /// from <paramref name="limit"/>, the shorter way round: negative once it has crossed back over the limit. The centre
    /// stays on the limit's side of the turn and the half-width is at most the cone's widest, so the edge never lies further than
    /// that behind the limit.
    /// </summary>
    private static int EarlierEdgeStepsFrom(RotationAngle center, TrigonometryAngle halfWidth, RotationAngle limit) =>
        (center.ToTrigonometryAngle() - halfWidth).StepsFrom(limit.ToTrigonometryAngle());

    /// <summary>
    /// How far the edge <paramref name="halfWidth"/> steps later in the turn than <paramref name="center"/> lies on from
    /// <paramref name="limit"/>, the shorter way round: positive once it has crossed over the limit, which it never lies
    /// further than the cone's widest half-width beyond.
    /// </summary>
    private static int LaterEdgeStepsFrom(RotationAngle center, TrigonometryAngle halfWidth, RotationAngle limit) =>
        (center.ToTrigonometryAngle() + halfWidth).StepsFrom(limit.ToTrigonometryAngle());

    /// <summary>Restores Samus' normal pose and input and movement modes, subsystem enables, suit shade request and beam state, and ends an auto-cancelled selection.</summary>
    private void Finish(SamusState samus)
    {
        // The X-ray teardown intentionally classifies a grounded turn as standing. Releasing
        // X-ray during a crouched turn therefore stands Samus up—the game's
        // X-ray stand-up glitch—and the radius difference moves her center upward.
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        bool crouching = movementType == SamusMovementType.Crouching;
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        int oldRadius = samus.Kinematics.Radius.Y;
        SamusPoseId targetPose = crouching
            ? facingLeft ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose
            : facingLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose;

        samus.ChangePose(targetPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        int radiusDifference = samus.Kinematics.Radius.Y - oldRadius;
        // Teardown owns a direct history shift, including the crouched-
        // turn stand-up glitch. It does not consume an ordinary input transition.
        samus.CommitPoseHistory();
        if (radiusDifference >= 0)
            samus.Position -= new Offset16<RoomPixels>(0, radiusDifference);

        // The teardown returns both Samus's pose input and her movement to her pose. X-Mode can arrange for
        // an active spark to hold her movement while the old visor effect reaches this
        // teardown; ending that spark while retaining the spark's colouring timer is the Blue Suit result.
        SamusMovementHandOver.FromXray(samus);
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        TimeIsFrozen = false;
        SuspendedSubsystems = XraySuspendedSubsystems.None;
        IsActive = false;
        OwnsSamusControl = false;
        WarmUp = XrayWarmUp.Over;
        BeamPhase = XrayBeamPhase.NoBeam;
        Closing = XrayClosing.Over;
        Angle = RotationAngle.Zero;
        AngularHalfWidth = RotationAngle.Zero;
        AngularWideningSpeed = RotationAngle.Zero;
        VisorShadeMode = XrayVisorShadeMode.TornDown;
        samus.AudioRequests.QueueSound(SamusSound.XrayEnd, SoundRequestPriority.Normal);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
    }

    /// <summary>Commits a frozen X-ray turn pose with refreshed radii, animation, and history.</summary>
    private static void ApplyXrayPoseChange(
        SamusState samus,
        SamusPoseId targetPose)
    {
        samus.ChangePose(targetPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        // X-ray's frozen-time pose input, turning or aiming, shifts her pose history after setting up her pose; the
        // ordinary pose transitions do not run.
        samus.CommitPoseHistory();
    }

    /// <summary>
    /// The visor pose <paramref name="movementType"/> admits (<see cref="SamusMovementTypeDefinitions"/>): standing-like
    /// and crouching movement types take different visor poses.
    /// </summary>
    private static XrayPosture ClassifyAllowedMovement(SamusMovementType movementType) =>
        SamusMovementTypeDefinitions.Movements[movementType].Xray;

    /// <summary>Starts the beam's closing on the update its step reads Run released.</summary>
    private void StartClosing()
    {
        BeamPhase = XrayBeamPhase.Closing;
        Closing = XrayClosing.Started;
    }

    /// <summary>Records the starting beam phase separately from whether this update removed the effect.</summary>
    private static XrayBeamStepResult SnapshotBeamStep(
        XrayBeamPhase phaseAtStart,
        bool completed) => new(
            phaseAtStart,
            completed);

    /// <summary>The live X-ray scope (<see cref="Scope"/>): its cone effect steps from its activation until its teardown.</summary>
    /// <param name="xray">The X-ray state the scope belongs to.</param>
    internal sealed class LiveScope(SamusXrayState xray)
    {
        /// <summary>
        /// Advances the cone effect by one update: an update of the beam's warm-up, until that is over, and then the beam's
        /// own step. The reveal itself remains outside this movement state; the display reads the beam's angle and width
        /// only once the warm-up is over.
        /// </summary>
        internal XrayBeamStepResult StepBeam(SamusState samus, GameplayAction controllerInput) => xray.StepBeam(samus, controllerInput);
    }

    /// <summary>The X-ray's hold on Samus's pose input and movement (<see cref="Control"/>).</summary>
    /// <param name="xray">The X-ray state that holds them.</param>
    internal sealed class SamusControl(SamusXrayState xray)
    {
        /// <summary>
        /// Runs X-ray's own pose input, including the exact standing and crouching turns and their completion on the
        /// update the turn's last frame has one update left to show.
        /// </summary>
        internal void HandlePoseInput(SamusState samus, GameplayAction controllerInput) => xray.HandlePoseInput(samus, controllerInput);

        /// <summary>
        /// Executes the X-ray scope's movement: turning does nothing, while stable X-ray poses force the aim's hold and one
        /// of the five aims the angle selects.
        /// </summary>
        internal void StepMovement(SamusState samus) => xray.StepMovement(samus);
    }
}
