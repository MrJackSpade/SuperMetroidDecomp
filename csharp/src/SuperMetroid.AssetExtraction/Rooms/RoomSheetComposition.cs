using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The composed pre-coloured room artwork (#1392): each tileset's block sheet and each library background's image.</summary>
/// <param name="BlockSheets">The block sheets by tileset.</param>
/// <param name="Backgrounds">The background images by name.</param>
/// <param name="Bodies">The boss bodies by the tileset of the room each shows in.</param>
internal sealed record RoomSheetComposition(IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> BlockSheets,
    IReadOnlyDictionary<string, ComposedRoomBackground> Backgrounds, IReadOnlyDictionary<TilesetDefinition, ComposedBossBody> Bodies);
