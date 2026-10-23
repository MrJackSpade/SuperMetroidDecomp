using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Room-select BG1 map and BG2 frame installed by $81:A725 and $82:9517.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed class FileSelectRoomMapGraphics
{
    /// <summary>The room map's menu screen: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Map landmarks, stations, and boss icons drawn around the saved-station marker.</summary>
    private readonly FileSelectMapIcons icons;
    /// <summary>Installed map sprite compositions whose page the menu screen loads.</summary>
    [NonSerialized] private MapSpriteCatalog? sprites;
    /// <summary>The installed map sprite sheet the icons, marker and arrows draw from.</summary>
    [NonSerialized] private SpriteSheet? mapSheet;
    /// <summary>The room map's palette, initialized from file-select presentation colors and subsequently updated by the menu's palette-animation owner.</summary>
    public ScreenPalette Palette => screen.Palette;
    /// <summary>Persistent downloaded-map and explored-cell state held by the icon owner.</summary>
    internal Bank80SystemState MapSystem => icons.MapSystem;

    /// <summary>Installs one area's revealed BG1 map and fixed BG2 frame into a separate menu PPU owner, following $81:A725/$82:9517 without choosing a load station or advancing menu timing.</summary>
    /// <param name="system">Persistence owner supplying downloaded-map, explored-cell, boss, and station visibility state; it is not replaced or cleared.</param>
    /// <param name="area">One of the six Zebes map areas, excluding Ceres.</param>
    /// <param name="revealMode">Optional presentation reveal override when projecting installed map cells; ordinary selection uses persistence-driven visibility.</param>
    /// <param name="mapPresentation">Required installed map tiles, palettes, screens, and sprite/layout resources, despite the optional parameter syntax.</param>
    /// <param name="sheets">Required installed sprite sheets, despite the optional parameter syntax: the map sprites and the background tiles.</param>
    public FileSelectRoomMapGraphics(Bank80SystemState system, AreaId area,
        MapRevealMode revealMode = MapRevealMode.None, AreaMapPresentationCatalog? mapPresentation = null, InstalledSpriteSheets? sheets = null)
    {
        Ensure.NotNull(system);
        if (mapPresentation is null)
            throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.");
        icons = new FileSelectMapIcons(system, area);
        icons.BindStations(mapPresentation.Stations);
        icons.BindLandmarks(mapPresentation.Landmarks);
        sprites = mapPresentation.Sprites;
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Room-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        icons.BindSprites(mapSheet);
        int index = AreaIds.ToIndex(area);
        if (index >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        screen = new MenuScreen(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, sprites, installed);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        MenuTilemaps.Load(screen.Tilemaps.AreaMap,
            AreaMapTilemapBuilder.Build(mapPresentation.Get(area), system, hidden, revealMode));

        LoadFrame(mapPresentation.Screens, area);
    }

    /// <summary>Installs the selected area's fixed BG2 room frame into menu VRAM.</summary>
    private void LoadFrame(MapScreenPresentation? screens, AreaId area)
    {
        (screens ?? throw new InvalidOperationException(
            "Room-map frame requires installed map-screen assets."))
            .LoadTo(screen.Tilemaps.Bg2, MapScreenDefinitions.RoomFrame(area));
    }

    /// <summary>Rebinds the installed sheets: the map sprites the icons, marker and arrows draw from, and the background tiles.</summary>
    internal void BindSheets(InstalledSpriteSheets? sheets)
    {
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Room-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        icons.BindSprites(mapSheet);
        screen.BindScreenTiles(installed);
    }

    /// <summary>Reprojects current host artwork without resetting menu animation or scroll state.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        icons.BindStations(catalog?.Stations);
        icons.BindLandmarks(catalog?.Landmarks);
        sprites = catalog?.Sprites;
        screen.BindMapSprites(sprites);
        if (catalog is not null)
            new CgramPalettes(screen.Palette).LoadImageExcept(catalog.Palettes.FileSelect, FrontendPaletteLayout.MapHighlightCycle);
        screen.BindMapTiles((catalog ?? throw new InvalidOperationException(
            "Room-map graphics require installed map presentation assets.")).Tiles);
        var system = icons.MapSystem;
        var area = icons.MapArea;
        screen.BindWorldArtwork(catalog.WorldArtwork);
        LoadFrame(catalog.Screens, area);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        MenuTilemaps.Load(screen.Tilemaps.AreaMap,
            AreaMapTilemapBuilder.Build(catalog.Get(area), system, hidden));
    }

    /// <summary>Composes landmarks, the saved-station marker, and arrows in native order without advancing animations.</summary>
    private void PrepareIcons(Position16<MapPixels> scroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        ScreenSprites entries = screen.Objects;
        entries.BeginFrame();
        icons.DrawBeforeMarker(entries, scroll);
        marker.Draw(entries, scroll, mapSheet);
        icons.DrawAfterMarker(entries, scroll,
            animations is null ? null : () => animations.DrawArrows(entries, mapSheet));
        entries.FinishFrame();
    }

    /// <summary>Captures the installed room map/frame and optional marker/arrows without advancing any animation.</summary>
    public LayeredScene CaptureRenderSnapshot(Position16<MapPixels> scroll = default,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, RoomMapContent content = RoomMapContent.Scene)
    {
        var layers = new List<RenderLayer>();
        // Both planes draw from the map characters' pre-coloured tiles (#1392).
        ScreenLayerPictures tiles = screen.RoomMapTiles;
        ScreenTileMap frame = ScreenTileMap.Of(screen.Tilemaps.Bg2, new Extent16<TilemapCells>(32, 32), tiles);
        if (content == RoomMapContent.FrameOnly)
            layers.Add(new TileRenderLayer(frame, new Position16<BgPlanePixels>(0, FileSelectMapRomData.RoomFrameVerticalScroll), null));
        else
        {
            ScreenTileMap map = ScreenTileMap.Of(screen.Tilemaps.AreaMap, new Extent16<TilemapCells>(64, 32), tiles);
            foreach (bool priority in new[] { false, true })
            {
                layers.Add(new TileRenderLayer(frame, new Position16<BgPlanePixels>(0, FileSelectMapRomData.RoomFrameVerticalScroll), priority));
                layers.Add(new TileRenderLayer(map, scroll.AsBgPlaneScroll(), priority));
            }
        }
        if (marker is not null && content != RoomMapContent.FrameOnly)
        {
            PrepareIcons(scroll, marker, animations);
            layers.Add(new ObjRenderLayer());
        }
        else
        {
            screen.Objects.BeginFrame();
            screen.Objects.FinishFrame();
        }
        return new(screen.Paint(), layers.ToArray(),
            SnesPpuLayout.MaximumMasterBrightness);
    }
}
