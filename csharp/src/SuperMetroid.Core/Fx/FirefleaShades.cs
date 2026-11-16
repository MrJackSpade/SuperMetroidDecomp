using static SuperMetroid.Core.Fx.FirefleaFxDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room's shade levels (<see cref="FirefleaFxDefinitions"/>): the triangular flashing wave and the capped darkness
/// ramp enemy deaths deepen.
/// </summary>
internal static class FirefleaShades
{
    /// <summary>
    /// Returns the triangular flashing shade level at flash phase <paramref name="phase"/>, 0 through 11: six less the
    /// phase's distance from six.
    /// </summary>
    /// <remarks>The six-frame timer and the phase's wrap are the Fireflea room's own.</remarks>
    internal static int FlashingShade(int phase)
    {
        if (phase is < 0 or >= FlashCount)
        {
            throw new InvalidDataException(
                $"Fireflea flash phase {phase} is outside the {FlashCount}-phase flash cycle.");
        }

        return 6 - Math.Abs(phase - 6);
    }

    /// <summary>Returns the darkness fade level darkness step <paramref name="step"/> shows (<see cref="FirefleaFxDefinitions.DarknessLevels"/>).</summary>
    /// <remarks>Every level plus the flashing shade stays within the fade scale.</remarks>
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
