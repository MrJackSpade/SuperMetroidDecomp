namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A fraction whose denominator is a power of two, counted exactly in a measure's finest units (#2555): the game's speeds,
/// distances, turns and zooms are authored as such fractions (three eighths of a pixel, a step and a half, thirteen
/// thirty-seconds of a pixel), and each measure keeps them without rounding because its finest unit is itself such a
/// fraction of the whole.
/// </summary>
internal static partial class PowerOfTwoFraction
{
    /// <summary>
    /// The count of finest units in <paramref name="numerator"/> <paramref name="denominator"/>ths of a whole, the
    /// denominator a power of two from 1 through <paramref name="finestUnitsPerWhole"/> so the count is exact.
    /// </summary>
    /// <param name="numerator">How many of the denominator's parts, negative for a measure that points backward.</param>
    /// <param name="denominator">The parts a whole divides into: a power of two no larger than the finest units.</param>
    /// <param name="finestUnitsPerWhole">The measure's finest units in one whole, a power of two.</param>
    /// <exception cref="ArgumentOutOfRangeException">The denominator is no power of two within the measure's precision.</exception>
    /// <exception cref="OverflowException">The count lies outside an <see cref="int"/>.</exception>
    internal static int InFinestUnits(int numerator, int denominator, int finestUnitsPerWhole)
    {
        int unitsEach = finestUnitsPerWhole / Ensure.BetweenInclusive(1, finestUnitsPerWhole, denominator);
        if (unitsEach * denominator != finestUnitsPerWhole)
            throw new ArgumentOutOfRangeException(nameof(denominator), denominator,
                $"The denominator must be a power of two from 1 through {finestUnitsPerWhole}.");
        return checked(numerator * unitsEach);
    }
}
