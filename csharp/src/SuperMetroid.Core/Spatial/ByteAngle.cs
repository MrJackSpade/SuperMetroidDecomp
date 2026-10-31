namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An angle of 256 units per turn that wraps at a full turn, in the sine-table convention: zero points right and
/// <c>$40</c> up (negative Y). This is the angle the game's sine and cosine lookups take; <see cref="RotationAngle"/>
/// is the form with a fraction, whose whole units are this value.
/// </summary>
[EncodingOwner("sine-table byte angle")]
public readonly record struct ByteAngle
{
    /// <summary>The angle of <paramref name="rawValue"/> table units, 0 through 255.</summary>
    /// <param name="rawValue">The angle in table units, 0 through 255.</param>
    public ByteAngle(int rawValue) => RawValue = checked((byte)rawValue);

    /// <summary>The angle in table units, 0 through 255.</summary>
    public byte RawValue { get; init; }

    /// <summary>
    /// The angle a computation in table units lands on: a turn past a full turn, or below zero, continues around the
    /// circle, as the game's angle arithmetic wraps.
    /// </summary>
    /// <param name="units">The computed angle in table units, which may lie outside one turn.</param>
    public static ByteAngle Wrapped(int units) => new(unchecked((byte)units));

    /// <summary>One quarter turn: up.</summary>
    public static ByteAngle QuarterTurn { get; } = new(0x40);

    /// <summary>One half turn: left.</summary>
    public static ByteAngle HalfTurn { get; } = new(0x80);

    /// <summary>
    /// The index this angle reads in a 128-entry half-wave table: the angle folded onto the first half of the turn, as
    /// the game's sine lookups fold it before restoring the sign from <see cref="IsInSecondHalf"/>.
    /// </summary>
    public int HalfWaveIndex => RawValue % HalfTurn.RawValue;

    /// <summary>Whether this angle lies in the second half of the turn, where the sine wave is negative.</summary>
    public bool IsInSecondHalf => RawValue >= HalfTurn.RawValue;

    /// <summary>Adds signed table units with the cartridge's byte wraparound.</summary>
    public ByteAngle AddTableUnits(int delta) => Wrapped(RawValue + delta);

    /// <summary>
    /// This angle carried into the half turn from zero up to (not including) <see cref="HalfTurn"/>, keeping its
    /// position within the half turn: an angle in the other half turns back by a half turn.
    /// </summary>
    public ByteAngle InFirstHalfTurn() => new(RawValue % HalfTurn.RawValue);

    /// <summary>
    /// This angle carried into the half turn from <see cref="HalfTurn"/> up to a full turn, keeping its position within
    /// the half turn: an angle in the other half turns on by a half turn.
    /// </summary>
    public ByteAngle InSecondHalfTurn() => new(RawValue % HalfTurn.RawValue + HalfTurn.RawValue);

    /// <summary>This angle as the high byte of an 8.8 <see cref="RotationAngle"/>.</summary>
    public RotationAngle ToAngle() => RotationAngle.FromTableIndex(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X2}";
}
