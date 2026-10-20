using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Debugger-visible translation of grapple firing, persistent-block acquisition, connected
/// pendulum terrain interaction, collision kicks, and release from banks $9B and $94.
/// </summary>
/// <remarks>
/// The retail game does not run grapple physics through movement type $16's tiny handler at
/// $90:A780. <c>GrappleBeamHandler</c> in bank $9B updates the pendulum before normal beta
/// movement, and bank $94 advances its angle while probing room collision. This class keeps
/// those responsibilities separate from ordinary aerial movement. Firing, persistent grapple
/// blocks, per-pixel rope adjustment, and the six-point angular terrain sweep are translated
/// here; solid/frozen-enemy wall-jump probing is shared with ordinary movement and retains
/// the enemy slot to shake. The firing-only Draygon-turret damage and both BTS-indexed swing
/// spike-damage tables are translated through the shared periodic-damage words. Breakable
/// block acquisition now installs the independent bank-$84 PLM lifecycle; enemy acquisition
/// and the live enemy/shake loop remain explicit later routes rather than being approximated.
/// Grapple's narrower water flag and persistent environment-selected
/// release handler are translated rather than collapsed into ordinary aerial movement.
/// </remarks>
public static class SamusGrappleMovement
{
    /// <summary>
    /// Ports <c>GrappleBeamFunc_FireGoToCancel</c> at <c>$9B:C51E</c>, stopping immediately
    /// before its function-pointer return. The current pose supplies all direction/origin
    /// policy; no host aiming vector is accepted.
    /// </summary>
    public static void BeginFiring(SamusState samus, SnesButton controllerInput = SnesButton.None)
    {
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Inactive)
            throw new InvalidOperationException("A grapple state is already active.");

        bool movingHeld = samus.Pose is SamusPoseId.DraygonGrabbedMovingLeftPose or
            SamusPoseId.DraygonGrabbedMovingRightPose;
        byte direction = movingHeld ? ReadDraygonHeldDirection(samus.Pose, controllerInput) : samus.PoseMetadata.ReadShotDirection();
        if (direction >= 16)
        {
            // $9B:C53F-C547 treats ROM sentinel directions as ordinary cancellation,
            // including a shot arriving while a permitted posture is transitioning.
            grapple.Phase = GrapplePhase.CancelPending;
            return;
        }
        if (direction >= 10)
        {
            throw new InvalidOperationException(
                $"Pose ${(int)samus.Pose:X2} has no fireable grapple direction (${direction:X2}).");
        }

        grapple.Phase = GrapplePhase.Firing;
        grapple.FireDirection = (SamusProjectileDirection)direction;
        grapple.PoseChangeAutoFireTimer = NativeCountdown.Start(SamusGrappleRomData.Firing.PoseChangeAutoFireFrames);
        var launch = GrappleFiringDefinitions.Launch(direction);
        grapple.ExtensionVelocity = launch.Velocity;
        grapple.Angle = launch.Angle;
        grapple.MirroredAngle = grapple.Angle;

        // `$9B:C4F0` selects run offsets solely for movement type one. Moonwalking is
        // distinct type `$10`, so every stable/aimed moonwalk pose naturally takes the
        // no-run origin and flare tables without a pose-number exception.
        bool useRunOffsets = samus.PoseMetadata.ReadMovementKind() == SamusMovementType.Running;
        var origin = GrappleFiringDefinitions.Origin(direction, useRunOffsets);
        var flare = ReadFlareOrigin(grapple, direction, useRunOffsets);
        sbyte graphicsYOffset = movingHeld ? SamusGrappleRomData.Firing.DraygonMovingGraphicsYOffset : samus.PoseMetadata.ReadGraphicsYOffset();
        int physicalYOffset = movingHeld ? SamusGrappleRomData.Firing.DraygonMovingGraphicsYOffset :
            SamusPoseProjectileOriginDefinitions.ReadYOffset(samus.Pose);

        grapple.OriginOffset = new(origin.X, unchecked((short)(origin.Y - physicalYOffset)));
        grapple.FlareOffset = new(flare.X, unchecked((short)(flare.Y - graphicsYOffset)));

        // The endpoint offset is a signed 16.16 displacement from Samus plus the origin
        // table. $9B:C51E clears all four words, not merely their whole halves.
        grapple.EndpointOffset = FixedVector<RoomPixels>.Zero;
        // $9B:C651-C65A.
        grapple.QuarterVelocity = FixedVector<RoomPixels>.Zero;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 12;
        grapple.AngularVelocity = 0;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = NativeCountdown.Start(0);
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        grapple.SpecialAngleHandling = false;
        grapple.SlowScrolling = false; // $9B:C66F
        grapple.WallJumpTimer = NativeCountdown.Start(0);
        grapple.CancelFromConnectedPose = false;
        InitializeBeamAnimation(grapple);

        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Fire);
        // $9B:C6A3-C6AE: firing again ends the released-from-swing movement handler, so
        // Samus moves with ordinary physics from this same frame.
        grapple.ReleasedMovementActive = false;
        // Native WRAM has two easily conflated coordinate pairs. Start is the hand/rope
        // origin used by connection positioning; Flare is the small-OBJ draw origin used
        // by $94:AFBA. They coincide while swinging but can differ while firing or locked.
        grapple.RopeStart = samus.Position.Whole + grapple.OriginOffset;
        grapple.BeamStart = samus.Position.Whole + grapple.FlareOffset;
        grapple.Anchor = samus.Position.Whole + grapple.OriginOffset;
    }

    /// <summary>
    /// <c>$94:A85E-A879</c> (and its Y twin): the velocity word is stored one byte into the
    /// four-byte quarter pair, which is then rotated right twice as 32 bits. The pair's low
    /// and high bytes are last frame's, and the second rotate carries the low bit into the
    /// top. A negative velocity then sets the top ten bits.
    /// </summary>
    private static SnesSignedSixteenSixteen QuarterExtensionVelocity(
        short velocity, SnesSignedSixteenSixteen previous)
    {
        uint pair = ((uint)((ushort)previous.WholePixels & 0xff00) << 16) | // cartridge-encoding: allow(ByteLane) - native ROR over the packed whole-high, velocity, fraction-low bytes
            ((uint)(ushort)velocity << 8) | // cartridge-encoding: allow(ByteLane) - native ROR over the packed whole-high, velocity, fraction-low bytes
            (uint)(previous.Fraction & 0x00ff); // cartridge-encoding: allow(ByteLane) - native ROR over the packed whole-high, velocity, fraction-low bytes
        uint rotated = (pair >> 2) | ((pair & 1) << 31);
        ushort whole = SnesFixedPosition.WholeOf(unchecked((int)rotated));
        if (velocity < 0)
            whole |= ushort.MaxValue & ~63; // The rotate's sign fills the top ten bits.
        return SnesSignedSixteenSixteen.FromParts(unchecked((short)whole), unchecked((ushort)rotated));
    }

    /// <summary>
    /// Reports whether grapple's replacement Samus draw-handler pointer is still installed.
    /// </summary>
    /// <remarks>
    /// Cancellation, release, drop, and grapple wall-jump are one-call teardown functions.
    /// On the frame that installs one of them, `$90:EB86` is still the selected handler even
    /// though its signed function-pointer test falls back to the ordinary body/echo path.
    /// Keeping that distinction explicit prevents the normal charge flare from leaking into
    /// the teardown frame; `$90:EB52`, which owns that flare, has not been restored yet.
    /// </remarks>
    public static bool UsesGrappleDrawingHandler(GrapplePhase phase) =>
        phase != GrapplePhase.Inactive;

    /// <summary>
    /// Mirrors `$90:EB86`'s signed function-pointer range test for the beam-specific path.
    /// </summary>
    public static bool UsesBeamSpecificDrawingPath(GrapplePhase phase) => phase is
        GrapplePhase.Firing or
        GrapplePhase.ConnectedSwinging or
        GrapplePhase.ConnectedLocked or
        GrapplePhase.WallGrab or
        GrapplePhase.WallGrabRelease;

    /// <summary>
    /// Ports <c>HandleGrappleBeamFlare</c> at `$9B:C036`. Call before atmosphere and Samus,
    /// exactly where the active half of `$90:EB86` calls it.
    /// </summary>
    /// <returns>True when the flare origin passed the native vertical visibility test.</returns>
    public static bool DrawFlareBeforeSamus(
        SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteCatalog? compositions = null,
        SpriteArtwork? standardObjects = null)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(sprites);

        SamusGrappleState grapple = samus.Grapple;
        if (!UsesBeamSpecificDrawingPath(grapple.Phase) || grapple.FlareCounter == 0)
            return false;

        // While firing, `$90:EB86` calls `$9B:BF1B` immediately before the flare. Grapple
        // physics ran before ordinary Samus movement, so the renderer must reconstruct both
        // hand origins from the final body position or a running shot visibly lags one frame.
        // Connected functions already publish their exact rope/flare pair during movement.
        if (grapple.Phase == GrapplePhase.Firing)
            RefreshFiringDrawOrigins(samus, grapple);

        // Counter one is a sentinel, not merely the first ordinary animation tick. Native
        // code force-selects main-flare frame 16 and timer three on every such call, then
        // performs the usual 16-bit DEC/BMI. The first visible frame therefore stores two.
        if (grapple.FlareCounter == 1)
        {
            grapple.FlareAnimationFrame = ChargeFlareAnimationDefinitions.GrappleInitialFrame;
            grapple.FlareAnimationTimer = NativeCountdown.Start(ChargeFlareAnimationDefinitions.GrappleInitialDelay);
        }

        grapple.FlareAnimationTimer = grapple.FlareAnimationTimer.Decrement().Remaining;
        if (grapple.FlareAnimationTimer.IsNegative)
        {
            grapple.FlareAnimationFrame = StepFlareFrame(grapple.FlareAnimationFrame, 1);
            byte delay = ChargeFlareAnimationDefinitions.ReadByte(
                SamusGrappleRomData.Firing.MainFlareAnimationDelays +
                    grapple.FlareAnimationFrame);
            if (ChargeFlareAnimationDefinitions.TryDecodeCommand(delay, out ChargeFlareDelayCommand command) &&
                command == ChargeFlareDelayCommand.Rewind)
            {
                // `$FE,n` is the compact loop command in the shared delay bytecode. The
                // subtraction applies to the already-incremented frame word and wraps like
                // 16-bit ADC/SBC; malformed ROM data remains visible instead of clamped.
                byte rewind = ChargeFlareAnimationDefinitions.ReadByte(
                    SamusGrappleRomData.Firing.MainFlareAnimationDelays +
                    StepFlareFrame(grapple.FlareAnimationFrame, 1));
                grapple.FlareAnimationFrame = StepFlareFrame(grapple.FlareAnimationFrame, -rewind);
                delay = ChargeFlareAnimationDefinitions.ReadByte(
                    SamusGrappleRomData.Firing.MainFlareAnimationDelays +
                        grapple.FlareAnimationFrame);
            }

            grapple.FlareAnimationTimer = NativeCountdown.FromRaw(delay);
        }

        ushort screenX = unchecked((ushort)(grapple.BeamStart.X - layer1.X));
        ushort screenY = unchecked((ushort)(grapple.BeamStart.Y - layer1.Y));
        // The assembly tests only Y's high byte. X is allowed to wrap into high OAM, while
        // any Y outside unsigned screen rows `$00-$FF` suppresses the whole spritemap.
        if ((SnesLittleEndian.HighByteInPlace(screenY)) != 0)
            return false;

        // The first word of both native facing rows is zero. Grapple uses the
        // shared main flare only; charge-spark direction offsets do not apply.
        ushort tableIndex = unchecked((ushort)(
            grapple.FlareAnimationFrame + SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions.MainFlareSelectorOffset));
        // Flare compositions name characters $50-$5D of the standard OBJ sheet.
        sprites.Add(new SpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleFlare),
            (compositions ?? throw new InvalidOperationException(
                "Grapple flare requires installed sprite compositions.")).Frame(tableIndex),
            standardObjects ?? throw new InvalidOperationException("Grapple flare requires the installed standard OBJ page."),
            new Position16<ScreenPixels>(screenX, screenY), SnesTileFlipFlags.None, 0, 0));
        return true;
    }

    /// <summary>
    /// Ports <c>GrappleBeamFunc_Firing</c> at <c>$9B:C703</c>, including the bank-$A0
    /// enemy pass which precedes <c>BlockCollGrappleBeam</c> at <c>$94:A85B</c>.
    /// </summary>
    public static GrappleMovementResult StepFiring(

        RoomLevelData level,
        SamusState samus,
        SnesButton controllerInput,
        RoomPlmSystem? plms = null,
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

        // The common pose-command tail clamps the camera's previous-position words after
        // command 9/10 snaps Samus to the accepted rope geometry. Retain the pre-snap body
        // position even though most firing frames return without consuming this sample.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // $9B:C703 treats release of Shoot as cancellation before length or collision work.
        if ((controllerInput & SnesButton.X) == 0)
            return QueueFiringCancellation(grapple);

        grapple.RopeLength += 12;
        if (grapple.RopeLength >= 128)
            return QueueFiringCancellation(grapple);

        // `$9B:C73D` tests the current endpoint against ordinary enemies before moving the
        // beam through its four block-collision substeps. The collision producer sets the
        // enemy's common grapple-handler bit; this consumer interprets only the header's
        // seven common reaction indexes. Kill/no-interaction deliberately fall through to
        // terrain, exactly like the native zero return from their reaction functions.
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
                // Kill and no interaction fall through to terrain, like the native zero return.
                case GrappleEnemyReaction.None:
                case GrappleEnemyReaction.Kill:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined GrappleEnemyReaction {enemy.Reaction}.");
            }
        }

        // $94:A85B quarters each velocity and performs four identical probes. The quarter
        // keeps stale bytes from the previous frame, so the four steps need not add up to
        // exactly one authored per-frame velocity.
        grapple.QuarterVelocity = new(
            QuarterExtensionVelocity(grapple.ExtensionVelocity.X.SignedRawValue, grapple.QuarterVelocity.X),
            QuarterExtensionVelocity(grapple.ExtensionVelocity.Y.SignedRawValue, grapple.QuarterVelocity.Y));
        GrappleBlockReaction finalReaction = GrappleBlockReaction.Passes;
        for (int substep = 0; substep < 4; substep++)
        {
            grapple.EndpointOffset += grapple.QuarterVelocity;
            PublishFiringGeometry(samus, grapple);

            GrappleBlockReaction reaction = ReactAtEndpoint(
                level,
                samus,
                grapple.Anchor,
                plms);
            finalReaction = reaction;
            if (reaction != GrappleBlockReaction.Connects)
                continue;

            // A connection is the native carry+overflow result. $94:A8DA centers the point in
            // the accepted 16x16 block before bank $9B chooses the swing/locked pose.
            grapple.Anchor = new(
                unchecked((ushort)((grapple.Anchor.X & ~15) | 8)),
                unchecked((ushort)((grapple.Anchor.Y & ~15) | 8)));
            return ConnectAcceptedFiringCore(
                samus,
                grapple,
                previousPosition,
                GrappleAnchorKind.Block,
                connectionPoseChange);
        }

        // `$94:A85B` does not stop when an intermediate probe returns carry without
        // overflow. It executes all four quarter-velocity probes and returns only the
        // fourth reaction to `$9B:C703`. At high running speed a thin solid cell can
        // therefore be sampled and then cleared before the caller decides whether to
        // cancel. Green Hill Zone's Grapple/Speed Booster gate exploit depends on this
        // general cartridge ordering; it is not a gate-specific collision exception.
        if (finalReaction.Collides())
            return QueueFiringCancellation(grapple);

        return new GrappleMovementResult(
            grapple.Phase,
            Fired: true,
            OwnsMovement: false);
    }

    /// <summary>
    /// Executes the post-function liquid-bit update at <c>$9B:C4B1-$C4EA</c>.
    /// </summary>
    /// <remarks>
    /// Grapple physics reads this bit during a function and the handler rewrites it only
    /// afterward, so callers invoke this exactly once at the end of the bank-$9B pass. The
    /// explicit phase set mirrors the native pointer-range test: release, cancel, dropped,
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
    public static GrappleMovementResult Step(SamusTerrain terrain, SamusState samus, SnesButton controllerInput, SnesButton newlyPressedInput, int nmiFrameCounter = 0, Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision = null, GrapplePoseChange poseChange = GrapplePoseChange.Immediate)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        if (grapple.Phase == GrapplePhase.ReleaseFromSwing)
        {
            // $9B:CB8B executes on the frame after $9B:C79D queued it. The launch velocity
            // was already published, so this pass changes art/handlers and clears grapple.
            SamusPoseId? pendingReleasePose = CompleteQueuedRelease(terrain.Level, samus, grapple, poseChange == GrapplePoseChange.Deferred);
            return new GrappleMovementResult(GrapplePhase.Inactive, PendingReleasePose: pendingReleasePose);
        }

        // These are literal bank-$9B function-pointer phases. Each handler owns the whole
        // grapple beta pass, so dispatch it before entering ordinary pendulum integration.
        if (grapple.Phase == GrapplePhase.ConnectedLocked)
            return StepLocked(terrain.Level, samus, controllerInput, enemyCollision);
        if (grapple.Phase == GrapplePhase.WallGrab)
            return StepWallGrab(terrain.Level, samus, controllerInput, enemyCollision);
        if (grapple.Phase == GrapplePhase.WallGrabRelease)
            return StepWallGrabRelease(terrain.Level, samus, newlyPressedInput);
        if (grapple.Phase == GrapplePhase.WallJumping)
            return CompleteGrappleWallJump(terrain.Level, samus, grapple);
        if (grapple.Phase == GrapplePhase.Dropped)
            return CompleteDropped(terrain.Level, samus, grapple, nmiFrameCounter, terrain.Plms, poseChange == GrapplePoseChange.Deferred);

        if (grapple.Phase != GrapplePhase.ConnectedSwinging)
            throw new InvalidOperationException("Connected grapple movement is not active.");

        // `$9B:BAD5` clamps the camera's previous-position words against the position from
        // the start of this frame after a special-angle snap. Retain that exact sample now;
        // the host runtime consumes the optional corrected values returned below.
        Position16<RoomPixels> previousPosition = samus.Position.Whole;

        // $9B:C79D first checks Shoot. Releasing it does not immediately install jumping
        // art: it computes velocity and queues the release function for the following frame.
        if ((controllerInput & SnesButton.X) == 0)
        {
            if (grapple.AngularVelocity == 0 && grapple.Angle == SnesAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase);
            }

            PropelSamusFromSwing(samus, grapple);
            // `$9B:C7C1` installs `$90:946E` immediately. The beam function remains in
            // its one-frame release-cleanup phase, but beta movement already uses the
            // independent release handler during this same gameplay frame.
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(grapple.Phase);
        }

        ApplyRopeAndDirectionInput(grapple, controllerInput, newlyPressedInput);
        bool ropeLengthBlocked = ApplyRopeLengthDelta(terrain.Level, samus, grapple);
        CalculateGravity(grapple);
        IntegrateAngularVelocity(grapple);
        ApplyJumpImpulse(grapple, newlyPressedInput);
        GrappleSwingCollisionResult swingCollision = AdvanceAngleWithTerrainCollision(
            terrain.Level,
            samus,
            grapple);

        // `$9B:BAD5` scans all eight ROM records only when bank $94 raised the close-body
        // flag. A match replaces pendulum positioning for this frame with the table's exact
        // pose, anchor-relative body offset, and locked/wallgrab function pointer.
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
                !IsStillConnectedToSupportedBlock(terrain.Level, samus, grapple));
        if (anchorDisconnected)
        {
            // The persistent block path normally remains connected forever. Retain the
            // native validation seam so a future dynamic/breakable PLM cannot leave a rope
            // attached to air. A motionless straight-down body queues `$C8C5`; a moving
            // pendulum uses the already translated velocity-preserving release handoff.
            if (grapple.AngularVelocity == 0 && grapple.Angle == SnesAngle.HalfTurn)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase,
                    TerrainCollided: swingCollision.Collided,
                    CollisionDistanceFromFeet: swingCollision.DistanceFromFeet,
                    RopeLengthBlocked: ropeLengthBlocked);
            }

            PropelSamusFromSwing(samus, grapple);
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(
                grapple.Phase,
                TerrainCollided: swingCollision.Collided,
                CollisionDistanceFromFeet: swingCollision.DistanceFromFeet,
                RopeLengthBlocked: ropeLengthBlocked);
        }

        PositionSamusFromPendulum(samus, grapple);

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
        SnesButton controllerInput,
        Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision)
    {
        SamusGrappleState grapple = samus.Grapple;

        // `$9B:C77E` does no pendulum work. It merely retains the frozen pose while Shoot
        // remains held and either an enemy or the stored block still supports the endpoint.
        bool shootHeld = (controllerInput & SnesButton.X) != 0;
        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorHeld = enemyAnchorHeld || (grapple.ValidateAnchorEnemy
            ? false
            : !grapple.ValidateAnchorBlock ||
                IsStillConnectedToSupportedBlock(level, samus, grapple));
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // Both release and block disconnection install `$C856`. Mark the origin because
        // firing cancellation coexists with ordinary movement, whereas a locked type-$16
        // body must own this beta pass until its pose-definition fallback is committed.
        grapple.CancelFromConnectedPose = !samus.DraygonGrabbed.IsActive;
        grapple.Phase = GrapplePhase.CancelPending;
        return new GrappleMovementResult(
            grapple.Phase,
            OwnsMovement: true);
    }

    /// <summary>
    /// Replays the connected-beam enemy test at <c>$9B:C7E4</c>. Any nonzero common
    /// reaction keeps the connection alive for this frame; attach reactions additionally
    /// follow a moving actor's center. A missing callback is accepted only for debugger
    /// states that did not opt into live-enemy validation.
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
    /// Applies common grapple reaction six. Unlike ordinary body contact, bank $9B selects
    /// horizontal knockback from Samus's facing direction, but suit division and the
    /// `$60/$05` hurt lifetimes are the same shared gameplay contracts.
    /// </summary>
    private static void ApplyGrappleEnemyDamage(

        SamusState samus,
        SnesButton controllerInput,
        int damageBeforeSuit)
    {
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? damageBeforeSuit / 4
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? damageBeforeSuit / 2
                : damageBeforeSuit;
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;
        samus.InvincibilityTimer = NativeCountdown.Start(96);

        // Facing left is knocked right and facing right is knocked left. Start owns the
        // authentic pose, animation, vertical speed, contact-damage cancellation, and
        // hurt-flash side effects instead of duplicating those writes here.
        KnockbackHorizontalDirection knockbackXDirection = SamusPoseMetadata.IsFacingLeft(samus.Pose)
            ? KnockbackHorizontalDirection.Right
            : KnockbackHorizontalDirection.Left;
        SamusKnockbackMovement.Start(samus, controllerInput, new KnockbackLaunch { KnockbackXDirection = knockbackXDirection, KnockbackTimer = NativeCountdown.Start(5) });
    }

    /// <summary>Keeps the wall-contact pose attached until Shoot or anchor support ends, then opens a 30-update wall-jump window.</summary>
    private static GrappleMovementResult StepWallGrab(
        RoomLevelData level,
        SamusState samus,
        SnesButton controllerInput,
        Func<Position16<RoomPixels>, GrappleEnemyCollision>? enemyCollision)
    {
        SamusGrappleState grapple = samus.Grapple;
        bool shootHeld = (controllerInput & SnesButton.X) != 0;
        bool enemyAnchorHeld = TryRetainEnemyAnchor(grapple, enemyCollision);
        bool anchorHeld = enemyAnchorHeld || (grapple.ValidateAnchorEnemy
            ? false
            : !grapple.ValidateAnchorBlock ||
                IsStillConnectedToSupportedBlock(level, samus, grapple));
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // `$9B:C81B` writes decimal 30 and changes only the grapple function. The contact
        // pose and beam remain visible throughout the following grace-window probes.
        grapple.WallJumpTimer = NativeCountdown.Start(30);
        grapple.Phase = GrapplePhase.WallGrabRelease;
        return new GrappleMovementResult(
            grapple.Phase);
    }

    /// <summary>During the release window, accepts a fresh Jump only when a 16-pixel probe finds a wall or solid enemy.</summary>
    private static GrappleMovementResult StepWallGrabRelease(

        RoomLevelData level,
        SamusState samus,
        SnesButton newlyPressedInput)
    {
        SamusGrappleState grapple = samus.Grapple;

        // DEC/BPL at `$9B:C832` gives values 29..0 thirty eligible checks. The next call
        // wraps zero to `$FFFF`, queues dropped, and deliberately performs no wall probe.
        grapple.WallJumpTimer = grapple.WallJumpTimer.Decrement().Remaining;
        if (grapple.WallJumpTimer.IsNegative)
        {
            grapple.Phase = GrapplePhase.Dropped;
            return new GrappleMovementResult(
                grapple.Phase);
        }

        // `$90:9CAC` probes solid/frozen enemies first, then 16 pixels of room blocks toward
        // the wall named by the contact pose's direction byte. The shared probe preserves
        // square slopes, BTS dispatch, available distance, and enemy identity.
        int signedProbe = samus.PoseMetadata.IsFacing(SamusFacingDirection.Left)
            ? -(16 << 16)
            : 16 << 16;
        BlockMoveResult wall = SamusBlockCollision.ProbeWallHorizontal(

            level,
            samus.Kinematics,
            signedProbe);
        bool jumpNew = (newlyPressedInput & SnesButton.A) != 0;
        if (wall.Collided && jumpNew)
        {
            if (wall.EnemyCollision is { EnemySlot: int enemySlot })
            {
                // `$90:9D24-$90:9D26` requests the contacted enemy's shake only when the
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
        // `$9B:C9CE` runs one frame after `$C832` selected it, matching every other grapple
        // function-pointer handoff. The Samus helper keeps the peculiar `$B8->$84` and
        // `$B9->$83` reversal separate from ordinary spin-contact wall jumps.
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        samus.PoseTransitions.ApplyGrappleWallJump();
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
        int nmiFrameCounter,
        RoomPlmSystem? plms,
        bool deferPoseChange)
    {
        SamusPoseId targetPose = SelectDroppedPose(samus);
        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        // C8C5 clears these words now, but publishes its pose to the transitional
        // slot. Normal beta movement still runs before that pose is committed.
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.Kinematics.YSpeed = default;
        bool emptyBeta = samus.PoseMetadata.ReadMovementKind() == SamusMovementType.Grappling;
        if (!deferPoseChange)
            samus.PoseTransitions.ApplyGrappleDropTransition(level, targetPose, nmiFrameCounter, plms);
        ClearConnectedGrapple(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            OwnsMovement: !deferPoseChange || emptyBeta,
            PendingDropPose: deferPoseChange ? targetPose : null);
    }

    /// <summary>Chooses the fall pose from shot direction and body radius, with standing exceptions for both swinging poses.</summary>
    internal static SamusPoseId SelectDroppedPose(SamusState samus)
    {
        // Swinging `$B2/$B3` bypass the direction tables and always fall to ordinary
        // standing `$01/$02`, even though their live collision radius is compact.
        if (samus.Pose == SamusPoseId.GrappleSwingRightPose)
            return SamusPoseId.FacingRightNormalPose;
        if (samus.Pose == SamusPoseId.GrappleSwingLeftPose)
            return SamusPoseId.FacingLeftNormalPose;

        byte shotDirection = samus.PoseMetadata.ReadShotDirection();
        bool ordinaryDirection = shotDirection < 10;
        if (!ordinaryDirection)
        {
            // `$9B:C8DB/$C93C` use the pose direction when the direction byte is a command
            // sentinel. Standing selects `$01/$02`; compact bodies select `$27/$28`.
            bool facingLeft = samus.PoseMetadata.IsFacing(SamusFacingDirection.Left);
            return samus.Kinematics.Radius.Y >= 17
                ? facingLeft ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose
                : facingLeft ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose;
        }

        return GrappleConnectionDefinitions.DroppedPose(shotDirection, compact: samus.Kinematics.Radius.Y < 17);
    }

    /// <summary>Applies an exact special-angle connection record, including its pose and bounded camera displacement.</summary>
    internal static bool TryHandleSpecialAngle(
        SamusState samus,
        SamusGrappleState grapple,
        Position16<RoomPixels> previousPosition,
        out GrappleMovementResult result)
    {
        // Preserve the native reverse scan and exact angle equality, not a tolerance
        // around a wall-grab angle. These records govern collision positioning.
        for (int record = GrappleConnectionDefinitions.SpecialAngleCount - 1; record >= 0; record--)
        {
            var special = GrappleConnectionDefinitions.SpecialAngle(record);
            if (special.Angle != grapple.Angle)
                continue;

            GrappleConnectionFunction function = special.Function;
            GrapplePhase phase = function switch
            {
                GrappleConnectionFunction.LockedInPlace => GrapplePhase.ConnectedLocked,
                GrappleConnectionFunction.WallGrab => GrapplePhase.WallGrab,
                GrappleConnectionFunction.Swinging => throw new InvalidDataException(
                    $"Grapple special-angle record {record} names the swinging function."),
                _ => throw new InvalidOperationException($"Undefined GrappleConnectionFunction {function}."),
            };

            samus.Pose = special.Pose;
            samus.Position = samus.Position.WithWhole(grapple.Anchor + special.SamusOffset);
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation(initialFrame: 0);
            grapple.Phase = phase;
            grapple.WallJumpTimer = NativeCountdown.Start(0);
            grapple.SlowScrolling = false; // $9B:BB0D

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

    /// <summary>Limits one axis of the previous position to twelve pixels using wrapped 16-bit displacement.</summary>
    private static ushort ClampPreviousAxis(ushort current, ushort previous)
    {
        short displacement = unchecked((short)(current - previous));
        if (displacement >= 13)
            return unchecked((ushort)(current - 12));
        if (displacement < -12)
            return unchecked((ushort)(current + 12));
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

    /// <summary>Publishes rope, physical endpoint, and flare origins with fractional carry from the firing offset.</summary>
    private static void PublishFiringGeometry(SamusState samus, SamusGrappleState grapple)
    {
        // Whole endpoint offsets are the signed upper words of the two 16.16 accumulators.
        // Arithmetic shift is intentional: a left/up beam must retain sign after fractional
        // accumulation, exactly like reading `$0DCE/$0DD2` as signed displacement words.
        grapple.RopeStart = samus.Position.Whole + grapple.OriginOffset;
        // $94:A8C6-A8F1 adds the offset to Samus's subpixel-inclusive position, so the
        // fractional carry reaches the endpoint before the whole origin offset is added.
        grapple.Anchor = samus.Position.Advance(grapple.EndpointOffset).Whole + grapple.OriginOffset;
        grapple.BeamStart = samus.Position.Whole + grapple.FlareOffset;
    }

    /// <summary>Dispatches the endpoint block and extensions, including PLM effects, into the native carry and overflow reaction.</summary>
    private static GrappleBlockReaction ReactAtEndpoint(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> endpoint,
        RoomPlmSystem? plms = null)
    {
        int blockX = endpoint.X >> 4;
        int blockY = endpoint.Y >> 4;
        RoomCollisionBlock initialBlock =
            level.GetCollisionBlockOrPrefilledSolid(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
        if (initialBlock.Index < 0)
        {
            // The prefilled `$8000` word dispatches as an ordinary solid endpoint: carry
            // set, overflow clear, which queues grapple cancellation rather than connecting.
            return GrappleBlockReaction.Solid;
        }

        int index = initialBlock.Index;
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            RoomCollisionBlock block =
                level.GetCollisionBlockByIndexOrPrefilledSolid(index);
            int resolvedX = block.Index < 0 ? -1 : block.Index % level.Size.X;
            int resolvedY = block.Index < 0 ? -1 : block.Index / level.Size.X;
            switch (block.CollisionType)
            {
                // These four dispatcher entries return clear carry: the beam remains live.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.UnusedAir:
                    return GrappleBlockReaction.Passes;

                // Slopes and these solid-family entries return carry with overflow clear,
                // which makes $9B:C703 queue cancellation rather than establish a rope.
                case RoomCollisionType.Slope:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpecialBlock:
                    return GrappleBlockReaction.Solid;

                case RoomCollisionType.SpikeBlock:
                    // `$94:A7FD` selects a bank-$84 grapple-reaction PLM by the low seven
                    // BTS bits. A negative BTS rejects the beam. Every ordinary entry uses
                    // `$84:CFD1` (carry set, overflow clear) and immediately deletes; BTS
                    // three alone uses Draygon's broken-turret setup `$84:CFD5`, which adds
                    // one whole periodic-damage unit and returns carry+overflow to connect.
                    if (block.Bts.RejectsGrappleReaction)
                        return GrappleBlockReaction.Passes;
                    if (block.Bts.GrappleReactionIndex == 3)
                    {
                        samus.LiquidPhysics.AccumulatePeriodicDamage(
                            subDamage: 0,
                            wholeDamage: 1);
                        return GrappleBlockReaction.Connects;
                    }

                    return GrappleBlockReaction.Solid;

                case RoomCollisionType.HorizontalExtension:
                    // Horizontal extensions interpret BTS as a signed block-index delta.
                    // BTS zero is the native terminator and behaves as air.
                    if (block.Bts == RoomBlockBehaviorValues.None)
                        return GrappleBlockReaction.Passes;
                    index += block.Bts.ExtensionOffset;
                    continue;

                case RoomCollisionType.VerticalExtension:
                    // Vertical extension uses the same signed BTS but scales by room width.
                    if (block.Bts == RoomBlockBehaviorValues.None)
                        return GrappleBlockReaction.Passes;
                    index += block.Bts.ExtensionOffset * level.Size.X;
                    continue;

                case RoomCollisionType.GrappleBlock:
                    // $94:A7D1 rejects bit-seven BTS. Values zero and three spawn persistent
                    // grapple PLM $D0D8, whose setup returns processor flags $41 (C=1,V=1).
                    // The persistent PLM has no level mutation, so this result is complete.
                    if (block.Bts.RejectsGrappleReaction)
                        return GrappleBlockReaction.Passes;
                    if (block.Bts.IsPersistentGrappleReaction)
                        return GrappleBlockReaction.Connects;

                    // BTS one/two spawn $D0DC/$D0E0. Setup_CFB5 synchronously saves the
                    // complete level word and clears BTS before returning C+V; the new PLM
                    // executes its first timer/draw record later in this same gameplay frame.
                    // A caller which omitted the independent room owner cannot honestly
                    // preserve that lifecycle, so keep the missing integration seam explicit.
                    if (block.Bts.IsBreakableGrappleReaction)
                    {
                        if (plms is null)
                        {
                            throw new InvalidOperationException(
                                "Breakable grapple acquisition requires a RoomPlmSystem.");
                        }
                        if (!plms.BlockReactions.TrySpawnBreakableGrappleBlock(level, block.Index, block.Bts))
                        {
                            throw new InvalidOperationException(
                                "All 40 native PLM slots are occupied during grapple acquisition.");
                        }

                        return GrappleBlockReaction.Connects;
                    }

                    // Nonnegative BTS values beyond the four authored grapple reactions
                    // would index outside $D0D8-$D0E0 in the native selection table.
                    throw new InvalidDataException(
                        $"Invalid grapple block BTS ${block.Bts.Value:X2} at ({resolvedX},{resolvedY}).");

                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.ShootableBlock:
                    // `$94:9E55/$9E73` use the same shootable reaction table as ordinary
                    // projectiles. Grapple owns no projectile family, so weapon-gated
                    // entries reject it while BTS 0..7 retain their unconditional block
                    // animation. The collision nibble—not PLM setup—owns carry: type 4 is
                    // air and type C is solid.
                    if (plms is null)
                    {
                        throw new InvalidOperationException(
                            "Shootable grapple reaction requires a RoomPlmSystem.");
                    }
                    // Blue caps occupy the same native table as breakable blocks but
                    // select door PLMs. Grapple's negative projectile index bypasses the
                    // power-bomb rejection in Setup_BlueDoor; the ordinary opening owner
                    // supplies all four cap tiles, timing, collision changes, and sound.
                    if (block.Bts.TryGetDownwardGateTrigger(out _))
                        plms.Gates.TrySpawnDownwardGateTrigger(level, block.Index, block.Bts, projectileType: 0);
                    else if (block.Bts.TryGetBlueDoorOrientation(out _))
                        plms.DoorPlms.TrySpawnBlueDoorOpening(level, block.Index, block.Bts, projectileType: 0);
                    else if (block.Bts == RoomBlockBehaviorValues.ResidentPlmProjectileTrigger)
                    {
                        // Generic shot-trigger setup also belongs to the grapple table.
                        // Notify the resident actor with no weapon family; its own native
                        // pre-instruction decides whether to reject the hit or wake up.
                        if (!plms.ProjectileTriggers.TryNotifyResidentProjectileHit(block.Index, projectileType: 0))
                            throw new InvalidOperationException(
                                $"Grapple trigger block {block.Index} at ({resolvedX},{resolvedY}) has no resident PLM owner.");
                    }
                    else if (block.Bts == RoomBlockBehaviorValues.CollectibleTrigger)
                        plms.CollectiblePlms.TryNotifyCollectibleProjectileHit(block.Index);
                    else
                    {
                        try
                        {
                            plms.BlockReactions.TrySpawnProjectileShotBlock(
                                level,
                                block.Index,
                                block.Bts,
                                projectileType: 0,
                                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
                        }
                        catch (ArgumentOutOfRangeException error) when (error.ParamName == "bts")
                        {
                            throw new CartridgeDispatchException(
                                $"grapple-shootable/type-{(int)block.CollisionType:X}/bts-{block.Bts.Value:X2}",
                                $"Grapple shootable dispatch type ${(int)block.CollisionType:X}/BTS ${block.Bts.Value:X2} " +
                                $"at block {block.Index} ({resolvedX},{resolvedY}) has no translated table entry.",
                                error);
                        }
                    }
                    return block.CollisionType == RoomCollisionType.ShootableBlock
                        ? GrappleBlockReaction.Solid
                        : GrappleBlockReaction.Passes;

                case RoomCollisionType.BombableAir:
                    // Bombable-air setup `$84:CEDA` immediately deletes its provisional
                    // PLM for grapple's zero projectile family. Carry remains clear.
                    return GrappleBlockReaction.Passes;

                case RoomCollisionType.BombableBlock:
                    // The solid twin performs the same rejected setup but independently
                    // returns carry set from `$94:9FF4`.
                    return GrappleBlockReaction.Solid;

                default:
                    throw new InvalidDataException(
                        $"Grapple block type ${(int)block.CollisionType:X1} escaped the complete " +
                        $"sixteen-entry dispatcher at ({resolvedX},{resolvedY}).");
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
        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Attach);
        // $9B:B98C bypasses ordinary connection pose/position setup while Draygon owns
        // Samus. Only the beam locks; the enemy continues to place the held body.
        SamusMovementType sourceMovementType = samus.PoseMetadata.ReadMovementType();
        if (sourceMovementType == SamusMovementType.DraygonHeld)
        {
            grapple.Phase = GrapplePhase.ConnectedLocked;
            grapple.ValidateAnchorBlock = anchor == GrappleAnchorKind.Block;
            grapple.ValidateAnchorEnemy = anchor == GrappleAnchorKind.Enemy;
            grapple.CancelFromConnectedPose = false;
            // The helper clears delta, then its firing caller immediately writes -8.
            grapple.RopeLengthDelta = SamusGrappleRomData.Physics.InitialConnectionRetraction;
            return new GrappleMovementResult(grapple.Phase, OwnsMovement: false);
        }

        bool movingVertically =
            samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0;
        int connectionTable = movingVertically
            ? SamusGrappleRomData.Connections.MovingVerticallyTable
            : sourceMovementType == SamusMovementType.Crouching
                ? SamusGrappleRomData.Connections.CrouchingTable
                : SamusGrappleRomData.Connections.DefaultTable;
        int recordAddress = connectionTable + (int)grapple.FireDirection * 4;
        var connection = GrappleConnectionDefinitions.ResolveConnection(recordAddress);
        GrappleConnectionFunction nextFunction = connection.Function;
        GrappleConnectionHandler handler = connection.Handler;

        // Each tiny native handler installs one prospective type-$16 pose and then jumps
        // to either BA61 (swinging) or BA9B (stuck). Keep the handler addresses visible:
        // pose alone is insufficient to distinguish malformed fallback data from native data.
        (SamusPoseId pose, bool swinging) = handler switch
        {
            GrappleConnectionHandler.SwingClockwise =>
                (SamusPoseId.GrappleSwingRightPose, true),
            GrappleConnectionHandler.SwingAnticlockwise =>
                (SamusPoseId.GrappleSwingLeftPose, true),
            GrappleConnectionHandler.StandingUpRight =>
                (SamusPoseId.GrappleStandingRightPose, false),
            GrappleConnectionHandler.StandingRight =>
                (SamusPoseId.GrappleStandingDownRightPose, false),
            GrappleConnectionHandler.StandingDown =>
                (SamusPoseId.GrappleStandingDownLeftPose, false),
            GrappleConnectionHandler.StandingUpLeft =>
                (SamusPoseId.GrappleStandingLeftPose, false),
            GrappleConnectionHandler.CrouchingUpRight =>
                (SamusPoseId.GrappleCrouchingRightPose, false),
            GrappleConnectionHandler.CrouchingRight =>
                (SamusPoseId.GrappleCrouchingDownRightPose, false),
            GrappleConnectionHandler.CrouchingDownLeft =>
                (SamusPoseId.GrappleCrouchingDownLeftPose, false),
            GrappleConnectionHandler.CrouchingUpLeft =>
                (SamusPoseId.GrappleCrouchingLeftPose, false),
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

        // BA61 and BA9B both derive the angle from the body position that fired the beam,
        // before command 9/10 changes pose and snaps Samus to the rope. The angle is stored
        // as an integer byte in the high half of the native word; its fraction becomes zero.
        SnesAngle angle = CalculateAngleFromXY(samus.Position.Whole - grapple.Anchor);
        grapple.Angle = angle;
        grapple.MirroredAngle = grapple.Angle;
        grapple.AngularVelocity = 0;
        grapple.RopeLengthDelta = 0;
        if (grapple.RopeLength >= 64)
            grapple.RopeLength -= 24;

        // $94:AC11 publishes the native grapple-beam Start pair from the accepted endpoint,
        // angle, and possibly shortened rope. This is the hand attachment point, not the
        // independently authored Flare pair from the firing tables.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            grapple,
            angle.ToByteAngle(),
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
        grapple.WallJumpTimer = NativeCountdown.Start(0);
        grapple.CancelFromConnectedPose = false;

        // The connection helper clears delta while choosing the new pose, but its caller
        // immediately starts automatic retraction. Omitting that caller tail leaves Samus
        // dangling unless the player explicitly supplies a fresh Up edge. Apply it for
        // both block and enemy connections, never for an arbitrary restored swing state.
        grapple.RopeLengthDelta = SamusGrappleRomData.Physics.InitialConnectionRetraction;

        if (poseChange == GrapplePoseChange.Deferred)
        {
            // `$9B:BA61/$BA9B` publish special prospective command 9/10. They do not
            // execute `$91:EF4F/$EF53` inline. A super-special command can therefore keep
            // the old pose and every speed word while the Grapple function remains live.
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
    /// Applies bank $91's accepted Grapple special-pose command after the frame's
    /// super-special transition owners have declined it.
    /// </summary>
    internal static Position16<RoomPixels> ApplyPendingConnectionPose(
        SamusState samus,
        GrapplePendingConnection pending)
    {
        Ensure.NotNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        samus.Pose = pending.Pose;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation(initialFrame: 0);

        if (pending.Swinging)
        {
            // Special pose command 9 runs $9B:BD95. Swinging copies Start into Flare, then
            // chooses the angle-authored animation frame and body offsets.
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            PositionSamusFromPendulum(samus, grapple);
        }
        else
        {
            // Special pose command 10 runs $9B:BEEB. The locked body is positioned from
            // Start minus the raw NO-RUN origin table, then Flare is independently rebuilt
            // from the raw no-run flare table. Graphics-Y correction does not participate.
            var origin = GrappleFiringDefinitions.Origin((byte)grapple.FireDirection, running: false);
            var flare = ReadFlareOrigin(grapple, (byte)grapple.FireDirection, running: false);
            samus.Position = samus.Position.WithWhole(grapple.RopeStart - new Offset16<RoomPixels>(origin.X, origin.Y));
            grapple.BeamStart = samus.Position.Whole + new Offset16<RoomPixels>(flare.X, flare.Y);
            grapple.Phase = GrapplePhase.ConnectedLocked;
        }

        // $91:EF53 is shared by connection commands 9 and 10. Speed-booster bookkeeping
        // has no host field yet, but every modeled speed word cleared there is reset here.
        // AccelerationMode is deliberately retained: the native common tail does not write it.
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.Kinematics.YSpeed = default;

        return ClampPreviousPosition(samus.Position.Whole, pending.Previous);
    }

    /// <summary>
    /// Integer octant translation of <c>CalculateAngleFromXY</c> at <c>$A0:C0B1</c>.
    /// The return byte is measured clockwise from negative Y: $00 up, $40 right, $80 down,
    /// and $C0 left. Division truncates exactly as the SNES unsigned divide registers do.
    /// </summary>
    internal static SnesAngle CalculateAngleFromXY(Offset16<RoomPixels> delta)
    {
        int x = delta.X;
        int y = delta.Y;
        bool xNegative = x < 0;
        bool yNegative = y < 0;
        int absoluteX = Math.Abs(x);
        int absoluteY = Math.Abs(y);
        if (absoluteX == 0 && absoluteY == 0)
            return SnesAngle.Zero;

        // The assembly advances its pointer byte offset by four for negative X and two
        // for negative Y, then indexes a word table. Converted to a zero-based entry index,
        // those are bit one for X and bit zero for Y—not the more tempting reverse order.
        int quadrant = (xNegative ? 2 : 0) | (yNegative ? 1 : 0);
        if (absoluteY < absoluteX)
        {
            int eighths = (SnesEightEight.FromWhole(absoluteY) / absoluteX) >> 3;
            return SnesAngle.NormalizeTableIndex(quadrant switch
            {
                0 => eighths + SnesAngle.QuarterTurn.TableIndex,
                1 => SnesAngle.QuarterTurn.TableIndex - eighths,
                2 => SnesAngle.ThreeQuarterTurn.TableIndex - eighths,
                _ => SnesAngle.ThreeQuarterTurn.TableIndex + eighths,
            });
        }

        int reciprocalEighths = (SnesEightEight.FromWhole(absoluteX) / absoluteY) >> 3;
        return SnesAngle.NormalizeTableIndex(quadrant switch
        {
            0 => SnesAngle.HalfTurn.TableIndex - reciprocalEighths,
            1 => unchecked((byte)reciprocalEighths),
            2 => reciprocalEighths + SnesAngle.HalfTurn.TableIndex,
            _ => 256 - reciprocalEighths,
        });
    }

    /// <summary>Seeds the first flare and point frames and all sixteen independently timed rope segment slots.</summary>
    internal static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        // `$9B:C51E` installs counter one after initializing the rope instruction slots.
        // The debugger's already-connected seam has no preceding firing history, so it
        // begins at the same authentic first flare state rather than inventing a static OBJ.
        grapple.FlareCounter = 1;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = NativeCountdown.Start(0);
        grapple.PointAnimationTimer = NativeCountdown.Start(GrapplePointAnimationDefinitions.Delay);
        grapple.PointAnimationFrame = 0;
        // GrappleFunc_AF87 at $94:AF87 seeds sixteen independent segment instruction
        // slots. Slot 15 starts on tile $24, slot 14 on $23, slot 13 on $22, slot 12 on
        // $21, and that four-phase pattern repeats down to zero. Timer one makes $94:AFBA
        // consume each initial record on its first draw.
        for (int slot = 0; slot < grapple.SegmentAnimationFrames.Length; slot++)
        {
            grapple.SegmentAnimationTimers[slot] = NativeCountdown.Start(1);
            grapple.SegmentAnimationFrames[slot] = unchecked((byte)(slot & 3));
            grapple.SegmentAnimationStarted[slot] = false;
        }
    }

    /// <summary>Recomputes visual beam origins from the final pose while retaining the separate physical endpoint offset.</summary>
    internal static void RefreshFiringDrawOrigins(
        SamusState samus,
        SamusGrappleState grapple)
    {
        bool useRunOffsets = samus.PoseMetadata.ReadMovementKind() == SamusMovementType.Running;
        var origin = GrappleFiringDefinitions.Origin((byte)grapple.FireDirection, useRunOffsets);
        var flare = ReadFlareOrigin(grapple, (byte)grapple.FireDirection, useRunOffsets);
        sbyte graphicsYOffset = samus.PoseMetadata.ReadGraphicsYOffset();
        byte physicalYOffset = SamusPoseProjectileOriginDefinitions.ReadYOffset(samus.Pose);

        // Recompute from the final pose/position, not cached launch offsets. Physical
        // Start and presentation Flare remain separate coordinate pairs.
        grapple.RopeStart = samus.Position.Whole + new Offset16<RoomPixels>(
            origin.X, unchecked((short)(origin.Y - physicalYOffset)));
        grapple.BeamStart = samus.Position.Whole + new Offset16<RoomPixels>(
            flare.X, unchecked((short)(flare.Y - graphicsYOffset)));
    }

    /// <summary>
    /// Moves the main-flare stream position by <paramref name="delta"/> as the cartridge's sixteen-bit ADC/SBC does:
    /// the position wraps through 65535 and zero, so malformed rewind data stays visible instead of clamped.
    /// </summary>
    private static int StepFlareFrame(int frame, int delta) => unchecked((ushort)(frame + delta));

    /// <summary>Reads the installed presentation offset for the current shot direction and running state.</summary>
    private static (short X, short Y) ReadFlareOrigin(SamusGrappleState grapple, byte direction, bool running)
    {
        var offset = (grapple.FlarePlacement ?? throw new InvalidOperationException(
            "Grapple flare requires installed placement definitions."))
            .Resolve(running, direction);
        return (offset.X, offset.Y);
    }

    /// <summary>
    /// Ports the post-Samus tile-selection and small-OBJ portion of `$90:EB86/$9B:BFA5/$94:AFBA`: the endpoint
    /// animation and rope orientation select the beam's characters, which its draws bind.
    /// </summary>
    public static void DrawConnectedBeam(
        SamusGrappleState grapple,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas? artwork = null,
        SamusPoseId samusPose = 0,
        SpriteArtwork? standardObjects = null)
    {
        Ensure.NotNull(grapple);
        Ensure.NotNull(sprites);
        if (!UsesBeamSpecificDrawingPath(grapple.Phase))
            return;

        // The native pair is a begin/end range, not two animation frames. Each
        // six-call interval advances the source by $200 through four endpoint tiles.
        GrapplePointAnimationDefinitions.Advance(grapple);
        // Natively `$9B:BFA5` uploads the endpoint frame and the four rope characters for the angle's sector to
        // OBJ characters $20-$24; the draws below bind that selection over the standard OBJ page.
        SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas installed = artwork ?? throw new InvalidOperationException(
            "The grapple beam requires installed grapple artwork.");
        SpriteArtwork tiles = installed.OnStandardObjects(standardObjects ?? throw new InvalidOperationException(
            "The grapple beam requires the installed standard OBJ page."), grapple.PointAnimationFrame, grapple.Angle);

        // `$9B:BFA5` increments the shared flare counter after both tile records, saturating
        // at 120 by a signed comparison. It happens even at zero rope length; only the OAM
        // rope renderer below is conditional. This is why a newly fired beam can animate
        // its muzzle flare before its first eight-pixel body segment exists.
        if (grapple.FlareCounter < 120)
            grapple.FlareCounter++;
        if (grapple.RopeLength <= 0)
            return;

        // $94:AFCF recalculates the draw vector from endpoint-minus-flare geometry. This
        // is observably different from merely reversing EndAngle while firing, and it also
        // keeps the rope visually attached after pose-specific art-origin correction.
        SnesAngle drawAngle = CalculateAngleFromXY(grapple.Anchor - grapple.BeamStart);
        // Preserve the fractional word across segments. Rounding each individual
        // displacement shortens diagonals and rounds negative steps incorrectly.
        int stepX = ReadSignedSine(drawAngle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex)
            * SamusGrappleRomData.Rendering.SegmentSampleToFixedPoint;
        int stepY = ReadSignedSine(drawAngle.TableIndex)
            * SamusGrappleRomData.Rendering.SegmentSampleToFixedPoint;

        // $94:AFDE derives X/Y flip bits from the grapple angle while retaining the packed
        // palette-five/priority-three instruction word. Tile $20 is the connected endpoint.
        int angleHigh = grapple.Angle.TableIndex;
        int flipBits = (angleHigh & 1 << 7) >> 1;
        flipBits |= 2 * ((((angleHigh ^ flipBits) & 1 << 6) ^ 1 << 6));
        ushort flipAttributes = SnesLittleEndian.Word(0, unchecked((byte)flipBits));
        int screenXFixed = unchecked(SnesSixteenSixteen.FromWhole(grapple.BeamStart.X - layer1.X - SamusGrappleRomData.Rendering.CharacterCenterOffset));
        int screenYFixed = unchecked(SnesSixteenSixteen.FromWhole(grapple.BeamStart.Y - layer1.Y - SamusGrappleRomData.Rendering.CharacterCenterOffset));
        // Native enters the loop before decrementing quotient-minus-one: even a
        // positive length whose masked quotient is zero visits one segment.
        int segmentCount = Math.Max(1, (grapple.RopeLength / SamusGrappleRomData.Rendering.SegmentSpacing)
            & SamusGrappleRomData.Rendering.SegmentCountMask);
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // $94:AFBA walks instruction slots from 15 downward regardless of rope length.
            // The first timer expiry reads the already-selected initial record; later
            // expiries advance through $21,$22,$23,$24 and $94:B0F4's goto back to $21.
            // Keeping the one-time first expiry explicit preserves the native five-draw
            // interval without pretending that all rope pieces share one animation frame.
            int instructionSlot = SamusGrappleRomData.Rendering.FirstSegmentSlot - segment;
            if ((grapple.SegmentAnimationTimers[instructionSlot] = grapple.SegmentAnimationTimers[instructionSlot].Decrement().Remaining).IsZero)
            {
                grapple.SegmentAnimationTimers[instructionSlot] = NativeCountdown.Start(SamusGrappleRomData.Rendering.SegmentAnimationDelay);
                if (grapple.SegmentAnimationStarted[instructionSlot])
                {
                    grapple.SegmentAnimationFrames[instructionSlot] = unchecked((byte)(
                        (grapple.SegmentAnimationFrames[instructionSlot] + 1) % SamusGrappleRomData.Rendering.SegmentAnimationFrameCount));
                }
                else
                {
                    grapple.SegmentAnimationStarted[instructionSlot] = true;
                }
            }

            ushort screenX = SnesFixedPosition.WholeOf(screenXFixed);
            ushort screenY = SnesFixedPosition.WholeOf(screenYFixed);
            // Native ticks this slot before the visibility check, then abandons
            // the remaining rope slots entirely at the first off-screen segment.
            if (((screenX | screenY) & SamusGrappleRomData.Rendering.SegmentOutsideViewportMask) != 0)
                break;
            ushort appearance = installed.Sprites?.Segment(grapple.SegmentAnimationFrames[instructionSlot])
                ?? unchecked((ushort)(SamusGrappleRomData.Rendering.FirstSegmentAttributes + grapple.SegmentAnimationFrames[instructionSlot]));
            var segmentAttributes = new SnesObjAttributeWord(unchecked((ushort)(appearance | flipAttributes)));
            sprites.Add(new SpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleRope, segment),
                ObjPartFrames.SmallObject(segmentAttributes, 0), tiles,
                new Position16<ScreenPixels>(screenX, screenY), SnesTileFlipFlags.None,
                (byte)segmentAttributes.PaletteIndex, (byte)segmentAttributes.Priority));
            screenXFixed = unchecked(screenXFixed + stepX);
            screenYFixed = unchecked(screenYFixed + stepY);
        }

        DrawBeamEndpoint(grapple, sprites, tiles, layer1, samusPose,
            installed.Sprites?.Endpoint ?? SamusGrappleRomData.Rendering.EndpointAttributes);
    }

    /// <summary>Preserves the pose-selected endpoint routines at $94:B0F9/B14B.</summary>
    private static void DrawBeamEndpoint(SamusGrappleState grapple, SpriteFrameBuffer sprites, SpriteArtwork tiles,
        Position16<RoomPixels> layer1, SamusPoseId samusPose, ushort attributes)
    {
        bool swinging = samusPose is SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose;
        ushort relativeY = unchecked((ushort)(grapple.Anchor.Y - layer1.Y));
        // The ordinary endpoint tests its center before subtracting the sprite
        // half-width. Swinging deliberately bypasses this vertical rejection.
        if (!swinging && (SnesLittleEndian.HighByteInPlace(relativeY)) != 0)
            return;

        // Only the swinging routine omits SEC between its two SBC operations.
        // Consequently unsigned camera subtraction can carry a borrow into the
        // half-character offset, even when wrapped coordinates look adjacent.
        int borrowX = swinging && grapple.Anchor.X < layer1.X ? 1 : 0;
        int borrowY = swinging && grapple.Anchor.Y < layer1.Y ? 1 : 0;
        var endpoint = new SnesObjAttributeWord(attributes);
        sprites.Add(new SpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleEndpoint),
            ObjPartFrames.SmallObject(endpoint, 0), tiles,
            new Position16<ScreenPixels>(unchecked((ushort)(grapple.Anchor.X - layer1.X - SamusGrappleRomData.Rendering.CharacterCenterOffset - borrowX)), unchecked((ushort)(relativeY - SamusGrappleRomData.Rendering.CharacterCenterOffset - borrowY))),
            SnesTileFlipFlags.None, (byte)endpoint.PaletteIndex, (byte)endpoint.Priority));
    }

    /// <summary>
    /// Completes the one-frame cancellation function installed by firing. The native
    /// routine also clears palette/sound bookkeeping not yet modeled by this runtime;
    /// every gameplay-visible firing word owned by this class is cleared here.
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
        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Stop);
        // The held route takes $90:F41E's no-op, not a connected-pose collision exit.
        // Draygon, rather than terrain ejection, still owns Samus's coordinates.
        if (!samus.DraygonGrabbed.IsActive)
            SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        if (cancelledConnectedPose)
        {
            // `$9B:C856` calls `$91:82D9` while the current movement type is `$16`.
            // Its command-six table entry kills all X/run momentum and the pose-definition
            // byte selects the exact standing/crouching body that existed before locking.
            SamusPoseId fallback = samus.PoseMetadata.ReadNoInputFallbackPose();
            if (fallback == SamusMovementRomData.Poses.RetainCurrentPoseFallback)
            {
                throw new InvalidDataException(
                    $"Connected grapple pose ${(int)samus.Pose:X2} has no cancellation fallback.");
            }

            samus.Pose = fallback;
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation(initialFrame: 0);
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            samus.HorizontalSpeed.BaseSpeed = default;
            samus.HorizontalSpeed.ExtraRunSpeed = default;
        }

        grapple.Phase = GrapplePhase.Inactive;
        // $9B:C66F/$C884 (cancel) and $90:F4E9 (kill) clear the slow-scroll flag.
        grapple.SlowScrolling = false;
        grapple.PoseChangeAutoFireTimer = NativeCountdown.Start(0);
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.ExtensionVelocity = default(EightEightVector<RoomPixels>);
        grapple.EndpointOffset = FixedVector<RoomPixels>.Zero;
        grapple.CancelFromConnectedPose = false;
        ClearFlareAnimation(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Fired: false,
            OwnsMovement: cancelledConnectedPose);
    }

    /// <summary>$9B:C6B2: held moving poses aim away from Draygon; Down wins over Up.</summary>
    private static byte ReadDraygonHeldDirection(SamusPoseId pose, SnesButton input)
    {
        bool left = pose == SamusPoseId.DraygonGrabbedMovingLeftPose;
        bool outward = (input & (left ? SnesButton.Left : SnesButton.Right)) != SnesButton.None;
        SamusProjectileDirection direction = left ? SamusProjectileDirection.Left : SamusProjectileDirection.Right;
        if (outward && (input & SnesButton.Down) != 0)
            direction = left ? SamusProjectileDirection.DownLeft : SamusProjectileDirection.DownRight;
        else if (outward && (input & SnesButton.Up) != 0)
            direction = left ? SamusProjectileDirection.UpLeft : SamusProjectileDirection.UpRight;
        return (byte)direction;
    }

    /// <summary>Firing branch of native $9B:C490/$B861, before the selected grapple function runs.</summary>
    internal static GrappleMovementResult? HandleFiringPoseChange(
        RoomLevelData level, SamusState samus, SnesButton controllerInput)
    {
        var grapple = samus.Grapple;
        grapple.PoseChangeAutoFireTimer = grapple.PoseChangeAutoFireTimer.DecrementToZero();
        byte direction = samus.PoseMetadata.ReadShotDirection();
        bool banned = GrappleConnectionDefinitions.CancelsFiring(samus.PoseMetadata.ReadMovementType());
        if (!banned && direction < 16)
        {
            if (direction == (byte)grapple.FireDirection) return null;
            if (!grapple.PoseChangeAutoFireTimer.IsZero)
            {
                // Native changes the function pointer before dispatch: restart from
                // the current pose and hand position this frame, without extending.
                // Do not steer an old endpoint or override the pose from held aim keys.
                QueueGrappleSound(samus, SamusGrappleRomData.Sounds.RestartStop);
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
        samus.LiquidPhysics.QueueMovementSound(request.SoundEffect, request.MaximumQueued);

    /// <summary>Places Samus at the pendulum body position and updates rope art, flare, and slow-scroll state.</summary>
    internal static void PositionSamusFromPendulum(
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $9B:BD9B-BDB1: a fast swing selects the slow-scrolling camera branch.
        ushort angularSpeed = unchecked((ushort)Math.Abs((int)grapple.AngularVelocity));
        grapple.SlowScrolling = unchecked((short)(angularSpeed -
            GrappleSlowScrollDefinitions.FastSwingAngularSpeed)) >= 0;
        // $94:AC11 calls the same position helper used by terrain probes. Besides scaling the
        // signed table components, it applies the block-side 7/8 endpoint bias documented in
        // CalculateCollisionPoint; using one helper prevents visible art from disagreeing
        // with the collision body by one pixel.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            grapple,
            grapple.Angle.ToByteAngle(),
            grapple.RopeLength);
        grapple.RopeStart = ropeStart.Pixel;

        // $9B:BD95 copies native Start to native Flare during swinging. BeamStart retains
        // the older public name because the renderer and debugger already consume it, but
        // it semantically represents the Flare/draw origin throughout this translation.
        grapple.BeamStart = ropeStart.Pixel;

        // Native shares a selector between physical body placement and displayed art.
        // Keep the authored physical mapping compiled so a visual-frame override cannot
        // move the collision body. Stock art still selects the same native frame.
        byte artFrame = (grapple.SwingFrames ?? throw new InvalidOperationException(
            "Grapple swing requires installed frame definitions."))
            .Resolve(grapple.MirroredAngle.ToByteAngle());
        var offset = GrappleBodyPlacementDefinitions.Offset(grapple.MirroredAngle.ToByteAngle(),
            SamusPoseMetadata.IsFacingLeft(samus.Pose));

        samus.Presentation.SetGrappleSwingAnimationFrame(artFrame);
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(ropeStart.Pixel.X + offset.X)), unchecked((ushort)(ropeStart.Pixel.Y + offset.Y))));
    }

    /// <summary>Converts the rope's angular velocity into launch speeds and facing direction on release.</summary>
    internal static void PropelSamusFromSwing(
        SamusState samus,
        SamusGrappleState grapple)
    {
        int absoluteVelocity = Math.Abs((int)grapple.AngularVelocity);
        int doubledVelocity = absoluteVelocity * 2;
        short cosine = ReadSignedSine(
            grapple.Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);

        uint verticalFixed = unchecked((uint)(Math.Abs(cosine) * doubledVelocity));
        samus.Kinematics.YSpeed = SnesUnsignedSixteenSixteen.FromRaw(verticalFixed);

        // $9B:CA65 selects Y direction from both angular-velocity sign and cosine sign.
        // Keeping that branch literal avoids deriving a screen-coordinate convention.
        bool movingDown = grapple.AngularVelocity >= 0 ? cosine >= 0 : cosine < 0;
        samus.Kinematics.YDirection = movingDown ? SamusVerticalDirection.Down : SamusVerticalDirection.Up;

        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        int phaseOffset = 64 - 3 * (doubledVelocity >> 9);
        byte horizontalAngle = grapple.Angle.AddTableUnits(-phaseOffset).TableIndex;
        int horizontalSine = Math.Abs(ReadSignedSine(horizontalAngle + 64));
        uint horizontalFixed = unchecked((uint)(horizontalSine * doubledVelocity));
        samus.HorizontalSpeed.BaseSpeed = SnesUnsignedSixteenSixteen.FromRaw(horizontalFixed);
    }

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
        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(level, samus.Kinematics);
        // $9B:CB8B bases facing on the angular-velocity word retained from the swing. A
        // nonnegative value selects left-facing $52; a negative value selects right $51.
        // It publishes the pose as a super-special prospective pose (command 7), which
        // bank $91 commits after this frame's hit interruption and movement.
        SamusPoseId releasePose = grapple.AngularVelocity >= 0
            ? SamusPoseId.NormalJumpForwardLeftPose
            : SamusPoseId.NormalJumpForwardRightPose;
        if (!deferPoseChange)
            ApplyReleasePose(samus, releasePose);
        grapple.Phase = GrapplePhase.Inactive;
        grapple.SlowScrolling = false; // $9B:CBBA
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.RopeLengthDelta = 0;
        grapple.CollisionBounceTimer = NativeCountdown.Start(0);
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        ClearFlareAnimation(grapple);
        return deferPoseChange ? releasePose : null;
    }

    /// <summary>Commits the <c>$9B:CB8B</c> release pose and its radius and animation.</summary>
    internal static void ApplyReleasePose(SamusState samus, SamusPoseId releasePose)
    {
        samus.Pose = releasePose;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation(initialFrame: 0);
    }

    /// <summary>Resets grapple-owned movement and rendering state so a later shot cannot inherit connection state.</summary>
    private static void ClearConnectedGrapple(SamusGrappleState grapple)
    {
        // `$9B:C8C5/$C9CE` share the same long cleanup tail. Palette, sound, and HUD-item
        // producers live outside this state object; every owned movement/rendering word is
        // reset here so a later firing edge cannot inherit wall-grace or collision state.
        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.AngularVelocity = 0;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = NativeCountdown.Start(0);
        grapple.SpecialAngleHandling = false;
        // $9B:C979 (dropped) and $9B:CA24 (wall jumping) clear the slow-scroll flag.
        grapple.SlowScrolling = false;
        grapple.WallJumpTimer = NativeCountdown.Start(0);
        grapple.CancelFromConnectedPose = false;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        ClearFlareAnimation(grapple);
    }

    /// <summary>Clears the three charge-flare words shared by grapple cancellation and connection cleanup.</summary>
    private static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // `$9B:C856/$C8C5/$C9CE/$CB8B` all clear these same shared charge/grapple WRAM
        // words before restoring the ordinary draw handler and projectile palette.
        grapple.FlareCounter = 0;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = NativeCountdown.Start(0);
    }

    /// <summary>Scales a signed 8.8 trigonometric component, preserving exact positive and negative unit endpoints.</summary>
    private static int ScaleCoordinate(short sine, int length) => sine switch
    {
        -256 => -length,
        256 => length,
        < 0 => -(SnesEightEight.Multiply(-sine, length)),
        _ => SnesEightEight.Multiply(sine, length),
    };

    /// <summary>Reads the signed negative-cosine table used to project grapple angles into room coordinates.</summary>
    private static short ReadSignedSine(int index)
    {
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }

    /// <summary>Changes rope length on Up or Down edges and pumps the swing only where directional input can accelerate it.</summary>
    private static void ApplyRopeAndDirectionInput(
        SamusGrappleState grapple,
        SnesButton controllerInput,
        SnesButton newlyPressedInput)
    {
        // $9B:BB64 changes length only on a new directional edge. Once selected, the signed
        // delta remains live until a bound or future collision code clears it.
        if ((newlyPressedInput & SnesButton.Up) != 0)
        {
            if (grapple.RopeLength != 0)
                grapple.RopeLengthDelta = -2;
        }
        else if ((newlyPressedInput & SnesButton.Down) != 0)
        {
            if (grapple.RopeLength < 64)
                grapple.RopeLengthDelta = 2;
            else
                grapple.RopeLength = 64;
        }

        // The branch accepts pumping only through the lower half of the circle:
        // angle $4000..$BFFF. At exactly $8000 a motionless pendulum gets the cartridge's
        // +/-$0100 kick before the normal +/-12 input acceleration is added.
        bool inPumpArc = grapple.Angle.RawValue >= SnesAngle.QuarterTurn.RawValue &&
            grapple.Angle.RawValue < SnesAngle.ThreeQuarterTurn.RawValue;
        if (!inPumpArc)
        {
            grapple.DirectionInputAcceleration = 0;
            return;
        }

        if ((controllerInput & SnesButton.Left) != 0)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = 256;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)(SamusGrappleRomData.Physics.DirectionInputMagnitude / 2)
                : SamusGrappleRomData.Physics.DirectionInputMagnitude;
            return;
        }

        if ((controllerInput & SnesButton.Right) != 0)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = -256;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)-(SamusGrappleRomData.Physics.DirectionInputMagnitude / 2)
                : (short)-SamusGrappleRomData.Physics.DirectionInputMagnitude;
            return;
        }

        grapple.DirectionInputAcceleration = 0;
    }

    /// <summary>Walks each intermediate rope length against terrain, stopping at the last clear length to prevent tunneling.</summary>
    private static bool ApplyRopeLengthDelta(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        if (grapple.RopeLengthDelta == 0)
            return false;

        // $94:AC31 does not jump directly by two pixels. It walks each intermediate length
        // and collision-tests Samus's leading edge, which prevents a fast rope adjustment
        // from tunnelling through a one-pixel boundary. Shortening probes length+8; growing
        // probes length+56, matching the front/back ends of the 48-pixel body line.
        int targetLength = grapple.RopeLength + grapple.RopeLengthDelta;
        int direction;
        int frontBoundaryOffset;
        if (grapple.RopeLengthDelta < 0)
        {
            direction = -1;
            frontBoundaryOffset = 8;
            if (targetLength < 8)
            {
                targetLength = 8;
                grapple.RopeLengthDelta = 0;
            }
        }
        else
        {
            direction = 1;
            frontBoundaryOffset = 56;
            if (targetLength >= 63)
            {
                targetLength = 63;
                grapple.RopeLengthDelta = 0;
            }
        }

        int currentLength = grapple.RopeLength;
        while (currentLength != targetLength)
        {
            int candidateLength = currentLength + direction;
            int probeDistance = candidateLength + frontBoundaryOffset;
            GrappleCollisionPoint point = CalculateCollisionPoint(
                grapple,
                grapple.Angle.ToByteAngle(),
                probeDistance);
            if (IsSwingCollision(level, samus, point.Block))
            {
                // Native stores GrappleCollision_NewBeamLength, which is the last accepted
                // length rather than the colliding candidate. It deliberately leaves the
                // signed delta alive so a held adjustment retries on the following frame.
                grapple.RopeLength = currentLength;
                return true;
            }

            currentLength = candidateLength;
        }

        grapple.RopeLength = targetLength;
        return false;
    }

    /// <summary>
    /// The D-pad kick-start ($9B:BBB9/BBF0) compares only the angle's high byte with $80;
    /// the release-time drop test ($9B:C7AC) instead requires exactly $8000.
    /// </summary>
    private static bool HangsStraightDown(SamusGrappleState grapple) =>
        grapple.Angle.TableIndex == SnesAngle.HalfTurn.TableIndex;

    /// <summary>Assigns quadrant-based gravity and correction, attenuated underwater and stopped near the downward rest angle.</summary>
    private static void CalculateGravity(SamusGrappleState grapple)
    {
        // $9B:BC1F is deliberately quadrant-based rather than trigonometric. The four
        // magnitudes are 1/4, full, full, and 1/4 gravity as angle crosses $4000 boundaries.
        int angle = grapple.Angle.RawValue;
        if ((angle & 3 << 14) == 3 << 14)
        {
            grapple.VelocityCorrection = -(
                SamusGrappleRomData.Physics.VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)-(SamusGrappleRomData.Physics.GravityMagnitude >> 3)
                : (short)-(SamusGrappleRomData.Physics.GravityMagnitude >> 2);
        }
        else if ((angle & 1 << 15) != 0)
        {
            // $9B:BC7B-BC84 compares only the high byte: all of $8000-$80FF hangs straight down.
            if (HangsStraightDown(grapple))
            {
                grapple.GravityAcceleration = 0;
                grapple.VelocityCorrection = 0;
                if (Math.Abs((int)grapple.AngularVelocity) < 256)
                    grapple.AngularVelocity = 0;
            }
            else
            {
                grapple.VelocityCorrection =
                    -SamusGrappleRomData.Physics.VelocityCorrectionMagnitude;
                grapple.GravityAcceleration = grapple.Submerged
                    ? (short)-(SamusGrappleRomData.Physics.GravityMagnitude >> 1)
                    : (short)-SamusGrappleRomData.Physics.GravityMagnitude;
            }
        }
        else if ((angle & 1 << 14) != 0)
        {
            grapple.VelocityCorrection =
                SamusGrappleRomData.Physics.VelocityCorrectionMagnitude;
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(SamusGrappleRomData.Physics.GravityMagnitude >> 1)
                : SamusGrappleRomData.Physics.GravityMagnitude;
        }
        else
        {
            grapple.VelocityCorrection =
                SamusGrappleRomData.Physics.VelocityCorrectionMagnitude >> 2;
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(SamusGrappleRomData.Physics.GravityMagnitude >> 3)
                : (short)(SamusGrappleRomData.Physics.GravityMagnitude >> 2);
        }
    }

    /// <summary>Combines input, gravity, and sign-sensitive correction before clamping pendulum angular velocity.</summary>
    private static void IntegrateAngularVelocity(SamusGrappleState grapple)
    {
        int velocity = grapple.AngularVelocity +
            grapple.DirectionInputAcceleration + grapple.GravityAcceleration;

        // $9B:BCFF adds the small correction when velocity and angle have opposite signs.
        // This looks unusual in decimal, but preserving the 16-bit sign-bit comparison is
        // essential around the $0000/$FFFF angle seam.
        if (((unchecked((ushort)velocity) ^ grapple.Angle.RawValue) &
             SnesAngle.HalfTurn.RawValue) != 0)
            velocity += grapple.VelocityCorrection;

        grapple.AngularVelocity = unchecked((short)Math.Clamp(
            velocity,
            -SamusGrappleRomData.Physics.MaximumAngularVelocity,
            SamusGrappleRomData.Physics.MaximumAngularVelocity));
    }

    /// <summary>Accepts a fresh Jump during the terrain-reflection window as a temporary angular kick, halved underwater.</summary>
    private static void ApplyJumpImpulse(SamusGrappleState grapple, SnesButton newlyPressedInput)
    {
        // $9B:BD44 only admits this extra angular impulse during the 16-frame terrain-
        // reflection timer. It is separate from ordinary Samus jump velocity and decays
        // inside bank $94 only after a successful angular movement pass.
        // Inputs here are normalized: A is Jump, B is Run. The native BIT JumpBinding
        // must agree with the release-window check, or tight anchors become inescapable
        // using Jump even though the unrelated Run button accidentally supplies the kick.
        if (!grapple.CollisionBounceTimer.IsZero &&
            (newlyPressedInput & SnesButton.A) != 0)
        {
            grapple.JumpImpulse = grapple.AngularVelocity switch
            {
                > 0 => grapple.Submerged
                    ? (short)(SamusGrappleRomData.Physics.JumpImpulseMagnitude / 2)
                    : SamusGrappleRomData.Physics.JumpImpulseMagnitude,
                < 0 => grapple.Submerged
                    ? (short)-(SamusGrappleRomData.Physics.JumpImpulseMagnitude / 2)
                    : (short)-SamusGrappleRomData.Physics.JumpImpulseMagnitude,
                _ => 0,
            };
        }
    }

    /// <summary>Sweeps each traversed angle-table byte against terrain and reflects or locks at the last clear angle.</summary>
    private static GrappleSwingCollisionResult AdvanceAngleWithTerrainCollision(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $94:ACFE first converts angular velocity into an angle delta, then walks from the
        // current high angle byte to the target one byte at a time. Fractional-only motion
        // within the same byte has no sweep, exactly as the target-byte equality branch.
        int combined = unchecked((short)(grapple.AngularVelocity + grapple.JumpImpulse));
        int factor = grapple.Submerged ? 160 : 256;
        int magnitude = SnesEightEight.Multiply(Math.Abs(combined), factor);
        if (magnitude == 0)
            return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);

        int signedDelta = combined < 0 ? -magnitude : magnitude;
        SnesAngle targetAngle = grapple.Angle.AddRaw(signedDelta);
        byte targetAngleByte = targetAngle.TableIndex;
        byte lastSafeAngleByte = grapple.Angle.TableIndex;
        int byteDirection = combined < 0 ? -1 : 1;

        // At the maximum retail velocity this loop executes at most five times. Keep an
        // explicit 256-byte guard anyway: it documents the cyclic angle domain and turns a
        // future corrupt state into a diagnostic rather than an infinite host loop.
        for (int angleSteps = 0;
             lastSafeAngleByte != targetAngleByte && angleSteps < 256;
             angleSteps++)
        {
            byte candidateAngleByte = unchecked((byte)(lastSafeAngleByte + byteDirection));
            GrappleSwingCollisionResult collision = ProbeSwingingBody(
                level,
                samus,
                grapple,
                new SnesByteAngle(candidateAngleByte));
            if (collision.Collided)
            {
                // $94:ADB4/$AE84 restore the last safe whole angle byte and force the
                // fractional half-byte `$80`. The body therefore stops just before the
                // colliding sample rather than snapping back to the frame's original angle.
                grapple.Angle = SnesAngle.FromRaw(
                    SnesAngle.FromTableIndex(unchecked(lastSafeAngleByte)).AddRaw(128).RawValue);
                grapple.MirroredAngle = grapple.Angle;

                bool closeCollision = grapple.RopeLength == 8 &&
                    collision.DistanceFromFeet is 6 or 5;
                if (closeCollision)
                {
                    // This bit asks bank $9B to compare against the eight exact locked and
                    // wallgrab angles. Velocity is stopped instead of reflected.
                    grapple.SpecialAngleHandling = true;
                    grapple.AngularVelocity = 0;
                    grapple.JumpImpulse = 0;
                }
                else
                {
                    grapple.CollisionBounceTimer = NativeCountdown.Start(16);
                    grapple.AngularVelocity = NegatedArithmeticHalf(grapple.AngularVelocity);
                    grapple.JumpImpulse = NegatedArithmeticHalf(grapple.JumpImpulse);
                }

                return collision;
            }

            lastSafeAngleByte = candidateAngleByte;
        }

        if (lastSafeAngleByte != targetAngleByte)
            throw new InvalidDataException("Grapple angle sweep exceeded one full revolution.");

        // No sampled byte collided, so preserve the full 16-bit target, clear the special
        // angle request, age the kick window, and damp extra velocity by six. Every one of
        // these operations is bypassed by the collision return above in the original code.
        grapple.Angle = targetAngle;
        grapple.MirroredAngle = targetAngle;
        grapple.SpecialAngleHandling = false;

        grapple.CollisionBounceTimer = grapple.CollisionBounceTimer.DecrementToZero();

        // $94:ACFE damps the temporary jump impulse toward zero by six after a successful
        // angle step. This word is separate from angular velocity and must not be folded in.
        if (grapple.JumpImpulse > 0)
            grapple.JumpImpulse = (short)Math.Max(0, grapple.JumpImpulse - 6);
        else if (grapple.JumpImpulse < 0)
            grapple.JumpImpulse = (short)Math.Min(0, grapple.JumpImpulse + 6);

        return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);
    }

    /// <summary>Checks six points spaced eight pixels along Samus's radial body line at a candidate swing angle.</summary>
    private static GrappleSwingCollisionResult ProbeSwingingBody(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        SnesByteAngle candidateAngle)
    {
        // $94:ABE6 checks six points on an eight-pixel cadence. The first point is eight
        // pixels beyond the hand; the last is 48 pixels beyond it. Together with rope length
        // this approximates Samus's body as a radial line while rotating around the anchor.
        int distance = grapple.RopeLength + 8;
        for (int distanceFromFeet = 6; distanceFromFeet >= 1; distanceFromFeet--)
        {
            GrappleCollisionPoint point = CalculateCollisionPoint(
                grapple,
                candidateAngle,
                distance);
            if (IsSwingCollision(level, samus, point.Block))
            {
                return new GrappleSwingCollisionResult(
                    Collided: true,
                    DistanceFromFeet: distanceFromFeet);
            }

            distance += 8;
        }

        return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);
    }

    /// <summary>Projects a body probe from the block-biased anchor and wraps its room-block coordinates to native bytes.</summary>
    internal static GrappleCollisionPoint CalculateCollisionPoint(
        SamusGrappleState grapple,
        SnesByteAngle angle,
        int distance)
    {
        short xSine = ReadSignedSine(angle.RawValue + 64);
        short yNegativeCosine = ReadSignedSine(angle.RawValue);

        // For a block anchor, $94:A95F-$A996 biases the low nibble toward the side from
        // which the rope leaves the block: eight for a nonnegative component, seven for a
        // negative one. This one-pixel asymmetry is observable in both collision probes and
        // Samus positioning, and the adjusted endpoint remains stored after the helper.
        grapple.Anchor = new(
            unchecked((ushort)((grapple.Anchor.X & ~15) | (xSine >= 0 ? 8 : 7))),
            unchecked((ushort)((grapple.Anchor.Y & ~15) | (yNegativeCosine >= 0 ? 8 : 7))));

        Position16<RoomPixels> point = grapple.Anchor + new Offset16<RoomPixels>(
            unchecked((short)ScaleCoordinate(xSine, distance)),
            unchecked((short)ScaleCoordinate(yNegativeCosine, distance)));

        // The native helper masks each shifted coordinate to one byte. Rooms are authored
        // within that domain; preserving the mask matters at 16-bit world-coordinate wrap.
        return new GrappleCollisionPoint(
            Pixel: point,
            Block: new(unchecked((byte)(point.X >> 4)), unchecked((byte)(point.Y >> 4))));
    }

    /// <summary>Resolves grapple swing collision through block extensions, including damaging spike reactions.</summary>
    private static bool IsSwingCollision(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomBlocks> blockPosition)
    {
        RoomCollisionBlock initialBlock =
            level.GetCollisionBlockOrPrefilledSolid(blockPosition);
        if (initialBlock.Index < 0)
            return true;

        int index = initialBlock.Index;
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            RoomCollisionBlock block =
                level.GetCollisionBlockByIndexOrPrefilledSolid(index);
            switch (block.CollisionType)
            {
                // The swing dispatcher intentionally treats shootable/bombable air as air;
                // unlike a firing endpoint, body contact does not spawn their PLMs.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnusedAir:
                case RoomCollisionType.BombableAir:
                    return false;

                case RoomCollisionType.SpikeAir:
                    // `$94:AA9E` has a mostly-zero BTS table: only spike-air BTS two queues
                    // `$0010` damage. It never collides, but it still starts the common
                    // 60-frame invulnerability and 10-frame knockback timers. The timer
                    // check must precede the BTS lookup so a second radial probe in this
                    // same six-point sweep cannot queue damage again.
                    ApplySwingSpikeDamage(samus, block.Bts.Value, block: SwingSpikeBlock.Plain);
                    return false;

                // Slopes are unconditional collision in this grapple-specific dispatcher.
                case RoomCollisionType.Slope:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpecialBlock:
                case RoomCollisionType.ShootableBlock:
                case RoomCollisionType.GrappleBlock:
                case RoomCollisionType.BombableBlock:
                    return true;

                case RoomCollisionType.SpikeBlock:
                    // `$94:AB17` always reports collision. Before setting carry it applies
                    // `$003C` for BTS zero or `$0010` for BTS one; every other entry is
                    // literally zero. A negative BTS skips the table entirely.
                    ApplySwingSpikeDamage(samus, block.Bts.Value, block: SwingSpikeBlock.Solid);
                    return true;

                case RoomCollisionType.HorizontalExtension:
                    if (block.Bts == RoomBlockBehaviorValues.None)
                        return false;
                    index += block.Bts.ExtensionOffset;
                    continue;

                case RoomCollisionType.VerticalExtension:
                    if (block.Bts == RoomBlockBehaviorValues.None)
                        return false;
                    index += block.Bts.ExtensionOffset * level.Size.X;
                    continue;

                default:
                    throw new InvalidDataException(
                        $"Invalid grapple swing collision type ${(int)block.CollisionType:X1}.");
            }
        }

        throw new InvalidDataException("Grapple swing extension chain exceeded sixteen blocks.");
    }

    /// <summary>Rechecks the stored endpoint through the firing dispatcher and retains the connection when carry is set.</summary>
    private static bool IsStillConnectedToSupportedBlock(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $9B:B8F1 calls the firing block dispatcher again at the stored endpoint and tests
        // carry only. Persistent type-$E/BTS-$00 or $03 therefore stays connected; replacing
        // it with air disconnects. PLM-producing dynamic blocks retain their explicit stop.
        GrappleBlockReaction reaction = ReactAtEndpoint(
            level,
            samus,
            grapple.Anchor);
        return reaction.Collides();
    }

    /// <summary>Applies BTS-specific spike damage once per invincibility window and starts knockback.</summary>
    private static void ApplySwingSpikeDamage(
        SamusState samus,
        int behavior,
        SwingSpikeBlock block)
    {
        // Both native handlers return immediately while `$18A8` is nonzero or when BTS is
        // negative. This shared guard is observable because one sweep can sample the same
        // damaging block up to six times, yet only its first sample may publish damage.
        if (!samus.InvincibilityTimer.IsZero || new RoomBlockBehavior(behavior).RejectsGrappleReaction)
            return;

        ushort damage = block == SwingSpikeBlock.Solid
            ? behavior switch
            {
                0 => (ushort)60,
                1 => (ushort)16,
                _ => (ushort)0,
            }
            : behavior == 2
                ? (ushort)16
                : (ushort)0;
        if (damage == 0)
            return;

        samus.LiquidPhysics.AccumulatePeriodicDamage(
            subDamage: 0,
            wholeDamage: damage);
        samus.InvincibilityTimer = NativeCountdown.Start(60);
        samus.KnockbackTimer = NativeCountdown.Start(10);
    }

    /// <summary>Negates an arithmetic-shifted half, preserving the cartridge's rounding for odd negative values.</summary>
    private static short NegatedArithmeticHalf(short value) =>
        unchecked((short)-(value >> 1));
}
