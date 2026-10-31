namespace SuperMetroid.Core.Bosses;

/// <summary>
/// One axis of a seeking boss's 8.8 velocity while it accelerates toward a target point: a run of steps, each raising or
/// lowering the velocity. A step that carries the velocity across zero, past either end of its word, makes the run's
/// next step one unit (1/256 pixel per frame) larger; and the run's first step is one unit larger when the signed
/// distance to the target wraps around the coordinate space, so the target lies the other way round from its plain
/// order. Ceres and Norfair Ridley (<c>$A6:D559</c>-<c>$A6:D61B</c>, <c>$A6:D62F</c>, <c>$A6:D6A6</c>) and the baby
/// Metroid (<c>$A9:F46B</c>, <c>$A9:F4E6</c>) seek this way; combining a run's steps into one integer sum changes their
/// trajectories.
/// </summary>
internal struct SeekingVelocityRun
{
    /// <summary>The velocity as a word, 0 through 65535; negative velocities lie in the word's upper half.</summary>
    private int velocity;

    /// <summary>The unit the run's next step adds to its size: one after a step crossed zero, otherwise zero.</summary>
    private int extraUnit;

    /// <summary>Starts a run from <paramref name="start"/> toward <paramref name="target"/> from <paramref name="position"/>.</summary>
    /// <param name="start">The axis velocity before the run.</param>
    /// <param name="position">The boss's whole coordinate on the axis, 0 through 65535.</param>
    /// <param name="target">The target's whole coordinate on the axis, 0 through 65535.</param>
    internal SeekingVelocityRun(SignedEightEight start, int position, int target)
    {
        velocity = start.RawValue;
        bool targetAhead = CoordinateAxis.Difference(position, target) < 0;
        bool orderedAhead = Ensure.BetweenInclusive(0, ushort.MaxValue, position) < Ensure.BetweenInclusive(0, ushort.MaxValue, target);
        extraUnit = targetAhead == orderedAhead ? 0 : 1;
    }

    /// <summary>The velocity the run has reached.</summary>
    internal readonly SignedEightEight Velocity => new(velocity);

    /// <summary>Starts a fresh run: the next step adds no unit for a crossing before it.</summary>
    internal void StartFresh() => extraUnit = 0;

    /// <summary>Raises the velocity by <paramref name="amount"/> units, 0 through 65535, plus any unit a crossing left.</summary>
    internal void Raise(int amount)
    {
        int sum = velocity + Ensure.BetweenInclusive(0, ushort.MaxValue, amount) + extraUnit;
        extraUnit = sum > ushort.MaxValue ? 1 : 0;
        velocity = WordWrap.Unsigned(sum);
    }

    /// <summary>Lowers the velocity by <paramref name="amount"/> units, 0 through 65535, plus any unit a crossing left.</summary>
    internal void Lower(int amount)
    {
        int difference = velocity - Ensure.BetweenInclusive(0, ushort.MaxValue, amount) - extraUnit;
        extraUnit = difference < 0 ? 1 : 0;
        velocity = WordWrap.Unsigned(difference);
    }
}
