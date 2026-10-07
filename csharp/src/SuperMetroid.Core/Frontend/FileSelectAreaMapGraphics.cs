using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Tile and palette owners for the saved-game world map, from $81:A37C-$81:A58A.
/// Labels and transition windows are separate owners; this renders the unmasked BGs.
/// </summary>
public sealed partial class FileSelectAreaMapGraphics
{
    private readonly ISnesAddressSpace bus;
    private readonly MenuPpuState ppu;
    [NonSerialized] private MapStaticPalettes? palettes;
    [NonSerialized] private WorldMapLabelLayout? labels;
    [NonSerialized] private MapScreenPresentation? screens;
    [NonSerialized] private MapSpriteCatalog? sprites;
    internal void BindLabels(WorldMapLabelLayout? content) => labels = content;

    /// <summary>Area-map graphics bound to every installed resource the file-select map draws.</summary>
    public static FileSelectAreaMapGraphics FromPresentation(ISnesAddressSpace bus, int selectedArea,
        AreaMapPresentationCatalog presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        var graphics = new FileSelectAreaMapGraphics(bus, selectedArea, presentation.Tiles, presentation.Palettes,
            presentation.Screens, presentation.WorldArtwork, presentation.Sprites);
        graphics.BindLabels(presentation.Labels);
        return graphics;
    }

    public FileSelectAreaMapGraphics(ISnesAddressSpace bus, int selectedArea, MapTileAtlas? mapTiles = null, MapStaticPalettes? mapPalettes = null,
        MapScreenPresentation? mapScreens = null, WorldMapArtwork? worldArtwork = null, MapSpriteCatalog? mapSprites = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        if (mapTiles is null || mapPalettes is null || mapScreens is null ||
            worldArtwork is null || mapSprites is null)
            throw new InvalidOperationException("Area-map graphics require installed presentation assets.");
        palettes = mapPalettes;
        screens = mapScreens;
        sprites = mapSprites;
        ppu = new MenuPpuState(bus, mapTiles, mapPalettes, worldArtwork, mapSprites,
            loadInitialBackground: false);
        LoadForeground();
        // State one completes its first-two-palette fade with these entries black.
        ppu.Cgram.SetColor(14, 0);
        ppu.Cgram.SetColor(30, 0);
        SelectArea(selectedArea);
    }

    public SnesVram Vram => ppu.Vram;
    public SnesCgram Cgram => ppu.Cgram;
    public int SelectedArea { get; private set; }

    public void SelectArea(int selectedArea)
    {
        if ((uint)selectedArea >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(selectedArea));
        LoadInstalledPalette(selectedArea);
        LoadBackground(selectedArea);
        SelectedArea = selectedArea;
    }

    private void LoadForeground()
    {
        (screens ?? throw new InvalidOperationException("World map requires installed screen assets."))
            .LoadTo(Vram, MenuPpuState.Bg1TilemapWord * 2, MapScreenDefinitions.WorldForeground);
    }

    private void LoadBackground(int selectedArea)
    {
        (screens ?? throw new InvalidOperationException("World map requires installed screen assets."))
            .LoadTo(Vram, FileSelectMapRomData.AreaBackgroundVram * 2,
                MapScreenDefinitions.WorldBackground((AreaId)selectedArea));
    }

    /// <summary>Refresh current layer artwork without selecting another area or restarting its palette fade.</summary>
    internal void BindScreens(MapScreenPresentation? content, WorldMapArtwork? artwork)
    {
        screens = content;
        ppu.BindWorldArtwork(bus, artwork);
        LoadForeground();
        LoadBackground(SelectedArea);
    }

    internal void BindPalettes(MapStaticPalettes? content)
    {
        palettes = content;
        if (content is not null) LoadInstalledPalette(SelectedArea);
    }
    internal void BindSprites(MapSpriteCatalog? content) { sprites = content; ppu.BindMapSprites(bus, content); }

    private void LoadInstalledPalette(int selectedArea)
    {
        var colors = palettes!.World((SuperMetroid.Core.Game.AreaId)selectedArea);
        for (int color = 0; color < colors.Length; color++) Cgram.SetColor(color, colors[color]);
    }

    /// <summary>
    /// Steady state six leaves BG1 on main and BG3 on sub, with additive color math
    /// enabled for BG1 and backdrop. Menu OBJ labels must be composed afterwards.
    /// </summary>
    // Layer scratch reused across draws; never part of saved state (restores reallocate it).
    [NonSerialized] private Rgba32[]? foregroundScratch;
    [NonSerialized] private Rgba32[]? subscreenScratch;
    [NonSerialized] private Rgba32[]? objScratch;
    // Owned frame: valid until this map renders again.
    [NonSerialized] private Rgba32[]? frameBuffer;

    public Rgba32[] RenderBackgrounds(bool includeBackdropInColorMath = true)
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);
        Rgba32[] foreground = foregroundScratch ??= new Rgba32[256 * 224];
        foreground.AsSpan().Clear();
        SnesBgTilemapRenderer.Composite4BppViewport(
            foreground, ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        SnesLayerCompositor.Composite(pixels, foreground);
        Rgba32[] subscreen = subscreenScratch ??= new Rgba32[256 * 28 * 8];
        subscreen.AsSpan().Clear();
        SnesBgTilemapRenderer.Render2Bpp(subscreen, ppu.Vram, ppu.Cgram,
            FileSelectMapRomData.AreaBackgroundVram, FileSelectMapRomData.AreaBackgroundCharacters,
            28, transparentColorZero: true);
        // $81:AAAC changes CGADSUB from $25 to $05: BG1 still adds BG3, but
        // transparent BG1 cells now retain the backdrop instead of adding BG3 to it.
        if (!includeBackdropInColorMath)
            for (int pixel = 0; pixel < subscreen.Length; pixel++)
                if (foreground[pixel].A == 0) subscreen[pixel] = default;
        SnesLayerCompositor.AddSubscreen(pixels, subscreen);
        return pixels;
    }

    /// <summary>
    /// Adds $81:A97E's label objects. Retail enables a label only if a used station
    /// bit refers to a real map coordinate, not an unused $FFFE or end $FFFF entry.
    /// Debug-only area/station navigation is deliberately not enabled here.
    /// </summary>
    public Rgba32[] Render(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true)
    {
        if (usedStationMasks.Length < FileSelectMapRomData.AreaCount)
            throw new ArgumentException("Area labels require six used-station masks.", nameof(usedStationMasks));
        Rgba32[] frame = RenderBackgrounds(includeBackdropInColorMath);
        OamBuffer oam = PrepareLabels(usedStationMasks);
        // CGADSUB excludes OBJ: labels are composited after BG1/subscreen addition.
        Rgba32[] objects = objScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objects, oam, ppu.Vram, ppu.Cgram, obsel: 0x03);
        SnesLayerCompositor.Composite(frame, objects);
        return frame;
    }

    private OamBuffer PrepareLabels(ReadOnlySpan<ushort> usedStationMasks)
    {
        if (usedStationMasks.Length < FileSelectMapRomData.AreaCount)
            throw new ArgumentException("Area labels require six used-station masks.", nameof(usedStationMasks));
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (labels is null || sprites is null)
            throw new InvalidOperationException("World-map labels require installed layout and sprite assets.");
        ushort title = MapSpriteDefinitions.WorldTitle;
        Draw(title, 128, 16, 0);
        for (int displayArea = 0; displayArea < FileSelectMapRomData.AreaCount; displayArea++)
        {
            ushort area = (ushort)FileSelectMapAreaOrder.Get(displayArea);
            if (area >= FileSelectMapRomData.AreaCount)
                throw new InvalidDataException("File-select map display table contains an invalid area.");
            if (MapSaveMarkerDefinitions.HasUsedMarker((AreaId)area, usedStationMasks[area]))
                DrawArea(area);
        }
        oam.FinalizeFrame();
        return oam;

        void DrawArea(ushort area)
        {
            Draw((ushort)(title + area + 1), (ushort)labels.Get(area).X,
                (ushort)labels.Get(area).Y, area == SelectedArea ? (ushort)0 : (ushort)0x200);
        }

        void Draw(ushort id, ushort x, ushort y, ushort palette)
        {
            sprites.Draw(id, oam, x, y, palette);
        }
    }
}
