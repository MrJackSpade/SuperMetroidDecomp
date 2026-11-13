using static SuperMetroid.Core.Samus.Abilities.CrystalFlashPaletteTimingDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>How long each step of the Crystal Flash body's colour pulse shows.</summary>
internal static class CrystalFlashPaletteTiming
{
    /// <summary>The colouring updates step <paramref name="step"/> of the body's pulse shows for.</summary>
    /// <remarks>Every one of the ten steps shows for ten calls, so a whole pulse is a hundred calls. Investigation: #625 / #668.</remarks>
    /// <exception cref="InvalidDataException">The step is not one of the pulse's.</exception>
    public static int DurationForStep(int step) =>
        step is >= 0 and < RecordCount
            ? AuthoredDuration
            : throw new InvalidDataException($"Crystal Flash body step {step} is not one of the pulse's {RecordCount}.");
}
