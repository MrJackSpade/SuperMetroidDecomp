using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frontend picture layers and picture grids a frame draws (#1467), each once; equal to another holding equal layers
/// and grids in the same order, so an unchanged screen keeps its packing.
/// </summary>
/// <param name="placed">The picture layers' placed pictures.</param>
/// <param name="grids">The picture grids.</param>
internal sealed class ScreenPictureLayers(ImmutableArray<PlacedPictures> placed, ImmutableArray<PictureGrid> grids) : IEquatable<ScreenPictureLayers>
{
    /// <summary>The picture layers' placed pictures.</summary>
    public ImmutableArray<PlacedPictures> Placed { get; } = placed;

    /// <summary>The picture grids.</summary>
    public ImmutableArray<PictureGrid> Grids { get; } = grids;

    /// <inheritdoc />
    public bool Equals(ScreenPictureLayers? other) => other is not null && Placed.SequenceEqual(other.Placed) && Grids.SequenceEqual(other.Grids);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenPictureLayers);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PlacedPictures layer in Placed)
            hash.Add(layer);
        foreach (PictureGrid grid in Grids)
            hash.Add(grid);
        return hash.ToHashCode();
    }
}
