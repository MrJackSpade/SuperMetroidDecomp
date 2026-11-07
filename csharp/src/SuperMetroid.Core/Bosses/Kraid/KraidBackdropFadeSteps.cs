namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The shared counter's values at which a fade of Kraid's backdrop took its steps (#1512), in rising order. A fade takes
/// consecutive values while it alone advances the counter; once a door's fade advances the same counter in the same
/// updates, the backdrop's steps take every other value, so the set names exactly the steps the colours went through.
/// </summary>
internal readonly record struct KraidBackdropFadeSteps
{
    /// <summary>Holds the set of values <paramref name="taken"/>, one bit per counter value.</summary>
    private KraidBackdropFadeSteps(int taken) => Taken = taken;

    /// <summary>One bit per counter value taken, value zero the lowest.</summary>
    internal int Taken { get; }

    /// <summary>A fade's first step, at counter value <paramref name="step"/>.</summary>
    internal static KraidBackdropFadeSteps At(int step)
    {
        Ensure.BetweenInclusive(0, KraidBackgroundDefinitions.BackdropFadeLastStep, step);
        return new KraidBackdropFadeSteps(1 << step);
    }

    /// <summary>The counter's value at the latest step.</summary>
    internal int Latest => 31 - int.LeadingZeroCount(Taken);

    /// <summary>Whether a step was taken at counter value <paramref name="step"/>.</summary>
    internal bool Took(int step) => (Taken & (1 << step)) != 0;

    /// <summary>Whether the only step taken is at counter value zero, which changes no colour.</summary>
    internal bool OnlyAtZero => Taken == 1;

    /// <summary>These steps followed by one at counter value <paramref name="step"/>.</summary>
    /// <exception cref="InvalidOperationException">The step does not follow the latest: the shared counter only rises during a fade.</exception>
    internal KraidBackdropFadeSteps Then(int step)
    {
        Ensure.BetweenInclusive(0, KraidBackgroundDefinitions.BackdropFadeLastStep, step);
        if (step <= Latest)
            throw new InvalidOperationException($"Kraid's backdrop fade took its step at counter {Latest}, then one at {step}.");
        return new KraidBackdropFadeSteps(Taken | (1 << step));
    }
}
