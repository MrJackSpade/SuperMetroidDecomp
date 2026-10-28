namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An angle of 256 units per turn that wraps at a full turn, in the sine-table convention: zero points right and
/// <c>$40</c> up (negative Y). This is the angle the game's sine and cosine lookups take; <see cref="RotationAngle"/>
/// is the form with a fraction, whose whole units are this value.
/// </summary>
[EncodingOwner("sine-table byte angle")]
public readonly record struct ByteAngle(byte RawValue)
{
    /// <summary>One quarter turn: up.</summary>
    public static ByteAngle QuarterTurn { get; } = new(0x40);

    /// <summary>One half turn: left.</summary>
    public static ByteAngle HalfTurn { get; } = new(0x80);

    /// <summary>Adds signed table units with the cartridge's byte wraparound.</summary>
    public ByteAngle AddTableUnits(int delta) => new(unchecked((byte)(RawValue + delta)));

    /// <summary>
    /// This angle carried into the half turn from zero up to (not including) <see cref="HalfTurn"/>, keeping its
    /// position within the half turn: an angle in the other half turns back by a half turn.
    /// </summary>
    public ByteAngle InFirstHalfTurn() => new((byte)(RawValue % HalfTurn.RawValue));

    /// <summary>
    /// This angle carried into the half turn from <see cref="HalfTurn"/> up to a full turn, keeping its position within
    /// the half turn: an angle in the other half turns on by a half turn.
    /// </summary>
    public ByteAngle InSecondHalfTurn() => new((byte)(RawValue % HalfTurn.RawValue + HalfTurn.RawValue));

    /// <summary>This angle as the high byte of an 8.8 <see cref="RotationAngle"/>.</summary>
    public RotationAngle ToAngle() => RotationAngle.FromTableIndex(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X2}";
}
