namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The direction of one link of Ridley's tail, in steps of a 256th of a turn counted from straight down without
/// wrapping: counterclockwise is positive, clockwise negative. The step within the turn (<see cref="Direction"/>) aims
/// the link; the whole count orders the swing limits and whip targets across the turn boundary, so a whip target a
/// whole turn round still lies beyond a limit that points the same way, and a link swings the whole turn to reach it.
/// </summary>
public readonly partial record struct RidleyTailAngle : IComparable<RidleyTailAngle>
{
    /// <summary>Holds the angle in steps from straight down.</summary>
    private RidleyTailAngle(int stepsFromStraightDown) => StepsFromStraightDown = stepsFromStraightDown;

    /// <summary>The angle in steps of the turn from straight down, counterclockwise positive, counted without
    /// wrapping.</summary>
    internal int StepsFromStraightDown { get; }

    /// <summary>The link pointing straight down from its anchor.</summary>
    public static RidleyTailAngle StraightDown => default;

    /// <summary>
    /// The direction the link's offset is computed from: the step within the turn, with straight down at
    /// zero, the link's horizontal offset from its sine and its vertical offset from its cosine.
    /// </summary>
    public TrigonometryAngle Direction => TrigonometryAngle.Wrapped(StepsFromStraightDown);

    /// <summary>The angle turned <paramref name="steps"/> steps of the turn counterclockwise.</summary>
    public RidleyTailAngle TurnedCounterclockwise(int steps) => new(StepsFromStraightDown + steps);

    /// <summary>The angle turned counterclockwise by the turn <paramref name="direction"/> makes from zero.</summary>
    public RidleyTailAngle TurnedCounterclockwise(TrigonometryAngle direction) =>
        TurnedCounterclockwise(direction.ToAngle().Sector(RotationAngle.StepsPerTurn));

    /// <summary>The angle turned <paramref name="turn"/> counterclockwise.</summary>
    public RidleyTailAngle TurnedCounterclockwise(RidleyTailTurn turn) => TurnedCounterclockwise(turn.Steps);

    /// <summary>The angle turned <paramref name="steps"/> steps of the turn clockwise.</summary>
    public RidleyTailAngle TurnedClockwise(int steps) => new(StepsFromStraightDown - steps);

    /// <summary>The angle turned <paramref name="turn"/> clockwise.</summary>
    public RidleyTailAngle TurnedClockwise(RidleyTailTurn turn) => TurnedClockwise(turn.Steps);

    /// <summary>The angle turned <paramref name="turn"/> the way <paramref name="direction"/> says.</summary>
    public RidleyTailAngle Turned(RotationDirection direction, RidleyTailTurn turn) =>
        direction == RotationDirection.Clockwise ? TurnedClockwise(turn) : TurnedCounterclockwise(turn);

    /// <summary>
    /// Whether a link swinging <paramref name="direction"/> at this angle has reached <paramref name="bound"/>: it lies at
    /// or clockwise of the bound swinging clockwise, at or counterclockwise of it swinging counterclockwise.
    /// </summary>
    public bool Reaches(RidleyTailAngle bound, RotationDirection direction) =>
        direction == RotationDirection.Clockwise ? this <= bound : this >= bound;

    /// <summary>The angle turned <paramref name="turns"/> whole turns counterclockwise, clockwise when negative: the same direction, ordered a turn further round.</summary>
    public RidleyTailAngle TurnedByWholeTurns(int turns) => new(StepsFromStraightDown + turns * RotationAngle.StepsPerTurn);

    /// <summary>The angle a facing change reflects this one to: mirrored about straight down, so the link points the same distance round the other way.</summary>
    public RidleyTailAngle Mirrored() => new(-StepsFromStraightDown);

    /// <summary>Whether <paramref name="left"/> lies further clockwise than <paramref name="right"/>.</summary>
    public static bool operator <(RidleyTailAngle left, RidleyTailAngle right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies further counterclockwise than <paramref name="right"/>.</summary>
    public static bool operator >(RidleyTailAngle left, RidleyTailAngle right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> lies at or clockwise of <paramref name="right"/>.</summary>
    public static bool operator <=(RidleyTailAngle left, RidleyTailAngle right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> lies at or counterclockwise of <paramref name="right"/>.</summary>
    public static bool operator >=(RidleyTailAngle left, RidleyTailAngle right) => left.CompareTo(right) >= 0;

    /// <summary>Orders angles from clockwise to counterclockwise.</summary>
    public int CompareTo(RidleyTailAngle other) => StepsFromStraightDown.CompareTo(other.StepsFromStraightDown);

    /// <inheritdoc />
    public override string ToString() => $"{StepsFromStraightDown}/256 turn from straight down";
}
