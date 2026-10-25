using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
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

        SamusPresentation.SetGrappleSwingAnimationFrame(samus, artFrame);
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


    /// <summary>Scales a signed 8.8 trigonometric component, preserving exact positive and negative unit endpoints.</summary>
    private static int ScaleCoordinate(short sine, int length) => sine switch
    {
        -256 => -length,
        256 => length,
        < 0 => -(SnesEightEight.Multiply(-sine, length)),
        _ => SnesEightEight.Multiply(sine, length),
    };


    /// <summary>Reads the signed negative-cosine table used to project grapple angles into room coordinates.</summary>
    internal static short ReadSignedSine(int index)
    {
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }


    /// <summary>Changes rope length on Up or Down edges and pumps the swing only where directional input can accelerate it.</summary>
    internal static void ApplyRopeAndDirectionInput(
        SamusGrappleState grapple,
        ControllerButton controllerInput,
        ControllerButton newlyPressedInput)
    {
        // $9B:BB64 changes length only on a new directional edge. Once selected, the signed
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
        // angle $4000..$BFFF. At exactly $8000 a motionless pendulum gets the cartridge's
        // +/-$0100 kick before the normal +/-12 input acceleration is added.
        bool inPumpArc = grapple.Angle.RawValue >= SnesAngle.QuarterTurn.RawValue &&
            grapple.Angle.RawValue < SnesAngle.ThreeQuarterTurn.RawValue;
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
                ? (short)(GrappleBeamDefinitions.Physics.DirectionInputMagnitude / 2)
                : GrappleBeamDefinitions.Physics.DirectionInputMagnitude;
            return;
        }

        if ((controllerInput & ControllerButton.Right) != ControllerButton.None)
        {
            if (HangsStraightDown(grapple) && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = -256;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)-(GrappleBeamDefinitions.Physics.DirectionInputMagnitude / 2)
                : (short)-GrappleBeamDefinitions.Physics.DirectionInputMagnitude;
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
    internal static void CalculateGravity(SamusGrappleState grapple)
    {
        // $9B:BC1F is deliberately quadrant-based rather than trigonometric. The four
        // magnitudes are 1/4, full, full, and 1/4 gravity as angle crosses $4000 boundaries.
        int angle = grapple.Angle.RawValue;
        if ((angle & 3 << 14) == 3 << 14)
        {
            grapple.VelocityCorrection = -(
                GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)-(GrappleBeamDefinitions.Physics.GravityMagnitude >> 3)
                : (short)-(GrappleBeamDefinitions.Physics.GravityMagnitude >> 2);
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
                    -GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude;
                grapple.GravityAcceleration = grapple.Submerged
                    ? (short)-(GrappleBeamDefinitions.Physics.GravityMagnitude >> 1)
                    : (short)-GrappleBeamDefinitions.Physics.GravityMagnitude;
            }
        }
        else if ((angle & 1 << 14) != 0)
        {
            grapple.VelocityCorrection =
                GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude;
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(GrappleBeamDefinitions.Physics.GravityMagnitude >> 1)
                : GrappleBeamDefinitions.Physics.GravityMagnitude;
        }
        else
        {
            grapple.VelocityCorrection =
                GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude >> 2;
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(GrappleBeamDefinitions.Physics.GravityMagnitude >> 3)
                : (short)(GrappleBeamDefinitions.Physics.GravityMagnitude >> 2);
        }
    }


    /// <summary>Combines input, gravity, and sign-sensitive correction before clamping pendulum angular velocity.</summary>
    internal static void IntegrateAngularVelocity(SamusGrappleState grapple)
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
            -GrappleBeamDefinitions.Physics.MaximumAngularVelocity,
            GrappleBeamDefinitions.Physics.MaximumAngularVelocity));
    }


    /// <summary>Accepts a fresh Jump during the terrain-reflection window as a temporary angular kick, halved underwater.</summary>
    internal static void ApplyJumpImpulse(SamusGrappleState grapple, ControllerButton newlyPressedInput)
    {
        // $9B:BD44 only admits this extra angular impulse during the 16-frame terrain-
        // reflection timer. It is separate from ordinary Samus jump velocity and decays
        // inside bank $94 only after a successful angular movement pass.
        // Inputs here are normalized: A is Jump, B is Run. The native BIT JumpBinding
        // must agree with the release-window check, or tight anchors become inescapable
        // using Jump even though the unrelated Run button accidentally supplies the kick.
        if (!grapple.CollisionBounceTimer.IsZero &&
            (newlyPressedInput & ControllerButton.A) != ControllerButton.None)
        {
            grapple.JumpImpulse = grapple.AngularVelocity switch
            {
                > 0 => grapple.Submerged
                    ? (short)(GrappleBeamDefinitions.Physics.JumpImpulseMagnitude / 2)
                    : GrappleBeamDefinitions.Physics.JumpImpulseMagnitude,
                < 0 => grapple.Submerged
                    ? (short)-(GrappleBeamDefinitions.Physics.JumpImpulseMagnitude / 2)
                    : (short)-GrappleBeamDefinitions.Physics.JumpImpulseMagnitude,
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
                    grapple.CollisionBounceTimer = WrappingCountdown.Start(16);
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
        // $9B:B8F1 calls the firing block dispatcher again at the stored endpoint and tests
        // carry only. Persistent type-$E/BTS-$00 or $03 therefore stays connected; replacing
        // it with air disconnects. PLM-producing dynamic blocks retain their explicit stop.
        GrappleBlockReaction reaction = SamusGrappleMovement.ReactAtEndpoint(
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
        samus.InvincibilityTimer = WrappingCountdown.Start(60);
        samus.KnockbackTimer = WrappingCountdown.Start(10);
    }


    /// <summary>Negates an arithmetic-shifted half, preserving the cartridge's rounding for odd negative values.</summary>
    private static short NegatedArithmeticHalf(short value) =>
        unchecked((short)-(value >> 1));
}
