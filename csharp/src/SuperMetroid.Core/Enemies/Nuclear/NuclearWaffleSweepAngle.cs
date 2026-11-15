namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// An angle along a Nuclear Waffle's sweep (#2042): steps (256ths of a turn) with a 256th-of-a-step fraction, counted on
/// past a full turn rather than wrapping. A sweep runs over the top of the orbit from just short of a full turn (240
/// steps) into the next turn (400 steps), and its links trail the head by up to 84 steps, so the angles a sweep compares
/// span more than one turn: an angle and the same direction a turn later stand at different places along the sweep and
/// clamp differently, which a <see cref="RotationAngle"/> that wraps at a full turn cannot tell apart. Only the orbit
/// reads the direction within the turn (<see cref="ToRotationAngle"/>).
/// </summary>
public readonly record struct NuclearWaffleSweepAngle : IComparable<NuclearWaffleSweepAngle>
{
    /// <summary>Number of fractions in one step.</summary>
    private const int FractionsPerStep = 256;

    /// <summary>The most whole steps a sweep angle counts: far past any sweep, which stays within its second turn.</summary>
    private const int MostSteps = ushort.MaxValue;

    /// <summary>The angle <paramref name="fractionsOfStep"/> 256ths of a step on from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle lies below zero or past the sweep angle's range.</exception>
    private NuclearWaffleSweepAngle(int fractionsOfStep) =>
        FractionsOfStep = Ensure.BetweenInclusive(0, (MostSteps + 1) * FractionsPerStep - 1, fractionsOfStep);

    /// <summary>The angle in 256ths of a step from zero, counted on past each full turn.</summary>
    [IntegerQuantity("sweep angle in 256ths of a step, counted on past each full turn")]
    public int FractionsOfStep { get; }

    /// <summary>The angle <paramref name="steps"/> whole steps on from zero, counted on past each full turn, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle lies below zero or past the sweep angle's range.</exception>
    public static NuclearWaffleSweepAngle FromSteps(int steps) =>
        new(Ensure.BetweenInclusive(0, MostSteps, steps) * FractionsPerStep);

    /// <summary>The whole step the angle lies in: the angle with its fraction dropped, as the head's place is read.</summary>
    public NuclearWaffleSweepAngle WholeStep => new(FractionsOfStep - FractionsOfStep % FractionsPerStep);

    /// <summary>The angle turned by <paramref name="steps"/> whole steps, back when negative, keeping its fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero or past the sweep angle's range.</exception>
    public NuclearWaffleSweepAngle TurnedBySteps(int steps) => new(FractionsOfStep + steps * FractionsPerStep);

    /// <summary>The direction the angle points within one turn, its fraction kept: the rotation angle the orbit is placed at.</summary>
    public RotationAngle ToRotationAngle() => RotationAngle.NormalizeRaw(FractionsOfStep);

    /// <summary>The angle turned one update's worth at <paramref name="speed"/>, its fraction carrying into its whole steps.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero or past the sweep angle's range.</exception>
    public static NuclearWaffleSweepAngle operator +(NuclearWaffleSweepAngle angle, AngularSpeed speed) =>
        new(angle.FractionsOfStep + speed.FractionsOfStep);

    /// <summary>Whether <paramref name="left"/> lies earlier along the sweep's count of steps than <paramref name="right"/>.</summary>
    public static bool operator <(NuclearWaffleSweepAngle left, NuclearWaffleSweepAngle right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies later along the sweep's count of steps than <paramref name="right"/>.</summary>
    public static bool operator >(NuclearWaffleSweepAngle left, NuclearWaffleSweepAngle right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> lies no later along the sweep's count of steps than <paramref name="right"/>.</summary>
    public static bool operator <=(NuclearWaffleSweepAngle left, NuclearWaffleSweepAngle right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> lies no earlier along the sweep's count of steps than <paramref name="right"/>.</summary>
    public static bool operator >=(NuclearWaffleSweepAngle left, NuclearWaffleSweepAngle right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public int CompareTo(NuclearWaffleSweepAngle other) => FractionsOfStep.CompareTo(other.FractionsOfStep);

    /// <inheritdoc />
    public override string ToString() => $"{FractionsOfStep}/256 step";
}
