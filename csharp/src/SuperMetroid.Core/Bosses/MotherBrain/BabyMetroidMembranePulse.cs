namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One phase of the Baby Metroid's eight-phase membrane pulse (#2259), whose colours its membrane shows. Each pulse that reaches
/// <see cref="CryPhase"/> counts toward its periodic cry.
/// </summary>
public readonly partial record struct BabyMetroidMembranePulse
{
    /// <summary>The phase at <paramref name="step"/> of the cycle.</summary>
    private BabyMetroidMembranePulse(int step) => Step = step;

    /// <summary>The phase's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>The cycle's first phase.</summary>
    public static BabyMetroidMembranePulse First { get; } = new(0);

    /// <summary>The phases of the pulse.</summary>
    private const int PulsePhases = 8;

    /// <summary>The phase on which a pulse counts toward the Baby Metroid's periodic cry.</summary>
    public static BabyMetroidMembranePulse CryPhase { get; } = new(5);

    /// <summary>Every phase of the cycle, in order: the eight its membrane pulses through.</summary>
    public static IReadOnlyList<BabyMetroidMembranePulse> All { get; } = [.. Enumerable.Range(0, PulsePhases).Select(step => new BabyMetroidMembranePulse(step))];
}
