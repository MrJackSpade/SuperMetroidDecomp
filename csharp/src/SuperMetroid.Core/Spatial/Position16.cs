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
