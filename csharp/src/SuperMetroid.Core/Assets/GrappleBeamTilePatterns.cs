namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed Grapple stroke artwork and calculated spark/transpose rules; independent supplied pixels are preserved. Collision, timing and angle-sector policies are excluded.</summary>
internal static class GrappleBeamTilePatterns
{
    /// <summary>Native SNES4bpp character side length; endpoint art occupies one8x8 tile.</summary>
    private const int TileSide = 8, HalfSide = TileSide / 2;
    /// <summary>$9A:8200/8400/8600/8800: chosen lattice center is half-side X, one pixel above half-side Y.</summary>
    private const int CenterX = HalfSide, CenterY = HalfSide - 1;
    /// <summary>$9A:8200/8600: filled diamond and square outline share half the half-side as their small shape scale.</summary>
    private const int SmallRadius = HalfSide / 2;
    /// <summary>$9A:8400/8600: hollow diamond and square's cardinal tips share the one-pixel-inset half-side.</summary>
    private const int MiddleRadius = HalfSide - 1;
    /// <summary>$9A:8800: outer diamond reaches the full half-side distance.</summary>
    private const int OuterRadius = HalfSide;
    /// <summary>$9A:8800: symmetric clipping reaches the nearest8x8 character edge.</summary>
    private static int ClipRadius => Math.Min(Math.Min(CenterX, TileSide - 1 - CenterX),
        Math.Min(CenterY, TileSide - 1 - CenterY));
    /// <summary>$9A:8800: selected one-pixel Manhattan center spark, independent of collision.</summary>
    private const int InnerRadius = 1;
    /// <summary>$9A:8200-929F: reviewed electric-stroke coverage uses palette ink15; its color changes remain palette-owned.</summary>
    private const byte Pen = 15;
    /// <summary>$9A:8200/$9B:BFBD first endpoint frame selects the filled diamond; reviewed visual ordering.</summary>
    private const int FilledDiamondFrame = 0;
    /// <summary>$9A:8400 second endpoint frame selects the hollow-center diamond; reviewed visual ordering.</summary>
    private const int HollowDiamondFrame = 1;
    /// <summary>$9A:8600 third endpoint frame selects the tipped square outline; reviewed visual ordering.</summary>
    private const int TippedSquareFrame = 2;
    /// <summary>$9A:8800 fourth endpoint frame selects the clipped diamond/inner spark; reviewed visual ordering.</summary>
    private const int ClippedDiamondFrame = 3;
    /// <summary>$9A:8200/8400/8600/8800 endpoint frames centered at pixel(4,3): filled diamond, hollow-center diamond, tipped square outline, clipped diamond outline with inner spark.</summary>
    internal static byte[] Point(int frame)
    {
        if ((uint)frame > ClippedDiamondFrame) throw new ArgumentOutOfRangeException(nameof(frame));
        var pixels = new byte[64];
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int dx = Math.Abs(x - CenterX), dy = Math.Abs(y - CenterY);
            int diamondDistance = dx + dy, squareDistance = Math.Max(dx, dy);
            bool filled = frame switch
            {
                FilledDiamondFrame => diamondDistance <= SmallRadius,
                HollowDiamondFrame => diamondDistance is <= MiddleRadius and not 0,
                TippedSquareFrame => squareDistance == SmallRadius || (diamondDistance == MiddleRadius && (dx == 0 || dy == 0)),
                _ => diamondDistance <= InnerRadius || (diamondDistance == OuterRadius && squareDistance <= ClipRadius),
            };
            if (filled) pixels[y * 8 + x] = Pen;
        }
        return SnesPlanarTileEncoder.Encode(pixels, 8, 8, 4);
    }

    /// <summary>$9A:9220-929F vertical characters transpose the four horizontal characters at $8220-829F without reordering animation frames.</summary>
    internal static byte[] VerticalSegments(ReadOnlySpan<byte> horizontal)
    {
        if (horizontal.Length != 128) throw new ArgumentException("Grapple segments require four planar characters.", nameof(horizontal));
        var vertical = new byte[128];
        for (int tile = 0; tile < 4; tile++)
        for (int plane = 0; plane < 4; plane++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int start = tile * 32 + plane / 2 * 16 + plane % 2;
            vertical[start + y * 2] |= (byte)(((horizontal[start + x * 2] >> (7 - y)) & 1) << (7 - x));
        }
        return vertical;
    }
    /// <summary>$9A:8220-829F/8A20-8A9F use only transparency and index15, so all four bitplanes share the same eight coverage rows per tile.</summary>
    internal static byte[]? InkCoverage(ReadOnlySpan<byte> planar)
    {
        var coverage = new byte[planar.Length / 4];
        for (int tile = 0; tile < planar.Length / 32; tile++)
        for (int y = 0; y < 8; y++)
        {
            byte row = planar[tile * 32 + y * 2];
            for (int plane = 1; plane < 4; plane++)
                if (planar[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] != row) return null;
            coverage[tile * 8 + y] = row;
        }
        return coverage;
    }

    /// <summary>Encodes independent binary Grapple coverage as native transparency/index15 planar pixels.</summary>
    internal static byte[] EncodeInk(ReadOnlySpan<byte> coverage)
    {
        var planar = new byte[coverage.Length * 4];
        for (int tile = 0; tile < coverage.Length / 8; tile++)
        for (int y = 0; y < 8; y++)
        for (int plane = 0; plane < 4; plane++)
            planar[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] = (Pen & 1 << plane) != 0 ? coverage[tile * 8 + y] : (byte)0;
        return planar;
    }
}
