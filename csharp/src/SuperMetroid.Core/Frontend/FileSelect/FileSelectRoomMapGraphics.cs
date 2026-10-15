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
    /// <summary>Menu address space shared with the PPU presentation owner.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Separate file-select PPU memory and installed background artwork.</summary>
    private readonly MenuPpuState ppu;
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
    /// <param name="bus">Required address-space context for menu PPU setup and drawing APIs.</param>
    /// <param name="system">Persistence owner supplying downloaded-map, explored-cell, boss, and station visibility state; it is not replaced or cleared.</param>
    /// <param name="area">One of the six Zebes map areas, excluding Ceres.</param>
    /// <param name="revealMode">Optional presentation reveal override when projecting installed map cells; ordinary selection uses persistence-driven visibility.</param>
    /// <param name="mapPresentation">Required installed map tiles, palettes, screens, and sprite/layout resources, despite the optional parameter syntax.</param>
    public FileSelectRoomMapGraphics(ISnesAddressSpace bus, Bank80SystemState system, AreaId area,
        MapRevealMode revealMode = MapRevealMode.None, AreaMapPresentationCatalog? mapPresentation = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(system);
        if (mapPresentation is null)
            throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.");
        this.bus = bus;
        icons = new FileSelectMapIcons(system, area);
        icons.BindStations(mapPresentation.Stations);
        icons.BindLandmarks(mapPresentation.Landmarks);
        sprites = mapPresentation.Sprites;
        icons.BindSprites(sprites);
        int index = AreaIds.ToIndex(area);
        if (index >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        ppu = new MenuPpuState(bus, mapPresentation.Tiles, mapPresentation.Palettes,
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

    /// <summary>BG2-only endpoint of $81:AC2D; room-map cells are not installed until $81:AD17.</summary>
    /// <returns>Owned 256-by-224 RGBA frame buffer, valid until this method renders again; BG2 uses fixed scroll (0, 24), with no map icons or timing updates.</returns>
    [PpuLayoutBoundary("backend: file-select room-map software PPU composition")]
    public Rgba32[] RenderFrameOnly()
    {
        Rgba32[] pixels = frameOnlyBuffer ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, new CgramPalettes(Cgram).BackdropRgba);
        // Composite directly: transparent cells leave the backdrop, as compositing a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, Vram, Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg2TilemapWord, CharacterBaseWord = FileSelectMapRomData.RoomCharacters }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 24), Size = new Extent16<ScreenPixels>(FrontendFrame.Width, FrontendFrame.Height), TilemapSize = new Extent16<TilemapCells>(32, 32) });
        return pixels;
    }

    /// <summary>Draws the saved-station marker over the room map without advancing its animation.</summary>
    /// <param name="scroll">BG1/map-icon scroll in whole pixels; the BG2 frame remains fixed.</param>
    /// <param name="marker">Required saved-station marker whose current animation state is read without stepping it.</param>
    /// <param name="animations">Optional current arrow-animation owner; drawing does not advance its state.</param>
    /// <returns>Owned 256-by-224 RGBA background buffer with icons composited in native OAM order; a subsequent background or combined render overwrites it.</returns>
    public Rgba32[] Render(Position16<MapPixels> scroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations = null)
    {
        Ensure.NotNull(marker);
        Rgba32[] pixels = RenderBackgrounds(scroll);
        OamBuffer oam = PrepareIcons(scroll, marker, animations);
        Rgba32[] objects = objScratch ??= new Rgba32[256 * 224];
        SnesObjRenderer.Render(objects, oam, Vram, Cgram, obsel: 3);
        SnesLayerCompositor.Composite(pixels, objects);
        return pixels;
    }

    /// <summary>Reusable OBJ pixels for map icons; restored saves allocate a fresh scratch buffer.</summary>
    [NonSerialized] private Rgba32[]? objScratch;
    /// <summary>Owned BG2-only frame buffer reused by subsequent frame-only renders.</summary>
    [NonSerialized] private Rgba32[]? frameOnlyBuffer;
    /// <summary>Owned BG1 and BG2 composition buffer reused by background and combined renders.</summary>
    [NonSerialized] private Rgba32[]? backgroundsBuffer;

    /// <summary>Composes landmarks, the saved-station marker, and arrows in native OAM order without advancing animations.</summary>
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

    /// <summary>Renders Mode-1 BG priorities with independent scrolling for map and fixed frame.</summary>
    /// <param name="scroll">BG1 scroll-register value in pixels.</param>
    /// <returns>Owned 256-by-224 RGBA buffer, overwritten by the next background or combined render; within each low/high priority tier the fixed BG2 frame precedes BG1 map cells.</returns>
    [PpuLayoutBoundary("backend: file-select room-map software PPU composition")]
    public Rgba32[] RenderBackgrounds(Position16<MapPixels> scroll)
    {
        Rgba32[] pixels = backgroundsBuffer ??= new Rgba32[256 * 224];
        Array.Fill(pixels, new CgramPalettes(ppu.Cgram).BackdropRgba);
        for (int tier = 0; tier < 2; tier++)
        {
            // Mode 1 orders BG2 before BG1 within each background priority tier.
            bool priority = tier == 1;
            // Composited directly into the frame; transparent pixels leave it untouched.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, Vram, Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg2TilemapWord, CharacterBaseWord = FileSelectMapRomData.RoomCharacters }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 24), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, Vram, Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg1TilemapWord, CharacterBaseWord = FileSelectMapRomData.RoomCharacters }, new BgViewport { Scroll = scroll.AsBgPlaneScroll(), Size = SnesPpuLayout.ScreenSize }, priority: priority);
        }
        return pixels;
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
        return new(PpuMemorySnapshot.Capture(Vram, Cgram, oam), layers.ToArray(), FileSelectMapRomData.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }
}
