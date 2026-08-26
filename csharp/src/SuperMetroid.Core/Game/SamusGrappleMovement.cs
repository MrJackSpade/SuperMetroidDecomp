using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible translation of the connected, unobstructed grapple pendulum and its
/// release seam from banks $9B and $94.
/// </summary>
/// <remarks>
/// The retail game does not run grapple physics through movement type $16's tiny handler at
/// $90:A780. <c>GrappleBeamHandler</c> in bank $9B updates the pendulum before normal beta
/// movement, and bank $94 advances its angle while probing room collision. This class keeps
/// those responsibilities separate from ordinary aerial movement. The currently admitted
/// route is a connection whose swept body does not touch terrain; collision reflection,
/// grapple-block acquisition, wall grab, and the wall-jump grace timer remain explicit later
/// routes rather than being approximated here.
/// </remarks>
public static class SamusGrappleMovement
{
    // These five values are the literal words at $9B:C118-$9B:C120. Names describe how
    // $9B:BB64-$9B:BD44 use them; the original storage itself is anonymous.
    private const short GravityMagnitude = 24;
    private const short DirectionInputMagnitude = 12;
    private const short VelocityCorrectionMagnitude = 5;
    private const short MaximumAngularVelocity = 0x480;
    private const short JumpImpulseMagnitude = 0x300;

    // The C decompilation materializes a convenient 320-word array beside $94:A957, but
    // the 65816 instructions at $94:A95F/$94:A9A3 actually issue long reads into bank $A0.
    // Index zero here is the negative-cosine quadrant at $A0:B3C3; index 64 is the label
    // SineCosineTables_8bitSine_SignExtended at $A0:B443. The 64-word prefix is what makes
    // every caller's `angle + 64` cosine lookup legal without wrapping the pointer.
    private const int SignedSineTable = 0xa0b3c3;
    private const int SwingFrameByAngle = 0x9bc1c2;
    private const int LeftPoseOffsetsByFrame = 0x9bc2c2;
    private const int RightPoseOffsetsByFrame = 0x9bc302;
    private const int GrapplePointTilePointers = 0x9bc342;
    private const int GrappleSegmentTilePointers = 0x9bc346;

    /// <summary>
    /// Installs an already-connected swinging state, equivalent to the airborne branch of
    /// <c>HandleConnectingGrapple</c> at $9B:B97C followed by $9B:BA61.
    /// </summary>
    /// <remarks>
    /// Anchor acquisition is intentionally outside this method: the caller must supply the
    /// exact grapple point selected by the beam/block collision pass. This is the same kind
    /// of narrow published-state seam used by the translated knockback and bomb-overlap code.
    /// </remarks>
    public static void ConnectUnobstructedSwing(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort anchorX,
        ushort anchorY,
        byte ropeLength,
        ushort angle,
        short angularVelocity,
        bool faceRight)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (ropeLength is < 8 or > 63)
            throw new ArgumentOutOfRangeException(nameof(ropeLength), "Retail connected rope length is 8..63 pixels.");
        if (angularVelocity is < -MaximumAngularVelocity or > MaximumAngularVelocity)
            throw new ArgumentOutOfRangeException(nameof(angularVelocity));
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
            throw new InvalidOperationException("A grapple state is already active.");

        SamusGrappleState grapple = samus.Grapple;
        grapple.Phase = GrapplePhase.ConnectedSwinging;
        grapple.AnchorX = anchorX;
        grapple.AnchorY = anchorY;
        grapple.RopeLength = ropeLength;
        grapple.RopeLengthDelta = 0;
        grapple.Angle = angle;
        grapple.MirroredAngle = angle;
        grapple.AngularVelocity = angularVelocity;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.Submerged = false;
        grapple.PointAnimationTimer = 5;
        grapple.PointAnimationFrame = 0;
        // GrappleFunc_AF87 at $94:AF87 seeds sixteen independent segment instruction
        // slots. Reading its deliberately overlapping WRAM arrays is confusing in C: the
        // resulting visible pattern is unambiguous, though. Slot 15 starts on tile $24,
        // slot 14 on $23, slot 13 on $22, slot 12 on $21, and that four-phase pattern
        // repeats down to slot zero. Every instruction timer begins at one so $94:AFBA
        // consumes the initial record on the first draw.
        for (int slot = 0; slot < grapple.SegmentAnimationFrames.Length; slot++)
        {
            grapple.SegmentAnimationTimers[slot] = 1;
            grapple.SegmentAnimationFrames[slot] = unchecked((byte)(slot & 3));
            grapple.SegmentAnimationStarted[slot] = false;
        }

        samus.Pose = faceRight
            ? SamusState.GrappleSwingRightPose
            : SamusState.GrappleSwingLeftPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        // $94:AC11 and $9B:BD95 run as soon as the connection is accepted. Publishing the
        // first pendulum position now prevents one frame of stale pre-grapple coordinates.
        PositionSamusFromPendulum(bus, samus, grapple);
    }

    /// <summary>
    /// Executes one connected grapple-function call. X is the default shoot binding used by
    /// this runtime; left/right pump the swing, newly pressed up/down adjust rope length.
    /// </summary>
    public static GrappleMovementResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort newlyPressedInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        if (grapple.Phase == GrapplePhase.ReleaseFromSwing)
        {
            // $9B:CB8B executes on the frame after $9B:C79D queued it. The launch velocity
            // was already published, so this pass changes art/handlers and clears grapple.
            CompleteQueuedRelease(bus, samus, grapple);
            return new GrappleMovementResult(GrapplePhase.Inactive, Released: true, ReleaseQueued: false);
        }

        if (grapple.Phase != GrapplePhase.ConnectedSwinging)
            throw new InvalidOperationException("Connected grapple movement is not active.");

        // $9B:C79D first checks Shoot. Releasing it does not immediately install jumping
        // art: it computes velocity and queues the release function for the following frame.
        if ((controllerInput & (ushort)SnesButton.X) == 0)
        {
            PropelSamusFromSwing(bus, samus, grapple);
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(grapple.Phase, Released: false, ReleaseQueued: true);
        }

        ApplyRopeAndDirectionInput(grapple, controllerInput, newlyPressedInput);
        ApplyRopeLengthDelta(grapple);
        CalculateGravity(grapple);
        IntegrateAngularVelocity(grapple);
        ApplyJumpImpulse(grapple, newlyPressedInput);
        AdvanceUnobstructedAngle(grapple);
        PositionSamusFromPendulum(bus, samus, grapple);

        return new GrappleMovementResult(grapple.Phase, Released: false, ReleaseQueued: false);
    }

    /// <summary>
    /// Ports the tile-upload and small-OBJ portion of $90:EB86/$9B:BFA5/$94:AFBA for a
    /// connected rope. Call after drawing Samus, matching the native grapple draw handler.
    /// </summary>
    public static void DrawConnectedBeam(
        ISnesAddressSpace bus,
        SamusGrappleState grapple,
        OamBuffer oam,
        VramWriteQueue vramWrites,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(grapple);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(vramWrites);
        if (grapple.Phase == GrapplePhase.Inactive || grapple.RopeLength == 0)
            return;

        // $9B:BFBD alternates the 32-byte grapple-point tile every six calls (timer five
        // decrements through zero and wraps). The two pointers live in bank $9B but name
        // graphics in bank $9A, exactly as the seven-byte VRAM queue record does.
        if (grapple.PointAnimationTimer == 0)
        {
            grapple.PointAnimationTimer = 5;
            grapple.PointAnimationFrame ^= 1;
        }
        else
        {
            grapple.PointAnimationTimer--;
        }
        ushort pointPointer = ReadWord(bus, GrapplePointTilePointers + grapple.PointAnimationFrame * 2);
        vramWrites.Enqueue(0x20, 0x9a0000 | pointPointer, 0x6200);

        // The 128-byte rope body source is selected by the same folded angle byte used in
        // $9B:BFFD. Horizontal, diagonal, and vertical source blocks therefore remain ROM
        // policy rather than a host renderer choosing a plausible rotated sprite.
        int foldedAngleOffset = ((grapple.Angle >> 9) & 0x7f) * 2;
        ushort segmentPointer = ReadWord(bus, GrappleSegmentTilePointers + foldedAngleOffset);
        vramWrites.Enqueue(0x80, 0x9a0000 | segmentPointer, 0x6210);

        byte angleByteTowardAnchor = unchecked((byte)((grapple.Angle >> 8) + 0x80));
        int stepX = ScaleCoordinate(ReadSignedSine(bus, angleByteTowardAnchor + 64), 8);
        int stepY = ScaleCoordinate(ReadSignedSine(bus, angleByteTowardAnchor), 8);

        // $94:AFDE derives X/Y flip bits from the grapple angle while retaining the packed
        // palette-five/priority-three instruction word. Tile $20 is the connected endpoint.
        int angleHigh = grapple.Angle >> 8;
        int flipBits = (angleHigh & 0x80) >> 1;
        flipBits |= 2 * ((((angleHigh ^ flipBits) & 0x40) ^ 0x40));
        ushort flipAttributes = unchecked((ushort)(flipBits << 8));
        int screenX = unchecked((short)(grapple.BeamStartX - layer1X)) - 4;
        int screenY = unchecked((short)(grapple.BeamStartY - layer1Y)) - 4;
        int segmentCount = (grapple.RopeLength / 8) & 0x0f;
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // $94:AFBA walks instruction slots from 15 downward regardless of rope length.
            // The first timer expiry reads the already-selected initial record; later
            // expiries advance through $21,$22,$23,$24 and $94:B0F4's goto back to $21.
            // Keeping the one-time first expiry explicit preserves the native six-draw
            // interval without pretending that all rope pieces share one animation frame.
            int instructionSlot = 15 - segment;
            if (grapple.SegmentAnimationTimers[instructionSlot]-- == 1)
            {
                grapple.SegmentAnimationTimers[instructionSlot] = 5;
                if (grapple.SegmentAnimationStarted[instructionSlot])
                {
                    grapple.SegmentAnimationFrames[instructionSlot] = unchecked((byte)(
                        (grapple.SegmentAnimationFrames[instructionSlot] + 1) & 3));
                }
                else
                {
                    grapple.SegmentAnimationStarted[instructionSlot] = true;
                }
            }

            ushort segmentAttributes = unchecked((ushort)(
                0x3a21 + grapple.SegmentAnimationFrames[instructionSlot] | flipAttributes));
            oam.AddRawSmallSprite(
                unchecked((ushort)screenX),
                unchecked((ushort)screenY),
                segmentAttributes);
            screenX += stepX;
            screenY += stepY;
        }

        oam.AddRawSmallSprite(
            unchecked((ushort)(grapple.AnchorX - layer1X - 4)),
            unchecked((ushort)(grapple.AnchorY - layer1Y - 4)),
            0x3a20);
    }

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
        bool inPumpArc = grapple.Angle >= 0x4000 && grapple.Angle < 0xc000;
        if (!inPumpArc)
        {
            grapple.DirectionInputAcceleration = 0;
            return;
        }

        if ((controllerInput & (ushort)SnesButton.Left) != 0)
        {
            if (grapple.Angle == 0x8000 && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = 0x100;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)(DirectionInputMagnitude / 2)
                : DirectionInputMagnitude;
            return;
        }

        if ((controllerInput & (ushort)SnesButton.Right) != 0)
        {
            if (grapple.Angle == 0x8000 && grapple.AngularVelocity == 0)
                grapple.AngularVelocity = -0x100;
            grapple.DirectionInputAcceleration = grapple.Submerged
                ? (short)-(DirectionInputMagnitude / 2)
                : (short)-DirectionInputMagnitude;
            return;
        }

        grapple.DirectionInputAcceleration = 0;
    }

    private static void ApplyRopeLengthDelta(SamusGrappleState grapple)
    {
        if (grapple.RopeLengthDelta == 0)
            return;

        int candidate = grapple.RopeLength + grapple.RopeLengthDelta;
        if (candidate < 8)
        {
            grapple.RopeLength = 8;
            grapple.RopeLengthDelta = 0;
        }
        else if (candidate >= 63)
        {
            grapple.RopeLength = 63;
            grapple.RopeLengthDelta = 0;
        }
        else
        {
            grapple.RopeLength = unchecked((byte)candidate);
        }
    }

    private static void CalculateGravity(SamusGrappleState grapple)
    {
        // $9B:BC1F is deliberately quadrant-based rather than trigonometric. The four
        // magnitudes are 1/4, full, full, and 1/4 gravity as angle crosses $4000 boundaries.
        int angle = grapple.Angle;
        if ((angle & 0xc000) == 0xc000)
        {
            grapple.VelocityCorrection = (short)-(VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)-(GravityMagnitude >> 3)
                : (short)-(GravityMagnitude >> 2);
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
                grapple.VelocityCorrection = -VelocityCorrectionMagnitude;
                grapple.GravityAcceleration = grapple.Submerged
                    ? (short)-(GravityMagnitude >> 1)
                    : (short)-GravityMagnitude;
            }
        }
        else if ((angle & 0x4000) != 0)
        {
            grapple.VelocityCorrection = VelocityCorrectionMagnitude;
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(GravityMagnitude >> 1)
                : GravityMagnitude;
        }
        else
        {
            grapple.VelocityCorrection = (short)(VelocityCorrectionMagnitude >> 2);
            grapple.GravityAcceleration = grapple.Submerged
                ? (short)(GravityMagnitude >> 3)
                : (short)(GravityMagnitude >> 2);
        }
    }

    private static void IntegrateAngularVelocity(SamusGrappleState grapple)
    {
        int velocity = grapple.AngularVelocity +
            grapple.DirectionInputAcceleration + grapple.GravityAcceleration;

        // $9B:BCFF adds the small correction when velocity and angle have opposite signs.
        // This looks unusual in decimal, but preserving the 16-bit sign-bit comparison is
        // essential around the $0000/$FFFF angle seam.
        if (((unchecked((ushort)velocity) ^ grapple.Angle) & 0x8000) != 0)
            velocity += grapple.VelocityCorrection;

        grapple.AngularVelocity = unchecked((short)Math.Clamp(
            velocity,
            -MaximumAngularVelocity,
            MaximumAngularVelocity));
    }

    private static void ApplyJumpImpulse(SamusGrappleState grapple, ushort newlyPressedInput)
    {
        // $9B:BD44 only admits this impulse during the 16-frame collision-reflection timer.
        // The unobstructed route normally leaves the timer at zero, but retaining the exact
        // gate and decay makes the state ready for the collision slice without changing ABI.
        if (grapple.CollisionBounceTimer != 0 &&
            (newlyPressedInput & (ushort)SnesButton.B) != 0)
        {
            grapple.JumpImpulse = grapple.AngularVelocity switch
            {
                > 0 => grapple.Submerged ? (short)(JumpImpulseMagnitude / 2) : JumpImpulseMagnitude,
                < 0 => grapple.Submerged ? (short)-(JumpImpulseMagnitude / 2) : (short)-JumpImpulseMagnitude,
                _ => 0,
            };
        }
    }

    private static void AdvanceUnobstructedAngle(SamusGrappleState grapple)
    {
        int combined = grapple.AngularVelocity + grapple.JumpImpulse;
        int factor = grapple.Submerged ? 160 : 256;
        int magnitude = (Math.Abs(combined) * factor) >> 8;
        if (magnitude == 0)
            return;

        int delta = combined < 0 ? -magnitude : magnitude;
        grapple.Angle = unchecked((ushort)(grapple.Angle + delta));
        grapple.MirroredAngle = grapple.Angle;

        if (grapple.CollisionBounceTimer != 0)
            grapple.CollisionBounceTimer--;

        // $94:ACFE damps the temporary jump impulse toward zero by six after a successful
        // angle step. This word is separate from angular velocity and must not be folded in.
        if (grapple.JumpImpulse > 0)
            grapple.JumpImpulse = (short)Math.Max(0, grapple.JumpImpulse - 6);
        else if (grapple.JumpImpulse < 0)
            grapple.JumpImpulse = (short)Math.Min(0, grapple.JumpImpulse + 6);
    }

    private static void PositionSamusFromPendulum(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        int angleIndex = grapple.Angle >> 8;
        short sineY = ReadSignedSine(bus, angleIndex);
        short sineX = ReadSignedSine(bus, angleIndex + 64);

        // $94:A957 scales the table's +/-256 sine values by rope length, then $94:AC11
        // publishes the beam-start point. Negative products are truncated by magnitude in
        // the 65816 routine, so ScaleCoordinate does not use C#'s negative arithmetic shift.
        int beamStartX = unchecked((ushort)(grapple.AnchorX + ScaleCoordinate(sineX, grapple.RopeLength)));
        int beamStartY = unchecked((ushort)(grapple.AnchorY + ScaleCoordinate(sineY, grapple.RopeLength)));
        grapple.BeamStartX = unchecked((ushort)beamStartX);
        grapple.BeamStartY = unchecked((ushort)beamStartY);

        // $9B:BD95 maps all 256 angle bytes onto the authentic swing-art frame, then adds
        // a frame-specific origin correction so Samus's hand remains attached to the beam.
        byte artFrame = bus.ReadByte(SwingFrameByAngle + (grapple.MirroredAngle >> 8));
        int offsetAddress = SamusState.ReadPoseXDirection(bus, samus.Pose) == 4
            ? LeftPoseOffsetsByFrame
            : RightPoseOffsetsByFrame;
        int pairAddress = offsetAddress + artFrame * 2;
        sbyte xOffset = unchecked((sbyte)bus.ReadByte(pairAddress));
        sbyte yOffset = unchecked((sbyte)bus.ReadByte(pairAddress + 1));

        samus.SetGrappleSwingAnimationFrame(artFrame);
        samus.XPosition = unchecked((ushort)(beamStartX + xOffset));
        samus.YPosition = unchecked((ushort)(beamStartY + yOffset));
    }

    private static void PropelSamusFromSwing(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        int absoluteVelocity = Math.Abs((int)grapple.AngularVelocity);
        int doubledVelocity = absoluteVelocity * 2;
        short cosine = ReadSignedSine(bus, (grapple.Angle >> 8) + 64);

        uint verticalFixed = unchecked((uint)(Math.Abs(cosine) * doubledVelocity));
        samus.Kinematics.YSpeed = unchecked((ushort)(verticalFixed >> 16));
        samus.Kinematics.YSubspeed = unchecked((ushort)verticalFixed);

        // $9B:CA65 selects Y direction from both angular-velocity sign and cosine sign.
        // Keeping that branch literal avoids deriving a screen-coordinate convention.
        bool movingDown = grapple.AngularVelocity >= 0 ? cosine >= 0 : cosine < 0;
        samus.Kinematics.YDirection = movingDown ? (ushort)2 : (ushort)1;

        samus.HorizontalSpeed.AccelerationMode = 2;
        int phaseOffset = 64 - 3 * (doubledVelocity >> 9);
        byte horizontalAngle = unchecked((byte)((grapple.Angle >> 8) - phaseOffset));
        int horizontalSine = Math.Abs(ReadSignedSine(bus, horizontalAngle + 64));
        uint horizontalFixed = unchecked((uint)(horizontalSine * doubledVelocity));
        samus.HorizontalSpeed.BaseSpeed = unchecked((ushort)(horizontalFixed >> 16));
        samus.HorizontalSpeed.BaseSubspeed = unchecked((ushort)horizontalFixed);
    }

    private static void CompleteQueuedRelease(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $9B:CB8B bases facing on the angular-velocity word retained from the swing. A
        // nonnegative value selects left-facing $52; a negative value selects right $51.
        samus.Pose = grapple.AngularVelocity >= 0
            ? SamusState.NormalJumpForwardLeftPose
            : SamusState.NormalJumpForwardRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        grapple.Phase = GrapplePhase.Inactive;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.RopeLengthDelta = 0;
        grapple.CollisionBounceTimer = 0;
    }

    private static int ScaleCoordinate(short sine, byte length) => sine switch
    {
        -256 => -length,
        256 => length,
        < 0 => -((-sine * length) >> 8),
        _ => (sine * length) >> 8,
    };

    private static short ReadSignedSine(ISnesAddressSpace bus, int index)
    {
        int address = SignedSineTable + index * 2;
        return unchecked((short)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Explicit function-pointer phase for the currently translated grapple route.</summary>
public enum GrapplePhase
{
    Inactive,
    ConnectedSwinging,
    ReleaseFromSwing,
}

/// <summary>
/// Named equivalents of the bank-$9B grapple WRAM words used by connected swinging.
/// </summary>
public sealed class SamusGrappleState
{
    public GrapplePhase Phase { get; set; }
    public ushort AnchorX { get; set; }
    public ushort AnchorY { get; set; }
    public ushort BeamStartX { get; set; }
    public ushort BeamStartY { get; set; }
    public byte RopeLength { get; set; }
    public short RopeLengthDelta { get; set; }
    public ushort Angle { get; set; }
    public ushort MirroredAngle { get; set; }
    public short AngularVelocity { get; set; }
    public short DirectionInputAcceleration { get; set; }
    public short GravityAcceleration { get; set; }
    public short VelocityCorrection { get; set; }
    public short JumpImpulse { get; set; }
    public ushort CollisionBounceTimer { get; set; }
    public bool Submerged { get; set; }
    public ushort PointAnimationTimer { get; set; }
    public byte PointAnimationFrame { get; set; }
    /// <summary>
    /// Sixteen timers at WRAM $7E:0D42. A connected rope draws slots 15 downward; retaining
    /// all sixteen makes shortening and lengthening the rope resume the original slot state.
    /// </summary>
    public ushort[] SegmentAnimationTimers { get; } = new ushort[16];

    /// <summary>
    /// Readable $21-$24 instruction phases corresponding to WRAM pointers $7E:0D62-$0D80.
    /// Values zero through three are added to base tile $21 only at the OAM write boundary.
    /// </summary>
    public byte[] SegmentAnimationFrames { get; } = new byte[16];

    /// <summary>
    /// Host-readable marker for $94:AFBA's special first timer expiry. The original stores
    /// the distinction implicitly in each instruction pointer; this flag keeps the C# state
    /// honest without exposing raw bank-$94 pointers as mutable gameplay data.
    /// </summary>
    public bool[] SegmentAnimationStarted { get; } = new bool[16];
}

/// <summary>One observable bank-$9B grapple-function result.</summary>
public readonly record struct GrappleMovementResult(
    GrapplePhase Phase,
    bool Released,
    bool ReleaseQueued);
