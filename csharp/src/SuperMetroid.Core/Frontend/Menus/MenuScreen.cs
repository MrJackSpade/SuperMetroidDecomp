using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
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
    /// <summary>Installs the shared world, map, sprite and palette artwork.</summary>
    public MenuScreen(MapTileAtlas? mapTiles, MapStaticPalettes? mapPalettes, WorldMapArtwork? worldArtwork, MapSpriteCatalog? sprites)
    {
        WorldArtwork = RequireWorldArtwork(worldArtwork);
        MapTiles = RequireMapTiles(mapTiles);
        Sprites = RequireSprites(sprites);
        Objects = new ScreenSprites(Sprites.Page, ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection));
        BindMapPalettes(mapPalettes);
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
        characters = null;
    }

    /// <summary>Rebinds the map characters.</summary>
    public void BindMapTiles(MapTileAtlas? mapTiles)
    {
        MapTiles = RequireMapTiles(mapTiles);
        characters = null;
    }

    /// <summary>Rebinds the menu OBJ characters.</summary>
    public void BindMapSprites(MapSpriteCatalog? sprites)
    {
        Sprites = RequireSprites(sprites);
        Objects.BindArtwork(Sprites.Page);
        characters = null;
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

    /// <summary>The composed character image of the bound artwork, until a rebind replaces it.</summary>
    [NonSerialized]
    private byte[]? characters;

    /// <summary>The bound artwork's composed character image.</summary>
    internal ReadOnlySpan<byte> Characters => characters ??= MenuScreenImage.Characters(this);
}
