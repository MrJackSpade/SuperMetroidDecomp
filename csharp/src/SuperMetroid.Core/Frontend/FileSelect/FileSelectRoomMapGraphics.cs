using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Room-select map and its frame.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed class FileSelectRoomMapGraphics
{
    /// <summary>The room map's menu screen: its artwork, pictures, colours and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Map landmarks, stations, and boss icons drawn around the saved-station marker.</summary>
    internal readonly FileSelectMapIcons icons;

    /// <summary>The installed map sprite sheet the icons, marker and arrows draw from.</summary>
    private SpriteSheet mapSheet;
    /// <summary>The installed room-map pictures the map's cells and the frame draw from (#1467).</summary>
    private SpriteSheet roomPictures;
    /// <summary>How the room map shows the area's cells (#1466, #1467).</summary>
    internal ShownAreaMap AreaMap { get; private set; }
    /// <summary>Persistent downloaded-map and explored-cell state held by the icon owner.</summary>
    internal SharedGameState MapSystem => icons.MapSystem;

    /// <summary>Installs one area's revealed map and fixed frame into a separate menu display owner, without choosing a load station or advancing menu timing.</summary>
    /// <param name="system">Persistence owner supplying downloaded-map, explored-cell, boss, and station visibility state; it is not replaced or cleared.</param>
    /// <param name="area">One of the six Zebes map areas, excluding Ceres.</param>
    /// <param name="screenContent">The installed screen content, whose area maps supply the stations and landmarks.</param>
    /// <param name="sheets">The installed sprite sheets: the map sprites and the background pictures.</param>
    public FileSelectRoomMapGraphics(SharedGameState system, AreaId area, InstalledScreenContent screenContent,
        InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(system);
        Ensure.NotNull(screenContent);
        InstalledSpriteSheets installed = Ensure.NotNull(sheets);
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        roomPictures = installed.Get(SpriteSheetId.RoomMapPictures);
        icons = new FileSelectMapIcons(system, area, screenContent.Maps.Stations, screenContent.Maps.Landmarks, mapSheet);
        if (!AreaIds.IsZebes(area))
            throw new ArgumentOutOfRangeException(nameof(area), area, "Ceres has no room map on the saved-game map.");
        screen = new MenuScreen(screenContent.Backdrops.Menus, installed);
        AreaMap = AreaMapProjection.Project(screenContent.Get(area), system);
    }

    /// <summary>Rebinds the installed sheets: the map sprites the icons, marker and arrows draw from, and the background pictures.</summary>
    internal void BindSheets(InstalledSpriteSheets sheets)
    {
        InstalledSpriteSheets installed = Ensure.NotNull(sheets);
        mapSheet = installed.Get(SpriteSheetId.MapSprites);
        roomPictures = installed.Get(SpriteSheetId.RoomMapPictures);
        icons.BindSprites(mapSheet);
        screen.BindScreenPictures(installed);
    }

    /// <summary>Reprojects current host artwork without resetting menu animation or scroll state.</summary>
    internal void BindScreenContent(InstalledScreenContent catalog)
    {
        Ensure.NotNull(catalog);
        icons.BindStations(catalog.Maps.Stations);
        icons.BindLandmarks(catalog.Maps.Landmarks);
        screen.Backdrop = catalog.Backdrops.Menus;
        var system = icons.MapSystem;
        var area = icons.MapArea;
        AreaMap = AreaMapProjection.Project(catalog.Get(area), system);
    }

    /// <summary>Composes landmarks, the saved-station marker, and arrows in drawing order without advancing animations.</summary>
    private void PrepareIcons(Offset16<MapPixels> scroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        ScreenSprites entries = screen.Sprites;
        entries.BeginFrame();
        SpriteSheet sheet = mapSheet;
        icons.DrawBeforeMarker(entries, scroll);
        marker.Draw(entries, scroll, sheet);
        icons.DrawAfterMarker(entries, scroll,
            animations is null ? null : () => animations.DrawArrows(entries, animations.HighlightShading, sheet));
        entries.FinishFrame();
    }

    /// <summary>Captures the installed room map/frame and optional marker/arrows without advancing any animation.</summary>
    public LayeredScene CaptureRenderSnapshot(Offset16<MapPixels> scroll = default,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, RoomMapContent content = RoomMapContent.Scene)
    {
        var layers = new List<DisplayLayer>();
        // The frame is the area's installed room-frame picture and the map a grid of its cells' pictures (#1467).
        SpriteSheet pictures = roomPictures;
        // A room frame the sheet holds as showing nothing is not placed (#1513).
        string roomFrame = AreaMapPictures.RoomFrameName(icons.MapArea);
        PlacedPlanePicture[] framePictures = pictures.ShowsNothing(roomFrame) ? []
            : [new PlacedPlanePicture(pictures.Frame(roomFrame), Offset16<PlanePixels>.Zero, PictureShading.Standard)];
        var frame = new PicturedPlane([.. framePictures]);
        var frameScroll = new Offset16<PlanePixels>(0, FileSelectMapDefinitions.RoomFrameVerticalScroll);
        // The room map scrolls through its frame's opening, the frame's border around it (#1514): each picture is drawn
        // whole, the map in front of the frame.
        layers.Add(new PicturedPlaneLayer(frame, frameScroll));
        if (content != RoomMapContent.FrameOnly)
            layers.Add(new PictureGridLayer(AreaMapPictures.Grid(AreaMap, pictures), scroll.AsPlaneOffset()) { Window = MapFrameOpening.Window(frameScroll) });
        if (marker is not null && content != RoomMapContent.FrameOnly)
        {
            PrepareIcons(scroll, marker, animations);
            layers.Add(screen.SpriteLayer());
        }
        else
        {
            screen.Sprites.BeginFrame();
            screen.Sprites.FinishFrame();
        }
        return new(screen.Paint(), layers.ToArray(),
            DisplayBrightness.Full);
    }
}
