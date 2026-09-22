namespace SuperMetroid.Core.Game;

/// <summary>Compiled physical column order and bitplane masks for Crocomire's melt.</summary>
internal static class CrocomireMeltingDefinitions
{
    /// <summary>
    /// <c>CrocomireMeltingXOffsetTable</c> at <c>$A4:9697-$A4:96C7</c>.
    /// The cartridge uses this permutation to choose the next physical X column.
    /// </summary>
    /// <remarks>
    /// All 49 bytes match the pinned NTSC J/U v1.0 ROM and permute X columns
    /// 0..48 exactly once. Retain this bounded authored dissolve order: a
    /// modular stride or simple block traversal does not reproduce it, and
    /// a guessed shuffle would obscure the visual sequence. The separate
    /// bitplane mask uses the chronological cursor's low three bits, not the
    /// selected X column's. Investigation: #625 / #672.
    /// </remarks>
    private static ReadOnlySpan<byte> ColumnOrder =>
    [
        0x2b, 0x28, 0x21, 0x1f, 0x2c, 0x10, 0x16, 0x17,
        0x0f, 0x00, 0x06, 0x07, 0x0b, 0x08, 0x01, 0x2a,
        0x0c, 0x24, 0x2e, 0x2d, 0x1a, 0x14, 0x1d, 0x23,
        0x1e, 0x29, 0x25, 0x22, 0x13, 0x19, 0x15, 0x12,
        0x30, 0x03, 0x09, 0x02, 0x1b, 0x05, 0x18, 0x1c,
        0x11, 0x0a, 0x04, 0x0d, 0x2f, 0x0e, 0x20, 0x26,
        0x27,
    ];

    /// <summary>
    /// <c>TilePixelColumnBitmasks</c> at <c>$A4:9BBD-$A4:9BC4</c>. Native indexes
    /// these by chronological table cursor rather than selected X column.
    /// </summary>
    /// <remarks>
    /// Exact bounded rule for cursor 0..48:
    /// <c>$FF XOR ($80 &gt;&gt; (cursor &amp; 7))</c>. All eight physical bytes
    /// match the pinned NTSC J/U v1.0 ROM; the rule reproduces all 49 cursor
    /// results and the complete production erase silhouette. The cartridge
    /// selects the mask by chronological cursor, not by the separately selected
    /// X column. This proof covers only the masks, not that column permutation.
    /// Investigation: #625 / #673.
    /// </remarks>
    private static ReadOnlySpan<byte> ColumnMasks =>
        [0x7f, 0xbf, 0xdf, 0xef, 0xf7, 0xfb, 0xfd, 0xfe];

    /// <summary>The number of authored physical columns in the melt permutation.</summary>
    public const int ColumnCount = 49;

    /// <summary>Returns the physical X column selected by one chronological cursor.</summary>
    public static byte SelectColumn(int cursor)
    {
        if ((uint)cursor >= ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(cursor));
        return ColumnOrder[cursor];
    }

    /// <summary>
    /// Returns the native clear mask for a chronological cursor, preserving the shipped
    /// bug that uses <c>cursor &amp; 7</c> instead of the selected column's low bits.
    /// </summary>
    public static byte SelectMask(int cursor)
    {
        if ((uint)cursor >= ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(cursor));
        return ColumnMasks[cursor & 7];
    }
}
