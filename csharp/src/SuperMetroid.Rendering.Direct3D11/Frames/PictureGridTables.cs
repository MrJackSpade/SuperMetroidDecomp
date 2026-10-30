using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The picture grids a frame draws (#1467), each once; equal to another holding equal grids in the same order, so an
/// unchanged map keeps its packing.
/// </summary>
/// <param name="grids">The grids.</param>
internal sealed class PictureGridTables(ImmutableArray<PictureGrid> grids) : IEquatable<PictureGridTables>
{
    /// <summary>The grids.</summary>
    public ImmutableArray<PictureGrid> Grids { get; } = grids;

    /// <inheritdoc />
    public bool Equals(PictureGridTables? other) => other is not null && Grids.SequenceEqual(other.Grids);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PictureGridTables);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PictureGrid grid in Grids)
            hash.Add(grid);
        return hash.ToHashCode();
    }
}
