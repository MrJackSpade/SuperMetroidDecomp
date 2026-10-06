namespace SuperMetroid.Core.Assets;

/// <summary>Exact primitive/transpose rules in the identified native Grapple characters; unmatched supplied pixels remain independent.</summary>
internal static class GrappleBeamTilePatterns
{
    /// <summary>$9A:8200/8400/8600/8800 chosen endpoint center X=4; its alignment remains required.</summary>
    private const int UnresolvedCenterX = 4;
    /// <summary>$9A:8200/8400/8600/8800 chosen endpoint center Y=3; its alignment remains required.</summary>
    private const int UnresolvedCenterY = 3;
    /// <summary>$9A:8200 chosen filled-diamond radius2 remains required.</summary>
    private const int UnresolvedFilledRadius = 2;
    /// <summary>$9A:8400 chosen hollow-center diamond radius3 remains required.</summary>
    private const int UnresolvedHollowRadius = 3;
    /// <summary>$9A:8600 chosen square-outline radius2 remains required.</summary>
    private const int UnresolvedSquareRadius = 2;
    /// <summary>$9A:8600 chosen cardinal tip distance3 remains required.</summary>
    private const int UnresolvedTipDistance = 3;
    /// <summary>$9A:8800 chosen outer-diamond radius4 remains required.</summary>
    private const int UnresolvedOuterRadius = 4;
    /// <summary>$9A:8800 chosen clipping half-width/height3 remains required.</summary>
    private const int UnresolvedClipRadius = 3;
    /// <summary>$9A:8800 chosen center-spark radius1 remains required.</summary>
    private const int UnresolvedInnerRadius = 1;
    /// <summary>$9A:8200-929F selected Grapple pixels use palette index15; this pen choice remains required independently of binary coverage.</summary>
    private const byte UnresolvedPen = 15;
    /// <summary>$9A:8200/$9B:BFBD first endpoint frame selects the filled diamond; ordering remains required.</summary>
    private const int FilledDiamondFrame = 0;
    /// <summary>$9A:8400 second endpoint frame selects the hollow-center diamond; ordering remains required.</summary>
    private const int HollowDiamondFrame = 1;
    /// <summary>$9A:8600 third endpoint frame selects the tipped square outline; ordering remains required.</summary>
    private const int TippedSquareFrame = 2;
    /// <summary>$9A:8800 fourth endpoint frame selects the clipped diamond/inner spark; ordering remains required.</summary>
    private const int ClippedDiamondFrame = 3;
    /// <summary>$9A:8200/8400/8600/8800 endpoint frames centered at pixel(4,3): filled diamond, hollow-center diamond, tipped square outline, clipped diamond outline with inner spark.</summary>
    internal static byte[] Point(int frame)
    {
        if ((uint)frame > ClippedDiamondFrame) throw new ArgumentOutOfRangeException(nameof(frame));
        var pixels = new byte[64];
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int dx = Math.Abs(x - UnresolvedCenterX), dy = Math.Abs(y - UnresolvedCenterY);
            int diamondDistance = dx + dy, squareDistance = Math.Max(dx, dy);
            bool filled = frame switch
            {
                FilledDiamondFrame => diamondDistance <= UnresolvedFilledRadius,
                HollowDiamondFrame => diamondDistance <= UnresolvedHollowRadius && diamondDistance != 0,
                TippedSquareFrame => squareDistance == UnresolvedSquareRadius || (diamondDistance == UnresolvedTipDistance && (dx == 0 || dy == 0)),
                _ => diamondDistance <= UnresolvedInnerRadius || (diamondDistance == UnresolvedOuterRadius && squareDistance <= UnresolvedClipRadius),
            };
            if (filled) pixels[y * 8 + x] = UnresolvedPen;
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
            planar[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] = (UnresolvedPen & 1 << plane) != 0 ? coverage[tile * 8 + y] : (byte)0;
        return planar;
    }
}
