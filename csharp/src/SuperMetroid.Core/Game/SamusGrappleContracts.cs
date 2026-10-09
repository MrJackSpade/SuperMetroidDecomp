namespace SuperMetroid.Core.Game;

/// <summary>Explicit function-pointer phase for the currently translated grapple route.</summary>
public enum GrapplePhase
{
    /// <summary>The grapple beam is inactive.</summary>
    Inactive,
    /// <summary>The beam is extending from Samus's hand.</summary>
    Firing,
    /// <summary>The firing beam has requested cancellation.</summary>
    CancelPending,
    /// <summary>Samus is swinging from a connected grapple anchor.</summary>
    ConnectedSwinging,
    /// <summary>Connected swinging has requested release.</summary>
    ReleaseFromSwing,
    /// <summary>Samus is held at a locked grapple angle.</summary>
    ConnectedLocked,
    /// <summary>Samus is holding a grapple wall-grab pose.</summary>
    WallGrab,
    /// <summary>The wall-grab connection is being released.</summary>
    WallGrabRelease,
    /// <summary>The independent grapple wall-jump movement is active.</summary>
    WallJumping,
    /// <summary>The grapple connection was dropped without a swing release.</summary>
    Dropped,
}

/// <summary>
/// The seven indices returned by <c>EnemyGrappleBeamCollisionDetection</c> at $A0:9E9A.
/// Values intentionally match bank-$9B's jump table rather than introducing host ordering.
/// </summary>
public enum GrappleEnemyReaction : ushort
{
    /// <summary>The enemy does not react to the grapple endpoint.</summary>
    None = 0,
    /// <summary>The beam attaches and grants the ordinary connection behavior.</summary>
    Attach = 1,
    /// <summary>The grapple endpoint kills the enemy.</summary>
    Kill = 2,
    /// <summary>The enemy cancels the extending beam.</summary>
    Cancel = 3,
    /// <summary>The beam attaches without granting enemy invincibility.</summary>
    AttachWithoutInvincibility = 4,
    /// <summary>The beam attaches and paralyzes the enemy.</summary>
    AttachAndParalyze = 5,
    /// <summary>The grapple interaction damages Samus.</summary>
    HurtSamus = 6,
}

/// <summary>One endpoint sample from the live interactive-enemy list.</summary>
/// <param name="Reaction">The grapple reaction declared by the sampled enemy.</param>
/// <param name="AnchorX">Horizontal anchor position in world pixels.</param>
/// <param name="AnchorY">Vertical anchor position in world pixels.</param>
/// <param name="EnemyDamage">Contact damage supplied when the reaction hurts Samus.</param>
public readonly record struct GrappleEnemyCollision(
    GrappleEnemyReaction Reaction,
    ushort AnchorX,
    ushort AnchorY,
    ushort EnemyDamage)
{
    /// <summary>Gets whether the reaction establishes a persistent grapple connection.</summary>
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
    /// <summary>Current host visual origins, rebound after state load; never changes physical hand offsets.</summary>
    [field: NonSerialized]
    public Assets.ChargeFlarePlacementCatalog? FlarePlacement { get; set; }
    /// <summary>Current displayed orientation mapping, never a collision/body offset source.</summary>
    [field: NonSerialized]
    public Assets.GrappleSwingFrameCatalog? SwingFrames { get; set; }
    /// <summary>Gets or sets the current grapple function-pointer phase.</summary>
    public GrapplePhase Phase { get; set; }
    /// <summary>Gets or sets the grapple anchor's horizontal world position in pixels.</summary>
    public ushort AnchorX { get; set; }
    /// <summary>Gets or sets the grapple anchor's vertical world position in pixels.</summary>
    public ushort AnchorY { get; set; }

    /// <summary>
    /// Native <c>GrappleBeam_StartX/YPosition</c>: the physical rope origin at Samus's hand.
    /// It differs from the flare/draw origin while firing and in stuck-in-place poses.
    /// </summary>
    public ushort RopeStartX { get; set; }
    /// <inheritdoc cref="RopeStartX"/>
    public ushort RopeStartY { get; set; }

    /// <summary>
    /// Native <c>GrappleBeam_FlareX/YPosition</c>, retained under the original host-facing
    /// BeamStart name for debugger/API compatibility. <c>DrawConnectedBeam</c> starts OAM
    /// here; do not use this pair to reconstruct command-10 body positioning.
    /// </summary>
    public ushort BeamStartX { get; set; }
    /// <inheritdoc cref="BeamStartX"/>
    public ushort BeamStartY { get; set; }
    /// <summary>
    /// Unsigned native length word at `$0D08`. Connected motion clamps it below 64, while
    /// firing must represent 120 before the following frame reaches the 128-pixel cutoff.
    /// </summary>
    public ushort RopeLength { get; set; }
    /// <summary>Gets or sets the signed per-update change in rope length.</summary>
    public short RopeLengthDelta { get; set; }
    /// <summary>Gets or sets the native grapple firing-direction index.</summary>
    public byte FireDirection { get; set; }
    /// <summary>Native $0CF6, GrappleBeam_PoseChangeAutoFireTimer; permits early pose-directed refiring.</summary>
    public ushort PoseChangeAutoFireTimer { get; set; }
    /// <summary>Gets or sets the signed horizontal endpoint extension velocity.</summary>
    public short ExtensionXVelocity { get; set; }
    /// <summary>Gets or sets the signed vertical endpoint extension velocity.</summary>
    public short ExtensionYVelocity { get; set; }

    /// <summary>
    /// <c>GrappleCollision_XQuarterSubVelocity/XQuarterVelocity</c> ($0D82/$0D84). Each firing
    /// frame rebuilds them from the extension velocity, but the outer bytes keep last frame's
    /// values, so they are state rather than a pure function of the velocity.
    /// </summary>
    public ushort XQuarterSubVelocity { get; set; }
    /// <inheritdoc cref="XQuarterSubVelocity"/>
    public ushort XQuarterVelocity { get; set; }
    /// <summary><c>GrappleCollision_YQuarterSubVelocity/YQuarterVelocity</c> ($0D86/$0D88).</summary>
    public ushort YQuarterSubVelocity { get; set; }
    /// <inheritdoc cref="YQuarterSubVelocity"/>
    public ushort YQuarterVelocity { get; set; }
    /// <summary>Gets or sets the physical hand-origin horizontal offset from Samus.</summary>
    public short OriginXOffset { get; set; }
    /// <summary>Gets or sets the physical hand-origin vertical offset from Samus.</summary>
    public short OriginYOffset { get; set; }
    /// <summary>Gets or sets the drawn flare's horizontal offset from Samus.</summary>
    public short FlareXOffset { get; set; }
    /// <summary>Gets or sets the drawn flare's vertical offset from Samus.</summary>
    public short FlareYOffset { get; set; }
    /// <summary>Gets or sets the endpoint's signed horizontal 16.16 offset from the rope origin.</summary>
    public int EndpointXOffsetFixed { get; set; }
    /// <summary>Gets or sets the endpoint's signed vertical 16.16 offset from the rope origin.</summary>
    public int EndpointYOffsetFixed { get; set; }
    /// <summary>Gets or sets the native grapple angle.</summary>
    public SnesAngle Angle { get; set; }
    /// <summary>Gets or sets the angle mirrored for the connected-swing calculations.</summary>
    public SnesAngle MirroredAngle { get; set; }
    /// <summary>Gets or sets the signed angular velocity.</summary>
    public short AngularVelocity { get; set; }
    /// <summary>Gets or sets the angular acceleration contributed by directional input.</summary>
    public short DirectionInputAcceleration { get; set; }
    /// <summary>Gets or sets the angular acceleration contributed by gravity.</summary>
    public short GravityAcceleration { get; set; }
    /// <summary>Gets or sets the native angular-velocity correction term.</summary>
    public short VelocityCorrection { get; set; }
    /// <summary>Gets or sets the signed vertical impulse applied by a grapple wall jump.</summary>
    public short JumpImpulse { get; set; }
    /// <summary>Gets or sets the countdown suppressing repeated swing-collision bounces.</summary>
    public ushort CollisionBounceTimer { get; set; }
    /// <summary>Gets or sets whether the grapple movement uses submerged physics.</summary>
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
    /// <c>GrappleBeam_SlowScrollingFlag</c>: set by <c>$9B:BD95</c> while the swing is fast,
    /// it switches <c>Main_Scrolling_Routine</c> to its slow three-pixel camera branch.
    /// </summary>
    public bool SlowScrolling { get; set; }

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

    /// <summary>Gets or sets the grapple endpoint animation countdown.</summary>
    public ushort PointAnimationTimer { get; set; }
    /// <summary>Ordinal of the native $200-byte-strided endpoint source, normally zero through three.</summary>
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
/// <param name="Phase">The phase selected after the grapple update.</param>
/// <param name="Fired">Whether the update created a newly firing beam.</param>
/// <param name="OwnsMovement">Whether grapple continues to own Samus movement.</param>
/// <param name="TerrainCollided">Whether the body sweep collided with terrain.</param>
/// <param name="CollisionDistanceFromFeet">Native body-sweep countdown at the collision point.</param>
/// <param name="RopeLengthBlocked">Whether collision prevented the requested rope-length change.</param>
/// <param name="WallJumpStarted">Whether release began the independent wall-jump handler.</param>
/// <param name="CameraPreviousX">Optional previous camera X coordinate published by the update.</param>
/// <param name="CameraPreviousY">Optional previous camera Y coordinate published by the update.</param>
/// <param name="PendingDropPose">Optional pose requested after a dropped connection.</param>
/// <param name="PendingConnection">Optional deferred pose and geometry for a new connection.</param>
/// <param name="PendingReleasePose">Optional pose requested after releasing a swing.</param>
public readonly record struct GrappleMovementResult(
    GrapplePhase Phase,
    bool Fired = false,
    bool OwnsMovement = true,
    bool TerrainCollided = false,
    int CollisionDistanceFromFeet = 0,
    bool RopeLengthBlocked = false,
    bool WallJumpStarted = false,
    ushort? CameraPreviousX = null,
    ushort? CameraPreviousY = null,
    byte? PendingDropPose = null,
    GrapplePendingConnection? PendingConnection = null,
    byte? PendingReleasePose = null);

/// <summary>
/// The prospective pose published by <c>HandleConnectingGrapple</c>. Bank $9B installs
/// the function, angle, and rope geometry immediately, while bank $91 applies this pose
/// later only if a higher-priority super-special transition did not win the frame.
/// </summary>
/// <param name="Pose">The prospective connected pose.</param>
/// <param name="Swinging">Whether the connection starts in swinging rather than locked mode.</param>
/// <param name="PreviousX">The previous horizontal position retained for movement resolution.</param>
/// <param name="PreviousY">The previous vertical position retained for movement resolution.</param>
public readonly record struct GrapplePendingConnection(
    byte Pose,
    bool Swinging,
    ushort PreviousX,
    ushort PreviousY);

/// <summary>World pixel and room-block coordinates produced by bank-$94's radial helper.</summary>
/// <param name="X">Horizontal world-pixel coordinate of the sampled point.</param>
/// <param name="Y">Vertical world-pixel coordinate of the sampled point.</param>
/// <param name="BlockX">Horizontal room-block index containing the point.</param>
/// <param name="BlockY">Vertical room-block index containing the point.</param>
internal readonly record struct GrappleCollisionPoint(
    ushort X,
    ushort Y,
    int BlockX,
    int BlockY);

/// <summary>
/// Result of the six-point angular body sweep. DistanceFromFeet retains the native countdown:
/// six is the point nearest the hand and one is the point furthest beyond Samus.
/// </summary>
/// <param name="Collided">Whether any sample in the angular body sweep collided with terrain.</param>
/// <param name="DistanceFromFeet">Native countdown identifying the colliding point's position in the sweep.</param>
internal readonly record struct GrappleSwingCollisionResult(bool Collided, int DistanceFromFeet);

/// <summary>
/// Processor-status subset returned by the bank-$94 grapple block-reaction dispatcher.
/// Carry means collision; overflow distinguishes a supported grapple connection from the
/// ordinary solid result that cancels the extending beam.
/// </summary>
/// <param name="Carry">Whether the block-reaction routine reported a collision.</param>
/// <param name="Overflow">Whether that collision supports a grapple connection rather than cancelling the beam.</param>
internal readonly record struct GrappleBlockReaction(bool Carry, bool Overflow);
