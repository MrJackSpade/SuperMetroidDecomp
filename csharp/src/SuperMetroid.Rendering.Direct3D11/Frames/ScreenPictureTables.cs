using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frontend layers whose character tiles a frame draws (#1392), each once; equal to another holding the same layers'
/// tiles in the same order, so an unchanged screen keeps its packing.
/// </summary>
/// <param name="layers">The layers' tiles.</param>
internal sealed class ScreenPictureTables(ImmutableArray<ScreenLayerPictures> layers) : IEquatable<ScreenPictureTables>
{
    /// <summary>The layers' tiles.</summary>
    public ImmutableArray<ScreenLayerPictures> Layers { get; } = layers;

    /// <inheritdoc />
    public bool Equals(ScreenPictureTables? other) => other is not null && Layers.SequenceEqual(other.Layers);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenPictureTables);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (ScreenLayerPictures layer in Layers)
            hash.Add(layer);
        return hash.ToHashCode();
    }
}
