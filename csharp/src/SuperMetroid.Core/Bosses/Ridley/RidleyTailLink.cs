namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One link of Ridley's tail (#2238): the base, one of the five shaft links or the tip. It is made with the tail, holding
/// its lengths, and knows the links either side of it. The tail is shared by Ceres Ridley and Lower Norfair Ridley;
/// encounter-specific controllers only change the tail's common targets and the links' swings.
/// </summary>
public sealed class RidleyTailLink
{
    /// <summary>Makes a link at its rest length and initial angle, joined after <paramref name="previous"/>.</summary>
    /// <param name="definition">The link's lengths and initial angle.</param>
    /// <param name="previous">The link nearer the body, or null for the base.</param>
    internal RidleyTailLink(RidleyTailLinkDefinition definition, RidleyTailLink? previous)
    {
        RestDistance = definition.RestDistance;
        LongestDistance = definition.LongestDistance;
        Previous = previous;
        if (previous is not null)
            previous.Next = this;
        Distance = definition.RestDistance;
        Angle = definition.InitialAngle;
        Stagger = RidleyTailDefinitions.InitialStagger;
    }

    /// <summary>The link's rest length, which it shrinks back to and which a facing change or a pogo bounce restores.</summary>
    public SignedEightEight RestDistance { get; }

    /// <summary>The longest the link stretches while whipping, stabbing or bouncing; the base holds its length and the tip never stretches past rest.</summary>
    public SignedEightEight LongestDistance { get; }

    /// <summary>The link nearer Ridley's body, whose angle this link's direction is turned by, or null for the base.</summary>
    public RidleyTailLink? Previous { get; }

    /// <summary>The link nearer the tip, which this link starts swinging, or null for the tip.</summary>
    public RidleyTailLink? Next { get; private set; }

    /// <summary>Gets whether this link is participating in the current wave or whip; deactivation resets its stagger and reverses its swing.</summary>
    public bool Active { get; internal set; }

    /// <summary>
    /// Gets how far the link has swung since it started without yet starting the next link: it grows by the tail's swing
    /// step until it reaches the ideal link spacing, then has none (null) once the next link has been started, so it
    /// compares below no spacing (#1413). A link starts with a stagger of seventeen steps.
    /// </summary>
    public RidleyTailTurn? Stagger { get; internal set; }

    /// <summary>Gets which way the link swings: clockwise toward decreasing angles, or counterclockwise.</summary>
    public RotationDirection SwingDirection { get; internal set; } = RotationDirection.Clockwise;

    /// <summary>
    /// Gets the live distance from the preceding tail anchor, between half a pixel under its rest length and its longest;
    /// its whole pixels supply the offset composition.
    /// </summary>
    public SignedEightEight Distance { get; internal set; }

    /// <summary>
    /// Gets the distance this link is extending toward for a whip, stab or bounce, or null while it is not extending.
    /// It is stored separately from the live distance and cleared once the live distance passes it.
    /// </summary>
    public SignedEightEight? TargetDistance { get; internal set; }

    /// <summary>
    /// Gets the link's angle, counted from straight down without wrapping: its direction within the turn aims the link,
    /// and the whole count orders angles across the turn boundary for the clamp and whip-target comparisons.
    /// </summary>
    public RidleyTailAngle Angle { get; internal set; }

    /// <summary>
    /// The direction the link points in: its own angle's direction turned by the link before it, as the offset
    /// composition, the drawn tip and the breakup's tip all read it; the base points by its own angle alone.
    /// </summary>
    public TrigonometryAngle PointingDirection =>
        Previous is { } previous ? Angle.Direction.TurnedBy(previous.Angle.Direction) : Angle.Direction;

    /// <summary>Gets the signed whole-pixel displacement from the preceding anchor.</summary>
    public Offset16<RoomPixels> Offset { get; internal set; }

    /// <summary>Gets the composed room position used for drawing and tail contact.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
}
