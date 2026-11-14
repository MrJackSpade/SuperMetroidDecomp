using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Grapple firing, persistent-block acquisition, connected
/// pendulum terrain interaction, collision kicks, and release.
/// </summary>
/// <remarks>
/// The retail game does not run grapple physics through the grappling movement type's tiny handler.
/// The grapple beam's handler updates the pendulum before the normal movement phase
/// movement, and the block collision advances its angle while probing room collision. This class keeps
/// those responsibilities separate from ordinary aerial movement. Firing, persistent and breakable
/// grapple blocks, enemy anchors, per-pixel rope adjustment, and the six-point angular terrain sweep are
/// here; solid/frozen-enemy wall-jump probing is shared with ordinary movement and asks the next enemy
/// pass to shake the enemy launched from. The firing-only Draygon-turret damage and both behaviour-indexed swing
/// spike-damage tables are applied through the shared periodic damage.
/// Grapple's narrower water flag and persistent environment-selected
/// release handler are kept apart from ordinary aerial movement.
/// </remarks>
public static class SamusGrappleMovement
{
    /// <summary>
    /// Fires the grapple beam (cancelling it when it cannot fire), stopping immediately
    /// before its phase returns. The current pose supplies all direction/origin
    /// policy; no separate aiming vector is accepted.
    /// </summary>
    public static void BeginFiring(SamusState samus, ControllerButton controllerInput = ControllerButton.None)
    {
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Inactive)
            throw new InvalidOperationException("A grapple state is already active.");

        bool movingHeld = samus.Pose is SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.DraygonGrabbedMovingRightPose;
        SamusProjectileDirection? aim = movingHeld ? ReadDraygonHeldDirection(samus.Pose, controllerInput) : SamusPoseMetadata.ReadAimDirection(samus);
        if (aim is not { } direction)
        {
            // A pose that aims nowhere (turning, or shooting disabled) cancels the grapple,
            // including a shot arriving while a permitted posture is transitioning.
            grapple.Phase = GrapplePhase.CancelPending;
            return;
        }

        grapple.Phase = GrapplePhase.Firing;
        grapple.FireDirection = direction;
        grapple.PoseChangeAutoFireTimer = GrappleBeamDefinitions.Firing.PoseChangeAutoFireFrames;
        var launch = GrappleFiring.Launch(direction);
        grapple.ExtensionVelocity = launch.Velocity;
        grapple.Angle = launch.Angle;
        grapple.MirroredAngle = grapple.Angle;

        // Firing selects run offsets solely for movement type one. Moonwalking is
        // distinct moonwalking type, so every stable/aimed moonwalk pose naturally takes the
        // no-run origin and flare tables without a pose-number exception.
        bool useRunOffsets = SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running;
        var origin = GrappleFiring.Origin(direction, useRunOffsets);
        var flare = ReadFlareOrigin(grapple, direction, useRunOffsets);
        int graphicsYOffset = movingHeld ? GrappleBeamDefinitions.Firing.DraygonMovingGraphicsYOffset : SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int physicalYOffset = movingHeld ? GrappleBeamDefinitions.Firing.DraygonMovingGraphicsYOffset :
            SamusPoseProjectileOrigins.ReadYOffset(samus.Pose);

        grapple.OriginOffset = new(origin.X, CoordinateAxis.Difference(origin.Y, physicalYOffset));
        grapple.FlareOffset = new(flare.X, CoordinateAxis.Difference(flare.Y, graphicsYOffset));

        // The endpoint offset is a signed 1/65536-pixel displacement from Samus plus the origin
        // table. Firing clears both axes, fraction and whole alike.
        grapple.EndpointOffset = FixedVector<RoomPixels>.Zero;
        // Firing's palette and sound setup.
        grapple.QuarterVelocity = FixedVector<RoomPixels>.Zero;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = GrappleBeamDefinitions.Physics.FiringExtension;
        grapple.AngularVelocity = 0;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        grapple.SpecialAngleHandling = false;
        grapple.SlowScrolling = false; // Firing clears the slow-scroll flag.
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        SamusGrappleBeamDrawing.InitializeBeamAnimation(grapple);

        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Fire);
        // The grapple's firing sound, like a beam shot's, cancels a pending resume of the charging sound (#1689).
        samus.ResumeChargingBeamSoundPending = false;
        // Firing again ends the released-from-swing movement handler, so
        // Samus moves with ordinary physics from this same frame.
        grapple.ReleasedMovementActive = false;
        // The game has two easily conflated coordinate pairs. Start is the hand/rope
        // origin used by connection positioning; Flare is the small-sprite draw origin used
        // by the rope draw. They coincide while swinging but can differ while firing or locked.
        grapple.RopeStart = samus.Position.Whole + grapple.OriginOffset;
        grapple.BeamStart = samus.Position.Whole + grapple.FlareOffset;
        grapple.Anchor = samus.Position.Whole + grapple.OriginOffset;
    }

    /// <summary>How far the grapple wall-jump probe looks for a wall toward the side Samus faces, in pixels.</summary>
    private const int WallJumpProbePixels = 16;

    /// <summary>
    /// Advances the extending grapple beam, including the enemy
    /// enemy pass which precedes the grapple beam's block collision.
    /// </summary>
    internal static GrappleMovementResult StepFiring(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        ISamusRoomObjects? roomObjects = null,
        Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision = null,
        GrapplePoseChange connectionPoseChange = GrapplePoseChange.Immediate)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Firing)
            throw new InvalidOperationException("Grapple firing is not active.");

        if (HandleFiringPoseChange(level, samus, controllerInput) is { } poseChange)
            return poseChange;

        // The connection's common tail clamps the camera's previous position after
        // the connection pose snaps Samus to the accepted rope geometry. Retain the pre-snap body
        // position even though most firing frames return without consuming this sample.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // Firing treats release of Shoot as cancellation before length or collision work.
        if ((controllerInput & ControllerButton.X) == ControllerButton.None)
            return QueueFiringCancellation(grapple);

        grapple.RopeLength += GrappleBeamDefinitions.Physics.FiringExtension;
        if (grapple.RopeLength >= GrappleBeamDefinitions.Physics.FiringCancelLength)
            return QueueFiringCancellation(grapple);

        // Firing tests the current endpoint against ordinary enemies before moving the
        // beam through its four block-collision substeps. The collision producer sets the
        // enemy's common grapple-handler flag; this consumer reads only the definition's
        // seven common reaction indexes. Kill/no-interaction deliberately fall through to
        // terrain, exactly like the zero result of their reactions.
        if (enemyCollision is not null)
        {
            GrappleEnemyCollision enemy = enemyCollision(grapple.Anchor);
            switch (enemy.Reaction)
            {
                case GrappleEnemyReaction.Attach:
                case GrappleEnemyReaction.AttachWithoutInvincibility:
                case GrappleEnemyReaction.AttachAndParalyze:
                    grapple.Anchor = enemy.Anchor;
                    return ConnectAcceptedFiringCore(
                        samus,
                        grapple,
                        previousPosition,
                        GrappleAnchorKind.Enemy,
                        connectionPoseChange);

                case GrappleEnemyReaction.Cancel:
                    return QueueFiringCancellation(grapple);

                case GrappleEnemyReaction.HurtSamus:
                    ApplyGrappleEnemyDamage(

                        samus,
                        controllerInput,
                        enemy.EnemyDamage);
                    return QueueFiringCancellation(grapple);
                // Kill and no interaction fall through to terrain, like the zero return.
                case GrappleEnemyReaction.None:
                case GrappleEnemyReaction.Kill:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined GrappleEnemyReaction {enemy.Reaction}.");
            }
        }

        // The rope moves its end in four equal substeps, probing after each; each keeps parts of the
        // update before's substep, so together they need not add up to the extension speed
        // (GrappleExtensionStepCarryover).
        grapple.QuarterVelocity = new(
            GrappleExtensionStepCarryover.Step(grapple.ExtensionVelocity.X, grapple.QuarterVelocity.X),
            GrappleExtensionStepCarryover.Step(grapple.ExtensionVelocity.Y, grapple.QuarterVelocity.Y));
        GrappleBlockReaction finalReaction = GrappleBlockReaction.Passes;
        for (int substep = 0; substep < GrappleExtensionStepCarryover.Substeps; substep++)
        {
            grapple.EndpointOffset += grapple.QuarterVelocity;
            PublishFiringGeometry(samus, grapple);

            GrappleBlockReaction reaction = ReactAtEndpoint(
                level,
                samus,
                grapple.Anchor,
                roomObjects);
            finalReaction = reaction;
            if (reaction != GrappleBlockReaction.Connects)
                continue;

            // The beam has connected. The collision centers the point in
            // the accepted 16x16 block before the grapple chooses the swing/locked pose.
            grapple.Anchor = grapple.Anchor.ToBlockCentre();
            return ConnectAcceptedFiringCore(
                samus,
                grapple,
                previousPosition,
                GrappleAnchorKind.Block,
                connectionPoseChange);
        }

        // The rope collision does not stop when an intermediate probe reports a solid block it
        // does not connect to. It executes all four quarter-velocity probes and returns only the
        // fourth reaction to the firing handler. At high running speed a thin solid cell can
        // therefore be sampled and then cleared before the caller decides whether to
        // cancel. Green Hill Zone's Grapple/Speed Booster gate exploit depends on this
        // general game ordering; it is not a gate-specific collision exception.
        if (finalReaction.Collides())
            return QueueFiringCancellation(grapple);

        return new GrappleMovementResult(
            grapple.Phase,
            Fired: true,
            OwnsMovement: false);
    }

    /// <summary>
    /// Executes the post-phase liquid-bit update.
    /// </summary>
    /// <remarks>
    /// Grapple physics reads this bit during a phase and the handler rewrites it only
    /// afterward, so callers invoke this exactly once at the end of the pass. The
    /// explicit phase set mirrors the phase-range test: release, cancel, dropped,
    /// wall-jump, and inactive phases all clear liquid physics immediately.
    /// </remarks>
    public static void RefreshLiquidPhysicsFlag(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool phaseCanRetainLiquid = samus.Grapple.Phase is
            GrapplePhase.Firing or GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease;
        samus.Grapple.Submerged = phaseCanRetainLiquid &&
            samus.LiquidPhysics.DetermineGrappleSubmersion(samus);
    }

    /// <summary>
    /// Executes one connected grapple-phase call. X is the default shoot binding used by
    /// this runtime; left/right pump the swing, newly pressed up/down adjust rope length.
    /// </summary>
    public static GrappleMovementResult Step(SamusTerrain terrain, SamusState samus, ControllerButton controllerInput, ControllerButton newlyPressedInput, int frameCounter = 0, Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision = null, GrapplePoseChange poseChange = GrapplePoseChange.Immediate)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        if (grapple.Phase == GrapplePhase.ReleaseFromSwing)
        {
            // The release cleanup executes on the frame after the swing queued it. The launch velocity
            // was already published, so this pass changes art/handlers and clears grapple.
            SamusPoseId? pendingReleasePose = CompleteQueuedRelease(terrain.Level, samus, grapple, poseChange == GrapplePoseChange.Deferred);
            return new GrappleMovementResult(GrapplePhase.Inactive, PendingReleasePose: pendingReleasePose);
        }

        // These are the game's own grapple phases. Each handler owns the whole
        // grapple movement phase, so dispatch it before entering ordinary pendulum integration.
        if (grapple.Phase == GrapplePhase.ConnectedLocked)
            return StepLocked(terrain.Level, samus, controllerInput, enemyCollision);
        if (grapple.Phase == GrapplePhase.WallGrab)
            return StepWallGrab(terrain.Level, samus, controllerInput, enemyCollision);
        if (grapple.Phase == GrapplePhase.WallGrabRelease)
            return StepWallGrabRelease(terrain, samus, newlyPressedInput);
        if (grapple.Phase == GrapplePhase.WallJumping)
            return CompleteGrappleWallJump(terrain.Level, samus, grapple);
        if (grapple.Phase == GrapplePhase.Dropped)
            return CompleteDropped(terrain.Level, samus, grapple, frameCounter, terrain.RoomObjects, poseChange == GrapplePoseChange.Deferred);

        if (grapple.Phase != GrapplePhase.ConnectedSwinging)
            throw new InvalidOperationException("Connected grapple movement is not active.");

        // The special-angle snap clamps the camera's previous position against the position from
        // the start of this frame after a special-angle snap. Retain that exact sample now;
        // the runtime consumes the optional corrected values returned below.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // The swing first checks Shoot. Releasing it does not immediately install jumping
        // art: it computes velocity and queues the release phase for the following frame.
        if ((controllerInput & ControllerButton.X) == ControllerButton.None)
        {
            if (grapple.AngularVelocity == 0 && grapple.Angle == RotationAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase);
            }

            SamusGrappleSwing.PropelSamusFromSwing(samus, grapple);
            // The release installs the released-from-swing movement handler immediately. The beam remains in
            // its one-frame release-cleanup phase, but movement already uses the
            // independent release handler during this same gameplay frame.
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(grapple.Phase);
        }

        SamusGrappleSwing.ApplyRopeAndDirectionInput(grapple, controllerInput, newlyPressedInput);
        SamusGrappleSwing.ApplyRopeLengthDelta(terrain.Level, samus, grapple);
        SamusGrappleSwing.CalculateGravity(grapple);
        SamusGrappleSwing.IntegrateAngularVelocity(grapple);
        SamusGrappleSwing.ApplyJumpImpulse(grapple, newlyPressedInput);
        SamusGrappleSwing.AdvanceAngleWithTerrainCollision(
            terrain.Level,
            samus,
            grapple);

        // The special-angle snap scans all eight records only when the collision raised the close-body
        // flag. A match replaces pendulum positioning for this frame with the table's exact
        // pose, anchor-relative body offset, and locked/wallgrab phase.
        if (grapple.SpecialAngleHandling &&
            TryHandleSpecialAngle(samus,
                grapple,
                previousPosition,
                out GrappleMovementResult special))
        {
            return special;
        }

        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorDisconnected = !enemyAnchorHeld &&
            (grapple.ValidateAnchorEnemy ||
             grapple.ValidateAnchorBlock &&
                !SamusGrappleSwing.IsStillConnectedToSupportedBlock(terrain.Level, samus, grapple));
        if (anchorDisconnected)
        {
            // A block that still stops the beam keeps the rope; one replaced with air (a breakable grapple block that
            // has crumbled), or an enemy anchor the rope has lost, ends the connection. A motionless straight-down
            // body queues the drop; a moving pendulum uses the velocity-preserving release handoff.
            if (grapple.AngularVelocity == 0 && grapple.Angle == RotationAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(grapple.Phase);
            }

            SamusGrappleSwing.PropelSamusFromSwing(samus, grapple);
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(grapple.Phase);
        }

        SamusGrappleSwing.PositionSamusFromPendulum(samus, grapple);

        return new GrappleMovementResult(grapple.Phase);
    }

    /// <summary>Holds the connected pose while Shoot and the stored block or enemy support the endpoint; otherwise queues cancellation.</summary>
    private static GrappleMovementResult StepLocked(
        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision)
    {
        SamusGrappleState grapple = samus.Grapple;

        // The locked handler does no pendulum work. It merely retains the frozen pose while Shoot
        // remains held and either an enemy or the stored block still supports the endpoint.
        bool shootHeld = (controllerInput & ControllerButton.X) != ControllerButton.None;
        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorHeld = enemyAnchorHeld || (grapple.ValidateAnchorEnemy
            ? false
            : !grapple.ValidateAnchorBlock ||
                SamusGrappleSwing.IsStillConnectedToSupportedBlock(level, samus, grapple));
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // Both release and block disconnection install the cancel. Mark the origin because
        // firing cancellation coexists with ordinary movement, whereas a locked grappling
        // body must own this movement phase until its pose-definition fallback is committed.
        grapple.CancelFromConnectedPose = !samus.DraygonGrabbed.IsActive;
        grapple.Phase = GrapplePhase.CancelPending;
        return new GrappleMovementResult(
            grapple.Phase,
            OwnsMovement: true);
    }

    /// <summary>
    /// Replays the connected-beam enemy test. Any nonzero common
    /// reaction keeps the connection alive for this frame; attach reactions additionally
    /// follow a moving actor's center. Without an enemy test the anchor is never an enemy
    /// and is not retained by this test.
    /// </summary>
    private static bool TryRetainEnemyAnchor(
        SamusGrappleState grapple,
        Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision)
    {
        if (enemyCollision is null)
            return false;

        GrappleEnemyCollision enemy = enemyCollision(grapple.Anchor);
        if (enemy.Reaction == GrappleEnemyReaction.None)
            return false;

        if (enemy.Attaches)
        {
            grapple.Anchor = enemy.Anchor;
            grapple.ValidateAnchorBlock = false;
            grapple.ValidateAnchorEnemy = true;
        }
        return true;
    }

    /// <summary>
    /// Applies common grapple reaction six. Unlike ordinary body contact, the grapple selects
    /// horizontal knockback from Samus's facing direction, but suit division and the
    /// 96-update invincibility and five-update knockback lifetimes are the same shared gameplay contracts.
    /// </summary>
    private static void ApplyGrappleEnemyDamage(

        SamusState samus,
        ControllerButton controllerInput,
        int damageBeforeSuit)
    {
        SamusSuitDamage.Hurt(samus, damageBeforeSuit);
        samus.InvincibilityTimer = WrappingCountdown.Start(96);

        // Facing left is knocked right and facing right is knocked left. Start owns the
        // authentic pose, animation, vertical speed, contact-damage cancellation, and
        // hurt-flash side effects instead of duplicating those writes here.
        KnockbackHorizontalDirection knockbackXDirection = SamusPoseMetadata.IsFacingLeft(samus.Pose)
            ? KnockbackHorizontalDirection.Right
            : KnockbackHorizontalDirection.Left;
        SamusKnockbackMovement.Start(samus, controllerInput, new KnockbackLaunch { KnockbackXDirection = knockbackXDirection, KnockbackTimer = 5 });
    }

    /// <summary>Keeps the wall-contact pose attached until Shoot or anchor support ends, then opens a 30-update wall-jump window.</summary>
    private static GrappleMovementResult StepWallGrab(
        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision)
    {
        SamusGrappleState grapple = samus.Grapple;
        bool shootHeld = (controllerInput & ControllerButton.X) != ControllerButton.None;
        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorHeld = enemyAnchorHeld || (grapple.ValidateAnchorEnemy
            ? false
            : !grapple.ValidateAnchorBlock ||
                SamusGrappleSwing.IsStillConnectedToSupportedBlock(level, samus, grapple));
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // The wall-grab writes decimal 30 and changes only the grapple phase. The contact
        // pose and beam remain visible throughout the following grace-window probes.
        grapple.WallJumpTimer = 30;
        grapple.Phase = GrapplePhase.WallGrabRelease;
        return new GrappleMovementResult(
            grapple.Phase);
    }

    /// <summary>During the release window, accepts a fresh Jump only when a 16-pixel probe finds a wall or solid enemy.</summary>
    private static GrappleMovementResult StepWallGrabRelease(

        SamusTerrain terrain,
        SamusState samus,
        ControllerButton newlyPressedInput)
    {
        SamusGrappleState grapple = samus.Grapple;

        // The countdown gives values 29..0 thirty eligible checks. The next call
        // runs past zero, queues dropped, and deliberately performs no wall probe.
        grapple.WallJumpTimer--;
        if (grapple.WallJumpTimer < 0)
        {
            grapple.Phase = GrapplePhase.Dropped;
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // Each eligible check first withdraws an earlier launch's enemy shake.
        samus.Kinematics.WithdrawWallJumpLaunchShake();

        // The wall-jump probe tests solid/frozen enemies first, then 16 pixels of room blocks toward
        // the wall named by the contact pose's facing. The shared probe preserves
        // square slopes, behaviour dispatch, available distance, and enemy identity.
        SignedSixteenSixteen signedProbe = SignedSixteenSixteen.FromParts(
            SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? -WallJumpProbePixels : WallJumpProbePixels, 0);
        // A bomb block the probe breaks starts its reaction through the room's objects, as any other contact does (#1582).
        BlockMoveResult wall = SamusBlockCollision.ProbeWallHorizontal(

            terrain.Level,
            samus.Kinematics, samus,
            signedProbe,
            terrain.RoomObjects);
        bool jumpNew = (newlyPressedInput & ControllerButton.A) != ControllerButton.None;
        if (wall.Collided && jumpNew)
        {
            // Accepted off a solid or frozen enemy, the wall jump asks this update's enemy pass to shake it.
            if (wall.EnemyCollision is { Enemy: { } launchedFrom })
                samus.Kinematics.RequestWallJumpLaunchShake(launchedFrom);
            grapple.Phase = GrapplePhase.WallJumping;
            return new GrappleMovementResult(
                grapple.Phase);
        }

        return new GrappleMovementResult(
            grapple.Phase);
    }

    /// <summary>Finishes the deferred wall jump by ejecting Samus, applying its dedicated pose transition, and clearing the beam.</summary>
    private static GrappleMovementResult CompleteGrappleWallJump(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // The wall-jump phase runs one frame after the grace check selected it, matching every other grapple
        // phase handoff. The Samus helper keeps the peculiar reversal of the left
        // wall-contact pose into the left wall jump, and the right into the right, separate from
        // ordinary spin-contact wall jumps.
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        SamusAerialPoseTransitions.ApplyGrappleWallJump(samus);
        ClearConnectedGrapple(grapple);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            WallJumpStarted: true);
    }

    /// <summary>Ends a dropped connection, clears movement speeds, and applies or defers the fall pose for the movement phase.</summary>
    private static GrappleMovementResult CompleteDropped(

        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        int frameCounter,
        ISamusRoomObjects? roomObjects,
        bool deferPoseChange)
    {
        SamusPoseId targetPose = SelectDroppedPose(samus);
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        // The drop clears these speeds now, but publishes its pose to the transitional
        // slot. Normal movement still runs before that pose is committed.
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;
        bool emptyMovement = SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Grappling;
        if (!deferPoseChange)
            SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus, level, targetPose, frameCounter, roomObjects);
        ClearConnectedGrapple(grapple);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            OwnsMovement: !deferPoseChange || emptyMovement,
            PendingDropPose: deferPoseChange ? targetPose : null);
    }

    /// <summary>Chooses the fall pose from shot direction and body radius, with standing exceptions for both swinging poses.</summary>
    internal static SamusPoseId SelectDroppedPose(SamusState samus)
    {
        // The swinging poses bypass the direction tables and always fall to ordinary
        // standing, even though their live collision radius is compact.
        if (samus.Pose == SamusPoseId.GrappleSwingRightPose)
            return SamusPoseId.FacingRightNormalPose;
        if (samus.Pose == SamusPoseId.GrappleSwingLeftPose)
            return SamusPoseId.FacingLeftNormalPose;

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } shotDirection)
        {
            // The drop phases use the pose's facing when the pose aims nowhere.
            // Standing selects the normal standing poses; compact bodies select crouching.
            bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
            return samus.Kinematics.Radius.Y >= 17
                ? facingLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose
                : facingLeft ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose;
        }

        return GrappleConnections.DroppedPose(shotDirection, compact: samus.Kinematics.Radius.Y < 17);
    }

    /// <summary>Applies an exact special-angle connection record, including its pose and bounded camera displacement.</summary>
    internal static bool TryHandleSpecialAngle(
        SamusState samus,
        SamusGrappleState grapple,
        Position16<RoomPixels> previousPosition,
        out GrappleMovementResult result)
    {
        // Preserve the reverse scan and exact angle equality, not a tolerance
        // around a wall-grab angle. These records govern collision positioning.
        IReadOnlyList<GrappleConnectionDefinitions.SpecialConnection> connections = GrappleConnections.SpecialConnections;
        for (int place = connections.Count - 1; place >= 0; place--)
        {
            var special = connections[place];
            if (special.Angle != grapple.Angle)
                continue;

            GrappleConnectionKind kind = special.Kind;
            GrapplePhase phase = kind switch
            {
                GrappleConnectionKind.LockedInPlace => GrapplePhase.ConnectedLocked,
                GrappleConnectionKind.WallGrab => GrapplePhase.WallGrab,
                GrappleConnectionKind.Swinging => throw new InvalidDataException(
                    $"Grapple special connection {special} names the swinging connection."),
                _ => throw new InvalidOperationException($"The grapple has no connection {kind}."),
            };

            samus.Pose = special.Pose;
            samus.Position = samus.Position.WithWhole(grapple.Anchor + special.SamusOffset);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
            grapple.Phase = phase;
            grapple.WallJumpTimer = 0;
            grapple.SlowScrolling = false; // Connecting clears the slow-scroll flag.

            Position16<RoomPixels> cameraPrevious = ClampPreviousPosition(samus.Position.Whole, previousPosition);
            result = new GrappleMovementResult(
                phase,
                CameraPrevious: cameraPrevious);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>Bounds the prior camera position to twelve pixels per axis after a special-angle snap.</summary>
    private static Position16<RoomPixels> ClampPreviousPosition(
        Position16<RoomPixels> current, Position16<RoomPixels> previous) =>
        new(ClampPreviousAxis(current.X, previous.X), ClampPreviousAxis(current.Y, previous.Y));

    /// <summary>Limits one axis of the previous position to twelve pixels using displacement wrapped at 65536.</summary>
    private static int ClampPreviousAxis(int current, int previous)
    {
        int displacement = CoordinateAxis.Difference(current, previous);
        if (displacement >= 13)
            return CoordinateAxis.Wrap(current - 12);
        if (displacement < -12)
            return CoordinateAxis.Wrap(current + 12);
        return previous;
    }

    /// <summary>Queues firing cleanup while leaving ordinary Samus movement in control of this update.</summary>
    private static GrappleMovementResult QueueFiringCancellation(SamusGrappleState grapple)
    {
        grapple.CancelFromConnectedPose = false;
        grapple.Phase = GrapplePhase.CancelPending;
        return new GrappleMovementResult(
            grapple.Phase,
            Fired: false,
            OwnsMovement: false);
    }

    /// <summary>Publishes rope, physical endpoint, and flare origins with the fraction's overflow from the firing offset.</summary>
    private static void PublishFiringGeometry(SamusState samus, SamusGrappleState grapple)
    {
        // Whole endpoint offsets are the signed whole parts of the two 1/65536-pixel sums. The signed halving is
        // intentional: a left/up beam must retain sign after fractional accumulation, as the endpoint offsets
        // read as signed displacements.
        grapple.RopeStart = samus.Position.Whole + grapple.OriginOffset;
        // The endpoint adds the offset to Samus's subpixel-inclusive position, so the
        // fraction's overflow reaches the endpoint before the whole origin offset is added.
        grapple.Anchor = samus.Position.Advance(grapple.EndpointOffset).Whole + grapple.OriginOffset;
        grapple.BeamStart = samus.Position.Whole + grapple.FlareOffset;
    }

    /// <summary>
    /// Dispatches the endpoint block and extensions, including room-object effects, into what the block does to the beam:
    /// lets it pass, stops it, or connects it.
    /// </summary>
    internal static GrappleBlockReaction ReactAtEndpoint(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> endpoint,
        ISamusRoomObjects? roomObjects = null)
    {
        RoomCollisionBlock block = level.GetCollisionBlockOrEdgeSolid(endpoint.ToRoomBlock());
        if (block.Position is null)
        {
            // The edge-solid block outside the room dispatches as an ordinary solid endpoint: it stops
            // the beam without connecting, which queues grapple cancellation.
            return GrappleBlockReaction.Solid;
        }

        // Each extension link leads on in the room's reading order; one that leads outside the grid reads the edge-solid block.
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            switch (block.CollisionType)
            {
                // These four block kinds let the beam pass: it remains live.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.UnrevealedAir:
                    return GrappleBlockReaction.Passes;

                // Slopes and these solid-family entries stop the beam without connecting,
                // which makes the firing handler queue cancellation rather than establish a rope.
                case RoomCollisionType.Slope:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpecialBlock:
                    return GrappleBlockReaction.Solid;

                case RoomCollisionType.SpikeBlock:
                    // The spike's reaction selects a grapple-reaction room object. A spike that lets the
                    // beam through rejects it. Every other spike uses the empty reaction, which stops the
                    // beam without latching, and immediately deletes; the turret spike alone uses Draygon's
                    // broken-turret setup, which adds one whole periodic-damage unit and latches the beam.
                    if (block.Behavior.Spike is not { } spike)
                        throw new InvalidDataException($"Grapple reached spike block {block.Position} with no spike reaction ({block.Behavior}).");
                    if (spike.LetsGrappleBeamThrough)
                        return GrappleBlockReaction.Passes;
                    if (spike == SpikeReaction.TurretSpike)
                    {
                        samus.LiquidPhysics.AccumulatePeriodicDamage(
                            subDamage: 0,
                            wholeDamage: 1);
                        return GrappleBlockReaction.Connects;
                    }

                    return GrappleBlockReaction.Solid;

                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                    // A horizontal extension repeats the block a signed number of blocks along its row, a vertical one along its column;
                    // an extension that ends the chain behaves as air.
                    if (block.Behavior.EndsExtension)
                        return GrappleBlockReaction.Passes;
                    block = level.LinkedBlockOrEdgeSolid(block);
                    continue;

                case RoomCollisionType.GrappleBlock:
                    // A rejecting reaction lets the beam through. The anchors spawn the persistent grapple
                    // room object, whose setup connects the beam.
                    // The persistent room object has no level mutation, so this result is complete.
                    if (block.Behavior.Grapple == GrappleReaction.Rejected)
                        return GrappleBlockReaction.Passes;
                    if (block.Behavior.Grapple is GrappleReaction.Anchor or GrappleReaction.UnrevealedAnchor)
                        return GrappleBlockReaction.Connects;

                    // Respawning and permanent breakable anchors spawn the breakable grapple room objects. Their setup synchronously saves the
                    // complete terrain block and clears its grapple reaction before connecting the beam; the new room object
                    // runs its first timed step later in this same gameplay frame.
                    // A caller which omitted the independent room owner cannot honestly
                    // preserve that lifecycle, so keep the missing integration seam explicit.
                    if (block.Behavior.Grapple is GrappleReaction.RespawningBreakableAnchor or GrappleReaction.PermanentBreakableAnchor)
                    {
                        if (roomObjects is null)
                        {
                            throw new InvalidOperationException(
                                "Breakable grapple acquisition requires a ISamusRoomObjects.");
                        }
                        if (!roomObjects.BlockReactions.TrySpawnBreakableGrappleBlock(level, block.At, block.Behavior))
                        {
                            throw new InvalidOperationException(
                                "All 40 room-object slots are occupied during grapple acquisition.");
                        }

                        return GrappleBlockReaction.Connects;
                    }

                    // A grapple block with no grapple reaction would index outside the four grapple room
                    // objects in the selection table.
                    throw new InvalidDataException(
                        $"Invalid grapple block behaviour ({block.Behavior}) at {block.Position}.");

                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.ShootableBlock:
                    // The shootable block reactions use the same table as ordinary
                    // projectiles. Grapple owns no projectile family, so weapon-gated
                    // entries reject it while behaviour 0..7 retain their unconditional block
                    // animation. The block's collision kind, not the room-object setup, decides whether
                    // the beam stops: shootable air lets it pass and a shootable block stops it.
                    if (roomObjects is null)
                    {
                        throw new InvalidOperationException(
                            "Shootable grapple reaction requires a ISamusRoomObjects.");
                    }
                    // Blue caps occupy the same table as breakable blocks but
                    // select door room objects. Grapple's negative projectile index bypasses the
                    // blue door setup's power-bomb rejection; the ordinary opening owner
                    // supplies all four cap blocks, timing, collision changes, and sound.
                    if (block.Behavior.TryGetDownwardGateTrigger(out _))
                        roomObjects.Gates.TrySpawnDownwardGateTrigger(level, block.At, block.Behavior, projectileType: SamusProjectileType.None);
                    else if (block.Behavior.TryGetBlueDoorOrientation(out _))
                        roomObjects.DoorObjects.TrySpawnBlueDoorOpening(level, block.At, block.Behavior, projectileType: SamusProjectileType.None);
                    else if (block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
                    {
                        // Generic shot-trigger setup also belongs to the grapple table.
                        // Notify the resident actor with no weapon family; its own
                        // frame behaviour decides whether to reject the hit or wake up.
                        if (!roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(block.At, projectileType: SamusProjectileType.None))
                            throw new InvalidOperationException(
                                $"Grapple trigger block {block.Position} has no resident room-object owner.");
                    }
                    else if (block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
                        roomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(block.At);
                    else
                    {
                        try
                        {
                            roomObjects.BlockReactions.TrySpawnProjectileShotBlock(
                                level,
                                block.At,
                                block.Behavior,
                                projectileType: SamusProjectileType.None,
                                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
                        }
                        catch (UnsupportedShotBlockReactionException error)
                        {
                            throw new UnmodeledBehaviorException(
                                $"grapple-shootable/type-{block.CollisionType}/behaviour-{block.Behavior.Shootable}",
                                $"Grapple shootable dispatch type {block.CollisionType}/behaviour ({block.Behavior}) " +
                                $"at block {block.Position} has no translated table entry.",
                                error);
                        }
                    }
                    return block.CollisionType == RoomCollisionType.ShootableBlock
                        ? GrappleBlockReaction.Solid
                        : GrappleBlockReaction.Passes;

                case RoomCollisionType.BombableAir:
                    // The bombable-air setup immediately deletes its provisional
                    // room object for grapple's zero projectile family. The beam passes.
                    return GrappleBlockReaction.Passes;

                case RoomCollisionType.BombableBlock:
                    // The solid twin performs the same rejected setup but independently
                    // reports the block solid to the beam from its collision reaction.
                    return GrappleBlockReaction.Solid;

                default:
                    throw new InvalidDataException(
                        $"Grapple block type {block.CollisionType} escaped the complete " +
                        $"sixteen block kinds at {block.Position}.");
            }
        }

        throw new InvalidDataException("Grapple extension chain exceeded sixteen blocks.");
    }

    /// <summary>Commits attachment, selecting the movement-specific pose and either locked or swinging handling.</summary>
    internal static GrappleMovementResult ConnectAcceptedFiringCore(
        SamusState samus,
        SamusGrappleState grapple,
        Position16<RoomPixels> previousPosition,
        GrappleAnchorKind anchor,
        GrapplePoseChange poseChange)
    {
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Attach);
        // While Draygon owns Samus the connection bypasses ordinary pose/position setup.
        // Samus. Only the beam locks; the enemy continues to place the held body.
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        if (sourceMovementType == SamusMovementType.DraygonHeld)
        {
            grapple.Phase = GrapplePhase.ConnectedLocked;
            grapple.ValidateAnchorBlock = anchor == GrappleAnchorKind.Block;
            grapple.ValidateAnchorEnemy = anchor == GrappleAnchorKind.Enemy;
            grapple.CancelFromConnectedPose = false;
            // The helper clears delta, then its firing caller immediately writes -8.
            grapple.RopeLengthDelta = GrappleBeamDefinitions.Physics.InitialConnectionRetraction;
            return new GrappleMovementResult(grapple.Phase, OwnsMovement: false);
        }

        bool movingVertically =
            samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0;
        GrappleConnectionPolicy policy = movingVertically
            ? GrappleConnectionPolicy.MovingVertically
            : sourceMovementType == SamusMovementType.Crouching
                ? GrappleConnectionPolicy.Crouching
                : GrappleConnectionPolicy.Default;
        var connection = GrappleConnections.ResolveConnection(policy, grapple.FireDirection);
        GrappleConnectionKind nextKind = connection.Kind;
        GrappleConnectionPose connectionPose = connection.Pose;

        // Each connection pose installs one prospective grappling pose and then continues
        // to either the swinging or the stuck connection. Keep the connection poses distinct:
        // pose alone is insufficient to distinguish malformed fallback data from data.
        (SamusPoseId pose, bool swinging) = connectionPose switch
        {
            GrappleConnectionPose.SwingClockwise =>
                new GrappleConnectedPose(SamusPoseId.GrappleSwingRightPose, true),
            GrappleConnectionPose.SwingAnticlockwise =>
                new GrappleConnectedPose(SamusPoseId.GrappleSwingLeftPose, true),
            GrappleConnectionPose.StandingUpRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingRightPose, false),
            GrappleConnectionPose.StandingRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingDownRightPose, false),
            GrappleConnectionPose.StandingDown =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingDownLeftPose, false),
            GrappleConnectionPose.StandingUpLeft =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingLeftPose, false),
            GrappleConnectionPose.CrouchingUpRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingRightPose, false),
            GrappleConnectionPose.CrouchingRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingDownRightPose, false),
            GrappleConnectionPose.CrouchingDownLeft =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingDownLeftPose, false),
            GrappleConnectionPose.CrouchingUpLeft =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingLeftPose, false),
            _ => throw new InvalidOperationException($"Undefined GrappleConnectionPose {connectionPose}."),
        };
        GrappleConnectionKind expectedKind = swinging
            ? GrappleConnectionKind.Swinging
            : GrappleConnectionKind.LockedInPlace;
        if (nextKind != expectedKind)
        {
            throw new InvalidDataException(
                $"Grapple connection pose {connectionPose} requires connection {expectedKind}, " +
                $"but the connection names {nextKind}.");
        }

        // Both the connection and the fired beam derive the angle from the body position that fired the beam,
        // before the connection pose is installed and snaps Samus to the rope. The angle is stored
        // as a whole step of the 256-step turn; its fraction becomes zero.
        RotationAngle angle = CalculateAngleFromXY(samus.Position.Whole - grapple.Anchor);
        grapple.Angle = angle;
        grapple.MirroredAngle = grapple.Angle;
        grapple.AngularVelocity = 0;
        grapple.RopeLengthDelta = 0;
        if (grapple.RopeLength >= 64)
            grapple.RopeLength -= 24;

        // The connection publishes the grapple-beam Start pair from the accepted endpoint,
        // angle, and possibly shortened rope. This is the hand attachment point, not the
        // independently authored Flare pair from the firing tables.
        GrappleCollisionPoint ropeStart = SamusGrappleSwing.CalculateCollisionPoint(
            grapple,
            angle.ToTrigonometryAngle(),
            grapple.RopeLength);
        grapple.RopeStart = ropeStart.Pixel;
        grapple.Phase = swinging
            ? GrapplePhase.ConnectedSwinging
            : GrapplePhase.ConnectedLocked;

        // Block and enemy acquisition share the same connection/pose machinery, but their following-frame validators
        // have different owners; the anchor kind keeps those origins exclusive, so a moving Powamp anchor is never
        // reinterpreted as room terrain.
        grapple.ValidateAnchorBlock = anchor == GrappleAnchorKind.Block;
        grapple.ValidateAnchorEnemy = anchor == GrappleAnchorKind.Enemy;
        grapple.SpecialAngleHandling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;

        // The connection helper clears delta while choosing the new pose, but its caller
        // immediately starts automatic retraction. Omitting that caller tail leaves Samus
        // dangling unless the player explicitly supplies a fresh Up edge. Apply it for
        // both block and enemy connections, never for an arbitrary restored swing state.
        grapple.RopeLengthDelta = GrappleBeamDefinitions.Physics.InitialConnectionRetraction;

        if (poseChange == GrapplePoseChange.Deferred)
        {
            // The swinging and stuck tails publish the pending connection pose. They do not
            // install that pose inline. The animation's pending transition pose can therefore keep
            // the old pose and every one of Samus's speeds while the grapple phase remains live.
            return new GrappleMovementResult(
                grapple.Phase,
                OwnsMovement: false,
                PendingConnection: new GrapplePendingConnection(
                    pose,
                    swinging,
                    previousPosition));
        }

        Position16<RoomPixels> cameraPrevious = ApplyPendingConnectionPose(
            samus,
            new GrapplePendingConnection(
                pose,
                swinging,
                previousPosition));
        return new GrappleMovementResult(
            grapple.Phase,
            OwnsMovement: true,
            CameraPrevious: cameraPrevious);
    }

    /// <summary>
    /// Applies the accepted grapple connection pose after the frame's
    /// super-special transition owners have declined it.
    /// </summary>
    internal static Position16<RoomPixels> ApplyPendingConnectionPose(
        SamusState samus,
        GrapplePendingConnection pending)
    {
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        samus.Pose = pending.Pose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);

        if (pending.Swinging)
        {
            // The swinging connection pose runs the swing. Swinging copies Start into Flare, then
            // chooses the angle-authored animation frame and body offsets.
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            SamusGrappleSwing.PositionSamusFromPendulum(samus, grapple);
        }
        else
        {
            // The locked connection pose runs the locked placement. The locked body is positioned from
            // Start minus the raw NO-RUN origin table, then Flare is independently rebuilt
            // from the raw no-run flare table. Graphics-Y correction does not participate.
            var origin = GrappleFiring.Origin(grapple.FireDirection, running: false);
            var flare = ReadFlareOrigin(grapple, grapple.FireDirection, running: false);
            samus.Position = samus.Position.WithWhole(grapple.RopeStart - new Offset16<RoomPixels>(origin.X, origin.Y));
            grapple.BeamStart = samus.Position.Whole + new Offset16<RoomPixels>(flare.X, flare.Y);
            grapple.Phase = GrapplePhase.ConnectedLocked;
        }

        // The swinging and locked connections share one tail: it cancels the speed boost, the echoes leaving the way
        // the connection pose faces, then clears the base, extra-run and vertical speeds. AccelerationMode is
        // deliberately retained: the common tail does not write it.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.Kinematics.YSpeed = default;

        return ClampPreviousPosition(samus.Position.Whole, pending.Previous);
    }

    /// <summary>
    /// Integer octant form of <c>CalculateAngleFromXY</c>.
    /// The returned angle is measured clockwise from negative Y: zero up, 64 right, 128 down,
    /// and 192 left. Division truncates exactly as the game's unsigned divide does.
    /// </summary>
    internal static RotationAngle CalculateAngleFromXY(Offset16<RoomPixels> delta)
    {
        int x = delta.X;
        int y = delta.Y;
        bool xNegative = x < 0;
        bool yNegative = y < 0;
        int absoluteX = Math.Abs(x);
        int absoluteY = Math.Abs(y);
        if (absoluteX == 0 && absoluteY == 0)
            return RotationAngle.Zero;

        // The quadrant counts negative X as two and negative Y as one, not the more tempting
        // reverse order.
        int quadrant = (xNegative ? 2 : 0) | (yNegative ? 1 : 0);
        if (absoluteY < absoluteX)
        {
            int steps = OctantSteps(EightEight.FromWhole(absoluteY) / absoluteX);
            return quadrant switch
            {
                0 => RotationAngle.QuarterTurn.TurnedBySteps(steps),
                1 => RotationAngle.QuarterTurn.TurnedBySteps(-steps),
                2 => RotationAngle.ThreeQuarterTurn.TurnedBySteps(-steps),
                _ => RotationAngle.ThreeQuarterTurn.TurnedBySteps(steps),
            };
        }

        int reciprocalSteps = OctantSteps(EightEight.FromWhole(absoluteX) / absoluteY);
        return quadrant switch
        {
            0 => RotationAngle.HalfTurn.TurnedBySteps(-reciprocalSteps),
            1 => RotationAngle.Zero.TurnedBySteps(reciprocalSteps),
            2 => RotationAngle.HalfTurn.TurnedBySteps(reciprocalSteps),
            _ => RotationAngle.Zero.TurnedBySteps(-reciprocalSteps),
        };
    }

    /// <summary>The angle steps in an eighth of a turn.</summary>
    private const int EighthTurnSteps = RotationAngle.StepsPerTurn / 8;

    /// <summary>
    /// The angle steps, 0 through 32, that a tangent of 0 through one whole, in 256ths, turns from its octant's axis: the
    /// tangent read linearly across the eighth of a turn, truncated.
    /// </summary>
    private static int OctantSteps(int tangentInTwoHundredFiftySixths) =>
        Ensure.BetweenInclusive(0, EightEight.FromWhole(1), tangentInTwoHundredFiftySixths) * EighthTurnSteps / EightEight.FromWhole(1);

    /// <summary>Reads the installed presentation offset for the current shot direction and running state.</summary>
    internal static Offset16<RoomPixels> ReadFlareOrigin(SamusGrappleState grapple, SamusProjectileDirection direction, bool running)
    {
        var offset = (grapple.FlarePlacement ?? throw new InvalidOperationException(
            "Grapple flare requires installed placement definitions."))
            .Resolve(running, SamusFlareAim.AtMuzzle(direction));
        return new Offset16<RoomPixels>(offset.X, offset.Y);
    }

    /// <summary>
    /// Completes the one-frame cancellation phase entered from firing: the stop sound, the ejection, the fallback
    /// pose of a cancelled connected pose, every firing value, and the end of an auto-cancelled selection. The equipped
    /// beams' colours return in the frame's grapple phase, after every grapple ending.
    /// </summary>
    public static GrappleMovementResult CompleteFiringCancellation(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.CancelPending)
            throw new InvalidOperationException("A grapple firing cancellation is not queued.");

        bool cancelledConnectedPose = grapple.CancelFromConnectedPose;
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Stop);
        // The held route takes the empty no-op, not a connected-pose collision exit.
        // Draygon, rather than terrain ejection, still owns Samus's coordinates.
        if (!samus.DraygonGrabbed.IsActive)
            SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        if (cancelledConnectedPose)
        {
            // The cancel runs the pose-definition fallback while the current movement type is grappling.
            // Its lookup-failure response kills all X/run momentum and the pose-definition
            // definition selects the exact standing/crouching body that existed before locking.
            SamusPoseId fallback = SamusPoseMetadata.ReadNoInputFallbackPose(samus);
            if (fallback == SamusMovementDefinitions.Poses.RetainCurrentPoseFallback)
            {
                throw new InvalidDataException(
                    $"Connected grapple pose {samus.Pose} has no cancellation fallback.");
            }

            samus.Pose = fallback;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            samus.HorizontalSpeed.BaseSpeed = default;
            samus.HorizontalSpeed.ExtraRunSpeed = default;
        }

        grapple.Phase = GrapplePhase.Inactive;
        // Cancel and kill both clear the slow-scroll flag.
        grapple.SlowScrolling = false;
        grapple.PoseChangeAutoFireTimer = 0;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.ExtensionVelocity = default(EightEightVector<RoomPixels>);
        grapple.EndpointOffset = FixedVector<RoomPixels>.Zero;
        grapple.CancelFromConnectedPose = false;
        SamusGrappleBeamDrawing.ClearFlareAnimation(grapple);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Fired: false,
            OwnsMovement: cancelledConnectedPose);
    }

    /// <summary>Held moving poses aim away from Draygon; Down wins over Up.</summary>
    private static SamusProjectileDirection ReadDraygonHeldDirection(SamusPoseId pose, ControllerButton input)
    {
        bool left = pose == SamusPoseId.DraygonGrabbedMovingLeftPose;
        bool outward = (input & (left ? ControllerButton.Left : ControllerButton.Right)) != ControllerButton.None;
        SamusProjectileDirection direction = left ? SamusProjectileDirection.Left : SamusProjectileDirection.Right;
        if (outward && (input & ControllerButton.Down) != ControllerButton.None)
            direction = left ? SamusProjectileDirection.DownLeft : SamusProjectileDirection.DownRight;
        else if (outward && (input & ControllerButton.Up) != ControllerButton.None)
            direction = left ? SamusProjectileDirection.UpLeft : SamusProjectileDirection.UpRight;
        return direction;
    }

    /// <summary>Firing branch of the grapple handler, before the selected grapple phase runs.</summary>
    internal static GrappleMovementResult? HandleFiringPoseChange(
        RoomLevelData level, SamusState samus, ControllerButton controllerInput)
    {
        var grapple = samus.Grapple;
        grapple.PoseChangeAutoFireTimer = Math.Max(0, grapple.PoseChangeAutoFireTimer - 1);
        SamusProjectileDirection? direction = SamusPoseMetadata.ReadAimDirection(samus);
        bool banned = GrappleConnections.CancelsFiring(SamusPoseMetadata.ReadMovementType(samus));
        if (!banned && direction is not null)
        {
            if (direction == grapple.FireDirection) return null;
            if (grapple.PoseChangeAutoFireTimer != 0)
            {
                // The game changes the phase before dispatch: restart from
                // the current pose and hand position this frame, without extending.
                // Do not steer an old endpoint or override the pose from held aim keys.
                QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.RestartStop);
                grapple.Phase = GrapplePhase.Inactive;
                BeginFiring(samus, controllerInput);
                return new GrappleMovementResult(GrapplePhase.Firing, Fired: true, OwnsMovement: false);
            }
        }
        // Unlike a cancellation requested inside the extension phase, this
        // decision precedes dispatch, so the cancel phase runs immediately.
        grapple.Phase = GrapplePhase.CancelPending;
        return CompleteFiringCancellation(level, samus);
    }

    /// <summary>Adds a grapple sound request to Samus's movement sound queue.</summary>
    private static void QueueGrappleSound(SamusState samus, SoundRequest request) =>
        samus.LiquidPhysics.QueueMovementSound(request.SoundEffect, request.Priority);

    /// <returns>
    /// The release pose when <paramref name="deferPoseChange"/> leaves it for the frame's
    /// pose commit; otherwise null, the pose having been applied here.
    /// </returns>
    private static SamusPoseId? CompleteQueuedRelease(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        bool deferPoseChange)
    {
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        // The release bases facing on the angular velocity retained from the swing. A
        // nonnegative value selects the left-facing forward jump; a negative value selects the right.
        // It publishes the pose as the pending transition pose, which
        // Samus's pose handling commits after this frame's hit interruption and movement.
        SamusPoseId releasePose = grapple.AngularVelocity >= 0
            ? SamusPoseId.NormalJumpForwardLeftPose
            : SamusPoseId.NormalJumpForwardRightPose;
        if (!deferPoseChange)
            ApplyReleasePose(samus, releasePose);
        grapple.Phase = GrapplePhase.Inactive;
        grapple.SlowScrolling = false; // Releasing clears the slow-scroll flag.
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.RopeLengthDelta = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        SamusGrappleBeamDrawing.ClearFlareAnimation(grapple);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
        return deferPoseChange ? releasePose : null;
    }

    /// <summary>Commits the release pose and its radius and animation.</summary>
    internal static void ApplyReleasePose(SamusState samus, SamusPoseId releasePose)
    {
        samus.Pose = releasePose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>Resets grapple-owned movement and rendering state so a later shot cannot inherit connection state.</summary>
    private static void ClearConnectedGrapple(SamusGrappleState grapple)
    {
        // The drop and wall-jump teardowns share the same long cleanup tail. Every owned movement/rendering value is
        // reset here so a later firing edge cannot inherit wall-grace or collision state; their callers end an
        // auto-cancelled selection, and the frame's grapple phase shows the equipped beams' colours again.
        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.AngularVelocity = 0;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.SpecialAngleHandling = false;
        // Dropping and wall jumping clear the slow-scroll flag.
        grapple.SlowScrolling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        SamusGrappleBeamDrawing.ClearFlareAnimation(grapple);
    }
}
