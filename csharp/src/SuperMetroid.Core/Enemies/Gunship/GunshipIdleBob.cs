namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// One of the four steps of the landed gunship's idle bob (#2343), in the order the ship takes them: each holds for its
/// updates and moves the ship by its offset; the last is followed by the first.
/// </summary>
public readonly record struct GunshipIdleBob
{
    /// <summary>The step at <paramref name="place"/> of the bob's authored cycle.</summary>
    private GunshipIdleBob(int place) => Place = place;

    /// <summary>The step's place in the bob's authored cycle (<see cref="GunshipMotionDefinitions.IdleBobs"/>).</summary>
    private int Place { get; }

    /// <summary>The step's definition.</summary>
    private GunshipIdleBobDefinition Step => GunshipMotionDefinitions.IdleBobs[Place];

    /// <summary>The cycle's first step.</summary>
    public static GunshipIdleBob First => new(0);

    /// <summary>The updates until the ship takes the step after this one.</summary>
    public int Timer => Step.Timer;

    /// <summary>How far this step moves the ship, in whole pixels, downward positive.</summary>
    public int YDelta => Step.YDelta;

    /// <summary>The step after this one; the last is followed by the first.</summary>
    public GunshipIdleBob Next => new((Place + 1) % GunshipMotionDefinitions.IdleBobs.Length);
}
