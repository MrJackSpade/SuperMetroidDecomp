using static SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitions;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>How long each step of the suit explosion shows.</summary>
internal static class SamusDeathExplosionTiming
{
    /// <summary>Returns the fixed duration for one authored explosion-frame index.</summary>
    public static ushort DurationForIndex(int index)
    {
        if (index is < 0 or >= RecordCount)
        {
            throw new InvalidDataException(
                $"Samus death-explosion timing index {index} is outside the " +
                $"{RecordCount}-record native sequence.");
        }

        return Durations[index];
    }
}
