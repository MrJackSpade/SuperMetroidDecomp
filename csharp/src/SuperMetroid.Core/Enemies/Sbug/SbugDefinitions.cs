namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's zigzag cadence.</summary>
internal static class SbugDefinitions
{
    /// <summary>
    /// The phase of the enemy's own update count that picks its zigzag leg: clear for sixteen updates, on the leg that
    /// keeps its left facing, then set for sixteen on the other, alternating as the count advances.
    /// </summary>
    internal static readonly UpdateCounterField ZigZagSecondLeg = new(4, 1);
}
