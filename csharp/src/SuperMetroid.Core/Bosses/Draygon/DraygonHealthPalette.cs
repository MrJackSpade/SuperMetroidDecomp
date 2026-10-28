using static SuperMetroid.Core.Bosses.Draygon.DraygonHealthPaletteDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Selects Draygon's health palette band from its health.</summary>
internal static class DraygonHealthPalette
{
    /// <summary>
    /// Returns the native byte index $00,$02,...,$0E into the health palette bands.
    /// Values above Draygon's authored maximum are corrupt restored state, not permission
    /// to continue through the adjacent $FFFF terminator and executable bank-$A5 data.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A5.asm:
    /// $A5:96EF has threshold(i)=750*(7-i), i=0..7; $A5:9701 selects the first
    /// nonnegative signed difference. For health 0..6000 no difference overflows,
    /// so integer division gives the band max(0,7-floor(health/750)) (native byte index twice that). Exact multiples stay
    /// in the higher band. The max describes the top band, not input clamping.
    /// The unreachable $FFFF terminator is not a ninth threshold.
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
