using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The import's layout of one Crocomire melt image (#1638): the cartridge's tile placements, import data only the import
/// composes his body sheet's melt pictures from; mechanics are not configurable.
/// </summary>
[SpatialComponents("External format: the melting tilemap JSON stores its declared grid dimensions as width/height integers.")]
public sealed record CrocomireMeltingTilemapDocument
{
    /// <summary>Visual tile-layout revision; loading requires <see cref="CrocomireMeltingArtworkFormat.TilemapVersion"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The layout's width in cells, 32: each row of the layout is one row of the image.</summary>
    public required int Width { get; init; }
    /// <summary>The layout's height in cells, 8.</summary>
    public required int Height { get; init; }
    /// <summary>The layout's cells row by row, top first, blank margins included.</summary>
    public required CrocomireMeltingTilemapCell[] Cells { get; init; }
}
