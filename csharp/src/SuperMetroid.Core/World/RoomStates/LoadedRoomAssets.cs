using SuperMetroid.Core.Assets;
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
    /// <summary>Retains the selected header, the room's grid, scroll grid, and installed graphics.</summary>
    private LoadedRoomAssets(
        SelectedRoom header,
        RoomLevelData levelData,
        RoomScrollGrid scrolls,
        RoomTilesetArtwork tileset)
    {
        Header = header;
        LevelData = levelData;
        Scrolls = scrolls;
        TilesetColours = tileset.Colours;
        Tileset = tileset.Tileset;
        Pictures = tileset.Pictures;
    }

    /// <summary>Selected room header and room-state metadata used to choose level source, graphics set, dimensions, scrolls, and door table; loading these assets does not execute its room ASM.</summary>
    public SelectedRoom Header { get; }
    /// <summary>The room's grid: terrain, behaviours and appearances, with the terrain beyond the room; its rows may exceed the visible screens.</summary>
    public RoomLevelData LevelData { get; }
    /// <summary>Room's screen-indexed scroll-control grid used to initialize the boundary camera, separate from current camera position or editable map-marker artwork.</summary>
    public RoomScrollGrid Scrolls { get; }
    /// <summary>The tileset's colours for each background set, which <see cref="LoadGraphics"/> gives the sets, leaving the sprite sets alone.</summary>
    public ShownBackgroundColourSets TilesetColours { get; }
    /// <summary>Compiled graphics-set source identities selected by the room-state index and used to resolve installed block pictures, characters, and palette artwork.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed pre-coloured room artwork: block sheets and library background images (#1392).</summary>
    public InstalledRoomArtwork Pictures { get; }

    /// <summary>The pre-coloured pictures of the tileset's blocks the room's planes draw from (#1392).</summary>
    public RoomBlockSheet BlockSheet => Pictures.Get(Tileset);

    /// <summary>Reads every compressed input named by the selected room and graphics set.</summary>
    public static LoadedRoomAssets Load(RoomScrollStorage scrollStorage, SelectedRoom header, RoomAssetCatalogs catalogs)
    {
        Ensure.NotNull(scrollStorage);
        Ensure.NotNull(header);
        Ensure.NotNull(catalogs);
        RoomStaticPalettes paletteArt = catalogs.Palettes;
        RoomLevelArtworks visualLayouts = catalogs.VisualLayouts;
        TilesetDefinition tileset = RoomTilesets.Get(header.State.GraphicsSet);

        // The terrain, behaviours and terrain beyond the room come from the stock-only installed level; only what the
        // blocks initially show comes from the separately replaceable layout catalog.
        RoomLevelData levelData = LoadLevelData(header, catalogs.LevelStreams.Get(header.State.Level),
            visualLayouts.Get(header.State.Level));
        RoomScrollGrid scrolls = LoadScrolls(scrollStorage, header);
        return new LoadedRoomAssets(
            header,
            levelData,
            scrolls,
            new RoomTilesetArtwork(tileset, paletteArt.Get(tileset.Palette).Sets, catalogs.BlockSheets));
    }

    /// <summary>
    /// The perspective floor a Ceres room shows (#1349, #1467): its tileset's installed floor, which the import pictures
    /// from the tileset's floor map and characters (#1422), with nothing drawn over it yet.
    /// </summary>
    /// <exception cref="InvalidDataException">No floor is installed for the room's tileset.</exception>
    public RoomFloor CreateFloor() => new(Pictures.FloorOf(Tileset));

    /// <summary>
    /// Gives every background set the tileset's colours for it (#1464). The room's tileset artwork shows in its block sheet's
    /// pictures (#1392, #1466), so nothing is bound.
    /// </summary>
    public void LoadGraphics(BackgroundColourSets colours) => TilesetColours.CopyTo(Ensure.NotNull(colours));

    /// <summary>
    /// Builds the room's grid from its installed level, with its own terrain beyond the room for this room's lifetime.
    /// </summary>
    /// <remarks>
    /// The room header sizes the camera's screens; the installed level sizes the grid. Most rooms make them equal, but
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
        return level.Load(header.Doors, visualLayout);
    }

    /// <summary>Creates the room's screen-indexed scroll grid from its selected state.</summary>
    private static RoomScrollGrid LoadScrolls(RoomScrollStorage scrollStorage, SelectedRoom header)
        => RoomScrollGrid.FromDefinition(
            scrollStorage,
            header.State.Scroll,
            header.Size);
}
