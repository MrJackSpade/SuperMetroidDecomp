using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map screens' picture sheets (#1467): each map cell any area map shows (#1513) in each shade, each area's room-map
/// frame, and each area's world-map foreground and background, pre-coloured under the palette of the screen that shows
/// them. The game places them by frame name; no map screen keeps a tilemap.
/// </summary>
internal static class MapPictureSheets
{
    /// <summary>
    /// The picture of each cell any area map can show, each once in each shade: the empty cell the map screens show where
    /// nothing is revealed, then every area's cells, each pictured from the map character that pictures it (#1513).
    /// </summary>
    internal static IEnumerable<ScreenPictureSubject> CellSubjects(InstalledAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        var cells = new HashSet<MapCell> { MapCell.Empty };
        var ordered = new List<MapCell> { MapCell.Empty };
        for (int index = 0; index < AreaIds.RetailCount; index++)
        {
            IAreaMapView map = maps.Game.Get(AreaIds.At(index));
            for (int y = 0; y < AreaMapSize.HeightInTiles; y++)
            for (int x = 0; x < AreaMapSize.WidthInTiles; x++)
            {
                MapCell cell = map.Cell(new Position16<MapTiles>((ushort)x, (ushort)y));
                if (cells.Add(cell))
                    ordered.Add(cell);
            }
        }
        Extent16<CharacterCells> one = new(1, 1);
        foreach (MapCell cell in ordered)
        foreach (MapCellShade shade in Enum.GetValues<MapCellShade>())
        {
            var shown = new ShownMapCell(cell, shade);
            yield return ScreenPictureSubject.Block(AreaMapPictures.FrameName(shown), default, one, new[] { MapCellCharacters.Tile(shown) });
        }
    }

    /// <summary>
    /// Requires the map characters <paramref name="maps"/> pictures its cells from to picture each cell once, and each
    /// cell a retail map stores explored to show alike in either shade under <paramref name="palette"/> (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">A cell's sources paint different pixels, or a stored-explored cell shows differently by shade.</exception>
    internal static void RequireCellsPicturedOnce(ImportAreaMapPresentation maps, ReadOnlySpan<RgbColor> palette)
    {
        ScreenCharacters characters = MapCharacters(maps);
        MapCellCharacters.RequireOnePicturePerCell(characters);
        MapCellCharacters.RequireStoredExploredCellsShowAlike(characters, palette);
    }

    /// <summary>The map characters, from tile zero, as the room maps and the pause map name them.</summary>
    internal static ScreenCharacters MapCharacters(ImportAreaMapPresentation maps)
    {
        int bytes = MapScreenDefinitions.MapTileCount * RoomTileCanvas.CharacterBytes;
        return new ScreenCharacters(TileInkDepth.SixteenColours).Place(0, Ensure.NotNull(maps).Characters.Map.Characters[..bytes].ToArray());
    }

    /// <summary>
    /// The room maps' sheet under the file-select palette: every map cell and each Zebes area's room frame, a picture
    /// painting no pixel, such as the empty map cell, held without a picture as showing nothing (#1513).
    /// </summary>
    internal static ComposedSpriteSheet ComposeRoomMap(ImportAreaMapPresentation maps)
    {
        ReadOnlySpan<RgbColor> palette = maps.Palettes.FileSelect;
        RequireCellsPicturedOnce(maps, palette);
        return ScreenPictureSheetComposer.ComposeHoldingBlanks([.. RoomMapSubjects(maps)], MapCharacters(maps), palette);
    }

    /// <summary>The room maps' pictures: every map cell, then each Zebes area's room frame.</summary>
    internal static IEnumerable<ScreenPictureSubject> RoomMapSubjects(InstalledAreaMapPresentation maps)
    {
        MapScreenPresentation screens = maps.Screens;
        foreach (ScreenPictureSubject cell in CellSubjects(maps))
            yield return cell;
        foreach (AreaId area in AreaIds.Zebes)
            yield return ScreenPictureSubject.Page(AreaMapPictures.RoomFrameName(area), screens.Page(MapScreenDefinitions.RoomFrame(area)), MapScreenDefinitions.PageColumns);
    }

    /// <summary>
    /// <paramref name="area"/>'s world-map foreground sheet under its world-map palette: the world map's foreground page.
    /// </summary>
    internal static ComposedSpriteSheet ComposeWorldForeground(ImportAreaMapPresentation maps, AreaId area) =>
        ScreenPictureSheetComposer.Compose(
            [ScreenPictureSubject.Page(AreaMapPictures.WorldForegroundFrame, maps.Screens.Page(MapScreenDefinitions.WorldForeground),
                MapScreenDefinitions.PageColumns)],
            new ScreenCharacters(TileInkDepth.SixteenColours).Place(0, maps.WorldCharacters.ForegroundCharacters()),
            maps.Palettes.World(area));

    /// <summary><paramref name="area"/>'s world-map background sheet under its world-map palette: its background page.</summary>
    internal static ComposedSpriteSheet ComposeWorldBackground(ImportAreaMapPresentation maps, AreaId area) =>
        ScreenPictureSheetComposer.Compose(
            [ScreenPictureSubject.Page(AreaMapPictures.WorldBackgroundFrame, maps.Screens.Page(MapScreenDefinitions.WorldBackground(area)),
                MapScreenDefinitions.PageColumns)],
            new ScreenCharacters(TileInkDepth.FourColours).Place(0, maps.WorldCharacters.BackgroundCharacters()),
            maps.Palettes.World(area));

    /// <summary>The frames a room-map sheet must hold.</summary>
    internal static IEnumerable<string> RoomMapFrameNames(InstalledAreaMapPresentation maps) => RoomMapSubjects(maps).Select(subject => subject.Name);
}
