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

    /// <summary>$A5:96EF, DraygonHealthBasedPaletteThresholds, eight reachable thresholds.</summary>
    /// <remarks>
    /// Issue #625 exact arithmetic: threshold(i)=750*(7-i), i=0..7. The whole
    /// search can also be replaced by byteIndex(h)=2*max(0,7-floor(h/750)), after
    /// rejecting h outside 0..6000. The max handles the top health band, not invalid
    /// inputs. LookupTableResearch verifies every threshold against the NTSC ROM
    /// and pinned bank_A5.asm, and both the formula and current caller for ALL
    /// 6,001 valid health values. Exact multiples of 750 stay in the higher band.
    /// The adjacent $FFFF terminator is not another threshold or formula input.
    /// Integer division fully explains the table; no rounding exceptions are needed.
    /// </remarks>
    private static ReadOnlySpan<ushort> Thresholds =>
        [5250, 4500, 3750, 3000, 2250, 1500, 750, 0];

    /// <summary>
    /// Returns the native byte index $00,$02,...,$0E into the health palette bands.
    /// Values above Draygon's authored maximum are corrupt restored state, not permission
    /// to continue through the adjacent $FFFF terminator and executable bank-$A5 data.
    /// </summary>
    public static ushort ByteIndexForHealth(ushort health)
    {
        if (health > MaximumAuthoredHealth)
        {
            throw new InvalidDataException(
                $"Draygon health {health} exceeds the authored maximum " +
                $"{MaximumAuthoredHealth} for palette-band selection.");
        }

        for (int index = 0; index < Thresholds.Length; index++)
        {
            if (health >= Thresholds[index])
                return unchecked((ushort)(index * sizeof(ushort)));
        }

        throw new InvalidOperationException(
            "The zero health threshold must match every authored Draygon health value.");
    }
}
