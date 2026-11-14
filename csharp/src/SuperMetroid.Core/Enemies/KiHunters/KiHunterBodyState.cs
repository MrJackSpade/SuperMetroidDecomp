using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>A Ki-Hunter body's state: its patrol, swoop, wingless hops and whether it still has its wings.</summary>
public sealed class KiHunterBodyState
{
    /// <summary>The animation the body plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KiHunterSegment> Playback { get; set; }

    /// <summary>Main-AI step.</summary>
    public KiHunterAction? Action { get; internal set; }

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
    /// The animation-change angle, in whole steps (256ths of a turn) of the swoop angle: 240 for a leftward swoop or 144 for a
    /// rightward one. Passing it installs the swoop animation.
    /// </summary>
    public int AnimationChangeAngle { get; internal set; }

    /// <summary>
    /// The swoop angle, in whole steps (256ths of a turn): 128 or 255 when a swoop begins, stepped by the whole angular velocity, and
    /// ending the swoop at 256 or below 128, so it stays from zero to 257.
    /// </summary>
    public int SwoopAngle { get; internal set; }

    /// <summary>The whole-angle velocity cap: minus two for a leftward swoop or two for a rightward one.</summary>
    public int MaximumAngularVelocity { get; internal set; }

    /// <summary>
    /// The angular velocity in steps of a turn per update: zero when a swoop begins, accelerated each update with the
    /// fraction carrying into the whole part, and capped on its whole part. Its whole part steps the swoop angle.
    /// </summary>
    public SignedSixteenSixteen AngularVelocity { get; internal set; }

    /// <summary>The angular acceleration: plus one eighth of a step of a turn per update squared for a rightward swoop, minus one eighth for a leftward one.</summary>
    public SignedSixteenSixteen AngularAcceleration { get; internal set; }

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
