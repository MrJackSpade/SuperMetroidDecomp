using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The shared menu screen <c>LoadInitialMenuTiles</c> ($81:8DDB) sets up for file select, options, game over and the
/// file-select maps (#1358): the installed artwork it draws, its own tilemaps and palette, and its sprite entries. It
/// publishes these; the capture composes them, so no menu owns PPU memory.
/// </summary>
internal sealed class MenuScreen
{
    /// <summary>Installs the shared world and palette artwork, and the sheets its backgrounds draw from.</summary>
    public MenuScreen(MapStaticPalettes? mapPalettes, WorldMapArtwork? worldArtwork, InstalledSpriteSheets? sheets)
    {
        WorldArtwork = RequireWorldArtwork(worldArtwork);
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

    /// <summary>
    /// The menus' pictures named <paramref name="frames"/>, each placed at its page's corner in drawing order, a later one
    /// over an earlier one (#1467): each lands where the cartridge's page shows it.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed menu pictures hold no such frame.</exception>
    internal PlacedPictures PagePictures(params ReadOnlySpan<string> frames)
    {
        SpriteSheet pictures = Sheets.Get(SpriteSheetId.MenuPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPicture>(frames.Length);
        foreach (string frame in frames)
            placed.Add(new PlacedPicture(pictures.Frame(frame), default));
        return new PlacedPictures(placed.MoveToImmutable());
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


    /// <summary>Loads the file-select palette image.</summary>
    public void BindMapPalettes(MapStaticPalettes? mapPalettes)
    {
        MapStaticPalettes palettes = mapPalettes ?? throw new InvalidOperationException("Menus require installed palettes.");
        palettes.FileSelect.LoadTo(Palette);
    }

    /// <summary>The installed world-map characters, which a menu cannot draw without.</summary>
    private static WorldMapArtwork RequireWorldArtwork(WorldMapArtwork? worldArtwork) =>
        worldArtwork ?? throw new InvalidOperationException("Menus require installed world-map artwork.");


    /// <summary>What the screen paints with: its palette and sprite entries.</summary>
    internal FramePaint Paint() => new(Palette.Shown(), Objects.Draws);
}
