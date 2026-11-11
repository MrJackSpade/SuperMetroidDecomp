namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map screens' frame (#1514): the pause map's and the file-select room map's frames share one shape, a border around
/// an opening through which their map scrolls.
/// </summary>
internal static class MapFrameDefinitions
{
    /// <summary>The frame plane's pixel at the opening's upper-left corner, just inside the frame's border.</summary>
    internal static readonly Position16<PlanePixels> OpeningCorner = new(8, 48);

    /// <summary>The opening's width and height in pixels; the border surrounds it on every side.</summary>
    internal static readonly Extent16<PlanePixels> OpeningSize = new(240, 144);
}
