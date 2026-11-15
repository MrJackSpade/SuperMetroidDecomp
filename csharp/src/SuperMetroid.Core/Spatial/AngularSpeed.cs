namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How far, and which way, an angle turns each update (#2042): whole steps (256ths of a turn) with a 256th-of-a-step
/// fraction, positive toward increasing angles and negative toward decreasing ones. Unlike a <see cref="RotationAngle"/>
/// it does not wrap at a full turn, so a speed toward decreasing angles reads as negative steps: its whole steps round
/// toward decreasing angles, as an enemy that steps its angle by a speed's whole steps reads them, with the fraction
/// carrying beneath them from update to update. Its fraction is a rotation angle's, so a speed turns a rotation angle
/// exactly. The same measure serves the change a turning speed takes each update.
/// </summary>
public readonly record struct AngularSpeed
{
    /// <summary>Number of fractions in one step.</summary>
    private const int FractionsPerStep = 256;

    /// <summary>The most whole steps a speed turns an update either way: half a turn.</summary>
    private const int MostWholeSteps = RotationAngle.StepsPerTurn / 2;

    /// <summary>The speed of <paramref name="fractionsOfStep"/> 256ths of a step an update, within half a turn either way.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    private AngularSpeed(int fractionsOfStep) =>
        FractionsOfStep = Ensure.BetweenInclusive(-MostWholeSteps * FractionsPerStep, MostWholeSteps * FractionsPerStep - 1, fractionsOfStep);

    /// <summary>The speed in 256ths of a step an update, -32768 through 32767: negative toward decreasing angles.</summary>
    [IntegerQuantity("signed angular speed in 256ths of a step an update")]
    public int FractionsOfStep { get; }

    /// <summary>No turning at all.</summary>
    public static AngularSpeed Zero => default;

    /// <summary>A speed of <paramref name="steps"/> whole steps an update, -128 through 127, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    public static AngularSpeed FromSteps(int steps) =>
        new(Ensure.BetweenInclusive(-MostWholeSteps, MostWholeSteps - 1, steps) * FractionsPerStep);

    /// <summary>
    /// A speed of <paramref name="fractionsOfStep"/> 256ths of a step an update, -32768 through 32767: the precision a
    /// rotation angle turns at.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed turns half a turn or more an update.</exception>
    public static AngularSpeed FromFractionsOfStep(int fractionsOfStep) => new(fractionsOfStep);

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

    /// <summary>The fraction beneath <see cref="WholeSteps"/>, 0 through 255 256ths of a step, toward increasing angles.</summary>
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
    /// beneath the cap.
    /// </summary>
    public AngularSpeed WithWholeStepsLimitedTo(AngularSpeed limit)
    {
        int whole = WholeSteps;
        int cap = limit.WholeSteps;
        bool beyond = limit.IsNegative ? whole < cap : whole > cap;
        return beyond ? new(cap * FractionsPerStep + Fraction) : this;
    }

    /// <summary>A speed changed by <paramref name="change"/>, its fractions carrying into its whole steps.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sum turns half a turn or more an update.</exception>
    public static AngularSpeed operator +(AngularSpeed speed, AngularSpeed change) =>
        new(speed.FractionsOfStep + change.FractionsOfStep);

    /// <summary>The same speed turning the other way.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed is the most negative one, which has no reversal within the range.</exception>
    public static AngularSpeed operator -(AngularSpeed speed) => new(-speed.FractionsOfStep);

    /// <inheritdoc />
    public override string ToString() => $"{FractionsOfStep}/256 step per update";
}
