using SuperMetroid.Core.Samus.Drawing;
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
/// The grapple beam's handler updates the pendulum before normal beta
/// movement, and the block collision advances its angle while probing room collision. This class keeps
/// those responsibilities separate from ordinary aerial movement. Firing, persistent grapple
/// blocks, per-pixel rope adjustment, and the six-point angular terrain sweep are modelled
/// here; solid/frozen-enemy wall-jump probing is shared with ordinary movement and retains
/// the enemy slot to shake. The firing-only Draygon-turret damage and both behaviour-indexed swing
/// spike-damage tables are applied through the shared periodic damage. Breakable
/// block acquisition now installs the independent room-object lifecycle; enemy acquisition
/// and the live enemy/shake loop remain explicit later routes rather than being approximated.
/// Grapple's narrower water flag and persistent environment-selected
/// release handler are modelled rather than collapsed into ordinary aerial movement.
/// </remarks>
public static class SamusGrappleMovement
{
    /// <summary>
    /// Fires the grapple beam (cancelling it when it cannot fire), stopping immediately
    /// before its function's return. The current pose supplies all direction/origin
    /// policy; no host aiming vector is accepted.
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

        // The endpoint offset is a signed 16.16 displacement from Samus plus the origin
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

    /// <summary>The 16.16 steps of a quarter of one 8.8 velocity step: an 8.8 velocity's quarter is the velocity times this.</summary>
    private const int QuarterOfVelocityStep = 64;

    /// <summary>The coarse whole pixels the previous quarter leaks a part of: its whole pixels in multiples of 256.</summary>
    private const int CoarseWholePixels = 256;

    /// <summary>The fine fraction steps the previous quarter leaks a part of: its fraction below 1/256 of a pixel.</summary>
    private const int FineFractionSteps = 256;

    /// <summary>The whole pixels each of the previous quarter's coarse 256-pixel steps leaks as.</summary>
    private const int LeakedWholePerCoarseStep = 64;

    /// <summary>The fine fraction steps that leak as one fraction step of the new quarter.</summary>
    private const int FineStepsPerLeakedStep = 4;

    /// <summary>The whole pixels an odd previous fraction moves a rising quarter by: half the whole range, downward.</summary>
    private const int OddFractionWholeShift = 32768;

    /// <summary>
    /// The rope's extension step for one of its four substeps, on one axis: a quarter of
    /// <paramref name="velocity"/>, rounded toward negative infinity, into which part of last update's quarter
    /// <paramref name="previous"/> leaks. The quarter's fraction always keeps the top six of the previous fraction's eight
    /// finest bits below its own, the previous fraction's 256ths quartered. A velocity that is not negative also takes the
    /// previous whole pixels' multiples of 256, each as 64 pixels, and an odd previous fraction moves its whole pixels
    /// half the range down; a negative velocity's quarter keeps only its own whole pixels.
    /// </summary>
    /// <remarks>
    /// The rope keeps the four substeps' quarter in the storage that held last update's, and builds it there without
    /// clearing what the velocity does not cover, so the substeps need not add up to the update's velocity.
    /// </remarks>
    private static SignedSixteenSixteen QuarterExtensionVelocity(
        SignedEightEight velocity, SignedSixteenSixteen previous)
    {
        SignedSixteenSixteen quarter = velocity.ScaledBy(QuarterOfVelocityStep);
        int whole = quarter.WholePixels;
        if (!velocity.IsNegative)
        {
            whole += previous.UnsignedWholePixels / CoarseWholePixels * LeakedWholePerCoarseStep;
            if (previous.Fraction % 2 != 0)
                whole -= OddFractionWholeShift;
        }

        int fraction = quarter.Fraction + previous.Fraction % FineFractionSteps / FineStepsPerLeakedStep;
        return SignedSixteenSixteen.FromParts(whole, fraction);
    }

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

        // The common pose-command tail clamps the camera's previous position after
        // command 9/10 snaps Samus to the accepted rope geometry. Retain the pre-snap body
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
        // terrain, exactly like the zero return from their reaction functions.
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

        // The rope collision quarters each velocity and performs four identical probes. The quarter
        // keeps stale parts of the previous frame's value, so the four steps need not add up to
        // exactly one authored per-frame velocity.
        grapple.QuarterVelocity = new(
            QuarterExtensionVelocity(grapple.ExtensionVelocity.X, grapple.QuarterVelocity.X),
            QuarterExtensionVelocity(grapple.ExtensionVelocity.Y, grapple.QuarterVelocity.Y));
        GrappleBlockReaction finalReaction = GrappleBlockReaction.Passes;
        for (int substep = 0; substep < 4; substep++)
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
            grapple.Anchor = new(
                CoordinateAxis.Wrap((grapple.Anchor.X & ~15) | 8),
                CoordinateAxis.Wrap((grapple.Anchor.Y & ~15) | 8));
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
    /// Executes the post-function liquid-bit update.
    /// </summary>
    /// <remarks>
    /// Grapple physics reads this bit during a function and the handler rewrites it only
    /// afterward, so callers invoke this exactly once at the end of the pass. The
    /// explicit phase set mirrors the function-range test: release, cancel, dropped,
    /// wall-jump, and inactive functions all clear liquid physics immediately.
    /// </remarks>
    public static void RefreshLiquidPhysicsFlag(SamusState samus)
    {
        Ensure.NotNull(samus);
        bool functionCanRetainLiquid = samus.Grapple.Phase is
            GrapplePhase.Firing or GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease;
        samus.Grapple.Submerged = functionCanRetainLiquid &&
            samus.LiquidPhysics.DetermineGrappleSubmersion(samus);
    }

    /// <summary>
    /// Executes one connected grapple-function call. X is the default shoot binding used by
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

        // These are literal function phases. Each handler owns the whole
        // grapple beta pass, so dispatch it before entering ordinary pendulum integration.
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
        // the host runtime consumes the optional corrected values returned below.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // The swing first checks Shoot. Releasing it does not immediately install jumping
        // art: it computes velocity and queues the release function for the following frame.
        if ((controllerInput & ControllerButton.X) == ControllerButton.None)
        {
            if (grapple.AngularVelocity == 0 && grapple.Angle == RotationAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase);
            }

            SamusGrappleSwing.PropelSamusFromSwing(samus, grapple);
            // The release installs the released-from-swing movement handler immediately. The beam function remains in
            // its one-frame release-cleanup phase, but beta movement already uses the
            // independent release handler during this same gameplay frame.
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(grapple.Phase);
        }

        SamusGrappleSwing.ApplyRopeAndDirectionInput(grapple, controllerInput, newlyPressedInput);
        bool ropeLengthBlocked = SamusGrappleSwing.ApplyRopeLengthDelta(terrain.Level, samus, grapple);
        SamusGrappleSwing.CalculateGravity(grapple);
        SamusGrappleSwing.IntegrateAngularVelocity(grapple);
        SamusGrappleSwing.ApplyJumpImpulse(grapple, newlyPressedInput);
        GrappleSwingCollisionResult swingCollision = SamusGrappleSwing.AdvanceAngleWithTerrainCollision(
            terrain.Level,
            samus,
            grapple);

        // The special-angle snap scans all eight records only when the collision raised the close-body
        // flag. A match replaces pendulum positioning for this frame with the table's exact
        // pose, anchor-relative body offset, and locked/wallgrab function.
        if (grapple.SpecialAngleHandling &&
            TryHandleSpecialAngle(samus,
                grapple,
                previousPosition,
                out GrappleMovementResult special))
        {
            return special with
            {
                TerrainCollided = swingCollision.Collided,
                CollisionDistanceFromFeet = swingCollision.DistanceFromFeet,
                RopeLengthBlocked = ropeLengthBlocked,
            };
        }

        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorDisconnected = !enemyAnchorHeld &&
            (grapple.ValidateAnchorEnemy ||
             grapple.ValidateAnchorBlock &&
                !SamusGrappleSwing.IsStillConnectedToSupportedBlock(terrain.Level, samus, grapple));
        if (anchorDisconnected)
        {
            // The persistent block path normally remains connected forever. Retain the
            // validation seam so a future dynamic/breakable room object cannot leave a rope
            // attached to air. A motionless straight-down body queues the drop; a moving
            // pendulum uses the velocity-preserving release handoff.
            if (grapple.AngularVelocity == 0 && grapple.Angle == RotationAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase,
                    TerrainCollided: swingCollision.Collided,
                    CollisionDistanceFromFeet: swingCollision.DistanceFromFeet,
                    RopeLengthBlocked: ropeLengthBlocked);
            }

            SamusGrappleSwing.PropelSamusFromSwing(samus, grapple);
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(
                grapple.Phase,
                TerrainCollided: swingCollision.Collided,
                CollisionDistanceFromFeet: swingCollision.DistanceFromFeet,
                RopeLengthBlocked: ropeLengthBlocked);
        }

        SamusGrappleSwing.PositionSamusFromPendulum(samus, grapple);

        return new GrappleMovementResult(
            grapple.Phase,
            TerrainCollided: swingCollision.Collided,
            CollisionDistanceFromFeet: swingCollision.DistanceFromFeet,
            RopeLengthBlocked: ropeLengthBlocked);
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
        // body must own this beta pass until its pose-definition fallback is committed.
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

        // The wall-grab writes decimal 30 and changes only the grapple function. The contact
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
            if (wall.EnemyCollision is { EnemySlot: int enemySlot })
            {
                // The wall jump requests the contacted enemy's shake only when the
                // fresh Jump edge actually accepts the grapple wall jump.
                samus.EnemySlotToShake = enemySlot;
            }
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
        // The wall-jump function runs one frame after the grace check selected it, matching every other grapple
        // function handoff. The Samus helper keeps the peculiar reversal of the left
        // wall-contact pose into the left wall jump, and the right into the right, separate from
        // ordinary spin-contact wall jumps.
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        SamusAerialPoseTransitions.ApplyGrappleWallJump(samus);
        ClearConnectedGrapple(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            WallJumpStarted: true);
    }

    /// <summary>Ends a dropped connection, clears movement speeds, and applies or defers the fall pose for the beta pass.</summary>
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
        // slot. Normal beta movement still runs before that pose is committed.
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;
        bool emptyBeta = SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Grappling;
        if (!deferPoseChange)
            SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus, level, targetPose, frameCounter, roomObjects);
        ClearConnectedGrapple(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            OwnsMovement: !deferPoseChange || emptyBeta,
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
            // The drop functions use the pose's facing when the pose aims nowhere.
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

            GrappleConnectionFunction function = special.Function;
            GrapplePhase phase = function switch
            {
                GrappleConnectionFunction.LockedInPlace => GrapplePhase.ConnectedLocked,
                GrappleConnectionFunction.WallGrab => GrapplePhase.WallGrab,
                GrappleConnectionFunction.Swinging => throw new InvalidDataException(
                    $"Grapple special connection {special} names the swinging function."),
                _ => throw new InvalidOperationException($"Undefined GrappleConnectionFunction {function}."),
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
        // Whole endpoint offsets are the signed whole parts of the two 16.16 sums. The signed halving is
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
        int blockX = endpoint.X >> 4;
        int blockY = endpoint.Y >> 4;
        RoomCollisionBlock block =
            level.GetCollisionBlockOrEdgeSolid(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
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
                // These four dispatcher entries let the beam pass: it remains live.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.UnusedAir:
                    return GrappleBlockReaction.Passes;

                // Slopes and these solid-family entries stop the beam without connecting,
                // which makes the firing handler queue cancellation rather than establish a rope.
                case RoomCollisionType.Slope:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpecialBlock:
                    return GrappleBlockReaction.Solid;

                case RoomCollisionType.SpikeBlock:
                    // The grapple block reaction selects a grapple-reaction room object by the low seven
                    // behaviour bits. A negative behaviour rejects the beam. Every ordinary entry uses
                    // the empty reaction, which stops the beam without latching, and immediately deletes; behaviour
                    // three alone uses Draygon's broken-turret setup, which adds
                    // one whole periodic-damage unit and latches the beam.
                    if (block.Behavior.Reaction.IsAreaSpecific)
                        return GrappleBlockReaction.Passes;
                    if (block.Behavior.Reaction == GrappleBlockReactionDefinitions.BrokenTurretSpike)
                    {
                        samus.LiquidPhysics.AccumulatePeriodicDamage(
                            subDamage: 0,
                            wholeDamage: 1);
                        return GrappleBlockReaction.Connects;
                    }

                    return GrappleBlockReaction.Solid;

                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                    // Horizontal extensions lead their signed behaviour in blocks along the rows, vertical ones in rows.
                    // behaviour zero is the terminator and behaves as air.
                    if (block.Behavior.EndsExtension)
                        return GrappleBlockReaction.Passes;
                    block = level.LinkedBlockOrEdgeSolid(block);
                    continue;

                case RoomCollisionType.GrappleBlock:
                    // The reaction rejects bit-seven behaviour. Values zero and three spawn the persistent
                    // grapple room object, whose setup connects the beam.
                    // The persistent room object has no level mutation, so this result is complete.
                    if (block.Behavior.Reaction.IsAreaSpecific)
                        return GrappleBlockReaction.Passes;
                    if (GrappleBlockReactionDefinitions.PersistentAnchors.Contains(block.Behavior.Reaction))
                        return GrappleBlockReaction.Connects;

                    // behaviour one/two spawn the breakable grapple room objects. Their setup synchronously saves the
                    // complete terrain block and clears the behaviour before connecting the beam; the new room object
                    // executes its first timer/draw record later in this same gameplay frame.
                    // A caller which omitted the independent room owner cannot honestly
                    // preserve that lifecycle, so keep the missing integration seam explicit.
                    if (block.Behavior.Reaction == GrappleBlockReactionDefinitions.RespawningBreakableAnchor ||
                        block.Behavior.Reaction == GrappleBlockReactionDefinitions.PermanentBreakableAnchor)
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

                    // Nonnegative behaviour values beyond the four authored grapple reactions
                    // would index outside the four grapple room objects in the selection table.
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
                    // supplies all four cap tiles, timing, collision changes, and sound.
                    if (block.Behavior.TryGetDownwardGateTrigger(out _))
                        roomObjects.Gates.TrySpawnDownwardGateTrigger(level, block.At, block.Behavior, projectileType: SamusProjectileType.None);
                    else if (block.Behavior.TryGetBlueDoorOrientation(out _))
                        roomObjects.DoorObjects.TrySpawnBlueDoorOpening(level, block.At, block.Behavior, projectileType: SamusProjectileType.None);
                    else if (block.Behavior.Reaction == BlockReactionDefinitions.ResidentObjectTrigger)
                    {
                        // Generic shot-trigger setup also belongs to the grapple table.
                        // Notify the resident actor with no weapon family; its own
                        // frame behaviour decides whether to reject the hit or wake up.
                        if (!roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(block.At, projectileType: SamusProjectileType.None))
                            throw new InvalidOperationException(
                                $"Grapple trigger block {block.Position} has no resident room-object owner.");
                    }
                    else if (block.Behavior.Reaction == BlockReactionDefinitions.CollectibleTrigger)
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
                                $"grapple-shootable/type-{block.CollisionType}/behaviour-{block.Behavior.Reaction}",
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
                        $"sixteen-entry dispatcher at {block.Position}.");
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
        GrappleConnectionTable connectionTable = movingVertically
            ? GrappleConnectionTable.MovingVertically
            : sourceMovementType == SamusMovementType.Crouching
                ? GrappleConnectionTable.Crouching
                : GrappleConnectionTable.Default;
        var connection = GrappleConnections.ResolveConnection(connectionTable, grapple.FireDirection);
        GrappleConnectionFunction nextFunction = connection.Function;
        GrappleConnectionHandler handler = connection.Handler;

        // Each tiny connection handler installs one prospective grappling pose and then jumps
        // to either the swinging or the stuck tail. Keep the handlers distinct:
        // pose alone is insufficient to distinguish malformed fallback data from data.
        (SamusPoseId pose, bool swinging) = handler switch
        {
            GrappleConnectionHandler.SwingClockwise =>
                new GrappleConnectedPose(SamusPoseId.GrappleSwingRightPose, true),
            GrappleConnectionHandler.SwingAnticlockwise =>
                new GrappleConnectedPose(SamusPoseId.GrappleSwingLeftPose, true),
            GrappleConnectionHandler.StandingUpRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingRightPose, false),
            GrappleConnectionHandler.StandingRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingDownRightPose, false),
            GrappleConnectionHandler.StandingDown =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingDownLeftPose, false),
            GrappleConnectionHandler.StandingUpLeft =>
                new GrappleConnectedPose(SamusPoseId.GrappleStandingLeftPose, false),
            GrappleConnectionHandler.CrouchingUpRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingRightPose, false),
            GrappleConnectionHandler.CrouchingRight =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingDownRightPose, false),
            GrappleConnectionHandler.CrouchingDownLeft =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingDownLeftPose, false),
            GrappleConnectionHandler.CrouchingUpLeft =>
                new GrappleConnectedPose(SamusPoseId.GrappleCrouchingLeftPose, false),
            _ => throw new InvalidOperationException($"Undefined GrappleConnectionHandler {handler}."),
        };
        GrappleConnectionFunction expectedFunction = swinging
            ? GrappleConnectionFunction.Swinging
            : GrappleConnectionFunction.LockedInPlace;
        if (nextFunction != expectedFunction)
        {
            throw new InvalidDataException(
                $"Grapple connection handler {handler} requires function {expectedFunction}, " +
                $"but the record names {nextFunction}.");
        }

        // Both the connection and the fired beam derive the angle from the body position that fired the beam,
        // before command 9/10 changes pose and snaps Samus to the rope. The angle is stored
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
            // The swinging and stuck tails publish special prospective command 9/10. They do not
            // execute the pose commands inline. A super-special command can therefore keep
            // the old pose and every one of Samus's speeds while the Grapple function remains live.
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
    /// Applies accepted Grapple special-pose command after the frame's
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
            // Special pose command 9 runs the swing. Swinging copies Start into Flare, then
            // chooses the angle-authored animation frame and body offsets.
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            SamusGrappleSwing.PositionSamusFromPendulum(samus, grapple);
        }
        else
        {
            // Special pose command 10 runs the locked placement. The locked body is positioned from
            // Start minus the raw NO-RUN origin table, then Flare is independently rebuilt
            // from the raw no-run flare table. Graphics-Y correction does not participate.
            var origin = GrappleFiring.Origin(grapple.FireDirection, running: false);
            var flare = ReadFlareOrigin(grapple, grapple.FireDirection, running: false);
            samus.Position = samus.Position.WithWhole(grapple.RopeStart - new Offset16<RoomPixels>(origin.X, origin.Y));
            grapple.BeamStart = samus.Position.Whole + new Offset16<RoomPixels>(flare.X, flare.Y);
            grapple.Phase = GrapplePhase.ConnectedLocked;
        }

        // The connection pose commands 9 and 10 share one tail. Speed-booster bookkeeping
        // has no host field yet, but every modeled speed cleared there is reset here.
        // AccelerationMode is deliberately retained: the common tail does not write it.
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
            int eighths = (EightEight.FromWhole(absoluteY) / absoluteX) >> 3;
            return quadrant switch
            {
                0 => RotationAngle.QuarterTurn.TurnedBySteps(eighths),
                1 => RotationAngle.QuarterTurn.TurnedBySteps(-eighths),
                2 => RotationAngle.ThreeQuarterTurn.TurnedBySteps(-eighths),
                _ => RotationAngle.ThreeQuarterTurn.TurnedBySteps(eighths),
            };
        }

        int reciprocalEighths = (EightEight.FromWhole(absoluteX) / absoluteY) >> 3;
        return quadrant switch
        {
            0 => RotationAngle.HalfTurn.TurnedBySteps(-reciprocalEighths),
            1 => RotationAngle.Zero.TurnedBySteps(reciprocalEighths),
            2 => RotationAngle.HalfTurn.TurnedBySteps(reciprocalEighths),
            _ => RotationAngle.Zero.TurnedBySteps(-reciprocalEighths),
        };
    }

    /// <summary>Reads the installed presentation offset for the current shot direction and running state.</summary>
    internal static Offset16<RoomPixels> ReadFlareOrigin(SamusGrappleState grapple, SamusProjectileDirection direction, bool running)
    {
        var offset = (grapple.FlarePlacement ?? throw new InvalidOperationException(
            "Grapple flare requires installed placement definitions."))
            .Resolve(running, SamusFlareAim.AtMuzzle(direction));
        return new Offset16<RoomPixels>(offset.X, offset.Y);
    }

    /// <summary>
    /// Completes the one-frame cancellation function installed by firing. The game's
    /// step also clears palette/sound bookkeeping not yet modeled by this runtime;
    /// every gameplay-visible firing value owned by this class is cleared here.
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
            // Its command-six table entry kills all X/run momentum and the pose-definition
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

    /// <summary>Firing branch of the grapple handler, before the selected grapple function runs.</summary>
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
                // The game changes the function before dispatch: restart from
                // the current pose and hand position this frame, without extending.
                // Do not steer an old endpoint or override the pose from held aim keys.
                QueueGrappleSound(samus, GrappleBeamDefinitions.Sounds.RestartStop);
                grapple.Phase = GrapplePhase.Inactive;
                BeginFiring(samus, controllerInput);
                return new GrappleMovementResult(GrapplePhase.Firing, Fired: true, OwnsMovement: false);
            }
        }
        // Unlike a cancellation requested inside the extension function, this
        // decision precedes dispatch, so the cancel function runs immediately.
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
        // It publishes the pose as a super-special prospective pose (command 7), which
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
        // The drop and wall-jump teardowns share the same long cleanup tail. Palette, sound, and HUD-item
        // producers live outside this state object; every owned movement/rendering value is
        // reset here so a later firing edge cannot inherit wall-grace or collision state.
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
