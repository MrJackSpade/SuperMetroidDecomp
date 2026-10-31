namespace SuperMetroid.Core.Spatial;

/// <summary>A signed sixteen-bit displacement within one coordinate space.</summary>
/// <remarks>
/// This type is the one owner of the signed coordinate difference: the distance between two wrapping
/// <see cref="Position16{TSpace}"/> coordinates is their sixteen-bit difference read as signed, so a position that has
/// wrapped past the room's top or left edge still lies a small negative distance away. The game tests the sign of these
/// differences (the camera's overshoot checks, Samus and enemy proximity tests), so the wrap is gameplay-observable
/// and its operators are the only place it is computed (<c>docs/spatial-types.md</c>).
/// </remarks>
/// <typeparam name="TSpace">The coordinate space the displacement is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y words.")]
[EncodingOwner("sixteen-bit signed coordinate difference")]
public readonly record struct Offset16<TSpace>
    where TSpace : ISpatialSpace
{
    /// <summary>The displacement by two amounts that must fit the signed sixteen-bit range, -32768 through 32767.</summary>
    /// <param name="x">The horizontal displacement.</param>
    /// <param name="y">The vertical displacement.</param>
    public Offset16(int x, int y)
    {
        X = checked((short)x);
        Y = checked((short)y);
    }

    /// <summary>The horizontal displacement, -32768 through 32767.</summary>
    public short X { get; init; }

    /// <summary>The vertical displacement, -32768 through 32767.</summary>
    public short Y { get; init; }

    /// <summary>No displacement.</summary>
    public static Offset16<TSpace> Zero => default;

    /// <summary>The displacement by two amounts that must fit the signed sixteen-bit range, -32768 through 32767.</summary>
    public static Offset16<TSpace> Of(int x, int y) => new(x, y);

    /// <summary>
    /// The displacement a computation lands on: each computed amount is wrapped to sixteen bits and read as signed
    /// (<see cref="CoordinateAxis.AsSigned"/>), so an amount past either end of the signed range continues from the
    /// other end.
    /// </summary>
    public static Offset16<TSpace> Wrapped(int x, int y) => new(CoordinateAxis.AsSigned(x), CoordinateAxis.AsSigned(y));

    /// <summary>Replaces the X axis with a displacement that must fit the signed sixteen-bit range.</summary>
    public Offset16<TSpace> WithX(int x) => this with { X = checked((short)x) };

    /// <summary>Replaces the Y axis with a displacement that must fit the signed sixteen-bit range.</summary>
    public Offset16<TSpace> WithY(int y) => this with { Y = checked((short)y) };

    /// <summary>Mirrors about the vertical axis: X negates (wrapping at sixteen bits), Y is unchanged.</summary>
    public Offset16<TSpace> MirroredX() => Wrapped(-X, Y);

    /// <summary>Adds two displacements, wrapping each axis at sixteen bits.</summary>
    public static Offset16<TSpace> operator +(Offset16<TSpace> left, Offset16<TSpace> right) =>
        Wrapped(left.X + right.X, left.Y + right.Y);

    /// <summary>Reverses the displacement, wrapping at sixteen bits.</summary>
    public static Offset16<TSpace> operator -(Offset16<TSpace> offset) =>
        Wrapped(-offset.X, -offset.Y);
}
