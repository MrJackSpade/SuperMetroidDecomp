using static SuperMetroid.Core.Fx.FirefleaFxDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room's shade levels (<see cref="FirefleaFxDefinitions"/>): the triangular flashing wave and the capped darkness
/// ramp enemy deaths deepen.
/// </summary>
internal static class FirefleaShades
{
    /// <summary>Returns the triangular flashing shade level 6-abs(index-6), index 0..11.</summary>
    /// <remarks>Independently checked for #1165 against every original Japanese and North American v1.0
    /// value and the pinned reference sources. The six-frame timer and phase wrap are caller-owned;
    /// this exact integer wave requires no sampled floating-point construction.</remarks>
    internal static int FlashingShade(int index)
    {
        if (index is < 0 or >= FlashCount)
        {
            throw new InvalidDataException(
                $"Fireflea flashing index {index} is outside the {FlashCount}-entry flash cycle.");
        }

        return 6 - Math.Abs(index - 6);
    }

    /// <summary>Returns the darkness fade level darkness step <paramref name="step"/> shows (<see cref="FirefleaFxDefinitions.DarknessLevels"/>).</summary>
    /// <remarks>Independently checked for #1165 against all seven original shades and the pinned reference sources.
    /// Every level plus the flashing shade stays within the fade scale.</remarks>
    internal static int DarknessShade(int step)
    {
        if (step < 0 || step >= DarknessLevels.Length)
        {
            throw new InvalidDataException(
                $"Fireflea darkness step {step} is outside the {DarknessLevels.Length} darkness steps.");
        }

        return DarknessLevels[step];
    }
}
