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
    /// Independently reviewed for #1165 against the Japanese and North American v1.0 release:
    /// threshold(i)=750*(7-i), i=0..7; the band is the first
    /// nonnegative signed difference. For health 0..6000 no difference overflows,
    /// so integer division gives the band max(0,7-floor(health/750)). Exact multiples stay
    /// in the higher band. The max describes the top band, not input clamping.
    /// There is no ninth threshold.
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
