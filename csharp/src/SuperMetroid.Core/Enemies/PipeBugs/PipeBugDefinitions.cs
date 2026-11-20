namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Norfair Pipe Bug formation's roles (#1840). Each member departs when its formation count reaches its target,
/// centred on the leader's (<see cref="LeaderRole"/>), each place above or below it a fixed number of updates later or sooner. Also the Brinstar Pipe Bug's flight speed
/// (#2076), and when and how each kind of bug emerges and flies (#2156).
/// </summary>
internal static class PipeBugDefinitions
{
    /// <summary>How far a Brinstar Pipe Bug flies sideways each update, in pixels.</summary>
    internal const int BrinstarFlightStep = 2;

    /// <summary>The leader: it holds its height after rising and departs at 104.</summary>
    internal static readonly NorfairPipeBugRole LeaderRole = new(104, Stagger: null);

    /// <summary>The member just above the leader: it rises 16 pixels further and departs at 96.</summary>
    private static readonly NorfairPipeBugRole UpperNearRole = new(96, new NorfairPipeBugStagger(VerticalDirection.Up, 16));

    /// <summary>The member farthest above the leader: it rises 32 pixels further and departs at 88.</summary>
    private static readonly NorfairPipeBugRole UpperFarRole = new(88, new NorfairPipeBugStagger(VerticalDirection.Up, 32));

    /// <summary>The member just below the leader: it sinks 16 pixels back and departs at 112.</summary>
    private static readonly NorfairPipeBugRole LowerNearRole = new(112, new NorfairPipeBugStagger(VerticalDirection.Down, 16));

    /// <summary>The member farthest below the leader: it sinks 32 pixels back and departs at 120.</summary>
    private static readonly NorfairPipeBugRole LowerFarRole = new(120, new NorfairPipeBugStagger(VerticalDirection.Down, 32));

    /// <summary>
    /// How the room lists a formation (#2556): the leader, then the near and far members above it, then the near and far
    /// members below it.
    /// </summary>
    internal static readonly EnemyComposite<NorfairPipeBugRole> Formation = new([LeaderRole, UpperNearRole, UpperFarRole, LowerNearRole, LowerFarRole]);

    /// <summary>How far a Brinstar Pipe Bug rises above where it spawned before it flies, in pixels.</summary>
    internal const int BrinstarEmergenceHeight = 16;

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, to wake a hidden Brinstar Pipe Bug.</summary>
    internal const int BrinstarTriggerWidth = 64;

    /// <summary>How far above a hidden Brinstar Pipe Bug Samus may be, in pixels, and still wake it.</summary>
    internal const int BrinstarTriggerTop = 96;

    /// <summary>How long a Brinstar Pipe Bug that left the screen waits before it may emerge again, in updates.</summary>
    internal const int BrinstarRespawnWait = 48;

    /// <summary>How far ahead horizontally Samus comes, in pixels, to wake a yellow Pipe Bug.</summary>
    internal const int YellowTriggerDistance = 192;

    /// <summary>How close vertically Samus comes, in pixels and exclusive, to wake a yellow Pipe Bug.</summary>
    internal const int YellowTriggerHeight = 48;

    /// <summary>How long a yellow Pipe Bug shows before it starts its straight flight, in updates.</summary>
    internal const int YellowEmergenceDelay = 24;

    /// <summary>How far past Samus a yellow Pipe Bug flies, in pixels, before it begins its one arc.</summary>
    internal const int YellowArcTriggerDistance = 48;

    /// <summary>The place along the quadratic speed curve, mark 40, a yellow Pipe Bug's vertical arc starts from.</summary>
    internal static QuadraticSpeedCurvePlace YellowArcStart { get; } = QuadraticSpeedCurvePlace.FromMarks(40);

    /// <summary>The speed a Norfair Pipe Bug rises and staggers at, one pixel an update.</summary>
    internal static SignedSixteenSixteen NorfairRiseSpeed { get; } = SignedSixteenSixteen.FromWholePixels(1);
}
