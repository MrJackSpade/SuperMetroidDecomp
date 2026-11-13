using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Swinging from a grapple point: the pendulum's angle, rope length and gravity, Samus's position and launch, and swing collision.</summary>
public static class SamusGrappleSwing
{

    /// <summary>Places Samus at the pendulum body position and updates rope art, flare, and slow-scroll state.</summary>
    internal static void PositionSamusFromPendulum(
        SamusState samus,
        SamusGrappleState grapple)
    {
        // A fast swing selects the slow-scrolling camera branch.
        grapple.SlowScrolling = Math.Abs(grapple.AngularVelocity) >= GrappleSlowScrollDefinitions.FastSwingAngularSpeed;
        // The swing calls the same position helper used by terrain probes. Besides scaling the
        // signed table components, it applies the block-side 7/8 endpoint bias documented in
        // CalculateCollisionPoint; using one helper prevents visible art from disagreeing
        // with the collision body by one pixel.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            grapple,
            grapple.Angle.ToTrigonometryAngle(),
            grapple.RopeLength);
        grapple.RopeStart = ropeStart.Pixel;

        // The swing copies Start to Flare during swinging. BeamStart retains
        // the older public name because the renderer consumes it, but
        // it semantically represents the Flare/draw origin throughout this translation.
        grapple.BeamStart = ropeStart.Pixel;

        // The game shares a selector between physical body placement and displayed art.
        // Keep the authored physical mapping compiled so a visual-frame override cannot
        // move the collision body. Stock art still selects the same frame.
        int artFrame = (grapple.SwingFrames ?? throw new InvalidOperationException(
            "Grapple swing requires installed frame definitions."))
            .Resolve(grapple.MirroredAngle.ToTrigonometryAngle());
        var offset = GrappleBodyPlacements.Offset(grapple.MirroredAngle.ToTrigonometryAngle(),
            SamusPoseMetadata.IsFacingLeft(samus.Pose));

        SamusPresentation.SetGrappleSwingAnimationFrame(samus, artFrame);
        samus.Position = samus.Position.WithWhole(new(CoordinateAxis.Wrap(ropeStart.Pixel.X + offset.X), CoordinateAxis.Wrap(ropeStart.Pixel.Y + offset.Y)));
    }


    /// <summary>Converts the rope's angular velocity into launch speeds and facing direction on release.</summary>
    internal static void PropelSamusFromSwing(
        SamusState samus,
        SamusGrappleState grapple)
    {
        int absoluteVelocity = Math.Abs(grapple.AngularVelocity);
        int doubledVelocity = absoluteVelocity * 2;
        int cosine = ReadSignedSine(grapple.Angle + RotationAngle.QuarterTurn);

        // The swing speed is a short angular velocity doubled and the component at most one, so each product stays
        // below 2^24 65536ths of a pixel and never wraps.
        samus.Kinematics.YSpeed = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(Math.Abs(cosine), doubledVelocity);

        // The release selects Y direction from both angular-velocity sign and cosine sign.
        // Keeping that branch literal avoids deriving a screen-coordinate convention.
        bool movingDown = grapple.AngularVelocity >= 0 ? cosine >= 0 : cosine < 0;
        samus.Kinematics.YDirection = movingDown ? SamusVerticalDirection.Down : SamusVerticalDirection.Up;

        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        int phaseOffset = 64 - 3 * (doubledVelocity >> 9);
        int horizontalSine = Math.Abs(Trigonometry.SignedSine(grapple.Angle.TurnedBySteps(-phaseOffset).ToTrigonometryAngle()));
        samus.HorizontalSpeed.BaseSpeed = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(horizontalSine, doubledVelocity);
    }


    /// <summary>Scales a signed 8.8 trigonometric component, preserving exact positive and negative unit endpoints.</summary>
    private static int ScaleCoordinate(int sine, int length) => sine switch
    {
        -256 => -length,
        256 => length,
        < 0 => -(EightEight.Multiply(-sine, length)),
        _ => EightEight.Multiply(sine, length),
    };


    /// <summary>
    /// The signed negative cosine of <paramref name="angle"/>'s whole step, the table's word at that step: the sine a
    /// quarter turn behind it.
    /// </summary>
    internal static int ReadSignedSine(RotationAngle angle) =>
        Trigonometry.SignedSine((angle - RotationAngle.QuarterTurn).ToTrigonometryAngle());


    /// <summary>Changes rope length on Up or Down edges and pumps the swing only where directional input can accelerate it.</summary>
    internal static void ApplyRopeAndDirectionInput(
        SamusGrappleState grapple,
        ControllerButton controllerInput,
        ControllerButton newlyPressedInput)
    {
        // The swing changes length only on a new directional edge. Once selected, the signed
        // delta remains live until a bound or future collision code clears it.
        if ((newlyPressedInput & ControllerButton.Up) != ControllerButton.None)
        {
            if (grapple.RopeLength != 0)
                grapple.RopeLengthDelta = -2;
        }
        else if ((newlyPressedInput & ControllerButton.Down) != ControllerButton.None)
        {
            if (grapple.RopeLength < 64)
                grapple.RopeLengthDelta = 2;
            else
                grapple.RopeLength = 64;
        }

        // The branch accepts pumping only through the lower half of the circle:
        // angle 16384..49151. At exactly 32768 a motionless pendulum gets the game's
        // +/-256 kick before the normal +/-12 input acceleration is added.
        bool inPumpArc = grapple.Angle >= RotationAngle.QuarterTurn &&
            grapple.Angle < RotationAngle.ThreeQuarterTurn;
        if (!inPumpArc)
        {
            grapple.DirectionInputAcceleration = 0;
            return;
        }

        if ((controllerInput & ControllerButton.Left) != ControllerButton.None)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = 256;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (GrappleBeamDefinitions.Physics.DirectionInputMagnitude / 2)
                : GrappleBeamDefinitions.Physics.DirectionInputMagnitude;
            return;
        }

        if ((controllerInput & ControllerButton.Right) != ControllerButton.None)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = -256;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? -(GrappleBeamDefinitions.Physics.DirectionInputMagnitude / 2)
                : -GrappleBeamDefinitions.Physics.DirectionInputMagnitude;
            return;
        }

        grapple.DirectionInputAcceleration = 0;
    }


    /// <summary>Walks each intermediate rope length against terrain, stopping at the last clear length to prevent tunneling.</summary>
    internal static bool ApplyRopeLengthDelta(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        if (grapple.RopeLengthDelta == 0)
            return false;

        // The rope-length adjustment does not jump directly by two pixels. It walks each intermediate length
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
                grapple.Angle.ToTrigonometryAngle(),
                probeDistance);
            if (IsSwingCollision(level, samus, point.Block))
            {
                // The game stores the new beam length, which is the last accepted
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
    /// The D-pad kick-start compares only the angle's whole step with straight down;
    /// the release-time drop test instead requires exactly half a turn.
    /// </summary>
    private static bool HangsStraightDown(SamusGrappleState grapple) =>
        grapple.Angle.WholeStep == RotationAngle.HalfTurn;


    /// <summary>Assigns quadrant-based gravity and correction, attenuated underwater and stopped near the downward rest angle.</summary>
    internal static void CalculateGravity(SamusGrappleState grapple)
    {
        // Swing gravity is deliberately quadrant-based rather than trigonometric. The four
        // magnitudes are 1/4, full, full, and 1/4 gravity as angle crosses quarter-turn boundaries.
        // The quadrant is read from the angle's top two bits, so each test is the angle at or past a quarter-turn mark.
        RotationAngle angle = grapple.Angle;
        if (angle >= RotationAngle.ThreeQuarterTurn)
        {
            grapple.VelocityCorrection = -(
                GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? -(GrappleBeamDefinitions.Physics.GravityMagnitude >> 3)
                : -(GrappleBeamDefinitions.Physics.GravityMagnitude >> 2);
        }
        else if (angle >= RotationAngle.HalfTurn)
        {
            // The hang test compares only the whole step: every angle from 32768 through 33023 hangs straight down.
            if (HangsStraightDown(grapple))
            {
                grapple.GravityAcceleration = 0;
                grapple.VelocityCorrection = 0;
                if (Math.Abs(grapple.AngularVelocity) < 256)
                    grapple.AngularVelocity = 0;
            }
            else
            {
                grapple.VelocityCorrection =
                    -GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude;
                grapple.GravityAcceleration = grapple.Submerged
                    ? -(GrappleBeamDefinitions.Physics.GravityMagnitude >> 1)
                    : -GrappleBeamDefinitions.Physics.GravityMagnitude;
            }
        }
        else if (angle >= RotationAngle.QuarterTurn)
        {
            grapple.VelocityCorrection =
                GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude;
            grapple.GravityAcceleration = grapple.Submerged
                ? (GrappleBeamDefinitions.Physics.GravityMagnitude >> 1)
                : GrappleBeamDefinitions.Physics.GravityMagnitude;
        }
        else
        {
            grapple.VelocityCorrection =
                GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude >> 2;
            grapple.GravityAcceleration = grapple.Submerged
                ? (GrappleBeamDefinitions.Physics.GravityMagnitude >> 3)
                : (GrappleBeamDefinitions.Physics.GravityMagnitude >> 2);
        }
    }


    /// <summary>Combines input, gravity, and sign-sensitive correction before clamping pendulum angular velocity.</summary>
    internal static void IntegrateAngularVelocity(SamusGrappleState grapple)
    {
        int velocity = grapple.AngularVelocity +
            grapple.DirectionInputAcceleration + grapple.GravityAcceleration;

        // The swing adds the small correction when the velocity and the angle lean opposite ways: a negative velocity
        // with an angle in the first half-turn, or a non-negative one with an angle in the second, so the test turns
        // over at the 0/65535 angle seam.
        if ((velocity < 0) != (grapple.Angle >= RotationAngle.HalfTurn))
            velocity += grapple.VelocityCorrection;

        grapple.AngularVelocity = Math.Clamp(
            velocity,
            -GrappleBeamDefinitions.Physics.MaximumAngularVelocity,
            GrappleBeamDefinitions.Physics.MaximumAngularVelocity);
    }


    /// <summary>Accepts a fresh Jump during the terrain-reflection window as a temporary angular kick, halved underwater.</summary>
    internal static void ApplyJumpImpulse(SamusGrappleState grapple, ControllerButton newlyPressedInput)
    {
        // The swing only admits this extra angular impulse during the 16-frame terrain-
        // reflection timer. It is separate from ordinary Samus jump velocity and decays
        // inside the angular movement only after a successful angular movement pass.
        // Inputs here are normalized: A is Jump, B is Run. The Jump-binding test
        // must agree with the release-window check, or tight anchors become inescapable
        // using Jump even though the unrelated Run button accidentally supplies the kick.
        if (grapple.CollisionBounceTimer != 0 &&
            (newlyPressedInput & ControllerButton.A) != ControllerButton.None)
        {
            grapple.JumpImpulse = grapple.AngularVelocity switch
            {
                > 0 => grapple.Submerged
                    ? (GrappleBeamDefinitions.Physics.JumpImpulseMagnitude / 2)
                    : GrappleBeamDefinitions.Physics.JumpImpulseMagnitude,
                < 0 => grapple.Submerged
                    ? -(GrappleBeamDefinitions.Physics.JumpImpulseMagnitude / 2)
                    : -GrappleBeamDefinitions.Physics.JumpImpulseMagnitude,
                _ => 0,
            };
        }
    }


    /// <summary>Sweeps each traversed angle-table byte against terrain and reflects or locks at the last clear angle.</summary>
    internal static GrappleSwingCollisionResult AdvanceAngleWithTerrainCollision(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // The angular movement first converts angular velocity into an angle delta, then walks from the
        // current whole angle step to the target one step at a time. Fractional-only motion
        // within the same step has no sweep, exactly as the target-step equality branch.
        int combined = grapple.AngularVelocity + grapple.JumpImpulse;
        int factor = grapple.Submerged ? 160 : 256;
        int magnitude = EightEight.Multiply(Math.Abs(combined), factor);
        if (magnitude == 0)
            return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);

        int signedDelta = combined < 0 ? -magnitude : magnitude;
        RotationAngle targetAngle = grapple.Angle.AddRaw(signedDelta);
        TrigonometryAngle targetStep = targetAngle.ToTrigonometryAngle();
        TrigonometryAngle lastSafeStep = grapple.Angle.ToTrigonometryAngle();
        int stepDirection = combined < 0 ? -1 : 1;

        // At the maximum retail velocity this loop executes at most five times. Keep an
        // explicit 256-step guard anyway: it documents the cyclic angle domain and turns a
        // future corrupt state into a diagnostic rather than an infinite host loop.
        for (int angleSteps = 0;
             lastSafeStep != targetStep && angleSteps < 256;
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
                // A collision restores the last safe whole angle step and forces the
                // fractional half value 128. The body therefore stops just before the
                // colliding sample rather than snapping back to the frame's original angle.
                grapple.Angle = RotationAngle.FromSteps(lastSafeStep.RawValue, fraction: 128);
                grapple.MirroredAngle = grapple.Angle;

                bool closeCollision = grapple.RopeLength == 8 &&
                    collision.DistanceFromFeet is 6 or 5;
                if (closeCollision)
                {
                    // This bit asks the swing to compare against the eight exact locked and
                    // wallgrab angles. Velocity is stopped instead of reflected.
                    grapple.SpecialAngleHandling = true;
                    grapple.AngularVelocity = 0;
                    grapple.JumpImpulse = 0;
                }
                else
                {
                    grapple.CollisionBounceTimer = 16;
                    grapple.AngularVelocity = NegatedArithmeticHalf(grapple.AngularVelocity);
                    grapple.JumpImpulse = NegatedArithmeticHalf(grapple.JumpImpulse);
                }

                return collision;
            }

            lastSafeStep = candidateStep;
        }

        if (lastSafeStep != targetStep)
            throw new InvalidDataException("Grapple angle sweep exceeded one full revolution.");

        // No sampled angle collided, so preserve the full fine target, clear the special
        // angle request, age the kick window, and damp extra velocity by six. Every one of
        // these operations is bypassed by the collision return above in the original code.
        grapple.Angle = targetAngle;
        grapple.MirroredAngle = targetAngle;
        grapple.SpecialAngleHandling = false;

        grapple.CollisionBounceTimer = Math.Max(0, grapple.CollisionBounceTimer - 1);

        // The angular movement damps the temporary jump impulse toward zero by six after a successful
        // angle step. This word is separate from angular velocity and must not be folded in.
        if (grapple.JumpImpulse > 0)
            grapple.JumpImpulse = Math.Max(0, grapple.JumpImpulse - 6);
        else if (grapple.JumpImpulse < 0)
            grapple.JumpImpulse = Math.Min(0, grapple.JumpImpulse + 6);

        return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);
    }


    /// <summary>Checks six points spaced eight pixels along Samus's radial body line at a candidate swing angle.</summary>
    private static GrappleSwingCollisionResult ProbeSwingingBody(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        TrigonometryAngle candidateAngle)
    {
        // The body sweep checks six points on an eight-pixel cadence. The first point is eight
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


    /// <summary>Projects a body probe from the block-biased anchor and wraps its room-block coordinates to bytes.</summary>
    internal static GrappleCollisionPoint CalculateCollisionPoint(
        SamusGrappleState grapple,
        TrigonometryAngle angle,
        int distance)
    {
        int xSine = Trigonometry.SignedSine(angle);
        int yNegativeCosine = Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn);

        // For a block anchor, the collision point biases the low nibble toward the side from
        // which the rope leaves the block: eight for a nonnegative component, seven for a
        // negative one. This one-pixel asymmetry is observable in both collision probes and
        // Samus positioning, and the adjusted endpoint remains stored after the helper.
        grapple.Anchor = new(
            CoordinateAxis.Wrap((grapple.Anchor.X & ~15) | (xSine >= 0 ? 8 : 7)),
            CoordinateAxis.Wrap((grapple.Anchor.Y & ~15) | (yNegativeCosine >= 0 ? 8 : 7)));

        Position16<RoomPixels> point = grapple.Anchor + new Offset16<RoomPixels>(
            CoordinateAxis.AsSigned(ScaleCoordinate(xSine, distance)),
            CoordinateAxis.AsSigned(ScaleCoordinate(yNegativeCosine, distance)));

        // The helper takes each block column and row within the scan cycle. Rooms are authored within it;
        // keeping the cycle matters where a point wraps around the room's coordinates.
        return new GrappleCollisionPoint(
            Pixel: point,
            Block: new(BlockScanCycle.Within(point.X >> 4), BlockScanCycle.Within(point.Y >> 4)));
    }


    /// <summary>Resolves grapple swing collision through block extensions, including damaging spike reactions.</summary>
    private static bool IsSwingCollision(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomBlocks> blockPosition)
    {
        RoomCollisionBlock initialBlock =
            level.GetCollisionBlockOrEdgeSolid(blockPosition);
        if (initialBlock.Index < 0)
            return true;

        int index = initialBlock.Index;
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            RoomCollisionBlock block =
                level.GetCollisionBlockByIndexOrEdgeSolid(index);
            switch (block.CollisionType)
            {
                // The swing dispatcher intentionally treats shootable/bombable air as air;
                // unlike a firing endpoint, body contact does not spawn their room objects.
                case RoomCollisionType.Air:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnusedAir:
                case RoomCollisionType.BombableAir:
                    return false;

                case RoomCollisionType.SpikeAir:
                    // The swing's spike reaction has a mostly-zero behaviour table: only spike-air behaviour two queues
                    // 16 damage. It never collides, but it still starts the common
                    // 60-frame invulnerability and 10-frame knockback timers. The timer
                    // check must precede the behaviour lookup so a second radial probe in this
                    // same six-point sweep cannot queue damage again.
                    ApplySwingSpikeDamage(samus, block.Behavior.Reaction, block: SwingSpikeBlock.Plain);
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
                    // The solid spike reaction always reports collision. Before setting carry it applies
                    // 60 damage for behaviour zero or 16 for behaviour one; every other entry is
                    // literally zero. A negative behaviour skips the table entirely.
                    ApplySwingSpikeDamage(samus, block.Behavior.Reaction, block: SwingSpikeBlock.Solid);
                    return true;

                case RoomCollisionType.HorizontalExtension:
                    if (block.Behavior.EndsExtension)
                        return false;
                    index += block.Behavior.ExtensionOffset;
                    continue;

                case RoomCollisionType.VerticalExtension:
                    if (block.Behavior.EndsExtension)
                        return false;
                    index += block.Behavior.ExtensionOffset * level.Size.X;
                    continue;

                default:
                    throw new InvalidDataException(
                        $"Invalid grapple swing collision type {block.CollisionType}.");
            }
        }

        throw new InvalidDataException("Grapple swing extension chain exceeded sixteen blocks.");
    }


    /// <summary>Rechecks the stored endpoint through the firing dispatcher and retains the connection when carry is set.</summary>
    internal static bool IsStillConnectedToSupportedBlock(
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // The anchor validation calls the firing block dispatcher again at the stored endpoint and tests
        // carry only. A persistent grapple block of behaviour zero or three therefore stays connected; replacing
        // it with air disconnects. Room-object-producing dynamic blocks retain their explicit stop.
        GrappleBlockReaction reaction = SamusGrappleMovement.ReactAtEndpoint(
            level,
            samus,
            grapple.Anchor);
        return reaction.Collides();
    }


    /// <summary>Applies behaviour-specific spike damage once per invincibility window and starts knockback.</summary>
    private static void ApplySwingSpikeDamage(
        SamusState samus,
        BlockReaction reaction,
        SwingSpikeBlock block)
    {
        // Both handlers return immediately while the invincibility timer is nonzero or for an area reaction. This shared
        // guard is observable because one sweep can sample the same damaging block up to six times, yet only its first
        // sample may publish damage.
        if (!samus.InvincibilityTimer.IsZero || reaction.IsAreaSpecific)
            return;

        int damage = block == SwingSpikeBlock.Solid
            ? reaction == SpikeHazardDefinitions.HeavySpikeReaction ? 60
                : reaction == SpikeHazardDefinitions.LightSpikeReaction ? 16
                : 0
            : reaction == SpikeHazardDefinitions.DamagingSpikeAirReaction
                ? 16
                : 0;
        if (damage == 0)
            return;

        samus.LiquidPhysics.AccumulatePeriodicDamage(
            subDamage: 0,
            wholeDamage: damage);
        samus.InvincibilityTimer = WrappingCountdown.Start(60);
        samus.KnockbackTimer = 10;
    }


    /// <summary>Negates an arithmetic-shifted half, preserving the game's rounding for odd negative values.</summary>
    private static int NegatedArithmeticHalf(int value) =>
        -(value >> 1);
}
