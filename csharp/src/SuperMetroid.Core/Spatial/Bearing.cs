namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A compass bearing of 256 units per turn, as the enemies' aim-at calculation produces it: zero points up and the
/// bearing advances clockwise, so <c>$40</c> is right and <c>$80</c> down. The sine and cosine lookups take the other
/// convention, <see cref="ByteAngle"/>; <see cref="ToTableAngle"/> converts between them.
/// </summary>
[EncodingOwner("clockwise byte bearing")]
public readonly record struct Bearing(byte RawValue)
{
    /// <summary>The same direction in the sine-table convention: a quarter turn minus the bearing.</summary>
    public ByteAngle ToTableAngle() => new(unchecked((byte)(ByteAngle.QuarterTurn.RawValue - RawValue)));

    /// <summary>
    /// The bearing's byte read unconverted as a table angle, as several cartridge routines do. The direction this names
    /// is the bearing mirrored about the diagonal, so routines that call this read sine as X and cosine as Y.
    /// </summary>
    public ByteAngle ReinterpretAsTableAngle() => new(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X2}";
}
