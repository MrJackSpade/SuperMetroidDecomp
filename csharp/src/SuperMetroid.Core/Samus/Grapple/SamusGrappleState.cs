using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The grapple state connected swinging uses.
/// </summary>
public sealed class SamusGrappleState
{
    /// <summary>Current host visual origins, rebound after state load; never changes physical hand offsets.</summary>
    public SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements? FlarePlacement { get; set; }
    /// <summary>Current displayed orientation mapping, never a collision/body offset source.</summary>
    public SuperMetroid.Core.Samus.Artwork.InstalledGrappleSwingFrames? SwingFrames { get; set; }
    /// <summary>Gets or sets the current grapple phase.</summary>
    public GrapplePhase Phase { get; set; }
    /// <summary>Gets or sets the grapple anchor (beam endpoint) in room pixels.</summary>
    public Position16<RoomPixels> Anchor { get; set; }

    /// <summary>
    /// The physical rope origin at Samus's hand.
    /// It differs from the flare/draw origin while firing and in stuck-in-place poses.
    /// </summary>
    public Position16<RoomPixels> RopeStart { get; set; }

    /// <summary>
    /// The flare position, retained under the older
    /// BeamStart name. <c>DrawConnectedBeam</c> starts drawing
    /// here; do not use this position to reconstruct the locked connection's body positioning.
    /// </summary>
    public Position16<RoomPixels> BeamStart { get; set; }
    /// <summary>
    /// Rope length in pixels. Connected motion clamps it below 64, while firing grows it by twelve per frame
    /// and cancels once it reaches 128, so it never leaves 0 through
    /// <see cref="GrappleBeamDefinitions.Physics.LongestRopeLength"/>; other writes are rejected, which keeps every
    /// signed length test an ordinary comparison.
    /// </summary>
    [IntegerQuantity("rope length in pixels")]
    public int RopeLength
    {
        get;
        set => field = Ensure.BetweenInclusive(0, GrappleBeamDefinitions.Physics.LongestRopeLength, value);
    }

    /// <summary>
    /// The signed per-update change in rope length: the connection's retraction, the Up or Down adjustment of two
    /// pixels, firing's extension or none.
    /// </summary>
    public int RopeLengthDelta
    {
        get;
        set => field = Ensure.BetweenInclusive(GrappleBeamDefinitions.Physics.InitialConnectionRetraction,
            GrappleBeamDefinitions.Physics.FiringExtension, value);
    }
    /// <summary>
    /// The direction the beam was fired in: one of the ten projectile directions, which select the launch velocity,
    /// the hand and flare origins and the connection record. Firing admits only those ten directions.
    /// </summary>
    public SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection FireDirection { get; set; }
    /// <summary>The pose-change auto-fire timer; permits early pose-directed refiring.</summary>
    public int PoseChangeAutoFireTimer { get; set; }
    /// <summary>Gets or sets the signed 1/256-pixel endpoint extension velocity.</summary>
    public EightEightVector<RoomPixels> ExtensionVelocity { get; set; }

    /// <summary>
    /// The substep the extending beam's end moved by, on both axes. Each firing update rebuilds it from the extension
    /// speed, but it keeps parts of the update before's (<see cref="GrappleExtensionStepCarryover"/>), so it is state
    /// rather than a pure function of the speed.
    /// </summary>
    public FixedVector<RoomPixels> QuarterVelocity { get; set; }
    /// <summary>Gets or sets the physical hand-origin offset from Samus.</summary>
    public Offset16<RoomPixels> OriginOffset { get; set; }
    /// <summary>Gets or sets the drawn flare's offset from Samus.</summary>
    public Offset16<RoomPixels> FlareOffset { get; set; }
    /// <summary>Gets or sets the endpoint's signed 1/65536-pixel offset from the rope origin.</summary>
    public FixedVector<RoomPixels> EndpointOffset { get; set; }
    /// <summary>Gets or sets the grapple angle.</summary>
    public RotationAngle Angle { get; set; }
    /// <summary>Gets or sets the angle mirrored for the connected-swing calculations.</summary>
    public RotationAngle MirroredAngle { get; set; }
    /// <summary>
    /// The signed angular velocity: the swing clamps every update's sum to the maximum either way before storing it,
    /// a still pendulum is pushed off at a fixed speed below it, and the bounce halves it; other writes are rejected.
    /// </summary>
    public int AngularVelocity
    {
        get;
        set => field = Ensure.BetweenInclusive(-GrappleBeamDefinitions.Physics.MaximumAngularVelocity,
            GrappleBeamDefinitions.Physics.MaximumAngularVelocity, value);
    }
    /// <summary>The angular acceleration contributed by directional input: its magnitude either way, halved underwater, or none.</summary>
    public int DirectionInputAcceleration
    {
        get;
        set => field = Ensure.BetweenInclusive(-GrappleBeamDefinitions.Physics.DirectionInputMagnitude,
            GrappleBeamDefinitions.Physics.DirectionInputMagnitude, value);
    }
    /// <summary>The angular acceleration contributed by gravity: its magnitude either way or a fraction of it, or none.</summary>
    public int GravityAcceleration
    {
        get;
        set => field = Ensure.BetweenInclusive(-GrappleBeamDefinitions.Physics.GravityMagnitude,
            GrappleBeamDefinitions.Physics.GravityMagnitude, value);
    }
    /// <summary>The angular-velocity correction term: its magnitude either way or a quarter of it, or none.</summary>
    public int VelocityCorrection
    {
        get;
        set => field = Ensure.BetweenInclusive(-GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude,
            GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude, value);
    }
    /// <summary>
    /// The signed angular impulse a jump during the bounce window adds: its magnitude either way, halved underwater or by
    /// a bounce, decaying toward none.
    /// </summary>
    public int JumpImpulse
    {
        get;
        set => field = Ensure.BetweenInclusive(-GrappleBeamDefinitions.Physics.JumpImpulseMagnitude,
            GrappleBeamDefinitions.Physics.JumpImpulseMagnitude, value);
    }
    /// <summary>Gets or sets the countdown suppressing repeated swing-collision bounces.</summary>
    public int CollisionBounceTimer { get; set; }
    /// <summary>Gets or sets whether the grapple movement uses submerged physics.</summary>
    public bool Submerged { get; set; }

    /// <summary>
    /// Whether the released-from-swing movement handler is running. The grapple
    /// beam becomes inactive one frame after release, but this independent Samus
    /// movement handler continues until the upward speed passes zero at the apex or vertical collision restores the
    /// normal handler. Keeping it separate from <see cref="Phase"/> prevents premature
    /// fallback to ordinary movement-type-two physics.
    /// </summary>
    public bool ReleasedMovementActive { get; set; }
    /// <summary>
    /// True only when the anchor came from the room block check. A beam started
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
    /// The close-collision flag. A close collision at minimum rope length
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
    /// How far the muzzle flare has come: firing marks it just fired, each connected-beam draw moves it on a stage, and
    /// teardown clears it before ordinary charge handling can resume.
    /// </summary>
    public GrappleFlareStage FlareStage { get; set; }

    /// <summary>
    /// The main-flare picture the grapple's flare shows, one of the main flare's pictures: the flare restarts at the
    /// main flare's loop when fired and plays its animation on from there.
    /// </summary>
    public ChargeFlarePhase FlarePicture
    {
        get;
        set => field = ChargeFlareAnimations.IsPicture(ChargeFlareComponent.MainFlare, value.Index)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "The grapple's flare shows only the main flare's pictures.");
    }

    /// <summary>The wait the grapple's flare holds its picture for: the picture shows for it and one update more.</summary>
    public WrappingCountdown FlareAnimationTimer { get; set; }

    /// <summary>Gets or sets the grapple endpoint animation countdown.</summary>
    public WrappingCountdown PointAnimationTimer { get; set; }
    /// <summary>
    /// Endpoint animation frame, one of the endpoint's <see cref="GrapplePointAnimationDefinitions.FrameCount"/> frames (#1627):
    /// a fresh beam starts at the first and every advance steps on from a frame of the cycle, returning to the first past
    /// the last, so no frame outside the cycle exists; writing one is rejected.
    /// </summary>
    public int PointAnimationFrame
    {
        get;
        set => field = Ensure.BetweenInclusive(0, GrapplePointAnimationDefinitions.FrameCount - 1, value);
    }

    /// <summary>
    /// Rope draws left before the rope's pictures step on: a fresh beam waits
    /// <see cref="GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay"/> draws for its first step and
    /// <see cref="GrappleBeamDefinitions.Rendering.SegmentAnimationDelay"/> for each after it. Each draw that shows rope
    /// counts one off and restarts the wait on the draw that reaches none, so other values are rejected.
    /// </summary>
    public int RopeAnimationTimer
    {
        get;
        set => field = Ensure.BetweenInclusive(1, GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay, value);
    } = GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay;

    /// <summary>
    /// The rope's one animation phase: how many times its pictures have stepped on since the beam fired, counted round
    /// its cycle of <see cref="GrappleBeamDefinitions.Rendering.SegmentAnimationFrameCount"/> pictures. The segment at
    /// the muzzle shows the cycle's last picture moved on by the phase, and each segment further along the rope the
    /// picture before its neighbour's, so the pattern runs out along the rope as the phase steps. Other values are
    /// rejected.
    /// </summary>
    public int RopeAnimationPhase
    {
        get;
        set => field = Ensure.BetweenInclusive(0, GrappleBeamDefinitions.Rendering.SegmentAnimationFrameCount - 1, value);
    }
}
