using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Map.Artwork;
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
    [NonSerialized] private WorldMapLabelPositions? labels;
    /// <summary>The installed sheets the world map's foreground and each area's background are pictured in (#1467).</summary>
    [NonSerialized] private InstalledSpriteSheets? worldSheets;
    /// <summary>The installed map sprite sheet the title and area labels draw from.</summary>
    [NonSerialized] private SpriteSheet? mapSheet;
    /// <summary>Replaces label positions without changing the selected area or PPU state.</summary>
    internal void BindLabels(WorldMapLabelPositions? content) => labels = content;

    /// <summary>Area-map graphics bound to every installed resource the file-select map draws.</summary>
    public static FileSelectAreaMapGraphics FromPresentation(int selectedArea,
        AreaMapPresentation presentation, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(presentation);
        var graphics = new FileSelectAreaMapGraphics(selectedArea, presentation.Palettes,
            presentation.WorldArtwork, sheets);
        graphics.BindLabels(presentation.Labels);
        return graphics;
    }

    /// <summary>Initializes the unmasked world-map BG/palette owners from installed artwork and selects the requested Zebes area; labels must be bound separately or through <see cref="FromPresentation"/>.</summary>
    /// <param name="selectedArea">Game-area identity 0..5, not the file-select label display-order ordinal.</param>
    /// <param name="mapPalettes">Required world-map palettes, including area-specific selections.</param>
    /// <param name="worldArtwork">Required world-map character artwork.</param>

    /// <param name="sheets">Required installed sprite sheets: the map sprites the title and labels draw from, and the background tiles.</param>
    /// <exception cref="InvalidOperationException">Any required presentation resource is absent despite its optional parameter default.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The selected area is outside the six Zebes identities.</exception>
    public FileSelectAreaMapGraphics(int selectedArea, MapStaticPalettes? mapPalettes = null,
        WorldMapArtwork? worldArtwork = null, InstalledSpriteSheets? sheets = null)
    {
        if (mapPalettes is null || worldArtwork is null || sheets is null)
            throw new InvalidOperationException("Area-map graphics require installed presentation assets.");
        Palettes = mapPalettes;
        worldSheets = sheets;
        mapSheet = sheets.Get(SpriteSheetId.MapSprites);
        screen = new MenuScreen(mapPalettes, worldArtwork, sheets);
        // State one completes its first-two-palette fade with these entries black.
        FileSelectMapEntry.BlackenEntryColours(screen.Palette);
        SelectArea(selectedArea);
    }

    /// <summary>Selected game-area identity 0..5 controlling the background, palette, and label highlight; this does not select a gameplay load station.</summary>
    public int SelectedArea { get; private set; }

    /// <summary>Loads the installed area's world-map palette before publishing its selection, without advancing menu input, windows, or transition timers.</summary>
    /// <param name="selectedArea">Game-area identity 0..5.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is outside the supported Zebes identities.</exception>
    public void SelectArea(int selectedArea)
    {
        if ((uint)selectedArea >= FileSelectMapDefinitions.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(selectedArea));
        var area = AreaIds.At(selectedArea);
        LoadInstalledPalette(area);
        SelectedArea = selectedArea;
    }

    /// <summary>
    /// Rebinds every installed resource the area map draws, after a content change or debugger restoration, without
    /// selecting another area or restarting its palette fade.
    /// </summary>
    internal void BindMapPresentation(AreaMapPresentation? catalog)
    {
        BindLabels(catalog?.Labels);
        BindPalettes(catalog?.Palettes);
        screen.BindWorldArtwork(catalog?.WorldArtwork);
    }

    /// <summary>Replaces installed area palettes and refreshes CGRAM for the current selection.</summary>
    private void BindPalettes(MapStaticPalettes? content)
    {
        Palettes = content;
        if (content is not null) LoadInstalledPalette(AreaIds.At(SelectedArea));
    }

    /// <summary>Rebinds the installed sheets: the map sprites the title and labels draw from, and the background tiles.</summary>
    internal void BindSheets(InstalledSpriteSheets? sheets)
    {
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Area-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        worldSheets = installed;
        screen.BindScreenTiles(installed);
    }

    /// <summary>Loads the selected area's installed world colors into menu CGRAM.</summary>
    private void LoadInstalledPalette(AreaId selectedArea)
    {
        Palettes.World(selectedArea).LoadTo(screen.Palette);
    }

    /// <summary>Builds title and valid used-station labels in display order, highlighting the selected area.</summary>
    private void PrepareLabels(PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        ScreenSprites entries = screen.Objects;
        entries.BeginFrame();
        if (labels is null || mapSheet is null)
            throw new InvalidOperationException("World-map labels require installed layout and sprite assets.");
        Draw(MapSpriteId.WorldTitle, new(128, 16), ScreenColourGroup.MapLabels);
        for (int displayArea = 0; displayArea < FileSelectMapDefinitions.AreaCount; displayArea++)
        {
            AreaId area = FileSelectMapAreaOrder.Get(displayArea);
            if (!AreaIds.IsZebes(area))
                throw new InvalidDataException("File-select map display table contains an invalid area.");
            int index = AreaIds.ToIndex(area);
            if (MapSaveMarkers.HasUsedMarker(area, progress.UsedStations(area)))
                DrawArea(area, index);
        }
        entries.FinishFrame();

        void DrawArea(AreaId shown, int shownIndex)
        {
            Draw(MapSpriteNames.WorldLabel(shown), labels.Get(shownIndex),
                shownIndex == SelectedArea ? ScreenColourGroup.MapLabels : ScreenColourGroup.MapDimLabels);
        }

        void Draw(MapSpriteId id, Position16<SpritePixels> origin, ScreenColourGroup colouring)
        {
            MapSpriteSheet.Draw(mapSheet, id, entries, origin, colouring);
        }
    }

    /// <summary>Captures BG1/subscreen math followed by labels, preserving OBJ exclusion from color addition.</summary>
    public LayeredScene CaptureRenderSnapshot(PersistentProgress progress, AreaMapColorMath colorMath = AreaMapColorMath.WithBackdrop)
    {
        PrepareLabels(progress);
        LayeredScene backgrounds = CaptureBackgrounds(colorMath);
        return new(backgrounds.Paint, [.. backgrounds.Layers, new SpritesLayer()], DisplayBrightness.Full);
    }

    /// <summary>The area map's BG1 foreground with its additive BG3 background, without the labels.</summary>
    /// <param name="colorMath">Whether the background is also added to the backdrop.</param>
    internal LayeredScene CaptureBackgrounds(AreaMapColorMath colorMath)
    {
        // The foreground and the area's background are their pictures under the area's world-map palette (#1467).
        InstalledSpriteSheets sheets = worldSheets ?? throw new InvalidOperationException("The world map's pictures are rebound after debugger restoration.");
        AreaId area = AreaIds.At(SelectedArea);
        var foreground = new PicturedPlaneLayer(new PicturedPlane(MenuScreen.PageSize,
            [new PlacedPlanePicture(sheets.Get(ScreenTileSheetDefinitions.AreaMapForegrounds[area]).Frame(AreaMapPictures.WorldForegroundFrame), default)]), default, null);
        var background = new PicturedPlane(MenuScreen.PageSize,
            [new PlacedPlanePicture(sheets.Get(ScreenTileSheetDefinitions.AreaMapBackgrounds[area]).Frame(AreaMapPictures.WorldBackgroundFrame), default)]);
        return new(screen.Paint(), new DisplayLayer[] {
            foreground,
            new AddedPicturedPlaneLayer(background, colorMath == AreaMapColorMath.WithBackdrop ? null : foreground) }, DisplayBrightness.Full);
    }
}
