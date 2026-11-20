namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// An angle along a Nuclear Waffle's sweep (#2042): steps of the turn with a fraction of a step, counted on
/// past a full turn rather than wrapping. A sweep runs over the top of the orbit from just short of a full turn into
/// the next turn, and its links trail the head, so the angles a sweep compares
/// span more than one turn: an angle and the same direction a turn later stand at different places along the sweep and
/// clamp differently, which a <see cref="RotationAngle"/> that wraps at a full turn cannot tell apart. Only the orbit
/// reads the direction within the turn (<see cref="ToRotationAngle"/>).
/// </summary>
public readonly record struct NuclearWaffleSweepAngle : IComparable<NuclearWaffleSweepAngle>
{
    /// <summary>Number of fractions in one step.</summary>
    private const int FractionsPerStep = 256;

    /// <summary>The angle <paramref name="fractionsOfStep"/> fractions of a step on from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle lies below zero.</exception>
    private NuclearWaffleSweepAngle(int fractionsOfStep) =>
        FractionsOfStep = Ensure.AtLeastZero(fractionsOfStep);

    /// <summary>The angle in fractions of a step from zero, counted on past each full turn.</summary>
    [IntegerQuantity("sweep angle in 256ths of a step, counted on past each full turn")]
    private int FractionsOfStep { get; }

    /// <summary>The angle <paramref name="steps"/> whole steps on from zero, counted on past each full turn, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The angle lies below zero.</exception>
    public static NuclearWaffleSweepAngle FromSteps(int steps) =>
        new(Ensure.AtLeastZero(steps) * FractionsPerStep);

    /// <summary>The whole step the angle lies in: the angle with its fraction dropped, as the head's place is read.</summary>
    public NuclearWaffleSweepAngle WholeStep => new(FractionsOfStep - FractionsOfStep % FractionsPerStep);

    /// <summary>The angle turned by <paramref name="steps"/> whole steps, back when negative, keeping its fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero.</exception>
    public NuclearWaffleSweepAngle TurnedBySteps(int steps) => new(FractionsOfStep + steps * FractionsPerStep);

    /// <summary>
    /// The angle turned <paramref name="steps"/> whole steps ahead along a sweep running <paramref name="direction"/>: on
    /// toward larger angles for a forward sweep, back for a backward one, keeping its fraction (#2439).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero.</exception>
    internal NuclearWaffleSweepAngle TurnedAhead(int steps, NuclearWaffleSweepDirection direction) => TurnedBySteps(steps * StepSign[direction]);

    /// <summary>The sign a sweep running each way turns its steps by: on toward larger angles forward, back backward.</summary>
    private static IdentityTable<NuclearWaffleSweepDirection, int> StepSign => field ??= new(new Dictionary<NuclearWaffleSweepDirection, int>
    {
        [NuclearWaffleSweepDirection.Forward] = 1,
        [NuclearWaffleSweepDirection.Backward] = -1,
    });

    /// <summary>
    /// The angle turned <paramref name="steps"/> whole steps behind along a sweep running <paramref name="direction"/>,
    /// keeping its fraction (#2439): where a link trails the one before it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero.</exception>
    internal NuclearWaffleSweepAngle TurnedBehind(int steps, NuclearWaffleSweepDirection direction) => TurnedAhead(-steps, direction);

    /// <summary>
    /// The angle turned one update's worth at <paramref name="speed"/> along a sweep running <paramref name="direction"/>,
    /// its fraction carrying into its whole steps (#2439).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero.</exception>
    internal NuclearWaffleSweepAngle TurnedOn(AngularSpeed speed, NuclearWaffleSweepDirection direction) => direction == NuclearWaffleSweepDirection.Forward ? this + speed : this + -speed;

    /// <summary>
    /// Whether the angle has reached <paramref name="mark"/> along a sweep running <paramref name="direction"/> (#2439): a
    /// forward sweep at or past it, a backward one strictly past it, as the game compares the two ways; an angle standing
    /// on a mark has not yet reached it going backward.
    /// </summary>
    internal bool HasReached(NuclearWaffleSweepAngle mark, NuclearWaffleSweepDirection direction) => direction == NuclearWaffleSweepDirection.Forward ? this >= mark : this < mark;

    /// <summary>The direction the angle points within one turn, its fraction kept: the rotation angle the orbit is placed at.</summary>
    public RotationAngle ToRotationAngle() => RotationAngle.NormalizeRaw(FractionsOfStep);

    /// <summary>The angle turned one update's worth at <paramref name="speed"/>, its fraction carrying into its whole steps.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The turned angle lies below zero.</exception>
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
