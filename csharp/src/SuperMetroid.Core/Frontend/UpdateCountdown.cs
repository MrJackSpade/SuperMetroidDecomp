namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A wait of a scene or of one of its phases, counted in updates at its true length (#2541), as a <see cref="ScreenFade"/>
/// steps the brightness and a <see cref="PacedFadeSteps"/> a colour fade at their own cadence: the scene or phase that
/// waits owns it, counts it once on each of its updates, and reads its age from it. A wait of <c>n</c> updates is over on
/// its <c>n</c>th count.
/// </summary>
internal sealed class UpdateCountdown
{
    /// <summary>A wait of <paramref name="updates"/> updates that has counted none yet.</summary>
    private UpdateCountdown(int updates) => Length = Ensure.BetweenInclusive(1, int.MaxValue, updates);

    /// <summary>The updates the wait lasts.</summary>
    [IntegerQuantity("updates the wait lasts")]
    internal int Length { get; }

    /// <summary>The updates the wait has counted so far, from none at its start to <see cref="Length"/> once it is over.</summary>
    [IntegerQuantity("updates the wait has counted")]
    internal int Age { get; private set; }

    /// <summary>Whether the wait has counted every one of its updates.</summary>
    internal bool IsOver => Age == Length;

    /// <summary>A wait of <paramref name="updates"/> updates that starts now, its first count on its first update.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal static UpdateCountdown Lasting(int updates) => new(updates);

    /// <summary>Counts one update of the wait.</summary>
    /// <returns>Whether this count ended the wait.</returns>
    /// <exception cref="InvalidOperationException">The wait is already over.</exception>
    internal bool Count()
    {
        if (IsOver)
            throw new InvalidOperationException($"A wait of {Length} updates is counted after it is over.");
        Age++;
        return IsOver;
    }
}
