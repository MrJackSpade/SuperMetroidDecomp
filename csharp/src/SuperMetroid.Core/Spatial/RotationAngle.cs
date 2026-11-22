namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A rotation angle that wraps at a full turn, measured in steps of a 256th of a turn, each with a 256th-of-a-step
/// fraction. Callers speak in angles: the named turns, angle arithmetic and comparison, the sector a heading lies in, and
/// the trigonometry angle (<see cref="ToTrigonometryAngle"/>) the sine and cosine lookups read, the angle's whole steps. Keeping
/// the fraction lets grapple and neck motion turn at sub-step precision while whole-step callers share the same turns.
/// </summary>
public readonly partial record struct RotationAngle : IComparable<RotationAngle>, System.Numerics.IUnaryNegationOperators<RotationAngle, RotationAngle>
{
    /// <summary>Number of whole steps in one complete turn.</summary>
    public const int StepsPerTurn = 256;

    /// <summary>Number of fractions in one step.</summary>
    internal const int FractionsPerStep = 256;

    /// <summary>Holds the angle in 256ths of a step, 0 through 65535: whole steps and their fraction.</summary>
    internal RotationAngle(ushort rawValue) => RawValue = rawValue;

    /// <summary>The angle in 256ths of a step, 0 through 65535; a full turn wraps to zero.</summary>
    internal ushort RawValue { get; }

    /// <summary>The whole steps of the angle, 0 through 255.</summary>
    private int Steps => RawValue / FractionsPerStep;

    /// <summary>The angle as the trigonometry angle the sine and cosine lookups read: its whole steps.</summary>
    public TrigonometryAngle ToTrigonometryAngle() => new(Steps);

    /// <summary>The whole step the angle lies in: the angle with its fraction dropped.</summary>
    public RotationAngle WholeStep => FromSteps(Steps);

    /// <summary>The angle in radians, fraction included: a half turn is pi, and a quarter turn is pi / 2.</summary>
    public double Radians => RawValue * Math.PI / HalfTurn.RawValue;

    /// <summary>Zero turns: the sine wave's zero angle.</summary>
    public static RotationAngle Zero { get; } = FromSteps(0);

    /// <summary>The finest turn past none: a 256th of a step (#2555).</summary>
    public static RotationAngle FinestTurn { get; } = new(1);

    /// <summary>One eighth of a turn on from <see cref="Zero"/>: the turn between neighbouring compass directions.</summary>
    public static RotationAngle EighthTurn { get; } = FromSteps(StepsPerTurn / 8);

    /// <summary>One quarter turn on from <see cref="Zero"/>: where the sine wave peaks.</summary>
    public static RotationAngle QuarterTurn { get; } = FromSteps(StepsPerTurn / 4);

    /// <summary>One half turn on from <see cref="Zero"/>: the opposite direction.</summary>
    public static RotationAngle HalfTurn { get; } = FromSteps(StepsPerTurn / 2);

    /// <summary>Three quarters of a turn on from <see cref="Zero"/>: where the sine wave is lowest.</summary>
    public static RotationAngle ThreeQuarterTurn { get; } = FromSteps(3 * StepsPerTurn / 4);

    /// <summary>The angle of <paramref name="steps"/> whole steps, 0 through 255, with no fraction.</summary>
    public static RotationAngle FromSteps(int steps) => FromSteps(steps, 0);

    /// <summary>The angle of <paramref name="steps"/> whole steps and <paramref name="fraction"/> 256ths of a step, each 0 through 255.</summary>
    internal static RotationAngle FromSteps(int steps, int fraction) =>
        new(checked((ushort)(Ensure.BetweenInclusive(0, StepsPerTurn - 1, steps) * FractionsPerStep +
            Ensure.BetweenInclusive(0, FractionsPerStep - 1, fraction))));

    /// <summary>
    /// The angle <paramref name="numerator"/> <paramref name="denominator"/>ths of a step round from zero (#2555), the
    /// denominator a power of two from 1 through 256 so the fraction is kept exactly: an eighth of a step, a step and a half.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The denominator is no power of two from 1 through 256, or the angle lies outside one turn.
    /// </exception>
    public static RotationAngle FromStepFraction(int numerator, int denominator) =>
        new(checked((ushort)Ensure.BetweenInclusive(0, StepsPerTurn * FractionsPerStep - 1,
            PowerOfTwoFraction.InFinestUnits(numerator, denominator, FractionsPerStep))));

    /// <summary>The angle halfway through whole step <paramref name="step"/>: the step with half a step's fraction.</summary>
    public static RotationAngle MiddleOfStep(TrigonometryAngle step) => FromSteps(step.RawValue, FractionsPerStep / 2);

    /// <summary>The angle a computation in 256ths of a step lands on: past a full turn, or below zero, it continues around the circle.</summary>
    public static RotationAngle NormalizeRaw(int rawValue) => new(unchecked((ushort)rawValue));

    /// <summary>The angle a computation in whole steps lands on, continuing around the circle past a full turn or below zero, with no fraction.</summary>
    public static RotationAngle WrappedSteps(int steps) => NormalizeRaw(steps * FractionsPerStep);

    /// <summary>Turns the angle by <paramref name="delta"/> 256ths of a step, continuing around the circle.</summary>
    internal RotationAngle AddRaw(int delta) => NormalizeRaw(RawValue + delta);

    /// <summary>
    /// Turns the angle by <paramref name="fractionsOfStep"/> 256ths of a step, back when negative, continuing around the
    /// circle: the sub-step precision the game's turn rates are authored in.
    /// </summary>
    internal RotationAngle TurnedByFractionsOfStep(int fractionsOfStep) => NormalizeRaw(RawValue + fractionsOfStep);

    /// <summary>
    /// The angle turned by <paramref name="speed"/>'s whole steps alone (#2555), keeping its own fraction, as a turn that
    /// steps an angle by whole steps leaves the speed's fraction to accumulate in the speed.
    /// </summary>
    public RotationAngle TurnedByWholeStepsOf(AngularSpeed speed) => TurnedBySteps(speed.WholeSteps);

    /// <summary>
    /// The angle turned on by <paramref name="turn"/> <paramref name="times"/> times (#2555), continuing around the circle
    /// past a full turn.
    /// </summary>
    /// <param name="turn">The turn each time takes.</param>
    /// <param name="times">How many times, zero or more.</param>
    public RotationAngle TurnedBy(RotationAngle turn, int times) => AddRaw(checked(turn.RawValue * Ensure.AtLeastZero(times)));

    /// <summary>
    /// The angle with its whole steps replaced by <paramref name="other"/>'s and its own fraction of a step kept (#2555),
    /// as a beam held at its full width keeps the part of a step its widening had reached.
    /// </summary>
    public RotationAngle WithWholeStepsOf(RotationAngle other) => new((ushort)(other.Steps * FractionsPerStep + RawValue % FractionsPerStep));

    /// <summary>Turns the angle by <paramref name="steps"/> whole steps, keeping its fraction, continuing around the circle.</summary>
    public RotationAngle TurnedBySteps(int steps) => AddRaw(steps * FractionsPerStep);

    /// <summary>
    /// The angle turned one update's worth at <paramref name="speed"/>, its fraction carrying into its whole steps and the
    /// result going on round the turn past either end.
    /// </summary>
    public RotationAngle TurnedBy(AngularSpeed speed) => TurnedByFractionsOfStep(speed.FractionsOfStep);

    /// <summary>
    /// The turn from <paramref name="origin"/> to this angle the shorter way round, in 256ths of a step, -32768 through
    /// 32767: negative when this angle lies behind the origin, as a heading compares itself with its target.
    /// </summary>
    public int TurnFrom(RotationAngle origin) => CoordinateAxis.Difference(RawValue, origin.RawValue);

    /// <summary>
    /// Which of <paramref name="sectors"/> equal arcs of the turn, numbered from zero at <see cref="Zero"/>, holds the
    /// angle's whole step (#1461): for a power of two, the step's top bits, which a pose or heading choice reads.
    /// </summary>
    /// <param name="sectors">The number of arcs the turn divides into, 1 through <see cref="StepsPerTurn"/>.</param>
    public int Sector(int sectors) =>
        Steps * Ensure.BetweenInclusive(1, StepsPerTurn, sectors) / StepsPerTurn;

    /// <summary>
    /// The turn split into <paramref name="parts"/> equal arcs: one arc, rounded back to a whole 256th of a step, as a
    /// turn made a share at a time, or the point halfway along an arc, is formed.
    /// </summary>
    /// <param name="parts">The number of equal arcs, at least one.</param>
    public RotationAngle DividedBy(int parts) => new((ushort)(RawValue / Ensure.BetweenInclusive(1, int.MaxValue, parts)));

    /// <summary>The angle turned on by <paramref name="turn"/>, continuing around the circle past a full turn.</summary>
    public static RotationAngle operator +(RotationAngle angle, RotationAngle turn) => angle.AddRaw(turn.RawValue);

    /// <summary>The angle turned back by <paramref name="turn"/>, continuing around the circle below zero.</summary>
    public static RotationAngle operator -(RotationAngle angle, RotationAngle turn) => angle.AddRaw(-turn.RawValue);

    /// <summary>The turn <paramref name="turn"/> taken the other way round the circle (#2563).</summary>
    public static RotationAngle operator -(RotationAngle turn) => Zero - turn;

    /// <summary>Whether <paramref name="left"/> lies earlier in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator <(RotationAngle left, RotationAngle right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies later in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator >(RotationAngle left, RotationAngle right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> lies no later in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator <=(RotationAngle left, RotationAngle right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> lies no earlier in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator >=(RotationAngle left, RotationAngle right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public int CompareTo(RotationAngle other) => RawValue.CompareTo(other.RawValue);

    /// <inheritdoc />
    public override string ToString() => $"{RawValue}/65536 turn";
}
