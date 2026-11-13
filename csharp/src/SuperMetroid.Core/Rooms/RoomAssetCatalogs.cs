using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's block pictures, layout and grid are compiled from.</summary>
public sealed record RoomAssetCatalogs
{
    /// <summary>Groups the complete installed catalogs required to compile a room state without reading game data.</summary>
    /// <param name="visualLayouts">The rooms' replaceable initial block appearances, selected by level.</param>
    /// <param name="levelStreams">The installed terrain of every room level, selected by level.</param>
    /// <param name="blockSheets">The pre-coloured block pictures of each tileset (#1392).</param>
    public RoomAssetCatalogs(
        RoomLevelArtworks visualLayouts, InstalledRoomLevelStreams levelStreams,
        InstalledRoomArtwork blockSheets)
    {
        BlockSheets = Ensure.NotNull(blockSheets);
        VisualLayouts = Ensure.NotNull(visualLayouts);
        LevelStreams = Ensure.NotNull(levelStreams);
    }

    /// <summary>Gets the complete source-keyed room visual-layout installation.</summary>
    public RoomLevelArtworks VisualLayouts { get; init; }
    /// <summary>Gets the installed terrain of every room level, which appearance overrides never alter.</summary>
    public InstalledRoomLevelStreams LevelStreams { get; init; }
    /// <summary>Gets the pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork BlockSheets { get; init; }
}
