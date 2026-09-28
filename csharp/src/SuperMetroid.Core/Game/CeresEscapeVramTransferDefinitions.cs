namespace SuperMetroid.Core.Game;

/// <summary>One seven-byte transfer record consumed by the Ceres escape graphics dispatcher.</summary>
internal readonly record struct CeresEscapeVramTransferDefinition(
    ushort Pointer, ushort ByteCount, int SourceAddress, ushort DestinationWord);

/// <summary>
/// Fixed transfer metadata from the three bank-$A6 Ceres escape lists. These
/// addresses describe DMA ordering and destinations; the source pixels remain
/// presentation data, not compiled mechanics.
/// </summary>
internal static class CeresEscapeVramTransferDefinitions
{
    /// <summary>$A6:0000, the fixed bank for Ceres escape transfer descriptors.</summary>
    internal const int Bank = 0xa60000;

    /// <summary>Escape timer sprite transfers beginning at $A6:C4CB.</summary>
    internal const ushort TimerSprites = 0xc4cb;

    /// <summary>Ceres escape timer BG1/BG2 and door transfers beginning at $A6:C4FE.</summary>
    internal const ushort TimerBackgrounds = 0xc4fe;

    /// <summary>Japanese-language warning overlay transfers beginning at $A6:C3B8.</summary>
    internal const ushort JapaneseOverlay = 0xc3b8;

    /// <summary>First warning-text character transfer at $A6:C4D9.</summary>
    internal const ushort WarningTextFirstTransfer = 0xc4d9;

    private static readonly CeresEscapeVramTransferDefinition[] Records =
    [
        new(0xc3b8, 0x0018, 0xa6c3f4, 0x528a),
        new(0xc3bf, 0x0018, 0xa6c40c, 0x52aa),
        new(0xc3c6, 0x0016, 0xa6c424, 0x52ca),
        new(0xc3cd, 0x0016, 0xa6c43a, 0x52ea),
        new(0xc4cb, 0x0200, 0xb0c000, 0x7e00),
        new(0xc4d2, 0x0120, 0xb0c200, 0x7f00),
        new(0xc4d9, 0x0200, 0xb7da00, 0x7820),
        new(0xc4e0, 0x0200, 0xb7dc00, 0x7920),
        new(0xc4e7, 0x0200, 0xb7de00, 0x7a20),
        new(0xc4ee, 0x0200, 0xb7e000, 0x7b20),
        new(0xc4f5, 0x0100, 0xb7e200, 0x7c20),
        new(0xc4fe, 0x0200, 0xb7da00, 0x1820),
        new(0xc505, 0x0200, 0xb7dc00, 0x1920),
        new(0xc50c, 0x0200, 0xb7de00, 0x1a20),
        new(0xc513, 0x0200, 0xb7e000, 0x1b20),
        new(0xc51a, 0x0100, 0xb7e200, 0x1c20),
        new(0xc521, 0x0200, 0xb0ba00, 0x0d00),
        new(0xc528, 0x0200, 0xb0bc00, 0x0e00),
        new(0xc52f, 0x0200, 0xb0be00, 0x0f00),
    ];

    internal static ReadOnlySpan<CeresEscapeVramTransferDefinition> All => Records;

    internal static bool IsTerminator(ushort pointer) =>
        pointer is 0xc3d4 or 0xc4fc or 0xc536;

    internal static bool IsDescriptorByteAddress(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (CeresEscapeVramTransferDefinition record in Records)
            if (offset >= record.Pointer && offset < record.Pointer + 7)
                return true;
        return IsTerminator(offset) ||
            IsTerminator(unchecked((ushort)(offset - 1)));
    }

    internal static bool TryGet(ushort pointer,
        out CeresEscapeVramTransferDefinition transfer)
    {
        foreach (CeresEscapeVramTransferDefinition candidate in Records)
        {
            if (candidate.Pointer == pointer)
            {
                transfer = candidate;
                return true;
            }
        }
        transfer = default;
        return false;
    }
}
