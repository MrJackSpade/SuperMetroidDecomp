namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled health-band policy for Draygon's palette handler. The selected color records
/// remain presentation data; this catalog owns only the fixed health-to-record algorithm.
/// </summary>
internal static class DraygonHealthPaletteDefinitions
{
    /// <summary>
    /// <c>DraygonHealthBasedPaletteThresholds</c> at $A5:96EF. Eight reachable words are
    /// followed by an unreachable $FFFF terminator at $A5:96FF.
    /// </summary>
    public const int NativeThresholdAddress = 0xa596ef;

    /// <summary>Draygon body's authored enemy-header health at $A0:DE43.</summary>
    public const ushort MaximumAuthoredHealth = 6000;

    /// <summary>
    /// Returns the native byte index $00,$02,...,$0E into the health palette bands.
    /// Values above Draygon's authored maximum are corrupt restored state, not permission
    /// to continue through the adjacent $FFFF terminator and executable bank-$A5 data.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A5.asm:
    /// $A5:96EF has threshold(i)=750*(7-i), i=0..7; $A5:9701 selects the first
    /// nonnegative signed difference. For health 0..6000 no difference overflows,
    /// so integer division gives 2*max(0,7-floor(health/750)). Exact multiples stay
    /// in the higher band. The max describes the top band, not input clamping.
    /// The unreachable $FFFF terminator is not a ninth threshold.
    /// </remarks>
    public static ushort ByteIndexForHealth(ushort health)
    {
        if (health > MaximumAuthoredHealth)
        {
            throw new InvalidDataException(
                $"Draygon health {health} exceeds the authored maximum " +
                $"{MaximumAuthoredHealth} for palette-band selection.");
        }

        return (ushort)(2 * Math.Max(0, 7 - health / 750));
    }
}
