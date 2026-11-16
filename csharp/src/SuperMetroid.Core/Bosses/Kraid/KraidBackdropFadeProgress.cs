namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// How far a fade of Kraid's backdrop has come (#2215), read from the shared fade counter's values at its first and latest
/// steps: the counter's value zero moves nothing, and each value from the fade's first moving one through the last moving
/// value (<see cref="KraidBackgroundDefinitions.BackdropFadeLastStep"/>) is an equal part of the way. A fade that starts
/// on a counter another fade advanced has fewer, larger parts; a door's fade advancing the counter in the same updates
/// carries the fade on faster.
/// </summary>
internal readonly record struct KraidBackdropFadeProgress
{
    /// <summary>Holds a fade whose steps took the counter's values <paramref name="first"/> through <paramref name="latest"/>.</summary>
    private KraidBackdropFadeProgress(int first, int latest)
    {
        First = first;
        Latest = latest;
    }

    /// <summary>The counter's value at the fade's first step.</summary>
    internal int First { get; }

    /// <summary>The counter's value at the fade's latest step.</summary>
    internal int Latest { get; }

    /// <summary>A fade's first step, at counter value <paramref name="step"/>.</summary>
    internal static KraidBackdropFadeProgress At(int step)
    {
        Ensure.BetweenInclusive(0, KraidBackgroundDefinitions.BackdropFadeLastStep, step);
        return new KraidBackdropFadeProgress(step, step);
    }

    /// <summary>This fade after a later step at counter value <paramref name="step"/>.</summary>
    /// <exception cref="InvalidOperationException">The step does not follow the latest: the shared counter only rises during a fade.</exception>
    internal KraidBackdropFadeProgress Then(int step)
    {
        Ensure.BetweenInclusive(0, KraidBackgroundDefinitions.BackdropFadeLastStep, step);
        if (step <= Latest)
            throw new InvalidOperationException($"Kraid's backdrop fade took its step at counter {Latest}, then one at {step}.");
        return new KraidBackdropFadeProgress(First, step);
    }

    /// <summary>The counter's first value that moves this fade's colours.</summary>
    private int FirstMoving => Math.Max(First, 1);

    /// <summary>The parts of the way the fade has come: none before its first moving value.</summary>
    internal int Moved => Math.Max(0, Latest - FirstMoving + 1);

    /// <summary>The parts of the whole way: its moving values from its first through the last moving value.</summary>
    internal int Length => KraidBackgroundDefinitions.BackdropFadeLastStep - FirstMoving + 1;

    /// <summary>Whether the fade has come the whole way: its latest step took the last moving value.</summary>
    internal bool IsComplete => Latest == KraidBackgroundDefinitions.BackdropFadeLastStep;
}
