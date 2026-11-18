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

    /// <summary>
    /// Every phase, in the order the heat pulse shows their looks, from <see cref="First"/> (#2366); a phase is named by its
    /// place here (#2421).
    /// </summary>
    internal static IReadOnlyList<HeatPhase> InOrder { get; } = [.. Enumerable.Range(0, Count).Select(place => new HeatPhase(place))];

    /// <summary>The phase's place in <see cref="InOrder"/>, which tells the phases apart and is read nowhere else.</summary>
    private readonly int place;

    /// <summary>Names the phase at <paramref name="place"/> of <see cref="InOrder"/>.</summary>
    private HeatPhase(int place) => this.place = place;

    /// <summary>The phase's place in the pulse, for diagnostics.</summary>
    public override string ToString() => $"heat phase {place}";
}
