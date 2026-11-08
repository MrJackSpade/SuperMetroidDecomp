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
    /// <summary>Number of whole sine-table units in one complete turn.</summary>
    public const int TableUnitsPerTurn = 256;

    /// <summary>Number of 1/256th fractions in one sine-table unit.</summary>
    private const int FractionsPerTableUnit = 256;

    /// <summary>Holds the angle in 1/256ths of a table unit, 0 through 65535: whole units and their fraction.</summary>
    private RotationAngle(ushort rawValue) => RawValue = rawValue;

    /// <summary>The angle in 1/256ths of a table unit, 0 through 65535; a full turn wraps to zero.</summary>
    public ushort RawValue { get; }

    /// <summary>The high-byte index consumed by 256-entry trigonometry tables.</summary>
    public byte TableIndex => unchecked((byte)(EightEight.WholeOf(RawValue)));

    /// <summary>The high byte as the eight-bit angle the trigonometry routines read.</summary>
    public ByteAngle ToByteAngle() => new(TableIndex);

    /// <summary>The angle in radians, fraction included: a half turn is pi, and the sine table's quarter turn is pi / 2.</summary>
    public double Radians => RawValue * Math.PI / HalfTurn.RawValue;

    /// <summary>Zero turns: the sine table's zero angle.</summary>
    public static RotationAngle Zero { get; } = FromTableIndex(0);

    /// <summary>One quarter turn on from <see cref="Zero"/>: where the sine table peaks.</summary>
    public static RotationAngle QuarterTurn { get; } = FromTableIndex(TableUnitsPerTurn / 4);

    /// <summary>One half turn on from <see cref="Zero"/>: the opposite direction.</summary>
    public static RotationAngle HalfTurn { get; } = FromTableIndex(TableUnitsPerTurn / 2);

    /// <summary>Three quarters of a turn on from <see cref="Zero"/>: where the sine table is lowest.</summary>
    public static RotationAngle ThreeQuarterTurn { get; } = FromTableIndex(3 * TableUnitsPerTurn / 4);

    /// <summary>The angle of exactly <paramref name="rawValue"/> 1/256ths of a table unit, 0 through 65535.</summary>
    public static RotationAngle FromRaw(int rawValue) => new(checked((ushort)rawValue));

    /// <summary>The angle of <paramref name="tableIndex"/> whole table units, 0 through 255, with no fraction.</summary>
    public static RotationAngle FromTableIndex(int tableIndex) => FromTableIndex(tableIndex, 0);

    /// <summary>The angle of <paramref name="tableIndex"/> whole table units and <paramref name="fraction"/> 256ths, each 0 through 255.</summary>
    public static RotationAngle FromTableIndex(int tableIndex, int fraction) =>
        new(checked((ushort)(Ensure.BetweenInclusive(0, TableUnitsPerTurn - 1, tableIndex) * FractionsPerTableUnit +
            Ensure.BetweenInclusive(0, FractionsPerTableUnit - 1, fraction))));

    /// <summary>The angle a computation in 1/256ths of a table unit lands on: past a full turn, or below zero, it continues around the circle.</summary>
    public static RotationAngle NormalizeRaw(int rawValue) => new(unchecked((ushort)rawValue));

    /// <summary>The angle a computation in whole table units lands on, continuing around the circle, with no fraction.</summary>
    public static RotationAngle NormalizeTableIndex(int tableIndex) =>
        FromTableIndex(unchecked((byte)tableIndex), 0);

    /// <summary>Turns the angle by <paramref name="delta"/> 1/256ths of a table unit, continuing around the circle.</summary>
    public RotationAngle AddRaw(int delta) => NormalizeRaw(RawValue + delta);

    /// <summary>
    /// The turn from this angle on to <paramref name="other"/>, in 1/256ths of a table unit, 0 through 65535: the arc
    /// in the direction of increasing angle, so an angle just behind this one lies almost a full turn on.
    /// </summary>
    public int UnitsTo(RotationAngle other) => NormalizeRaw(other.RawValue - RawValue).RawValue;

    /// <summary>Adds whole sine-table units while retaining the fractional low byte.</summary>
    public RotationAngle AddTableUnits(int delta) => AddRaw(delta << 8);

    /// <summary>
    /// Which of <paramref name="sectors"/> equal arcs of the turn, numbered from zero at <see cref="Zero"/>, holds the
    /// angle's table index (#1461): for a power of two, the index's top bits, which a pose or heading choice reads.
    /// </summary>
    /// <param name="sectors">The number of arcs the turn divides into, 1 through <see cref="TableUnitsPerTurn"/>.</param>
    public int Sector(int sectors) =>
        TableIndex * Ensure.BetweenInclusive(1, TableUnitsPerTurn, sectors) / TableUnitsPerTurn;

    /// <summary>The angle turned on by <paramref name="turn"/>, continuing around the circle past a full turn.</summary>
    public static RotationAngle operator +(RotationAngle angle, RotationAngle turn) => angle.AddRaw(turn.RawValue);

    /// <summary>The angle turned back by <paramref name="turn"/>, continuing around the circle below zero.</summary>
    public static RotationAngle operator -(RotationAngle angle, RotationAngle turn) => angle.AddRaw(-turn.RawValue);

    /// <summary>Whether <paramref name="left"/> lies earlier in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator <(RotationAngle left, RotationAngle right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies later in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator >(RotationAngle left, RotationAngle right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> lies no later in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator <=(RotationAngle left, RotationAngle right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> lies no earlier in the turn from <see cref="Zero"/> than <paramref name="right"/>.</summary>
    public static bool operator >=(RotationAngle left, RotationAngle right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public int CompareTo(RotationAngle other) => RawValue.CompareTo(other.RawValue);

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X4} (table ${TableIndex:X2})";
}
