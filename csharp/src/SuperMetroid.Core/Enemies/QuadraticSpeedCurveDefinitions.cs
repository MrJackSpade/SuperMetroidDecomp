namespace SuperMetroid.Core.Enemies;

/// <summary>The shape of the shared quadratic enemy speed curve, along which speed grows with the triangle numbers.</summary>
/// <remarks>
/// Reviewed for #1165 against both of the Japanese and North American v1.0 release's copies of the curve. Mark <c>n</c>'s speed is the triangle number
/// <c>n(n+1)/2</c> in 256ths of a pixel per frame, but the authored fractions are widened by
/// <see cref="FractionMultiplier"/>/256 with the whole pixels the widening adds dropped, so speeds do not climb evenly.
/// </remarks>
public static class QuadraticSpeedCurveDefinitions
{
    /// <summary>The number of marks along the curve, its start through its last mark.</summary>
    public const int MarkCount = 95;

    /// <summary>The authored widening of each mark's fraction, in 256ths.</summary>
    internal const int FractionMultiplier = 265;
}
