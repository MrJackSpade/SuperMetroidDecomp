namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Compiled physical patrol definitions for the Cacatac enemy family.</summary>
internal static class CacatacMovementDefinitions
{
    /// <summary>
    /// In pixels: the short patrol selector travels one
    /// block; the five wider patrol selectors travel four through eight blocks.
    /// The initializer adds and subtracts this distance from the spawn position, wrapping at the room coordinate
    /// range as every room position does. Selector six has no authored distance and is rejected.
    /// </summary>
    internal static readonly int[] TravelDistances = [16, 64, 80, 96, 112, 128];
}
