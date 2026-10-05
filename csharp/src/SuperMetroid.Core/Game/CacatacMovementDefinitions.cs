namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical patrol definitions for the Cacatac enemy family.</summary>
internal static class CacatacMovementDefinitions
{
    /// <summary>
    /// $A2:9F36 CacatacMaxTravelDistances: the short patrol selector travels one
    /// block; the five wider patrol selectors travel four through eight blocks.
    /// The initializer adds/subtracts this pixel distance with 16-bit wrapping.
    /// Selector six would reach the adjacent function pointer and stays rejected.
    /// </summary>
    internal static ushort TravelDistance(byte index)
    {
        if (index >= 6)
            throw new InvalidDataException(
                $"Cacatac travel-distance index {index} is outside the 6-entry native table.");
        return (ushort)(16 * (index == 0 ? 1 : index + 3));
    }
}
