using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One of a boss body frame's horizontal tile runs as the cartridge writes it, in 32-by-64 tilemap coordinates: import
/// data only the import composes the frame's picture from (#1638).
/// </summary>
[SpatialComponents("External format: the BG2 frame JSON stores each write destination as x/y tilemap cells.")]
public sealed record EnemyBg2WriteDocument
{
    /// <summary>Gets the zero-based destination column in the 32-tile-wide BG2 map.</summary>
    public required int X { get; init; }

    /// <summary>Gets the zero-based destination row in the 64-tile-high BG2 map.</summary>
    public required int Y { get; init; }

    /// <summary>Gets the tiles placed left to right from the destination: each cell's tile, palette, priority and mirroring.</summary>
    public required RoomBackgroundTilemapCell[] Tiles { get; init; }
}
