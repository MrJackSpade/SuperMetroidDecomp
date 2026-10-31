namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Steps through Crocomire's hidden-wall rumble schedule.</summary>
internal static class CrocomireRumble
{
    /// <summary>The step the rumble starts at: the first oscillation's return target.</summary>
    internal const int FirstStep = 0;

    /// <summary>Whether <paramref name="step"/> has passed the schedule's last target, which ends the rumble.</summary>
    /// <exception cref="InvalidDataException"><paramref name="step"/> lies outside the schedule.</exception>
    internal static bool IsComplete(int step)
    {
        int count = CrocomireRumbleDefinitions.All.Length;
        if ((uint)step > (uint)count)
            throw new InvalidDataException($"Crocomire rumble step {step} is outside its {count} targets.");
        return step == count;
    }

    /// <summary>The target at <paramref name="step"/>, which has not completed the schedule.</summary>
    /// <exception cref="InvalidDataException"><paramref name="step"/> is not one of the schedule's targets.</exception>
    internal static CrocomireRumbleDefinition At(int step) =>
        (uint)step < (uint)CrocomireRumbleDefinitions.All.Length
            ? CrocomireRumbleDefinitions.All[step]
            : throw new InvalidDataException($"Crocomire rumble step {step} is not one of its targets.");
}
