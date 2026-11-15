using static SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitions;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>How long each step of the suit explosion shows.</summary>
internal static class SamusDeathExplosionTiming
{
    /// <summary>Returns the updates one authored explosion step shows.</summary>
    public static int DurationForIndex(int index)
    {
        if (index is < 0 or >= StepCount)
        {
            throw new InvalidDataException(
                $"Samus death-explosion step {index} is not one of the " +
                $"explosion's {StepCount} steps.");
        }

        return Durations[index];
    }
}
