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
    internal SamusGrappleState(SamusArtworkBinding artwork) => Artwork = Ensure.NotNull(artwork);

    /// <summary>The installed artwork the flare's placement, the swing's pictures and the rope draw from.</summary>
    internal SamusArtworkBinding Artwork { get; }
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
    /// Rope length in pixels. Connected motion clamps it below the swing's longest rope, while firing grows it each frame
    /// and cancels once it reaches its longest, so it never leaves 0 through
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
    /// The signed per-update change in rope length: the connection's retraction, the Up or Down adjustment
    /// (<see cref="GrappleBeamDefinitions.Swing.RopeLengthStep"/>), firing's extension or none.
    /// </summary>
    public int RopeLengthDelta
    {
        get;
        set => field = Ensure.BetweenInclusive(GrappleBeamDefinitions.Physics.InitialConnectionRetraction,
            GrappleBeamDefinitions.Physics.FiringExtension, value);
    }
    /// <summary>
    /// The direction the beam was fired in: one of the ten projectile directions, which select the launch velocity,
    /// the hand and flare origins and the connection. Firing admits only those ten directions.
    /// </summary>
    public SamusProjectileDirection FireDirection { get; set; }
    /// <summary>The pose-change auto-fire timer; permits early pose-directed refiring.</summary>
    public UpdateCountdown PoseChangeAutoFireTimer { get; set; } = UpdateCountdown.Ended();
    /// <summary>The velocity the beam's end extends at, either way on each axis.</summary>
    public PixelVelocity<RoomPixels> ExtensionVelocity { get; set; }

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
    /// <summary>The endpoint's offset from the rope origin, its fraction of a pixel included.</summary>
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
    public UpdateCountdown CollisionBounceTimer { get; set; } = UpdateCountdown.Ended();
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
    /// What the connected beam's anchor is checked against on each following update, or null when nothing checks it. A
    /// block anchor came from the room block check and holds while a grapple block still stops the beam; an enemy anchor
    /// came from the interactive-enemy list and must be reacquired every connected update. A beam started already
    /// connected checks nothing, because its anchor need not be a grapple block of the room.
    /// </summary>
    public GrappleAnchorKind? ValidatedAnchor { get; set; }

    /// <summary>
    /// Whether the swing rests against a collision it met while the rope was at its shortest, close to her hand: until the
    /// swing next turns without colliding, each connected update compares its angle with the locked and wall-grab angles
    /// Samus snaps to. A swing that does not turn stays at rest; the grapple's start, end and connection free it.
    /// </summary>
    public bool RestsCloseToHand { get; private set; }

    /// <summary>
    /// How the camera follows the swing: slowly, three pixels an update, while the swing is fast, and ordinarily otherwise.
    /// The swing's positioning sets it each update; the grapple's firing, connection, release and end return it to ordinary.
    /// </summary>
    public GrappleCameraScroll CameraScroll { get; private set; }

    /// <summary>The swing stops against a collision met at its shortest rope, close to her hand.</summary>
    internal void RestCloseToHand() => RestsCloseToHand = true;

    /// <summary>The swing turns without colliding, or the grapple starts, connects or ends: it no longer rests.</summary>
    internal void SwingFreely() => RestsCloseToHand = false;

    /// <summary>The swing's positioning makes the camera follow it slowly while <paramref name="fast"/>, ordinarily otherwise.</summary>
    internal void FollowSwing(bool fast) => CameraScroll = fast ? GrappleCameraScroll.Slow : GrappleCameraScroll.Ordinary;

    /// <summary>The grapple's firing, connection, release or end returns the camera to its ordinary scroll.</summary>
    internal void ScrollCameraOrdinarily() => CameraScroll = GrappleCameraScroll.Ordinary;

    /// <summary>
    /// The wall-grab release window's wall-jump checks: wall-grab release opens it for
    /// <see cref="GrappleBeamDefinitions.WallGrab.WallJumpWindowFrames"/>, each update of the window counts one check, and
    /// the update that finds it over drops Samus.
    /// </summary>
    public UpdateCountdown WallJumpTimer { get; set; } = UpdateCountdown.Ended();

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
    /// The endpoint's animation (#1627, #2388): its pictures, over and over; a fresh beam starts
    /// it on its first.
    /// </summary>
    internal ShowingAnimationPlayer<GrappleEndpointSegment, GrappleEndpointFrame, NoAnimationCue> Endpoint { get; } =
        ShowingAnimationPlayer<GrappleEndpointSegment, GrappleEndpointFrame, NoAnimationCue>.StartShowing(
            GrappleAnimations.EndpointSegments, GrappleAnimations.Endpoint);

    /// <summary>
    /// The rope's travelling pattern (#2388): the picture its muzzle segment shows, stepping on every
    /// <see cref="GrappleBeamDefinitions.Rendering.SegmentAnimationDelay"/> rope draws, a fresh beam's first after
    /// <see cref="GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay"/>. Each segment further along the rope
    /// shows the picture before its neighbour's (<see cref="GrappleAnimations.RopePictureBefore"/>).
    /// </summary>
    internal ShowingAnimationPlayer<GrappleRopeSegment, GrappleRopePicture, NoAnimationCue> Rope { get; } =
        ShowingAnimationPlayer<GrappleRopeSegment, GrappleRopePicture, NoAnimationCue>.StartShowing(
            GrappleAnimations.RopeSegments, GrappleAnimations.Rope, GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay);
}
