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
    /// #625 / #651, as the Japanese and North American v1.0 release fades the eyes: step 0..15 shows shade
    /// 15-step, steps 16..63 show shade zero, and step 64 completes the
    /// fade. All 65 steps match the pinned release.
    /// No step follows the completing one, so a later step is refused.
    /// The eye-fade verification checks every shade and the completing
    /// step through the fade that consumes it.
    /// </remarks>
    public static CeresRidleyEyeFadeStep Get(int offset) => offset switch
    {
        < 0 => throw new InvalidDataException($"Ceres Ridley eye-fade offset {offset} precedes the authored schedule."),
        < 16 => new(false, 15 - offset),
        < PaletteStepCount => new(false, 0),
        PaletteStepCount => new(true, 0),
        _ => throw new InvalidDataException(
            $"Ceres Ridley eye-fade offset {offset} exceeds the authored " +
            $"{PaletteStepCount + 1}-entry schedule."),
    };
}
