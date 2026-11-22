namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One frame of the Zebetites' shared two-colour pulse (#2259), which every Zebetite in the room shows and each primary
/// half steps.
/// </summary>
public readonly partial record struct ZebetitePulse
{
    /// <summary>The frame at <paramref name="step"/> of the cycle.</summary>
    private ZebetitePulse(int step) => Step = step;

    /// <summary>The frame's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>The frames of the pulse.</summary>
    private const int PulseFrames = 8;

    /// <summary>Every frame of the cycle, in order: the eight they pulse through, the first after the last.</summary>
    public static IReadOnlyList<ZebetitePulse> All { get; } = [.. Enumerable.Range(0, PulseFrames).Select(step => new ZebetitePulse(step))];
}
