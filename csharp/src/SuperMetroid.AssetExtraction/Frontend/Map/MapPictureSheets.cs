using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map screens' picture sheets (#1467): each map cell's shape in each colour variant and tier any area map shows it
/// in, each area's room-map frame, and each area's world-map foreground and background, pre-coloured under the palette of
/// the screen that shows them. The game places them by frame name; no map screen keeps a tilemap.
/// </summary>
internal static class MapPictureSheets
{
    /// <summary>
    /// The picture of each cell any area map can show, each once: every area's cells explored and unexplored, and the
    /// hidden cells the map screens show in their place.
    /// </summary>
    internal static IEnumerable<ScreenPictureSubject> CellSubjects(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        var shown = new List<ShownMapCell>
        {
            ShownMapCell.Unexplored(maps.HiddenCells.Unrevealed),
            ShownMapCell.Unexplored(maps.HiddenCells.WithoutAreaMap),
        };
        for (int index = 0; index < AreaIds.RetailCount; index++)
        {
            IAreaMapView map = maps.Get(AreaIds.At(index));
            for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
            for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
            {
                MapCell cell = map.Cell(new Position16<MapTiles>((ushort)x, (ushort)y));
                shown.Add(ShownMapCell.Explored(cell));
                shown.Add(ShownMapCell.Unexplored(cell));
            }
        }
        var named = new HashSet<string>(StringComparer.Ordinal);
        Extent16<TilemapCells> one = new(1, 1);
        foreach (ShownMapCell cell in shown)
        {
            string name = AreaMapPictures.FrameName(cell);
            if (named.Add(name))
                yield return ScreenPictureSubject.Block(name, default, one,
                    new[] { new BackgroundTile(cell.Cell.Shape.Picture, cell.ColourVariant, cell.Cell.InFront, TileFlip.None) });
        }
    }

    /// <summary>The map characters, from tile zero, as the room maps and the pause map name them.</summary>
    internal static ScreenCharacters MapCharacters(AreaMapPresentation maps)
    {
        int bytes = MapScreenDefinitions.MapTileCount * RoomTileCanvas.CharacterBytes;
        return new ScreenCharacters(TileInkDepth.SixteenColours).Place(0, MapCharacterArtwork.Of(Ensure.NotNull(maps)).Map.Characters[..bytes].ToArray());
    }

    /// <summary>The room maps' sheet under the file-select palette: every map cell and each Zebes area's room frame.</summary>
    internal static ComposedSpriteSheet ComposeRoomMap(AreaMapPresentation maps) =>
        ScreenPictureSheetComposer.Compose([.. RoomMapSubjects(maps)], MapCharacters(maps), ScreenColourImage.Encode(maps.Palettes.FileSelect));

    /// <summary>The room maps' pictures: every map cell, then each Zebes area's room frame.</summary>
    internal static IEnumerable<ScreenPictureSubject> RoomMapSubjects(AreaMapPresentation maps)
    {
        MapScreenPresentation screens = InstalledAreaMapPresentation.ScreensOf(maps);
        foreach (ScreenPictureSubject cell in CellSubjects(maps))
            yield return cell;
        foreach (AreaId area in AreaIds.Zebes)
            yield return ScreenPictureSubject.Page(AreaMapPictures.RoomFrameName(area), screens.Page(MapScreenDefinitions.RoomFrame(area)), MapScreenDefinitions.PageColumns);
    }

    /// <summary>
    /// <paramref name="area"/>'s world-map foreground sheet under its world-map palette: the world map's foreground page.
    /// </summary>
    internal static ComposedSpriteSheet ComposeWorldForeground(AreaMapPresentation maps, AreaId area) =>
        ScreenPictureSheetComposer.Compose(
            [ScreenPictureSubject.Page(AreaMapPictures.WorldForegroundFrame, InstalledAreaMapPresentation.ScreensOf(maps).Page(MapScreenDefinitions.WorldForeground),
                MapScreenDefinitions.PageColumns)],
            new ScreenCharacters(TileInkDepth.SixteenColours).Place(0, WorldMapCharacters.Of(maps.WorldArtwork).ForegroundCharacters()),
            ScreenColourImage.Encode(maps.Palettes.World(area)));

    /// <summary><paramref name="area"/>'s world-map background sheet under its world-map palette: its background page.</summary>
    internal static ComposedSpriteSheet ComposeWorldBackground(AreaMapPresentation maps, AreaId area) =>
        ScreenPictureSheetComposer.Compose(
            [ScreenPictureSubject.Page(AreaMapPictures.WorldBackgroundFrame, InstalledAreaMapPresentation.ScreensOf(maps).Page(MapScreenDefinitions.WorldBackground(area)),
                MapScreenDefinitions.PageColumns)],
            new ScreenCharacters(TileInkDepth.FourColours).Place(0, WorldMapCharacters.Of(maps.WorldArtwork).BackgroundCharacters()),
            ScreenColourImage.Encode(maps.Palettes.World(area)));

    /// <summary>The frames a room-map sheet must hold.</summary>
    internal static IEnumerable<string> RoomMapFrameNames(AreaMapPresentation maps) => RoomMapSubjects(maps).Select(subject => subject.Name);
}
