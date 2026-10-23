using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The shared menu screen <c>LoadInitialMenuTiles</c> ($81:8DDB) sets up for file select, options, game over and the
/// file-select maps (#1358): the installed artwork it draws, its own tilemaps and palette, and its sprite entries. It
/// publishes these; the capture composes them, so no menu owns PPU memory.
/// </summary>
internal sealed class MenuScreen
{
    /// <summary>Installs the shared world, map, sprite and palette artwork, and the sheets its backgrounds draw from.</summary>
    public MenuScreen(MapTileAtlas? mapTiles, MapStaticPalettes? mapPalettes, WorldMapArtwork? worldArtwork, MapSpriteCatalog? sprites,
        InstalledSpriteSheets? sheets)
    {
        WorldArtwork = RequireWorldArtwork(worldArtwork);
        MapTiles = RequireMapTiles(mapTiles);
        Sprites = RequireSprites(sprites);
        Objects = new ScreenSprites();
        BindMapPalettes(mapPalettes);
        BindScreenTiles(sheets);
    }

    /// <summary>The installed sheets the screen's background tiles come from (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    private InstalledSpriteSheets Sheets { get => field ?? throw new InvalidOperationException("The menu's screen tiles are rebound after debugger restoration."); set; }

    /// <summary>Each area map's tiles, once read.</summary>
    [NonSerialized] private Dictionary<AreaId, AreaMapTiles>? areaMapTiles;

    /// <summary>Rebinds the sheets the screen's background tiles come from (#1392).</summary>
    public void BindScreenTiles(InstalledSpriteSheets? sheets)
    {
        Sheets = sheets ?? throw new InvalidOperationException("Menus require the installed sprite sheets.");
        MenuTiles = null;
        RoomMapTiles = null;
        areaMapTiles = null;
    }

    /// <summary>What the menu planes show under the file-select palette (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    internal ScreenLayerPictures MenuTiles { get => field ??= MenuScreenTiles.Menu(Sheets); private set; }

    /// <summary>What the room map's planes show under the file-select palette (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    internal ScreenLayerPictures RoomMapTiles { get => field ??= MenuScreenTiles.RoomMap(Sheets); private set; }

    /// <summary>What the area map's layers show under <paramref name="area"/>'s world-map palette (#1392).</summary>
    internal AreaMapTiles AreaMapTiles(AreaId area)
    {
        areaMapTiles ??= [];
        if (!areaMapTiles.TryGetValue(area, out AreaMapTiles? tiles))
            areaMapTiles.Add(area, tiles = MenuScreenTiles.AreaMap(Sheets, area));
        return tiles;
    }

    /// <summary>The installed world-map characters.</summary>
    [field: NonSerialized]
    [AllowNull]
    public WorldMapArtwork WorldArtwork { get => field ?? throw new InvalidOperationException("The menu's world-map characters are rebound after debugger restoration."); private set; }

    /// <summary>The installed map characters.</summary>
    [field: NonSerialized]
    [AllowNull]
    public MapTileAtlas MapTiles { get => field ?? throw new InvalidOperationException("The menu's map characters are rebound after debugger restoration."); private set; }

    /// <summary>The installed menu sprite catalog, whose page the sprite entries draw from.</summary>
    [field: NonSerialized]
    [AllowNull]
    public MapSpriteCatalog Sprites { get => field ?? throw new InvalidOperationException("The menu sprite catalog is rebound after debugger restoration."); private set; }

    /// <summary>The menu's tilemaps.</summary>
    public MenuTilemaps Tilemaps { get; } = new();

    /// <summary>The menu's palette.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>The menu's sprite entries over the installed menu OBJ page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>Rebinds the world characters, keeping tilemaps and ongoing palette state.</summary>
    public void BindWorldArtwork(WorldMapArtwork? worldArtwork)
    {
        WorldArtwork = RequireWorldArtwork(worldArtwork);
    }

    /// <summary>Rebinds the map characters.</summary>
    public void BindMapTiles(MapTileAtlas? mapTiles)
    {
        MapTiles = RequireMapTiles(mapTiles);
    }

    /// <summary>Rebinds the menu OBJ characters.</summary>
    public void BindMapSprites(MapSpriteCatalog? sprites)
    {
        Sprites = RequireSprites(sprites);
    }

    /// <summary>Loads the file-select palette image.</summary>
    public void BindMapPalettes(MapStaticPalettes? mapPalettes)
    {
        MapStaticPalettes palettes = mapPalettes ?? throw new InvalidOperationException("Menus require installed palettes.");
        new CgramPalettes(Palette).LoadImage(palettes.FileSelect);
    }

    /// <summary>The installed world-map characters, which a menu cannot draw without.</summary>
    private static WorldMapArtwork RequireWorldArtwork(WorldMapArtwork? worldArtwork) =>
        worldArtwork ?? throw new InvalidOperationException("Menus require installed world-map artwork.");

    /// <summary>The installed map characters, which a menu cannot draw without.</summary>
    private static MapTileAtlas RequireMapTiles(MapTileAtlas? mapTiles) =>
        mapTiles ?? throw new InvalidOperationException("Menus require installed map tiles.");

    /// <summary>The installed menu sprite catalog, which a menu cannot draw without.</summary>
    private static MapSpriteCatalog RequireSprites(MapSpriteCatalog? sprites) =>
        sprites ?? throw new InvalidOperationException("Menus require installed sprite artwork.");

    /// <summary>What the screen paints with: its palette and sprite entries.</summary>
    internal FramePaint Paint() => new(Palette.Colors, Objects.Draws);
}
