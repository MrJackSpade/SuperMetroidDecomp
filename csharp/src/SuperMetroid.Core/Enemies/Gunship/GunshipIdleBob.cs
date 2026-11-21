using SuperMetroid.Core.Bosses;
namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// One of the four steps of the landed gunship's idle bob (#2343), in the order the ship takes them: each holds for its
/// updates and moves the ship by its offset; the last is followed by the first.
/// </summary>
public readonly record struct GunshipIdleBob
{
    /// <summary>The step <paramref name="step"/> holds, with its place in the cycle.</summary>
    private GunshipIdleBob(ListStep<GunshipIdleBobDefinition> step) => Step = step;

    /// <summary>The step's definition, with its place in the cycle.</summary>
    private ListStep<GunshipIdleBobDefinition> Step { get; }

    /// <summary>The cycle's first step.</summary>
    public static GunshipIdleBob First => new(ListStep<GunshipIdleBobDefinition>.FirstOf(GunshipMotionDefinitions.IdleBobs));

    /// <summary>The updates until the ship takes the step after this one.</summary>
    public int Timer => Step.Entry.Timer;

    /// <summary>How far this step moves the ship, in whole pixels, downward positive.</summary>
    public int YDelta => Step.Entry.YDelta;

    /// <summary>The step after this one; the last is followed by the first.</summary>
    public GunshipIdleBob Next => new(Step.NextInCycle);
}
