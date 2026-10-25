namespace SuperMetroid.Core.Spatial;

/// <summary>An unsigned sixteen-bit size or half-extent (radius) within one coordinate space.</summary>
/// <remarks>
/// This type is the one owner of the spatial family's size representation: sizes and radii share the sixteen-bit
/// axes of <see cref="Position16{TSpace}"/> (<c>docs/spatial-types.md</c>, "Shapes"). The wrap is
/// gameplay-observable: a radius is added to and subtracted from wrapping coordinates to find an actor's edges, and
/// some actors store a negated contour as their radius (the Mama Turtle's shell), which the game then compares
/// unsigned as a large extent.
/// </remarks>
/// <typeparam name="TSpace">The coordinate space the extent is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y extents.")]
[EncodingOwner("sixteen-bit wrapping size")]
public readonly record struct Extent16<TSpace>([property: IntegerQuantity("width in pixels")] ushort X, [property: IntegerQuantity("height in pixels")] ushort Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No extent.</summary>
    public static Extent16<TSpace> Zero => default;

    /// <summary>Replaces the X extent with a size that must already be a sixteen-bit word.</summary>
    public Extent16<TSpace> WithX(int x) => this with { X = checked((ushort)x) };

    /// <summary>Replaces the Y extent with a size that must already be a sixteen-bit word.</summary>
    public Extent16<TSpace> WithY(int y) => this with { Y = checked((ushort)y) };

    /// <summary>The number of cells in a row-major grid of this size.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int Area => X * Y;

    /// <summary>Whether <paramref name="cell"/> lies inside a grid of this size anchored at the origin.</summary>
    public bool Contains(Position16<TSpace> cell) => cell.X < X && cell.Y < Y;

    /// <summary>The row-major index of <paramref name="cell"/>, without a bounds check.</summary>
    public int RowMajorIndex(Position16<TSpace> cell) => cell.Y * X + cell.X;

    /// <summary>The cell at a row-major index in a grid of this width.</summary>
    public Position16<TSpace> CellAtRowMajorIndex(int index) => new(checked((ushort)(index % X)), checked((ushort)(index / X)));
}
