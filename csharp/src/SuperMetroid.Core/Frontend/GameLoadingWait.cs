namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The waiting updates still owed after the current game load, and the state the game enters once they have passed.
/// </summary>
internal sealed partial class GameLoadingWait
{
    /// <summary>The load's black wait, counted once each waiting update.</summary>
    internal UpdateCountdown Wait { get; private set; } = UpdateCountdown.Ended();

    /// <summary>Where the game goes once the waits finish.</summary>
    internal GameLoadingCompletion Completion { get; private set; }

    /// <summary>
    /// True while the load waits out frames of the display: the next update continues that wait instead of starting a new
    /// update of the loading phase (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool Resumes => Wait.IsRunning;

    /// <summary>Owes <paramref name="waits"/> waiting updates, after which the game enters <paramref name="completion"/>.</summary>
    internal void Begin(int waits, GameLoadingCompletion completion)
    {
        Wait = UpdateCountdown.Lasting(waits);
        Completion = completion;
    }

    /// <summary>Counts one wait; returns whether it was the last.</summary>
    internal bool CountWait() => Wait.StepEnds();
}
