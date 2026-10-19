namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One wrapping 8-bit bearing in the convention of the bank-$A0 angle calculation (<c>CalculateAngleOf_12_14_Offset</c>):
/// zero points up and the bearing advances clockwise, so <c>$40</c> is right and <c>$80</c> down. The trigonometry routines
/// read the other convention, <see cref="SnesByteAngle"/>; <see cref="ToTableAngle"/> converts between them.
/// </summary>
[SnesEncodingOwner]
public readonly record struct SnesBearing(byte RawValue)
{
    /// <summary>The same direction in the sine-table convention: a quarter turn minus the bearing.</summary>
    public SnesByteAngle ToTableAngle() => new(unchecked((byte)(SnesByteAngle.QuarterTurn.RawValue - RawValue)));

    /// <summary>
    /// The bearing's byte read unconverted as a table angle, as several cartridge routines do. The direction this names
    /// is the bearing mirrored about the diagonal, so routines that call this read sine as X and cosine as Y.
    /// </summary>
    public SnesByteAngle ReinterpretAsTableAngle() => new(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X2}";
}
