using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's block pictures, layout, grid and scroll cells are built from.</summary>
public sealed record RoomAssetCatalogs
{
    /// <summary>Groups the complete installed catalogs required to build a room state without reading game data.</summary>
    /// <param name="visualLayouts">The rooms' replaceable initial block appearances, selected by level.</param>
    /// <param name="levelStreams">The installed terrain of every room level, selected by level.</param>
    /// <param name="blockSheets">The pre-coloured block pictures of each tileset (#1392).</param>
    /// <param name="scrollCellsPastRoomScreens">The cells each explicit scroll layout's load writes past the room's screens.</param>
    public RoomAssetCatalogs(
        RoomLevelArtworks visualLayouts, InstalledRoomLevelStreams levelStreams,
        InstalledRoomArtwork blockSheets, ScrollCellsPastRoomScreens<RoomScrollState> scrollCellsPastRoomScreens)
    {
        BlockSheets = Ensure.NotNull(blockSheets);
        VisualLayouts = Ensure.NotNull(visualLayouts);
        LevelStreams = Ensure.NotNull(levelStreams);
        ScrollCellsPastRoomScreens = Ensure.NotNull(scrollCellsPastRoomScreens);
    }

    /// <summary>Gets the complete source-keyed room visual-layout installation.</summary>
    public RoomLevelArtworks VisualLayouts { get; init; }
    /// <summary>Gets the installed terrain of every room level, which appearance overrides never alter.</summary>
    public InstalledRoomLevelStreams LevelStreams { get; init; }
    /// <summary>Gets the pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork BlockSheets { get; init; }
    /// <summary>Gets the cells each explicit scroll layout's load writes past the room's screens (#2356).</summary>
    public ScrollCellsPastRoomScreens<RoomScrollState> ScrollCellsPastRoomScreens { get; init; }
}
