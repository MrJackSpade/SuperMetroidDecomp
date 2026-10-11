namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A host-side signed integer coordinate or displacement: a grid cell, an image pixel or an authored
/// placement whose arithmetic is ordinary non-wrapping <see cref="int"/> arithmetic. Native sixteen-bit
/// WRAM words use <see cref="Position16{TSpace}"/>/<see cref="Offset16{TSpace}"/> instead.
/// </summary>
/// <typeparam name="TSpace">The coordinate space; points in different spaces never convert implicitly.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y integers.")]
public readonly record struct Point32<TSpace>(int X, int Y)
    where TSpace : ISpatialSpace
{
    /// <summary>The origin.</summary>
    public static Point32<TSpace> Zero => default;

    /// <summary>Replaces the X axis.</summary>
    public Point32<TSpace> WithX(int x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public Point32<TSpace> WithY(int y) => this with { Y = y };

    /// <summary>Translates by another point used as a displacement.</summary>
    public static Point32<TSpace> operator +(Point32<TSpace> left, Point32<TSpace> right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>The displacement from <paramref name="right"/> to <paramref name="left"/>.</summary>
    public static Point32<TSpace> operator -(Point32<TSpace> left, Point32<TSpace> right) =>
        new(left.X - right.X, left.Y - right.Y);

    /// <summary>Reverses the displacement.</summary>
    public static Point32<TSpace> operator -(Point32<TSpace> point) => new(-point.X, -point.Y);

    /// <summary>The same coordinate as native sixteen-bit words, keeping each axis's low word (two's complement for negatives).</summary>
    public Position16<TSpace> ToPosition16() => new(unchecked((ushort)X), unchecked((ushort)Y));

    /// <summary>The same coordinate as native sixteen-bit words, failing if either axis lies outside 0..65535.</summary>
    public Position16<TSpace> ToExactPosition16() => new(checked((ushort)X), checked((ushort)Y));

    /// <summary>The same displacement as native signed sixteen-bit words, keeping each axis's low word.</summary>
    public Offset16<TSpace> ToOffset16() => new(unchecked((short)X), unchecked((short)Y));
}

/// <summary>A host-side integer width and height: an image, a grid or a rectangle size.</summary>
/// <typeparam name="TSpace">The coordinate space the size is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its width and height.")]
public readonly record struct Size32<TSpace>(int Width, int Height)
    where TSpace : ISpatialSpace
{
    /// <summary>No size.</summary>
    public static Size32<TSpace> Zero => default;

    /// <summary>The number of cells in a row-major grid of this size, failing on signed overflow.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int Area => checked(Width * Height);

    /// <summary>Whether <paramref name="point"/> lies inside a grid of this size anchored at the origin.</summary>
    public bool Contains(Point32<TSpace> point) =>
        (uint)point.X < (uint)Width && (uint)point.Y < (uint)Height;

    /// <summary>The row-major index of <paramref name="point"/>, without a bounds check.</summary>
    public int RowMajorIndex(Point32<TSpace> point) => point.Y * Width + point.X;

    /// <summary>The point at a row-major index in a grid of this width.</summary>
    public Point32<TSpace> PointAtRowMajorIndex(int index) => new(index % Width, index / Width);
}
