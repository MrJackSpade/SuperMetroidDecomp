namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical patrol definitions for the Cacatac enemy family.</summary>
internal static class CacatacMovementDefinitions
{
    /// <summary>
    /// Cacatac travel distances at $A2:9F36: six word values selected by the
    /// low byte of population parameter two. The initializer adds and subtracts
    /// the selected distance from the spawn X coordinate with 16-bit wrapping.
    /// </summary>
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
