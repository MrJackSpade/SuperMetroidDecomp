namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The NMI continuations still owed by the current <c>$82:8000</c> loading dispatch, and the dispatcher state it selects
/// after its final wait returns.
/// </summary>
internal sealed class GameLoadingWait
{
    /// <summary>The accepted blank NMI waits left.</summary>
    internal int WaitsRemaining { get; private set; }

    /// <summary>Where the dispatcher goes once the waits finish.</summary>
    internal GameLoadingCompletion Completion { get; private set; }

    /// <summary>
    /// True when the next update is an NMI continuation inside the loading dispatch rather than a new main-loop dispatch,
    /// so the main loop's RNG call is absent.
    /// </summary>
    internal bool Resumes => WaitsRemaining > 0;

    /// <summary>Owes <paramref name="waits"/> NMI waits, after which the dispatcher enters <paramref name="completion"/>.</summary>
    internal void Begin(int waits, GameLoadingCompletion completion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(waits);
        WaitsRemaining = waits;
        Completion = completion;
    }

    /// <summary>Counts one wait; returns whether it was the last.</summary>
    internal bool CountWait() => --WaitsRemaining == 0;
}
