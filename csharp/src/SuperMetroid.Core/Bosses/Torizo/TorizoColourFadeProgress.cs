namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How far a Torizo's colour fade has come (#2236), read from the shared fade counter's values at its first and latest
/// steps: the counter's value zero moves nothing, and each value from the fade's first moving one through the last moving
/// value (<see cref="TorizoShadeDefinitions.FadeLastStep"/>) is an equal part of the way. A fade that starts on a counter
/// another fade advanced, or restarts from a hurt colouring partway, has fewer, larger parts.
/// </summary>
internal readonly record struct TorizoColourFadeProgress
{
    /// <summary>Holds a fade whose steps took the counter's values <paramref name="first"/> through <paramref name="latest"/>.</summary>
    private TorizoColourFadeProgress(int first, int latest)
    {
        First = first;
        Latest = latest;
    }

    /// <summary>The counter's value at the fade's first step.</summary>
    internal int First { get; }

    /// <summary>The counter's value at the fade's latest step.</summary>
    internal int Latest { get; }

    /// <summary>A fade's first step, at counter value <paramref name="step"/>.</summary>
    internal static TorizoColourFadeProgress At(int step)
    {
        Ensure.BetweenInclusive(0, TorizoShadeDefinitions.FadeLastStep, step);
        return new TorizoColourFadeProgress(step, step);
    }

    /// <summary>This fade after a later step at counter value <paramref name="step"/>.</summary>
    /// <exception cref="InvalidOperationException">The step does not follow the latest: the shared counter only rises during a fade.</exception>
    internal TorizoColourFadeProgress Then(int step)
    {
        Ensure.BetweenInclusive(0, TorizoShadeDefinitions.FadeLastStep, step);
        if (step <= Latest)
            throw new InvalidOperationException($"A Torizo's colour fade took its step at counter {Latest}, then one at {step}.");
        return new TorizoColourFadeProgress(First, step);
    }

    /// <summary>
    /// The part of this fade run from counter value <paramref name="first"/>, a step it took: what it moved of colours
    /// written at that step, such as his belly glow's next step.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="first"/> is not one of the fade's steps.</exception>
    internal TorizoColourFadeProgress Since(int first) => new(Ensure.BetweenInclusive(First, Latest, first), Latest);

    /// <summary>The counter's first value that moves this fade's colours.</summary>
    private int FirstMoving => Math.Max(First, 1);

    /// <summary>The parts of the way the fade has come: none before its first moving value.</summary>
    internal int Moved => Math.Max(0, Latest - FirstMoving + 1);

    /// <summary>The parts of the whole way: its moving values from its first through the last moving value.</summary>
    internal int Length => TorizoShadeDefinitions.FadeLastStep - FirstMoving + 1;
}
