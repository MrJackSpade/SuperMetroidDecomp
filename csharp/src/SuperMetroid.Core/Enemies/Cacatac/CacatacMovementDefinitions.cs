namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled physical patrol definitions for the Cacatac enemy family.</summary>
internal static class CacatacMovementDefinitions
{
    /// <summary>
    /// $A2:9F36 CacatacMaxTravelDistances, in pixels: the short patrol selector travels one
    /// block; the five wider patrol selectors travel four through eight blocks.
    /// The initializer adds/subtracts this pixel distance with 16-bit wrapping.
    /// Selector six would reach the adjacent function pointer and stays rejected.
    /// </summary>
    internal static readonly ushort[] TravelDistances = [16, 64, 80, 96, 112, 128];
}
