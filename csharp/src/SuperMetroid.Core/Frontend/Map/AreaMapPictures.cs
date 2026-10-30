using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Shows an area map as the map screens draw it (#1467): a picture grid of the map's cells, each the installed picture of
/// its shape in the colour variant and tier the screen shows it in, mirrored as the cell mirrors it.
/// </summary>
internal static class AreaMapPictures
{
    /// <summary>The frame name of the picture of <paramref name="shown"/>'s shape, colour variant and tier, before mirroring.</summary>
    internal static string FrameName(ShownMapCell shown) =>
        $"Map.Cell.{shown.Cell.Shape.Picture}.{shown.ColourVariant}.{(shown.Cell.InFront ? "Front" : "Back")}";

    /// <summary>The frame name of <paramref name="area"/>'s room-map frame page.</summary>
    internal static string RoomFrameName(AreaId area) => $"Map.RoomFrame.{area}";

    /// <summary>The frame name of the world map's foreground page in an area's world-map sheet.</summary>
    internal const string WorldForegroundFrame = "World.Foreground";

    /// <summary>The frame name of an area's world-map background page in its world-map sheet.</summary>
    internal const string WorldBackgroundFrame = "World.Background";

    /// <summary>The grid of <paramref name="map"/>'s cells, each pictured from <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet pictures no such cell.</exception>
    internal static PictureGrid Grid(ShownAreaMap map, SpriteSheet sheet)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(sheet);
        var cells = ImmutableArray.CreateBuilder<GridPicture?>(map.Cells.Length);
        foreach (ShownMapCell shown in map.Cells)
            cells.Add(new GridPicture(sheet.Frame(FrameName(shown)), shown.Cell.Mirroring));
        return new PictureGrid(new Extent16<TilemapCells>(AreaMapLayout.WidthInTiles, AreaMapLayout.HeightInTiles), cells.MoveToImmutable());
    }
}
