using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's eight-way random phase timers (#2262): its short, medium and long durations, and which of them
/// each of its eight picks takes.
/// </summary>
public sealed partial class PhantoonTimerSchedule
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

    /// <summary>
    /// The duration, in updates, a choice by a counter's cadence takes (#2556): the values <paramref name="cadence"/> reads
    /// of <paramref name="counter"/> take the picks' durations in turn, in pick order, so a cadence of fewer than eight values
    /// chooses among the first picks only, as the first round's eye-closed choice does.
    /// </summary>
    /// <param name="cadence">The cycle of the counter the choice reads, of at most eight steps.</param>
    /// <param name="counter">The counter the choice reads.</param>
    /// <exception cref="ArgumentException"><paramref name="cadence"/> has more than eight steps.</exception>
    internal int ChosenOn(UpdateCycle cadence, int counter) => cadence.Steps <= PickCount
        ? DurationOf(Picks[cadence.StepAt(counter)])
        : throw new ArgumentException($"A Phantoon timer's choice reads at most {PickCount} values.", nameof(cadence));

    /// <summary>
    /// The duration a random draw chooses (#2510): the eight values <paramref name="field"/> reads take the picks' durations
    /// in turn, in pick order.
    /// </summary>
    /// <param name="field">The field of the sample the draw reads, of eight values.</param>
    /// <exception cref="ArgumentException"><paramref name="field"/> does not read eight values.</exception>
    internal WeightedRandomChoice<int> ChosenBy(RandomSampleField field) => field.Outcomes == PickCount
        ? WeightedRandomChoice<int>.InTurn(field, [.. Picks.Select(DurationOf)])
        : throw new ArgumentException($"A Phantoon timer's draw reads {PickCount} values, not {field.Outcomes}.", nameof(field));

    /// <summary>The duration, in updates, a pick of <paramref name="length"/> takes.</summary>
    private int DurationOf(PhantoonTimerLength length) => length switch
    {
        PhantoonTimerLength.Short => Durations.Short,
        PhantoonTimerLength.Medium => Durations.Medium,
        PhantoonTimerLength.Long => Durations.Long,
        _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonTimerLength)} {length}."),
    };
}
