namespace SuperMetroid.Core;

/// <summary>
/// A repeating cycle of steps paced by a count (#2563): <see cref="Steps"/> steps in turn, each held for
/// <see cref="CountsPerStep"/> counts, the first step again once the last has been held. A catalog names the cycle each
/// stepped picture, shake or colour keeps, stated as its rhythm; the count it is read on is the owner's own update count,
/// timer or tally.
/// </summary>
/// <remarks>
/// A count read here may run up or down and below zero: the step is the count's place in the cycle counted from below,
/// so a count of -1 sits on the cycle's last count and the step it holds.
/// </remarks>
public readonly record struct UpdateCycle
{
    /// <summary>A cycle of <paramref name="steps"/> steps, each held for <paramref name="countsPerStep"/> counts.</summary>
    /// <param name="steps">How many steps the cycle passes through, two or more.</param>
    /// <param name="countsPerStep">How many counts each step is held for, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The cycle has fewer than two steps, or a step is held for no count.</exception>
    public UpdateCycle(int steps, int countsPerStep)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(steps, 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(countsPerStep, 1);
        Steps = steps;
        CountsPerStep = countsPerStep;
    }

    /// <summary>How many steps the cycle passes through.</summary>
    public int Steps { get; }

    /// <summary>How many counts each step is held for.</summary>
    public int CountsPerStep { get; }

    /// <summary>The step, 0 through <see cref="Steps"/> less one, the cycle holds when its count reads <paramref name="count"/>.</summary>
    /// <param name="count">The count that paces the cycle.</param>
    public int StepAt(int count)
    {
        int length = Steps * CountsPerStep;
        int place = ((count % length) + length) % length;
        return place / CountsPerStep;
    }
}
