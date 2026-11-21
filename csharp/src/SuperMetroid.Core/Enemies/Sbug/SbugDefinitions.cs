namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's zigzag cadence.</summary>
internal static class SbugDefinitions
{
    /// <summary>
    /// The phase of the enemy's own update count that picks its zigzag leg: the first sixteen updates, on the leg that
    /// keeps its left facing, then sixteen on the other, alternating as the count advances.
    /// </summary>
    internal static readonly UpdateAlternation ZigZagSecondLeg = new(16);
}
