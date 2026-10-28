using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A tilemap word at a cell.</summary>
/// <param name="Column">The column.</param>
/// <param name="Row">The row.</param>
/// <param name="Tile">The tile.</param>
internal readonly record struct TilemapCell(int Column, int Row, SnesBgTilemapWord Tile);
