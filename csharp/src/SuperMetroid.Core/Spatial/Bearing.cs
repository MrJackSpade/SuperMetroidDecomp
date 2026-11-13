namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A compass bearing of 256 units per turn, as the enemies' aim-at calculation produces it: zero points up and the
/// bearing advances clockwise, so <c>64</c> is right and <c>128</c> down. The sine and cosine lookups take the other
/// convention, <see cref="TrigonometryAngle"/>; <see cref="ToTableAngle"/> converts between them.
/// </summary>
[EncodingOwner("clockwise byte bearing")]
public readonly record struct Bearing
{
    /// <summary>The bearing of <paramref name="rawValue"/> units clockwise from up, 0 through 255.</summary>
    /// <param name="rawValue">The bearing in units of a 256th of a turn, 0 through 255.</param>
    public Bearing(int rawValue) => RawValue = checked((byte)rawValue);

    /// <summary>The bearing in units of a 256th of a turn, 0 through 255.</summary>
    public byte RawValue { get; init; }

    /// <summary>
    /// The bearing a computation lands on: a bearing past a full turn, or below zero, continues around the circle.
    /// </summary>
    /// <param name="units">The computed bearing, which may lie outside one turn.</param>
    public static Bearing Wrapped(int units) => new(unchecked((byte)units));

    /// <summary>The same direction in the sine-table convention: a quarter turn minus the bearing.</summary>
    public TrigonometryAngle ToTableAngle() => TrigonometryAngle.Wrapped(TrigonometryAngle.QuarterTurn.RawValue - RawValue);

    /// <summary>
    /// The table angle of the same number of units: the bearing's direction mirrored across the line between up and
    /// right, since the bearing counts clockwise from up and the table angle the other way from right. Aim calculations that
    /// take this angle read its sine as X and its cosine as Y, which turns the mirror back into the bearing's direction.
    /// </summary>
    public TrigonometryAngle ToMirroredTableAngle() => new(RawValue);

    /// <summary>The bearing turned <paramref name="steps"/> 256ths of a turn clockwise, anticlockwise when negative, continuing around the circle.</summary>
    public Bearing TurnedBySteps(int steps) => Wrapped(RawValue + steps);

    /// <summary>
    /// Which of <paramref name="sectors"/> equal arcs of the turn, numbered clockwise from zero at up, holds the bearing:
    /// for a power of two, the bearing's top bits, which a facing or look choice reads.
    /// </summary>
    /// <param name="sectors">The number of arcs the turn divides into, 1 through 256.</param>
    public int Sector(int sectors) => RawValue * Ensure.BetweenInclusive(1, RotationAngle.StepsPerTurn, sectors) / RotationAngle.StepsPerTurn;

    /// <summary>
    /// The straight direction, up, right, down or left, nearest the bearing: the one whose quarter of the turn, centred
    /// on it, holds the bearing. A bearing exactly halfway between two takes the later one clockwise.
    /// </summary>
    public CompassDirection NearestStraightDirection() =>
        StraightDirectionsClockwise[TurnedBySteps(RotationAngle.StepsPerTurn / 8).Sector(StraightDirectionsClockwise.Length)];

    /// <summary>
    /// The compass direction opening the eighth of the turn that holds the bearing: each direction's eighth runs from it
    /// clockwise up to the next, so a bearing takes the direction it has last turned past.
    /// </summary>
    public CompassDirection DirectionOfEighth() => DirectionsClockwise[Sector(DirectionsClockwise.Length)];

    /// <summary>
    /// The compass direction nearest the bearing: the one whose eighth of the turn, centred on it, holds the bearing. A
    /// bearing exactly halfway between two takes the later one clockwise.
    /// </summary>
    public CompassDirection NearestDirection() =>
        DirectionsClockwise[TurnedBySteps(RotationAngle.StepsPerTurn / 16).Sector(DirectionsClockwise.Length)];

    /// <summary>The bearing pointing straight along <paramref name="direction"/>: a whole number of eighths of the turn clockwise from up.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not a compass direction.</exception>
    public static Bearing Toward(CompassDirection direction)
    {
        int eighths = Array.IndexOf(DirectionsClockwise, direction);
        return eighths >= 0
            ? new(eighths * (RotationAngle.StepsPerTurn / DirectionsClockwise.Length))
            : throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Undefined {nameof(CompassDirection)}.");
    }

    /// <summary>The four straight directions in clockwise order from up, each a quarter of the turn on from the last.</summary>
    private static readonly CompassDirection[] StraightDirectionsClockwise =
        [CompassDirection.Up, CompassDirection.Right, CompassDirection.Down, CompassDirection.Left];

    /// <summary>The eight compass directions in clockwise order from up, each an eighth of the turn on from the last.</summary>
    private static readonly CompassDirection[] DirectionsClockwise =
    [
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
    ];

    /// <inheritdoc />
    public override string ToString() => $"{RawValue}/256 turn clockwise from up";
}
