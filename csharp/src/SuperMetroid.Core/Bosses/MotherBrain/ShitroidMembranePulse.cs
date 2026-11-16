namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One phase of the Shitroid's eight-phase membrane pulse (#2259), whose colours its membrane shows. Each pulse that reaches
/// <see cref="CryPhase"/> counts toward its periodic cry.
/// </summary>
public readonly record struct ShitroidMembranePulse
{
    /// <summary>The phase at <paramref name="step"/> of the cycle.</summary>
    private ShitroidMembranePulse(int step) => Step = step;

    /// <summary>The phase's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>The cycle's first phase.</summary>
    public static ShitroidMembranePulse First { get; } = new(0);

    /// <summary>The phases of the pulse.</summary>
    private const int PulsePhases = 8;

    /// <summary>The phase on which a pulse counts toward the Shitroid's periodic cry.</summary>
    public static ShitroidMembranePulse CryPhase { get; } = new(5);

    /// <summary>Every phase of the cycle, in order: the eight its membrane pulses through.</summary>
    public static IReadOnlyList<ShitroidMembranePulse> All { get; } = [.. Enumerable.Range(0, PulsePhases).Select(step => new ShitroidMembranePulse(step))];

    /// <summary>The phase after this one; the last is followed by the first.</summary>
    public ShitroidMembranePulse Next => new((Step + 1) % PulsePhases);
}
