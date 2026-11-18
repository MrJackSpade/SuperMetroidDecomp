using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Samus's grapple beam: its phase, anchor, rope, swing, flare and the release that ends it.
/// </summary>
public sealed class SamusGrappleState
{
    /// <summary>Starts an inactive grapple whose flare, swing pictures and rope draw from <paramref name="artwork"/>.</summary>
    internal SamusGrappleState(SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding artwork) => Artwork = Ensure.NotNull(artwork);

    /// <summary>The installed artwork the flare's placement, the swing's pictures and the rope draw from.</summary>
    internal SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding Artwork { get; }
    /// <summary>The current grapple phase.</summary>
    public GrapplePhase Phase { get; set; }
    /// <summary>The grapple anchor (beam endpoint) in room pixels.</summary>
    public Position16<RoomPixels> Anchor { get; set; }

    /// <summary>
    /// The physical rope origin at Samus's hand.
    /// It differs from the flare/draw origin while firing and in stuck-in-place poses.
    /// </summary>
    public Position16<RoomPixels> RopeStart { get; set; }

    /// <summary>
    /// Where the beam's flare shows and its rope is drawn from: <see cref="SamusGrappleBeamDrawing.DrawConnectedBeam"/>
    /// starts drawing here. Do not use this position to reconstruct the locked connection's body positioning.
    /// </summary>
    public Position16<RoomPixels> FlareOrigin { get; set; }
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
    /// <summary>The signed 1/256-pixel endpoint extension velocity.</summary>
    public EightEightVector<RoomPixels> ExtensionVelocity { get; set; }

    /// <summary>
    /// The substep the extending beam's end moved by, on both axes. Each firing update rebuilds it from the extension
    /// speed, but it keeps parts of the update before's (<see cref="GrappleExtensionStepCarryover"/>), so it is state
    /// rather than a pure function of the speed.
    /// </summary>
    public FixedVector<RoomPixels> ExtensionSubstep { get; set; }
    /// <summary>The hand's offset from Samus.</summary>
    public Offset16<RoomPixels> OriginOffset { get; set; }
    /// <summary>The drawn flare's offset from Samus.</summary>
    public Offset16<RoomPixels> FlareOffset { get; set; }
    /// <summary>The endpoint's signed 1/65536-pixel offset from the rope origin.</summary>
    public FixedVector<RoomPixels> EndpointOffset { get; set; }
    /// <summary>The grapple angle.</summary>
    public RotationAngle Angle { get; set; }
    /// <summary>The angle mirrored for the connected-swing calculations.</summary>
    public RotationAngle MirroredAngle { get; set; }
    /// <summary>
    /// The signed angular velocity: the swing clamps every update's sum to the maximum either way before storing it,
    /// a still pendulum is pushed off at a fixed speed below it, and the bounce halves it; other writes are rejected.
    /// </summary>
    public AngularSpeed AngularVelocity
    {
        get;
        set => field = NoFasterThan(GrappleBeamDefinitions.Physics.MaximumAngularVelocity, value);
    }
    /// <summary>The angular acceleration contributed by directional input: its magnitude either way, halved underwater, or none.</summary>
    public AngularSpeed DirectionInputAcceleration
    {
        get;
        set => field = NoFasterThan(GrappleBeamDefinitions.Physics.DirectionInputMagnitude, value);
    }
    /// <summary>The angular acceleration contributed by gravity: its magnitude either way or a fraction of it, or none.</summary>
    public AngularSpeed GravityAcceleration
    {
        get;
        set => field = NoFasterThan(GrappleBeamDefinitions.Physics.GravityMagnitude, value);
    }
    /// <summary>The angular-velocity correction term: its magnitude either way or a quarter of it, or none.</summary>
    public AngularSpeed VelocityCorrection
    {
        get;
        set => field = NoFasterThan(GrappleBeamDefinitions.Physics.VelocityCorrectionMagnitude, value);
    }
    /// <summary>
    /// The signed angular impulse a jump during the bounce window adds: its magnitude either way, halved underwater or by
    /// a bounce, decaying toward none.
    /// </summary>
    public AngularSpeed JumpImpulse
    {
        get;
        set => field = NoFasterThan(GrappleBeamDefinitions.Physics.JumpImpulseMagnitude, value);
    }

    /// <summary><paramref name="value"/>, which turns no faster than <paramref name="most"/> either way; a faster one is rejected.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> turns faster than <paramref name="most"/>.</exception>
    private static AngularSpeed NoFasterThan(AngularSpeed most, AngularSpeed value) =>
        value >= -most && value <= most
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, $"The swing's angular terms turn no faster than {most} either way.");
    /// <summary>The countdown suppressing repeated swing-collision bounces.</summary>
    public int CollisionBounceTimer { get; set; }
    /// <summary>Whether the grapple movement uses submerged physics.</summary>
    public bool Submerged { get; set; }

    /// <summary>
    /// Whether the released-from-swing movement is running. The grapple
    /// beam becomes inactive one frame after release, but this independent Samus
    /// movement continues until the upward speed passes zero at the apex or vertical collision restores the
    /// pose's own movement. Keeping it separate from <see cref="Phase"/> prevents premature
    /// fallback to ordinary normal-jumping physics.
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
    /// Whether the swing collided while the rope was at its shortest: such a collision has the next update look for the
    /// locked or wall-grab angle Samus snaps to, and successful swing motion clears it.
    /// </summary>
    public bool CloseCollision { get; set; }

    /// <summary>
    /// Set by the swing while it is fast,
    /// it switches the camera's scrolling to its slow three-pixel branch.
    /// </summary>
    public bool SlowScrolling { get; set; }

    /// <summary>
    /// The wall-grab grace counter. Wall-grab release seeds 30, then the grace check performs
    /// one decrement-and-test wall-jump check per frame until the count passes zero.
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
    /// The grapple's flare (#2396): the main flare's animation, which the flare restarts at the main flare's loop when
    /// fired and plays on from there, and which teardown clears.
    /// </summary>
    internal ChargeFlareComponentState Flare { get; } = new(ChargeFlareComponent.MainFlare);

    /// <summary>
    /// The endpoint's animation (#1627, #2388): its four pictures, six beam draws each, over and over; a fresh beam starts
    /// it on its first.
    /// </summary>
    internal AnimationPlayer<GrappleEndpointSegment, GrappleEndpointFrame, NoAnimationCue> Endpoint { get; } =
        AnimationPlayer<GrappleEndpointSegment, GrappleEndpointFrame, NoAnimationCue>.StartShowing(
            GrappleAnimations.EndpointSegments, GrappleAnimations.Endpoint);

    /// <summary>
    /// The rope's travelling pattern (#2388): the picture its muzzle segment shows, stepping on every
    /// <see cref="GrappleBeamDefinitions.Rendering.SegmentAnimationDelay"/> rope draws, a fresh beam's first after
    /// <see cref="GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay"/>. Each segment further along the rope
    /// shows the picture before its neighbour's (<see cref="GrappleAnimations.RopePictureBefore"/>).
    /// </summary>
    internal AnimationPlayer<GrappleRopeSegment, GrappleRopePicture, NoAnimationCue> Rope { get; } =
        AnimationPlayer<GrappleRopeSegment, GrappleRopePicture, NoAnimationCue>.StartShowing(
            GrappleAnimations.RopeSegments, GrappleAnimations.Rope, GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay);
}
