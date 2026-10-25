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
[SnesEncodingOwner]
public readonly record struct Offset16<TSpace>(short X, short Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No displacement.</summary>
    public static Offset16<TSpace> Zero => default;

    /// <summary>Replaces the X axis with a displacement that must fit the signed sixteen-bit range.</summary>
    public Offset16<TSpace> WithX(int x) => this with { X = checked((short)x) };

    /// <summary>Replaces the Y axis with a displacement that must fit the signed sixteen-bit range.</summary>
    public Offset16<TSpace> WithY(int y) => this with { Y = checked((short)y) };

    /// <summary>Mirrors about the vertical axis: X negates (wrapping at sixteen bits), Y is unchanged.</summary>
    public Offset16<TSpace> MirroredX() => new(unchecked((short)-X), Y);

    /// <summary>Adds two displacements, wrapping each axis at sixteen bits.</summary>
    public static Offset16<TSpace> operator +(Offset16<TSpace> left, Offset16<TSpace> right) =>
        new(unchecked((short)(left.X + right.X)), unchecked((short)(left.Y + right.Y)));

    /// <summary>Reverses the displacement, wrapping at sixteen bits.</summary>
    public static Offset16<TSpace> operator -(Offset16<TSpace> offset) =>
        new(unchecked((short)-offset.X), unchecked((short)-offset.Y));
}
