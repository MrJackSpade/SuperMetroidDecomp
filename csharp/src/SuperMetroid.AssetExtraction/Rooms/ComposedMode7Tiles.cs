using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>A composed Ceres tileset's Mode 7 tiles: the picture and what each tile was pictured over.</summary>
/// <param name="Tileset">The tileset.</param>
/// <param name="Sheet">The picture of every tile.</param>
/// <param name="Shown">What showed at each tile's characters, or null where nothing did.</param>
internal sealed record ComposedMode7Tiles(TilesetDefinition Tileset, ComposedSpriteSheet Sheet, IReadOnlyList<BgArtworkShown?> Shown);
