namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One wrapping 16-bit SNES turn. A complete revolution is <c>$10000</c>; routines that
/// use the shared 256-entry sine table consume <see cref="TableIndex"/>, the high byte.
/// Keeping the fractional low byte allows grapple and neck motion to retain native 8.8
/// angular precision while byte-angle callers share the same domain and named turns.
/// </summary>
public readonly record struct SnesAngle : IComparable<SnesAngle>
{
    /// <summary>Number of high-byte sine-table units in one complete turn.</summary>
    public const int TableUnitsPerTurn = 0x100;

    /// <summary>Number of native 8.8 units in one complete turn before word wrapping.</summary>
    public const int RawUnitsPerTurn = 0x1_0000;

    private SnesAngle(ushort rawValue) => RawValue = rawValue;

    /// <summary>The complete wrapping 8.8-turn word used by native state.</summary>
    public ushort RawValue { get; }

    /// <summary>The high-byte index consumed by 256-entry trigonometry tables.</summary>
    public byte TableIndex => unchecked((byte)(RawValue >> 8));

    /// <summary>The byte offset for a table containing one 16-bit sample per angle.</summary>
    public int SineTableByteOffset => TableIndex * sizeof(ushort);

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

    /// <summary>Normalizes an arbitrary raw angle through native 16-bit wraparound.</summary>
    public static SnesAngle NormalizeRaw(int rawValue) => new(unchecked((ushort)rawValue));

    /// <summary>Normalizes an arbitrary 256-unit turn value through native byte wraparound.</summary>
    public static SnesAngle NormalizeTableIndex(int tableIndex) =>
        FromTableIndex(unchecked((byte)tableIndex));

    /// <summary>Adds a signed native 8.8 delta with cartridge word wraparound.</summary>
    public SnesAngle AddRaw(int delta) => NormalizeRaw(RawValue + delta);

    /// <summary>Adds whole sine-table units while retaining the fractional low byte.</summary>
    public SnesAngle AddTableUnits(int delta) => AddRaw(delta << 8);

    /// <summary>
    /// Returns the native signed shortest delta from this angle to <paramref name="target"/>.
    /// The exactly-opposite case is <see cref="short.MinValue"/>, matching 65816 subtraction.
    /// </summary>
    public short SignedDeltaTo(SnesAngle target) =>
        unchecked((short)(target.RawValue - RawValue));

    /// <summary>
    /// Returns the signed high-byte delta used by byte-angle dispatchers. The opposite
    /// direction is <see cref="sbyte.MinValue"/>, matching native eight-bit truncation.
    /// </summary>
    public sbyte SignedTableDeltaTo(SnesAngle target) =>
        unchecked((sbyte)(target.TableIndex - TableIndex));

    /// <inheritdoc />
    public int CompareTo(SnesAngle other) => RawValue.CompareTo(other.RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X4} (table ${TableIndex:X2})";
}
