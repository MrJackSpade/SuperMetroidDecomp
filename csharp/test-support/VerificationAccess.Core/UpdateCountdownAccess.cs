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
}
