namespace SuperMetroid.Core.Escape;

/// <summary>What the escape timer's phases show (<see cref="EscapeTimer"/>).</summary>
internal static class EscapeTimerPhaseDefinitions
{
    /// <summary>The phases the timer neither runs nor shows in: before a request, while it starts, and stopped at time-up.</summary>
    internal static readonly EscapeTimerPhase[] Inactive = [EscapeTimerPhase.Inactive, EscapeTimerPhase.Starting, EscapeTimerPhase.StoppedAtTimeUp];
}
