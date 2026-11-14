namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>A slope block's geometry: which slope shape it has, and how the shape is mirrored.</summary>
public readonly record struct SlopeGeometry
{
    /// <summary>A slope of <paramref name="shape"/> mirrored by <paramref name="mirroring"/>.</summary>
    /// <param name="shape">The shape, one of <see cref="SlopeShapeDefinitions.All"/>.</param>
    /// <param name="mirroring">How the shape is mirrored.</param>
    /// <exception cref="ArgumentNullException"><paramref name="shape"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mirroring"/> is undefined.</exception>
    public SlopeGeometry(SlopeShape shape, BlockMirroring mirroring)
    {
        Shape = Ensure.NotNull(shape);
        Mirroring = mirroring is BlockMirroring.None or BlockMirroring.Horizontal or BlockMirroring.Vertical or BlockMirroring.Both
            ? mirroring
            : throw new ArgumentOutOfRangeException(nameof(mirroring), mirroring, "Undefined slope mirroring.");
    }

    /// <summary>The slope's shape.</summary>
    public SlopeShape Shape { get; }

    /// <summary>How the shape is mirrored.</summary>
    public BlockMirroring Mirroring { get; }

    /// <summary>Whether the shape's surface is a height profile rather than solid quarters.</summary>
    public bool IsNonSquare => !Shape.IsSquare;

    /// <summary>Whether the shape is reflected left to right.</summary>
    public bool FlipsHorizontally => Mirroring.HasFlag(BlockMirroring.Horizontal);

    /// <summary>Whether the shape is reflected top to bottom.</summary>
    public bool FlipsVertically => Mirroring.HasFlag(BlockMirroring.Vertical);

    /// <summary>Names the shape and its mirroring.</summary>
    public override string ToString() => $"{Shape} ({Mirroring})";
}
