namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One wrapping 8-bit turn in the sine-table convention: 256 units per turn, zero pointing right and <c>$40</c> up
/// (negative Y). This is the angle the cartridge's eight-bit trigonometry routines read; <see cref="SnesAngle"/> is the
/// 8.8 form whose high byte is this value.
/// </summary>
[EncodingOwner("sine-table byte angle")]
public readonly record struct SnesByteAngle(byte RawValue)
{
    /// <summary>One quarter turn: up.</summary>
    public static SnesByteAngle QuarterTurn { get; } = new(0x40);

    /// <summary>One half turn: left.</summary>
    public static SnesByteAngle HalfTurn { get; } = new(0x80);

    /// <summary>Adds signed table units with the cartridge's byte wraparound.</summary>
    public SnesByteAngle AddTableUnits(int delta) => new(unchecked((byte)(RawValue + delta)));

    /// <summary>This angle as the high byte of an 8.8 <see cref="SnesAngle"/>.</summary>
    public SnesAngle ToAngle() => SnesAngle.FromTableIndex(RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X2}";
}
