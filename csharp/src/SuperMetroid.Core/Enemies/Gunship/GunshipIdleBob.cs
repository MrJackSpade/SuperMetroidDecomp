namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// One of the four steps of the landed gunship's idle bob (#2343), in the order the ship takes them: each holds for its
/// updates and moves the ship by its offset; the last is followed by the first.
/// </summary>
public readonly record struct GunshipIdleBob
{
    /// <summary>The step at <paramref name="step"/> of the cycle.</summary>
    private GunshipIdleBob(int step) => Step = step;

    /// <summary>The step's place in the cycle, from zero.</summary>
    private int Step { get; }

    /// <summary>Every step of the cycle, in the order the ship takes them.</summary>
    private static IReadOnlyList<GunshipIdleBob> All { get; } =
        [.. Enumerable.Range(0, GunshipMotionDefinitions.IdleBobs.Length).Select(step => new GunshipIdleBob(step))];

    /// <summary>The cycle's first step.</summary>
    public static GunshipIdleBob First => All[0];

    /// <summary>The updates until the ship takes the step after this one.</summary>
    public int Timer => GunshipMotionDefinitions.IdleBobs[Step].Timer;

    /// <summary>How far this step moves the ship, in whole pixels, downward positive.</summary>
    public int YDelta => GunshipMotionDefinitions.IdleBobs[Step].YDelta;

    /// <summary>The step after this one; the last is followed by the first.</summary>
    public GunshipIdleBob Next => All[(Step + 1) % All.Count];
}
