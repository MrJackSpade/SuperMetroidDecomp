namespace SuperMetroid.Core.Spatial;

/// <summary>An unsigned sixteen-bit size or half-extent (radius) within one coordinate space.</summary>
/// <remarks>
/// This type is the one owner of the spatial family's size representation: sizes and radii share the sixteen-bit
/// axes of <see cref="Position16{TSpace}"/> (<c>docs/spatial-types.md</c>, "Shapes"), so containment and grid
/// indexing compare a position with an extent in the same wrapped coordinate words.
/// </remarks>
/// <typeparam name="TSpace">The coordinate space the extent is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y extents.")]
[SnesEncodingOwner]
public readonly record struct Extent16<TSpace>(ushort X, ushort Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No extent.</summary>
    public static Extent16<TSpace> Zero => default;

    /// <summary>Replaces the X extent.</summary>
    public Extent16<TSpace> WithX(ushort x) => this with { X = x };

    /// <summary>Replaces the Y extent.</summary>
    public Extent16<TSpace> WithY(ushort y) => this with { Y = y };

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
