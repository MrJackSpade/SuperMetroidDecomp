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
public static partial class SamusGrappleMovement
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
        bool useRunOffsets = samus.ReadMovementKind(bus) == SamusMovementType.Running;
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

        int orientationTable = SamusState.IsFacingLeft(bus, samus.Pose)
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
        int signedProbe = samus.IsFacingLeft(bus)
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
            bool facingLeft = samus.IsFacingLeft(bus);
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

}
