using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible translation of grapple firing, persistent-block acquisition, the
/// connected unobstructed pendulum, and its release seam from banks $9B and $94.
/// </summary>
/// <remarks>
/// The retail game does not run grapple physics through movement type $16's tiny handler at
/// $90:A780. <c>GrappleBeamHandler</c> in bank $9B updates the pendulum before normal beta
/// movement, and bank $94 advances its angle while probing room collision. This class keeps
/// those responsibilities separate from ordinary aerial movement. Firing and persistent
/// grapple blocks are translated here; breakable grapple PLMs, enemy acquisition, connected
/// body collision/reflection, wall grab, and the wall-jump grace timer remain explicit later
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

    // $9B:C0DB-$C1C1 is the complete firing policy table. Values are deliberately read
    // from ROM instead of copied into host arrays: a debugger can correlate every live
    // word with the cartridge, and altered/revision ROMs retain their authored behavior.
    private const int FireXVelocityTable = 0x9bc0db;
    private const int FireYVelocityTable = 0x9bc0ef;
    private const int FireAngleTable = 0x9bc104;
    private const int NoRunOriginXTable = 0x9bc122;
    private const int NoRunOriginYTable = 0x9bc136;
    private const int NoRunBeamStartXTable = 0x9bc14a;
    private const int NoRunBeamStartYTable = 0x9bc15e;
    private const int RunOriginXTable = 0x9bc172;
    private const int RunOriginYTable = 0x9bc186;
    private const int RunBeamStartXTable = 0x9bc19a;
    private const int RunBeamStartYTable = 0x9bc1ae;

    /// <summary>
    /// Ports <c>GrappleBeamFunc_FireGoToCancel</c> at <c>$9B:C51E</c>, stopping immediately
    /// before its function-pointer return. The current pose supplies all direction/origin
    /// policy; no host aiming vector is accepted.
    /// </summary>
    public static void BeginFiring(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Inactive)
            throw new InvalidOperationException("A grapple state is already active.");

        byte direction = samus.ReadShotDirection(bus);
        if ((direction & 0xf0) != 0 || direction >= 10)
        {
            throw new NotSupportedException(
                $"Pose ${samus.Pose:X2} has no fireable grapple direction (${direction:X2}).");
        }

        int tableOffset = direction * 2;
        grapple.Phase = GrapplePhase.Firing;
        grapple.FireDirection = direction;
        grapple.ExtensionXVelocity = unchecked((short)ReadWord(bus, FireXVelocityTable + tableOffset));
        grapple.ExtensionYVelocity = unchecked((short)ReadWord(bus, FireYVelocityTable + tableOffset));
        grapple.Angle = ReadWord(bus, FireAngleTable + tableOffset);
        grapple.MirroredAngle = grapple.Angle;

        // Moonwalk shares movement type one but takes the no-run table in native code.
        // Pose $49/$4A are the two moonwalk records in the retail pose set.
        bool useRunOffsets = samus.ReadMovementType(bus) == 1 && samus.Pose is not (0x49 or 0x4a);
        int originXTable = useRunOffsets ? RunOriginXTable : NoRunOriginXTable;
        int originYTable = useRunOffsets ? RunOriginYTable : NoRunOriginYTable;
        int beamStartXTable = useRunOffsets ? RunBeamStartXTable : NoRunBeamStartXTable;
        int beamStartYTable = useRunOffsets ? RunBeamStartYTable : NoRunBeamStartYTable;
        sbyte graphicsYOffset = samus.ReadGraphicsYOffset(bus);

        grapple.OriginXOffset = unchecked((short)ReadWord(bus, originXTable + tableOffset));
        grapple.OriginYOffset = unchecked((short)(
            (short)ReadWord(bus, originYTable + tableOffset) - graphicsYOffset));
        grapple.BeamStartXOffset = unchecked((short)ReadWord(bus, beamStartXTable + tableOffset));
        grapple.BeamStartYOffset = unchecked((short)(
            (short)ReadWord(bus, beamStartYTable + tableOffset) - graphicsYOffset));

        // The endpoint-offset pair is a signed 16.16 displacement from Samus plus the
        // origin table. $9B:C51E clears all four words, not merely their whole halves.
        grapple.EndpointXOffsetFixed = 0;
        grapple.EndpointYOffsetFixed = 0;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 12;
        grapple.AngularVelocity = 0;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = 0;
        InitializeBeamAnimation(grapple);

        // The beam-start/flare point is independent of the projectile endpoint while the
        // beam is extending. Publish both immediately so the firing frame can be rendered.
        grapple.BeamStartX = unchecked((ushort)(samus.XPosition + grapple.BeamStartXOffset));
        grapple.BeamStartY = unchecked((ushort)(samus.YPosition + grapple.BeamStartYOffset));
        grapple.AnchorX = unchecked((ushort)(samus.XPosition + grapple.OriginXOffset));
        grapple.AnchorY = unchecked((ushort)(samus.YPosition + grapple.OriginYOffset));
    }

    /// <summary>
    /// Ports the block-only portion of <c>GrappleBeamFunc_Firing</c> at <c>$9B:C703</c>
    /// and <c>BlockCollGrappleBeam</c> at <c>$94:A85B</c>. Enemy grapple collision remains
    /// an explicit producer seam because the runtime has not introduced live enemies.
    /// </summary>
    public static GrappleMovementResult StepFiring(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Firing)
            throw new InvalidOperationException("Grapple firing is not active.");

        // $9B:C703 treats release of Shoot as cancellation before length or collision work.
        if ((controllerInput & (ushort)SnesButton.X) == 0)
            return QueueFiringCancellation(grapple);

        grapple.RopeLength = unchecked((ushort)(grapple.RopeLength + 12));
        if (grapple.RopeLength >= 128)
            return QueueFiringCancellation(grapple);

        // $94:A85B shifts each table velocity left six and performs four identical probes.
        // Since the table is velocity*100h, the four additions together move exactly one
        // authored per-frame velocity while retaining every intermediate collision point.
        int xSubstep = grapple.ExtensionXVelocity << 6;
        int ySubstep = grapple.ExtensionYVelocity << 6;
        for (int substep = 0; substep < 4; substep++)
        {
            grapple.EndpointXOffsetFixed = unchecked(grapple.EndpointXOffsetFixed + xSubstep);
            grapple.EndpointYOffsetFixed = unchecked(grapple.EndpointYOffsetFixed + ySubstep);
            PublishFiringGeometry(samus, grapple);

            GrappleBlockReaction reaction = ReactAtEndpoint(level, grapple.AnchorX, grapple.AnchorY);
            if (!reaction.Carry)
                continue;
            if (!reaction.Overflow)
                return QueueFiringCancellation(grapple);

            // Carry+overflow is the native connected result. $94:A8DA centers the point in
            // the accepted 16x16 block before bank $9B chooses the swing/locked pose.
            grapple.AnchorX = unchecked((ushort)((grapple.AnchorX & 0xfff0) | 8));
            grapple.AnchorY = unchecked((ushort)((grapple.AnchorY & 0xfff0) | 8));
            ConnectFiringToAirborneSwing(bus, samus, grapple);
            return new GrappleMovementResult(
                grapple.Phase,
                Released: false,
                ReleaseQueued: false,
                Fired: false,
                Connected: true,
                CancelQueued: false,
                Cancelled: false,
                OwnsMovement: true);
        }

        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            Fired: true,
            Connected: false,
            CancelQueued: false,
            Cancelled: false,
            OwnsMovement: false);
    }

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
        InitializeBeamAnimation(grapple);

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
        // $9B:C005 takes the high angle byte, shifts it right once, then clears bit zero.
        // The result is already an even byte offset into the word table. Multiplying that
        // value by two again selects unrelated data for half the angles (notably firing
        // right at $C000), producing a correctly positioned but visually blank rope.
        int foldedAngleOffset = (grapple.Angle >> 9) & 0xfe;
        ushort segmentPointer = ReadWord(bus, GrappleSegmentTilePointers + foldedAngleOffset);
        vramWrites.Enqueue(0x80, 0x9a0000 | segmentPointer, 0x6210);

        // $94:AFCF recalculates the draw vector from endpoint-minus-flare geometry. This
        // is observably different from merely reversing EndAngle while firing, and it also
        // keeps the rope visually attached after pose-specific art-origin correction.
        int beamDeltaX = unchecked((short)(grapple.AnchorX - grapple.BeamStartX));
        int beamDeltaY = unchecked((short)(grapple.AnchorY - grapple.BeamStartY));
        byte drawAngle = CalculateAngleFromXY(beamDeltaX, beamDeltaY);
        int stepX = ScaleCoordinate(ReadSignedSine(bus, drawAngle + 64), 8);
        int stepY = ScaleCoordinate(ReadSignedSine(bus, drawAngle), 8);

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

    /// <summary>
    /// Completes the one-frame cancellation function installed by firing. The native
    /// routine also clears palette/sound bookkeeping not yet modeled by this runtime;
    /// every gameplay-visible firing word owned by this class is cleared here.
    /// </summary>
    public static GrappleMovementResult CompleteFiringCancellation(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.CancelPending)
            throw new InvalidOperationException("A grapple firing cancellation is not queued.");

        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.ExtensionXVelocity = 0;
        grapple.ExtensionYVelocity = 0;
        grapple.EndpointXOffsetFixed = 0;
        grapple.EndpointYOffsetFixed = 0;
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Released: false,
            ReleaseQueued: false,
            Fired: false,
            Connected: false,
            CancelQueued: false,
            Cancelled: true,
            OwnsMovement: false);
    }

    private static GrappleMovementResult QueueFiringCancellation(SamusGrappleState grapple)
    {
        grapple.Phase = GrapplePhase.CancelPending;
        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            Fired: false,
            Connected: false,
            CancelQueued: true,
            Cancelled: false,
            OwnsMovement: false);
    }

    private static void PublishFiringGeometry(SamusState samus, SamusGrappleState grapple)
    {
        // Whole endpoint offsets are the signed upper words of the two 16.16 accumulators.
        // Arithmetic shift is intentional: a left/up beam must retain sign after fractional
        // accumulation, exactly like reading `$0DCE/$0DD2` as signed displacement words.
        int endpointOffsetX = grapple.EndpointXOffsetFixed >> 16;
        int endpointOffsetY = grapple.EndpointYOffsetFixed >> 16;
        grapple.AnchorX = unchecked((ushort)(
            samus.XPosition + grapple.OriginXOffset + endpointOffsetX));
        grapple.AnchorY = unchecked((ushort)(
            samus.YPosition + grapple.OriginYOffset + endpointOffsetY));
        grapple.BeamStartX = unchecked((ushort)(samus.XPosition + grapple.BeamStartXOffset));
        grapple.BeamStartY = unchecked((ushort)(samus.YPosition + grapple.BeamStartYOffset));
    }

    private static GrappleBlockReaction ReactAtEndpoint(
        RoomLevelData level,
        ushort endpointX,
        ushort endpointY)
    {
        int blockX = endpointX >> 4;
        int blockY = endpointY >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            // Native code would index the room's contiguous decompression allocation. That
            // adjacent-WRAM representation does not exist in RoomLevelData, so wrapping to
            // an unrelated logical block would fabricate collision. Fail at the boundary.
            throw new NotSupportedException(
                $"Grapple endpoint (${endpointX:X4},${endpointY:X4}) left translated room storage.");
        }

        int index = blockY * level.WidthInBlocks + blockX;
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            // C# integer division truncates a negative dividend toward zero, so checking
            // only `index / width` would accidentally make index -1 look like row zero.
            // Validate the linear address first; the unsigned comparison covers both a
            // negative extension and one beyond the decompressed level-data allocation.
            if ((uint)index >= (uint)(level.WidthInBlocks * level.HeightInBlocks))
            {
                throw new NotSupportedException(
                    $"Grapple extension BTS resolved outside translated room storage at index ${index:X4}.");
            }

            int resolvedX = index % level.WidthInBlocks;
            int resolvedY = index / level.WidthInBlocks;

            RoomCollisionBlock block = level.GetCollisionBlock(resolvedX, resolvedY);
            switch (block.CollisionType)
            {
                // These four dispatcher entries return clear carry: the beam remains live.
                case 0:
                case 2:
                case 3:
                case 6:
                    return new GrappleBlockReaction(Carry: false, Overflow: false);

                // Slopes and these solid-family entries return carry with overflow clear,
                // which makes $9B:C703 queue cancellation rather than establish a rope.
                case 1:
                case 8:
                case 9:
                case 0x0b:
                    return new GrappleBlockReaction(Carry: true, Overflow: false);

                case 5:
                    // Horizontal extensions interpret BTS as a signed block-index delta.
                    // BTS zero is the native terminator and behaves as air.
                    if (block.Behavior == 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    index += unchecked((sbyte)block.Behavior);
                    continue;

                case 0x0d:
                    // Vertical extension uses the same signed BTS but scales by room width.
                    if (block.Behavior == 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    index += unchecked((sbyte)block.Behavior) * level.WidthInBlocks;
                    continue;

                case 0x0e:
                    // $94:A7D1 rejects bit-seven BTS. Values zero and three spawn persistent
                    // grapple PLM $D0D8, whose setup returns processor flags $41 (C=1,V=1).
                    // The persistent PLM has no level mutation, so this result is complete.
                    if ((block.Behavior & 0x80) != 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    if (block.Behavior is 0 or 3)
                        return new GrappleBlockReaction(Carry: true, Overflow: true);

                    // BTS one/two spawn breakable PLMs $D0DC/$D0E0. Returning connected
                    // without installing their instruction lifecycle would create a rope
                    // that never breaks, so deliberately stop rather than invent behavior.
                    throw new NotSupportedException(
                        $"Breakable grapple block BTS ${block.Behavior:X2} at ({resolvedX},{resolvedY}) " +
                        "requires the untranslated bank-$84 PLM lifecycle.");

                default:
                    // Types 4/7/A/C/F route through PLM setup functions whose returned
                    // processor flags and room mutation are the collision result. They are
                    // not equivalent to generic air or solid and cannot be guessed here.
                    throw new NotSupportedException(
                        $"Grapple block reaction type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} " +
                        $"at ({resolvedX},{resolvedY}) requires the untranslated bank-$84 PLM pipeline.");
            }
        }

        throw new InvalidDataException("Grapple extension chain exceeded sixteen blocks.");
    }

    private static void ConnectFiringToAirborneSwing(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // This slice admits the moving-vertically table at $9B:C3EE. A stationary body can
        // select locked-in-place poses for directions 2..7; treating it as a pendulum would
        // be visibly false, so that route remains explicit until its handler is translated.
        if (samus.Kinematics.YSpeed == 0 && samus.Kinematics.YSubspeed == 0)
        {
            throw new NotSupportedException(
                "Stationary grapple connection requires the locked-in-place bank-$9B handler.");
        }

        // Directions zero through four select clockwise pose $B2; five through nine select
        // anticlockwise pose $B3. $9B:BA61 then derives the rope angle from the actual body
        // and accepted block center—never from the original firing direction.
        bool faceRight = grapple.FireDirection < 5;
        samus.Pose = faceRight
            ? SamusState.GrappleSwingRightPose
            : SamusState.GrappleSwingLeftPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        int deltaX = unchecked((short)(samus.XPosition - grapple.AnchorX));
        int deltaY = unchecked((short)(samus.YPosition - grapple.AnchorY));
        byte angleByte = CalculateAngleFromXY(deltaX, deltaY);
        grapple.Angle = unchecked((ushort)(angleByte << 8));
        grapple.MirroredAngle = grapple.Angle;
        grapple.AngularVelocity = 0;
        grapple.RopeLengthDelta = 0;
        if (grapple.RopeLength >= 64)
            grapple.RopeLength = unchecked((ushort)(grapple.RopeLength - 24));
        grapple.Phase = GrapplePhase.ConnectedSwinging;
        PositionSamusFromPendulum(bus, samus, grapple);
    }

    /// <summary>
    /// Integer octant translation of <c>CalculateAngleFromXY</c> at <c>$A0:C0B1</c>.
    /// The return byte is measured clockwise from negative Y: $00 up, $40 right, $80 down,
    /// and $C0 left. Division truncates exactly as the SNES unsigned divide registers do.
    /// </summary>
    internal static byte CalculateAngleFromXY(int x, int y)
    {
        bool xNegative = x < 0;
        bool yNegative = y < 0;
        int absoluteX = Math.Abs(x);
        int absoluteY = Math.Abs(y);
        if (absoluteX == 0 && absoluteY == 0)
            return 0;

        // The assembly advances its pointer byte offset by four for negative X and two
        // for negative Y, then indexes a word table. Converted to a zero-based entry index,
        // those are bit one for X and bit zero for Y—not the more tempting reverse order.
        int quadrant = (xNegative ? 2 : 0) | (yNegative ? 1 : 0);
        if (absoluteY < absoluteX)
        {
            int eighths = ((absoluteY << 8) / absoluteX) >> 3;
            return quadrant switch
            {
                0 => unchecked((byte)(eighths + 0x40)),
                1 => unchecked((byte)(0x40 - eighths)),
                2 => unchecked((byte)(0xc0 - eighths)),
                _ => unchecked((byte)(0xc0 + eighths)),
            };
        }

        int reciprocalEighths = ((absoluteX << 8) / absoluteY) >> 3;
        return quadrant switch
        {
            0 => unchecked((byte)(0x80 - reciprocalEighths)),
            1 => unchecked((byte)reciprocalEighths),
            2 => unchecked((byte)(reciprocalEighths + 0x80)),
            _ => unchecked((byte)(0x100 - reciprocalEighths)),
        };
    }

    private static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        grapple.PointAnimationTimer = 5;
        grapple.PointAnimationFrame = 0;
        // GrappleFunc_AF87 at $94:AF87 seeds sixteen independent segment instruction
        // slots. Slot 15 starts on tile $24, slot 14 on $23, slot 13 on $22, slot 12 on
        // $21, and that four-phase pattern repeats down to zero. Timer one makes $94:AFBA
        // consume each initial record on its first draw.
        for (int slot = 0; slot < grapple.SegmentAnimationFrames.Length; slot++)
        {
            grapple.SegmentAnimationTimers[slot] = 1;
            grapple.SegmentAnimationFrames[slot] = unchecked((byte)(slot & 3));
            grapple.SegmentAnimationStarted[slot] = false;
        }
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
            grapple.RopeLength = unchecked((ushort)candidate);
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

    private static int ScaleCoordinate(short sine, ushort length) => sine switch
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
    Firing,
    CancelPending,
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
    /// <summary>
    /// Unsigned native length word at `$0D08`. Connected motion clamps it below 64, while
    /// firing must represent 120 before the following frame reaches the 128-pixel cutoff.
    /// </summary>
    public ushort RopeLength { get; set; }
    public short RopeLengthDelta { get; set; }
    public byte FireDirection { get; set; }
    public short ExtensionXVelocity { get; set; }
    public short ExtensionYVelocity { get; set; }
    public short OriginXOffset { get; set; }
    public short OriginYOffset { get; set; }
    public short BeamStartXOffset { get; set; }
    public short BeamStartYOffset { get; set; }
    public int EndpointXOffsetFixed { get; set; }
    public int EndpointYOffsetFixed { get; set; }
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
    bool ReleaseQueued,
    bool Fired = false,
    bool Connected = false,
    bool CancelQueued = false,
    bool Cancelled = false,
    bool OwnsMovement = true);

/// <summary>
/// Processor-status subset returned by the bank-$94 grapple block-reaction dispatcher.
/// Carry means collision; overflow distinguishes a supported grapple connection from the
/// ordinary solid result that cancels the extending beam.
/// </summary>
internal readonly record struct GrappleBlockReaction(bool Carry, bool Overflow);
