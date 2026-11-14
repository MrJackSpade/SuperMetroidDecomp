using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Shows an area map as the map screens draw it (#1467): a picture grid of the map's cells, each the installed picture of
/// the cell (#1513) in the shade the screen shows it in.
/// </summary>
internal static class AreaMapPictures
{
    /// <summary>The frame name of the picture of <paramref name="shown"/>'s cell in its shade.</summary>
    internal static string FrameName(ShownMapCell shown) => $"Map.Cell.{shown.Cell.Name}.{shown.Shade}";

    /// <summary>The frame name of <paramref name="area"/>'s room-map frame page.</summary>
    internal static string RoomFrameName(AreaId area) => $"Map.RoomFrame.{area}";

    /// <summary>The frame name of the world map's foreground page in an area's world-map sheet.</summary>
    internal const string WorldForegroundFrame = "World.Foreground";

    /// <summary>The frame name of an area's world-map background page in its world-map sheet.</summary>
    internal const string WorldBackgroundFrame = "World.Background";

    /// <summary>
    /// The grid of <paramref name="map"/>'s cells, each pictured from <paramref name="sheet"/> in the standard colours the
    /// import pre-coloured it in under its screen's colours (#1512); a cell whose picture the sheet holds as showing
    /// nothing draws nothing (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no such cell.</exception>
    internal static PictureGrid Grid(ShownAreaMap map, SpriteSheet sheet)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(sheet);
        var cells = ImmutableArray.CreateBuilder<GridPicture?>(map.Cells.Length);
        foreach (ShownMapCell shown in map.Cells)
        {
            string frame = FrameName(shown);
            cells.Add(sheet.ShowsNothing(frame) ? null : new GridPicture(sheet.Frame(frame), PictureFlip.None, PictureShading.Standard));
        }
        return new PictureGrid(new Extent16<MapTiles>(AreaMapSize.WidthInMapTiles, AreaMapSize.HeightInMapTiles), cells.MoveToImmutable());
    }
}
