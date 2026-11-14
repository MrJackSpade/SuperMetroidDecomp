using static SuperMetroid.Core.Enemies.QuadraticSpeedCurveDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>The shared quadratic enemy speed curve, whose speeds grow with the triangle numbers.</summary>
public static class QuadraticSpeedCurve
{
    /// <summary>Every mark's speeds, in order along the curve.</summary>
    private static readonly QuadraticCurveSpeed[] Marks = [.. Enumerable.Range(0, MarkCount).Select(Build)];

    /// <summary>The speeds at mark <paramref name="mark"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="mark"/> is not one of the authored marks.</exception>
    public static QuadraticCurveSpeed AtMark(int mark) => (uint)mark < (uint)Marks.Length
        ? Marks[mark]
        : throw new InvalidDataException($"Quadratic speed curve mark {mark} is outside the {MarkCount} authored marks.");

    /// <summary>
    /// The speeds at mark <paramref name="mark"/>: the triangle number in 256ths of a pixel, its fraction widened by
    /// <see cref="FractionMultiplier"/> with the whole pixels the widening adds dropped, and that speed negated.
    /// </summary>
    private static QuadraticCurveSpeed Build(int mark)
    {
        int triangle = mark * (mark + 1) / 2;
        SignedSixteenSixteen speed = SignedSixteenSixteen.FromParts(
            EightEight.WholeOf(triangle),
            SixteenSixteen.FractionOf(EightEight.FractionOf(triangle) * FractionMultiplier));
        return new QuadraticCurveSpeed(speed, -speed);
    }
}
