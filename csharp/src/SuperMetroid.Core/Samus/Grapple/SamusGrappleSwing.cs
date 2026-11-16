using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Swinging from a grapple point: the pendulum's angle, rope length and gravity, Samus's position and launch, and swing collision.</summary>
public static class SamusGrappleSwing
{
    /// <summary>The angle steps in a quarter turn, the lead a motionless release's horizontal speed angle takes on the rope.</summary>
    private const int QuarterTurnSteps = RotationAngle.StepsPerTurn / 4;

    /// <summary>The angle steps the release's horizontal speed angle loses of its quarter-turn lead per whole angle step of swing speed.</summary>
    private const int ReleaseLeadStepsPerSwingStep = 3;

    /// <summary>The share of gravity and of the velocity correction the swing takes above the anchor's level: a quarter.</summary>
    private const int UpperQuadrantShare = 4;

    /// <summary>The share of gravity a submerged swing takes: a half.</summary>
    private const int SubmergedGravityShare = 2;

    /// <summary>
    /// How far into its block a block anchor sits on an axis whose rope component is not negative: the rope leaves the
    /// block's middle.
    /// </summary>
    private const int AnchorBiasTowardPositive = 8;

    /// <summary>How far into its block a block anchor sits on an axis whose rope component is negative: one pixel short of its middle.</summary>
    private const int AnchorBiasTowardNegative = AnchorBiasTowardPositive - 1;

    /// <summary>Places Samus at the pendulum body position and updates rope art, flare, and slow-scroll state.</summary>
    internal static void PositionSamusFromPendulum(
        SamusState samus,
        SamusGrappleState grapple)
    {
        // A fast swing selects the slow-scrolling camera branch.
        grapple.SlowScrolling = grapple.AngularVelocity.Magnitude >= GrappleSlowScrollDefinitions.FastSwingAngularSpeed;
        // The swing calls the same position helper used by terrain probes. Besides scaling the
        // angle's signed sine and cosine, it applies the block-side 7/8 endpoint bias documented in
        // CalculateCollisionPoint; using one helper prevents visible art from disagreeing
        // with the collision body by one pixel.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            grapple,
            grapple.Angle.ToTrigonometryAngle(),
            grapple.RopeLength);
        grapple.RopeStart = ropeStart.Pixel;

        // While swinging, the flare shows and the rope is drawn from the rope's start.
        grapple.FlareOrigin = ropeStart.Pixel;

        // Body placement and displayed art both follow the swing direction nearest the mirrored angle.
        // Keep the authored physical mapping compiled so a visual-frame override cannot
        // move the collision body. Stock art still selects the same frame.
        var direction = GrappleSwingDirection.Nearest(grapple.MirroredAngle.ToTrigonometryAngle());
        GrappleSwingPicture picture = grapple.Artwork.GrappleSwingFrames.Resolve(direction);
        var offset = GrappleBodyPlacements.Offset(direction, SamusPoseMetadata.IsFacingLeft(samus.Pose));

        SamusPresentation.ShowGrappleSwingPicture(samus, picture);
        samus.Position = samus.Position.WithWhole(new(CoordinateAxis.Wrap(ropeStart.Pixel.X + offset.X), CoordinateAxis.Wrap(ropeStart.Pixel.Y + offset.Y)));
    }


    /// <summary>Converts the rope's angular velocity into launch speeds and facing direction on release.</summary>
    internal static void PropelSamusFromSwing(
        SamusState samus,
        SamusGrappleState grapple)
    {
        AngularSpeed swingSpeed = grapple.AngularVelocity.Magnitude;
        AngularSpeed releaseSpeed = swingSpeed + swingSpeed;
        UnitComponent cosine = ReadSignedSine(grapple.Angle + RotationAngle.QuarterTurn);

        // Samus leaves at twice the swing's speed, each step of turn a pixel, along each axis's share of the rope's
        // direction. The swing speed is at most four and a half steps and the component at most one, so each speed
        // stays below 2^24 65536ths of a pixel and never wraps.
        samus.Kinematics.YSpeed = cosine.SizeTimes(releaseSpeed);

        // The release selects Y direction from both angular-velocity sign and cosine sign.
        // Keeping that branch literal avoids deriving a screen-coordinate convention.
        bool movingDown = grapple.AngularVelocity.IsNegative ? cosine.IsNegative : !cosine.IsNegative;
        samus.Kinematics.YDirection = movingDown ? SamusVerticalDirection.Down : SamusVerticalDirection.Up;

        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        int phaseOffset = QuarterTurnSteps - ReleaseLeadStepsPerSwingStep * swingSpeed.WholeSteps;
        UnitComponent horizontal = UnitComponent.SineOf(grapple.Angle.TurnedBySteps(-phaseOffset).ToTrigonometryAngle());
        samus.HorizontalSpeed.BaseSpeed = horizontal.SizeTimes(releaseSpeed);
    }


    /// <summary>
    /// The signed negative cosine of <paramref name="angle"/>'s whole step: the sine a quarter turn behind it.
    /// </summary>
    internal static UnitComponent ReadSignedSine(RotationAngle angle) =>
        UnitComponent.SineOf((angle - RotationAngle.QuarterTurn).ToTrigonometryAngle());


    /// <summary>Changes rope length on Up or Down edges and pumps the swing only where directional input can accelerate it.</summary>
    internal static void ApplyRopeAndDirectionInput(
        SamusGrappleState grapple,
        GameplayAction controllerInput,
        GameplayAction newlyPressedInput)
    {
        // The swing changes length only on a new directional edge. Once selected, the signed
        // delta remains live until a bound or a later collision clears it.
        if ((newlyPressedInput & GameplayAction.Up) != GameplayAction.None)
        {
            if (grapple.RopeLength != 0)
                grapple.RopeLengthDelta = -GrappleBeamDefinitions.Swing.RopeLengthStep;
        }
        else if ((newlyPressedInput & GameplayAction.Down) != GameplayAction.None)
        {
            if (grapple.RopeLength < GrappleBeamDefinitions.Swing.RopeLengthCap)
                grapple.RopeLengthDelta = GrappleBeamDefinitions.Swing.RopeLengthStep;
            else
                grapple.RopeLength = GrappleBeamDefinitions.Swing.RopeLengthCap;
        }

        // The branch accepts pumping only through the lower half of the circle, from a quarter turn up to three quarters.
        // Hanging straight down, a motionless pendulum gets the kick before the normal input acceleration is added.
        bool inPumpArc = grapple.Angle >= RotationAngle.QuarterTurn &&
            grapple.Angle < RotationAngle.ThreeQuarterTurn;
        if (!inPumpArc)
        {
            grapple.DirectionInputAcceleration = AngularSpeed.Zero;
            return;
        }

        if ((controllerInput & GameplayAction.Left) != GameplayAction.None)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == AngularSpeed.Zero)
                grapple.AngularVelocity = GrappleBeamDefinitions.Physics.MotionlessPumpKick;
            grapple.DirectionInputAcceleration = DirectionInputPush(grapple);
            return;
        }

        if ((controllerInput & GameplayAction.Right) != GameplayAction.None)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == AngularSpeed.Zero)
                grapple.AngularVelocity = -GrappleBeamDefinitions.Physics.MotionlessPumpKick;
            grapple.DirectionInputAcceleration = -DirectionInputPush(grapple);
            return;
        }

        grapple.DirectionInputAcceleration = AngularSpeed.Zero;
    }


    /// <summary>How hard a held direction pushes the swing toward increasing angles: half as hard under water.</summary>
    private static AngularSpeed DirectionInputPush(SamusGrappleState grapple) => grapple.Submerged
        ? GrappleBeamDefinitions.Physics.DirectionInputMagnitude.DividedBy(2)
        : GrappleBeamDefinitions.Physics.DirectionInputMagnitude;


    /// <summary>Walks each intermediate rope length against terrain, stopping at the last clear length to prevent tunneling.</summary>
    internal static void ApplyRopeLengthDelta(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        if (grapple.RopeLengthDelta == 0)
            return;

        // The rope-length adjustment does not jump directly by two pixels. It walks each intermediate length
        // and collision-tests Samus's leading edge, which prevents a fast rope adjustment
        // from tunnelling through a one-pixel boundary. Shortening probes her hand end; growing
        // probes her feet end, the two ends of the body line the swing sweeps.
        int targetLength = grapple.RopeLength + grapple.RopeLengthDelta;
        int direction;
        int frontBoundaryOffset;
        if (grapple.RopeLengthDelta < 0)
        {
            direction = -1;
            frontBoundaryOffset = GrappleBeamDefinitions.Swing.ShorteningProbeOffset;
            if (targetLength < GrappleBeamDefinitions.Swing.ShortestRope)
            {
                targetLength = GrappleBeamDefinitions.Swing.ShortestRope;
                grapple.RopeLengthDelta = 0;
            }
        }
        else
        {
            direction = 1;
            frontBoundaryOffset = GrappleBeamDefinitions.Swing.LengtheningProbeOffset;
            if (targetLength >= GrappleBeamDefinitions.Swing.LongestLetOutRope)
            {
                targetLength = GrappleBeamDefinitions.Swing.LongestLetOutRope;
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
                grapple.Angle.ToTrigonometryAngle(),
                probeDistance);
            if (IsSwingCollision(level, samus, point.Block))
            {
                // The game stores the new beam length, which is the last accepted
                // length rather than the colliding candidate. It deliberately leaves the
                // signed delta alive so a held adjustment retries on the following frame.
                grapple.RopeLength = currentLength;
                return;
            }

            currentLength = candidateLength;
        }

        grapple.RopeLength = targetLength;
    }


    /// <summary>
    /// The directions' kick-start compares only the angle's whole step with straight down;
    /// the release-time drop test instead requires exactly half a turn.
    /// </summary>
    private static bool HangsStraightDown(SamusGrappleState grapple) =>
        grapple.Angle.WholeStep == RotationAngle.HalfTurn;


    /// <summary>Assigns quadrant-based gravity and correction, attenuated underwater and stopped near the downward rest angle.</summary>
    internal static void CalculateGravity(SamusGrappleState grapple)
    {
        // Swing gravity is deliberately quadrant-based rather than trigonometric. The four
        // magnitudes are 1/4, full, full, and 1/4 gravity as angle crosses quarter-turn boundaries.
        // Each quadrant test is the angle at or past a quarter-turn mark.
        // A submerged swing takes half the gravity, and the upper quadrants a quarter of that, the halving's odd part
        // dropped before the quarter is taken.
        RotationAngle angle = grapple.Angle;
        AngularSpeed gravity = grapple.Submerged
            ? GrappleBeamDefinitions.Physics.GravityMagnitude.DividedBy(SubmergedGravityShare)
            : GrappleBeamDefinitions.Physics.GravityMagnitude;
        AngularSpeed correction = GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude;
        if (angle >= RotationAngle.ThreeQuarterTurn)
        {
            grapple.VelocityCorrection = -correction.DividedBy(UpperQuadrantShare);
            grapple.GravityAcceleration = -gravity.DividedBy(UpperQuadrantShare);
        }
        else if (angle >= RotationAngle.HalfTurn)
        {
            // The hang test compares only the whole step: every angle within the half turn's step hangs straight down.
            if (HangsStraightDown(grapple))
            {
                grapple.GravityAcceleration = AngularSpeed.Zero;
                grapple.VelocityCorrection = AngularSpeed.Zero;
                if (grapple.AngularVelocity.Magnitude < GrappleBeamDefinitions.Physics.RestingAngularVelocity)
                    grapple.AngularVelocity = AngularSpeed.Zero;
            }
            else
            {
                grapple.VelocityCorrection = -correction;
                grapple.GravityAcceleration = -gravity;
            }
        }
        else if (angle >= RotationAngle.QuarterTurn)
        {
            grapple.VelocityCorrection = correction;
            grapple.GravityAcceleration = gravity;
        }
        else
        {
            grapple.VelocityCorrection = correction.DividedBy(UpperQuadrantShare);
            grapple.GravityAcceleration = gravity.DividedBy(UpperQuadrantShare);
        }
    }


    /// <summary>Combines input, gravity, and sign-sensitive correction before clamping pendulum angular velocity.</summary>
    internal static void IntegrateAngularVelocity(SamusGrappleState grapple)
    {
        AngularSpeed velocity = grapple.AngularVelocity +
            grapple.DirectionInputAcceleration + grapple.GravityAcceleration;

        // The swing adds the small correction when the velocity and the angle lean opposite ways: a negative velocity
        // with an angle in the first half-turn, or a non-negative one with an angle in the second, so the test turns
        // over where the angle comes round to zero.
        if (velocity.IsNegative != (grapple.Angle >= RotationAngle.HalfTurn))
            velocity += grapple.VelocityCorrection;

        grapple.AngularVelocity = velocity.LimitedTo(GrappleBeamDefinitions.Physics.MaximumAngularVelocity);
    }


    /// <summary>Accepts a fresh Jump during the terrain-reflection window as a temporary angular kick, halved underwater.</summary>
    internal static void ApplyJumpImpulse(SamusGrappleState grapple, GameplayAction newlyPressedInput)
    {
        // The swing only admits this extra angular impulse during the 16-frame terrain-
        // reflection timer. It is separate from ordinary Samus jump velocity and decays
        // inside the angular movement only after a successful angular movement pass.
        // Inputs here are normalized: A is Jump, B is Run. The Jump-binding test
        // must agree with the release-window check, or tight anchors become inescapable
        // using Jump even though the unrelated Dash accidentally supplies the kick.
        if (grapple.CollisionBounceTimer != 0 &&
            (newlyPressedInput & GameplayAction.Jump) != GameplayAction.None)
        {
            AngularSpeed impulse = grapple.Submerged
                ? GrappleBeamDefinitions.Physics.JumpImpulseMagnitude.DividedBy(2)
                : GrappleBeamDefinitions.Physics.JumpImpulseMagnitude;
            grapple.JumpImpulse = grapple.AngularVelocity == AngularSpeed.Zero ? AngularSpeed.Zero
                : grapple.AngularVelocity.IsNegative ? -impulse
                : impulse;
        }
    }


    /// <summary>Sweeps each traversed angle step against terrain and reflects or locks at the last clear angle.</summary>
    internal static void AdvanceAngleWithTerrainCollision(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // The angular movement first converts angular velocity into an angle delta, then walks from the
        // current whole angle step to the target one step at a time. Fractional-only motion
        // within the same step has no sweep, exactly as the target-step equality branch.
        // A submerged swing turns through only a share of its speed, rounded toward no turning.
        AngularSpeed combined = grapple.AngularVelocity + grapple.JumpImpulse;
        AngularSpeed turn = grapple.Submerged
            ? combined.ScaledBy(GrappleBeamDefinitions.Physics.SubmergedSwingShare, GrappleBeamDefinitions.Physics.SubmergedSwingParts)
            : combined;
        if (turn == AngularSpeed.Zero)
            return;

        RotationAngle targetAngle = grapple.Angle.TurnedBy(turn);
        TrigonometryAngle targetStep = targetAngle.ToTrigonometryAngle();
        TrigonometryAngle lastSafeStep = grapple.Angle.ToTrigonometryAngle();
        int stepDirection = combined.IsNegative ? -1 : 1;

        // At the maximum retail velocity this loop executes at most five times. Keep an
        // explicit one-turn guard anyway: it documents the cyclic angle domain and turns a
        // future corrupt state into a diagnostic rather than an infinite loop.
        for (int angleSteps = 0;
             lastSafeStep != targetStep && angleSteps < RotationAngle.StepsPerTurn;
             angleSteps++)
        {
            TrigonometryAngle candidateStep = lastSafeStep.TurnedBySteps(stepDirection);
            GrappleSwingCollisionResult collision = ProbeSwingingBody(
                level,
                samus,
                grapple,
                candidateStep);
            if (collision.Collided)
            {
                // A collision restores the middle of the last safe whole angle step. The body
                // therefore stops just before the colliding sample rather than snapping back
                // to the frame's original angle.
                grapple.Angle = RotationAngle.MiddleOfStep(lastSafeStep);
                grapple.MirroredAngle = grapple.Angle;

                bool closeCollision = grapple.RopeLength == GrappleBeamDefinitions.Swing.ShortestRope &&
                    collision.StruckPointFromHand < GrappleBeamDefinitions.Swing.StoppingPointsNearHand;
                if (closeCollision)
                {
                    // This bit asks the swing to compare against the eight exact locked and
                    // wallgrab angles. Velocity is stopped instead of reflected.
                    grapple.SpecialAngleHandling = true;
                    grapple.AngularVelocity = AngularSpeed.Zero;
                    grapple.JumpImpulse = AngularSpeed.Zero;
                }
                else
                {
                    grapple.CollisionBounceTimer = GrappleBeamDefinitions.Physics.CollisionBounceFrames;
                    grapple.AngularVelocity = ReversedAtHalfSpeed(grapple.AngularVelocity);
                    grapple.JumpImpulse = ReversedAtHalfSpeed(grapple.JumpImpulse);
                }

                return;
            }

            lastSafeStep = candidateStep;
        }

        if (lastSafeStep != targetStep)
            throw new InvalidDataException("Grapple angle sweep exceeded one full revolution.");

        // No sampled angle collided, so preserve the full fine target, clear the special
        // angle request, age the kick window, and damp the extra velocity. Every one of
        // these operations is bypassed by the collision return above.
        grapple.Angle = targetAngle;
        grapple.MirroredAngle = targetAngle;
        grapple.SpecialAngleHandling = false;

        grapple.CollisionBounceTimer = Math.Max(0, grapple.CollisionBounceTimer - 1);

        // The angular movement damps the temporary jump impulse toward zero after a successful
        // angle step. This impulse is separate from angular velocity and must not be folded in.
        grapple.JumpImpulse = grapple.JumpImpulse.SlowedTowardRest(GrappleBeamDefinitions.Physics.JumpImpulseDecay);
    }


    /// <summary>
    /// Checks the evenly spaced points along Samus's radial body line at a candidate swing angle, from the hand toward
    /// the feet, and reports the first that strikes terrain.
    /// </summary>
    private static GrappleSwingCollisionResult ProbeSwingingBody(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        TrigonometryAngle candidateAngle)
    {
        // The body sweep checks its points on an even cadence from just beyond the hand toward the feet. Together with
        // rope length this approximates Samus's body as a radial line while rotating around the anchor.
        for (int pointFromHand = 0; pointFromHand < GrappleBeamDefinitions.Swing.BodyPointCount; pointFromHand++)
        {
            int distance = grapple.RopeLength + ((pointFromHand + 1) * GrappleBeamDefinitions.Swing.BodyPointSpacing);
            GrappleCollisionPoint point = CalculateCollisionPoint(
                grapple,
                candidateAngle,
                distance);
            if (IsSwingCollision(level, samus, point.Block))
                return GrappleSwingCollisionResult.Struck(pointFromHand);
        }

        return GrappleSwingCollisionResult.Clear;
    }


    /// <summary>Projects a body probe from the block-biased anchor and wraps its room-block coordinates below 256.</summary>
    internal static GrappleCollisionPoint CalculateCollisionPoint(
        SamusGrappleState grapple,
        TrigonometryAngle angle,
        int distance)
    {
        UnitComponent xSine = UnitComponent.SineOf(angle);
        UnitComponent yNegativeCosine = UnitComponent.SineOf(angle - TrigonometryAngle.QuarterTurn);

        // For a block anchor, the collision point biases the position within the block toward the side from
        // which the rope leaves the block, one pixel nearer the block's start for a negative component. This one-pixel
        // asymmetry is observable in both collision probes and Samus positioning, and the adjusted endpoint remains
        // stored after the helper. The biased point stays within the anchor's block.
        grapple.Anchor = new(
            BiasedWithinBlock(grapple.Anchor.X, xSine),
            BiasedWithinBlock(grapple.Anchor.Y, yNegativeCosine));

        Position16<RoomPixels> point = grapple.Anchor + new Offset16<RoomPixels>(
            xSine.ReachOf(distance),
            yNegativeCosine.ReachOf(distance));

        // The helper takes each block column and row within the scan cycle. Rooms are authored within it;
        // keeping the cycle matters where a point wraps around the room's coordinates.
        return new GrappleCollisionPoint(
            Pixel: point,
            Block: new(
                BlockScanCycle.Within(RoomGridConversions.PixelToBlock(point.X)),
                BlockScanCycle.Within(RoomGridConversions.PixelToBlock(point.Y))));
    }

    /// <summary>
    /// The anchor's coordinate on one axis moved within its block to where the rope leaves it: the block's start plus
    /// <see cref="AnchorBiasTowardPositive"/> for a <paramref name="ropeComponent"/> that is not negative, or
    /// <see cref="AnchorBiasTowardNegative"/> for a negative one.
    /// </summary>
    private static int BiasedWithinBlock(int anchor, UnitComponent ropeComponent) =>
        RoomGridConversions.BlockStart(anchor) + (ropeComponent.IsNegative ? AnchorBiasTowardNegative : AnchorBiasTowardPositive);


    /// <summary>Resolves grapple swing collision through block extensions, including damaging spike reactions.</summary>
    private static bool IsSwingCollision(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomBlocks> blockPosition)
    {
        RoomCollisionBlock block =
            level.GetCollisionBlockOrEdgeSolid(blockPosition);
        if (block.Position is null)
            return true;

        // Each extension link leads on in the room's reading order; one that leads outside the grid reads the edge-solid block.
        for (int extensionDepth = 0; extensionDepth <= GrappleBeamDefinitions.Blocks.LongestExtensionChain; extensionDepth++)
        {
            switch (block.CollisionType)
            {
                // The swing step intentionally treats shootable/bombable air as air;
                // unlike a firing endpoint, body contact does not spawn their room objects.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnrevealedAir:
                case RoomCollisionType.BombableAir:
                    return false;

                case RoomCollisionType.SpikeAir:
                    // Spike air is harmless to the swing except damaging spike air, which queues
                    // 16 damage. It never collides, but it still starts the common
                    // 60-frame invulnerability and 10-frame knockback timers. The timer
                    // check must precede the behaviour lookup so a second radial probe in this
                    // same six-point sweep cannot queue damage again.
                    ApplySwingSpikeDamage(samus, block.Behavior.Spike, block: SwingSpikeBlock.Plain);
                    return false;

                // Slopes are unconditional collision in this grapple-specific step.
                case RoomCollisionType.Slope:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpecialBlock:
                case RoomCollisionType.ShootableBlock:
                case RoomCollisionType.GrappleBlock:
                case RoomCollisionType.BombableBlock:
                    return true;

                case RoomCollisionType.SpikeBlock:
                    // The solid spike reaction always reports collision. Before reporting it, it applies
                    // 60 damage for the heavy spike or 16 for the light spike; every other spike deals
                    // none.
                    ApplySwingSpikeDamage(samus, block.Behavior.Spike, block: SwingSpikeBlock.Solid);
                    return true;

                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                    if (block.Behavior.EndsExtension)
                        return false;
                    block = level.LinkedBlockOrEdgeSolid(block);
                    continue;

                default:
                    throw new InvalidDataException(
                        $"Invalid grapple swing collision type {block.CollisionType}.");
            }
        }

        throw new InvalidDataException("Grapple swing extension chain exceeded sixteen blocks.");
    }


    /// <summary>Rechecks the stored endpoint through the firing step and retains the connection while the block there stops the beam.</summary>
    internal static bool IsStillConnectedToSupportedBlock(
        SamusTerrain terrain,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // The anchor validation calls the firing block check again at the stored endpoint and tests
        // only whether the block stops the beam. A persistent anchor, revealed or unrevealed by the X-ray scope, therefore stays connected; replacing
        // it with air disconnects. Room-object-producing dynamic blocks retain their explicit stop.
        GrappleBlockReaction reaction = SamusGrappleMovement.ReactAtEndpoint(
            terrain.Level,
            samus,
            grapple.Anchor,
            terrain.RoomObjects);
        return reaction.Collides();
    }


    /// <summary>Applies behaviour-specific spike damage once per invincibility window and starts knockback.</summary>
    private static void ApplySwingSpikeDamage(
        SamusState samus,
        SpikeReaction? reaction,
        SwingSpikeBlock block)
    {
        // Both steps return immediately while the invincibility timer is nonzero. This shared guard is observable
        // because one sweep can sample the same damaging block up to six times, yet only its first sample may publish
        // damage. Every reaction other than the damaging ones, and a spike with no reaction, deals none.
        if (!samus.InvincibilityTimer.IsZero)
            return;

        // The swing's own selection: the turret spike, which the ground collision treats as light, costs nothing here.
        int damage = block == SwingSpikeBlock.Solid
            ? reaction == SpikeReaction.HeavySpike ? SpikeHazardDefinitions.HeavySpikeDamage
                : reaction == SpikeReaction.LightSpike ? SpikeHazardDefinitions.LightSpikeDamage
                : 0
            : reaction == SpikeReaction.DamagingSpikeAir
                ? SpikeHazardDefinitions.LightSpikeDamage
                : 0;
        if (damage == 0)
            return;

        samus.PeriodicDamage.Accumulate(
            subDamage: 0,
            wholeDamage: damage);
        samus.InvincibilityTimer = WrappingCountdown.Start(SpikeHazardDefinitions.InvincibilityFrames);
        samus.KnockbackTimer = SpikeHazardDefinitions.KnockbackFrames;
    }


    /// <summary>
    /// The swing speed a bounce off terrain reverses <paramref name="velocity"/> into: half as fast the other way, the half
    /// rounded toward negative values, so an odd negative velocity rebounds one 256th of a step faster than the odd
    /// positive velocity of the same size.
    /// </summary>
    internal static AngularSpeed ReversedAtHalfSpeed(AngularSpeed velocity) => -velocity.HalvedRoundingDown();
}
