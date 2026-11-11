namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Sbug's zigzag cadence and the division of the turn into its eight facings.</summary>
internal static class SbugDefinitions
{
    /// <summary>
    /// The phase of the enemy's own update count that picks its zigzag leg: clear for sixteen updates, on the leg that
    /// keeps its left facing, then set for sixteen on the other, alternating as the count advances.
    /// </summary>
    internal static readonly UpdateCounterField ZigZagSecondLeg = new(4, 1);

    /// <summary>
    /// The heading, in 256ths of a turn, at which the first facing's eighth of the turn begins; each later facing's
    /// eighth follows counter-clockwise.
    /// </summary>
    internal const int FirstFacingStart = 48;

    /// <summary>Heading table units covered by one of the eight facings.</summary>
    internal const int StepsPerFacing = 32;
}
