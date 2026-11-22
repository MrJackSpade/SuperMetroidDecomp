using static SuperMetroid.Core.Escape.EscapeTimerCadenceDefinitions;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// Counts the escape timer down at its cadence (<see cref="EscapeTimerCadenceDefinitions"/>): the centiseconds each update
/// of the 128-update period removes.
/// </summary>
internal static partial class EscapeTimerCadence
{
    /// <summary>The centiseconds the update at <paramref name="frameCounter"/> removes: its period place's authored step.</summary>
    internal static int Centiseconds(int frameCounter) => CentisecondsRemoved[PeriodPlace.StepAt(frameCounter)];
}
