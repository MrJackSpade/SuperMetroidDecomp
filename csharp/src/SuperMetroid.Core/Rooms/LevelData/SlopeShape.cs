using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// One of the shapes a slope block takes (#1805): the height of its surface under each pixel column, how much of a
/// grounded horizontal move crosses it, and how much of a crawler's tangent speed carries along it. The shapes are the
/// catalog's (<see cref="SlopeShapeDefinitions"/>); only the import's codec knows how the game numbers them.
/// </summary>
/// <remarks>
/// A square shape is made of solid quarters, which Samus, her shots and enemies read through the square slopes; every
/// other shape is a height profile they read column by column. Both kinds keep a height under each column.
/// </remarks>
public sealed class SlopeShape
{
    /// <summary>The pixel columns of a block, each with its own surface height.</summary>
    public const int Columns = 16;

    /// <summary>The largest height a column holds: a surface below the block, twenty pixels down from its top.</summary>
    public const int BelowBlockHeight = 20;

    /// <summary>The surface's height under each column.</summary>
    private readonly ImmutableArray<int> heights;

    /// <summary>Names a shape and its profile.</summary>
    /// <param name="name">The shape's name.</param>
    /// <param name="kind">Whether the shape is made of solid quarters or read as a height profile.</param>
    /// <param name="heights">The surface's height under each of the <see cref="Columns"/> columns, 0 through <see cref="BelowBlockHeight"/>.</param>
    /// <param name="groundedSpeedShare">The share of a grounded horizontal move that crosses the shape.</param>
    /// <param name="crawlerTangentShare">The share of a crawler's tangent speed that carries along the shape.</param>
    /// <exception cref="ArgumentException">The name is blank or the profile does not hold one height per column.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A height is out of its range.</exception>
    internal SlopeShape([InstalledKey] string name, SlopeShapeKind kind, ImmutableArray<int> heights, SpeedShare groundedSpeedShare, SpeedShare crawlerTangentShare)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (heights.IsDefault || heights.Length != Columns)
            throw new ArgumentException($"Slope shape {name} needs one height for each of the {Columns} columns.", nameof(heights));
        foreach (int height in heights)
            _ = Ensure.BetweenInclusive(0, BelowBlockHeight, height);
        Name = name;
        IsSquare = kind switch
        {
            SlopeShapeKind.Square => true,
            SlopeShapeKind.Profile => false,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined slope shape kind."),
        };
        this.heights = heights;
        GroundedSpeedShare = groundedSpeedShare;
        CrawlerTangentShare = crawlerTangentShare;
    }

    /// <summary>The shape's name, which the installation writes it as.</summary>
    public string Name { get; }

    /// <summary>Whether the shape is made of solid quarters rather than read as a height profile.</summary>
    public bool IsSquare { get; }

    /// <summary>The share of a grounded horizontal move that crosses the shape.</summary>
    public SpeedShare GroundedSpeedShare { get; }

    /// <summary>The share of a crawler's tangent speed that carries along the shape.</summary>
    public SpeedShare CrawlerTangentShare { get; }

    /// <summary>
    /// The surface's height under <paramref name="column"/>: the pixels from the block's top down to the surface, sixteen for
    /// a column whose surface is the block's bottom edge and <see cref="BelowBlockHeight"/> for a surface below it. Each is a
    /// distance every reader computes with, so no height stands for a missing surface (#2558).
    /// </summary>
    /// <param name="column">The column, 0 through 15, already mirrored for the slope's facing.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="column"/> is not a column of the block.</exception>
    public int HeightAt(int column) => heights[Ensure.BetweenInclusive(0, Columns - 1, column)];

    /// <summary>The shape's name.</summary>
    public override string ToString() => Name;
}
