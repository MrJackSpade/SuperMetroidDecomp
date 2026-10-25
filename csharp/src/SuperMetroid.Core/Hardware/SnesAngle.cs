namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One wrapping 16-bit SNES turn. A complete revolution is <c>$10000</c>; routines that
/// use the shared 256-entry sine table consume <see cref="TableIndex"/>, the high byte.
/// Keeping the fractional low byte allows grapple and neck motion to retain native 8.8
/// angular precision while byte-angle callers share the same domain and named turns.
/// </summary>
[EncodingOwner("8.8 rotation angle word")]
public readonly record struct SnesAngle : IComparable<SnesAngle>
{
    /// <summary>Number of high-byte sine-table units in one complete turn.</summary>
    public const int TableUnitsPerTurn = 0x100;

    /// <summary>Retains the complete native angle word, including its fractional low byte.</summary>
    private SnesAngle(ushort rawValue) => RawValue = rawValue;

    /// <summary>The complete wrapping 8.8-turn word used by native state.</summary>
    public ushort RawValue { get; }

    /// <summary>The high-byte index consumed by 256-entry trigonometry tables.</summary>
    public byte TableIndex => unchecked((byte)(SnesEightEight.WholeOf(RawValue)));

    /// <summary>The high byte as the eight-bit angle the trigonometry routines read.</summary>
    public SnesByteAngle ToByteAngle() => new(TableIndex);

    /// <summary>Zero turns.</summary>
    public static SnesAngle Zero { get; } = new(0x0000);

    /// <summary>One quarter turn (<c>$40</c> table units / <c>$4000</c> raw).</summary>
    public static SnesAngle QuarterTurn { get; } = new(0x4000);

    /// <summary>One half turn (<c>$80</c> table units / <c>$8000</c> raw).</summary>
    public static SnesAngle HalfTurn { get; } = new(0x8000);

    /// <summary>Three quarters of a turn (<c>$C0</c> table units / <c>$C000</c> raw).</summary>
    public static SnesAngle ThreeQuarterTurn { get; } = new(0xc000);

    /// <summary>Preserves a native 8.8 angle word exactly.</summary>
    public static SnesAngle FromRaw(ushort rawValue) => new(rawValue);

    /// <summary>Promotes a 256-unit turn value into the high byte of native angle state.</summary>
    public static SnesAngle FromTableIndex(byte tableIndex) =>
        new(unchecked((ushort)(tableIndex << 8)));

    /// <summary>A 256-unit turn value with a fractional low byte, in 1/256 table units.</summary>
    public static SnesAngle FromTableIndex(byte tableIndex, byte fraction) =>
        new(unchecked((ushort)(tableIndex << 8 | fraction)));

    /// <summary>Normalizes an arbitrary raw angle through native 16-bit wraparound.</summary>
    public static SnesAngle NormalizeRaw(int rawValue) => new(unchecked((ushort)rawValue));

    /// <summary>Normalizes an arbitrary 256-unit turn value through native byte wraparound.</summary>
    public static SnesAngle NormalizeTableIndex(int tableIndex) =>
        FromTableIndex(unchecked((byte)tableIndex));

    /// <summary>Adds a signed native 8.8 delta with cartridge word wraparound.</summary>
    public SnesAngle AddRaw(int delta) => NormalizeRaw(RawValue + delta);

    /// <summary>Adds whole sine-table units while retaining the fractional low byte.</summary>
    public SnesAngle AddTableUnits(int delta) => AddRaw(delta << 8);

    /// <inheritdoc />
    public int CompareTo(SnesAngle other) => RawValue.CompareTo(other.RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X4} (table ${TableIndex:X2})";
}
