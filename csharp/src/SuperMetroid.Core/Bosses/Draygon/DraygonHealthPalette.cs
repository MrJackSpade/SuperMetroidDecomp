using static SuperMetroid.Core.Bosses.Draygon.DraygonHealthPaletteDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Selects Draygon's health palette band from its health.</summary>
internal static class DraygonHealthPalette
{
    /// <summary>
    /// Returns the health palette band, 0 through 7.
    /// Values above Draygon's authored maximum are corrupt restored state, not permission
    /// to select a band past the authored ones.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the NTSC J/U v1.0 release:
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
                $"{MaximumAuthoredHealth} for palette-band selection.");
        }

        return Math.Max(0, LowestHealthBand - health / HealthPerBand);
    }
}
