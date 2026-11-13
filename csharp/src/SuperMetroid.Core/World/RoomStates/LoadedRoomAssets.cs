using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The level, scroll grid and installed graphics a selected room state loads.
/// </summary>
/// <remarks>
/// This is the data-only portion of a room load. It deliberately runs no room-specific setup; that behaviour belongs to
/// each room's own code.
/// </remarks>
[RoomLifetime]
public sealed class LoadedRoomAssets
{
    /// <summary>Retains the selected definition, the room's grid, scroll grid, and installed graphics.</summary>
    private LoadedRoomAssets(
        SelectedRoom header,
        RoomLevelData levelData,
        RoomScrollGrid scrolls,
        RoomTilesetArtwork tileset)
    {
        Header = header;
        LevelData = levelData;
        Scrolls = scrolls;
        Tileset = tileset.Tileset;
        Pictures = tileset.Pictures;
    }

    /// <summary>Selected room definition and room-state metadata used to choose level source, graphics set, dimensions, scrolls, and door table; loading these assets does not run its room setup.</summary>
    public SelectedRoom Header { get; }
    /// <summary>The room's grid: terrain, behaviours and appearances, with the terrain beyond the room; its rows may exceed the visible screens.</summary>
    public RoomLevelData LevelData { get; }
    /// <summary>Room's screen-indexed scroll-control grid used to initialize the boundary camera, separate from current camera position or editable map-marker artwork.</summary>
    public RoomScrollGrid Scrolls { get; }
    /// <summary>Compiled graphics-set source identities selected by the room-state index and used to resolve installed block pictures, characters, and palette artwork.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed pre-coloured room artwork: block sheets and library background images (#1392).</summary>
    public InstalledRoomArtwork Pictures { get; }

    /// <summary>The pre-coloured pictures of the tileset's blocks the room's planes draw from (#1392).</summary>
    public RoomBlockSheet BlockSheet => Pictures.Get(Tileset);

    /// <summary>Reads every compressed input named by the selected room and graphics set.</summary>
    public static LoadedRoomAssets Load(ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> scrollCells, SelectedRoom header, RoomAssetCatalogs catalogs)
    {
        Ensure.NotNull(scrollCells);
        Ensure.NotNull(header);
        Ensure.NotNull(catalogs);
        RoomLevelArtworks visualLayouts = catalogs.VisualLayouts;
        TilesetDefinition tileset = RoomTilesets.Get(header.State.GraphicsSet);

        // The terrain, behaviours and terrain beyond the room come from the stock-only installed level; only what the
        // blocks initially show comes from the separately replaceable layout catalog.
        RoomLevelData levelData = LoadLevelData(header, catalogs.LevelStreams.Get(header.State.Level),
            visualLayouts.Get(header.State.Level));
        RoomScrollGrid scrolls = LoadScrolls(scrollCells, header);
        return new LoadedRoomAssets(
            header,
            levelData,
            scrolls,
            new RoomTilesetArtwork(tileset, catalogs.BlockSheets));
    }

    /// <summary>
    /// The turning floor the room's state shows (#1349, #1467, #1649): its tileset's installed floor, which the import
    /// pictures from the tileset's floor map and characters (#1422), with nothing drawn over it yet; null for a state that
    /// shows none (<see cref="RoomTurningFloor.None"/>), whose tileset installs none.
    /// </summary>
    /// <exception cref="InvalidDataException">The state shows a floor, but none is installed for the room's tileset.</exception>
    public RoomFloor? CreateFloor() => RoomTurningFloors.ShowsOne(Header.State) ? new RoomFloor(Pictures.FloorOf(Tileset)) : null;

    /// <summary>
    /// Builds the room's grid from its installed level, with its own terrain beyond the room for this room's lifetime.
    /// </summary>
    /// <remarks>
    /// The room definition sizes the camera's screens; the installed level sizes the grid. Most rooms make them equal, but
    /// a shipped Wrecked Ship room is a counterexample: its 6x3 camera owns a 6x4-screen level. Room objects and enemy
    /// collision may reach those extra rows, so the grid keeps them.
    /// </remarks>
    private static RoomLevelData LoadLevelData(SelectedRoom header, InstalledRoomLevel level, RoomLevelArtwork visualLayout)
    {
        Extent16<RoomBlocks> visibleSize = header.Size.ToRoomBlocks();
        if (level.Size.X != visibleSize.X || level.Size.Y < visibleSize.Y)
        {
            throw new InvalidDataException(
                $"Room {header.Definition.Name}'s installed {level.Size.X}x{level.Size.Y}-block level cannot contain its " +
                $"{header.Size.X}x{header.Size.Y} visible screens.");
        }
        // The level gets its own terrain beyond the grid, so writes there last exactly the room's lifetime.
        return new RoomLevelData(level.Size,
            new RoomLevelPlanes(level.Foreground.AsSpan(), level.Behaviors.AsSpan(), level.Background.AsSpan()),
            new TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior>(level.Following, level.BeyondCount,
                RoomTerrainBlocks.EdgeSolid, RoomBlockBehavior.None),
            header.Doors, visualLayout);
    }

    /// <summary>Creates the room's screen-indexed scroll grid from its selected state.</summary>
    private static RoomScrollGrid LoadScrolls(ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> scrollCells, SelectedRoom header)
        => RoomScrollGrid.FromDefinition(
            scrollCells,
            header.State.Scroll,
            header.Size);
}
