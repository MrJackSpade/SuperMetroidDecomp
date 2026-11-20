namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's tail as both encounters swing it (#2228, #2238): its base, five shaft links and tip, made with Ridley at
/// their rest lengths and initial angles; what it is doing, how far it may swing, the whip it aims and how hard its tip
/// strikes.
/// </summary>
public sealed class RidleyTailState
{
    /// <summary>Makes a tail at its initial layout, striking for <paramref name="damage"/>, that starts no whips of its own.</summary>
    /// <param name="damage">The tip's contact damage before Samus's suit reduction.</param>
    internal RidleyTailState(int damage)
    {
        Damage = damage;
        Base = new RidleyTailLink(RidleyTailDefinitions.BaseLink, previous: null);
        var shaft = new List<RidleyTailLink>();
        RidleyTailLink previous = Base;
        foreach (RidleyTailLinkDefinition definition in RidleyTailDefinitions.ShaftLinks)
        {
            previous = new RidleyTailLink(definition, previous);
            shaft.Add(previous);
        }
        Shaft = shaft;
        Tip = new RidleyTailLink(RidleyTailDefinitions.TipLink, previous);
        Links = [Base, .. Shaft, Tip];
    }

    /// <summary>The link at Ridley's hip, which every wave starts from.</summary>
    public RidleyTailLink Base { get; }

    /// <summary>The five shaft links from the body out.</summary>
    public IReadOnlyList<RidleyTailLink> Shaft { get; }

    /// <summary>The tail's tip, which strikes Samus, probes the floor and blocks shots.</summary>
    public RidleyTailLink Tip { get; }

    /// <summary>The joint before the tip: the last shaft link, which also blocks shots and turns the tip's pose.</summary>
    public RidleyTailLink InnerJoint => Shaft[^1];

    /// <summary>Every link from the base to the tip, in the order the tail moves, composes and extends them.</summary>
    public IReadOnlyList<RidleyTailLink> Links { get; }

    /// <summary>What Ridley's tail is doing: nothing, curling and whipping in flight, or one of the pogo and stab movements.</summary>
    public RidleyTailAction Action { get; internal set; } = RidleyTailAction.None;
    /// <summary>Gets how far each swinging link turns each update and adds to its stagger.</summary>
    public RidleyTailTurn SwingStep { get; internal set; } = RidleyTailDefinitions.SlowNeutralSwing;
    /// <summary>
    /// Gets the facing-dependent furthest-clockwise angle clockwise tail movement may reach. The tail is made with a
    /// left-facing Ridley's whichever way he faces (#2471), as the game's tail setup gives it, until his tail's action sets
    /// the bounds for his facing.
    /// </summary>
    internal RidleyTailAngle MinimumClockwiseAngle { private get; set; } = RidleyTailReach.MinimumClockwise(RidleyFacing.Left);
    /// <summary>
    /// Gets the facing-dependent furthest-counterclockwise angle counterclockwise tail movement may reach, made with a
    /// left-facing Ridley's as <see cref="MinimumClockwiseAngle"/> is.
    /// </summary>
    internal RidleyTailAngle MaximumCounterClockwiseAngle { private get; set; } = RidleyTailReach.MaximumCounterClockwise(RidleyFacing.Left);
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
    /// The whip the fight orders of his tail, which his tail's neutral control carries out (#1940, #2562): an ordered whip
    /// aims a whole turn further along its swing than an unordered random one.
    /// </summary>
    public RidleyTailWhipOrder Whip { get; } = new();
    /// <summary>Gets whether neutral tail control may start random or proximity-triggered whips; the Ceres swoop disables it.</summary>
    public bool IdleWhipEnabled { get; internal set; }
    /// <summary>Gets the distance tail links extend each update toward their targets (<see cref="RidleyTailDefinitions.StandardExtensionSpeed"/> or <see cref="RidleyTailDefinitions.PogoExtensionSpeed"/>).</summary>
    public SignedEightEight ExtensionSpeed { get; internal set; } = RidleyTailDefinitions.StandardExtensionSpeed;
    /// <summary>
    /// Gets how far a link swings before it starts the next one, which sets how tightly the tail
    /// curls.
    /// </summary>
    public RidleyTailTurn IdealLinkSpacing { get; internal set; } = RidleyTailDefinitions.StandardLinkSpacing;
    /// <summary>Gets the tail tip's contact damage before Samus's suit reduction (<see cref="RidleyTailDefinitions.CeresTipDamage"/> or <see cref="RidleyTailDefinitions.NorfairTipDamage"/>).</summary>
    public int Damage { get; }

    /// <summary>The whip target a link swinging <paramref name="direction"/> chases, or null while none is aimed that way.</summary>
    internal RidleyTailAngle? WhipTarget(RotationDirection direction) =>
        direction == RotationDirection.Clockwise ? ClockwiseWhipTarget : CounterClockwiseWhipTarget;

    /// <summary>The furthest a link swinging <paramref name="direction"/> may swing before it turns back.</summary>
    internal RidleyTailAngle SwingLimit(RotationDirection direction) =>
        direction == RotationDirection.Clockwise ? MinimumClockwiseAngle : MaximumCounterClockwiseAngle;
}
