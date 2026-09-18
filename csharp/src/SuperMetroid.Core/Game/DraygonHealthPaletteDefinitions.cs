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
