namespace SuperMetroid.Core.Escape;

/// <summary>
/// Verification access to <see cref="EscapeTimer"/> state only its own start writes (#2137): a check places the countdown
/// directly instead of running the timer down to it, and reaches the timer's corner through its real start (#2499).
/// </summary>
internal static class EscapeTimerAccess
{
    /// <summary>Sets the countdown of a check's timer and runs it into its corner.</summary>
    extension(EscapeTimer self)
    {
        /// <summary>Sets the remaining countdown, as the timer's start installs its duration.</summary>
        /// <param name="centiseconds">The remaining countdown, 0 through <see cref="EscapeTimerDefinitions.LongestTime"/>.</param>
        internal void SetRemaining(int centiseconds) => PrivateState.Call(self, "SetRemaining", centiseconds);

        /// <summary>
        /// Starts the Ceres countdown and runs the timer's own updates until it counts down in its corner
        /// (<see cref="EscapeTimerPhase.InPlace"/>), as a running escape's timer does; the check then sets the countdown it
        /// needs.
        /// </summary>
        internal void RunIntoPlace()
        {
            self.RequestCeresStart();
            for (int frame = 0; self.Phase != EscapeTimerPhase.InPlace; frame++)
                self.Process(frame, holdAtOneSecond: false);
        }
    }
}
