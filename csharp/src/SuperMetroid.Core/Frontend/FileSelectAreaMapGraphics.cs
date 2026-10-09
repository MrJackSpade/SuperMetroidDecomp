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
    /// <summary>Address space used by the menu PPU when binding artwork and screen resources.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Owned menu PPU state containing this map's VRAM and CGRAM.</summary>
    private readonly MenuPpuState ppu;
    /// <summary>Installed palette source used when an area selection loads its colors.</summary>
    [NonSerialized] private MapStaticPalettes? palettes;
    /// <summary>Optional station-label coordinates, bound separately from the background artwork.</summary>
    [NonSerialized] private WorldMapLabelLayout? labels;
    /// <summary>Installed foreground and per-area background tilemaps.</summary>
    [NonSerialized] private MapScreenPresentation? screens;
    /// <summary>Installed OBJ graphics used to draw the map title and unlocked area labels.</summary>
    [NonSerialized] private MapSpriteCatalog? sprites;
    /// <summary>Binds label coordinates independently of the map's background and palette resources.</summary>
    /// <param name="content">Label layout to use, or null when labels are not yet installed.</param>
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
        LoadInstalledPalette(selectedArea);
        LoadBackground(selectedArea);
        SelectedArea = selectedArea;
    }

    /// <summary>Copies the fixed world-map foreground tilemap into the owned BG1 tilemap region.</summary>
    private void LoadForeground()
    {
        (screens ?? throw new InvalidOperationException("World map requires installed screen assets."))
            .LoadTo(Vram, MenuPpuState.Bg1TilemapWord * 2, MapScreenDefinitions.WorldForeground);
    }

    /// <summary>Loads the background tilemap associated with the selected game area.</summary>
    /// <param name="selectedArea">Validated Zebes area identity used to select its background resource.</param>
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

    /// <summary>Replaces the palette source and immediately refreshes CGRAM for the current selection.</summary>
    /// <param name="content">Installed world-map palette data, or null while assets are unavailable.</param>
    internal void BindPalettes(MapStaticPalettes? content)
    {
        palettes = content;
        if (content is not null) LoadInstalledPalette(SelectedArea);
    }
    /// <summary>Replaces the OBJ sprite catalog and updates the menu PPU's sprite binding.</summary>
    /// <param name="content">Map-label sprite data, or null when the artwork is not installed.</param>
    internal void BindSprites(MapSpriteCatalog? content) { sprites = content; ppu.BindMapSprites(bus, content); }

    /// <summary>Writes the selected area's installed colors into the owned CGRAM.</summary>
    /// <param name="selectedArea">Validated game-area identity selecting the world-map palette.</param>
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
    /// <summary>Reusable RGBA target for rasterizing the map's BG1 foreground layer.</summary>
    [NonSerialized] private Rgba32[]? foregroundScratch;
    /// <summary>Reusable RGBA target for rasterizing the additive BG3 subscreen.</summary>
    [NonSerialized] private Rgba32[]? subscreenScratch;
    /// <summary>Reusable RGBA target for the map's OBJ label layer.</summary>
    [NonSerialized] private Rgba32[]? objScratch;
    // Owned frame: valid until this map renders again.
    /// <summary>Reusable composed frame returned to callers until the next render overwrites it.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Composes the unmasked 256-by-224 BG1 main scene plus additive BG3 subscreen, excluding OBJ labels and transition clipping; no menu state advances.</summary>
    /// <param name="includeBackdropInColorMath">True for steady/return $25 color math; false for forward-window $05 behavior, where transparent BG1 cells retain the backdrop without BG3 addition.</param>
    /// <returns>The owned RGBA frame, valid until this graphics owner renders again.</returns>
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

    /// <summary>Builds OBJ entries for the title and areas with at least one used station marker.</summary>
    /// <param name="usedStationMasks">Per-area station-use bits that determine which area labels are visible.</param>
    /// <returns>Finalized OAM data ready for the map's OBJ renderer.</returns>
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
