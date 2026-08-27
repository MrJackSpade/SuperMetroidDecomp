using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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

    // `$9B:C036` deliberately reuses the main charge-flare delay program and bank-$93
    // spritemap index tables. Grapple owns independent copies of the three live WRAM words,
    // however, because `$90:EB86` bypasses the ordinary projectile charge-flare handler.
    private const int MainFlareAnimationDelays = 0x90c487;
    private const int RightFlareSpritemapOffsets = 0x93a225;
    private const int LeftFlareSpritemapOffsets = 0x93a22b;

    // Eight ten-byte records at $9B:C43E drive close-collision snapping. Keeping the
    // function words named lets us validate the ROM table instead of reducing its final
    // field to a guessed host boolean.
    private const int SpecialAngleTable = 0x9bc43e;
    private const int SpecialAngleRecordSize = 10;
    private const ushort LockedInPlaceFunction = 0xc77e;
    private const ushort SwingingFunction = 0xc79d;
    private const ushort WallGrabFunction = 0xc814;
    private const int DroppedStandingPoseTable = 0x9bc9ba;
    private const int DroppedCrouchingPoseTable = 0x9bc9c4;

    // $9B:C0DB-$C1C1 is the complete firing policy table. Values are deliberately read
    // from ROM instead of copied into host arrays: a debugger can correlate every live
    // word with the cartridge, and altered/revision ROMs retain their authored behavior.
    private const int FireXVelocityTable = 0x9bc0db;
    private const int FireYVelocityTable = 0x9bc0ef;
    private const int FireAngleTable = 0x9bc104;
    private const int NoRunOriginXTable = 0x9bc122;
    private const int NoRunOriginYTable = 0x9bc136;
    private const int NoRunFlareXTable = 0x9bc14a;
    private const int NoRunFlareYTable = 0x9bc15e;
    private const int RunOriginXTable = 0x9bc172;
    private const int RunOriginYTable = 0x9bc186;
    private const int RunFlareXTable = 0x9bc19a;
    private const int RunFlareYTable = 0x9bc1ae;

    // HandleConnectingGrapple at $9B:B97C selects one of these three ten-record tables.
    // Every four-byte record is {next grapple-function word, connection-handler word}.
    // Reading both words from ROM preserves the deliberately surprising crouching entries
    // for horizontal fire, and validating the pair prevents a bad mapping from silently
    // turning a locked body into a pendulum (or vice versa).
    private const int DefaultConnectionTable = 0x9bc3c6;
    private const int MovingVerticallyConnectionTable = 0x9bc3ee;
    private const int CrouchingConnectionTable = 0x9bc416;
    private const ushort ConnectSwingClockwiseHandler = 0xb9d9;
    private const ushort ConnectSwingAnticlockwiseHandler = 0xb9e2;
    private const ushort ConnectStandingUpRightHandler = 0xb9ea;
    private const ushort ConnectStandingRightHandler = 0xb9f3;
    private const ushort ConnectStandingDownHandler = 0xb9fc;
    private const ushort ConnectStandingUpLeftHandler = 0xba05;
    private const ushort ConnectCrouchingUpRightHandler = 0xba0e;
    private const ushort ConnectCrouchingRightHandler = 0xba17;
    private const ushort ConnectCrouchingDownLeftHandler = 0xba20;
    private const ushort ConnectCrouchingUpLeftHandler = 0xba29;

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

        // `$9B:C4F0` selects run offsets solely for movement type one. Moonwalking is
        // distinct type `$10`, so every stable/aimed moonwalk pose naturally takes the
        // no-run origin and flare tables without a pose-number exception.
        bool useRunOffsets = samus.ReadMovementType(bus) == 1;
        int originXTable = useRunOffsets ? RunOriginXTable : NoRunOriginXTable;
        int originYTable = useRunOffsets ? RunOriginYTable : NoRunOriginYTable;
        int flareXTable = useRunOffsets ? RunFlareXTable : NoRunFlareXTable;
        int flareYTable = useRunOffsets ? RunFlareYTable : NoRunFlareYTable;
        sbyte graphicsYOffset = samus.ReadGraphicsYOffset(bus);

        grapple.OriginXOffset = unchecked((short)ReadWord(bus, originXTable + tableOffset));
        grapple.OriginYOffset = unchecked((short)(
            (short)ReadWord(bus, originYTable + tableOffset) - graphicsYOffset));
        grapple.FlareXOffset = unchecked((short)ReadWord(bus, flareXTable + tableOffset));
        grapple.FlareYOffset = unchecked((short)(
            (short)ReadWord(bus, flareYTable + tableOffset) - graphicsYOffset));

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
        grapple.ValidateAnchorBlock = false;
        grapple.SpecialAngleHandling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        InitializeBeamAnimation(grapple);

        // Native WRAM has two easily conflated coordinate pairs. Start is the hand/rope
        // origin used by connection positioning; Flare is the small-OBJ draw origin used
        // by $94:AFBA. They coincide while swinging but can differ while firing or locked.
        grapple.RopeStartX = unchecked((ushort)(samus.XPosition + grapple.OriginXOffset));
        grapple.RopeStartY = unchecked((ushort)(samus.YPosition + grapple.OriginYOffset));
        grapple.BeamStartX = unchecked((ushort)(samus.XPosition + grapple.FlareXOffset));
        grapple.BeamStartY = unchecked((ushort)(samus.YPosition + grapple.FlareYOffset));
        grapple.AnchorX = unchecked((ushort)(samus.XPosition + grapple.OriginXOffset));
        grapple.AnchorY = unchecked((ushort)(samus.YPosition + grapple.OriginYOffset));
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
        ISnesAddressSpace bus,
        SamusState samus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(oam);

        SamusGrappleState grapple = samus.Grapple;
        if (!UsesBeamSpecificDrawingPath(grapple.Phase) || grapple.FlareCounter == 0)
            return false;

        // While firing, `$90:EB86` calls `$9B:BF1B` immediately before the flare. Grapple
        // physics ran before ordinary Samus movement, so the renderer must reconstruct both
        // hand origins from the final body position or a running shot visibly lags one frame.
        // Connected functions already publish their exact rope/flare pair during movement.
        if (grapple.Phase == GrapplePhase.Firing)
            RefreshFiringDrawOrigins(bus, samus, grapple);

        // Counter one is a sentinel, not merely the first ordinary animation tick. Native
        // code force-selects main-flare frame 16 and timer three on every such call, then
        // performs the usual 16-bit DEC/BMI. The first visible frame therefore stores two.
        if (grapple.FlareCounter == 1)
        {
            grapple.FlareAnimationFrame = 16;
            grapple.FlareAnimationTimer = 3;
        }

        grapple.FlareAnimationTimer = unchecked((ushort)(grapple.FlareAnimationTimer - 1));
        if (unchecked((short)grapple.FlareAnimationTimer) < 0)
        {
            grapple.FlareAnimationFrame = unchecked((ushort)(grapple.FlareAnimationFrame + 1));
            byte delay = bus.ReadByte(MainFlareAnimationDelays + grapple.FlareAnimationFrame);
            if (delay == 0xfe)
            {
                // `$FE,n` is the compact loop command in the shared delay bytecode. The
                // subtraction applies to the already-incremented frame word and wraps like
                // 16-bit ADC/SBC; malformed ROM data remains visible instead of clamped.
                byte rewind = bus.ReadByte(
                    MainFlareAnimationDelays +
                    unchecked((ushort)(grapple.FlareAnimationFrame + 1)));
                grapple.FlareAnimationFrame = unchecked((ushort)(
                    grapple.FlareAnimationFrame - rewind));
                delay = bus.ReadByte(
                    MainFlareAnimationDelays + grapple.FlareAnimationFrame);
            }

            grapple.FlareAnimationTimer = delay;
        }

        ushort screenX = unchecked((ushort)(grapple.BeamStartX - layer1X));
        ushort screenY = unchecked((ushort)(grapple.BeamStartY - layer1Y));
        // The assembly tests only Y's high byte. X is allowed to wrap into high OAM, while
        // any Y outside unsigned screen rows `$00-$FF` suppresses the whole spritemap.
        if ((screenY & 0xff00) != 0)
            return false;

        int orientationTable = SamusState.ReadPoseXDirection(bus, samus.Pose) == 4
            ? LeftFlareSpritemapOffsets
            : RightFlareSpritemapOffsets;
        ushort tableIndex = unchecked((ushort)(
            grapple.FlareAnimationFrame + ReadWord(bus, orientationTable)));
        oam.AddFlareSpritemap(bus, tableIndex, screenX, screenY);
        return true;
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
        ushort controllerInput,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.Firing)
            throw new InvalidOperationException("Grapple firing is not active.");

        // The common pose-command tail clamps the camera's previous-position words after
        // command 9/10 snaps Samus to the accepted rope geometry. Retain the pre-snap body
        // position even though most firing frames return without consuming this sample.
        ushort previousXPosition = samus.XPosition;
        ushort previousYPosition = samus.YPosition;

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

            GrappleBlockReaction reaction = ReactAtEndpoint(
                level,
                samus,
                grapple.AnchorX,
                grapple.AnchorY,
                plms);
            if (!reaction.Carry)
                continue;
            if (!reaction.Overflow)
                return QueueFiringCancellation(grapple);

            // Carry+overflow is the native connected result. $94:A8DA centers the point in
            // the accepted 16x16 block before bank $9B chooses the swing/locked pose.
            grapple.AnchorX = unchecked((ushort)((grapple.AnchorX & 0xfff0) | 8));
            grapple.AnchorY = unchecked((ushort)((grapple.AnchorY & 0xfff0) | 8));
            return ConnectAcceptedFiring(
                bus,
                samus,
                grapple,
                previousXPosition,
                previousYPosition);
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
        // This public helper is the debugger's already-accepted-anchor seam. Its caller may
        // deliberately place an anchor where the current room has no grapple block (the
        // Landing Site regression does exactly that), so only a beam acquired through the
        // block dispatcher opts into bank-$9B's per-frame connection revalidation.
        grapple.ValidateAnchorBlock = false;
        grapple.SpecialAngleHandling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        InitializeBeamAnimation(grapple);

        samus.Pose = faceRight
            ? SamusState.GrappleSwingRightPose
            : SamusState.GrappleSwingLeftPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        // $94:AC11 and $9B:BD95 run as soon as the connection is accepted. Publishing the
        // first pendulum position now prevents one frame of stale pre-grapple coordinates.
        PositionSamusFromPendulum(bus, samus, grapple);

        // This helper represents a connection that has already passed `$9B:C79D`; publish
        // the handler-tail flag that retail would leave for the following swing frame.
        RefreshLiquidPhysicsFlag(samus);
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
        ArgumentNullException.ThrowIfNull(samus);
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
    public static GrappleMovementResult Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort newlyPressedInput,
        ushort nmiFrameCounter = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;

        if (grapple.Phase == GrapplePhase.ReleaseFromSwing)
        {
            // $9B:CB8B executes on the frame after $9B:C79D queued it. The launch velocity
            // was already published, so this pass changes art/handlers and clears grapple.
            CompleteQueuedRelease(bus, samus, grapple);
            return new GrappleMovementResult(GrapplePhase.Inactive, Released: true, ReleaseQueued: false);
        }

        // These are literal bank-$9B function-pointer phases. Each handler owns the whole
        // grapple beta pass, so dispatch it before entering ordinary pendulum integration.
        if (grapple.Phase == GrapplePhase.ConnectedLocked)
            return StepLocked(level, samus, controllerInput);
        if (grapple.Phase == GrapplePhase.WallGrab)
            return StepWallGrab(level, samus, controllerInput);
        if (grapple.Phase == GrapplePhase.WallGrabRelease)
            return StepWallGrabRelease(bus, level, samus, newlyPressedInput);
        if (grapple.Phase == GrapplePhase.WallJumping)
            return CompleteGrappleWallJump(bus, samus, grapple);
        if (grapple.Phase == GrapplePhase.Dropped)
            return CompleteDropped(bus, level, samus, grapple, nmiFrameCounter);

        if (grapple.Phase != GrapplePhase.ConnectedSwinging)
            throw new InvalidOperationException("Connected grapple movement is not active.");

        // `$9B:BAD5` clamps the camera's previous-position words against the position from
        // the start of this frame after a special-angle snap. Retain that exact sample now;
        // the host runtime consumes the optional corrected values returned below.
        ushort previousXPosition = samus.XPosition;
        ushort previousYPosition = samus.YPosition;

        // $9B:C79D first checks Shoot. Releasing it does not immediately install jumping
        // art: it computes velocity and queues the release function for the following frame.
        if ((controllerInput & (ushort)SnesButton.X) == 0)
        {
            if (grapple.AngularVelocity == 0 && grapple.Angle == 0x8000)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase,
                    Released: false,
                    ReleaseQueued: false,
                    DropQueued: true);
            }

            PropelSamusFromSwing(bus, samus, grapple);
            // `$9B:C7C1` installs `$90:946E` immediately. The beam function remains in
            // its one-frame release-cleanup phase, but beta movement already uses the
            // independent release handler during this same gameplay frame.
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(grapple.Phase, Released: false, ReleaseQueued: true);
        }

        ApplyRopeAndDirectionInput(grapple, controllerInput, newlyPressedInput);
        bool ropeLengthBlocked = ApplyRopeLengthDelta(bus, level, samus, grapple);
        CalculateGravity(grapple);
        IntegrateAngularVelocity(grapple);
        ApplyJumpImpulse(grapple, newlyPressedInput);
        GrappleSwingCollisionResult terrain = AdvanceAngleWithTerrainCollision(
            bus,
            level,
            samus,
            grapple);

        // `$9B:BAD5` scans all eight ROM records only when bank $94 raised the close-body
        // flag. A match replaces pendulum positioning for this frame with the table's exact
        // pose, anchor-relative body offset, and locked/wallgrab function pointer.
        if (grapple.SpecialAngleHandling &&
            TryHandleSpecialAngle(
                bus,
                samus,
                grapple,
                previousXPosition,
                previousYPosition,
                out GrappleMovementResult special))
        {
            return special with
            {
                TerrainCollided = terrain.Collided,
                CollisionDistanceFromFeet = terrain.DistanceFromFeet,
                RopeLengthBlocked = ropeLengthBlocked,
            };
        }

        if (grapple.ValidateAnchorBlock && !IsStillConnectedToSupportedBlock(level, samus, grapple))
        {
            // The persistent block path normally remains connected forever. Retain the
            // native validation seam so a future dynamic/breakable PLM cannot leave a rope
            // attached to air. A motionless straight-down body queues `$C8C5`; a moving
            // pendulum uses the already translated velocity-preserving release handoff.
            if (grapple.AngularVelocity == 0 && grapple.Angle == 0x8000)
            {
                grapple.Phase = GrapplePhase.Dropped;
                return new GrappleMovementResult(
                    grapple.Phase,
                    Released: false,
                    ReleaseQueued: false,
                    TerrainCollided: terrain.Collided,
                    CollisionDistanceFromFeet: terrain.DistanceFromFeet,
                    RopeLengthBlocked: ropeLengthBlocked,
                    AnchorDisconnected: true,
                    DropQueued: true);
            }

            PropelSamusFromSwing(bus, samus, grapple);
            grapple.ReleasedMovementActive = true;
            grapple.Phase = GrapplePhase.ReleaseFromSwing;
            return new GrappleMovementResult(
                grapple.Phase,
                Released: false,
                ReleaseQueued: true,
                TerrainCollided: terrain.Collided,
                CollisionDistanceFromFeet: terrain.DistanceFromFeet,
                RopeLengthBlocked: ropeLengthBlocked,
                AnchorDisconnected: true);
        }

        PositionSamusFromPendulum(bus, samus, grapple);

        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            TerrainCollided: terrain.Collided,
            CollisionDistanceFromFeet: terrain.DistanceFromFeet,
            RopeLengthBlocked: ropeLengthBlocked,
            AnchorDisconnected: false);
    }

    private static GrappleMovementResult StepLocked(
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput)
    {
        SamusGrappleState grapple = samus.Grapple;

        // `$9B:C77E` does no pendulum work. It merely retains the frozen pose while Shoot
        // remains held and either an enemy or the stored block still supports the endpoint.
        bool shootHeld = (controllerInput & (ushort)SnesButton.X) != 0;
        bool anchorHeld = !grapple.ValidateAnchorBlock ||
            IsStillConnectedToSupportedBlock(level, samus, grapple);
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase,
                Released: false,
                ReleaseQueued: false,
                LockedInPlace: true);
        }

        // Both release and block disconnection install `$C856`. Mark the origin because
        // firing cancellation coexists with ordinary movement, whereas a locked type-$16
        // body must own this beta pass until its pose-definition fallback is committed.
        grapple.CancelFromConnectedPose = true;
        grapple.Phase = GrapplePhase.CancelPending;
        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            CancelQueued: true,
            OwnsMovement: true,
            LockedInPlace: true,
            AnchorDisconnected: !anchorHeld);
    }

    private static GrappleMovementResult StepWallGrab(
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput)
    {
        SamusGrappleState grapple = samus.Grapple;
        bool shootHeld = (controllerInput & (ushort)SnesButton.X) != 0;
        bool anchorHeld = !grapple.ValidateAnchorBlock ||
            IsStillConnectedToSupportedBlock(level, samus, grapple);
        if (shootHeld && anchorHeld)
        {
            return new GrappleMovementResult(
                grapple.Phase,
                Released: false,
                ReleaseQueued: false,
                WallGrabEntered: true);
        }

        // `$9B:C81B` writes decimal 30 and changes only the grapple function. The contact
        // pose and beam remain visible throughout the following grace-window probes.
        grapple.WallJumpTimer = 30;
        grapple.Phase = GrapplePhase.WallGrabRelease;
        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            WallJumpWindowOpened: true,
            AnchorDisconnected: !anchorHeld);
    }

    private static GrappleMovementResult StepWallGrabRelease(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort newlyPressedInput)
    {
        SamusGrappleState grapple = samus.Grapple;

        // DEC/BPL at `$9B:C832` gives values 29..0 thirty eligible checks. The next call
        // wraps zero to `$FFFF`, queues dropped, and deliberately performs no wall probe.
        grapple.WallJumpTimer = unchecked((ushort)(grapple.WallJumpTimer - 1));
        if ((grapple.WallJumpTimer & 0x8000) != 0)
        {
            grapple.Phase = GrapplePhase.Dropped;
            return new GrappleMovementResult(
                grapple.Phase,
                Released: false,
                ReleaseQueued: false,
                DropQueued: true);
        }

        // `$90:9CAC` probes solid/frozen enemies first, then 16 pixels of room blocks toward
        // the wall named by the contact pose's direction byte. The shared probe preserves
        // square slopes, BTS dispatch, available distance, and enemy identity.
        int signedProbe = samus.ReadPoseXDirection(bus) == 4
            ? -(16 << 16)
            : 16 << 16;
        BlockMoveResult wall = SamusBlockCollision.ProbeWallHorizontal(
            bus,
            level,
            samus.Kinematics,
            signedProbe);
        bool jumpNew = (newlyPressedInput & (ushort)SnesButton.A) != 0;
        if (wall.Collided && jumpNew)
        {
            if (wall.EnemyCollision is { EnemyIndex: ushort enemyIndex })
            {
                // `$90:9D24-$90:9D26` requests the contacted enemy's shake only when the
                // fresh Jump edge actually accepts the grapple wall jump.
                samus.EnemyIndexToShake = enemyIndex;
            }
            grapple.Phase = GrapplePhase.WallJumping;
            return new GrappleMovementResult(
                grapple.Phase,
                Released: false,
                ReleaseQueued: false,
                WallJumpQueued: true,
                WallProbeCollided: true);
        }

        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            WallProbeCollided: wall.Collided);
    }

    private static GrappleMovementResult CompleteGrappleWallJump(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // `$9B:C9CE` runs one frame after `$C832` selected it, matching every other grapple
        // function-pointer handoff. The Samus helper keeps the peculiar `$B8->$84` and
        // `$B9->$83` reversal separate from ordinary spin-contact wall jumps.
        samus.ApplyGrappleWallJump(bus);
        ClearConnectedGrapple(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Released: false,
            ReleaseQueued: false,
            WallJumpStarted: true);
    }

    private static GrappleMovementResult CompleteDropped(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        ushort nmiFrameCounter)
    {
        byte targetPose = SelectDroppedPose(bus, samus);
        samus.ApplyGrappleDropTransition(bus, level, targetPose, nmiFrameCounter);
        ClearConnectedGrapple(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Released: false,
            ReleaseQueued: false,
            Dropped: true);
    }

    private static byte SelectDroppedPose(ISnesAddressSpace bus, SamusState samus)
    {
        // Swinging `$B2/$B3` bypass the direction tables and always fall to ordinary
        // standing `$01/$02`, even though their live collision radius is compact.
        if (samus.Pose == SamusState.GrappleSwingRightPose)
            return SamusState.FacingRightNormalPose;
        if (samus.Pose == SamusState.GrappleSwingLeftPose)
            return SamusState.FacingLeftNormalPose;

        byte shotDirection = samus.ReadShotDirection(bus);
        bool ordinaryDirection = (shotDirection & 0xf0) == 0 && shotDirection < 10;
        if (!ordinaryDirection)
        {
            // `$9B:C8DB/$C93C` use the pose direction when the direction byte is a command
            // sentinel. Standing selects `$01/$02`; compact bodies select `$27/$28`.
            bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
            return samus.Kinematics.YRadius >= 17
                ? facingLeft ? SamusState.FacingLeftNormalPose : SamusState.FacingRightNormalPose
                : facingLeft ? SamusState.CrouchingLeftPose : SamusState.CrouchingRightPose;
        }

        int table = samus.Kinematics.YRadius >= 17
            ? DroppedStandingPoseTable
            : DroppedCrouchingPoseTable;
        return bus.ReadByte(table + shotDirection);
    }

    private static bool TryHandleSpecialAngle(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple,
        ushort previousXPosition,
        ushort previousYPosition,
        out GrappleMovementResult result)
    {
        // Native starts at record seven (`X=$46`) and walks backward by ten. Record order is
        // semantically irrelevant for unique angles, but preserving it makes duplicate data
        // in a modified ROM resolve exactly like the cartridge loop.
        for (int record = 7; record >= 0; record--)
        {
            int address = SpecialAngleTable + record * SpecialAngleRecordSize;
            if (ReadWord(bus, address) != grapple.Angle)
                continue;

            ushort poseWord = ReadWord(bus, address + 2);
            if ((poseWord & 0xff00) != 0)
                throw new InvalidDataException($"Grapple special-angle pose word ${poseWord:X4} is not byte-sized.");

            short xOffset = unchecked((short)ReadWord(bus, address + 4));
            short yOffset = unchecked((short)ReadWord(bus, address + 6));
            ushort function = ReadWord(bus, address + 8);
            GrapplePhase phase = function switch
            {
                LockedInPlaceFunction => GrapplePhase.ConnectedLocked,
                WallGrabFunction => GrapplePhase.WallGrab,
                _ => throw new InvalidDataException(
                    $"Grapple special-angle record {record} names unknown function ${function:X4}."),
            };

            samus.Pose = unchecked((byte)poseWord);
            samus.XPosition = unchecked((ushort)(grapple.AnchorX + xOffset));
            samus.YPosition = unchecked((ushort)(grapple.AnchorY + yOffset));
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus, initialFrame: 0);
            grapple.Phase = phase;
            grapple.WallJumpTimer = 0;

            ushort cameraPreviousX = ClampPreviousPosition(samus.XPosition, previousXPosition);
            ushort cameraPreviousY = ClampPreviousPosition(samus.YPosition, previousYPosition);
            result = new GrappleMovementResult(
                phase,
                Released: false,
                ReleaseQueued: false,
                SpecialAngleHandled: true,
                LockedInPlace: phase == GrapplePhase.ConnectedLocked,
                WallGrabEntered: phase == GrapplePhase.WallGrab,
                CameraPreviousX: cameraPreviousX,
                CameraPreviousY: cameraPreviousY);
            return true;
        }

        result = default;
        return false;
    }

    private static ushort ClampPreviousPosition(ushort current, ushort previous)
    {
        short displacement = unchecked((short)(current - previous));
        if (displacement >= 13)
            return unchecked((ushort)(current - 12));
        if (displacement < -12)
            return unchecked((ushort)(current + 12));
        return previous;
    }

    /// <summary>
    /// Ports the post-Samus tile-upload and small-OBJ portion of
    /// `$90:EB86/$9B:BFA5/$94:AFBA`.
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
        if (!UsesBeamSpecificDrawingPath(grapple.Phase))
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

        // `$9B:BFA5` increments the shared flare counter after both tile records, saturating
        // at 120 by a signed comparison. It happens even at zero rope length; only the OAM
        // rope renderer below is conditional. This is why a newly fired beam can animate
        // its muzzle flare before its first eight-pixel body segment exists.
        if (unchecked((short)(grapple.FlareCounter - 120)) < 0)
            grapple.FlareCounter = unchecked((ushort)(grapple.FlareCounter + 1));
        if (grapple.RopeLength == 0)
            return;

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
    public static GrappleMovementResult CompleteFiringCancellation(
        ISnesAddressSpace bus,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.CancelPending)
            throw new InvalidOperationException("A grapple firing cancellation is not queued.");

        bool cancelledConnectedPose = grapple.CancelFromConnectedPose;
        if (cancelledConnectedPose)
        {
            // `$9B:C856` calls `$91:82D9` while the current movement type is `$16`.
            // Its command-six table entry kills all X/run momentum and the pose-definition
            // byte selects the exact standing/crouching body that existed before locking.
            byte fallback = samus.ReadNoInputFallbackPose(bus);
            if (fallback == 0xff)
            {
                throw new InvalidDataException(
                    $"Connected grapple pose ${samus.Pose:X2} has no cancellation fallback.");
            }

            samus.Pose = fallback;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus, initialFrame: 0);
            samus.HorizontalSpeed.AccelerationMode = 0;
            samus.HorizontalSpeed.BaseSpeed = 0;
            samus.HorizontalSpeed.BaseSubspeed = 0;
            samus.HorizontalSpeed.ExtraRunSpeed = 0;
            samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        }

        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.ExtensionXVelocity = 0;
        grapple.ExtensionYVelocity = 0;
        grapple.EndpointXOffsetFixed = 0;
        grapple.EndpointYOffsetFixed = 0;
        grapple.CancelFromConnectedPose = false;
        ClearFlareAnimation(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Released: false,
            ReleaseQueued: false,
            Fired: false,
            Connected: false,
            CancelQueued: false,
            Cancelled: true,
            OwnsMovement: cancelledConnectedPose,
            LockedInPlace: cancelledConnectedPose);
    }

    private static GrappleMovementResult QueueFiringCancellation(SamusGrappleState grapple)
    {
        grapple.CancelFromConnectedPose = false;
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
        grapple.RopeStartX = unchecked((ushort)(samus.XPosition + grapple.OriginXOffset));
        grapple.RopeStartY = unchecked((ushort)(samus.YPosition + grapple.OriginYOffset));
        grapple.AnchorX = unchecked((ushort)(
            grapple.RopeStartX + endpointOffsetX));
        grapple.AnchorY = unchecked((ushort)(
            grapple.RopeStartY + endpointOffsetY));
        grapple.BeamStartX = unchecked((ushort)(samus.XPosition + grapple.FlareXOffset));
        grapple.BeamStartY = unchecked((ushort)(samus.YPosition + grapple.FlareYOffset));
    }

    private static GrappleBlockReaction ReactAtEndpoint(
        RoomLevelData level,
        SamusState samus,
        ushort endpointX,
        ushort endpointY,
        RoomPlmSystem? plms = null)
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

                case 0x0a:
                    // `$94:A7FD` selects a bank-$84 grapple-reaction PLM by the low seven
                    // BTS bits. A negative BTS rejects the beam. Every ordinary entry uses
                    // `$84:CFD1` (carry set, overflow clear) and immediately deletes; BTS
                    // three alone uses Draygon's broken-turret setup `$84:CFD5`, which adds
                    // one whole periodic-damage unit and returns carry+overflow to connect.
                    if ((block.Behavior & 0x80) != 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    if (block.Behavior == 3)
                    {
                        samus.LiquidPhysics.AccumulatePeriodicDamage(
                            subDamage: 0,
                            wholeDamage: 1);
                        return new GrappleBlockReaction(Carry: true, Overflow: true);
                    }

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

                    // BTS one/two spawn $D0DC/$D0E0. Setup_CFB5 synchronously saves the
                    // complete level word and clears BTS before returning C+V; the new PLM
                    // executes its first timer/draw record later in this same gameplay frame.
                    // A caller which omitted the independent room owner cannot honestly
                    // preserve that lifecycle, so keep the missing integration seam explicit.
                    if (block.Behavior is 1 or 2)
                    {
                        if (plms is null)
                        {
                            throw new InvalidOperationException(
                                "Breakable grapple acquisition requires a RoomPlmSystem.");
                        }
                        if (!plms.TrySpawnBreakableGrappleBlock(level, block.Index, block.Behavior))
                        {
                            throw new InvalidOperationException(
                                "All 40 native PLM slots are occupied during grapple acquisition.");
                        }

                        return new GrappleBlockReaction(Carry: true, Overflow: true);
                    }

                    // Nonnegative BTS values beyond the four authored grapple reactions
                    // would index outside $D0D8-$D0E0 in the native selection table.
                    throw new InvalidDataException(
                        $"Invalid grapple block BTS ${block.Behavior:X2} at ({resolvedX},{resolvedY}).");

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

    private static GrappleMovementResult ConnectAcceptedFiring(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple,
        ushort previousXPosition,
        ushort previousYPosition)
    {
        // Movement type $1A is the Draygon-held actor route at $9B:B98C. It bypasses all
        // three direction tables and depends on untranslated enemy ownership/positioning.
        // A room-block connection should never normally arrive here in that pose, but an
        // explicit boundary is safer than fabricating either of the ordinary routes.
        byte sourceMovementType = samus.ReadMovementType(bus);
        if (sourceMovementType == 0x1a)
        {
            throw new NotSupportedException(
                "Draygon-held grapple connection requires the untranslated enemy actor route.");
        }

        bool movingVertically =
            samus.Kinematics.YSpeed != 0 || samus.Kinematics.YSubspeed != 0;
        int connectionTable = movingVertically
            ? MovingVerticallyConnectionTable
            : sourceMovementType == 5
                ? CrouchingConnectionTable
                : DefaultConnectionTable;
        int recordAddress = connectionTable + grapple.FireDirection * 4;
        ushort nextFunction = ReadWord(bus, recordAddress);
        ushort handler = ReadWord(bus, recordAddress + 2);

        // Each tiny native handler installs one prospective type-$16 pose and then jumps
        // to either BA61 (swinging) or BA9B (stuck). Keep the handler addresses visible:
        // pose alone is insufficient to distinguish malformed table data from retail data.
        (byte pose, bool swinging) = handler switch
        {
            ConnectSwingClockwiseHandler => (SamusState.GrappleSwingRightPose, true),
            ConnectSwingAnticlockwiseHandler => (SamusState.GrappleSwingLeftPose, true),
            ConnectStandingUpRightHandler => ((byte)0xa8, false),
            ConnectStandingRightHandler => ((byte)0xaa, false),
            ConnectStandingDownHandler => ((byte)0xab, false),
            ConnectStandingUpLeftHandler => ((byte)0xa9, false),
            ConnectCrouchingUpRightHandler => ((byte)0xb4, false),
            ConnectCrouchingRightHandler => ((byte)0xb6, false),
            ConnectCrouchingDownLeftHandler => ((byte)0xb7, false),
            ConnectCrouchingUpLeftHandler => ((byte)0xb5, false),
            _ => throw new InvalidDataException(
                $"Grapple connection direction {grapple.FireDirection} names unknown handler ${handler:X4}."),
        };
        ushort expectedFunction = swinging ? SwingingFunction : LockedInPlaceFunction;
        if (nextFunction != expectedFunction)
        {
            throw new InvalidDataException(
                $"Grapple connection handler ${handler:X4} requires function ${expectedFunction:X4}, " +
                $"but the ROM record names ${nextFunction:X4}.");
        }

        // BA61 and BA9B both derive the angle from the body position that fired the beam,
        // before command 9/10 changes pose and snaps Samus to the rope. The angle is stored
        // as an integer byte in the high half of the native word; its fraction becomes zero.
        int deltaX = unchecked((short)(samus.XPosition - grapple.AnchorX));
        int deltaY = unchecked((short)(samus.YPosition - grapple.AnchorY));
        byte angleByte = CalculateAngleFromXY(deltaX, deltaY);
        grapple.Angle = unchecked((ushort)(angleByte << 8));
        grapple.MirroredAngle = grapple.Angle;
        grapple.AngularVelocity = 0;
        grapple.RopeLengthDelta = 0;
        if (grapple.RopeLength >= 64)
            grapple.RopeLength = unchecked((ushort)(grapple.RopeLength - 24));

        // $94:AC11 publishes the native grapple-beam Start pair from the accepted endpoint,
        // angle, and possibly shortened rope. This is the hand attachment point, not the
        // independently authored Flare pair from the firing tables.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            bus,
            grapple,
            angleByte,
            grapple.RopeLength);
        grapple.RopeStartX = ropeStart.X;
        grapple.RopeStartY = ropeStart.Y;

        samus.Pose = pose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        if (swinging)
        {
            // Special pose command 9 runs $9B:BD95. Swinging copies Start into Flare, then
            // chooses the angle-authored animation frame and body offsets.
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            PositionSamusFromPendulum(bus, samus, grapple);
        }
        else
        {
            // Special pose command 10 runs $9B:BEEB. The locked body is positioned from
            // Start minus the raw NO-RUN origin table, then Flare is independently rebuilt
            // from the raw no-run flare table. Graphics-Y correction does not participate.
            int tableOffset = grapple.FireDirection * 2;
            short originX = unchecked((short)ReadWord(bus, NoRunOriginXTable + tableOffset));
            short originY = unchecked((short)ReadWord(bus, NoRunOriginYTable + tableOffset));
            short flareX = unchecked((short)ReadWord(bus, NoRunFlareXTable + tableOffset));
            short flareY = unchecked((short)ReadWord(bus, NoRunFlareYTable + tableOffset));
            samus.XPosition = unchecked((ushort)(grapple.RopeStartX - originX));
            samus.YPosition = unchecked((ushort)(grapple.RopeStartY - originY));
            grapple.BeamStartX = unchecked((ushort)(samus.XPosition + flareX));
            grapple.BeamStartY = unchecked((ushort)(samus.YPosition + flareY));
            grapple.Phase = GrapplePhase.ConnectedLocked;
        }

        // $91:EF53 is shared by connection commands 9 and 10. Speed-booster bookkeeping
        // has no host field yet, but every modeled speed word cleared there is reset here.
        // AccelerationMode is deliberately retained: the native common tail does not write it.
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.ExtraRunSpeed = 0;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;

        // Unlike ConnectUnobstructedSwing's explicit debugger seam, this anchor came from
        // BlockGrappleReaction and must be revalidated every connected frame at $9B:C802.
        grapple.ValidateAnchorBlock = true;
        grapple.SpecialAngleHandling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;

        ushort cameraPreviousX = ClampPreviousPosition(samus.XPosition, previousXPosition);
        ushort cameraPreviousY = ClampPreviousPosition(samus.YPosition, previousYPosition);
        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            Connected: true,
            OwnsMovement: true,
            LockedInPlace: !swinging,
            CameraPreviousX: cameraPreviousX,
            CameraPreviousY: cameraPreviousY);
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
        // `$9B:C51E` installs counter one after initializing the rope instruction slots.
        // The debugger's already-connected seam has no preceding firing history, so it
        // begins at the same authentic first flare state rather than inventing a static OBJ.
        grapple.FlareCounter = 1;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = 0;
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

    private static void RefreshFiringDrawOrigins(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        int tableOffset = grapple.FireDirection * 2;
        bool useRunOffsets = samus.ReadMovementType(bus) == 1;
        int originXTable = useRunOffsets ? RunOriginXTable : NoRunOriginXTable;
        int originYTable = useRunOffsets ? RunOriginYTable : NoRunOriginYTable;
        int flareXTable = useRunOffsets ? RunFlareXTable : NoRunFlareXTable;
        int flareYTable = useRunOffsets ? RunFlareYTable : NoRunFlareYTable;
        sbyte graphicsYOffset = samus.ReadGraphicsYOffset(bus);

        // `$9B:BF1B` rereads ROM instead of trusting `$0D0A/$0D0C`, then publishes Start
        // and previous-frame/Flare as separate coordinate pairs. Retain the same behavior
        // so a debugger edit to the offset tables is visible immediately in presentation.
        grapple.RopeStartX = unchecked((ushort)(
            samus.XPosition + (short)ReadWord(bus, originXTable + tableOffset)));
        grapple.RopeStartY = unchecked((ushort)(
            samus.YPosition + (short)ReadWord(bus, originYTable + tableOffset) -
            graphicsYOffset));
        grapple.BeamStartX = unchecked((ushort)(
            samus.XPosition + (short)ReadWord(bus, flareXTable + tableOffset)));
        grapple.BeamStartY = unchecked((ushort)(
            samus.YPosition + (short)ReadWord(bus, flareYTable + tableOffset) -
            graphicsYOffset));
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
                unchecked((byte)(grapple.Angle >> 8)),
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
        // $9B:BD44 only admits this extra angular impulse during the 16-frame terrain-
        // reflection timer. It is separate from ordinary Samus jump velocity and decays
        // inside bank $94 only after a successful angular movement pass.
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
        ushort targetAngle = unchecked((ushort)(grapple.Angle + signedDelta));
        byte targetAngleByte = unchecked((byte)(targetAngle >> 8));
        byte lastSafeAngleByte = unchecked((byte)(grapple.Angle >> 8));
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
                grapple.Angle = unchecked((ushort)((lastSafeAngleByte << 8) | 0x80));
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
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            throw new NotSupportedException(
                $"Grapple body probe left translated room storage at block ({blockX},{blockY}).");
        }

        int index = blockY * level.WidthInBlocks + blockX;
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            if ((uint)index >= (uint)(level.WidthInBlocks * level.HeightInBlocks))
            {
                throw new NotSupportedException(
                    $"Grapple swing extension BTS resolved outside room storage at index ${index:X4}.");
            }

            int resolvedX = index % level.WidthInBlocks;
            int resolvedY = index / level.WidthInBlocks;
            RoomCollisionBlock block = level.GetCollisionBlock(resolvedX, resolvedY);
            switch (block.CollisionType)
            {
                // The swing dispatcher intentionally treats shootable/bombable air as air;
                // unlike a firing endpoint, body contact does not spawn their PLMs.
                case 0:
                case 3:
                case 4:
                case 6:
                case 7:
                    return false;

                case 2:
                    // `$94:AA9E` has a mostly-zero BTS table: only spike-air BTS two queues
                    // `$0010` damage. It never collides, but it still starts the common
                    // 60-frame invulnerability and 10-frame knockback timers. The timer
                    // check must precede the BTS lookup so a second radial probe in this
                    // same six-point sweep cannot queue damage again.
                    ApplySwingSpikeDamage(samus, block.Behavior, solidSpike: false);
                    return false;

                // Slopes are unconditional collision in this grapple-specific dispatcher.
                case 1:
                case 8:
                case 9:
                case 0x0b:
                case 0x0c:
                case 0x0e:
                case 0x0f:
                    return true;

                case 0x0a:
                    // `$94:AB17` always reports collision. Before setting carry it applies
                    // `$003C` for BTS zero or `$0010` for BTS one; every other entry is
                    // literally zero. A negative BTS skips the table entirely.
                    ApplySwingSpikeDamage(samus, block.Behavior, solidSpike: true);
                    return true;

                case 5:
                    if (block.Behavior == 0)
                        return false;
                    index += unchecked((sbyte)block.Behavior);
                    continue;

                case 0x0d:
                    if (block.Behavior == 0)
                        return false;
                    index += unchecked((sbyte)block.Behavior) * level.WidthInBlocks;
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

    private static void PositionSamusFromPendulum(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $94:AC11 calls the same position helper used by terrain probes. Besides scaling the
        // signed table components, it applies the block-side 7/8 endpoint bias documented in
        // CalculateCollisionPoint; using one helper prevents visible art from disagreeing
        // with the collision body by one pixel.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            bus,
            grapple,
            unchecked((byte)(grapple.Angle >> 8)),
            grapple.RopeLength);
        grapple.RopeStartX = ropeStart.X;
        grapple.RopeStartY = ropeStart.Y;

        // $9B:BD95 copies native Start to native Flare during swinging. BeamStart retains
        // the older public name because the renderer and debugger already consume it, but
        // it semantically represents the Flare/draw origin throughout this translation.
        grapple.BeamStartX = ropeStart.X;
        grapple.BeamStartY = ropeStart.Y;

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
        samus.XPosition = unchecked((ushort)(ropeStart.X + xOffset));
        samus.YPosition = unchecked((ushort)(ropeStart.Y + yOffset));
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
        ClearFlareAnimation(grapple);
    }

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
        grapple.CollisionBounceTimer = 0;
        grapple.SpecialAngleHandling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        grapple.ValidateAnchorBlock = false;
        ClearFlareAnimation(grapple);
    }

    private static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // `$9B:C856/$C8C5/$C9CE/$CB8B` all clear these same shared charge/grapple WRAM
        // words before restoring the ordinary draw handler and projectile palette.
        grapple.FlareCounter = 0;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = 0;
    }

    private static int ScaleCoordinate(short sine, int length) => sine switch
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
    ConnectedLocked,
    WallGrab,
    WallGrabRelease,
    WallJumping,
    Dropped,
}

/// <summary>
/// Named equivalents of the bank-$9B grapple WRAM words used by connected swinging.
/// </summary>
public sealed class SamusGrappleState
{
    public GrapplePhase Phase { get; set; }
    public ushort AnchorX { get; set; }
    public ushort AnchorY { get; set; }

    /// <summary>
    /// Native <c>GrappleBeam_StartX/YPosition</c>: the physical rope origin at Samus's hand.
    /// It differs from the flare/draw origin while firing and in stuck-in-place poses.
    /// </summary>
    public ushort RopeStartX { get; set; }
    public ushort RopeStartY { get; set; }

    /// <summary>
    /// Native <c>GrappleBeam_FlareX/YPosition</c>, retained under the original host-facing
    /// BeamStart name for debugger/API compatibility. <c>DrawConnectedBeam</c> starts OAM
    /// here; do not use this pair to reconstruct command-10 body positioning.
    /// </summary>
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
    public short FlareXOffset { get; set; }
    public short FlareYOffset { get; set; }
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

    /// <summary>
    /// Host-readable equivalent of movement-handler pointer <c>$90:946E</c>. The grapple
    /// beam function becomes inactive one frame after release, but this independent Samus
    /// movement handler continues until apex underflow or vertical collision restores the
    /// normal handler. Keeping it separate from <see cref="Phase"/> prevents premature
    /// fallback to ordinary movement-type-two physics.
    /// </summary>
    public bool ReleasedMovementActive { get; set; }
    /// <summary>
    /// True only when the anchor came from the room block dispatcher. Debugger-published
    /// already-connected anchors deliberately leave this false because they may have no
    /// corresponding type-$E block in the selected diagnostic room.
    /// </summary>
    public bool ValidateAnchorBlock { get; set; }

    /// <summary>
    /// Host-readable form of bit 15 in `$0D26`. A close collision at minimum rope length
    /// requests bank-$9B's exact locked/wallgrab angle lookup; successful swing motion clears it.
    /// </summary>
    public bool SpecialAngleHandling { get; set; }

    /// <summary>
    /// Native `$0D30` grace counter. Wall-grab release seeds 30, then `$9B:C832` performs
    /// one DEC/BPL wall-jump check per frame until zero wraps to `$FFFF`.
    /// </summary>
    public ushort WallJumpTimer { get; set; }

    /// <summary>
    /// Distinguishes `$C856` reached from a locked type-$16 pose from ordinary firing
    /// cancellation, whose pre-existing movement and pose continue in the same frame.
    /// </summary>
    public bool CancelFromConnectedPose { get; set; }

    /// <summary>
    /// Native shared flare counter `$0CD0`. Grapple seeds one, increments it after tile
    /// uploads, and saturates at 120; teardown returns it to zero before ordinary charge
    /// handling can resume.
    /// </summary>
    public ushort FlareCounter { get; set; }

    /// <summary>Native main-flare bytecode index `$0CD2`, stored as a 16-bit WRAM word.</summary>
    public ushort FlareAnimationFrame { get; set; }

    /// <summary>Native decrement-before-test animation timer `$0CD8`.</summary>
    public ushort FlareAnimationTimer { get; set; }

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
    bool OwnsMovement = true,
    bool TerrainCollided = false,
    int CollisionDistanceFromFeet = 0,
    bool RopeLengthBlocked = false,
    bool AnchorDisconnected = false,
    bool SpecialAngleHandled = false,
    bool LockedInPlace = false,
    bool WallGrabEntered = false,
    bool WallJumpWindowOpened = false,
    bool WallProbeCollided = false,
    bool WallJumpQueued = false,
    bool WallJumpStarted = false,
    bool DropQueued = false,
    bool Dropped = false,
    ushort? CameraPreviousX = null,
    ushort? CameraPreviousY = null);

/// <summary>World pixel and room-block coordinates produced by bank-$94's radial helper.</summary>
internal readonly record struct GrappleCollisionPoint(
    ushort X,
    ushort Y,
    int BlockX,
    int BlockY);

/// <summary>
/// Result of the six-point angular body sweep. DistanceFromFeet retains the native countdown:
/// six is the point nearest the hand and one is the point furthest beyond Samus.
/// </summary>
internal readonly record struct GrappleSwingCollisionResult(bool Collided, int DistanceFromFeet);

/// <summary>
/// Processor-status subset returned by the bank-$94 grapple block-reaction dispatcher.
/// Carry means collision; overflow distinguishes a supported grapple connection from the
/// ordinary solid result that cancels the extending beam.
/// </summary>
internal readonly record struct GrappleBlockReaction(bool Carry, bool Overflow);
