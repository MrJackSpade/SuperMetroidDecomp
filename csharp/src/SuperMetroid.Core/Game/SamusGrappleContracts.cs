namespace SuperMetroid.Core.Game;

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
/// The seven indices returned by <c>EnemyGrappleBeamCollisionDetection</c> at $A0:9E9A.
/// Values intentionally match bank-$9B's jump table rather than introducing host ordering.
/// </summary>
public enum GrappleEnemyReaction : ushort
{
    None = 0,
    Attach = 1,
    Kill = 2,
    Cancel = 3,
    AttachWithoutInvincibility = 4,
    AttachAndParalyze = 5,
    HurtSamus = 6,
}

/// <summary>One endpoint sample from the live interactive-enemy list.</summary>
public readonly record struct GrappleEnemyCollision(
    bool Collided,
    GrappleEnemyReaction Reaction,
    ushort EnemyNativeIndex,
    ushort AnchorX,
    ushort AnchorY,
    ushort EnemyDamage)
{
    public bool Attaches => Reaction is
        GrappleEnemyReaction.Attach or
        GrappleEnemyReaction.AttachWithoutInvincibility or
        GrappleEnemyReaction.AttachAndParalyze;
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
    /// True when the accepted endpoint came from bank-$A0's interactive-enemy list. Such an
    /// anchor must be reacquired every connected frame; it cannot be treated as the permanent
    /// debugger seam merely because <see cref="ValidateAnchorBlock"/> is false.
    /// </summary>
    public bool ValidateAnchorEnemy { get; set; }

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
