using static SuperMetroid.Core.Bosses.Ridley.CeresRidleyEyeFadeDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Steps Ceres Ridley's eye fade through its authored shade schedule.</summary>
internal static class CeresRidleyEyeFade
{
    /// <summary>
    /// The eye fade at one of its steps. The first sixteen steps show shades
    /// fifteen down through zero; the remaining 48 steps hold shade zero before the fade completes.
    /// </summary>
    /// <remarks>
    /// Steps 0 through 15 show shade fifteen minus the step, steps 16 through 63 show shade zero, and step 64 completes
    /// the fade.
    /// No step follows the completing one, so a later step is refused.
    /// </remarks>
    public static CeresRidleyEyeFadeStep Get(int offset) => offset switch
    {
        < 0 => throw new InvalidDataException($"Ceres Ridley eye-fade offset {offset} precedes the authored schedule."),
        < 16 => new(false, 15 - offset),
        < FadeStepCount => new(false, 0),
        FadeStepCount => new(true, 0),
        _ => throw new InvalidDataException(
            $"Ceres Ridley eye-fade offset {offset} exceeds the authored " +
            $"{FadeStepCount + 1}-entry schedule."),
    };
}
