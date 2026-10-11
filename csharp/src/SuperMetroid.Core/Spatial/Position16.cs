namespace SuperMetroid.Core.Spatial;

/// <summary>An unsigned sixteen-bit position whose axes wrap at the native word boundary.</summary>
/// <typeparam name="TSpace">The coordinate space; positions in different spaces never convert implicitly.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y words.")]
public readonly record struct Position16<TSpace>(ushort X, ushort Y)
    where TSpace : ISpatialSpace
{
    /// <summary>The origin.</summary>
    public static Position16<TSpace> Zero => default;

    /// <summary>Replaces the X axis.</summary>
    public Position16<TSpace> WithX(ushort x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public Position16<TSpace> WithY(ushort y) => this with { Y = y };

    /// <summary>Translates by a signed offset, wrapping each axis at sixteen bits.</summary>
    public static Position16<TSpace> operator +(Position16<TSpace> position, Offset16<TSpace> offset) =>
        new(unchecked((ushort)(position.X + offset.X)), unchecked((ushort)(position.Y + offset.Y)));

    /// <summary>Translates by the negated offset, wrapping each axis at sixteen bits.</summary>
    public static Position16<TSpace> operator -(Position16<TSpace> position, Offset16<TSpace> offset) =>
        new(unchecked((ushort)(position.X - offset.X)), unchecked((ushort)(position.Y - offset.Y)));

    /// <summary>The wrapped signed delta from <paramref name="origin"/> to <paramref name="position"/>.</summary>
    public static Offset16<TSpace> operator -(Position16<TSpace> position, Position16<TSpace> origin) =>
        new(unchecked((short)(position.X - origin.X)), unchecked((short)(position.Y - origin.Y)));
}

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

    /// <summary>Adds two displacements, wrapping each axis at sixteen bits.</summary>
    public static Offset16<TSpace> operator +(Offset16<TSpace> left, Offset16<TSpace> right) =>
        new(unchecked((short)(left.X + right.X)), unchecked((short)(left.Y + right.Y)));

    /// <summary>Subtracts two displacements, wrapping each axis at sixteen bits.</summary>
    public static Offset16<TSpace> operator -(Offset16<TSpace> left, Offset16<TSpace> right) =>
        new(unchecked((short)(left.X - right.X)), unchecked((short)(left.Y - right.Y)));

    /// <summary>Reverses the displacement, wrapping at sixteen bits.</summary>
    public static Offset16<TSpace> operator -(Offset16<TSpace> offset) =>
        new(unchecked((short)-offset.X), unchecked((short)-offset.Y));
}

/// <summary>An unsigned sixteen-bit size or half-extent (radius) within one coordinate space.</summary>
/// <typeparam name="TSpace">The coordinate space the extent is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y extents.")]
public readonly record struct Extent16<TSpace>(ushort X, ushort Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No extent.</summary>
    public static Extent16<TSpace> Zero => default;

    /// <summary>Replaces the X extent.</summary>
    public Extent16<TSpace> WithX(ushort x) => this with { X = x };

    /// <summary>Replaces the Y extent.</summary>
    public Extent16<TSpace> WithY(ushort y) => this with { Y = y };
}
