namespace SuperMetroid.Core.Enemies;

/// <summary>The shape of the shared quadratic enemy speed curve, along which speed grows with the triangle numbers.</summary>
/// <remarks>
/// Both of the game's copies of the curve agree. Mark <c>n</c>'s speed is the triangle number
/// <c>n(n+1)/2</c> in 256ths of a pixel per frame, but the authored fractions are widened by
/// <see cref="FractionMultiplier"/>/256 with the whole pixels the widening adds dropped, so speeds do not climb evenly.
/// </remarks>
public static class QuadraticSpeedCurveDefinitions
{
    /// <summary>The number of marks along the curve, its start through its last mark.</summary>
    public const int MarkCount = 95;

    /// <summary>The authored widening of each mark's fraction, in 256ths.</summary>
    internal const int FractionMultiplier = 265;

    /// <summary>
    /// The highest climb, in whole pixels, a hop's starting place is found for (<see cref="QuadraticCurveClimb"/>): up to
    /// it the game's height test agrees with a plain sum and comparison, because the sum passes the height by less than
    /// the curve's fastest coarse speed, under eighteen pixels, and so never reaches the 128 pixels from which the game's
    /// test would read it as short of the height again. The hops climb 16 to 64 pixels.
    /// </summary>
    internal const int LargestClimbHeight = 96;
}
