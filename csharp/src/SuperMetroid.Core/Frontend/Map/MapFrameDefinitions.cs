namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map screens' frame (#1514): the pause map's and the file-select room map's frames share one shape, a border around
/// an opening through which their map scrolls.
/// </summary>
internal static class MapFrameDefinitions
{
    /// <summary>The first cell inside the frame's border, on the frame's plane: the opening's top-left cell.</summary>
    internal static readonly Position16<TilemapCells> OpeningCorner = new(1, 6);

    /// <summary>The opening's width and height in cells; the border surrounds it on every side.</summary>
    internal static readonly Extent16<TilemapCells> OpeningSize = new(30, 18);
}
