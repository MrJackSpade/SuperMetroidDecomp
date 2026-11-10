namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The waiting updates still owed after the current game load, and the state the game enters once they have passed.
/// </summary>
internal sealed class GameLoadingWait
{
    /// <summary>The black waiting updates left.</summary>
    internal int WaitsRemaining { get; private set; }

    /// <summary>Where the game goes once the waits finish.</summary>
    internal GameLoadingCompletion Completion { get; private set; }

    /// <summary>
    /// True while the load waits out frames of the display: the next update continues that wait instead of starting a new
    /// update of the loading phase (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool Resumes => WaitsRemaining > 0;

    /// <summary>Owes <paramref name="waits"/> waiting updates, after which the game enters <paramref name="completion"/>.</summary>
    internal void Begin(int waits, GameLoadingCompletion completion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(waits);
        WaitsRemaining = waits;
        Completion = completion;
    }

    /// <summary>Counts one wait; returns whether it was the last.</summary>
    internal bool CountWait() => --WaitsRemaining == 0;
}
