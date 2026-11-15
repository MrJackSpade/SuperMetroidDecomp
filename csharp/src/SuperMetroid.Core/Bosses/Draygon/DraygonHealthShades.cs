using static SuperMetroid.Core.Bosses.Draygon.DraygonHealthShadeDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Selects Draygon's health shade band from his health.</summary>
internal static class DraygonHealthShades
{
    /// <summary>
    /// Returns the health shade band, 0 through 7.
    /// Values above Draygon's authored maximum are corrupt restored state, not permission
    /// to select a band past the authored ones.
    /// </summary>
    /// <remarks>
    /// Each band covers 750 health, the highest band from 5250 up: the band is seven less the whole 750s of health, and
    /// never below zero. Health exactly on a boundary stays in the higher band.
    /// </remarks>
    public static int BandForHealth(int health)
    {
        if (health > MaximumAuthoredHealth)
        {
            throw new InvalidDataException(
                $"Draygon health {health} exceeds the authored maximum " +
                $"{MaximumAuthoredHealth} for health-band selection.");
        }

        return Math.Max(0, LowestHealthBand - health / HealthPerBand);
    }
}
