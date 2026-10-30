namespace SuperMetroid.Core.Enemies;

/// <summary>The shape of the shared quadratic enemy speed table, in which speed grows with the triangle numbers.</summary>
/// <remarks>
/// Reviewed for #1165 against both NTSC J/U v1.0 copies of the table. Row <c>n</c>'s speed is the triangle number
/// <c>n(n+1)/2</c> in 256ths of a pixel per frame, but the authored fractions are widened by
/// <see cref="FractionMultiplier"/>/256 with the carry past the fraction dropped, so speeds do not climb evenly.
/// </remarks>
public static class EnemyQuadraticSpeedDefinitions
{
    /// <summary>The number of rows, zero through 94.</summary>
    public const int RowCount = 95;

    /// <summary>The authored widening of each row's fraction, in 256ths.</summary>
    internal const int FractionMultiplier = 265;
}
