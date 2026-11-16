namespace SuperMetroid.Core.Escape;

/// <summary>
/// Verification access to <see cref="EscapeTimer"/> state only its own start writes (#2137): a check places the countdown
/// and its running step directly instead of running the timer up to them.
/// </summary>
internal static class EscapeTimerAccess
{
    /// <summary>Sets the countdown and the running step of a check's timer.</summary>
    extension(EscapeTimer self)
    {
        /// <summary>Sets the remaining countdown, as the timer's start installs its duration.</summary>
        /// <param name="centiseconds">The remaining countdown, 0 through <see cref="EscapeTimerDefinitions.LongestTime"/>.</param>
        internal void SetRemaining(int centiseconds) => PrivateState.Call(self, "SetRemaining", centiseconds);

        /// <summary>Selects <paramref name="phase"/> with the timer running, as the timer's start does.</summary>
        internal void SelectRunningPhase(EscapeTimerPhase phase) => PrivateState.Call(self, "SelectRunningPhase", phase);
    }
}
