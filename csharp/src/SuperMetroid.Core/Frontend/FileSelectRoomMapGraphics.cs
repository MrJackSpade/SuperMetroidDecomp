using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Room-select BG1 map and BG2 frame installed by $81:A725 and $82:9517.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed partial class FileSelectRoomMapGraphics
{
    private readonly ISnesAddressSpace bus;
    private readonly MenuPpuState ppu;
    private readonly FileSelectMapIcons icons;
    [NonSerialized] private MapSpriteCatalog? sprites;
    /// <summary>Live menu-owned VRAM containing the installed BG1 area map, BG2 frame, and map characters; not the gameplay room's VRAM instance.</summary>
    public SnesVram Vram => ppu.Vram;
    /// <summary>Live menu-owned CGRAM, initialized from file-select presentation colors and subsequently updated by the menu's palette-animation owner.</summary>
    public SnesCgram Cgram => ppu.Cgram;
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
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(system);
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
        ppu.Vram.LoadBytes(MenuPpuState.Bg1TilemapWord * 2,
            AreaMapTilemapBuilder.Build(mapPresentation.Get(area), system, hidden, revealMode));

        LoadFrame(mapPresentation.Screens, area);
    }

    private void LoadFrame(MapScreenPresentation? screens, AreaId area)
    {
        (screens ?? throw new InvalidOperationException(
            "Room-map frame requires installed map-screen assets."))
            .LoadTo(Vram, MenuPpuState.Bg2TilemapWord * 2, MapScreenDefinitions.RoomFrame(area));
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
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color is < MapAnimationRomData.PaletteDestination or >= (MapAnimationRomData.PaletteDestination + MapPaletteCycleFormat.ColorCount))
                    Cgram.SetColor(color, catalog.Palettes.FileSelect[color]);
        (catalog ?? throw new InvalidOperationException(
            "Room-map graphics require installed map presentation assets."))
            .Tiles.LoadTo(Vram, FileSelectMapRomData.RoomCharacters * 2);
        var system = icons.MapSystem;
        var area = icons.MapArea;
        ppu.BindWorldArtwork(catalog?.WorldArtwork);
        LoadFrame(catalog?.Screens, area);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        ppu.Vram.LoadBytes(MenuPpuState.Bg1TilemapWord * 2,
            AreaMapTilemapBuilder.Build((catalog ?? throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.")).Get(area), system, hidden));
    }

    /// <summary>BG2-only endpoint of $81:AC2D; room-map cells are not installed until $81:AD17.</summary>
    /// <returns>Owned 256-by-224 RGBA frame buffer, valid until this method renders again; BG2 uses fixed scroll (0, 24), with no map icons or timing updates.</returns>
    public Rgba32[] RenderFrameOnly()
    {
        Rgba32[] pixels = frameOnlyBuffer ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, Cgram.GetRgba(0));
        // Composite directly: transparent cells leave the backdrop, as compositing a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels,
            Vram, Cgram, MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters,
            new Position16<BgPlanePixels>(0, 24), new Extent16<ScreenPixels>(FrontendFrame.Width, FrontendFrame.Height), new Extent16<TilemapCells>(32, 32));
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
        ArgumentNullException.ThrowIfNull(marker);
        Rgba32[] pixels = RenderBackgrounds(scroll);
        OamBuffer oam = PrepareIcons(scroll, marker, animations);
        Rgba32[] objects = objScratch ??= new Rgba32[256 * 224];
        SnesObjRenderer.Render(objects, oam, Vram, Cgram, obsel: 0x03);
        SnesLayerCompositor.Composite(pixels, objects);
        return pixels;
    }

    // OBJ scratch reused across draws; never part of saved state (restores reallocate it).
    [NonSerialized] private Rgba32[]? objScratch;
    // Owned frames: each is valid until the same method renders again.
    [NonSerialized] private Rgba32[]? frameOnlyBuffer;
    [NonSerialized] private Rgba32[]? backgroundsBuffer;

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
    public Rgba32[] RenderBackgrounds(Position16<MapPixels> scroll)
    {
        Rgba32[] pixels = backgroundsBuffer ??= new Rgba32[256 * 224];
        Array.Fill(pixels, ppu.Cgram.GetRgba(0));
        for (int tier = 0; tier < 2; tier++)
        {
            // Mode 1 orders BG2 before BG1 within each background priority tier.
            bool priority = tier == 1;
            // Composited directly into the frame; transparent pixels leave it untouched.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels,
                Vram, Cgram, MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters,
                new Position16<BgPlanePixels>(0, 24), SnesPpuLayout.ScreenSize, new Extent16<TilemapCells>(32, 32), priority: priority);
            SnesBgTilemapRenderer.Composite4BppViewport(pixels,
                Vram, Cgram, MenuPpuState.Bg1TilemapWord, FileSelectMapRomData.RoomCharacters,
                scroll.AsBgPlaneScroll(), SnesPpuLayout.ScreenSize, priority: priority);
        }
        return pixels;
    }
}
