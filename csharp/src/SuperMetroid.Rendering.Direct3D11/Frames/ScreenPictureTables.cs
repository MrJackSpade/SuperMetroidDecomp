using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frontend layers whose character tiles a frame draws (#1392), each once; equal to another holding the same layers'
/// tiles in the same order, so an unchanged screen keeps its packing.
/// </summary>
/// <param name="layers">The layers' tiles.</param>
/// <param name="mode7">The Mode 7 maps' tiles.</param>
internal sealed class ScreenPictureTables(ImmutableArray<ScreenLayerPictures> layers, ImmutableArray<Mode7TilePictures> mode7) : IEquatable<ScreenPictureTables>
{
    /// <summary>The layers' tiles.</summary>
    public ImmutableArray<ScreenLayerPictures> Layers { get; } = layers;

    /// <summary>The Mode 7 maps' tiles.</summary>
    public ImmutableArray<Mode7TilePictures> Mode7 { get; } = mode7;

    /// <inheritdoc />
    public bool Equals(ScreenPictureTables? other) => other is not null && Layers.SequenceEqual(other.Layers) && Mode7.SequenceEqual(other.Mode7);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenPictureTables);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (ScreenLayerPictures layer in Layers)
            hash.Add(layer);
        foreach (Mode7TilePictures tiles in Mode7)
            hash.Add(tiles);
        return hash.ToHashCode();
    }
}
