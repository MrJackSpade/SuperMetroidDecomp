using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>The room effect plane blended over gameplay, with its own scroll for each screen line.</summary>
public sealed record OverlayBlendLayer : DisplayLayer
{
    /// <summary>The layer's own per-line scrolls, copied so later edits to the caller's cannot change a submitted layer.</summary>
    private readonly Position16<TilePlanePixels>[] scrolls;
    /// <summary>
    /// The layer's own 224 scrolls, one per screen line from the top, including the HUD's
    /// lines rather than starting below them; each is added to the screen position
    /// before the map wraps.
    /// </summary>
    public ReadOnlySpan<Position16<TilePlanePixels>> Scrolls => scrolls;
    /// <summary>The overlay plane the layer shows (#1348), as its cells' pictures (#1403).</summary>
    public Layer3Cells Plane { get; }
    /// <summary>The plane row the layer's tilemap begins on: 0 for liquids, 32 for an atmosphere's page.</summary>
    public int FirstRow { get; }
    /// <summary>The map's height, 32 or 64 eight-pixel rows, where it wraps vertically; the plane is always 32 tiles wide.</summary>
    public int MapHeightTiles { get; }
    /// <summary>First participating screen line, inclusive; lines before it are untouched, and 224 disables the plane for the entire frame.</summary>
    public int FirstScanline { get; }
    /// <summary>How the layer combines with the screen when drawn on its own; within a gameplay blend it follows that blend instead.</summary>
    public BlendOperation Operation { get; }

    /// <summary>Captures a four-colour overlay to blend and copies its per-line scrolls, checking the map height, first line and operation.</summary>
    /// <param name="plane">The overlay plane the layer shows.</param>
    /// <param name="firstRow">The plane row the layer's tilemap begins on.</param>
    /// <param name="mapHeightTiles">The map's height, either 32 or 64 eight-pixel rows.</param>
    /// <param name="firstScanline">Inclusive first screen line from 0 through 224; 224 means no visible lines participate.</param>
    /// <param name="operation">Addition or subtraction when the layer is drawn on its own.</param>
    /// <param name="scrolls">Exactly 224 scrolls, one per screen line; the layer keeps its own copy.</param>
    public OverlayBlendLayer(Layer3Cells plane, int firstRow, int mapHeightTiles,
        int firstScanline, BlendOperation operation, ReadOnlySpan<Position16<TilePlanePixels>> scrolls)
    {
        if (mapHeightTiles is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(mapHeightTiles));
        if (firstRow < 0 || firstRow + mapHeightTiles > Layer3Plane.Rows) throw new ArgumentOutOfRangeException(nameof(firstRow));
        if (firstScanline is < 0 or > ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(firstScanline));
        if (operation is not (BlendOperation.Add or BlendOperation.Subtract))
            throw new ArgumentOutOfRangeException(nameof(operation));
        Ensure.LengthEqual(scrolls, ScreenGeometry.ScreenHeightPixels);
        Plane = Ensure.NotNull(plane);
        FirstRow = firstRow;
        MapHeightTiles = mapHeightTiles;
        FirstScanline = firstScanline;
        Operation = operation;
        this.scrolls = scrolls.ToArray();
    }
}
