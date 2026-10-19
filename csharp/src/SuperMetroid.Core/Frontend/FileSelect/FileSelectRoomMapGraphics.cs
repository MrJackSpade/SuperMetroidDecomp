using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Room-select BG1 map and BG2 frame installed by $81:A725 and $82:9517.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed class FileSelectRoomMapGraphics
{
    /// <summary>Separate file-select PPU memory and installed background artwork.</summary>
    internal readonly MenuPpuState ppu;
    /// <summary>Map landmarks, stations, and boss icons drawn around the saved-station marker.</summary>
    private readonly FileSelectMapIcons icons;
    /// <summary>Installed map sprite compositions used when drawing the marker and animated arrows.</summary>
    [NonSerialized] private MapSpriteCatalog? sprites;
    /// <summary>Live menu-owned VRAM containing the installed BG1 area map, BG2 frame, and map characters; not the gameplay room's VRAM instance.</summary>
    public SnesVram Vram => ppu.Vram;
    /// <summary>Live menu-owned CGRAM, initialized from file-select presentation colors and subsequently updated by the menu's palette-animation owner.</summary>
    public SnesCgram Cgram => ppu.Cgram;
    /// <summary>Persistent downloaded-map and explored-cell state held by the icon owner.</summary>
    internal Bank80SystemState MapSystem => icons.MapSystem;

    /// <summary>Installs one area's revealed BG1 map and fixed BG2 frame into a separate menu PPU owner, following $81:A725/$82:9517 without choosing a load station or advancing menu timing.</summary>
    /// <param name="system">Persistence owner supplying downloaded-map, explored-cell, boss, and station visibility state; it is not replaced or cleared.</param>
    /// <param name="area">One of the six Zebes map areas, excluding Ceres.</param>
    /// <param name="revealMode">Optional presentation reveal override when projecting installed map cells; ordinary selection uses persistence-driven visibility.</param>
    /// <param name="mapPresentation">Required installed map tiles, palettes, screens, and sprite/layout resources, despite the optional parameter syntax.</param>
    public FileSelectRoomMapGraphics(Bank80SystemState system, AreaId area,
        MapRevealMode revealMode = MapRevealMode.None, AreaMapPresentationCatalog? mapPresentation = null)
    {
        Ensure.NotNull(system);
        if (mapPresentation is null)
            throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.");
        icons = new FileSelectMapIcons(system, area);
        icons.BindStations(mapPresentation.Stations);
        icons.BindLandmarks(mapPresentation.Landmarks);
        sprites = mapPresentation.Sprites;
        icons.BindSprites(sprites);
        int index = AreaIds.ToIndex(area);
        if (index >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        ppu = new MenuPpuState(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, sprites, loadInitialBackground: false);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        new MenuVram(ppu.Vram).AreaMapTilemap.Load(
            AreaMapTilemapBuilder.Build(mapPresentation.Get(area), system, hidden, revealMode));

        LoadFrame(mapPresentation.Screens, area);
    }

    /// <summary>Installs the selected area's fixed BG2 room frame into menu VRAM.</summary>
    private void LoadFrame(MapScreenPresentation? screens, AreaId area)
    {
        (screens ?? throw new InvalidOperationException(
            "Room-map frame requires installed map-screen assets."))
            .LoadTo(new MenuVram(Vram).Bg2Tilemap, MapScreenDefinitions.RoomFrame(area));
    }

    /// <summary>Reprojects current host artwork without resetting menu animation or scroll state.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        icons.BindStations(catalog?.Stations);
        icons.BindLandmarks(catalog?.Landmarks);
        sprites = catalog?.Sprites;
        icons.BindSprites(sprites);
        ppu.BindMapSprites(sprites);
        if (catalog is not null)
            new CgramPalettes(Cgram).LoadImageExcept(catalog.Palettes.FileSelect, FrontendPaletteLayout.MapHighlightCycle);
        (catalog ?? throw new InvalidOperationException(
            "Room-map graphics require installed map presentation assets."))
            .Tiles.LoadTo(new MenuVram(Vram).MapCharacters);
        var system = icons.MapSystem;
        var area = icons.MapArea;
        ppu.BindWorldArtwork(catalog?.WorldArtwork);
        LoadFrame(catalog?.Screens, area);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        new MenuVram(ppu.Vram).AreaMapTilemap.Load(
            AreaMapTilemapBuilder.Build((catalog ?? throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.")).Get(area), system, hidden));
    }

    /// <summary>Composes landmarks, the saved-station marker, and arrows in native OAM order without advancing animations.</summary>
    [PpuLayoutBoundary("frontend OAM writer: FileSelectRoomMapGraphics.PrepareIcons until #1358 moves it onto sprite entries")]
    private OamBuffer PrepareIcons(Position16<MapPixels> scroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        icons.DrawBeforeMarker(oam, scroll);
        marker.Draw(oam, scroll, sprites);
        icons.DrawAfterMarker(oam, scroll,
            animations is null ? null : () => animations.DrawArrows(oam, sprites));
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Captures the installed room map/frame and optional marker/arrows without advancing any animation.</summary>
    [PpuLayoutBoundary("codec: file-select room-map render packet layers")]
    public LayeredRenderSnapshot CaptureRenderSnapshot(Position16<MapPixels> scroll = default,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, bool frameOnly = false)
    {
        var layers = new List<RenderLayer>();
        if (frameOnly)
            layers.Add(new Bg4BppRenderLayer(MenuVramLayout.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters, new Position16<BgPlanePixels>(0, FileSelectMapRomData.RoomFrameVerticalScroll), new Extent16<TilemapCells>(32, 32), null));
        else
            foreach (bool priority in new[] { false, true })
            {
                layers.Add(new Bg4BppRenderLayer(MenuVramLayout.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters, new Position16<BgPlanePixels>(0, FileSelectMapRomData.RoomFrameVerticalScroll), new Extent16<TilemapCells>(32, 32), priority));
                layers.Add(new Bg4BppRenderLayer(MenuVramLayout.Bg1TilemapWord, FileSelectMapRomData.RoomCharacters,
                    scroll.AsBgPlaneScroll(), new Extent16<TilemapCells>(64, 32), priority));
            }
        OamBuffer oam;
        if (marker is not null && !frameOnly)
        {
            oam = PrepareIcons(scroll, marker, animations);
            layers.Add(new ObjRenderLayer());
        }
        else { oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame(); }
        return new(PpuMemorySnapshot.CaptureFrontend(Vram, Cgram, oam, FileSelectMapRomData.ObjectSelection), layers.ToArray(), FileSelectMapRomData.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }
}
