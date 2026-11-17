namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// How far Shitroid's drain of its Sidehopper victim has gone (#2369): untouched, latched on, then each step of the drain
/// the victim shows, the last of which leaves it a corpse. Gameplay reads it: it decides whether the victim hops or
/// drains after a landing and whether a shot, touch or Power Bomb starts it rotting.
/// </summary>
public readonly record struct SidehopperDrainStage
{
    /// <summary>The stage <paramref name="stage"/> steps into the drain.</summary>
    private SidehopperDrainStage(int stage) => Stage = stage;

    /// <summary>How many steps into the drain the stage is, from none (untouched) through eight (drained).</summary>
    [IntegerQuantity("drain progress counted from zero to eight; comparisons test how far draining has gone")]
    public int Stage { get; }

    /// <summary>Before Shitroid feeds: the victim still hops.</summary>
    public static SidehopperDrainStage Untouched { get; } = new(0);

    /// <summary>Shitroid has latched on to feed; the victim still shows the colours of Shitroid's room entry.</summary>
    public static SidehopperDrainStage LatchedOn { get; } = new(1);

    /// <summary>Fully drained: the victim is a corpse.</summary>
    public static SidehopperDrainStage Drained { get; } = new(8);

    /// <summary>The stages that show a step of the drain's colours, in order: every stage from the one after latching on.</summary>
    internal static IReadOnlyList<SidehopperDrainStage> Shaded { get; } =
        [.. Enumerable.Range(LatchedOn.Stage + 1, Drained.Stage - LatchedOn.Stage).Select(stage => new SidehopperDrainStage(stage))];

    /// <summary>Whether Shitroid has begun draining the victim.</summary>
    internal bool HasBegun => this != Untouched;

    /// <summary>Whether the victim is fully drained.</summary>
    internal bool IsDrained => this == Drained;

    /// <summary>Whether the stage shows a step of the drain's colours rather than the colours of Shitroid's room entry.</summary>
    internal bool IsShaded => Stage > LatchedOn.Stage;

    /// <summary>The stage's step of the drain's colours, from the first shaded stage.</summary>
    /// <exception cref="InvalidOperationException">The stage shows no step of the drain's colours.</exception>
    internal int ShadeStep => IsShaded
        ? Stage - LatchedOn.Stage - 1
        : throw new InvalidOperationException($"Drain stage {Stage} shows the colours of Shitroid's room entry.");

    /// <summary>The next stage of the drain, a step further on.</summary>
    /// <exception cref="InvalidOperationException">The victim is already drained.</exception>
    internal SidehopperDrainStage Next => IsDrained
        ? throw new InvalidOperationException("A drained Sidehopper drains no further.")
        : new(Stage + 1);
}
