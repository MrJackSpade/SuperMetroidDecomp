namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>A slope block's geometry: which of the slope shapes it has, and how the shape is mirrored.</summary>
public readonly record struct SlopeGeometry
{
    /// <summary>The last slope shape.</summary>
    public const int LastShape = 31;

    /// <summary>The first shape whose surface is a height profile; the square shapes before it are made of solid quarters.</summary>
    public const int FirstNonSquareShape = 5;

    /// <summary>A slope of <paramref name="shape"/> mirrored by <paramref name="mirroring"/>.</summary>
    /// <param name="shape">The shape, 0 through <see cref="LastShape"/>: the shape <see cref="SlopeHeights"/> and the square slopes read.</param>
    /// <param name="mirroring">How the shape is mirrored.</param>
    public SlopeGeometry(int shape, BlockMirroring mirroring)
    {
        Shape = Ensure.BetweenInclusive(0, LastShape, shape);
        Mirroring = mirroring is BlockMirroring.None or BlockMirroring.Horizontal or BlockMirroring.Vertical or BlockMirroring.Both
            ? mirroring
            : throw new ArgumentOutOfRangeException(nameof(mirroring), mirroring, "Undefined slope mirroring.");
    }

    /// <summary>The slope's shape.</summary>
    public int Shape { get; }

    /// <summary>How the shape is mirrored.</summary>
    public BlockMirroring Mirroring { get; }

    /// <summary>Whether the shape's surface is a height profile rather than solid quarters.</summary>
    public bool IsNonSquare => Shape >= FirstNonSquareShape;

    /// <summary>Whether the shape is reflected left to right.</summary>
    public bool FlipsHorizontally => Mirroring.HasFlag(BlockMirroring.Horizontal);

    /// <summary>Whether the shape is reflected top to bottom.</summary>
    public bool FlipsVertically => Mirroring.HasFlag(BlockMirroring.Vertical);

    /// <summary>Names the shape and its mirroring.</summary>
    public override string ToString() => $"shape {Shape} ({Mirroring})";
}
