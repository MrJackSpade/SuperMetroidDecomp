using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>The room effect plane blended over gameplay, with its own scroll for each physical line.</summary>
public sealed record FxPlaneBlendRenderLayer : RenderLayer
{
    /// <summary>Owned physical-scanline scroll pairs, copied so later caller edits cannot change a submitted layer.</summary>
    private readonly Position16<BgPlanePixels>[] scrolls;
    /// <summary>
    /// The owned 224 literal scroll-register pairs indexed by physical output scanline, including HUD
    /// lines rather than starting at the gameplay viewport; each is added to the output coordinate
    /// before tilemap wrapping.
    /// </summary>
    public ReadOnlySpan<Position16<BgPlanePixels>> Scrolls => scrolls;
    /// <summary>The BG3 plane the layer samples (#1348), as its cells' pictures (#1403).</summary>
    public Layer3Cells Plane { get; }
    /// <summary>The plane row the layer's tilemap begins on: 0 for liquids, 32 for an atmosphere's page.</summary>
    public int FirstRow { get; }
    /// <summary>Tilemap height of 32 or 64 eight-pixel rows, determining vertical wrapping; the plane has a fixed width of 32 tiles.</summary>
    public int MapHeightTiles { get; }
    /// <summary>First participating physical output scanline, inclusive; lines before it are untouched, and 224 disables the plane for the entire frame.</summary>
    public int FirstScanline { get; }
    /// <summary>Expanded-byte RGB equation used when this plane is composited as a standalone layer; source-aware gameplay composition instead follows its enclosing color-math controls.</summary>
    public ExpandedColorMathOperation Operation { get; }

    /// <summary>Captures a 2-bpp background color-math plane and copies its physical-line scroll table, validating the map height, first scanline, and operation without retaining caller-owned mutable storage.</summary>
    /// <param name="plane">The BG3 plane the layer samples.</param>
    /// <param name="firstRow">The plane row the layer's tilemap begins on.</param>
    /// <param name="mapHeightTiles">Tilemap height, either 32 or 64 eight-pixel rows.</param>
    /// <param name="firstScanline">Inclusive first physical scanline from 0 through 224; 224 means no visible lines participate.</param>
    /// <param name="operation">Addition or subtraction for the standalone expanded-byte compositor.</param>
    /// <param name="scrolls">Exactly 224 register pairs, indexed by physical output scanline; the constructor takes an owned copy.</param>
    public FxPlaneBlendRenderLayer(Layer3Cells plane, int firstRow, int mapHeightTiles,
        int firstScanline, ExpandedColorMathOperation operation, ReadOnlySpan<Position16<BgPlanePixels>> scrolls)
    {
        if (mapHeightTiles is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(mapHeightTiles));
        if (firstRow < 0 || firstRow + mapHeightTiles > Layer3Plane.Rows) throw new ArgumentOutOfRangeException(nameof(firstRow));
        if (firstScanline is < 0 or > ScreenGeometry.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(firstScanline));
        if (operation is not (ExpandedColorMathOperation.Add or ExpandedColorMathOperation.Subtract))
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
