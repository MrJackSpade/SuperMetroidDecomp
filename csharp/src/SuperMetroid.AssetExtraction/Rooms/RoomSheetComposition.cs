using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The composed pre-coloured room artwork (#1392, #1466): each tileset's block sheet with the appearance inputs its blocks
/// depend on, each library background's image, and the sheets of the words written over the rooms.
/// </summary>
/// <param name="BlockSheets">The block sheets by tileset.</param>
/// <param name="BlockAppearances">The appearance inputs each block depends on, by tileset (#1466).</param>
/// <param name="Backgrounds">The background images by name.</param>
/// <param name="TileWords">The sheets of words written over BG1 and BG2, by tileset.</param>
/// <param name="Bg3Words">The sheets of BG3 words, by tileset.</param>
/// <param name="Messages">The tiles of each form of each message, by image name.</param>
/// <param name="MessageBoxes">The message box images, by tileset.</param>
/// <param name="FloorTiles">The Mode 7 tiles, by Ceres tileset.</param>
internal sealed record RoomSheetComposition(IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> BlockSheets,
    IReadOnlyDictionary<TilesetDefinition, RoomBlockAppearances> BlockAppearances,
    IReadOnlyDictionary<string, ComposedRoomBackground> Backgrounds, IReadOnlyDictionary<TilesetDefinition, ComposedTileWordSheet> TileWords,
    IReadOnlyDictionary<TilesetDefinition, ComposedTileWordSheet> Bg3Words, IReadOnlyDictionary<string, ImmutableArray<BackgroundTile>> Messages,
    IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> MessageBoxes, IReadOnlyDictionary<TilesetDefinition, ComposedMode7Tiles> FloorTiles);
