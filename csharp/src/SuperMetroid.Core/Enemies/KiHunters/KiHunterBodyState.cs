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

    /// <summary>
    /// The swoop target: Samus's room position when the swoop begins, around which the dive's ellipse is centred. Only
    /// the X axis is stored and meaningful.
    /// </summary>
    public Position16<RoomPixels> SwoopTarget
    {
        get;
        internal set => field = new(value.X, 0);
    }

    /// <summary>
    /// The swoop origin: the body's room position when the swoop begins, from which the dive's ellipse hangs. Only the Y
    /// axis is stored and meaningful.
    /// </summary>
    public Position16<RoomPixels> SwoopOrigin
    {
        get;
        internal set => field = new(0, value.Y);
    }

    /// <summary>
    /// The swoop angle at which the swoop's swipe animation starts: a sixteenth of a turn short of a full turn for a
    /// leftward swoop, a sixteenth of a turn past the half turn for a rightward one. Passing it installs the swoop
    /// animation.
    /// </summary>
    public RotationAngle AnimationChangeAngle { get; internal set; }

    /// <summary>
    /// The swoop angle, in whole steps: a half turn, or one step short of a full turn, when a swoop begins, stepped by the
    /// whole steps of the angular velocity. While the swoop runs the angle lies in the second half of the turn; a rightward
    /// swoop ends as its angle comes round past the full turn onto the first half, a leftward one as it falls below the
    /// half turn, so the angle is read again only once the next swoop has set it.
    /// </summary>
    public RotationAngle SwoopAngle { get; internal set; }

    /// <summary>The fastest the swoop turns, whose whole steps cap the angular velocity's: two steps an update either way.</summary>
    public AngularSpeed MaximumAngularVelocity { get; internal set; }

    /// <summary>
    /// The swoop's turning speed: none when a swoop begins, accelerated each update with the fraction carrying into the
    /// whole steps, and capped on its whole steps. Its whole steps step the swoop angle.
    /// </summary>
    public AngularSpeed AngularVelocity { get; internal set; }

    /// <summary>The change in the swoop's turning speed each update: an eighth of a step, toward increasing angles for a rightward swoop and decreasing ones for a leftward one.</summary>
    public AngularSpeed AngularAcceleration { get; internal set; }

    /// <summary>The upper patrol boundary, sixteen pixels above the spawn Y. Only the Y axis is stored and meaningful.</summary>
    public Position16<RoomPixels> UpperPatrol { get; internal set; }

    /// <summary>The lower patrol boundary, sixteen pixels below the spawn Y. Only the Y axis is stored and meaningful.</summary>
    public Position16<RoomPixels> LowerPatrol { get; internal set; }

    /// <summary>The body's initial room position retained from initialization rather than the changing patrol position; collision recovery returns to patrol after rising above its Y.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The thinking countdown: twelve after landing or twenty-four after acid emission, consumed once the animation selects ground wait.</summary>
    public int WaitTimer { get; internal set; }

    /// <summary>The swipe-animation latch, cleared on swoop setup and set once the angle passes its facing-specific trigger.</summary>
    public bool SwoopAnimationChanged { get; internal set; }

    /// <summary>The whole-pixel swoop ellipse radii: the absolute horizontal separation from Samus and Samus Y minus body Y when a swoop begins.</summary>
    public Extent16<RoomPixels> SwoopRadius { get; internal set; }

    /// <summary>The velocity in pixels per update; positive Y is downward and gravity accumulates through the Y fraction.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The wingless latch, set by a wingless spawn or by health reaching the wings' detach health.</summary>
    public bool HasLostWings { get; internal set; }
}
