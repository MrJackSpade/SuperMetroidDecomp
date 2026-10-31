namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A compass bearing of 256 units per turn, as the enemies' aim-at calculation produces it: zero points up and the
/// bearing advances clockwise, so <c>$40</c> is right and <c>$80</c> down. The sine and cosine lookups take the other
/// convention, <see cref="ByteAngle"/>; <see cref="ToTableAngle"/> converts between them.
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
    public ByteAngle ToTableAngle() => ByteAngle.Wrapped(ByteAngle.QuarterTurn.RawValue - RawValue);

    /// <summary>
    /// The bearing's byte read unconverted as a table angle, as several cartridge routines do. The direction this names
    /// is the bearing mirrored about the diagonal, so routines that call this read sine as X and cosine as Y.
    /// </summary>
    public ByteAngle ReinterpretAsTableAngle() => new(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X2}";
}
