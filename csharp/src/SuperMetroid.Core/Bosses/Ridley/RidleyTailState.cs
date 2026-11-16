namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's tail as both encounters swing it (#2228): its seven segments, what it is doing, how far it may swing, the whip
/// it aims and how hard its tip strikes.
/// </summary>
public sealed class RidleyTailState
{
    /// <summary>Makes a tail at its initial layout, striking for <paramref name="damage"/>, that starts no whips of its own.</summary>
    /// <param name="damage">The tip's contact damage before Samus's suit reduction.</param>
    internal RidleyTailState(int damage)
    {
        Damage = damage;
    }

    /// <summary>What Ridley's tail is doing: nothing, curling and whipping in flight, or one of the pogo and stab movements.</summary>
    public RidleyTailAction Action { get; internal set; } = RidleyTailAction.None;
    /// <summary>Gets the angular step (zero through eight) used by tail segment motion and stagger propagation.</summary>
    public int AngleDelta { get; internal set; } = 1;
    /// <summary>Gets the facing-dependent furthest-clockwise angle clockwise tail movement may reach.</summary>
    public RidleyTailAngle MinimumClockwiseAngle { get; internal set; } = RidleyTailDefinitions.InitialMinimumClockwise;
    /// <summary>Gets the facing-dependent furthest-counterclockwise angle counterclockwise tail movement may reach.</summary>
    public RidleyTailAngle MaximumCounterClockwiseAngle { get; internal set; } = RidleyTailDefinitions.InitialMaximumCounterClockwise;
    /// <summary>
    /// Gets the tail angle a clockwise whip stops at, or null while no clockwise whip is aimed. Aimed targets lie within
    /// a turn either side of straight down.
    /// </summary>
    public RidleyTailAngle? ClockwiseWhipTarget { get; internal set; }
    /// <summary>
    /// Gets the tail angle a counterclockwise whip stops at, or null while no counterclockwise whip is aimed. Aimed
    /// targets lie within a turn either side of straight down.
    /// </summary>
    public RidleyTailAngle? CounterClockwiseWhipTarget { get; internal set; }
    /// <summary>
    /// Gets whether a one-shot tail whip is requested, which neutral tail control consumes (#1940): a requested whip aims a
    /// whole turn further along its swing than an unrequested random one.
    /// </summary>
    public bool WhipRequested { get; internal set; }
    /// <summary>Gets whether neutral tail control may start random or proximity-triggered whips; the Ceres swoop disables it.</summary>
    public bool IdleWhipEnabled { get; internal set; }
    /// <summary>Gets the distance (fifteen sixteenths or one and a half pixels) tail segments extend each update toward their targets.</summary>
    public SignedEightEight ExtensionSpeed { get; internal set; } = RidleyTailDefinitions.StandardExtensionSpeed;
    /// <summary>Gets the angular stagger threshold (eight through 23) used to propagate a wave or whip from one segment to the next.</summary>
    public int IdealInterSegmentAngle { get; internal set; } = RidleyTailDefinitions.IdealInterSegmentSpacing;
    /// <summary>Gets the seven tail segments, base first, at their rest lengths and sequential initial angles.</summary>
    public RidleyTailSegment[] Segments { get; } = RidleyTail.CreateInitialRidleyTailSegments();
    /// <summary>Gets the tail tip's contact damage before Samus's suit reduction: fifteen in Ceres, 120 in Lower Norfair.</summary>
    public int Damage { get; }
}
