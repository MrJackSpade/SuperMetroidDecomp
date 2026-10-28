namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A rotation angle in 8.8 turn units that wraps at a full turn: 256 table units per revolution, each with a
/// 1/256th fraction. The shared 256-entry sine table reads <see cref="TableIndex"/>, the whole units; keeping the
/// fraction lets grapple and neck motion turn at sub-unit precision while byte-angle callers share the same named
/// turns.
/// </summary>
[EncodingOwner("8.8 rotation angle word")]
public readonly record struct RotationAngle : IComparable<RotationAngle>
{
    /// <summary>Number of high-byte sine-table units in one complete turn.</summary>
    public const int TableUnitsPerTurn = 0x100;

    /// <summary>Retains the complete native angle word, including its fractional low byte.</summary>
    private RotationAngle(ushort rawValue) => RawValue = rawValue;

    /// <summary>The complete wrapping 8.8-turn word used by native state.</summary>
    public ushort RawValue { get; }

    /// <summary>The high-byte index consumed by 256-entry trigonometry tables.</summary>
    public byte TableIndex => unchecked((byte)(EightEight.WholeOf(RawValue)));

    /// <summary>The high byte as the eight-bit angle the trigonometry routines read.</summary>
    public ByteAngle ToByteAngle() => new(TableIndex);

    /// <summary>
    /// This angle's 1/256th fraction read as a table angle: what an eight-bit routine handed the whole angle word
    /// reads, as the Fireflea initializer does before its main AI reads the table index.
    /// </summary>
    public ByteAngle ReinterpretFractionAsTableAngle() => new(unchecked((byte)RawValue));

    /// <summary>Zero turns.</summary>
    public static RotationAngle Zero { get; } = new(0x0000);

    /// <summary>One quarter turn (<c>$40</c> table units / <c>$4000</c> raw).</summary>
    public static RotationAngle QuarterTurn { get; } = new(0x4000);

    /// <summary>One half turn (<c>$80</c> table units / <c>$8000</c> raw).</summary>
    public static RotationAngle HalfTurn { get; } = new(0x8000);

    /// <summary>Three quarters of a turn (<c>$C0</c> table units / <c>$C000</c> raw).</summary>
    public static RotationAngle ThreeQuarterTurn { get; } = new(0xc000);

    /// <summary>Preserves a native 8.8 angle word exactly.</summary>
    public static RotationAngle FromRaw(ushort rawValue) => new(rawValue);

    /// <summary>Promotes a 256-unit turn value into the high byte of native angle state.</summary>
    public static RotationAngle FromTableIndex(byte tableIndex) =>
        new(unchecked((ushort)(tableIndex << 8)));

    /// <summary>A 256-unit turn value with a fractional low byte, in 1/256 table units.</summary>
    public static RotationAngle FromTableIndex(byte tableIndex, byte fraction) =>
        new(unchecked((ushort)(tableIndex << 8 | fraction)));

    /// <summary>Normalizes an arbitrary raw angle through native 16-bit wraparound.</summary>
    public static RotationAngle NormalizeRaw(int rawValue) => new(unchecked((ushort)rawValue));

    /// <summary>Normalizes an arbitrary 256-unit turn value through native byte wraparound.</summary>
    public static RotationAngle NormalizeTableIndex(int tableIndex) =>
        FromTableIndex(unchecked((byte)tableIndex));

    /// <summary>Adds a signed native 8.8 delta with cartridge word wraparound.</summary>
    public RotationAngle AddRaw(int delta) => NormalizeRaw(RawValue + delta);

    /// <summary>Adds whole sine-table units while retaining the fractional low byte.</summary>
    public RotationAngle AddTableUnits(int delta) => AddRaw(delta << 8);

    /// <summary>
    /// Which of <paramref name="sectors"/> equal arcs of the turn, numbered from zero at <see cref="Zero"/>, holds the
    /// angle's table index (#1461): for a power of two, the index's top bits, which a pose or heading choice reads.
    /// </summary>
    /// <param name="sectors">The number of arcs the turn divides into, 1 through <see cref="TableUnitsPerTurn"/>.</param>
    public int Sector(int sectors) =>
        TableIndex * Ensure.BetweenInclusive(1, TableUnitsPerTurn, sectors) / TableUnitsPerTurn;

    /// <inheritdoc />
    public int CompareTo(RotationAngle other) => RawValue.CompareTo(other.RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X4} (table ${TableIndex:X2})";
}
