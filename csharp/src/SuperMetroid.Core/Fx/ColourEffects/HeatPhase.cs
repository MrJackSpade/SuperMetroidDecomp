namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One of the sixteen phases of the heat pulse a heated Norfair room glows in (#1598): the room's heat glow publishes each
/// phase as it shows that phase's look, and Samus's heat shimmer follows the published phase, restarting her suit's pulse
/// at the same phase's look. Phases run from <see cref="First"/> up and round again (<see cref="InOrder"/>).
/// </summary>
public readonly record struct HeatPhase
{
    /// <summary>The phases the heat pulse has.</summary>
    public const int Count = 16;

    /// <summary>The phase the pulse starts at, and the one the session holds before any room publishes a phase.</summary>
    public static HeatPhase First { get; } = new(0);

    /// <summary>Every phase, in the order the heat pulse shows their looks, from <see cref="First"/> (#2366).</summary>
    internal static IReadOnlyList<HeatPhase> InOrder { get; } = [.. Enumerable.Range(0, Count).Select(index => new HeatPhase(index))];

    /// <summary>Names phase <paramref name="index"/> of the pulse.</summary>
    /// <param name="index">The phase's place in the pulse, from zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">The pulse has no such phase.</exception>
    public HeatPhase(int index) => Index = Ensure.BetweenInclusive(0, Count - 1, index);

    /// <summary>The phase's place among the pulse's phases, from zero.</summary>
    public int Index { get; }
}
