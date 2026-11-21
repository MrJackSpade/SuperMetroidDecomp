namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How far, and which way, an angle turns each update (#2042): whole steps of the turn with a fraction of a step,
/// positive toward increasing angles and negative toward decreasing ones. Unlike a <see cref="RotationAngle"/>
/// it does not wrap at a full turn, so a speed toward decreasing angles reads as negative steps: its whole steps round
/// toward decreasing angles, as an enemy that steps its angle by a speed's whole steps reads them, with the fraction
/// carrying beneath them from update to update. Its fraction is a rotation angle's, so a speed turns a rotation angle
/// exactly. The same measure serves the change a turning speed takes each update.
/// </summary>
public readonly record struct AngularSpeed : IComparable<AngularSpeed>, System.Numerics.IUnaryNegationOperators<AngularSpeed, AngularSpeed>
{
    /// <summary>Number of fractions in one step.</summary>
    private const int FractionsPerStep = 256;

    /// <summary>The most whole steps a speed turns an update either way: half a turn.</summary>
    private const int MostWholeSteps = RotationAngle.StepsPerTurn / 2;

    /// <summary>The speed of <paramref name="fractionsOfStep"/> fractions of a step an update, within half a turn
    /// either way.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    internal AngularSpeed(int fractionsOfStep) =>
        FractionsOfStep = Ensure.BetweenInclusive(-MostWholeSteps * FractionsPerStep, MostWholeSteps * FractionsPerStep - 1, fractionsOfStep);

    /// <summary>The speed in fractions of a step an update: negative toward decreasing angles.</summary>
    [IntegerQuantity("signed angular speed in 256ths of a step an update")]
    public int FractionsOfStep { get; }

    /// <summary>No turning at all.</summary>
    public static AngularSpeed Zero => default;

    /// <summary>A speed of <paramref name="steps"/> whole steps an update, -128 through 127, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    public static AngularSpeed FromSteps(int steps) =>
        new(Ensure.BetweenInclusive(-MostWholeSteps, MostWholeSteps - 1, steps) * FractionsPerStep);

    /// <summary>
    /// A speed of <paramref name="fractionsOfStep"/> fractions of a step an update: the precision a rotation angle
    /// turns at.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    public static AngularSpeed FromFractionsOfStep(int fractionsOfStep) => new(fractionsOfStep);

    /// <summary>
    /// The turning speed of one step an update for each pixel of <paramref name="speed"/> (#2555), its fraction turning
    /// likewise: a 256th of a step for each 256th of a pixel, the speed's finer fraction dropped, toward decreasing angles
    /// for a speed toward decreasing coordinates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    public static AngularSpeed StepPerPixelOf(SignedSixteenSixteen speed) => new(speed.InTwoFiftySixths);

    /// <summary>
    /// A speed of <paramref name="numerator"/> <paramref name="denominator"/>ths of a step an update (#2555), the denominator
    /// a power of two from 1 through 256 so the fraction is kept exactly: a step and a half an update, or a quarter step.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The denominator is no power of two from 1 through 256, or the speed turns half a turn or more an update.
    /// </exception>
    public static AngularSpeed FromStepFraction(int numerator, int denominator) =>
        new(PowerOfTwoFraction.InFinestUnits(numerator, denominator, FractionsPerStep));

    /// <summary>
    /// The speed's whole steps, rounded toward decreasing angles: a speed of a fraction of a step toward decreasing
    /// angles reads minus one step, as an angle stepped by whole steps turns.
    /// </summary>
    [IntegerQuantity("signed whole steps an angle turns an update")]
    public int WholeSteps
    {
        get
        {
            int whole = Math.DivRem(FractionsOfStep, FractionsPerStep, out int remainder);
            return remainder < 0 ? whole - 1 : whole;
        }
    }

    /// <summary>The fraction beneath <see cref="WholeSteps"/>, less than one step, toward increasing angles.</summary>
    private int Fraction => FractionsOfStep - WholeSteps * FractionsPerStep;

    /// <summary>Whether the speed turns toward decreasing angles.</summary>
    public bool IsNegative => FractionsOfStep < 0;

    /// <summary>
    /// The speed split into <paramref name="parts"/> equal shares: one share, rounded toward no turning to a whole 256th
    /// of a step.
    /// </summary>
    /// <param name="parts">The number of equal shares, at least one.</param>
    public AngularSpeed DividedBy(int parts) => new(FractionsOfStep / Ensure.BetweenInclusive(1, int.MaxValue, parts));

    /// <summary>
    /// The speed with its whole steps held to <paramref name="limit"/>'s when they lie beyond them in the limit's
    /// direction, its fraction kept: a speed toward increasing angles is held to at most the limit's whole steps, one
    /// toward decreasing angles to at least them, as a turn that caps only its whole steps lets its fraction accumulate
    /// beneath the cap, as a Ki-Hunter's swoop caps its turning.
    /// </summary>
    public AngularSpeed CappedInWholeSteps(AngularSpeed limit)
    {
        int whole = WholeSteps;
        int cap = limit.WholeSteps;
        bool beyond = limit.IsNegative ? whole < cap : whole > cap;
        return beyond ? new(cap * FractionsPerStep + Fraction) : this;
    }

    /// <summary>How fast the speed turns, whichever way: the speed itself, or the same speed turning the other way.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed is the most negative one, which has no reversal within the range.</exception>
    public AngularSpeed Magnitude => new(Math.Abs(FractionsOfStep));

    /// <summary>
    /// The speed scaled by <paramref name="share"/> parts of <paramref name="parts"/> (#2110), rounded toward no turning
    /// to a whole 256th of a step, as a swing slowed by water turns through only a share of its speed.
    /// </summary>
    /// <param name="share">The parts of the speed kept, zero or more.</param>
    /// <param name="parts">The parts the speed divides into, at least one.</param>
    public AngularSpeed ScaledBy(int share, int parts) =>
        new(FractionsOfStep * Ensure.BetweenInclusive(0, int.MaxValue, share) / Ensure.BetweenInclusive(1, int.MaxValue, parts));

    /// <summary>Half the speed, rounded toward decreasing angles to a whole 256th of a step (#2110).</summary>
    public AngularSpeed HalvedRoundingDown() => new((int)Math.Floor(FractionsOfStep / 2.0));

    /// <summary>
    /// The speed held within <paramref name="most"/> either way (#2110): a speed faster than it toward either side turns
    /// at it toward that side.
    /// </summary>
    /// <param name="most">The fastest the speed may turn, either way; not negative.</param>
    public AngularSpeed LimitedTo(AngularSpeed most)
    {
        int limit = Ensure.BetweenInclusive(0, int.MaxValue, most.FractionsOfStep);
        return new(Math.Clamp(FractionsOfStep, -limit, limit));
    }

    /// <summary>
    /// The speed slowed toward no turning by <paramref name="slowing"/> (#2110), stopping at none rather than passing it.
    /// </summary>
    /// <param name="slowing">How much slower the speed turns; not negative.</param>
    public AngularSpeed SlowedTowardRest(AngularSpeed slowing)
    {
        int amount = Ensure.BetweenInclusive(0, int.MaxValue, slowing.FractionsOfStep);
        return IsNegative ? new(Math.Min(0, FractionsOfStep + amount)) : new(Math.Max(0, FractionsOfStep - amount));
    }

    /// <summary>A speed changed by <paramref name="change"/>, its fractions carrying into its whole steps.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sum turns half a turn or more an update.</exception>
    public static AngularSpeed operator +(AngularSpeed speed, AngularSpeed change) =>
        new(speed.FractionsOfStep + change.FractionsOfStep);

    /// <summary>The same speed turning the other way.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed is the most negative one, which has no reversal within the range.</exception>
    public static AngularSpeed operator -(AngularSpeed speed) => new(-speed.FractionsOfStep);

    /// <inheritdoc />
    public int CompareTo(AngularSpeed other) => FractionsOfStep.CompareTo(other.FractionsOfStep);

    /// <summary>Whether <paramref name="left"/> turns further toward decreasing angles than <paramref name="right"/>.</summary>
    public static bool operator <(AngularSpeed left, AngularSpeed right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> turns further toward increasing angles than <paramref name="right"/>.</summary>
    public static bool operator >(AngularSpeed left, AngularSpeed right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> turns no further toward increasing angles than <paramref name="right"/>.</summary>
    public static bool operator <=(AngularSpeed left, AngularSpeed right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> turns no further toward decreasing angles than <paramref name="right"/>.</summary>
    public static bool operator >=(AngularSpeed left, AngularSpeed right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public override string ToString() => $"{FractionsOfStep}/256 step per update";
}
