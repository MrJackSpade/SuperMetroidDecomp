namespace SuperMetroid.Core.Spatial;

/// <summary>A signed sixteen-bit displacement within one coordinate space.</summary>
/// <typeparam name="TSpace">The coordinate space the displacement is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y words.")]
public readonly record struct Offset16<TSpace>(short X, short Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No displacement.</summary>
    public static Offset16<TSpace> Zero => default;

    /// <summary>Replaces the X axis.</summary>
    public Offset16<TSpace> WithX(short x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public Offset16<TSpace> WithY(short y) => this with { Y = y };

    /// <summary>Mirrors about the vertical axis: X negates (wrapping at sixteen bits), Y is unchanged.</summary>
    public Offset16<TSpace> MirroredX() => new(unchecked((short)-X), Y);

    /// <summary>Adds two displacements, wrapping each axis at sixteen bits.</summary>
    public static Offset16<TSpace> operator +(Offset16<TSpace> left, Offset16<TSpace> right) =>
        new(unchecked((short)(left.X + right.X)), unchecked((short)(left.Y + right.Y)));

    /// <summary>Reverses the displacement, wrapping at sixteen bits.</summary>
    public static Offset16<TSpace> operator -(Offset16<TSpace> offset) =>
        new(unchecked((short)-offset.X), unchecked((short)-offset.Y));
}
