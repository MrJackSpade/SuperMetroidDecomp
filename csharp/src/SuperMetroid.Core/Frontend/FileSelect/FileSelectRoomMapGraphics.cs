using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Room-select BG1 map and BG2 frame installed by $81:A725 and $82:9517.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed class FileSelectRoomMapGraphics
{
    /// <summary>The room map's menu screen: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Map landmarks, stations, and boss icons drawn around the saved-station marker.</summary>
    internal readonly FileSelectMapIcons icons;

    /// <summary>The installed map sprite sheet the icons, marker and arrows draw from.</summary>
    [NonSerialized] private SpriteSheet? mapSheet;
    /// <summary>The installed room-map pictures the map's cells and the frame draw from (#1467).</summary>
    [NonSerialized] private SpriteSheet? roomPictures;
    /// <summary>How the room map shows the area's cells (#1466, #1467).</summary>
    internal ShownAreaMap areaMap;
    /// <summary>The room map's palette, initialized from file-select presentation colors and subsequently updated by the menu's palette-animation owner.</summary>
    public ScreenPalette Palette => screen.Palette;
    /// <summary>Persistent downloaded-map and explored-cell state held by the icon owner.</summary>
    internal SharedGameState MapSystem => icons.MapSystem;

    /// <summary>Installs one area's revealed BG1 map and fixed BG2 frame into a separate menu PPU owner, following $81:A725/$82:9517 without choosing a load station or advancing menu timing.</summary>
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
        int index = AreaIds.ToIndex(area);
        if (index >= FileSelectMapDefinitions.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        screen = new MenuScreen(mapPresentation.Palettes,
            mapPresentation.WorldArtwork, installed);
        areaMap = AreaMapProjection.Project(mapPresentation.Get(area), system,
            mapPresentation.HiddenCells.RoomSelect(system.HasAreaMap(area)), revealMode);
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

        if (catalog is not null)
            catalog.Palettes.FileSelect.LoadToExcept(screen.Palette, screen.Palette.Colours(ScreenColourGroup.MapHighlight));
        _ = catalog ?? throw new InvalidOperationException("Room-map graphics require installed map presentation assets.");
        var system = icons.MapSystem;
        var area = icons.MapArea;
        screen.BindWorldArtwork(catalog.WorldArtwork);
        areaMap = AreaMapProjection.Project(catalog.Get(area), system, catalog.HiddenCells.RoomSelect(system.HasAreaMap(area)));
    }

    /// <summary>Composes landmarks, the saved-station marker, and arrows in native order without advancing animations.</summary>
    private void PrepareIcons(Position16<MapPixels> scroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        ScreenSprites entries = screen.Objects;
        entries.BeginFrame();
        icons.DrawBeforeMarker(entries, scroll);
        marker.Draw(entries, scroll, mapSheet);
        icons.DrawAfterMarker(entries, scroll,
            animations is null ? null : () => animations.DrawArrows(entries, mapSheet));
        entries.FinishFrame();
    }

    /// <summary>Captures the installed room map/frame and optional marker/arrows without advancing any animation.</summary>
    public LayeredScene CaptureRenderSnapshot(Position16<MapPixels> scroll = default,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, RoomMapContent content = RoomMapContent.Scene)
    {
        var layers = new List<DisplayLayer>();
        // The frame is the area's installed room-frame picture and the map a grid of its cells' pictures (#1467).
        SpriteSheet pictures = roomPictures ?? throw new InvalidOperationException("The room map's pictures are rebound after debugger restoration.");
        var frame = new PicturedPlane(MenuScreen.PageSize, [new PlacedPlanePicture(pictures.Frame(AreaMapPictures.RoomFrameName(icons.MapArea)), default)]);
        var frameScroll = new Position16<TilePlanePixels>(0, FileSelectMapDefinitions.RoomFrameVerticalScroll);
        if (content == RoomMapContent.FrameOnly)
            layers.Add(new PicturedPlaneLayer(frame, frameScroll, null));
        else
        {
            PictureGrid map = AreaMapPictures.Grid(areaMap, pictures);
            foreach (bool priority in new[] { false, true })
            {
                layers.Add(new PicturedPlaneLayer(frame, frameScroll, priority));
                layers.Add(new PictureGridLayer(map, scroll.AsBgPlaneScroll(), priority));
            }
        }
        if (marker is not null && content != RoomMapContent.FrameOnly)
        {
            PrepareIcons(scroll, marker, animations);
            layers.Add(new SpritesLayer());
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
