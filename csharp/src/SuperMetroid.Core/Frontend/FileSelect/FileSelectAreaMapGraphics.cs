using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Tile and palette owners for the saved-game world map, from $81:A37C-$81:A58A.
/// Labels and transition windows are separate owners; this renders the unmasked BGs.
/// </summary>
public sealed class FileSelectAreaMapGraphics
{
    /// <summary>Runtime address space retained for menu PPU setup.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Owns the map's VRAM, CGRAM, and installed tile and sprite resources.</summary>
    private readonly MenuPpuState ppu;
    /// <summary>The installed area-map world colors, rebound when presentation content changes.</summary>
    [field: NonSerialized]
    [AllowNull]
    private MapStaticPalettes Palettes { get => field ?? throw new InvalidOperationException("No area-map world colors are installed."); set; }
    /// <summary>Installed world-label positions used when building OAM.</summary>
    [NonSerialized] private WorldMapLabelLayout? labels;
    /// <summary>Installed foreground and per-area background tilemaps.</summary>
    [NonSerialized] private MapScreenPresentation? screens;
    /// <summary>Installed OBJ art for the title and area labels.</summary>
    [NonSerialized] private MapSpriteCatalog? sprites;
    /// <summary>Replaces label positions without changing the selected area or PPU state.</summary>
    internal void BindLabels(WorldMapLabelLayout? content) => labels = content;

    /// <summary>Area-map graphics bound to every installed resource the file-select map draws.</summary>
    public static FileSelectAreaMapGraphics FromPresentation(ISnesAddressSpace bus, int selectedArea,
        AreaMapPresentationCatalog presentation)
    {
        Ensure.NotNull(presentation);
        var graphics = new FileSelectAreaMapGraphics(bus, selectedArea, presentation.Tiles, presentation.Palettes,
            presentation.Screens, presentation.WorldArtwork, presentation.Sprites);
        graphics.BindLabels(presentation.Labels);
        return graphics;
    }

    /// <summary>Initializes the unmasked world-map BG/palette owners from installed artwork and selects the requested Zebes area; labels must be bound separately or through <see cref="FromPresentation"/>.</summary>
    /// <param name="bus">Runtime address space used by menu PPU setup.</param>
    /// <param name="selectedArea">Game-area identity 0..5, not the file-select label display-order ordinal.</param>
    /// <param name="mapTiles">Required installed map-character atlas.</param>
    /// <param name="mapPalettes">Required world-map palettes, including area-specific selections.</param>
    /// <param name="mapScreens">Required foreground and per-area background tilemap resources.</param>
    /// <param name="worldArtwork">Required world-map character artwork.</param>
    /// <param name="mapSprites">Required map-label OBJ artwork.</param>
    /// <exception cref="InvalidOperationException">Any required presentation resource is absent despite its optional parameter default.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The selected area is outside the six Zebes identities.</exception>
    public FileSelectAreaMapGraphics(ISnesAddressSpace bus, int selectedArea, MapTileAtlas? mapTiles = null, MapStaticPalettes? mapPalettes = null,
        MapScreenPresentation? mapScreens = null, WorldMapArtwork? worldArtwork = null, MapSpriteCatalog? mapSprites = null)
    {
        this.bus = Ensure.NotNull(bus);
        if (mapTiles is null || mapPalettes is null || mapScreens is null ||
            worldArtwork is null || mapSprites is null)
            throw new InvalidOperationException("Area-map graphics require installed presentation assets.");
        Palettes = mapPalettes;
        screens = mapScreens;
        sprites = mapSprites;
        ppu = new MenuPpuState(bus, mapTiles, mapPalettes, worldArtwork, mapSprites,
            loadInitialBackground: false);
        LoadForeground();
        // State one completes its first-two-palette fade with these entries black.
        new FrontendPalettes(ppu.Cgram).FileSelectEntryBlackFirst.Write(0, Bgr555.Black);
        new FrontendPalettes(ppu.Cgram).FileSelectEntryBlackSecond.Write(0, Bgr555.Black);
        SelectArea(selectedArea);
    }

    /// <summary>Owned menu VRAM containing installed map characters, foreground/background tilemaps, and label OBJ art; not gameplay room VRAM.</summary>
    public SnesVram Vram => ppu.Vram;
    /// <summary>Owned menu CGRAM used for selected-area palette loading and entry fades; rendering reads it without advancing those fades.</summary>
    public SnesCgram Cgram => ppu.Cgram;
    /// <summary>Selected game-area identity 0..5 controlling the background, palette, and label highlight; this does not select a gameplay load station.</summary>
    public int SelectedArea { get; private set; }

    /// <summary>Loads the installed area's world-map palette and background before publishing its selection, without advancing menu input, windows, or transition timers.</summary>
    /// <param name="selectedArea">Game-area identity 0..5.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is outside the supported Zebes identities.</exception>
    public void SelectArea(int selectedArea)
    {
        if ((uint)selectedArea >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(selectedArea));
        var area = (AreaId)selectedArea;
        LoadInstalledPalette(area);
        LoadBackground(area);
        SelectedArea = selectedArea;
    }

    /// <summary>Copies the installed world foreground into menu BG1 tilemap VRAM.</summary>
    private void LoadForeground()
    {
        (screens ?? throw new InvalidOperationException("World map requires installed screen assets."))
            .LoadTo(new MenuVram(Vram).Bg1Tilemap, MapScreenDefinitions.WorldForeground);
    }

    /// <summary>Copies the selected area's installed background into menu BG3 tilemap VRAM.</summary>
    private void LoadBackground(AreaId selectedArea)
    {
        (screens ?? throw new InvalidOperationException("World map requires installed screen assets."))
            .LoadTo(new MenuVram(Vram).AreaBackgroundTilemap, MapScreenDefinitions.WorldBackground(selectedArea));
    }

    /// <summary>Refresh current layer artwork without selecting another area or restarting its palette fade.</summary>
    internal void BindScreens(MapScreenPresentation? content, WorldMapArtwork? artwork)
    {
        screens = content;
        ppu.BindWorldArtwork(artwork);
        LoadForeground();
        LoadBackground((AreaId)SelectedArea);
    }

    /// <summary>Replaces installed area palettes and refreshes CGRAM for the current selection.</summary>
    internal void BindPalettes(MapStaticPalettes? content)
    {
        Palettes = content;
        if (content is not null) LoadInstalledPalette((AreaId)SelectedArea);
    }
    /// <summary>Rebinds label OBJ art in both this owner and its menu PPU state.</summary>
    internal void BindSprites(MapSpriteCatalog? content) { sprites = content; ppu.BindMapSprites(content); }

    /// <summary>Loads the selected area's installed world colors into menu CGRAM.</summary>
    private void LoadInstalledPalette(AreaId selectedArea)
    {
        var colors = Palettes.World(selectedArea);
        new CgramPalettes(Cgram).LoadImage(colors);
    }

    // Layer scratch reused across draws; never part of saved state (restores reallocate it).
    /// <summary>Transient BG1 render plane reused across map draws and reallocated after restore.</summary>
    [NonSerialized] private Rgba32[]? foregroundScratch;
    /// <summary>Transient BG3 addition plane reused across map draws.</summary>
    [NonSerialized] private Rgba32[]? subscreenScratch;
    /// <summary>Transient OBJ label plane, composed after BG color math.</summary>
    [NonSerialized] private Rgba32[]? objScratch;
    // Owned frame: valid until this map renders again.
    /// <summary>Owned composite frame, overwritten by the next map render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Composes the unmasked 256-by-224 BG1 main scene plus additive BG3 subscreen, excluding OBJ labels and transition clipping; no menu state advances.</summary>
    /// <param name="includeBackdropInColorMath">True for steady/return $25 color math; false for forward-window $05 behavior, where transparent BG1 cells retain the backdrop without BG3 addition.</param>
    /// <returns>The owned RGBA frame, valid until this graphics owner renders again.</returns>
    [PpuLayoutBoundary("backend: file-select area-map software PPU composition")]
    public Rgba32[] RenderBackgrounds(bool includeBackdropInColorMath = true)
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);
        Rgba32[] foreground = foregroundScratch ??= new Rgba32[256 * 224];
        foreground.AsSpan().Clear();
        SnesBgTilemapRenderer.Composite4BppViewport(foreground, ppu.Vram, ppu.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg1TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) });
        SnesLayerCompositor.Composite(pixels, foreground);
        Rgba32[] subscreen = subscreenScratch ??= new Rgba32[256 * 28 * 8];
        subscreen.AsSpan().Clear();
        SnesBgTilemapRenderer.Render2Bpp(subscreen, ppu.Vram, ppu.Cgram, new BgVramAddresses { TilemapBaseWord = FileSelectMapRomData.AreaBackgroundVram, CharacterBaseWord = FileSelectMapRomData.AreaBackgroundCharacters }, 28, transparentColorZero: true);
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

    /// <summary>Builds title and valid used-station labels in display order, highlighting the selected area.</summary>
    private OamBuffer PrepareLabels(ReadOnlySpan<ushort> usedStationMasks)
    {
        if (usedStationMasks.Length < FileSelectMapRomData.AreaCount)
            throw new ArgumentException("Area labels require six used-station masks.", nameof(usedStationMasks));
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (labels is null || sprites is null)
            throw new InvalidOperationException("World-map labels require installed layout and sprite assets.");
        Draw(MapSpriteId.WorldTitle, new(128, 16), 0);
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
            Draw(MapSpriteDefinitions.WorldLabel((AreaId)area), labels.Get(area),
                area == SelectedArea ? (ushort)0 : (ushort)0x200);
        }

        void Draw(MapSpriteId id, Position16<OamPixels> origin, ushort palette)
        {
            sprites.Draw(id, oam, origin, palette);
        }
    }

    /// <summary>Captures BG1/subscreen math followed by labels, preserving OBJ exclusion from color addition.</summary>
    [PpuLayoutBoundary("codec: file-select area-map render packet layers")]
    public LayeredRenderSnapshot CaptureRenderSnapshot(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true)
    {
        OamBuffer oam = PrepareLabels(usedStationMasks);
        var foreground = new Bg4BppRenderLayer(MenuVramLayout.Bg1TilemapWord, 0, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null);
        return new(PpuMemorySnapshot.Capture(Vram, Cgram, oam), new RenderLayer[] {
            foreground,
            new BgSubscreenAddRenderLayer(FileSelectMapRomData.AreaBackgroundVram,
                FileSelectMapRomData.AreaBackgroundCharacters, includeBackdropInColorMath ? null : foreground),
            new ObjRenderLayer() }, FileSelectMapRomData.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
    }
}
