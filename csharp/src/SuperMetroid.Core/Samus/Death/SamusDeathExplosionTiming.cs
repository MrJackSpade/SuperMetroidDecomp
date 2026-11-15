using static SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitions;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>How long each step of the suit explosion shows.</summary>
internal static class SamusDeathExplosionTiming
{
    /// <summary>Returns the updates the explosion's <paramref name="step"/> shows.</summary>
    public static int StepDuration(int step)
    {
        if (step is < 0 or >= StepCount)
        {
            throw new InvalidDataException(
                $"Samus death-explosion step {step} is not one of the " +
                $"explosion's {StepCount} steps.");
        }

        return Durations[step];
    }
}
