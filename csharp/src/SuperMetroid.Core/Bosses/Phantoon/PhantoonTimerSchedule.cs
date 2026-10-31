using static SuperMetroid.Core.Bosses.Phantoon.PhantoonTimerDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Allocation-free calculated view of one native eight-bucket Phantoon phase-timer table.</summary>
public readonly struct PhantoonTimerSchedule : IReadOnlyList<ushort>
{
    /// <summary>Selects which native phase's buckets and duration scale this view exposes.</summary>
    private readonly TimerKind kind;
    /// <summary>Binds a calculated schedule to one Phantoon combat phase.</summary>
    internal PhantoonTimerSchedule(TimerKind kind) => this.kind = kind;
    /// <summary>Reads the authored short/medium/long permutation for this phase.</summary>
    private DurationChoice[] Choices => PhantoonTimerDefinitions.Choices[kind];
    /// <summary>Gets the eight random-selection buckets in the schedule.</summary>
    public int Count => Choices.Length;

    /// <summary>Calculates the selected phase duration in gameplay updates.</summary>
    /// <param name="index">Bucket index from zero through seven, normally selected by masked RNG or frame bits.</param>
    /// <returns>The exact short, medium, or long duration for this schedule.</returns>
    public ushort this[int index]
    {
        get
        {
            DurationChoice choice = Choices[index];
            int shortest = ShortestVulnerableExposureFrames * (kind == TimerKind.EyeClosed ? EyeShortScale : kind == TimerKind.RainHiding ? RainShortScale : 1);
            int medium = shortest * (kind == TimerKind.EyeClosed ? EyeMediumScale : TierDoubling);
            return CoordinateAxis.Wrap(choice switch
            {
                DurationChoice.Short => shortest,
                DurationChoice.Medium => medium,
                DurationChoice.Long => medium * TierDoubling,
                _ => throw new InvalidOperationException(),
            });
        }
    }
    /// <summary>Enumerates all eight calculated durations in native bucket order.</summary>
    /// <returns>An enumerator over the phase durations.</returns>
    public IEnumerator<ushort> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same eight phase buckets to non-generic collection consumers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
