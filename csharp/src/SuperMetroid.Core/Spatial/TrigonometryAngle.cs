namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An angle counted in steps of a 256th of a turn that wraps at a full turn, in the trigonometric convention: zero
/// points right and <c>64</c> up (negative Y). This is the angle the game's sine and cosine take
/// (<see cref="Trigonometry"/>); <see cref="RotationAngle"/> is the form with a fraction, whose whole steps are this value.
/// </summary>
[EncodingOwner("sine-table byte angle")]
public readonly record struct TrigonometryAngle : IComparable<TrigonometryAngle>
{
    /// <summary>Orders angles by how far they lie through the turn from zero.</summary>
    public int CompareTo(TrigonometryAngle other) => RawValue.CompareTo(other.RawValue);

    /// <summary>The angle of <paramref name="rawValue"/> steps, 0 through 255.</summary>
    /// <param name="rawValue">The angle in steps of a 256th of a turn, 0 through 255.</param>
    public TrigonometryAngle(int rawValue) => RawValue = checked((byte)rawValue);

    /// <summary>The angle in steps of a 256th of a turn, 0 through 255.</summary>
    public byte RawValue { get; init; }

    /// <summary>
    /// The angle a computation in steps lands on: a turn past a full turn, or below zero, continues around the
    /// circle, as the game's angle arithmetic wraps.
    /// </summary>
    /// <param name="units">The computed angle in steps, which may lie outside one turn.</param>
    public static TrigonometryAngle Wrapped(int units) => new(unchecked((byte)units));

    /// <summary>No turn: right.</summary>
    public static TrigonometryAngle Zero { get; } = new(0);

    /// <summary>One quarter turn: up.</summary>
    public static TrigonometryAngle QuarterTurn { get; } = new(0x40);

    /// <summary>One half turn: left.</summary>
    public static TrigonometryAngle HalfTurn { get; } = new(0x80);

    /// <summary>
    /// The steps this angle lies past the start of its half of the turn, 0 through 127: the angle folded onto the first
    /// half of the turn, as the game's sine folds it before restoring the sign from <see cref="IsInSecondHalf"/>.
    /// </summary>
    public int StepsIntoHalfTurn => RawValue % HalfTurn.RawValue;

    /// <summary>
    /// Whether this angle lies in the first or the third quarter of the turn: less than a quarter turn into its half (#2439).
    /// </summary>
    public bool IsInFirstOrThirdQuarter => StepsIntoHalfTurn < QuarterTurn.RawValue;

    /// <summary>Whether this angle lies in the second half of the turn, where the sine wave is negative.</summary>
    public bool IsInSecondHalf => RawValue >= HalfTurn.RawValue;

    /// <summary>Turns the angle by <paramref name="delta"/> signed steps, continuing around the turn.</summary>
    public TrigonometryAngle TurnedBySteps(int delta) => Wrapped(RawValue + delta);

    /// <summary>The angle turned on by <paramref name="turn"/>, wrapping around the full turn.</summary>
    public TrigonometryAngle TurnedBy(TrigonometryAngle turn) => Wrapped(RawValue + turn.RawValue);

    /// <summary>
    /// The turn from <paramref name="origin"/> to this angle the shorter way round, in steps, -128 through 127: negative
    /// when this angle lies behind the origin, as an edge compares itself with a limit it may not cross.
    /// </summary>
    public int StepsFrom(TrigonometryAngle origin) => unchecked((sbyte)(RawValue - origin.RawValue));

    /// <summary>
    /// Which of <paramref name="directions"/> equally spaced directions, numbered from zero at <see cref="Zero"/>, lies
    /// nearest this angle: an angle halfway between two takes the later one, and an angle within half a spacing before
    /// a full turn takes the first.
    /// </summary>
    /// <param name="directions">The number of directions the turn holds, 1 through 256.</param>
    public int NearestDirection(int directions)
    {
        Ensure.BetweenInclusive(1, StepsPerTurn, directions);
        return (RawValue * directions + StepsPerTurn / 2) / StepsPerTurn % directions;
    }

    /// <summary>The steps in one full turn.</summary>
    private const int StepsPerTurn = 256;

    /// <summary>The angle turned on by <paramref name="turn"/>, wrapping around the full turn.</summary>
    public static TrigonometryAngle operator +(TrigonometryAngle angle, TrigonometryAngle turn) => angle.TurnedBy(turn);

    /// <summary>
    /// The angle turned back by <paramref name="turn"/>, wrapping below zero: the sine a quarter turn back is the negative
    /// cosine the up-zero headings read their vertical speed from.
    /// </summary>
    public static TrigonometryAngle operator -(TrigonometryAngle angle, TrigonometryAngle turn) =>
        Wrapped(angle.RawValue - turn.RawValue);

    /// <summary>Whether <paramref name="left"/> lies earlier in the turn from zero than <paramref name="right"/>.</summary>
    public static bool operator <(TrigonometryAngle left, TrigonometryAngle right) => left.RawValue < right.RawValue;

    /// <summary>Whether <paramref name="left"/> lies later in the turn from zero than <paramref name="right"/>.</summary>
    public static bool operator >(TrigonometryAngle left, TrigonometryAngle right) => left.RawValue > right.RawValue;

    /// <summary>Whether <paramref name="left"/> lies no later in the turn from zero than <paramref name="right"/>.</summary>
    public static bool operator <=(TrigonometryAngle left, TrigonometryAngle right) => left.RawValue <= right.RawValue;

    /// <summary>Whether <paramref name="left"/> lies no earlier in the turn from zero than <paramref name="right"/>.</summary>
    public static bool operator >=(TrigonometryAngle left, TrigonometryAngle right) => left.RawValue >= right.RawValue;

    /// <summary>
    /// This angle carried into the half turn from zero up to (not including) <see cref="HalfTurn"/>, keeping its
    /// position within the half turn: an angle in the other half turns back by a half turn.
    /// </summary>
    public TrigonometryAngle InFirstHalfTurn() => new(RawValue % HalfTurn.RawValue);

    /// <summary>
    /// This angle carried into the half turn from <see cref="HalfTurn"/> up to a full turn, keeping its position within
    /// the half turn: an angle in the other half turns on by a half turn.
    /// </summary>
    public TrigonometryAngle InSecondHalfTurn() => new(RawValue % HalfTurn.RawValue + HalfTurn.RawValue);

    /// <summary>
    /// This angle carried into the half turn on the right, from just past straight down up to straight up (#2555),
    /// keeping its direction's height: an angle on the left is reflected across the vertical, as an aim turned toward the
    /// right is.
    /// </summary>
    public TrigonometryAngle InRightHalfTurn() => QuarterTurn - (QuarterTurn - this).InFirstHalfTurn();

    /// <summary>
    /// This angle carried into the half turn on the left, from just past straight up down to straight down (#2555),
    /// keeping its direction's height: an angle on the right is reflected across the vertical.
    /// </summary>
    public TrigonometryAngle InLeftHalfTurn() => QuarterTurn - (QuarterTurn - this).InSecondHalfTurn();

    /// <summary>
    /// The steps from straight up round to this angle, turning clockwise as the screen shows it (#2555): the order the
    /// compass directions an aim picks are counted in, up first.
    /// </summary>
    private int StepsClockwiseFromUp => (QuarterTurn.RawValue - RawValue + StepsPerTurn) % StepsPerTurn;

    /// <summary>The steps of a whole turn.</summary>
    private const int StepsPerTurn = 256;

    /// <summary>The eight compass directions in clockwise order from up, each an eighth of the turn on from the last.</summary>
    internal static readonly CompassDirection[] DirectionsClockwise =
    [
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
    ];

    /// <summary>The four straight directions in clockwise order from up, each a quarter of the turn on from the last.</summary>
    private static readonly CompassDirection[] StraightDirectionsClockwise =
        [CompassDirection.Up, CompassDirection.Right, CompassDirection.Down, CompassDirection.Left];

    /// <summary>
    /// The compass direction opening the eighth of the turn that holds the angle: each direction's eighth runs from it
    /// clockwise up to the next, so an angle takes the direction it has last turned past, clockwise.
    /// </summary>
    public CompassDirection DirectionOfEighth() => DirectionsClockwise[StepsClockwiseFromUp / (StepsPerTurn / 8)];

    /// <summary>
    /// The compass direction nearest the angle: the one whose eighth of the turn, centred on it, holds the angle. An angle
    /// exactly halfway between two takes the later one clockwise.
    /// </summary>
    public CompassDirection NearestDirection() =>
        DirectionsClockwise[(StepsClockwiseFromUp + StepsPerTurn / 16) % StepsPerTurn / (StepsPerTurn / 8)];

    /// <summary>
    /// The straight direction, up, right, down or left, nearest the angle: the one whose quarter of the turn, centred on
    /// it, holds the angle. An angle exactly halfway between two takes the later one clockwise.
    /// </summary>
    public CompassDirection NearestStraightDirection() =>
        StraightDirectionsClockwise[(StepsClockwiseFromUp + StepsPerTurn / 8) % StepsPerTurn / (StepsPerTurn / 4)];

    /// <summary>This angle as the whole steps of an 8.8 <see cref="RotationAngle"/>.</summary>
    public RotationAngle ToAngle() => RotationAngle.FromSteps(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"{RawValue}/256 turn";
}
