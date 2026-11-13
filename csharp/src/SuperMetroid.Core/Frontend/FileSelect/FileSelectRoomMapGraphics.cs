using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Room-select map and its frame.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed class FileSelectRoomMapGraphics
{
    /// <summary>The room map's menu screen: its artwork, pictures, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Map landmarks, stations, and boss icons drawn around the saved-station marker.</summary>
    internal readonly FileSelectMapIcons icons;

    /// <summary>The installed map sprite sheet the icons, marker and arrows draw from.</summary>
    private SpriteSheet? mapSheet;
    /// <summary>The installed room-map pictures the map's cells and the frame draw from (#1467).</summary>
    private SpriteSheet? roomPictures;
    /// <summary>How the room map shows the area's cells (#1466, #1467).</summary>
    internal ShownAreaMap AreaMap { get; private set; }
    /// <summary>Persistent downloaded-map and explored-cell state held by the icon owner.</summary>
    internal SharedGameState MapSystem => icons.MapSystem;

    /// <summary>Installs one area's revealed map and fixed frame into a separate menu display owner, without choosing a load station or advancing menu timing.</summary>
    /// <param name="system">Persistence owner supplying downloaded-map, explored-cell, boss, and station visibility state; it is not replaced or cleared.</param>
    /// <param name="area">One of the six Zebes map areas, excluding Ceres.</param>
    /// <param name="revealMode">Optional presentation reveal override when projecting installed map cells; ordinary selection uses persistence-driven visibility.</param>
    /// <param name="mapPresentation">Required installed map tiles, palettes, screens, and sprite/layout resources, despite the optional parameter syntax.</param>
    /// <param name="sheets">Required installed sprite sheets, despite the optional parameter syntax: the map sprites and the background tiles.</param>
    public FileSelectRoomMapGraphics(SharedGameState system, AreaId area,
        MapRevealMode revealMode = MapRevealMode.None, AreaMapPresentation? mapPresentation = null, InstalledSpriteSheets? sheets = null)
    {
        Ensure.NotNull(system);
        if (mapPresentation is null)
            throw new InvalidOperationException(
                "Room-map graphics require installed map presentation assets.");
        icons = new FileSelectMapIcons(system, area);
        icons.BindStations(mapPresentation.Stations);
        icons.BindLandmarks(mapPresentation.Landmarks);
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Room-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        roomPictures = installed.Get(SpriteSheetId.RoomMapPictures);
        icons.BindSprites(mapSheet);
        if (!AreaIds.IsZebes(area))
            throw new ArgumentOutOfRangeException(nameof(area), area, "Ceres has no room map on the saved-game map.");
        screen = new MenuScreen(mapPresentation.Backdrops.FileSelect, installed);
        AreaMap = AreaMapProjection.Project(mapPresentation.Get(area), system, revealMode);
    }

    /// <summary>Rebinds the installed sheets: the map sprites the icons, marker and arrows draw from, and the background tiles.</summary>
    internal void BindSheets(InstalledSpriteSheets? sheets)
    {
        InstalledSpriteSheets installed = sheets ?? throw new InvalidOperationException("Room-map graphics require the installed sprite sheets.");
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        roomPictures = installed.Get(SpriteSheetId.RoomMapPictures);
        icons.BindSprites(mapSheet);
        screen.BindScreenTiles(installed);
    }

    /// <summary>Reprojects current host artwork without resetting menu animation or scroll state.</summary>
    internal void BindMapPresentation(AreaMapPresentation? catalog)
    {
        icons.BindStations(catalog?.Stations);
        icons.BindLandmarks(catalog?.Landmarks);

        _ = catalog ?? throw new InvalidOperationException("Room-map graphics require installed map presentation assets.");
        screen.Backdrop = catalog.Backdrops.FileSelect;
        var system = icons.MapSystem;
        var area = icons.MapArea;
        AreaMap = AreaMapProjection.Project(catalog.Get(area), system);
    }

    /// <summary>Composes landmarks, the saved-station marker, and arrows in drawing order without advancing animations.</summary>
    private void PrepareIcons(Position16<MapPixels> scroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        ScreenSprites entries = screen.Objects;
        entries.BeginFrame();
        icons.DrawBeforeMarker(entries, scroll);
        marker.Draw(entries, scroll, mapSheet);
        icons.DrawAfterMarker(entries, scroll,
            animations is null ? null : () => animations.DrawArrows(entries, animations.HighlightShading, mapSheet));
        entries.FinishFrame();
    }

    /// <summary>Captures the installed room map/frame and optional marker/arrows without advancing any animation.</summary>
    public LayeredScene CaptureRenderSnapshot(Position16<MapPixels> scroll = default,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, RoomMapContent content = RoomMapContent.Scene)
    {
        var layers = new List<DisplayLayer>();
        // The frame is the area's installed room-frame picture and the map a grid of its cells' pictures (#1467).
        SpriteSheet pictures = roomPictures ?? throw new InvalidOperationException("The room map's pictures are bound when the game is bound to its installed content.");
        // A room frame the sheet holds as showing nothing is not placed (#1513).
        string roomFrame = AreaMapPictures.RoomFrameName(icons.MapArea);
        PlacedPlanePicture[] framePictures = pictures.ShowsNothing(roomFrame) ? []
            : [new PlacedPlanePicture(pictures.Frame(roomFrame), Offset16<PlanePixels>.Zero, PictureShading.Standard)];
        var frame = new PicturedPlane([.. framePictures]);
        var frameScroll = new Offset16<PlanePixels>(0, FileSelectMapDefinitions.RoomFrameVerticalScroll);
        // The room map scrolls through its frame's opening, the frame's border around it (#1514): each picture is drawn
        // whole, and the per-tile priority tiers that once interleaved the two are a presentation quirk the model does not keep.
        layers.Add(new PicturedPlaneLayer(frame, frameScroll));
        if (content != RoomMapContent.FrameOnly)
            layers.Add(new PictureGridLayer(AreaMapPictures.Grid(AreaMap, pictures), scroll.AsPlaneOffset()) { Window = MapFrameOpening.Window(frameScroll) });
        if (marker is not null && content != RoomMapContent.FrameOnly)
        {
            PrepareIcons(scroll, marker, animations);
            layers.Add(screen.Sprites());
        }
        else
        {
            screen.Objects.BeginFrame();
            screen.Objects.FinishFrame();
        }
        return new(screen.Paint(), layers.ToArray(),
            DisplayBrightness.Full);
    }
}
