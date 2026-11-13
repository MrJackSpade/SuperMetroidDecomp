namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The whole pixel at which Samus's probe for solid enemies tests her reached centre (#1621): her centre moved by the
/// probe's reach, then one pixel further along the probe when the reached point lies inside a pixel or the reach's
/// fraction carried it across a pixel's edge, except that a reach whose fraction carried it across onto pixel 0 stays on
/// pixel 0. A probe whose fraction crosses a pixel's edge therefore tests two pixels beyond the whole pixels it reaches,
/// which lands a falling Samus on a frozen enemy one pixel further down than her true reach, and a probe that crosses onto
/// the room's first pixel row or column, from either side, tests that row or column itself.
/// </summary>
/// <remarks>
/// Consulted once per directional solid-enemy probe, before any enemy is tested, in every horizontal and vertical move
/// that tests solid enemies. Evidence: the original probe forms the reached centre with a subtract or add that borrows or
/// carries from the fraction into the whole pixel, and then steps the whole pixel once more, outward, unless the
/// decrement or increment that took the borrow or carry left the whole pixel at zero; the solid-enemy collision check
/// recovers the tested pixel from collision outcomes for every direction, an exact carry, and both room edges, and the
/// 100% movie's landing on a frozen Rinka in Mother Brain's room needs the doubled step.
/// </remarks>
[CompatibilityService("Samus's solid-enemy probe tests one pixel past her reach inside a pixel, two past it when the reach's fraction crosses a pixel's edge, but pixel 0 itself when the crossing lands there")]
public static class SolidEnemyProbeReach
{
    /// <summary>The steps of a pixel's fraction: a fraction sum that reaches them crosses into the next pixel.</summary>
    private const int FractionSteps = 65536;

    /// <summary>
    /// The pixel tested when Samus's centre at <paramref name="centre"/> reaches <paramref name="reach"/> towards larger
    /// coordinates (right or down), 0 through 65535.
    /// </summary>
    /// <param name="centre">Samus's centre on the probe's axis.</param>
    /// <param name="reach">How far the probe reaches.</param>
    public static int TestedPixelForward(FixedCoordinate centre, UnsignedSixteenSixteen reach)
    {
        FixedCoordinate reached = centre.MovedForward(reach);
        bool crossedPixelEdge = centre.Fraction + reach.Fraction >= FractionSteps;
        return Stepped(reached, crossedPixelEdge, step: 1);
    }

    /// <summary>
    /// The pixel tested when Samus's centre at <paramref name="centre"/> reaches <paramref name="reach"/> towards smaller
    /// coordinates (left or up), 0 through 65535.
    /// </summary>
    /// <param name="centre">Samus's centre on the probe's axis.</param>
    /// <param name="reach">How far the probe reaches.</param>
    public static int TestedPixelBack(FixedCoordinate centre, UnsignedSixteenSixteen reach)
    {
        FixedCoordinate reached = centre.MovedBack(reach);
        bool crossedPixelEdge = centre.Fraction < reach.Fraction;
        return Stepped(reached, crossedPixelEdge, step: -1);
    }

    /// <summary>
    /// The reached point's pixel, stepped once by <paramref name="step"/> (wrapping past either end of the room's
    /// coordinates) when the point lies inside a pixel or its fraction crossed a pixel's edge, unless a crossing landed on
    /// pixel 0.
    /// </summary>
    private static int Stepped(FixedCoordinate reached, bool crossedPixelEdge, int step)
    {
        if (crossedPixelEdge && reached.Whole == 0)
            return 0;
        if (!crossedPixelEdge && reached.Fraction == 0)
            return reached.Whole;
        return CoordinateAxis.Wrap(reached.Whole + step);
    }
}
