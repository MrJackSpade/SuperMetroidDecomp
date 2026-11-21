namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One of the phases of the heat pulse a heated Norfair room glows in (#1598): the room's heat glow publishes each
/// phase as it shows that phase's look, and Samus's heat shimmer follows the published phase, restarting her suit's pulse
/// at the same phase's look. Phases run from <see cref="First"/> up and round again (<see cref="InOrder"/>).
/// </summary>
public readonly record struct HeatPhase
{
    /// <summary>The phases the heat pulse has.</summary>
    public const int Count = 16;

    /// <summary>The phase the pulse starts at, and the one the session holds before any room publishes a phase.</summary>
    public static HeatPhase First => InOrder[0];

    /// <summary>
    /// Every phase, in the order the heat pulse shows their looks, from <see cref="First"/> (#2366), each with the frame of
    /// the pulse that shows its look and the updates the pulse shows it for: longest at the coolest and quickest between.
    /// </summary>
    internal static IReadOnlyList<HeatPhase> InOrder { get; } =
    [
        new(0, 16), new(1, 4), new(2, 4), new(3, 5), new(4, 6), new(5, 7), new(6, 8), new(7, 8),
        new(8, 8), new(9, 8), new(10, 7), new(11, 6), new(12, 5), new(13, 4), new(14, 4), new(15, 16),
    ];

    /// <summary>Names the phase whose look frame <paramref name="look"/> of the pulse shows for <paramref name="lookUpdates"/> updates.</summary>
    private HeatPhase(int look, int lookUpdates)
    {
        Look = look;
        LookUpdates = lookUpdates;
    }

    /// <summary>The frame of a heat pulse that shows the phase's look (#2421): a pulse's frames are the phases' looks, in phase order.</summary>
    internal int Look { get; }

    /// <summary>The updates the heat pulse shows the phase's look for.</summary>
    internal int LookUpdates { get; }

    /// <summary>The phase's look in the pulse, for diagnostics.</summary>
    public override string ToString() => $"heat phase {Look}";
}
