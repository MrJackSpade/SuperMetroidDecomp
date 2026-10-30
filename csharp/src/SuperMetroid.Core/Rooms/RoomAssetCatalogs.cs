using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's palette, block pictures, layout and grid are compiled from.</summary>
public sealed record RoomAssetCatalogs
{
    /// <summary>Groups the complete installed catalogs required to compile a room state without cartridge reads.</summary>
    /// <param name="palettes">Static RGB5 palettes selected by the room graphics-set source.</param>
    /// <param name="visualLayouts">The rooms' replaceable initial block appearances, selected by level.</param>
    /// <param name="levelStreams">The installed terrain of every room level, selected by level.</param>
    /// <param name="blockSheets">The pre-coloured block pictures of each tileset (#1392).</param>
    public RoomAssetCatalogs(RoomStaticPalettes palettes,
        RoomLevelArtworks visualLayouts, InstalledRoomLevelStreams levelStreams,
        InstalledRoomArtwork blockSheets)
    {
        BlockSheets = Ensure.NotNull(blockSheets);
        Palettes = Ensure.NotNull(palettes);
        VisualLayouts = Ensure.NotNull(visualLayouts);
        LevelStreams = Ensure.NotNull(levelStreams);
    }

    /// <summary>Gets the complete static room-palette installation.</summary>
    public RoomStaticPalettes Palettes { get; init; }
    /// <summary>Gets the complete source-keyed room visual-layout installation.</summary>
    public RoomLevelArtworks VisualLayouts { get; init; }
    /// <summary>Gets the installed terrain of every room level, which appearance overrides never alter.</summary>
    public InstalledRoomLevelStreams LevelStreams { get; init; }
    /// <summary>Gets the pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork BlockSheets { get; init; }
}
