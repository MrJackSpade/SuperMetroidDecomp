using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Rope input, gravity, angular integration, terrain collision, anchor validation, and spike damage.
/// </summary>
public static partial class SamusGrappleMovement
{
    private static void ApplyRopeAndDirectionInput(
        SamusGrappleState grapple,
        ushort controllerInput,
        ushort newlyPressedInput)
    {
        // $9B:BB64 changes length only on a new directional edge. Once selected, the signed
        // delta remains live until a bound or future collision code clears it.
        if ((newlyPressedInput & (ushort)SnesButton.Up) != 0)
        {
            if (grapple.RopeLength != 0)
                grapple.RopeLengthDelta = -2;
        }
        else if ((newlyPressedInput & (ushort)SnesButton.Down) != 0)
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

        if ((controllerInput & (ushort)SnesButton.Left) != 0)
        {
            if (grapple.Angle == SnesAngle.HalfTurn && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = 0x100;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)(SamusGrappleRomData.Physics.DirectionInputMagnitude / 2)
                : SamusGrappleRomData.Physics.DirectionInputMagnitude;
            return;
        }

        if ((controllerInput & (ushort)SnesButton.Right) != 0)
        {
            if (grapple.Angle == SnesAngle.HalfTurn && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = -0x100;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)-(SamusGrappleRomData.Physics.DirectionInputMagnitude / 2)
                : (short)-SamusGrappleRomData.Physics.DirectionInputMagnitude;
            return;
        }

        grapple.DirectionInputAcceleration = 0;
    }

    private static bool ApplyRopeLengthDelta(
        ISnesAddressSpace bus,
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
                bus,
                grapple,
                grapple.Angle.TableIndex,
                probeDistance);
            if (IsSwingCollision(level, samus, point.BlockX, point.BlockY))
            {
                // Native stores GrappleCollision_NewBeamLength, which is the last accepted
                // length rather than the colliding candidate. It deliberately leaves the
                // signed delta alive so a held adjustment retries on the following frame.
                grapple.RopeLength = unchecked((ushort)currentLength);
                return true;
            }

            currentLength = candidateLength;
        }

        grapple.RopeLength = unchecked((ushort)targetLength);
        return false;
    }

    private static void CalculateGravity(SamusGrappleState grapple)
    {
        // $9B:BC1F is deliberately quadrant-based rather than trigonometric. The four
        // magnitudes are 1/4, full, full, and 1/4 gravity as angle crosses $4000 boundaries.
        int angle = grapple.Angle.RawValue;
        if ((angle & 0xc000) == 0xc000)
        {
            grapple.VelocityCorrection = (short)-(
                SamusGrappleRomData.Physics.VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)-(SamusGrappleRomData.Physics.GravityMagnitude >> 3)
                : (short)-(SamusGrappleRomData.Physics.GravityMagnitude >> 2);
        }
        else if ((angle & 0x8000) != 0)
        {
            if (angle == 0x8000)
            {
                grapple.GravityAcceleration = 0;
                grapple.VelocityCorrection = 0;
                if (Math.Abs((int)grapple.AngularVelocity) < 0x100)
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
        else if ((angle & 0x4000) != 0)
        {
            grapple.VelocityCorrection =
                SamusGrappleRomData.Physics.VelocityCorrectionMagnitude;
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(SamusGrappleRomData.Physics.GravityMagnitude >> 1)
                : SamusGrappleRomData.Physics.GravityMagnitude;
        }
        else
        {
            grapple.VelocityCorrection = (short)(
                SamusGrappleRomData.Physics.VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(SamusGrappleRomData.Physics.GravityMagnitude >> 3)
                : (short)(SamusGrappleRomData.Physics.GravityMagnitude >> 2);
        }
    }

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

    private static void ApplyJumpImpulse(SamusGrappleState grapple, ushort newlyPressedInput)
    {
        // $9B:BD44 only admits this extra angular impulse during the 16-frame terrain-
        // reflection timer. It is separate from ordinary Samus jump velocity and decays
        // inside bank $94 only after a successful angular movement pass.
        if (grapple.CollisionBounceTimer != 0 &&
            (newlyPressedInput & (ushort)SnesButton.B) != 0)
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

    private static GrappleSwingCollisionResult AdvanceAngleWithTerrainCollision(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $94:ACFE first converts angular velocity into an angle delta, then walks from the
        // current high angle byte to the target one byte at a time. Fractional-only motion
        // within the same byte has no sweep, exactly as the target-byte equality branch.
        int combined = unchecked((short)(grapple.AngularVelocity + grapple.JumpImpulse));
        int factor = grapple.Submerged ? 160 : 256;
        int magnitude = (Math.Abs(combined) * factor) >> 8;
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
                bus,
                level,
                samus,
                grapple,
                candidateAngleByte);
            if (collision.Collided)
            {
                // $94:ADB4/$AE84 restore the last safe whole angle byte and force the
                // fractional half-byte `$80`. The body therefore stops just before the
                // colliding sample rather than snapping back to the frame's original angle.
                grapple.Angle = SnesAngle.FromRaw(
                    unchecked((ushort)((lastSafeAngleByte << 8) | 0x80)));
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
                    grapple.CollisionBounceTimer = 16;
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

        if (grapple.CollisionBounceTimer != 0)
            grapple.CollisionBounceTimer--;

        // $94:ACFE damps the temporary jump impulse toward zero by six after a successful
        // angle step. This word is separate from angular velocity and must not be folded in.
        if (grapple.JumpImpulse > 0)
            grapple.JumpImpulse = (short)Math.Max(0, grapple.JumpImpulse - 6);
        else if (grapple.JumpImpulse < 0)
            grapple.JumpImpulse = (short)Math.Min(0, grapple.JumpImpulse + 6);

        return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);
    }

    private static GrappleSwingCollisionResult ProbeSwingingBody(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        byte candidateAngleByte)
    {
        // $94:ABE6 checks six points on an eight-pixel cadence. The first point is eight
        // pixels beyond the hand; the last is 48 pixels beyond it. Together with rope length
        // this approximates Samus's body as a radial line while rotating around the anchor.
        int distance = grapple.RopeLength + 8;
        for (int distanceFromFeet = 6; distanceFromFeet >= 1; distanceFromFeet--)
        {
            GrappleCollisionPoint point = CalculateCollisionPoint(
                bus,
                grapple,
                candidateAngleByte,
                distance);
            if (IsSwingCollision(level, samus, point.BlockX, point.BlockY))
            {
                return new GrappleSwingCollisionResult(
                    Collided: true,
                    DistanceFromFeet: distanceFromFeet);
            }

            distance += 8;
        }

        return new GrappleSwingCollisionResult(Collided: false, DistanceFromFeet: 0);
    }

    private static GrappleCollisionPoint CalculateCollisionPoint(
        ISnesAddressSpace bus,
        SamusGrappleState grapple,
        byte angleByte,
        int distance)
    {
        short xSine = ReadSignedSine(bus, angleByte + 64);
        short yNegativeCosine = ReadSignedSine(bus, angleByte);

        // For a block anchor, $94:A95F-$A996 biases the low nibble toward the side from
        // which the rope leaves the block: eight for a nonnegative component, seven for a
        // negative one. This one-pixel asymmetry is observable in both collision probes and
        // Samus positioning, and the adjusted endpoint remains stored after the helper.
        grapple.AnchorX = unchecked((ushort)(
            (grapple.AnchorX & 0xfff0) | (xSine >= 0 ? 8 : 7)));
        grapple.AnchorY = unchecked((ushort)(
            (grapple.AnchorY & 0xfff0) | (yNegativeCosine >= 0 ? 8 : 7)));

        ushort x = unchecked((ushort)(grapple.AnchorX + ScaleCoordinate(xSine, distance)));
        ushort y = unchecked((ushort)(grapple.AnchorY + ScaleCoordinate(yNegativeCosine, distance)));

        // The native helper masks each shifted coordinate to one byte. Rooms are authored
        // within that domain; preserving the mask matters at 16-bit world-coordinate wrap.
        return new GrappleCollisionPoint(
            X: x,
            Y: y,
            BlockX: (x >> 4) & 0xff,
            BlockY: (y >> 4) & 0xff);
    }

    private static bool IsSwingCollision(
        RoomLevelData level,
        SamusState samus,
        int blockX,
        int blockY)
    {
        RoomCollisionBlock initialBlock =
            level.GetCollisionBlockOrPrefilledSolid(blockX, blockY);
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
                    ApplySwingSpikeDamage(samus, block.Behavior, solidSpike: false);
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
                    ApplySwingSpikeDamage(samus, block.Behavior, solidSpike: true);
                    return true;

                case RoomCollisionType.HorizontalExtension:
                    if (block.Bts == RoomBlockBehaviorValues.None)
                        return false;
                    index += block.Bts.ExtensionOffset;
                    continue;

                case RoomCollisionType.VerticalExtension:
                    if (block.Bts == RoomBlockBehaviorValues.None)
                        return false;
                    index += block.Bts.ExtensionOffset * level.WidthInBlocks;
                    continue;

                default:
                    throw new InvalidDataException(
                        $"Invalid grapple swing collision type ${block.CollisionType:X1}.");
            }
        }

        throw new InvalidDataException("Grapple swing extension chain exceeded sixteen blocks.");
    }

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
            grapple.AnchorX,
            grapple.AnchorY);
        return reaction.Carry;
    }

    private static void ApplySwingSpikeDamage(
        SamusState samus,
        byte behavior,
        bool solidSpike)
    {
        // Both native handlers return immediately while `$18A8` is nonzero or when BTS is
        // negative. This shared guard is observable because one sweep can sample the same
        // damaging block up to six times, yet only its first sample may publish damage.
        if (samus.InvincibilityTimer != 0 || (behavior & 0x80) != 0)
            return;

        ushort damage = solidSpike
            ? behavior switch
            {
                0 => (ushort)0x003c,
                1 => (ushort)0x0010,
                _ => (ushort)0,
            }
            : behavior == 2
                ? (ushort)0x0010
                : (ushort)0;
        if (damage == 0)
            return;

        samus.LiquidPhysics.AccumulatePeriodicDamage(
            subDamage: 0,
            wholeDamage: damage);
        samus.InvincibilityTimer = 0x003c;
        samus.KnockbackTimer = 0x000a;
    }

    private static short NegatedArithmeticHalf(short value) =>
        unchecked((short)-(value >> 1));

}
