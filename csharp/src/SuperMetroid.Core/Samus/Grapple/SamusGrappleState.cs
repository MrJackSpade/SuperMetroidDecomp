namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Named equivalents of the grapple words used by connected swinging.
/// </summary>
public sealed class SamusGrappleState
{
    /// <summary>Current host visual origins, rebound after state load; never changes physical hand offsets.</summary>
    public SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements? FlarePlacement { get; set; }
    /// <summary>Current displayed orientation mapping, never a collision/body offset source.</summary>
    public SuperMetroid.Core.Samus.Artwork.InstalledGrappleSwingFrames? SwingFrames { get; set; }
    /// <summary>Gets or sets the current grapple function-pointer phase.</summary>
    public GrapplePhase Phase { get; set; }
    /// <summary>Gets or sets the grapple anchor (beam endpoint) in room pixels.</summary>
    public Position16<RoomPixels> Anchor { get; set; }

    /// <summary>
    /// The physical rope origin at Samus's hand.
    /// It differs from the flare/draw origin while firing and in stuck-in-place poses.
    /// </summary>
    public Position16<RoomPixels> RopeStart { get; set; }

    /// <summary>
    /// The flare position, retained under the original host-facing
    /// BeamStart name. <c>DrawConnectedBeam</c> starts drawing
    /// here; do not use this position to reconstruct command-10 body positioning.
    /// </summary>
    public Position16<RoomPixels> BeamStart { get; set; }
    /// <summary>
    /// Rope length in pixels. Connected motion clamps it below 64, while firing grows it by twelve per frame
    /// and cancels once it reaches 128, so it never leaves 0 through 132; writes outside 0 through 32767 are
    /// rejected, which keeps every signed length test an ordinary comparison.
    /// </summary>
    [IntegerQuantity("rope length in pixels")]
    public int RopeLength
    {
        get;
        set => field = Ensure.BetweenInclusive(0, short.MaxValue, value);
    }

    /// <summary>Gets or sets the signed per-update change in rope length, -32768 through 32767.</summary>
    public int RopeLengthDelta { get; set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value); }
    /// <summary>
    /// The direction the beam was fired in: one of the ten projectile directions, which select the launch velocity,
    /// the hand and flare origins and the connection record. Firing admits only those ten directions.
    /// </summary>
    public SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection FireDirection { get; set; }
    /// <summary>The pose-change auto-fire timer; permits early pose-directed refiring.</summary>
    public int PoseChangeAutoFireTimer { get; set; }
    /// <summary>Gets or sets the signed 8.8 endpoint extension velocity.</summary>
    public EightEightVector<RoomPixels> ExtensionVelocity { get; set; }

    /// <summary>
    /// The beam's quarter velocity, whole and fractional, on both axes.
    /// Each firing frame rebuilds them from the extension
    /// velocity, but the outer bytes keep last frame's values, so they are state rather than
    /// a pure function of the velocity.
    /// </summary>
    public FixedVector<RoomPixels> QuarterVelocity { get; set; }
    /// <summary>Gets or sets the physical hand-origin offset from Samus.</summary>
    public Offset16<RoomPixels> OriginOffset { get; set; }
    /// <summary>Gets or sets the drawn flare's offset from Samus.</summary>
    public Offset16<RoomPixels> FlareOffset { get; set; }
    /// <summary>Gets or sets the endpoint's signed 16.16 offset from the rope origin.</summary>
    public FixedVector<RoomPixels> EndpointOffset { get; set; }
    /// <summary>Gets or sets the grapple angle.</summary>
    public RotationAngle Angle { get; set; }
    /// <summary>Gets or sets the angle mirrored for the connected-swing calculations.</summary>
    public RotationAngle MirroredAngle { get; set; }
    /// <summary>
    /// Gets or sets the signed angular velocity, -32768 through 32767: the swing clamps every update's sum to that range
    /// before storing it, and the bounce halves it.
    /// </summary>
    public int AngularVelocity { get; set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value); }
    /// <summary>Gets or sets the angular acceleration contributed by directional input, one of the swing's constants.</summary>
    public int DirectionInputAcceleration { get; set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value); }
    /// <summary>Gets or sets the angular acceleration contributed by gravity, one of the swing's constants.</summary>
    public int GravityAcceleration { get; set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value); }
    /// <summary>Gets or sets the angular-velocity correction term, -32768 through 32767.</summary>
    public int VelocityCorrection { get; set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value); }
    /// <summary>Gets or sets the signed vertical impulse applied by a grapple wall jump, -32768 through 32767.</summary>
    public int JumpImpulse { get; set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value); }
    /// <summary>Gets or sets the countdown suppressing repeated swing-collision bounces.</summary>
    public int CollisionBounceTimer { get; set; }
    /// <summary>Gets or sets whether the grapple movement uses submerged physics.</summary>
    public bool Submerged { get; set; }

    /// <summary>
    /// Host-readable equivalent of the released-from-swing movement handler. The grapple
    /// beam function becomes inactive one frame after release, but this independent Samus
    /// movement handler continues until apex underflow or vertical collision restores the
    /// normal handler. Keeping it separate from <see cref="Phase"/> prevents premature
    /// fallback to ordinary movement-type-two physics.
    /// </summary>
    public bool ReleasedMovementActive { get; set; }
    /// <summary>
    /// True only when the anchor came from the room block dispatcher. A beam started
    /// already connected leaves this false, because its anchor need not be a grapple block
    /// of the room.
    /// </summary>
    public bool ValidateAnchorBlock { get; set; }

    /// <summary>
    /// True when the accepted endpoint came from the interactive-enemy list. Such an
    /// anchor must be reacquired every connected frame; it cannot be treated as a permanent
    /// already-connected anchor merely because <see cref="ValidateAnchorBlock"/> is false.
    /// </summary>
    public bool ValidateAnchorEnemy { get; set; }

    /// <summary>
    /// Host-readable form of the close-collision flag. A close collision at minimum rope length
    /// requests the exact locked/wallgrab angle lookup; successful swing motion clears it.
    /// </summary>
    public bool SpecialAngleHandling { get; set; }

    /// <summary>
    /// Set by the swing while it is fast,
    /// it switches the camera's scrolling to its slow three-pixel branch.
    /// </summary>
    public bool SlowScrolling { get; set; }

    /// <summary>
    /// The wall-grab grace counter. Wall-grab release seeds 30, then the grace check performs
    /// one decrement-and-test wall-jump check per frame until zero wraps to 65535.
    /// </summary>
    public int WallJumpTimer { get; set; }

    /// <summary>
    /// Distinguishes a cancel reached from a locked grappling pose from ordinary firing
    /// cancellation, whose pre-existing movement and pose continue in the same frame.
    /// </summary>
    public bool CancelFromConnectedPose { get; set; }

    /// <summary>
    /// Grapple flare counter. Grapple seeds one, increments it after tile uploads, and saturates at 120;
    /// teardown returns it to zero before ordinary charge handling can resume. Writes outside 0 through 32767
    /// are rejected, which keeps the saturation test an ordinary comparison.
    /// </summary>
    [IntegerQuantity("updates the grapple flare has shown")]
    public int FlareCounter
    {
        get;
        set => field = Ensure.BetweenInclusive(0, short.MaxValue, value);
    }

    /// <summary>
    /// Position in the main-flare delay stream, 0 through 65535. The flare step advances and rewinds it as a
    /// sixteen-bit counter, so a malformed rewind operand wraps it exactly as the game's word arithmetic does;
    /// values outside that range are rejected.
    /// </summary>
    public int FlareAnimationFrame { get; set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Decrement-before-test animation timer.</summary>
    public WrappingCountdown FlareAnimationTimer { get; set; }

    /// <summary>Gets or sets the grapple endpoint animation countdown.</summary>
    public WrappingCountdown PointAnimationTimer { get; set; }
    /// <summary>
    /// Endpoint animation frame, normally zero through three. The advance keeps every ordinal it produces below
    /// 128 and restored ordinals are 0 through 255; other writes are rejected.
    /// </summary>
    public int PointAnimationFrame
    {
        get;
        set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>
    /// Sixteen rope segment timers. A connected rope draws slots 15 downward; retaining
    /// all sixteen makes shortening and lengthening the rope resume the original slot state.
    /// </summary>
    public WrappingCountdown[] SegmentAnimationTimers { get; } = new WrappingCountdown[16];

    /// <summary>
    /// Readable four-tile animation phases of the sixteen rope slots.
    /// Values zero through three are added to the first rope tile only when drawn.
    /// </summary>
    public int[] SegmentAnimationFrames { get; } = new int[16];

    /// <summary>
    /// Host-readable marker for the rope draw's special first timer expiry. The original stores
    /// the distinction implicitly in each instruction pointer; this flag keeps the C# state
    /// honest without exposing raw pointers as mutable gameplay data.
    /// </summary>
    public bool[] SegmentAnimationStarted { get; } = new bool[16];
}
