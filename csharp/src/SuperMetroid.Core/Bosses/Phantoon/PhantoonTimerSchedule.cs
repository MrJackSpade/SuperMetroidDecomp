using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's eight-way random phase timers (#2262): its short, medium and long durations, and which of them
/// each of its eight picks takes.
/// </summary>
public sealed class PhantoonTimerSchedule
{
    /// <summary>Keeps a timer's durations and its picks.</summary>
    /// <param name="durations">The timer's short, medium and long durations, in updates.</param>
    /// <param name="picks">The length each of the eight picks takes, in pick order.</param>
    internal PhantoonTimerSchedule(PhantoonTimerDurations durations, ImmutableArray<PhantoonTimerLength> picks)
    {
        Durations = durations;
        Picks = picks.Length == PickCount
            ? picks
            : throw new ArgumentException($"A Phantoon timer has {PickCount} picks, not {picks.Length}.", nameof(picks));
    }

    /// <summary>The number of picks a random number chooses among.</summary>
    internal const int PickCount = 8;

    /// <summary>The timer's short, medium and long durations, in updates.</summary>
    public PhantoonTimerDurations Durations { get; }

    /// <summary>The length each pick takes, in pick order.</summary>
    internal ImmutableArray<PhantoonTimerLength> Picks { get; }

    /// <summary>The duration, in updates, that pick <paramref name="pick"/>, zero through seven, takes.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pick"/> is outside zero through seven.</exception>
    public int this[int pick] => Picks[Ensure.BetweenInclusive(0, PickCount - 1, pick)] switch
    {
        PhantoonTimerLength.Short => Durations.Short,
        PhantoonTimerLength.Medium => Durations.Medium,
        PhantoonTimerLength.Long => Durations.Long,
        _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonTimerLength)} {Picks[pick]}."),
    };
}
