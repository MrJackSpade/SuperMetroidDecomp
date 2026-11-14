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
/// by the import (#1512). Labels and transition windows are separate owners; this renders the unmasked map layers.
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
    public static FileSelectAreaMapGraphics FromPresentation(AreaId selectedArea,
        AreaMapPresentation presentation, InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(presentation);
        var graphics = new FileSelectAreaMapGraphics(selectedArea, presentation.Backdrops, sheets);
        graphics.BindLabels(presentation.Labels);
        return graphics;
    }

    /// <summary>Initializes the unmasked world-map picture and backdrop owners from installed artwork and selects the requested Zebes area; labels must be bound separately or through <see cref="FromPresentation"/>.</summary>
    /// <param name="selectedArea">One of the six Zebes areas.</param>
    /// <param name="backdrops">Required menu backdrops, including each area's world-map backdrop.</param>
    /// <param name="sheets">Required installed sprite sheets: the map sprites the title and labels draw from, and the background pictures.</param>
    /// <exception cref="InvalidOperationException">Any required presentation resource is absent despite its optional parameter default.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The selected area is not one of the six Zebes areas.</exception>
    public FileSelectAreaMapGraphics(AreaId selectedArea, MenuBackdrops? backdrops = null,
        InstalledSpriteSheets? sheets = null)
    {
        if (backdrops is null || sheets is null)
            throw new InvalidOperationException("Area-map graphics require installed presentation assets.");
        Backdrops = backdrops;
        worldSheets = sheets;
        mapSheet = sheets.Get(SpriteSheetId.MapSprites);
        screen = new MenuScreen(backdrops.FileSelect, sheets);
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
        screen.Backdrop = Backdrops.World(selectedArea);
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
    }

    /// <summary>Replaces the installed backdrops and shows the current selection's.</summary>
    private void BindBackdrops(MenuBackdrops? content)
    {
        Backdrops = content;
        if (content is not null) screen.Backdrop = content.World(SelectedArea);
    }

    /// <summary>Rebinds the installed sheets: the map sprites the title and labels draw from, and the background pictures.</summary>
    internal void BindSheets(InstalledSpriteSheets? sheets)
    {
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Area-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        worldSheets = installed;
        screen.BindScreenPictures(installed);
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
        foreach (AreaId area in FileSelectMapAreaOrder.Labels)
        {
            if (MapSaveMarkers.HasUsedMarker(area, progress.UsedStations))
                DrawArea(area);
        }
        entries.FinishFrame();

        void DrawArea(AreaId shown)
        {
            Draw(MapSpriteNames.WorldLabel(shown), labels.Get(shown),
                shown == SelectedArea ? MapSpriteColouring.Labels : MapSpriteColouring.DimLabels);
        }

        void Draw(MapSpriteId id, Position16<SpritePixels> origin, MapSpriteColouring colouring)
        {
            MapSpriteSheet.Draw(mapSheet, id, entries, origin, MapSpriteScreen.FileSelect, colouring);
        }
    }

    /// <summary>Captures the area map's foreground and its glowing background, then the labels drawn over both, unlit by the glow (#1514).</summary>
    public LayeredScene CaptureRenderSnapshot(PersistentProgress progress, AreaMapBackgroundGlow glow = AreaMapBackgroundGlow.WholeScreen)
    {
        PrepareLabels(progress);
        LayeredScene backgrounds = CaptureBackgrounds(glow);
        return new(backgrounds.Paint, [.. backgrounds.Layers, screen.Sprites()], DisplayBrightness.Full);
    }

    /// <summary>The area map's foreground with its background glowing over it, without the labels (#1514).</summary>
    /// <param name="glow">Where the background glows: over the whole screen, or only over the foreground.</param>
    internal LayeredScene CaptureBackgrounds(AreaMapBackgroundGlow glow)
    {
        // The foreground and the area's background are their pictures in the area's world-map colours (#1467, #1512).
        InstalledSpriteSheets sheets = worldSheets ?? throw new InvalidOperationException("The world map's pictures are bound when the game is bound to its installed content.");
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
