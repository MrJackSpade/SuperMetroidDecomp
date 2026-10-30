using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The room effect plane blended over gameplay, its scroll on each axis described line by line (#1468): water's wave
/// moves it sideways below the surface, and a liquid's rows above its surface show the plane's top.
/// </summary>
public sealed record OverlayBlendLayer : DisplayLayer
{
    /// <summary>The layer's horizontal scroll line by line, added to the screen position before the map wraps.</summary>
    public LineScroll Horizontal { get; }
    /// <summary>The layer's vertical scroll line by line, added to the screen position before the map wraps.</summary>
    public LineScroll Vertical { get; }
    /// <summary>The overlay plane the layer shows (#1348), as its cells' pictures (#1403).</summary>
    public Layer3Cells Plane { get; }
    /// <summary>The plane row the layer's tilemap begins on: 0 for liquids, 32 for an atmosphere's page.</summary>
    public int FirstRow { get; }
    /// <summary>The map's height, 32 or 64 eight-pixel rows, where it wraps vertically; the plane is always 32 tiles wide.</summary>
    public int MapHeightTiles { get; }
    /// <summary>First participating screen line, inclusive; lines before it are untouched, and 224 disables the plane for the entire frame.</summary>
    public int FirstLine { get; }
    /// <summary>How the layer combines with the screen when drawn on its own; within a gameplay blend it follows that blend instead.</summary>
    public BlendOperation Operation { get; }

    /// <summary>Captures a four-colour overlay to blend with its line scrolls, checking the map height, first line and operation.</summary>
    /// <param name="plane">The overlay plane the layer shows.</param>
    /// <param name="firstRow">The plane row the layer's tilemap begins on.</param>
    /// <param name="mapHeightTiles">The map's height, either 32 or 64 eight-pixel rows.</param>
    /// <param name="firstScanline">Inclusive first screen line from 0 through 224; 224 means no visible lines participate.</param>
    /// <param name="operation">Addition or subtraction when the layer is drawn on its own.</param>
    /// <param name="horizontal">The layer's horizontal scroll line by line.</param>
    /// <param name="vertical">The layer's vertical scroll line by line.</param>
    public OverlayBlendLayer(Layer3Cells plane, int firstRow, int mapHeightTiles,
        int firstScanline, BlendOperation operation, LineScroll horizontal, LineScroll vertical)
    {
        if (mapHeightTiles is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(mapHeightTiles));
        if (firstRow < 0 || firstRow + mapHeightTiles > Layer3Plane.Rows) throw new ArgumentOutOfRangeException(nameof(firstRow));
        if (firstScanline is < 0 or > ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(firstScanline));
        if (operation is not (BlendOperation.Add or BlendOperation.Subtract))
            throw new ArgumentOutOfRangeException(nameof(operation));
        Plane = Ensure.NotNull(plane);
        FirstRow = firstRow;
        MapHeightTiles = mapHeightTiles;
        FirstLine = firstScanline;
        Operation = operation;
        Horizontal = Ensure.NotNull(horizontal);
        Vertical = Ensure.NotNull(vertical);
    }
}
