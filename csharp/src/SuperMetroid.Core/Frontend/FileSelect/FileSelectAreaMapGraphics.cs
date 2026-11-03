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
/// Picture and backdrop owners for the saved-game world map: each area's pictures are pre-coloured in its world-map colours
/// by the import (#1512). Labels and transition windows are separate owners; this renders the unmasked BGs.
/// </summary>
public sealed class FileSelectAreaMapGraphics
{
    /// <summary>The map's menu screen: its artwork, backdrop and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>The installed world-map backdrops, rebound when presentation content changes.</summary>
    [AllowNull]
    private MenuBackdrops Backdrops { get => field ?? throw new InvalidOperationException("No area-map backdrops are installed."); set; }
    /// <summary>Installed world-label positions used when placing the screen's sprites.</summary>
    private WorldMapLabelPositions? labels;
    /// <summary>The installed sheets the world map's foreground and each area's background are pictured in (#1467).</summary>
    private InstalledSpriteSheets? worldSheets;
    /// <summary>The installed map sprite sheet the title and area labels draw from.</summary>
    private SpriteSheet? mapSheet;
    /// <summary>Replaces label positions without changing the selected area or display state.</summary>
    internal void BindLabels(WorldMapLabelPositions? content) => labels = content;

    /// <summary>Area-map graphics bound to every installed resource the file-select map draws.</summary>
    public static FileSelectAreaMapGraphics FromPresentation(int selectedArea,
        AreaMapPresentation presentation, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(presentation);
        var graphics = new FileSelectAreaMapGraphics(selectedArea, presentation.Backdrops,
            presentation.WorldArtwork, sheets);
        graphics.BindLabels(presentation.Labels);
        return graphics;
    }

    /// <summary>Initializes the unmasked world-map picture and backdrop owners from installed artwork and selects the requested Zebes area; labels must be bound separately or through <see cref="FromPresentation"/>.</summary>
    /// <param name="selectedArea">Game-area identity 0..5, not the file-select label display-order ordinal.</param>
    /// <param name="backdrops">Required menu backdrops, including each area's world-map backdrop.</param>
    /// <param name="worldArtwork">Required world-map character artwork.</param>

    /// <param name="sheets">Required installed sprite sheets: the map sprites the title and labels draw from, and the background tiles.</param>
    /// <exception cref="InvalidOperationException">Any required presentation resource is absent despite its optional parameter default.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The selected area is outside the six Zebes identities.</exception>
    public FileSelectAreaMapGraphics(int selectedArea, MenuBackdrops? backdrops = null,
        WorldMapArtwork? worldArtwork = null, InstalledSpriteSheets? sheets = null)
    {
        if (backdrops is null || worldArtwork is null || sheets is null)
            throw new InvalidOperationException("Area-map graphics require installed presentation assets.");
        Backdrops = backdrops;
        worldSheets = sheets;
        mapSheet = sheets.Get(SpriteSheetId.MapSprites);
        screen = new MenuScreen(backdrops.FileSelect, worldArtwork, sheets);
        SelectArea(selectedArea);
    }

    /// <summary>Selected game-area identity 0..5 controlling the background, backdrop, and label highlight; this does not select a gameplay load station.</summary>
    public int SelectedArea { get; private set; }

    /// <summary>Shows the installed area's world-map backdrop before publishing its selection, without advancing menu input, windows, or transition timers.</summary>
    /// <param name="selectedArea">Game-area identity 0..5.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is outside the supported Zebes identities.</exception>
    public void SelectArea(int selectedArea)
    {
        if ((uint)selectedArea >= FileSelectMapDefinitions.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(selectedArea));
        var area = AreaIds.At(selectedArea);
        screen.Backdrop = Backdrops.World(area);
        SelectedArea = selectedArea;
    }

    /// <summary>
    /// Binds every installed resource the area map draws, when the game is bound to its installed content, without
    /// selecting another area.
    /// </summary>
    internal void BindMapPresentation(AreaMapPresentation? catalog)
    {
        BindLabels(catalog?.Labels);
        BindBackdrops(catalog?.Backdrops);
        screen.BindWorldArtwork(catalog?.WorldArtwork);
    }

    /// <summary>Replaces the installed backdrops and shows the current selection's.</summary>
    private void BindBackdrops(MenuBackdrops? content)
    {
        Backdrops = content;
        if (content is not null) screen.Backdrop = content.World(AreaIds.At(SelectedArea));
    }

    /// <summary>Rebinds the installed sheets: the map sprites the title and labels draw from, and the background tiles.</summary>
    internal void BindSheets(InstalledSpriteSheets? sheets)
    {
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Area-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        worldSheets = installed;
        screen.BindScreenTiles(installed);
    }

    /// <summary>Builds title and valid used-station labels in display order, highlighting the selected area.</summary>
    private void PrepareLabels(PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        ScreenSprites entries = screen.Objects;
        entries.BeginFrame();
        if (labels is null || mapSheet is null)
            throw new InvalidOperationException("World-map labels require installed layout and sprite assets.");
        Draw(MapSpriteId.WorldTitle, new(128, 16), MapSpriteColouring.Labels);
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
                shownIndex == SelectedArea ? MapSpriteColouring.Labels : MapSpriteColouring.DimLabels);
        }

        void Draw(MapSpriteId id, Position16<SpritePixels> origin, MapSpriteColouring colouring)
        {
            MapSpriteSheet.Draw(mapSheet, id, entries, origin, MapSpriteScreen.FileSelect, colouring);
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
        // The foreground and the area's background are their pictures in the area's world-map colours (#1467, #1512).
        InstalledSpriteSheets sheets = worldSheets ?? throw new InvalidOperationException("The world map's pictures are bound when the game is bound to its installed content.");
        AreaId area = AreaIds.At(SelectedArea);
        var foreground = new PicturedPlaneLayer(new PicturedPlane(MenuScreen.PageSize,
            [new PlacedPlanePicture(sheets.Get(ScreenTileSheetDefinitions.AreaMapForegrounds[area]).Frame(AreaMapPictures.WorldForegroundFrame), default, PictureShading.Standard)]), default, null);
        var background = new PicturedPlane(MenuScreen.PageSize,
            [new PlacedPlanePicture(sheets.Get(ScreenTileSheetDefinitions.AreaMapBackgrounds[area]).Frame(AreaMapPictures.WorldBackgroundFrame), default, PictureShading.Standard)]);
        return new(screen.Paint(), new DisplayLayer[] {
            foreground,
            new AddedPicturedPlaneLayer(background, colorMath == AreaMapColorMath.WithBackdrop ? null : foreground) }, DisplayBrightness.Full);
    }
}
