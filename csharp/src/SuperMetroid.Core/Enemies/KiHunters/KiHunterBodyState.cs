using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>A Ki-Hunter body's state: its patrol, swoop, wingless hops and whether it still has its wings.</summary>
public sealed class KiHunterBodyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal KiHunterBodyState(KiHunterAction action) => Action = action;

    /// <summary>The animation the body plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KiHunterSegment> Playback { get; set; }

    /// <summary>Main-AI step.</summary>
    public KiHunterAction Action { get; internal set; }

    /// <summary>The swoop target: Samus's room X when the swoop begins, around which the dive's ellipse is centred.</summary>
    public int SwoopTargetX { get; internal set; }

    /// <summary>The swoop origin: the body's room Y when the swoop begins, from which the dive's ellipse hangs.</summary>
    public int SwoopOriginY { get; internal set; }

    /// <summary>
    /// The swoop angle at which the swoop's swipe animation starts, the way the body swoops (#2474): a sixteenth of a turn
    /// short of a full turn for a leftward swoop, a sixteenth of a turn past the half turn for a rightward one. Passing it
    /// starts the swipe.
    /// </summary>
    public RotationAngle SwipeAngle => Heading == HorizontalDirection.Left
        ? KiHunterMotionDefinitions.LeftSwoopAnimationChangeAngle
        : KiHunterMotionDefinitions.RightSwoopAnimationChangeAngle;

    /// <summary>
    /// The swoop angle, in whole steps: a half turn, or one step short of a full turn, when a swoop begins, stepped by the
    /// whole steps of the angular velocity. While the swoop runs the angle lies in the second half of the turn; a rightward
    /// swoop ends as its angle comes round past the full turn onto the first half, a leftward one as it falls below the
    /// half turn, so the angle is read again only once the next swoop has set it.
    /// </summary>
    public RotationAngle SwoopAngle { get; internal set; }

    /// <summary>
    /// The swoop's turning speed: none when a swoop begins, accelerated each update with the fraction carrying into the
    /// whole steps, and capped on its whole steps. Its whole steps step the swoop angle.
    /// </summary>
    public AngularSpeed AngularVelocity { get; internal set; }

    /// <summary>The upper patrol boundary's room Y, sixteen pixels above the spawn Y.</summary>
    public int UpperPatrolY { get; internal set; }

    /// <summary>The lower patrol boundary's room Y, sixteen pixels below the spawn Y.</summary>
    public int LowerPatrolY { get; internal set; }

    /// <summary>The body's initial room position retained from initialization rather than the changing patrol position; collision recovery returns to patrol after rising above its Y.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The thinking countdown: twelve after landing or twenty-four after acid emission, consumed once the animation selects ground wait.</summary>
    public UpdateCountdown WaitTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The whole-pixel swoop ellipse radii: the absolute horizontal separation from Samus and Samus Y minus body Y when a swoop begins.</summary>
    public Extent16<RoomPixels> SwoopRadius { get; internal set; }

    /// <summary>
    /// The way the body flies, swoops or hops (#2338), which its flight and landing animations face: a swoop heads the way
    /// it arcs, and a wall turns it around.
    /// </summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>
    /// How fast the body moves along <see cref="Heading"/>, in whole pixels an update (#2338): a pixel while it flies or
    /// recovers from a swoop, <see cref="KiHunterMotionDefinitions.GroundJumpHorizontalSpeed"/> while it hops.
    /// </summary>
    public int HorizontalSpeed { get; internal set; }

    /// <summary>The horizontal velocity: <see cref="HorizontalSpeed"/> toward <see cref="Heading"/> (#2338).</summary>
    public SignedSixteenSixteen HorizontalVelocity =>
        SignedSixteenSixteen.FromWholePixels(Heading == HorizontalDirection.Right ? HorizontalSpeed : -HorizontalSpeed);

    /// <summary>The vertical velocity in pixels per update; positive is downward and gravity accumulates through the fraction.</summary>
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>
    /// Whether the body still flies on its wings (#2473): it patrols, swoops, swipes or recovers from a swoop. A wingless
    /// spawn starts falling and health at the wings' detach health sets it falling, and no wingless action leads back to
    /// flight, so every other action is a body that has lost its wings.
    /// </summary>
    public bool Flies => Action is KiHunterAction.FlyingPatrol or KiHunterAction.Swooping or KiHunterAction.Swiping
        or KiHunterAction.RecoveringFromSwoop;
}
