using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Room-select BG1 map and BG2 frame installed by $81:A725 and $82:9517.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed partial class FileSelectRoomMapGraphics
{
    private readonly ISnesAddressSpace bus;
    private readonly MenuPpuState ppu;
    private readonly FileSelectMapIcons icons;
    [NonSerialized] private MapSpriteCatalog? sprites;
    public SnesVram Vram => ppu.Vram;
    public SnesCgram Cgram => ppu.Cgram;
    internal Bank80SystemState MapSystem => icons.MapSystem;

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
        ppu.BindMapSprites(bus, sprites);
        if (catalog is not null)
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color < MapAnimationRomData.PaletteDestination || color >= MapAnimationRomData.PaletteDestination + MapPaletteCycleFormat.ColorCount)
                    Cgram.SetColor(color, catalog.Palettes.FileSelect[color]);
        (catalog ?? throw new InvalidOperationException(
            "Room-map graphics require installed map presentation assets."))
            .Tiles.LoadTo(Vram, FileSelectMapRomData.RoomCharacters * 2);
        var system = icons.MapSystem;
        var area = icons.MapArea;
        ppu.BindWorldArtwork(bus, catalog?.WorldArtwork);
        LoadFrame(catalog?.Screens, area);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        ppu.Vram.LoadBytes(MenuPpuState.Bg1TilemapWord * 2,
            AreaMapTilemapBuilder.Build((catalog ?? throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.")).Get(area), system, hidden));
    }

    /// <summary>BG2-only endpoint of $81:AC2D; room-map cells are not installed until $81:AD17.</summary>
    public Rgba32[] RenderFrameOnly()
    {
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, Cgram.GetRgba(0));
        // Composite directly: transparent cells leave the backdrop, as compositing a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels,
            Vram, Cgram, MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters,
            0, 24, FrontendFrame.Width, FrontendFrame.Height, 32, 32);
        return pixels;
    }

    /// <summary>Draws the saved-station marker over the room map without advancing its animation.</summary>
    public Rgba32[] Render(ushort horizontalScroll, ushort verticalScroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations = null)
    {
        ArgumentNullException.ThrowIfNull(marker);
        Rgba32[] pixels = RenderBackgrounds(horizontalScroll, verticalScroll);
        OamBuffer oam = PrepareIcons(horizontalScroll, verticalScroll, marker, animations);
        Rgba32[] objects = objScratch ??= new Rgba32[256 * 224];
        SnesObjRenderer.Render(objects, oam, Vram, Cgram, obsel: 0x03);
        SnesLayerCompositor.Composite(pixels, objects);
        return pixels;
    }

    // OBJ scratch reused across draws; never part of saved state (restores reallocate it).
    [NonSerialized] private Rgba32[]? objScratch;

    private OamBuffer PrepareIcons(ushort horizontalScroll, ushort verticalScroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        icons.DrawBeforeMarker(oam, horizontalScroll, verticalScroll);
        marker.Draw(bus, oam, horizontalScroll, verticalScroll, sprites);
        icons.DrawAfterMarker(oam, horizontalScroll, verticalScroll,
            animations is null ? null : () => animations.DrawArrows(oam, sprites));
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Renders Mode-1 BG priorities with independent scrolling for map and fixed frame.</summary>
    public Rgba32[] RenderBackgrounds(ushort horizontalScroll, ushort verticalScroll)
    {
        var pixels = new Rgba32[256 * 224];
        Array.Fill(pixels, ppu.Cgram.GetRgba(0));
        for (int tier = 0; tier < 2; tier++)
        {
            // Mode 1 orders BG2 before BG1 within each background priority tier.
            bool priority = tier == 1;
            // Composited directly into the frame; transparent pixels leave it untouched.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels,
                Vram, Cgram, MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters,
                0, 24, 256, 224, 32, 32, priority: priority);
            SnesBgTilemapRenderer.Composite4BppViewport(pixels,
                Vram, Cgram, MenuPpuState.Bg1TilemapWord, FileSelectMapRomData.RoomCharacters,
                horizontalScroll, verticalScroll, 256, 224, priority: priority);
        }
        return pixels;
    }
}
