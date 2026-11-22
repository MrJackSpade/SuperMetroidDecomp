namespace SuperMetroid.Core;

/// <summary>Builds the waits a check or a recorded state's import places partway through, which production only starts.</summary>
internal static class UpdateCountdownAccess
{
    /// <summary>A wait of <paramref name="length"/> updates that has counted <paramref name="age"/> of them.</summary>
    /// <exception cref="InvalidOperationException">The age is past the wait's length.</exception>
    internal static UpdateCountdown AtAge(int length, int age)
    {
        UpdateCountdown wait = UpdateCountdown.For(length);
        for (int counted = 0; counted < age; counted++)
            wait.Count();
        return wait;
    }

    /// <summary>
    /// The wait a recorded count of updates left stands for: a count at zero or below, which the cartridge's countdown has
    /// run past, is over.
    /// </summary>
    internal static UpdateCountdown Recorded(int count) => UpdateCountdown.For(Math.Max(0, count));

    /// <summary>
    /// Makes <paramref name="wait"/> the wait <paramref name="placed"/> is, of its length and with its updates left: a wait
    /// its owner holds for good, which a check or an import places partway through.
    /// </summary>
    internal static void PlaceAt(this UpdateCountdown wait, UpdateCountdown placed)
    {
        wait.Length = placed.Length;
        wait.UpdatesLeft = placed.UpdatesLeft;
    }
}
