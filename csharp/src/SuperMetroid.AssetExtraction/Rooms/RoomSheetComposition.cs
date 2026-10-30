using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The composed pre-coloured room artwork (#1392, #1466): each tileset's block sheet with the appearance inputs its blocks
/// depend on, each library background's image, and the sheets of the pictures written over the rooms.
/// </summary>
/// <param name="BlockSheets">The block sheets by tileset.</param>
/// <param name="BlockAppearances">The appearance inputs each block depends on, by tileset (#1466).</param>
/// <param name="Backgrounds">The background images by name.</param>
/// <param name="Written">The sheets of pictures written over BG1 and BG2, by tileset.</param>
/// <param name="Bg3Pictures">The sheets of BG3 pictures, by tileset.</param>
/// <param name="MessageBoxes">The message box images, by tileset.</param>
/// <param name="Floors">The turning floors and the pictures drawn over them, by Ceres tileset (#1467).</param>
internal sealed record RoomSheetComposition(IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> BlockSheets,
    IReadOnlyDictionary<TilesetDefinition, RoomBlockAppearances> BlockAppearances,
    IReadOnlyDictionary<string, ComposedRoomBackground> Backgrounds, IReadOnlyDictionary<TilesetDefinition, ComposedWrittenPictureSheet> Written,
    IReadOnlyDictionary<TilesetDefinition, ComposedWrittenPictureSheet> Bg3Pictures,
    IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> MessageBoxes, IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> Floors);
