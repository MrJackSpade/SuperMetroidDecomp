namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A place along the shared quadratic speed curve (<see cref="QuadraticSpeedCurve"/>), counted in 256ths of a mark: its
/// whole marks select the mark whose speed applies, and its fraction carries a slower climb or fall toward the next mark.
/// The enemies that accelerate along the curve (the boulders, the Ki-Hunter's detached wing, the large Maridia snail,
/// Botwoon's falling head, the Puyo, and the whole-mark movers: the Beetom's, Choot's and hoppers' hops, the platforms,
/// the yellow Pipe Bug's arc, the Polyp's rock and the Yapping Maw's curl) step such a place each update instead of a
/// speed; a step is a place's difference in the same 256ths. Places only grow, shrink and compare; nothing wraps, because
/// each mover caps or floors its place well inside the curve, and a place past the curve's last mark or before its start
/// fails when its speed is read.
/// </summary>
public readonly record struct QuadraticSpeedCurvePlace : IComparable<QuadraticSpeedCurvePlace>
{
    /// <summary>Keeps the place's reading in 256ths of a mark.</summary>
    private QuadraticSpeedCurvePlace(int twoHundredFiftySixthsOfMark) => InTwoHundredFiftySixthsOfMark = twoHundredFiftySixthsOfMark;

    /// <summary>The place in 256ths of a mark, below zero only for a mover that keeps a place it took before the start.</summary>
    [IntegerQuantity("place along the quadratic speed curve in 256ths of a mark")]
    public int InTwoHundredFiftySixthsOfMark { get; }

    /// <summary>The curve's start, its first mark with no fraction: the place an accelerating mover starts from.</summary>
    public static QuadraticSpeedCurvePlace Start => default;

    /// <summary>The step of one whole mark, by which the whole-mark movers move along the curve each update.</summary>
    public static QuadraticSpeedCurvePlace OneMark => FromMarks(1);

    /// <summary>The curve's last mark, the furthest place whose speed can be read.</summary>
    public static QuadraticSpeedCurvePlace LastMark => FromMarks(QuadraticSpeedCurveDefinitions.MarkCount - 1);

    /// <summary>The place one mark past <see cref="LastMark"/>, where the curve has no speed.</summary>
    public static QuadraticSpeedCurvePlace PastLastMark => FromMarks(QuadraticSpeedCurveDefinitions.MarkCount);

    /// <summary>The place <paramref name="marks"/> whole marks along the curve, or the step of that many marks.</summary>
    public static QuadraticSpeedCurvePlace FromMarks(int marks) => new(EightEight.FromWhole(marks));

    /// <summary>The place <paramref name="twoHundredFiftySixths"/> 256ths of a mark along the curve, or the step of that many.</summary>
    public static QuadraticSpeedCurvePlace FromTwoHundredFiftySixthsOfMark(int twoHundredFiftySixths) => new(twoHundredFiftySixths);

    /// <summary>The whole marks of the place, its fraction dropped toward negative infinity.</summary>
    [IntegerQuantity("whole marks of a quadratic speed curve place")]
    public int WholeMarks => EightEight.WholeOf(InTwoHundredFiftySixthsOfMark);

    /// <summary>The speed that applies at this place: the speed of its whole marks.</summary>
    /// <exception cref="InvalidDataException">The place lies past the curve's last mark or before its start.</exception>
    public QuadraticCurveSpeed Speed => QuadraticSpeedCurve.AtMark(WholeMarks);

    /// <summary>Whether the place lies before the curve's start.</summary>
    public bool IsNegative => InTwoHundredFiftySixthsOfMark < 0;

    /// <summary>The place moved on by <paramref name="step"/>.</summary>
    public static QuadraticSpeedCurvePlace operator +(QuadraticSpeedCurvePlace place, QuadraticSpeedCurvePlace step) =>
        new(checked(place.InTwoHundredFiftySixthsOfMark + step.InTwoHundredFiftySixthsOfMark));

    /// <summary>The place moved back by <paramref name="step"/>.</summary>
    public static QuadraticSpeedCurvePlace operator -(QuadraticSpeedCurvePlace place, QuadraticSpeedCurvePlace step) =>
        new(checked(place.InTwoHundredFiftySixthsOfMark - step.InTwoHundredFiftySixthsOfMark));

    /// <summary>The earlier of two places.</summary>
    public static QuadraticSpeedCurvePlace Min(QuadraticSpeedCurvePlace left, QuadraticSpeedCurvePlace right) =>
        left <= right ? left : right;

    /// <summary>Orders places by how far along the curve they lie.</summary>
    public int CompareTo(QuadraticSpeedCurvePlace other) => InTwoHundredFiftySixthsOfMark.CompareTo(other.InTwoHundredFiftySixthsOfMark);

    /// <summary>Whether <paramref name="left"/> lies earlier along the curve.</summary>
    public static bool operator <(QuadraticSpeedCurvePlace left, QuadraticSpeedCurvePlace right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies further along the curve.</summary>
    public static bool operator >(QuadraticSpeedCurvePlace left, QuadraticSpeedCurvePlace right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> lies no further along the curve.</summary>
    public static bool operator <=(QuadraticSpeedCurvePlace left, QuadraticSpeedCurvePlace right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> lies no earlier along the curve.</summary>
    public static bool operator >=(QuadraticSpeedCurvePlace left, QuadraticSpeedCurvePlace right) => left.CompareTo(right) >= 0;
}
