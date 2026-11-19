using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// State for stored Speed Booster shine and the shinespark that moves Samus (<see cref="SamusMovementOwner.Shinespark"/>).
/// </summary>
/// <remarks>
/// Its timers count whole updates and its movement keeps fractions of a pixel; the stored shine and the spark show
/// Samus's full-body cycle shades. Block collision uses the shared step; solid enemies stop a sideways spark through it,
/// and an upward spark tests them before its block move.
/// </remarks>
public sealed class SamusShinesparkState
{
    /// <summary>The update's audio requests the spark's sounds join as they are made: Samus's (#2057).</summary>
    internal GameplaySoundRequests AudioRequests { get; }

    /// <summary>
    /// The upward spark's acceleration in pixels per frame per frame: 7.0 when windup begins, raised by Samus's
    /// vertical acceleration on every update of a vertical or diagonal spark, and cleared when the crash finishes. A suit pickup
    /// during a suspended windup replaces it with the light beam's progress.
    /// </summary>
    private readonly SuitPickupSparkCarryover<SamusSuitPickupStage> _acceleration;

    /// <summary>
    /// How far the crash's orbiting copies have turned, 0 through <see cref="CrashOrbitTravelCarryover.LargestTravel"/>:
    /// the crash's entry keeps what the last orbit or the first echo it released left, and the first echo overwrites it
    /// with its height while it flies.
    /// </summary>
    private readonly CrashOrbitTravelCarryover _orbitTravel;

    /// <summary>The special suit colouring holding Samus's suit, which the stored shine and the shinespark take.</summary>
    private readonly SamusSpecialColouring _colouring;

    /// <summary>Creates a spark owner whose upward acceleration and orbit travel live in Samus's compatibility services.</summary>
    /// <param name="acceleration">Samus's suit pickup carryover, which holds the spark's upward acceleration.</param>
    /// <param name="orbitTravel">Samus's crash orbit carryover, which holds how far the crash's orbit has turned.</param>
    /// <param name="colouring">Samus's special suit colouring, which the shine and the spark take.</param>
    /// <param name="audioRequests">Samus's audio requests, which the spark's sounds join.</param>
    internal SamusShinesparkState(SuitPickupSparkCarryover<SamusSuitPickupStage> acceleration, CrashOrbitTravelCarryover orbitTravel,
        SamusSpecialColouring colouring, GameplaySoundRequests audioRequests)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        _colouring = Ensure.NotNull(colouring);
        _acceleration = Ensure.NotNull(acceleration);
        _orbitTravel = Ensure.NotNull(orbitTravel);
    }

    /// <summary>The special colouring timer: 180 stored, 60 windup, and 15 during active motion.</summary>
    public int ShineTimer { get; private set; }

    /// <summary>
    /// Where the shine's glow stands (#2387): the stored shine's six shades or the shinespark's four, one each colouring
    /// update and over again, in the suit Samus shows. Every suit's glow runs its shades alike, so a change of suit goes on
    /// from where the glow stood; each start of the shine or the spark starts it again.
    /// </summary>
    internal AnimationPlayback<SamusGlowSegment> ShineGlow { get; private set; } = ShineGlowStart;

    /// <summary>The shine's glow before its first shade.</summary>
    private static AnimationPlayback<SamusGlowSegment> ShineGlowStart => SamusGlowPlay.Before(SamusGlowSegment.StoredShinePower);

    /// <summary>
    /// The updates the windup still waits for a direction: <see cref="ShinesparkDefinitions.WindupFrames"/> when it
    /// starts, launching her upward on the update it runs out.
    /// </summary>
    public int WindupCountdown { get; private set; }

    /// <summary>
    /// The updates the crash still holds its echoes on Samus once its orbit has drawn in:
    /// <see cref="ShinesparkDefinitions.CrashHoldFrames"/> when the hold starts, finishing the crash on the update it runs
    /// out.
    /// </summary>
    public int CrashHoldCountdown { get; private set; }

    /// <summary>The shinespark's phase: a stored shine, or the windup, flight or crash that moves Samus.</summary>
    public ShinesparkPhase Phase { get; private set; }

    /// <summary>
    /// Ends the spark's hold on Samus's movement, and a stored shine with it, when another owner takes her movement
    /// (<see cref="SamusMovementHandOver"/>). It does not clear the independently kept colouring timer, boost counter or
    /// echoes; Draygon's release, which ends a spark unconditionally, relies on that distinction.
    /// </summary>
    internal void RelinquishMovement() => Phase = ShinesparkPhase.Inactive;

    /// <summary>
    /// Whether the spark moves Samus (<see cref="SamusMovementOwner.Shinespark"/>): from the windup, through the launch
    /// and the crash, until the crash finishes.
    /// </summary>
    public bool OwnsMovement => Phase is
        ShinesparkPhase.Windup or ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or
        ShinesparkPhase.Diagonal or ShinesparkPhase.Crash or ShinesparkPhase.CrashHold or
        ShinesparkPhase.CrashFinish;

    /// <summary>
    /// Crystal Flash takes over both Samus's movement and the shared special
    /// colouring timer, and takes Samus's suit colouring. The new owner holds them; do not resume this old owner
    /// after Flash finishes. Extra run speed and the boost counter are not cleared.
    /// </summary>
    internal void RelinquishToCrystalFlash()
    {
        Phase = ShinesparkPhase.Inactive;
        ShineTimer = 0;
        ShineGlow = ShineGlowStart;
    }

/// <summary>
    /// The stage of the crash's orbit: expanding at crash entry, orbiting once the radius reaches sixteen,
    /// and contracting once the orbit's travel reaches 128.
    /// </summary>
    public ShinesparkCrashOrbitStage CrashOrbitStage { get; private set; }

    /// <summary>
    /// Distance of the two orbiting copies from Samus in pixels, 0 through 16: zero at crash entry, raised by four each
    /// update the orbit expands until it reaches sixteen and lowered by four each update it contracts until it reaches
    /// zero.
    /// </summary>
    public int CrashRadius { get; private set; }

    /// <summary>The first orbiting copy's angle around Samus.</summary>
    public RotationAngle FirstCrashEchoAngle { get; private set; }

    /// <summary>The second orbiting copy's angle, half a turn from the first.</summary>
    public RotationAngle SecondCrashEchoAngle { get; private set; }

    /// <summary>
    /// The steps of a 256-step turn both orbiting copies turn by on each orbiting update: four while Samus faces left and
    /// minus four otherwise, set at crash entry.
    /// </summary>
    private int _crashOrbitTurn;

    /// <summary>Where the first orbiting copy is shown while the crash runs.</summary>
    public Position16<RoomPixels> FirstOrbitingCopy { get; private set; }

    /// <summary>Where the second orbiting copy is shown while the crash runs; it is drawn before the first.</summary>
    public Position16<RoomPixels> SecondOrbitingCopy { get; private set; }

    /// <summary>The echo the crash releases first, only when Samus's shot count leaves room for both.</summary>
    public ShinesparkCrashEcho FirstReleasedEcho { get; } = new();

    /// <summary>The echo the crash releases second, whenever the shot count leaves room for one; it is drawn before the first.</summary>
    public ShinesparkCrashEcho SecondReleasedEcho { get; } = new();

    /// <summary>
    /// Starting the X-ray scope clears the special suit-colouring timer and X-ray's colouring
    /// takes over Samus's suit. Release the shine's colouring so it cannot keep ticking or
    /// repaint over the visor's new colours.
    /// </summary>
    internal void RelinquishColoursToXray()
    {
        ShineTimer = 0;
        ShineGlow = ShineGlowStart;
        if (Phase == ShinesparkPhase.Stored)
            Phase = ShinesparkPhase.Inactive;
    }

    /// <summary>
    /// Clearing Samus's shots (a room load, the elevator, the opening cinematic's flashbacks) forgets the released echoes
    /// and restarts the crash's orbit: its stage, radius, angles, turn and copies, and its travel, which the next crash
    /// starts from. The spark's phase and its unrelated timers are unchanged.
    /// </summary>
    internal void ResetProjectileEchoStorage()
    {
        FirstReleasedEcho.Forget();
        SecondReleasedEcho.Forget();
        _orbitTravel.Clear();
        FirstCrashEchoAngle = SecondCrashEchoAngle = RotationAngle.Zero;
        _crashOrbitTurn = 0;
        FirstOrbitingCopy = SecondOrbitingCopy = Position16<RoomPixels>.Zero;
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
    }

    /// <summary>
    /// A crouch at the full boost stage stores a shinespark.
    /// </summary>
    public bool TryStoreFromSpeedBooster(SamusSpeedBoostCounter speedBoostCounter)
    {
        if (!speedBoostCounter.IsAtFullStage)
            return false;

        ShineTimer = ShinesparkDefinitions.StoredShineFrames;
        _colouring.Take(SamusSpecialColouringKind.SpeedBoosterShine);
        ShineGlow = ShineGlowStart;
        Phase = ShinesparkPhase.Stored;
        return true;
    }

    /// <summary>
    /// Starts the shinespark windup after Samus's pose handling has
    /// selected windup <see cref="SamusPoseId.ShinesparkWindupRightPose"/> and <see cref="SamusPoseId.ShinesparkWindupLeftPose"/>.
    /// </summary>
    public void BeginWindup(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!SamusInputSnapshot.HoldsShine(samus))
            throw new InvalidOperationException("Shinespark windup requires a live stored shine.");

        InitializeWindup(samus);
    }

    /// <summary>The two shinespark demos start the windup without a stored shine.</summary>
    internal void BeginDemoLaunch(SamusState samus, SamusPoseId targetPose)
    {
        InitializeWindup(samus);
        BeginDirectionalLaunch(samus, targetPose);
    }

    /// <summary>Cancels beam charge and installs the spark windup, shade and boost state.</summary>
    private void InitializeWindup(SamusState samus)
    {
        // The windup checks the charge counter before clearing it. A charge that has
        // reached the draw-time sound threshold first queues the charge-cancellation sound; shorter
        // charges are still torn down without a sound call.
        BeamChargeLevel cancelledCharge = samus.BeamCharge.Level;
        if (cancelledCharge.IsCharging)
        {
            samus.BeamCharge.ClearChargeAndFlare();
            if (cancelledCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
                AudioRequests.QueueSound(SamusSound.Silence, SoundRequestPriority.High);
        }

        // The windup takes over Samus's movement and suit colouring. Flash's live
        // colouring timer can admit this launch even after its movement was interrupted.
        SamusMovementHandOver.To(samus, SamusMovementOwner.Shinespark);
        samus.CrystalFlash.RelinquishSuitColouring();
        // A Reserve-Mode X-Ray cone effect survives, but directional spark setup replaces
        // X-ray's suit colouring with the shinespark's.
        samus.Xray.RelinquishSuitColouring();

        // The windup sets these whatever they held. The speed boost stays at its full stage through the windup
        // even though her base speed is zero and her extra speed becomes exactly eight pixels per frame.
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        samus.Kinematics.YSpeed = default;
        samus.KnockbackDirection = Movement.SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        samus.HorizontalSpeed.SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage;
        samus.HorizontalSpeed.ExtraRunSpeed = ShinesparkDefinitions.WindupRunSpeed;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.BombJumpRequest = SamusBombJumpRequest.None;

        _acceleration.StartSpark(ShinesparkDefinitions.WindupInitialVerticalAcceleration);
        WindupCountdown = ShinesparkDefinitions.WindupFrames;
        ShineTimer = ShinesparkDefinitions.WindupShadeFrames;
        _colouring.Take(SamusSpecialColouringKind.Shinesparking);
        ShineGlow = ShineGlowStart;
        Phase = ShinesparkPhase.Windup;
        samus.HurtFlash.Clear();
    }

    /// <summary>
    /// Starts the launch the shinespark's direction selects for a horizontal, vertical or diagonal shinespark pose,
    /// taking its pose at once.
    /// </summary>
    public void BeginDirectionalLaunch(SamusState samus, SamusPoseId targetPose)
    {
        BeginLaunchAwaitingPose(samus, targetPose);
        ApplyDirectionalLaunchPose(samus, targetPose);
    }

    /// <summary>
    /// Starts the launch <paramref name="targetPose"/> selects, leaving the pose itself for the frame's pose update
    /// (<see cref="ApplyDirectionalLaunchPose"/>).
    /// </summary>
    private void BeginLaunchAwaitingPose(SamusState samus, SamusPoseId targetPose)
    {
        Ensure.NotNull(samus);
        // The launch takes Samus's movement from whoever holds it. A windup whose hold
        // her pose's movement took back (a knockback ending in a windup pose) keeps its
        // pose and the windup's speeds, and a direction still launches from it.
        // Only the six directional shinespark poses launch a spark; any other pose is a broken contract, not another
        // launch the game has.
        Phase = ShinesparkDefinitions.LaunchPhases.TryGetValue(targetPose, out ShinesparkPhase launch)
            ? launch
            : throw new ArgumentOutOfRangeException(nameof(targetPose), targetPose,
                "Only the six directional shinespark poses launch a spark.");

        samus.InputSnapshot.ShinesparkPoseInputLocked = true;
        samus.InputSnapshot.AutoJumpInputPending = false;
        samus.HorizontalSpeed.ClearSpeedEchoes();
        AudioRequests.QueueSound(RoomSound.Shinespark, SoundRequestPriority.High);
    }

    /// <summary>Commits the interrupted launch pose after the current frame's animation.</summary>
    internal static void ApplyDirectionalLaunchPose(SamusState samus, SamusPoseId pose)
    {
        samus.Pose = pose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>Runs one frame of the spark's windup, flight or crash.</summary>
    /// <param name="terrain">The room terrain Samus's block collision probes: the level data and the room objects that react to touched blocks (#633).</param>
    /// <param name="samus">Live player state; movement, energy, contact damage, pose state, and echo coordinates may change during the call.</param>
    /// <param name="frame">What one shinespark step reads from the frame: clocks, projectiles, invincibility and the previous checkpoint (#633).</param>
    internal ShinesparkMovementResult Step(SamusTerrain terrain, SamusState samus, ShinesparkFrame frame)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        if (Phase == ShinesparkPhase.Windup)
        {
            // The windup launches Samus upward on the update its countdown runs out.
            bool timedOut = --WindupCountdown == 0;
            SamusPoseId? pendingLaunchPose = null;
            if (timedOut)
            {
                SamusPoseId verticalPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
                    ? SamusPoseId.ShinesparkVerticalLeftPose
                    : SamusPoseId.ShinesparkVerticalRightPose;
                // The launch's pose waits for the frame's pose update, so Samus's animation still sees the windup body.
                BeginLaunchAwaitingPose(samus, verticalPose);
                pendingLaunchPose = verticalPose;
            }

            return new ShinesparkMovementResult(
                timedOut, PendingLaunchPose: pendingLaunchPose);
        }

        if (Phase is not (ShinesparkPhase.Horizontal or
            ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal))
        {
            if (Phase == ShinesparkPhase.Crash)
                return StepCrashOrbit(samus);
            if (Phase == ShinesparkPhase.CrashHold)
                return StepCrashHold();
            if (Phase == ShinesparkPhase.CrashFinish)
                return FinishCrash(samus, frame.Projectiles);

// A stored shine or no spark at all does not move Samus. Returning
            // an empty snapshot is defensive; runtime normally never runs them here.
            return new ShinesparkMovementResult(
                false);
        }

        samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.Shinespark;
        samus.HurtFlash.HoldAt(ShinesparkDefinitions.MovingSparkHurtFlashCount);

        // Each moving phase samples echoes before motion. Runtime suppresses its ordinary
        // post-motion producer while the spark moves Samus.
        samus.HorizontalSpeed.CaptureSpeedEchoPosition(frame.GameTimeFrames, samus.Position.Whole);

        BlockMoveResult? horizontal = null;
        BlockMoveResult? vertical = null;
        // Each axis ends by capping how far the camera's previous position trails Samus.
        if (Phase is ShinesparkPhase.Horizontal or ShinesparkPhase.Diagonal)
        {
            horizontal = MoveX(terrain.Level, samus, terrain.RoomObjects);
            CapPreviousX(samus, frame.PreviousCheckpoint);
        }
        if (Phase is ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal)
        {
            vertical = MoveY(terrain.Level, samus, frame.FrameCounter, terrain.RoomObjects);
            CapPreviousY(samus, frame.PreviousCheckpoint);
        }

        // The game's X and Y movement share one collision flag. Diagonal movement runs Y
        // last, so a clear vertical probe replaces an earlier wall hit and allows the
        // spark to slide upward. Combining both results would terminate a spark
        // early; use the last executed axis, including when its result is clear.
        bool collided = vertical?.Collided ?? horizontal?.Collided ?? false;
        // The cannot-die assist bypasses only the energy exit. Terrain/enemy collision still
        // ends all three launch directions in the ordinary crash.
        bool hasSustainingEnergy = samus.Health >= ShinesparkDefinitions.MinimumSustainingEnergy;
        bool lowEnergy = !frame.SamusCannotDie && !hasSustainingEnergy;
        if (collided || lowEnergy)
            BeginCrash(samus);

        // At exactly 30 energy, this frame still moves and drains to 29. The following
        // frame moves once more before the shinespark's end check observes that 29 is below the threshold.
        // With the cannot-die assist, energy loss continues without requiring refills: below
        // the cutoff, continue draining to one, never decrementing zero or one.
        bool drained = frame.SamusCannotDie
            ? samus.Health > 1
            : hasSustainingEnergy;
        if (drained)
            samus.Health = SamusQuantity.DecrementPositive(samus.Health);

        return new ShinesparkMovementResult(
            false);
    }

    /// <summary>
    /// Runs the stored shine's or the shinespark's suit colouring, restoring Samus's
    /// normal colours when its countdown runs out.
    /// </summary>
    public bool UpdateColouring(SamusColours colours, SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);
        SamusSpecialColouringKind colouring = _colouring.Holder;
        if (colouring is not (SamusSpecialColouringKind.SpeedBoosterShine or SamusSpecialColouringKind.Shinesparking))
            return false;

        if (colouring == SamusSpecialColouringKind.SpeedBoosterShine &&
            ShineTimer == ShinesparkDefinitions.StoredShineFrames - ShinesparkDefinitions.StoredShineSoundDelayFrames)
            AudioRequests.QueueSound(RoomSound.StoredShinespark, SoundRequestPriority.High);

        bool expired = --ShineTimer <= 0;
        if (expired)
        {
            _colouring.Release(colouring);
            ShineGlow = ShineGlowStart;
            if (Phase == ShinesparkPhase.Stored)
                Phase = ShinesparkPhase.Inactive;
            SamusSuitShade.Show(colours, equippedItems);
            return true;
        }

        SamusSuit suit = equippedItems.ShownSuit();
        SamusGlowSegment glow = colouring == SamusSpecialColouringKind.SpeedBoosterShine
            ? SamusGlowAnimationDefinitions.StoredShine[suit]
            : SamusGlowAnimationDefinitions.ActiveShinespark[suit];
        AnimationPlayback<SamusGlowSegment> playback = ShineGlow;
        colours.ShowSuit(SamusGlowPlay.NextShade(glow, ref playback).Shown);
        ShineGlow = playback;
        return true;
    }

    /// <summary>
    /// Stops translational spark motion, sets the orbit's angles and turn, ends the boost's echoes, places both orbiting
    /// copies on Samus and starts the crash. The orbit's travel is not restarted: the orbit counts on from
    /// what the last orbit or the first echo it released left (<see cref="CrashOrbitTravelCarryover"/>).
    /// </summary>
    private void BeginCrash(SamusState samus)
    {
        // The crash entry kills translational velocity before the crash starts.
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.SpeedBoostCounter = default;
        // Ending the shinespark keeps this frame's contact damage. The start of the next
        // update clears it before the crash's first frame runs.
        samus.HurtFlash.Clear();
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        ShinesparkDepartureAngles orbitStart = facingLeft
            ? ShinesparkEchoDefinitions.CrashOrbitStartFacingLeft
            : ShinesparkEchoDefinitions.CrashOrbitStartFacingRight;
        FirstCrashEchoAngle = orbitStart.First;
        SecondCrashEchoAngle = orbitStart.Second;
        _crashOrbitTurn = facingLeft ? ShinesparkDefinitions.CrashOrbitTurnSteps : -ShinesparkDefinitions.CrashOrbitTurnSteps;
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
        samus.HorizontalSpeed.ClearSpeedEchoes();
        FirstOrbitingCopy = SecondOrbitingCopy = samus.Position.Whole;
        Phase = ShinesparkPhase.Crash;
        // The crash entry queues its two sounds together.
        AudioRequests.QueueSound(SamusSound.SamusDamaged, SoundRequestPriority.Normal);
        AudioRequests.QueueSound(RoomSound.ShinesparkEnd, SoundRequestPriority.Normal);
    }

    /// <summary>Runs the crash's orbit: its expanding, orbiting and contracting stages.</summary>
    private ShinesparkMovementResult StepCrashOrbit(
        SamusState samus)
    {
        ShineTimer = ShinesparkDefinitions.SparkShadeFrames;
        switch (CrashOrbitStage)
        {
            case ShinesparkCrashOrbitStage.Expanding:
            {
                // The crash tests the old radius against the orbit's start, after expanding.
                // The transition frame therefore publishes the orbiting radius and the orbiting stage.
                int oldRadius = CrashRadius;
                CrashRadius += ShinesparkDefinitions.CrashRadiusStep;
                if (oldRadius >= ShinesparkDefinitions.CrashOrbitStartRadius)
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Orbiting;
                break;
            }

            case ShinesparkCrashOrbitStage.Orbiting:
                FirstCrashEchoAngle = FirstCrashEchoAngle.TurnedBySteps(_crashOrbitTurn);
                SecondCrashEchoAngle = SecondCrashEchoAngle.TurnedBySteps(_crashOrbitTurn);
                if (_orbitTravel.Advance())
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Contracting;
                break;

            case ShinesparkCrashOrbitStage.Contracting:
                CrashRadius -= ShinesparkDefinitions.CrashRadiusStep;
                if (CrashRadius == 0)
                {
                    Phase = ShinesparkPhase.CrashHold;
                    CrashHoldCountdown = ShinesparkDefinitions.CrashHoldFrames;
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
                    FirstCrashEchoAngle = RotationAngle.Zero;
                    SecondCrashEchoAngle = RotationAngle.Zero;
                }
                break;

            default:
                throw new InvalidOperationException(
                    $"Shinespark crash orbit stage {CrashOrbitStage} is undefined.");
        }

        // The copies stand the crash radius, 0 through 16, from Samus along their angles; placing them past the room's edge
        // continues from its other end, as every room position does.
        FirstOrbitingCopy = samus.Position.Whole + OffsetAlong(FirstCrashEchoAngle, CrashRadius);
        SecondOrbitingCopy = samus.Position.Whole + OffsetAlong(SecondCrashEchoAngle, CrashRadius);

        return new ShinesparkMovementResult(
            false);
    }

    /// <summary>Runs one update of the crash's hold, finishing the crash once the hold runs out.</summary>
    private ShinesparkMovementResult StepCrashHold()
    {
        ShineTimer = ShinesparkDefinitions.SparkShadeFrames;
        if (--CrashHoldCountdown == 0)
            Phase = ShinesparkPhase.CrashFinish;
        return new ShinesparkMovementResult(
            false);
    }

    /// <summary>
    /// The crash's end: releases its echoes on Samus's shots as far as her shot count leaves room, restores standing,
    /// and expires the shinesparking shade. A released first echo leaves Samus's height as the next orbit's travel; a
    /// crash that releases no first echo leaves the travel its own orbit ended on.
    /// </summary>
    internal ShinesparkMovementResult FinishCrash(SamusState samus, SamusProjectileSystem projectiles)
    {
        Ensure.NotNull(projectiles);
        // Resolve the crash axis before publishing the standing pose.
        var departureAngles = ShinesparkEchoDepartures.DepartureAngles(samus.Pose, projectiles.DrainedCrashDepartures);
        samus.HorizontalSpeed.ClearSpeedEchoes();

        if (projectiles.Shinespark.ReleaseCrashEchoes(FirstReleasedEcho, SecondReleasedEcho, departureAngles,
                SamusBeamCombinations.Of(samus.EquippedBeams), samus.Position.Whole))
            _orbitTravel.LeaveEchoHeight(samus.Position.Whole.Y);

        ShineTimer = ShinesparkDefinitions.FinishedShadeFrames;
        _acceleration.StopSpark();
        Phase = ShinesparkPhase.Inactive;
        return new ShinesparkMovementResult(
            false, CrashSequenceFinished: true);
    }

    /// <summary>Commits the crash's transitional standing pose after this frame's animation.</summary>
    public static void ApplyCrashFinishPose(SamusState samus)
    {
        // Finishing the crash returns Samus's input and movement to her pose, and so frees a
        // Samus drained mid-crash too.
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        SamusMovementHandOver.BackToPose(samus, SamusMovementOwner.Shinespark);
        SamusPoseId standingPose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? SamusPoseId.FacingLeftNormalPose
            : SamusPoseId.FacingRightNormalPose;
        int previousRadius = samus.Kinematics.Radius.Y;
        samus.Pose = standingPose;
        // The transitional pose returns Samus to her pose's movement and
        // aligns the standing body's bottom with the old crash body. The input phase alone
        // publishes the new live radius on the following frame.
        samus.AlignBottomAfterPoseChange(previousRadius, SamusPoseMetadata.ReadPoseYRadius(standingPose));
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// Runs one update of the released echo <paramref name="shot"/> carries: its radius grows by eight, and the echo is
    /// placed that far from Samus's current position along its angle, at the radius <see cref="ShinesparkEchoReturn"/>
    /// places it at, which brings an echo still on the screen back to Samus. An echo placed left of or above the camera's corner, or <see cref="ShinesparkDefinitions.EchoScreenReach"/> or more right of
    /// or below it, has left the screen; the first echo leaves its height for the crash's orbit, or nothing once it has
    /// left (<see cref="CrashOrbitTravelCarryover"/>).
    /// </summary>
    /// <param name="samus">Samus, around whom the echo is placed.</param>
    /// <param name="cameraPosition">The camera's room position.</param>
    /// <param name="shot">The shot carrying one of the crash's released echoes.</param>
    /// <returns>Whether the echo is still on the screen; its shot is cleared otherwise.</returns>
    /// <exception cref="InvalidOperationException">The shot carries neither of the crash's released echoes.</exception>
    internal bool StepReleasedEcho(SamusState samus, Position16<RoomPixels> cameraPosition, SamusProjectile shot)
    {
        ShinesparkCrashEcho echo = FirstReleasedEcho.IsCarriedBy(shot) ? FirstReleasedEcho
            : SecondReleasedEcho.IsCarriedBy(shot) ? SecondReleasedEcho
            : throw new InvalidOperationException("The shot carries none of the crash's released echoes.");

        // The point is recalculated from Samus's current position on every update; these are expanding radial
        // copies, not ordinary projectiles with independent velocity.
        ShinesparkEchoShotState flight = shot.KindStateAs<ShinesparkEchoShotState>();
        flight.Radius += ShinesparkDefinitions.EchoRadiusStep;
        Position16<RoomPixels> point = samus.Position.Whole +
            OffsetAlong(flight.Angle.ToAngle(), ShinesparkEchoReturn.PlacementRadius(flight.Radius));
        bool onScreen = CoordinateAxis.Difference(point.X, cameraPosition.X) is >= 0 and < ShinesparkDefinitions.EchoScreenReach &&
            CoordinateAxis.Difference(point.Y, cameraPosition.Y) is >= 0 and < ShinesparkDefinitions.EchoScreenReach;

        if (echo == FirstReleasedEcho)
        {
            if (onScreen)
                _orbitTravel.LeaveEchoHeight(point.Y);
            else
                _orbitTravel.Clear();
        }
        if (!onScreen)
            return false;

        echo.MoveTo(point);
        shot.Position = shot.Position.WithWhole(point);
        return true;
    }

    /// <summary>
    /// The offset <paramref name="radius"/> pixels from Samus along <paramref name="angle"/> at which the crash places its
    /// orbiting copies and echoes: each component the radius scaled by the angle's sine or cosine, truncated toward zero.
    /// </summary>
    internal static Offset16<RoomPixels> OffsetAlong(
        RotationAngle angle,
        int radius)
    {
        int x = LookupSignedComponent(angle, radius);
        int y = LookupSignedComponent(
            angle - RotationAngle.QuarterTurn,
            radius);
        return new(x, y);
    }

    /// <summary>The radius scaled by the angle's sine, truncated toward zero.</summary>
    private static int LookupSignedComponent(
        RotationAngle angle,
        int radius)
    {
        // The game multiplies the positive half-wave, discards the fraction, then
        // restores sign. Shifting a negative product would round differently.
        return Trigonometry.ScaledBySine(radius, angle.ToTrigonometryAngle());
    }

    /// <summary>
    /// After horizontal spark movement, a previous X more than
    /// 15 pixels behind Samus is pulled to 15 behind, in either direction.
    /// </summary>
    /// <remarks>
    /// Both positions are Samus's in the room, so the pulled position lies between them.
    /// </remarks>
    private static void CapPreviousX(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        int delta = samus.Position.X.Whole - samus.PeekPreviousPosition(checkpoint).X;
        if (delta >= 0)
        {
            if (delta > ShinesparkDefinitions.MostPreviousXLag)
                samus.WritePreviousXPosition(samus.Position.X.Whole - ShinesparkDefinitions.MostPreviousXLag);
        }
        else if (delta < -ShinesparkDefinitions.MostPreviousXLag)
        {
            samus.WritePreviousXPosition(samus.Position.X.Whole + ShinesparkDefinitions.MostPreviousXLag);
        }
    }

    /// <summary>
    /// After vertical spark movement, a previous Y more than 14
    /// pixels below Samus is pulled to 14 below.
    /// </summary>
    /// <remarks>
    /// Both positions are Samus's in the room, so the pulled position lies between them.
    /// </remarks>
    private static void CapPreviousY(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        if (samus.Position.Y.Whole - samus.PeekPreviousPosition(checkpoint).Y < -ShinesparkDefinitions.MostPreviousYLag)
            samus.WritePreviousYPosition(samus.Position.Y.Whole + ShinesparkDefinitions.MostPreviousYLag);
    }

    /// <summary>Accelerates horizontal spark speed up to fifteen pixels per frame and probes breakable room blocks.</summary>
    private BlockMoveResult MoveX(
        RoomLevelData level,
        SamusState samus,
        ISamusRoomObjects roomObjects)
    {
        ShineTimer = ShinesparkDefinitions.SparkShadeFrames;
        samus.HorizontalSpeed.ExtraRunSpeed += samus.Kinematics.YAcceleration;
        if (samus.HorizontalSpeed.ExtraRunSpeed.Whole >= ShinesparkDefinitions.MostRunSpeed)
        {
            samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(ShinesparkDefinitions.MostRunSpeed, 0);
        }

        var displacement = SamusHorizontalDisplacement.Toward(SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left), samus, baseSpeed: default);
        return SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = displacement.Displacement, CanBreakBombBlocks = true, CollisionMovementDirection = displacement.CollisionDirection });
    }

    /// <summary>Accumulates upward spark acceleration while capping only the temporary collision displacement and probing solid enemies first.</summary>
    private BlockMoveResult MoveY(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ShineTimer = ShinesparkDefinitions.SparkShadeFrames;
        _acceleration.Accelerate(samus.Kinematics.YAcceleration.AsDisplacement());
        samus.Kinematics.YSpeed += _acceleration.SparkAcceleration;

        // The clamp operates on a temporary displacement, not stored velocity.
        // Keep accumulating the speed even after movement reaches its cap.
        UnsignedSixteenSixteen displacementSpeed = samus.Kinematics.YSpeed.Whole >= ShinesparkDefinitions.MostRiseStep
            ? samus.Kinematics.YSpeed.WithWhole(ShinesparkDefinitions.MostRiseStep)
            : samus.Kinematics.YSpeed;
        SignedSixteenSixteen upwardDisplacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            samus.Kinematics,
            -displacementSpeed.AsDisplacement());

        // A rise meets the room's blocks over at most 15 whole pixels, keeping its fraction. A move that outside
        // forces reverse downward is not capped. The spark's speed never passes fourteen pixels, so every rise is
        // far short of a distance the cap could misjudge.
        SignedSixteenSixteen collisionMagnitude = -upwardDisplacement;
        if (collisionMagnitude.WholePixels >= ShinesparkDefinitions.MostRiseCollisionStep)
        {
            collisionMagnitude = SignedSixteenSixteen.FromParts(ShinesparkDefinitions.MostRiseCollisionStep, collisionMagnitude.Fraction);
            upwardDisplacement = -collisionMagnitude;
        }
        // A spark striking a solid enemy collides without moving up the distance to it, unlike an ordinary upward move,
        // which reaches the enemy's edge; where the spark crashes and recovers depends on it.
        SolidEnemyCollisionResult enemy = SamusSolidEnemyCollision.Probe(
            samus.Kinematics, samus.Kinematics.InteractiveEnemies, SamusCollisionDirection.Up,
            UnsignedSixteenSixteen.FromDisplacement(collisionMagnitude));
        samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Up, enemy.Enemy);
        if (enemy.Collided)
            return new BlockMoveResult(SignedSixteenSixteen.Zero, true, EnemyCollision: enemy);

        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = upwardDisplacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), CanBreakBombBlocks = true, IncludeSolidEnemies = false });
    }
}
