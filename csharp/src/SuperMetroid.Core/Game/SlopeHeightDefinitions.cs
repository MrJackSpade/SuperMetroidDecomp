namespace SuperMetroid.Core.Game;

/// <summary>Shared discrete collision heights for Samus, enemies, projectiles and bomb spread.</summary>
public static class SlopeHeightDefinitions
{
    /// <summary>
    /// $94:8B2B..8D2A, native SlopeDefinitions_SlopeTopXOffsetByYPixel (kAlignYPos_Tab0).
    /// Despite the assembly label's swapped axes, these are top Y offsets indexed by X:
    /// thirty-two authored sixteen-column profiles, including unused/overhanging shapes.
    /// </summary>
    private static ReadOnlySpan<byte> Heights =>
    [
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        16, 16, 16, 16, 16, 16, 16, 16, 0, 0, 0, 0, 0, 0, 0, 0,
        16, 16, 16, 16, 16, 16, 16, 16, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        16, 15, 14, 13, 12, 11, 10, 9, 9, 10, 11, 12, 13, 14, 15, 16,
        16, 14, 12, 10, 8, 6, 4, 2, 2, 4, 6, 8, 10, 12, 14, 16,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        12, 12, 12, 12, 8, 8, 8, 8, 4, 4, 4, 4, 0, 0, 0, 0,
        14, 14, 12, 12, 10, 10, 8, 8, 6, 6, 4, 4, 2, 2, 0, 0,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 16, 16, 16,
        16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 15, 14, 13, 12, 11, 10, 9,
        8, 7, 6, 5, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0,
        16, 16, 15, 15, 14, 14, 13, 13, 12, 12, 11, 11, 10, 10, 9, 9,
        8, 8, 7, 7, 6, 6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1,
        16, 16, 16, 15, 15, 15, 14, 14, 14, 13, 13, 13, 12, 12, 12, 11,
        11, 11, 10, 10, 10, 9, 9, 9, 8, 8, 8, 7, 7, 7, 6, 6,
        6, 5, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 1, 1, 1,
        20, 20, 20, 20, 20, 20, 20, 20, 16, 14, 12, 10, 8, 6, 4, 2,
        16, 14, 12, 10, 8, 6, 4, 2, 0, 0, 0, 0, 0, 0, 0, 0,
        20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 15, 12, 9, 6, 3,
        20, 20, 20, 20, 20, 20, 14, 11, 8, 5, 2, 0, 0, 0, 0, 0,
        16, 13, 10, 7, 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    ];

    /// <summary>Returns the native five-bit height. Callers own BTS mirroring and floor/ceiling collision rules.</summary>
    public static byte Read(int shape, int column)
    {
        if ((uint)shape >= 32) throw new ArgumentOutOfRangeException(nameof(shape));
        if ((uint)column >= 16) throw new ArgumentOutOfRangeException(nameof(column));
        return (byte)(Heights[shape * 16 + column] & 0x1f);
    }
}
