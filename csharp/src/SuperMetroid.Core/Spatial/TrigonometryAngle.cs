namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An angle of 256 units per turn that wraps at a full turn, in the sine-table convention: zero points right and
/// <c>$40</c> up (negative Y). This is the angle the game's sine and cosine lookups take; <see cref="RotationAngle"/>
/// is the form with a fraction, whose whole units are this value.
/// </summary>
[EncodingOwner("sine-table byte angle")]
public readonly record struct TrigonometryAngle : IComparable<TrigonometryAngle>
{
    /// <summary>Orders angles by how far they lie through the turn from zero.</summary>
    public int CompareTo(TrigonometryAngle other) => RawValue.CompareTo(other.RawValue);

    /// <summary>The angle of <paramref name="rawValue"/> table units, 0 through 255.</summary>
    /// <param name="rawValue">The angle in table units, 0 through 255.</param>
    public TrigonometryAngle(int rawValue) => RawValue = checked((byte)rawValue);

    /// <summary>The angle in table units, 0 through 255.</summary>
    public byte RawValue { get; init; }

    /// <summary>
    /// The angle a computation in table units lands on: a turn past a full turn, or below zero, continues around the
    /// circle, as the game's angle arithmetic wraps.
    /// </summary>
    /// <param name="units">The computed angle in table units, which may lie outside one turn.</param>
    public static TrigonometryAngle Wrapped(int units) => new(unchecked((byte)units));

    /// <summary>One quarter turn: up.</summary>
    public static TrigonometryAngle QuarterTurn { get; } = new(0x40);

    /// <summary>One half turn: left.</summary>
    public static TrigonometryAngle HalfTurn { get; } = new(0x80);

    /// <summary>
    /// The index this angle reads in a 128-entry half-wave table: the angle folded onto the first half of the turn, as
    /// the game's sine lookups fold it before restoring the sign from <see cref="IsInSecondHalf"/>.
    /// </summary>
    public int HalfWaveIndex => RawValue % HalfTurn.RawValue;

    /// <summary>Whether this angle lies in the second half of the turn, where the sine wave is negative.</summary>
    public bool IsInSecondHalf => RawValue >= HalfTurn.RawValue;

    /// <summary>Turns the angle by <paramref name="delta"/> signed steps, continuing around the turn.</summary>
    public TrigonometryAngle TurnedBySteps(int delta) => Wrapped(RawValue + delta);

    /// <summary>The angle turned on by <paramref name="turn"/>, wrapping around the full turn.</summary>
    public TrigonometryAngle TurnedBy(TrigonometryAngle turn) => Wrapped(RawValue + turn.RawValue);

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

    /// <summary>This angle as the high byte of an 8.8 <see cref="RotationAngle"/>.</summary>
    public RotationAngle ToAngle() => RotationAngle.FromSteps(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"{RawValue}/256 turn";
}
