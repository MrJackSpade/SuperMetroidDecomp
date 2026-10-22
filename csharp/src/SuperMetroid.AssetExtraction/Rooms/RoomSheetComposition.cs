using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The composed pre-coloured room artwork (#1392): each tileset's block sheet and each library background's image.</summary>
/// <param name="BlockSheets">The block sheets by tileset.</param>
/// <param name="Backgrounds">The background images by name.</param>
/// <param name="TileWords">The sheets of words written over BG1 and BG2, by tileset.</param>
/// <param name="Bg3Words">The sheets of BG3 words, by tileset.</param>
/// <param name="Messages">The words of each form of each message, by image name.</param>
/// <param name="MessageBoxes">The message box images, by tileset.</param>
internal sealed record RoomSheetComposition(IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> BlockSheets,
    IReadOnlyDictionary<string, ComposedRoomBackground> Backgrounds, IReadOnlyDictionary<TilesetDefinition, ComposedTileWordSheet> TileWords,
    IReadOnlyDictionary<TilesetDefinition, ComposedTileWordSheet> Bg3Words, IReadOnlyDictionary<string, ImmutableArray<ushort>> Messages,
    IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> MessageBoxes);
