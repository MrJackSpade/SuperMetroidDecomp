using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frontend tile layers a frame draws (#1392, #1403): their cells and tile sets, each once; equal to another holding
/// the same cells and tiles in the same order, so an unchanged screen keeps its packing.
/// </summary>
/// <param name="maps">The layers' cells.</param>
/// <param name="mode7">The Mode 7 maps' tiles.</param>
internal sealed class ScreenPictureTables(ImmutableArray<ScreenTileMap> maps, ImmutableArray<Mode7TilePictures> mode7) : IEquatable<ScreenPictureTables>
{
    /// <summary>The layers' cells.</summary>
    public ImmutableArray<ScreenTileMap> Maps { get; } = maps;

    /// <summary>The tile sets the layers' cells show, each once.</summary>
    public ImmutableArray<ScreenLayerPictures> Layers { get; } = [.. maps.Select(map => map.Tiles).Distinct()];

    /// <summary>The Mode 7 maps' tiles.</summary>
    public ImmutableArray<Mode7TilePictures> Mode7 { get; } = mode7;

    /// <inheritdoc />
    public bool Equals(ScreenPictureTables? other) => other is not null && Maps.SequenceEqual(other.Maps) && Mode7.SequenceEqual(other.Mode7);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenPictureTables);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (ScreenTileMap map in Maps)
            hash.Add(map);
        foreach (Mode7TilePictures tiles in Mode7)
            hash.Add(tiles);
        return hash.ToHashCode();
    }
}
