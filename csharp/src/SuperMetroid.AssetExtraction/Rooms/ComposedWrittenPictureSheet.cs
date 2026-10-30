using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A composed written picture sheet (#1392, #1466): its pictures, each named for the values of the appearance inputs it
/// depends on, and which inputs each picture depends on.
/// </summary>
/// <param name="Layer">The backgrounds the pictures are written over.</param>
/// <param name="Tileset">The tileset whose rooms the pictures are written in.</param>
/// <param name="Sheet">The pictures.</param>
/// <param name="Inputs">The inputs each picture depends on, by the name its pictures begin with.</param>
internal sealed record ComposedWrittenPictureSheet(WrittenPictureLayer Layer, TilesetDefinition Tileset, ComposedSpriteSheet Sheet,
    IReadOnlyDictionary<string, ImmutableArray<AppearanceInput>> Inputs);
