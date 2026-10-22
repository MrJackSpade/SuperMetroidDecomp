using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Tile and palette owners for the saved-game world map, from $81:A37C-$81:A58A.
/// Labels and transition windows are separate owners; this renders the unmasked BGs.
/// </summary>
public sealed class FileSelectAreaMapGraphics
{
    /// <summary>The map's menu screen: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>The installed area-map world colors, rebound when presentation content changes.</summary>
    [field: NonSerialized]
    [AllowNull]
    private MapStaticPalettes Palettes { get => field ?? throw new InvalidOperationException("No area-map world colors are installed."); set; }
    /// <summary>Installed world-label positions used when building OAM.</summary>
    [NonSerialized] private WorldMapLabelLayout? labels;
    /// <summary>Installed foreground and per-area background tilemaps.</summary>
    [NonSerialized] private MapScreenPresentation? screens;
    /// <summary>The installed map sprite sheet the title and area labels draw from.</summary>
    [NonSerialized] private SpriteSheet? mapSheet;
    /// <summary>Replaces label positions without changing the selected area or PPU state.</summary>
    internal void BindLabels(WorldMapLabelLayout? content) => labels = content;

    /// <summary>Area-map graphics bound to every installed resource the file-select map draws.</summary>
    public static FileSelectAreaMapGraphics FromPresentation(int selectedArea,
        AreaMapPresentationCatalog presentation, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(presentation);
        var graphics = new FileSelectAreaMapGraphics(selectedArea, presentation.Tiles, presentation.Palettes,
            presentation.Screens, presentation.WorldArtwork, presentation.Sprites, sheets);
        graphics.BindLabels(presentation.Labels);
        return graphics;
    }

    /// <summary>Initializes the unmasked world-map BG/palette owners from installed artwork and selects the requested Zebes area; labels must be bound separately or through <see cref="FromPresentation"/>.</summary>
    /// <param name="selectedArea">Game-area identity 0..5, not the file-select label display-order ordinal.</param>
    /// <param name="mapTiles">Required installed map-character atlas.</param>
    /// <param name="mapPalettes">Required world-map palettes, including area-specific selections.</param>
    /// <param name="mapScreens">Required foreground and per-area background tilemap resources.</param>
    /// <param name="worldArtwork">Required world-map character artwork.</param>
    /// <param name="mapSprites">Required map-label OBJ artwork.</param>
    /// <param name="sheets">Required installed sprite sheets: the map sprites the title and labels draw from, and the background tiles.</param>
    /// <exception cref="InvalidOperationException">Any required presentation resource is absent despite its optional parameter default.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The selected area is outside the six Zebes identities.</exception>
    public FileSelectAreaMapGraphics(int selectedArea, MapTileAtlas? mapTiles = null, MapStaticPalettes? mapPalettes = null,
        MapScreenPresentation? mapScreens = null, WorldMapArtwork? worldArtwork = null, MapSpriteCatalog? mapSprites = null,
        InstalledSpriteSheets? sheets = null)
    {
        if (mapTiles is null || mapPalettes is null || mapScreens is null ||
            worldArtwork is null || mapSprites is null || sheets is null)
            throw new InvalidOperationException("Area-map graphics require installed presentation assets.");
        Palettes = mapPalettes;
        screens = mapScreens;
        mapSheet = sheets.Get(SpriteSheetId.MapSprites);
        screen = new MenuScreen(mapTiles, mapPalettes, worldArtwork, mapSprites, sheets);
        LoadForeground();
        // State one completes its first-two-palette fade with these entries black.
        new FrontendPalettes(screen.Palette).FileSelectEntryBlackFirst.Write(0, Bgr555.Black);
        new FrontendPalettes(screen.Palette).FileSelectEntryBlackSecond.Write(0, Bgr555.Black);
        SelectArea(selectedArea);
    }

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
            .LoadTo(screen.Tilemaps.Bg1, MapScreenDefinitions.WorldForeground);
    }

    /// <summary>Copies the selected area's installed background into menu BG3 tilemap VRAM.</summary>
    private void LoadBackground(AreaId selectedArea)
    {
        (screens ?? throw new InvalidOperationException("World map requires installed screen assets."))
            .LoadTo(screen.Tilemaps.AreaBackground, MapScreenDefinitions.WorldBackground(selectedArea));
    }

    /// <summary>
    /// Rebinds every installed resource the area map draws, after a content change or debugger restoration, without
    /// selecting another area or restarting its palette fade.
    /// </summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        BindLabels(catalog?.Labels);
        BindPalettes(catalog?.Palettes);
        BindScreens(catalog?.Screens, catalog?.WorldArtwork);
        screen.BindMapTiles(catalog?.Tiles);
        BindSprites(catalog?.Sprites);
    }

    /// <summary>Refresh current layer artwork without selecting another area or restarting its palette fade.</summary>
    private void BindScreens(MapScreenPresentation? content, WorldMapArtwork? artwork)
    {
        screens = content;
        screen.BindWorldArtwork(artwork);
        LoadForeground();
        LoadBackground((AreaId)SelectedArea);
    }

    /// <summary>Replaces installed area palettes and refreshes CGRAM for the current selection.</summary>
    private void BindPalettes(MapStaticPalettes? content)
    {
        Palettes = content;
        if (content is not null) LoadInstalledPalette((AreaId)SelectedArea);
    }
    /// <summary>Rebinds the menu screen's label OBJ art.</summary>
    private void BindSprites(MapSpriteCatalog? content) => screen.BindMapSprites(content);

    /// <summary>Rebinds the installed sheets: the map sprites the title and labels draw from, and the background tiles.</summary>
    internal void BindSheets(InstalledSpriteSheets? sheets)
    {
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Area-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        screen.BindScreenTiles(installed);
    }

    /// <summary>Loads the selected area's installed world colors into menu CGRAM.</summary>
    private void LoadInstalledPalette(AreaId selectedArea)
    {
        var colors = Palettes.World(selectedArea);
        new CgramPalettes(screen.Palette).LoadImage(colors);
    }

    /// <summary>Builds title and valid used-station labels in display order, highlighting the selected area.</summary>
    private void PrepareLabels(ReadOnlySpan<ushort> usedStationMasks)
    {
        if (usedStationMasks.Length < FileSelectMapRomData.AreaCount)
            throw new ArgumentException("Area labels require six used-station masks.", nameof(usedStationMasks));
        ScreenSprites entries = screen.Objects;
        entries.BeginFrame();
        if (labels is null || mapSheet is null)
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
        entries.FinishFrame();

        void DrawArea(ushort area)
        {
            Draw(MapSpriteDefinitions.WorldLabel((AreaId)area), labels.Get(area),
                area == SelectedArea ? (ushort)0 : SnesObjPalettes.Palette(1).PaletteBits);
        }

        void Draw(MapSpriteId id, Position16<OamPixels> origin, ushort palette)
        {
            MapSpriteSheet.Draw(mapSheet, id, entries, origin, palette);
        }
    }

    /// <summary>Captures BG1/subscreen math followed by labels, preserving OBJ exclusion from color addition.</summary>
    [PpuLayoutBoundary("codec: file-select area-map render packet layers")]
    public LayeredRenderSnapshot CaptureRenderSnapshot(ReadOnlySpan<ushort> usedStationMasks, AreaMapColorMath colorMath = AreaMapColorMath.WithBackdrop)
    {
        PrepareLabels(usedStationMasks);
        // The foreground and the area's background draw from the area's pre-coloured tiles (#1392).
        ScreenCharacters tiles = screen.AreaMapTiles((AreaId)SelectedArea);
        var foreground = new Bg4BppRenderLayer(MenuVramLayout.Bg1TilemapWord, 0, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null,
            tiles.Pictures(0, BgBitDepth.FourBpp));
        return new(MenuScreenImage.Capture(screen), new RenderLayer[] {
            foreground,
            new BgSubscreenAddRenderLayer(FileSelectMapRomData.AreaBackgroundVram,
                FileSelectMapRomData.AreaBackgroundCharacters, colorMath == AreaMapColorMath.WithBackdrop ? null : foreground,
                Pictures: tiles.Pictures(FileSelectMapRomData.AreaBackgroundCharacters, BgBitDepth.TwoBpp)),
            new ObjRenderLayer() }, SnesPpuLayout.MaximumMasterBrightness);
    }
}
