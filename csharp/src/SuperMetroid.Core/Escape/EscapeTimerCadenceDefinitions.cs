using System.Collections.Immutable;

namespace SuperMetroid.Core.Escape;

/// <summary>The escape timer's countdown cadence (#2135): the centiseconds each update of its repeating period removes.</summary>
internal static class EscapeTimerCadenceDefinitions
{
    /// <summary>
    /// The frame counter's place in the countdown's period, which picks the centiseconds the update removes (#1619): the
    /// counter's lowest seven bits, so the cadence repeats every 128 updates.
    /// </summary>
    internal static readonly UpdateCounterField PeriodPlace = new(0, 7);

    /// <summary>Decimal timer time unit: one second contains 100 centiseconds.</summary>
    internal const int CentisecondsPerSecond = 100;

    /// <summary>
    /// The centiseconds each update of the period removes, by its <see cref="PeriodPlace"/>: one or two, 43 ones and 85
    /// twos, so the 128 updates count 2.13 seconds. Which updates remove one decides the update the escape expires on.
    /// </summary>
    internal static readonly ImmutableArray<int> CentisecondsRemoved =
    [
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 1, 2, 2, 1, 2, 2, 1, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
        1, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1, 2, 2,
    ];
}
