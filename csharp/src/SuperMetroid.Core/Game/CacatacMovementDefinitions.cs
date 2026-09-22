namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical patrol definitions for the Cacatac enemy family.</summary>
internal static class CacatacMovementDefinitions
{
    /// <summary>
    /// Cacatac travel distances at $A2:9F36: six word values selected by the
    /// low byte of population parameter two. The initializer adds and subtracts
    /// the selected distance from the spawn X coordinate with 16-bit wrapping.
    /// </summary>
    /// <remarks>
    /// #625 / #646 retained table: all six unsigned words at $A2:9F36-$9F41
    /// match the pinned NTSC J/U v1.0 ROM and bank-A2 disassembly. Indices 1..5
    /// fit 16*(index+3), but index 0 is 16; this piecewise fit is less clear than
    /// the six authored patrol widths. The native initializer doubles the low-byte
    /// selector, then reads the word twice. Index 6 would select the adjacent
    /// $9FBA function pointer and is rejected. VerifyCacatacMovementDefinitions
    /// checks all entries through wrapped production initializers and both invalid
    /// selector boundaries with the original ROM range forbidden.
    /// </remarks>
    private static ReadOnlySpan<ushort> TravelDistances =>
    [
        16, 64, 80, 96, 112, 128,
    ];

    internal static ushort TravelDistance(byte index)
    {
        if (index >= TravelDistances.Length)
        {
            throw new InvalidDataException(
                $"Cacatac travel-distance index {index} is outside the {TravelDistances.Length}-entry native table.");
        }

        return TravelDistances[index];
    }
}
