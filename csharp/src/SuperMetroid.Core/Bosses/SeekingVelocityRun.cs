namespace SuperMetroid.Core.Bosses;

/// <summary>
/// One axis of a seeking boss's 8.8 velocity while it accelerates toward a target point: a run of steps, each raising or
/// lowering the velocity. A step that takes the velocity across zero, wrapping past either end of its range, makes the run's
/// next step one unit (1/256 pixel per frame) larger; and the run's first step is one unit larger when the signed
/// distance to the target wraps around the coordinate space, so the target lies the other way round from its plain
/// order. Ceres and Norfair Ridley and the baby
/// Metroid seek this way; combining a run's steps into one integer sum changes their
/// trajectories.
/// </summary>
internal struct SeekingVelocityRun
{
    /// <summary>The unit the run's next step adds to its size: one after a step crossed zero, otherwise zero.</summary>
    private int extraUnit;

    /// <summary>Starts a run from <paramref name="start"/> toward <paramref name="target"/> from <paramref name="position"/>.</summary>
    /// <param name="start">The axis velocity before the run.</param>
    /// <param name="position">The boss's whole coordinate on the axis, as its position holds it.</param>
    /// <param name="target">The target's whole coordinate on the axis, as its position holds it.</param>
    internal SeekingVelocityRun(SignedEightEight start, int position, int target)
    {
        Velocity = start;
        bool targetAhead = CoordinateAxis.Difference(position, target) < 0;
        bool orderedAhead = position < target;
        extraUnit = targetAhead == orderedAhead ? 0 : 1;
    }

    /// <summary>The velocity the run has reached.</summary>
    internal SignedEightEight Velocity { get; private set; }

    /// <summary>Starts a fresh run: the next step adds no unit for a crossing before it.</summary>
    internal void StartFresh() => extraUnit = 0;

    /// <summary>Raises the velocity by <paramref name="amount"/> units, never negative, plus any unit a crossing left.</summary>
    internal void Raise(int amount)
    {
        int sum = Velocity.RawValue + Ensure.AtLeastZero(amount) + extraUnit;
        extraUnit = sum > ushort.MaxValue ? 1 : 0;
        Velocity = SignedEightEight.Wrapped(sum);
    }

    /// <summary>Lowers the velocity by <paramref name="amount"/> units, never negative, plus any unit a crossing left.</summary>
    internal void Lower(int amount)
    {
        int difference = Velocity.RawValue - Ensure.AtLeastZero(amount) - extraUnit;
        extraUnit = difference < 0 ? 1 : 0;
        Velocity = SignedEightEight.Wrapped(difference);
    }
}
