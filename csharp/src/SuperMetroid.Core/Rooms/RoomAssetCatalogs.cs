using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's character, palette, block picture, layout and level-allocation graph is compiled from.</summary>
public sealed record RoomAssetCatalogs
{
    /// <summary>Groups the complete installed catalogs required to compile a room state without cartridge reads.</summary>
    /// <param name="characters">Character artwork selected by the room graphics-set source.</param>
    /// <param name="palettes">Static RGB5 palettes selected by the room graphics-set source.</param>
    /// <param name="visualLayouts">Compiled room layouts selected by their 24-bit level-data source.</param>
    /// <param name="levelStreams">Stock native level allocations selected by their 24-bit level-data source.</param>
    /// <param name="blockSheets">The pre-coloured block pictures of each tileset (#1392).</param>
    public RoomAssetCatalogs(RoomCharacterAtlases characters, RoomStaticPalettes palettes,
        RoomLevelArtworks visualLayouts, InstalledRoomLevelStreams levelStreams,
        InstalledRoomArtwork blockSheets)
    {
        BlockSheets = Ensure.NotNull(blockSheets);
        Characters = Ensure.NotNull(characters);
        Palettes = Ensure.NotNull(palettes);
        VisualLayouts = Ensure.NotNull(visualLayouts);
        LevelStreams = Ensure.NotNull(levelStreams);
    }

    /// <summary>Gets the complete room-character artwork installation.</summary>
    public RoomCharacterAtlases Characters { get; init; }
    /// <summary>Gets the complete static room-palette installation.</summary>
    public RoomStaticPalettes Palettes { get; init; }
    /// <summary>Gets the complete source-keyed room visual-layout installation.</summary>
    public RoomLevelArtworks VisualLayouts { get; init; }
    /// <summary>Gets the stock native collision/BTS and streaming allocations, which visual overrides never alter.</summary>
    public InstalledRoomLevelStreams LevelStreams { get; init; }
    /// <summary>Gets the pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork BlockSheets { get; init; }
}
