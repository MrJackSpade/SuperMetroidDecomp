using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>A composed tile word sheet: its part images and where it pictures each tile word over each artwork.</summary>
/// <param name="Layer">The backgrounds the words are written over.</param>
/// <param name="Tileset">The tileset whose rooms the words are written in.</param>
/// <param name="Sheet">The part images.</param>
/// <param name="Tiles">Every tile pictured.</param>
internal sealed record ComposedTileWordSheet(TileWordLayer Layer, TilesetDefinition Tileset, ComposedSpriteSheet Sheet, IReadOnlyList<TileWordSheetTile> Tiles);
