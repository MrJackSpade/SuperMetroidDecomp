using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A composed tile word sheet (#1392, #1466): its part images, where it pictures each tile word for each value of the
/// appearance inputs it depends on, and which inputs those are.
/// </summary>
/// <param name="Layer">The backgrounds the words are written over.</param>
/// <param name="Tileset">The tileset whose rooms the words are written in.</param>
/// <param name="Sheet">The part images.</param>
/// <param name="Tiles">Every tile pictured.</param>
/// <param name="Inputs">The inputs each tile word that depends on any depends on.</param>
internal sealed record ComposedTileWordSheet(TileWordLayer Layer, TilesetDefinition Tileset, ComposedSpriteSheet Sheet, IReadOnlyList<TileWordSheetTile> Tiles,
    IReadOnlyDictionary<BackgroundTile, ImmutableArray<AppearanceInput>> Inputs);
