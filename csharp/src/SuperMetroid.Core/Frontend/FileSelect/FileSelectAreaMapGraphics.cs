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
/// by the import (#1512). Labels and transition windows are separate owners; this renders the unmasked map layers.
/// </summary>
public sealed class FileSelectAreaMapGraphics
{
    /// <summary>The map's menu screen: its artwork, backdrop and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>The installed world-map backdrops, rebound when presentation content changes.</summary>
    private MenuBackdrops backdrops;
    /// <summary>Installed world-label positions used when placing the screen's sprites.</summary>
    private WorldMapLabelPositions labels;
    /// <summary>The installed sheets the world map's foreground and each area's background are pictured in (#1467).</summary>
    private InstalledSpriteSheets worldSheets;
    /// <summary>The installed map sprite sheet the title and area labels draw from.</summary>
    private SpriteSheet mapSheet;
    /// <summary>Replaces label positions without changing the selected area or display state.</summary>
    internal void BindLabels(WorldMapLabelPositions content) => labels = Ensure.NotNull(content);

    /// <summary>
    /// Initializes the unmasked world-map picture, backdrop and label owners from the installed screen content and sheets
    /// the file-select map draws, and selects the requested Zebes area.
    /// </summary>
    /// <param name="selectedArea">One of the six Zebes areas.</param>
    /// <param name="presentation">The installed screen content: the menu backdrops, each area's world-map backdrop included, and the world-map labels' positions.</param>
    /// <param name="sheets">The installed sprite sheets: the map sprites the title and labels draw from, and the background pictures.</param>
    /// <exception cref="ArgumentOutOfRangeException">The selected area is not one of the six Zebes areas.</exception>
    public FileSelectAreaMapGraphics(AreaId selectedArea, InstalledScreenContent presentation, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(presentation);
        Ensure.NotNull(sheets);
        backdrops = presentation.Backdrops;
        labels = presentation.Maps.Labels;
        worldSheets = sheets;
        mapSheet = sheets.Get(SpriteSheetId.MapSprites);
        screen = new MenuScreen(backdrops.Menus, sheets);
        SelectArea(selectedArea);
    }

    /// <summary>The selected Zebes area, controlling the background, backdrop, and label highlight; this does not select a gameplay load station.</summary>
    public AreaId SelectedArea { get; private set; }

    /// <summary>Shows the installed area's world-map backdrop before publishing its selection, without advancing menu input, windows, or transition timers.</summary>
    /// <param name="selectedArea">One of the six Zebes areas.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not one of the six Zebes areas.</exception>
    public void SelectArea(AreaId selectedArea)
    {
        if (!AreaIds.IsZebes(selectedArea))
            throw new ArgumentOutOfRangeException(nameof(selectedArea), selectedArea, "Only the six Zebes areas have world-map selections.");
        screen.Backdrop = backdrops.World(selectedArea);
        SelectedArea = selectedArea;
    }

    /// <summary>
    /// Binds every installed resource the area map draws, when the game is bound to its installed content, without
    /// selecting another area.
    /// </summary>
    internal void BindScreenContent(InstalledScreenContent catalog)
    {
        Ensure.NotNull(catalog);
        BindLabels(catalog.Maps.Labels);
        BindBackdrops(catalog.Backdrops);
    }

    /// <summary>Replaces the installed backdrops and shows the current selection's.</summary>
    private void BindBackdrops(MenuBackdrops content)
    {
        backdrops = Ensure.NotNull(content);
        screen.Backdrop = content.World(SelectedArea);
    }

    /// <summary>Rebinds the installed sheets: the map sprites the title and labels draw from, and the background pictures.</summary>
    internal void BindSheets(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        mapSheet = sheets.Get(SpriteSheetId.MapSprites);
        worldSheets = sheets;
        screen.BindScreenPictures(sheets);
    }

    /// <summary>Builds title and valid used-station labels in display order, highlighting the selected area.</summary>
    private void PrepareLabels(PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        ScreenSprites entries = screen.Sprites;
        entries.BeginFrame();
        Draw(MapSpriteId.WorldTitle, FileSelectMapDefinitions.WorldTitlePosition, MapSpriteColouring.Labels);
        foreach (AreaId area in FileSelectMapAreaOrder.Labels)
        {
            if (MapSaveMarkers.HasUsedMarker(area, progress.UsedStations))
                DrawArea(area);
        }
        entries.FinishFrame();

        void DrawArea(AreaId shown)
        {
            Draw(Ensure.Listed(MapSpriteDefinitions.WorldLabels, shown), labels.Get(shown),
                shown == SelectedArea ? MapSpriteColouring.Labels : MapSpriteColouring.DimLabels);
        }

        void Draw(MapSpriteId id, Offset16<ScreenPixels> origin, MapSpriteColouring colouring)
        {
            MapSpriteSheet.Draw(mapSheet, id, entries, origin, MapSpriteScreen.FileSelect, colouring);
        }
    }

    /// <summary>Captures the area map's foreground and its glowing background, then the labels drawn over both, unlit by the glow (#1514).</summary>
    public LayeredScene CaptureRenderSnapshot(PersistentProgress progress, AreaMapBackgroundGlow glow = AreaMapBackgroundGlow.WholeScreen)
    {
        PrepareLabels(progress);
        LayeredScene backgrounds = CaptureBackgrounds(glow);
        return new(backgrounds.Paint, [.. backgrounds.Layers, screen.SpriteLayer()], DisplayBrightness.Full);
    }

    /// <summary>The area map's foreground with its background glowing over it, without the labels (#1514).</summary>
    /// <param name="glow">Where the background glows: over the whole screen, or only over the foreground.</param>
    internal LayeredScene CaptureBackgrounds(AreaMapBackgroundGlow glow)
    {
        // The foreground and the area's background are their pictures in the area's world-map colours (#1467, #1512).
        InstalledSpriteSheets sheets = worldSheets;
        AreaId area = SelectedArea;
        var foreground = new PicturedPlaneLayer(new PicturedPlane(
            [new PlacedPlanePicture(sheets.Get(ScreenPictureSheetDefinitions.AreaMapForegrounds[area]).Frame(AreaMapPictures.WorldForegroundFrame), Offset16<PlanePixels>.Zero, PictureShading.Standard)]),
            Offset16<PlanePixels>.Zero);
        var background = new PicturedPlane(
            [new PlacedPlanePicture(sheets.Get(ScreenPictureSheetDefinitions.AreaMapBackgrounds[area]).Frame(AreaMapPictures.WorldBackgroundFrame), Offset16<PlanePixels>.Zero, PictureShading.Standard)]);
        return new(screen.Paint(), new DisplayLayer[] {
            foreground,
            new AddedPicturedPlaneLayer(background, GlowsOver(glow, foreground)) }, DisplayBrightness.Full);
    }

    /// <summary>The plane <paramref name="glow"/> limits the background's glow to: none over the whole screen, or the foreground.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="glow"/> is undefined.</exception>
    private static PicturedPlaneLayer? GlowsOver(AreaMapBackgroundGlow glow, PicturedPlaneLayer foreground) => glow switch
    {
        AreaMapBackgroundGlow.WholeScreen => null,
        AreaMapBackgroundGlow.ForegroundOnly => foreground,
        _ => throw new ArgumentOutOfRangeException(nameof(glow), glow, "Undefined area map glow."),
    };
}
