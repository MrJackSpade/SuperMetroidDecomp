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
/// The grapple's swing is not the grappling movement type's own movement: the grapple beam's step swings the
/// pendulum before the normal movement phase, and the swing's block collision advances its angle as it tests the room's
/// blocks. Firing, persistent and breakable
/// grapple blocks, enemy anchors, per-pixel rope adjustment, and the six-point angular terrain sweep are
/// here; solid/frozen-enemy wall-jump probing is shared with ordinary movement and asks the next enemy
/// pass to shake the enemy launched from. The firing-only Draygon-turret damage and the swing's spike damage, which
/// depends on the kind of spike block, are applied through the shared periodic damage.
/// Grapple's narrower water flag and persistent environment-selected
/// release are kept apart from ordinary aerial movement.
/// </remarks>
public static class SamusGrappleMovement
{
    /// <summary>
    /// Fires the grapple beam (cancelling it when it cannot fire), stopping immediately
    /// before its phase returns. The current pose supplies all direction/origin
    /// policy; no separate aiming vector is accepted.
    /// </summary>
    public static void BeginFiring(SamusState samus, GameplayAction controllerInput)
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

        // Firing selects run offsets solely for <see cref="SamusMovementType.Running"/>. Moonwalking is
        // its own movement type, so every stable/aimed moonwalk pose fires from the standing beam origin and flare,
        // with no exception for any pose.
        bool useRunOffsets = SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Running;
        var origin = GrappleFiring.Origin(direction, useRunOffsets);
        var flare = ReadFlareOrigin(grapple, direction, useRunOffsets);
        int artOrigin = movingHeld ? GrappleBeamDefinitions.Firing.DraygonMovingArtOrigin : SamusPoseMetadata.ReadArtOrigin(samus);
        int physicalYOffset = movingHeld ? GrappleBeamDefinitions.Firing.DraygonMovingArtOrigin :
            SamusPoseProjectileOrigins.ReadYOffset(samus.Pose);

        // The hand and flare offsets and the pose's offsets are a few pixels each, so their differences are ordinary.
        grapple.OriginOffset = new(origin.X, origin.Y - physicalYOffset);
        grapple.FlareOffset = new(flare.X, flare.Y - artOrigin);

        // The endpoint offset is a displacement from Samus plus the firing
        // origin. Firing clears both axes, fraction and whole alike.
        grapple.EndpointOffset = FixedVector<RoomPixels>.Zero;
        // Firing's shade and sound setup.
        grapple.ExtensionSubstep = FixedVector<RoomPixels>.Zero;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = GrappleBeamDefinitions.Physics.FiringExtension;
        grapple.AngularVelocity = AngularSpeed.Zero;
        grapple.DirectionInputAcceleration = AngularSpeed.Zero;
        grapple.GravityAcceleration = AngularSpeed.Zero;
        grapple.VelocityCorrection = AngularSpeed.Zero;
        grapple.JumpImpulse = AngularSpeed.Zero;
        grapple.CollisionBounceTimer = 0;
        grapple.ValidatedAnchor = null;
        grapple.CloseCollision = false;
        grapple.SlowScrolling = false; // Firing clears the slow-scroll flag.
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        SamusGrappleBeamDrawing.InitializeBeamAnimation(grapple);

        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Fire);
        // The grapple's firing sound, like a beam shot's, cancels a pending resume of the charging sound (#1689).
        samus.ResumeChargingBeamSoundPending = false;
        // Firing again ends the released-from-swing movement, so
        // Samus moves with ordinary physics from this same frame.
        grapple.ReleasedMovementActive = false;
        // The game has two easily conflated coordinate pairs. Start is the hand/rope
        // origin used by connection positioning; Flare is the small-sprite draw origin used
        // by the rope draw. They coincide while swinging but can differ while firing or locked.
        grapple.RopeStart = samus.Position.Whole + grapple.OriginOffset;
        grapple.FlareOrigin = samus.Position.Whole + grapple.FlareOffset;
        grapple.Anchor = samus.Position.Whole + grapple.OriginOffset;
    }

    /// <summary>
    /// Advances the extending grapple beam: its test against enemies, then its block collision.
    /// </summary>
    internal static GrappleMovementResult StepFiring(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        ISamusRoomObjects roomObjects,
        Func<Position16<RoomPixels>, GrappleEnemyCollision> enemyCollision)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Firing)
            throw new InvalidOperationException("Grapple firing is not active.");

        if (HandleFiringPoseChange(level, samus, controllerInput) is { } poseChange)
            return poseChange;

        // Finishing the connection clamps the camera's previous position after
        // the connection pose snaps Samus to the accepted rope geometry. Retain the pre-snap body
        // position even though most firing frames return without consuming this sample.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // Firing treats release of Shoot as cancellation before length or collision work.
        if ((controllerInput & GameplayAction.Shoot) == GameplayAction.None)
            return QueueFiringCancellation(grapple);

        grapple.RopeLength += GrappleBeamDefinitions.Physics.FiringExtension;
        if (grapple.RopeLength >= GrappleBeamDefinitions.Physics.FiringCancelLength)
            return QueueFiringCancellation(grapple);

        // Firing tests the current endpoint against ordinary enemies before moving the
        // beam through its four block-collision substeps. The collision producer sets the
        // enemy's common grapple-response flag; this reads only the definition's
        // seven common reactions. Kill and no interaction deliberately go on to the room's blocks, exactly like an
        // enemy with no grapple reaction.
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
                    GrappleAnchorKind.Enemy);

            case GrappleEnemyReaction.Cancel:
                return QueueFiringCancellation(grapple);

            case GrappleEnemyReaction.HurtSamus:
                ApplyGrappleEnemyDamage(
                    samus,
                    controllerInput,
                    enemy.EnemyDamage);
                return QueueFiringCancellation(grapple);
            // Kill and no interaction go on to the room's blocks, as if the enemy were not there.
            case GrappleEnemyReaction.None:
            case GrappleEnemyReaction.Kill:
                break;
            default:
                throw new InvalidOperationException($"Undefined GrappleEnemyReaction {enemy.Reaction}.");
        }

        // The rope moves its end in four equal substeps, probing after each; each keeps parts of the
        // update before's substep, so together they need not add up to the extension speed
        // (GrappleExtensionStepCarryover).
        grapple.ExtensionSubstep = new(
            GrappleExtensionStepCarryover.Step(grapple.ExtensionVelocity.X, grapple.ExtensionSubstep.X),
            GrappleExtensionStepCarryover.Step(grapple.ExtensionVelocity.Y, grapple.ExtensionSubstep.Y));
        GrappleBlockReaction finalReaction = GrappleBlockReaction.Passes;
        for (int substep = 0; substep < GrappleExtensionStepCarryover.Substeps; substep++)
        {
            grapple.EndpointOffset += grapple.ExtensionSubstep;
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
                GrappleAnchorKind.Block);
        }

        // The rope collision does not stop when an intermediate probe reports a solid block it
        // does not connect to. It runs all four substep probes and returns only the
        // fourth reaction to the firing step. At high running speed a thin solid cell can
        // therefore be sampled and then cleared before the grapple decides whether to
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
    /// Executes the post-phase update of whether the grapple is submerged.
    /// </summary>
    /// <remarks>
    /// Grapple physics reads this flag during a phase and the grapple beam's step rewrites it only
    /// afterward, so it runs exactly once at the end of the pass. Only a firing, connected or wall-grabbing beam can stay
    /// submerged: release, cancel, dropped, wall-jump, and inactive phases all clear liquid physics immediately.
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
    /// Runs one update of the connected grapple. Releasing Shoot lets go of the beam, Left and Right pump the swing, and
    /// a fresh press of Up or Down adjusts the rope's length.
    /// A release or drop leaves its pose for the frame's pose update to commit.
    /// </summary>
    internal static GrappleMovementResult Step(SamusTerrain terrain, SamusState samus, GameplayAction controllerInput, GameplayAction newlyPressedInput, Func<Position16<RoomPixels>, GrappleEnemyCollision> enemyCollision)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        if (grapple.Phase == GrapplePhase.ReleaseFromSwing)
        {
            // The release cleanup executes on the frame after the swing queued it. The launch velocity
            // was already published, so this pass changes art and movement and clears grapple.
            SamusPoseId pendingReleasePose = CompleteQueuedRelease(terrain.Level, samus, grapple);
            return new GrappleMovementResult(GrapplePhase.Inactive, PendingReleasePose: pendingReleasePose);
        }

        // These are the game's own grapple phases. Each phase owns the whole
        // grapple movement phase, so run it before entering ordinary pendulum integration.
        if (grapple.Phase == GrapplePhase.ConnectedLocked)
            return StepLocked(terrain, samus, controllerInput, enemyCollision);
        if (grapple.Phase == GrapplePhase.WallGrab)
            return StepWallGrab(terrain, samus, controllerInput, enemyCollision);
        if (grapple.Phase == GrapplePhase.WallGrabRelease)
            return StepWallGrabRelease(terrain, samus, newlyPressedInput);
        if (grapple.Phase == GrapplePhase.WallJumping)
            return CompleteGrappleWallJump(terrain.Level, samus, grapple);
        if (grapple.Phase == GrapplePhase.Dropped)
            return CompleteDropped(terrain.Level, samus, grapple);

        if (grapple.Phase != GrapplePhase.ConnectedSwinging)
            throw new InvalidOperationException("Connected grapple movement is not active.");

        // The special-angle snap clamps the camera's previous position against the position from
        // the start of this frame after a special-angle snap. Retain that exact sample now;
        // the runtime consumes the optional corrected values returned below.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // The swing first checks Shoot. Releasing it does not immediately install jumping
        // art: it computes velocity and queues the release phase for the following frame.
        if ((controllerInput & GameplayAction.Shoot) == GameplayAction.None)
        {
            if (grapple.AngularVelocity == AngularSpeed.Zero && grapple.Angle == RotationAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase);
            }

            SamusGrappleSwing.PropelSamusFromSwing(samus, grapple);
            // The release hands her movement to the released-from-swing movement immediately. The beam remains in
            // its one-frame release-cleanup phase, but movement already uses the
            // independent release movement during this same gameplay frame.
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

        // The special-angle snap checks all eight special angles only after a close collision
        // (CloseCollision). A match replaces pendulum positioning for this frame with that angle's exact
        // pose, anchor-relative body offset, and locked/wallgrab phase.
        if (grapple.CloseCollision &&
            TryHandleSpecialAngle(samus,
                grapple,
                previousPosition,
                out GrappleMovementResult special))
        {
            return special;
        }

        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorDisconnected = !enemyAnchorHeld && !IsValidatedAnchorHeld(terrain, samus, grapple);
        if (anchorDisconnected)
        {
            // A block that still stops the beam keeps the rope; one replaced with air (a breakable grapple block that
            // has crumbled), or an enemy anchor the rope has lost, ends the connection. A motionless straight-down
            // body queues the drop; a moving pendulum uses the velocity-preserving release handoff.
            if (grapple.AngularVelocity == AngularSpeed.Zero && grapple.Angle == RotationAngle.HalfTurn)
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
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        Func<Position16<RoomPixels>, GrappleEnemyCollision> enemyCollision)
    {
        SamusGrappleState grapple = samus.Grapple;

        // The locked phase does no pendulum work. It merely retains the frozen pose while Shoot
        // remains held and either an enemy or the stored block still supports the endpoint.
        bool shootHeld = (controllerInput & GameplayAction.Shoot) != GameplayAction.None;
        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorHeld = enemyAnchorHeld || IsValidatedAnchorHeld(terrain, samus, grapple);
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
    /// Whether the anchor the connected beam checks still holds this update when the enemy test retained none
    /// (<see cref="SamusGrappleState.ValidatedAnchor"/>): an enemy anchor is lost, a block anchor holds while a grapple
    /// block still stops the beam, and an anchor nothing checks always holds.
    /// </summary>
    private static bool IsValidatedAnchorHeld(SamusTerrain terrain, SamusState samus, SamusGrappleState grapple) =>
        grapple.ValidatedAnchor switch
        {
            null => true,
            GrappleAnchorKind.Block => SamusGrappleSwing.IsStillConnectedToSupportedBlock(terrain, samus, grapple),
            GrappleAnchorKind.Enemy => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(GrappleAnchorKind)} {grapple.ValidatedAnchor}."),
        };

    /// <summary>
    /// Repeats the connected beam's enemy test. Any reaction of the enemy at the anchor to the grapple beam keeps the
    /// connection alive for this frame; attach reactions additionally follow a moving actor's center.
    /// </summary>
    private static bool TryRetainEnemyAnchor(
        SamusGrappleState grapple,
        Func<Position16<RoomPixels>, GrappleEnemyCollision> enemyCollision)
    {
        GrappleEnemyCollision enemy = enemyCollision(grapple.Anchor);
        if (enemy.Reaction == GrappleEnemyReaction.None)
            return false;

        if (enemy.Attaches)
        {
            grapple.Anchor = enemy.Anchor;
            grapple.ValidatedAnchor = GrappleAnchorKind.Enemy;
        }
        return true;
    }

    /// <summary>
    /// Applies the common grapple reaction that hurts Samus. Unlike ordinary body contact, the grapple selects
    /// horizontal knockback from Samus's facing direction; suit division is shared, and the invincibility and knockback
    /// last <see cref="GrappleBeamDefinitions.Contact"/>'s updates.
    /// </summary>
    private static void ApplyGrappleEnemyDamage(
        SamusState samus,
        GameplayAction controllerInput,
        int damageBeforeSuit)
    {
        SamusSuitDamage.Hurt(samus, damageBeforeSuit);
        samus.InvincibilityTimer = WrappingCountdown.Start(GrappleBeamDefinitions.Contact.HurtInvincibilityFrames);

        // Facing left is knocked right and facing right is knocked left. Start owns the
        // authentic pose, animation, vertical speed, contact-damage cancellation, and
        // hurt-flash side effects instead of duplicating those writes here.
        KnockbackHorizontalDirection knockbackXDirection = SamusPoseMetadata.IsFacingLeft(samus.Pose)
            ? KnockbackHorizontalDirection.Right
            : KnockbackHorizontalDirection.Left;
        SamusKnockbackMovement.Start(samus, controllerInput, new KnockbackLaunch { KnockbackXDirection = knockbackXDirection, KnockbackTimer = GrappleBeamDefinitions.Contact.HurtKnockbackFrames },
            poseCollision: null);
    }

    /// <summary>Keeps the wall-contact pose attached until Shoot or anchor support ends, then opens the wall-jump window.</summary>
    private static GrappleMovementResult StepWallGrab(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        Func<Position16<RoomPixels>, GrappleEnemyCollision> enemyCollision)
    {
        SamusGrappleState grapple = samus.Grapple;
        bool shootHeld = (controllerInput & GameplayAction.Shoot) != GameplayAction.None;
        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorHeld = enemyAnchorHeld || IsValidatedAnchorHeld(terrain, samus, grapple);
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // The wall-grab opens the window and changes only the grapple phase. The contact
        // pose and beam remain visible throughout the following grace-window probes.
        grapple.WallJumpTimer = GrappleBeamDefinitions.WallGrab.WallJumpWindowFrames;
        grapple.Phase = GrapplePhase.WallGrabRelease;
        return new GrappleMovementResult(
            grapple.Phase);
    }

    /// <summary>During the release window, accepts a fresh Jump only when a 16-pixel probe finds a wall or solid enemy.</summary>
    private static GrappleMovementResult StepWallGrabRelease(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction newlyPressedInput)
    {
        SamusGrappleState grapple = samus.Grapple;

        // The window gives thirty eligible checks. The update that finds none left queues dropped and deliberately
        // performs no wall probe.
        if (grapple.WallJumpTimer == 0)
        {
            grapple.Phase = GrapplePhase.Dropped;
            return new GrappleMovementResult(
                grapple.Phase);
        }

        grapple.WallJumpTimer--;

        // Each eligible check first withdraws an earlier launch's enemy shake.
        samus.Kinematics.WithdrawWallJumpLaunchShake();

        // The wall-jump probe tests solid/frozen enemies first, then 16 pixels of room blocks toward
        // the wall named by the contact pose's facing. The shared probe preserves
        // square slopes, block behaviour, available distance, and enemy identity.
        SignedSixteenSixteen signedProbe = SignedSixteenSixteen.FromParts(
            SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) ? -GrappleBeamDefinitions.WallGrab.WallJumpProbePixels : GrappleBeamDefinitions.WallGrab.WallJumpProbePixels, 0);
        // A bomb block the probe breaks starts its reaction through the room's objects, as any other contact does (#1582).
        BlockMoveResult wall = SamusBlockCollision.ProbeWallHorizontal(
            terrain.Level,
            samus.Kinematics, samus, samus.Pose,
            signedProbe,
            terrain.RoomObjects);
        bool jumpNew = (newlyPressedInput & GameplayAction.Jump) != GameplayAction.None;
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
        // phase handoff. The wall-jump phase keeps the peculiar reversal of the left
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

    /// <summary>Ends a dropped connection, clears movement speeds, and leaves the fall pose for the frame's pose update.</summary>
    private static GrappleMovementResult CompleteDropped(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        SamusPoseId targetPose = SelectDroppedPose(samus);
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        // The drop clears these speeds now, but publishes its pose as the pending pose
        // change. Normal movement still runs before that pose is committed.
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;
        bool emptyMovement = SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Grappling;
        ClearConnectedGrapple(grapple);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            OwnsMovement: emptyMovement,
            PendingDropPose: targetPose);
    }

    /// <summary>Chooses the fall pose from shot direction and body radius, with standing exceptions for both swinging poses.</summary>
    internal static SamusPoseId SelectDroppedPose(SamusState samus)
    {
        // The swinging poses ignore the shot direction and always fall to ordinary
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
            return samus.Kinematics.Radius.Y >= GrappleBeamDefinitions.Connection.StandingDropRadius
                ? facingLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose
                : facingLeft ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose;
        }

        return GrappleConnections.DroppedPose(shotDirection, compact: samus.Kinematics.Radius.Y < GrappleBeamDefinitions.Connection.StandingDropRadius);
    }

    /// <summary>Applies an exact special-angle connection, including its pose and bounded camera displacement.</summary>
    internal static bool TryHandleSpecialAngle(
        SamusState samus,
        SamusGrappleState grapple,
        Position16<RoomPixels> previousPosition,
        out GrappleMovementResult result)
    {
        // The connections are scanned last to first, and an angle matches only exactly, with no
        // tolerance around a wall-grab angle. These connections govern collision positioning.
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
            SamusPresentation.InitializeAnimation(samus);
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

    /// <summary>
    /// Limits one axis of the previous position to twelve pixels from the current one. Both are Samus's positions in the
    /// room, before and after the snap, so a limited previous position lies between them.
    /// </summary>
    private static int ClampPreviousAxis(int current, int previous)
    {
        int displacement = current - previous;
        if (displacement > GrappleBeamDefinitions.Connection.MostSnapPreviousLag)
            return current - GrappleBeamDefinitions.Connection.MostSnapPreviousLag;
        if (displacement < -GrappleBeamDefinitions.Connection.MostSnapPreviousLag)
            return current + GrappleBeamDefinitions.Connection.MostSnapPreviousLag;
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
        // The rope start and the flare sit at whole-pixel offsets from Samus's whole position. The endpoint offsets are
        // displacements either way, so a left or upward beam keeps its direction as its fraction accumulates.
        grapple.RopeStart = samus.Position.Whole + grapple.OriginOffset;
        // The endpoint adds the offset to Samus's subpixel-inclusive position, so the
        // fraction's overflow reaches the endpoint before the whole origin offset is added.
        grapple.Anchor = samus.Position.Advance(grapple.EndpointOffset).Whole + grapple.OriginOffset;
        grapple.FlareOrigin = samus.Position.Whole + grapple.FlareOffset;
    }

    /// <summary>
    /// Resolves the endpoint block and extensions, including room-object effects, into what the block does to the beam:
    /// lets it pass, stops it, or connects it.
    /// </summary>
    internal static GrappleBlockReaction ReactAtEndpoint(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> endpoint,
        ISamusRoomObjects roomObjects)
    {
        RoomCollisionBlock block = level.GetCollisionBlockOrEdgeSolid(endpoint.ToRoomBlock());
        if (block.Position is null)
        {
            // The edge-solid block outside the room reacts as an ordinary solid endpoint: it stops
            // the beam without connecting, which queues grapple cancellation.
            return GrappleBlockReaction.Solid;
        }

        // Each extension link leads on in the room's reading order; one that leads outside the grid reads the edge-solid block.
        for (int extensionDepth = 0; extensionDepth <= GrappleBeamDefinitions.Blocks.LongestExtensionChain; extensionDepth++)
        {
            switch (block.CollisionType)
            {
                // These four block kinds let the beam pass: it remains live.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.UnrevealedAir:
                    return GrappleBlockReaction.Passes;

                // Slopes and these solid kinds stop the beam without connecting,
                // which makes the firing step queue cancellation rather than establish a rope.
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
                        samus.PeriodicDamage.Accumulate(new UnsignedSixteenSixteen(whole: 1, fraction: 0));
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
                    if (block.Behavior.Grapple is GrappleReaction.RespawningBreakableAnchor or GrappleReaction.PermanentBreakableAnchor)
                    {
                        if (!roomObjects.BlockReactions.TrySpawnBreakableGrappleBlock(level, block.At, block.Behavior))
                        {
                            throw new InvalidOperationException(
                                "Every room object is in use during grapple acquisition.");
                        }

                        return GrappleBlockReaction.Connects;
                    }

                    // A grapple block with no grapple reaction would select none of the four grapple room
                    // objects.
                    throw new InvalidDataException(
                        $"Invalid grapple block behaviour ({block.Behavior}) at {block.Position}.");

                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.ShootableBlock:
                    // The shootable block reactions are the same as ordinary
                    // projectiles'. Grapple owns no projectile family, so weapon-gated
                    // reactions reject it while the eight any-weapon breaks retain their unconditional block
                    // animation. The block's collision kind, not the room-object setup, decides whether
                    // the beam stops: shootable air lets it pass and a shootable block stops it.
                    // Blue caps react like breakable blocks but
                    // spawn door room objects. Grapple, which is no shot, bypasses the
                    // blue door setup's power-bomb rejection; the ordinary opening owner
                    // supplies all four cap blocks, timing, collision changes, and sound.
                    if (block.Behavior.TryGetDownwardGateTrigger(out _))
                        roomObjects.Gates.TrySpawnDownwardGateTrigger(level, block.At, block.Behavior, projectileType: SamusProjectileType.None);
                    else if (block.Behavior.TryGetBlueDoorOrientation(out _))
                        roomObjects.DoorObjects.TrySpawnBlueDoorOpening(level, block.At, block.Behavior, projectileType: SamusProjectileType.None);
                    else if (block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
                    {
                        // A shot-triggered block also reacts to the grapple: its resident actor hears of a hit with no
                        // weapon, and decides itself whether to refuse the hit or wake up.
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
                                $"Grapple shootable reaction type {block.CollisionType}/behaviour ({block.Behavior}) " +
                                $"at block {block.Position} has no grapple reaction.",
                                error);
                        }
                    }
                    return block.CollisionType == RoomCollisionType.ShootableBlock
                        ? GrappleBlockReaction.Solid
                        : GrappleBlockReaction.Passes;

                case RoomCollisionType.BombableAir:
                    // The bombable-air setup immediately deletes its provisional
                    // room object, because the grapple is no projectile. The beam passes.
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

    /// <summary>
    /// Commits attachment, selecting the movement-specific pose and either locked or swinging handling; the pose itself
    /// is left pending for the frame's pose update (<see cref="ApplyPendingConnectionPose"/>).
    /// </summary>
    internal static GrappleMovementResult ConnectAcceptedFiringCore(
        SamusState samus,
        SamusGrappleState grapple,
        Position16<RoomPixels> previousPosition,
        GrappleAnchorKind anchor)
    {
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Attach);
        // While Draygon holds Samus the connection leaves her pose and position alone: only the beam locks, and
        // Draygon keeps placing her.
        SamusMovementType sourceMovementType = SamusPoseMetadata.ReadMovementType(samus);
        if (sourceMovementType == SamusMovementType.DraygonHeld)
        {
            grapple.Phase = GrapplePhase.ConnectedLocked;
            grapple.ValidatedAnchor = anchor;
            grapple.CancelFromConnectedPose = false;
            // Connecting clears the delta, then the firing immediately sets it to -8.
            grapple.RopeLengthDelta = GrappleBeamDefinitions.Physics.InitialConnectionRetraction;
            return new GrappleMovementResult(grapple.Phase, OwnsMovement: false);
        }

        bool movingVertically = !samus.Kinematics.YSpeed.IsZero;
        GrappleConnectionPolicy policy = movingVertically
            ? GrappleConnectionPolicy.MovingVertically
            : sourceMovementType == SamusMovementType.Crouching
                ? GrappleConnectionPolicy.Crouching
                : GrappleConnectionPolicy.Default;
        var connection = GrappleConnections.ResolveConnection(policy, grapple.FireDirection);
        // The connection names the pose it installs; it swings exactly when its kind does.
        SamusPoseId pose = connection.Pose;
        bool swinging = connection.Kind == GrappleConnectionKind.Swinging;

        // Both the connection and the fired beam derive the angle from the body position that fired the beam,
        // before the connection pose is installed and snaps Samus to the rope. The angle is stored
        // as a whole step of the 256-step turn; its fraction becomes zero.
        RotationAngle angle = CalculateAngleFromXY(samus.Position.Whole - grapple.Anchor);
        grapple.Angle = angle;
        grapple.MirroredAngle = grapple.Angle;
        grapple.AngularVelocity = AngularSpeed.Zero;
        grapple.RopeLengthDelta = 0;
        if (grapple.RopeLength >= GrappleBeamDefinitions.Connection.LongRope)
            grapple.RopeLength -= GrappleBeamDefinitions.Connection.LongRopeShortening;

        // The connection publishes the grapple-beam Start pair from the accepted endpoint,
        // angle, and possibly shortened rope. This is the hand attachment point, not the
        // flare position authored separately for each firing direction.
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
        grapple.ValidatedAnchor = anchor;
        grapple.CloseCollision = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;

        // Connecting clears the delta while choosing the new pose, but the firing
        // immediately starts automatic retraction. Omitting that retraction leaves Samus
        // dangling unless the player explicitly supplies a fresh Up edge. Apply it for
        // both block and enemy connections, never for an arbitrary restored swing state.
        grapple.RopeLengthDelta = GrappleBeamDefinitions.Physics.InitialConnectionRetraction;

        // A swinging or locked connection publishes the pending connection pose. It does not
        // install that pose at once. The animation's pending transition pose can therefore keep
        // the old pose and every one of Samus's speeds while the grapple phase remains live.
        return new GrappleMovementResult(
            grapple.Phase,
            OwnsMovement: false,
            PendingConnection: new GrapplePendingConnection(
                pose,
                swinging,
                previousPosition));
    }

    /// <summary>
    /// Applies the accepted grapple connection pose, when none of the frame's interruptions that outrank it changed
    /// Samus's pose.
    /// </summary>
    internal static Position16<RoomPixels> ApplyPendingConnectionPose(
        SamusState samus,
        GrapplePendingConnection pending)
    {
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        samus.Pose = pending.Pose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);

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
            // Start minus the no-run firing origin, then Flare is independently rebuilt
            // from the no-run flare origin. Graphics-Y correction does not participate.
            var origin = GrappleFiring.Origin(grapple.FireDirection, running: false);
            var flare = ReadFlareOrigin(grapple, grapple.FireDirection, running: false);
            samus.Position = samus.Position.WithWhole(grapple.RopeStart - origin);
            grapple.FlareOrigin = samus.Position.Whole + flare;
            grapple.Phase = GrapplePhase.ConnectedLocked;
        }

        // A swinging and a locked connection finish connecting the same way: the speed boost is cancelled, the echoes
        // leaving the way the connection pose faces, then the base, extra-run and vertical speeds are cleared.
        // AccelerationMode is kept: connecting does not change it.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.Kinematics.YSpeed = default;

        return ClampPreviousPosition(samus.Position.Whole, pending.Previous);
    }

    /// <summary>
    /// The direction of <paramref name="delta"/> as an angle, worked out in whole numbers one octant at a time.
    /// The returned angle is measured clockwise from negative Y: zero up, 64 right, 128 down,
    /// and 192 left. Each octant's steps are truncated toward zero, as the game's are.
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

        // A mostly horizontal offset turns from the quarter turn (rightward) or three-quarter turn (leftward), forward when
        // both components lean the same way; a mostly vertical one turns from the half turn (downward) or zero (upward),
        // forward when they lean opposite ways.
        bool leanSameWay = xNegative == yNegative;
        if (absoluteY < absoluteX)
        {
            int steps = OctantSteps(absoluteY, absoluteX);
            RotationAngle horizontalAxis = xNegative ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn;
            return horizontalAxis.TurnedBySteps(leanSameWay ? steps : -steps);
        }

        int reciprocalSteps = OctantSteps(absoluteX, absoluteY);
        RotationAngle verticalAxis = yNegative ? RotationAngle.Zero : RotationAngle.HalfTurn;
        return verticalAxis.TurnedBySteps(leanSameWay ? -reciprocalSteps : reciprocalSteps);
    }

    /// <summary>The angle steps in an eighth of a turn.</summary>
    private const int EighthTurnSteps = RotationAngle.StepsPerTurn / 8;

    /// <summary>
    /// The angle steps, 0 through 32, that an offset of <paramref name="minor"/> across and <paramref name="major"/> along
    /// its octant's axis turns from that axis: the tangent, the minor part over the major, read linearly across the
    /// eighth of a turn and truncated. Truncating the tangent to 256ths before scaling it to the eighth of a turn's 32
    /// steps truncates the same as this one division.
    /// </summary>
    /// <param name="minor">The offset across the axis, 0 through <paramref name="major"/>.</param>
    /// <param name="major">The offset along the axis, at least one.</param>
    private static int OctantSteps(int minor, int major) =>
        Ensure.BetweenInclusive(0, Ensure.BetweenInclusive(1, int.MaxValue, major), minor) * EighthTurnSteps / major;

    /// <summary>Reads the installed presentation offset for the current shot direction and running state.</summary>
    internal static Offset16<RoomPixels> ReadFlareOrigin(SamusGrappleState grapple, SamusProjectileDirection direction, bool running) =>
        grapple.Artwork.GrappleFlarePlacement.Resolve(running, SamusFlareAim.AtMuzzle(direction));

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
        // The held route does nothing here, rather than leave by a connected-pose collision.
        // Draygon, rather than terrain ejection, still owns Samus's coordinates.
        if (!samus.DraygonGrabbed.IsActive)
            SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        if (cancelledConnectedPose)
        {
            // Cancelling from a connected pose takes the pose's no-input fallback while Samus still grapples: it
            // stops all her horizontal and running speed and returns her to the standing or crouching pose she
            // held before the beam locked.
            if (!SamusPoseMetadata.ReadNoInputFallback(samus).TryGetPose(out SamusPoseId fallback))
            {
                throw new InvalidDataException(
                    $"Connected grapple pose {samus.Pose} has no cancellation fallback.");
            }

            samus.Pose = fallback;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
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
    private static SamusProjectileDirection ReadDraygonHeldDirection(SamusPoseId pose, GameplayAction input)
    {
        bool left = pose == SamusPoseId.DraygonGrabbedMovingLeftPose;
        bool outward = (input & (left ? GameplayAction.Left : GameplayAction.Right)) != GameplayAction.None;
        SamusProjectileDirection direction = left ? SamusProjectileDirection.Left : SamusProjectileDirection.Right;
        if (outward && (input & GameplayAction.Down) != GameplayAction.None)
            direction = left ? SamusProjectileDirection.DownLeft : SamusProjectileDirection.DownRight;
        else if (outward && (input & GameplayAction.Up) != GameplayAction.None)
            direction = left ? SamusProjectileDirection.UpLeft : SamusProjectileDirection.UpRight;
        return direction;
    }

    /// <summary>Changes Samus's pose as she fires the grapple beam, before the grapple's phase runs.</summary>
    internal static GrappleMovementResult? HandleFiringPoseChange(
        RoomLevelData level, SamusState samus, GameplayAction controllerInput)
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
                // The game changes the phase before running it: restart from
                // the current pose and hand position this frame, without extending.
                // Do not steer an old endpoint or override the pose from held aim keys.
                QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.RestartStop);
                grapple.Phase = GrapplePhase.Inactive;
                BeginFiring(samus, controllerInput);
                return new GrappleMovementResult(GrapplePhase.Firing, Fired: true, OwnsMovement: false);
            }
        }
        // Unlike a cancellation requested inside the extension phase, this
        // decision precedes running the phase, so the cancel phase runs immediately.
        grapple.Phase = GrapplePhase.CancelPending;
        return CompleteFiringCancellation(level, samus);
    }

    /// <summary>Adds a grapple sound request to Samus's audio requests.</summary>
    private static void QueueGrappleSound(SamusState samus, SoundRequest request) =>
        samus.AudioRequests.QueueSound(request.SoundEffect, request.Priority);

    /// <returns>The release pose, which the frame's pose update commits (<see cref="ApplyReleasePose"/>).</returns>
    private static SamusPoseId CompleteQueuedRelease(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        // The release bases facing on the angular velocity retained from the swing. A
        // nonnegative value selects the left-facing forward jump; a negative value selects the right.
        // It publishes the pose as the pending transition pose, which
        // Samus's pose handling commits after this frame's hit interruption and movement.
        SamusPoseId releasePose = !grapple.AngularVelocity.IsNegative
            ? SamusPoseId.NormalJumpForwardLeftPose
            : SamusPoseId.NormalJumpForwardRightPose;
        grapple.Phase = GrapplePhase.Inactive;
        grapple.SlowScrolling = false; // Releasing clears the slow-scroll flag.
        grapple.DirectionInputAcceleration = AngularSpeed.Zero;
        grapple.GravityAcceleration = AngularSpeed.Zero;
        grapple.VelocityCorrection = AngularSpeed.Zero;
        grapple.JumpImpulse = AngularSpeed.Zero;
        grapple.RopeLengthDelta = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.ValidatedAnchor = null;
        SamusGrappleBeamDrawing.ClearFlareAnimation(grapple);
        SamusHudSelection.ReturnAutoCancelledItem(samus);
        return releasePose;
    }

    /// <summary>Commits the release pose and its radius and animation.</summary>
    internal static void ApplyReleasePose(SamusState samus, SamusPoseId releasePose)
    {
        samus.Pose = releasePose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>Resets grapple-owned movement and rendering state so a later shot cannot inherit connection state.</summary>
    private static void ClearConnectedGrapple(SamusGrappleState grapple)
    {
        // The drop and wall-jump teardowns end with the same long cleanup. Every owned movement/rendering value is
        // reset here so a later firing edge cannot inherit wall-grace or collision state; both teardowns end an
        // auto-cancelled selection, and the frame's grapple phase shows the equipped beams' colours again.
        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.AngularVelocity = AngularSpeed.Zero;
        grapple.DirectionInputAcceleration = AngularSpeed.Zero;
        grapple.GravityAcceleration = AngularSpeed.Zero;
        grapple.VelocityCorrection = AngularSpeed.Zero;
        grapple.JumpImpulse = AngularSpeed.Zero;
        grapple.CollisionBounceTimer = 0;
        grapple.CloseCollision = false;
        // Dropping and wall jumping clear the slow-scroll flag.
        grapple.SlowScrolling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        grapple.ValidatedAnchor = null;
        SamusGrappleBeamDrawing.ClearFlareAnimation(grapple);
    }
}
