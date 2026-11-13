namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Golden Torizo's walking displacements, one per walking step: the first half of the steps walk left and the second
/// half walk right, each advancing the body by its stride phase's planted-foot movement.
/// </summary>
/// <remarks>
/// Every walking cue in Golden Torizo's animations names one of these steps (0 to 19 here); a cue between two steps
/// reaches no retail path and is not reproduced (#1473).
/// </remarks>
public static class GoldenTorizoWalk
{
    /// <summary>The number of walking steps: one left-walking and one right-walking step per stride phase.</summary>
    public static int StepCount => GoldenTorizoStrideDefinitions.SupportFootXs.Length * 2;

    /// <summary>The signed whole-pixel displacement of a walking step.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is not a walking step.</exception>
    public static int Velocity(int step)
    {
        if ((uint)step >= (uint)StepCount) throw new ArgumentOutOfRangeException(nameof(step));
        int phases = GoldenTorizoStrideDefinitions.SupportFootXs.Length;
        return (step < phases ? -1 : 1) * GoldenTorizoStride.HorizontalAdvance(step % phases);
    }
}
