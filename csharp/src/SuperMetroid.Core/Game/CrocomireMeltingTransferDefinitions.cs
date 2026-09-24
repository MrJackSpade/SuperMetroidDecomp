namespace SuperMetroid.Core.Game;

/// <summary>One source-to-scratch copy authored in Crocomire's melt table.</summary>
internal readonly record struct CrocomireMeltingCopy(ushort SourceWord, ushort DestinationWord);

/// <summary>One eight-byte scratch-to-VRAM transfer authored in the same table.</summary>
internal readonly record struct CrocomireMeltingUpload(
    ushort ByteCount, ushort DestinationWord, byte SourceBank, ushort SourceWord);

/// <summary>
/// Fixed control and transfer metadata for one Crocomire dissolve pass. The graphics
/// bytes at the selected bank-$A4 sources remain presentation data, not mechanics.
/// </summary>
internal readonly record struct CrocomireMeltingPass(
    ushort HeaderOffset,
    ushort MaximumAdjustedDestinationY,
    ushort DistortionEndY,
    ushort WordsToCopy,
    byte SourceBank,
    ushort TransferStartOffset,
    ushort TransferEndOffset,
    ushort NextHeaderOffset,
    ReadOnlyMemory<CrocomireMeltingCopy> Copies,
    ReadOnlyMemory<CrocomireMeltingUpload> Uploads);

/// <summary>
/// The two complete mixed instruction/data records at $A4:9BC5-$A4:9C78.
/// Keeping native offsets preserves Crocomire's serialized melt cursor while removing
/// its runtime metadata reads from the cartridge.
/// </summary>
internal static class CrocomireMeltingTransferDefinitions
{
    /// <summary>First header's native offset from $A4:9BC5.</summary>
    internal const ushort FirstHeaderOffset = 0x0000;

    /// <summary>Second header's native offset from $A4:9BC5.</summary>
    internal const ushort SecondHeaderOffset = 0x0054;

    /// <summary>Native bank-$A4 base for independent ROM-oracle comparison.</summary>
    internal const int NativeSourceAddress = 0xa49bc5;

    /// <summary>Exclusive end of the two native records.</summary>
    internal const int NativeByteCount = 0x00b4;

    private static readonly CrocomireMeltingPass First = new(
        HeaderOffset: FirstHeaderOffset,
        MaximumAdjustedDestinationY: 0x0058,
        DistortionEndY: 0x0030,
        WordsToCopy: 0x0200,
        SourceBank: 0xa4,
        TransferStartOffset: 0x0022,
        TransferEndOffset: 0x0052,
        NextHeaderOffset: SecondHeaderOffset,
        Copies: new CrocomireMeltingCopy[]
        {
            new(0xa07d, 0x4000), new(0xa27d, 0x4200),
            new(0xa47d, 0x4400), new(0xa67d, 0x4600),
            new(0xa87d, 0x4800), new(0xaa7d, 0x4a00),
        },
        Uploads: new CrocomireMeltingUpload[]
        {
            new(0x0160, 0x0000, 0x7e, 0x4000),
            new(0x0160, 0x0100, 0x7e, 0x4200),
            new(0x0160, 0x0200, 0x7e, 0x4400),
            new(0x0160, 0x0300, 0x7e, 0x4600),
            new(0x0160, 0x0400, 0x7e, 0x4800),
            new(0x0160, 0x0500, 0x7e, 0x4a00),
        });

    private static readonly CrocomireMeltingPass Second = new(
        HeaderOffset: SecondHeaderOffset,
        MaximumAdjustedDestinationY: 0x0058,
        DistortionEndY: 0x0030,
        WordsToCopy: 0x0200,
        SourceBank: 0xa4,
        TransferStartOffset: 0x007a,
        TransferEndOffset: 0x00b2,
        NextHeaderOffset: NativeByteCount,
        Copies: new CrocomireMeltingCopy[]
        {
            new(0xac7d, 0x4000), new(0xae7d, 0x4200),
            new(0xb07d, 0x4400), new(0xb27d, 0x4600),
            new(0xb47d, 0x4800), new(0xb67d, 0x4a00),
            new(0xb87d, 0x4c00),
        },
        Uploads: new CrocomireMeltingUpload[]
        {
            new(0x0160, 0x0000, 0x7e, 0x4000),
            new(0x0160, 0x0100, 0x7e, 0x4200),
            new(0x0160, 0x0200, 0x7e, 0x4400),
            new(0x0160, 0x0300, 0x7e, 0x4600),
            new(0x0160, 0x0400, 0x7e, 0x4800),
            new(0x0160, 0x0500, 0x7e, 0x4a00),
            new(0x0160, 0x0600, 0x7e, 0x4c00),
        });

    private static readonly CrocomireMeltingPass[] AllPasses = [First, Second];

    internal static ReadOnlySpan<CrocomireMeltingPass> Passes => AllPasses;

    internal static CrocomireMeltingPass Header(ushort offset) => offset switch
    {
        FirstHeaderOffset => First,
        SecondHeaderOffset => Second,
        _ => throw new InvalidDataException(
            $"Crocomire melt header offset ${offset:X4} is not compiled."),
    };

    internal static CrocomireMeltingPass Transfers(ushort offset) => offset switch
    {
        0x0022 => First,
        0x007a => Second,
        _ => throw new InvalidDataException(
            $"Crocomire melt transfer start ${offset:X4} is not compiled."),
    };

    /// <summary>Distinguishes a native transfer terminator from a valid record.</summary>
    internal static bool TryUpload(int offset, out CrocomireMeltingUpload upload)
    {
        CrocomireMeltingPass pass = offset < Second.TransferStartOffset ? First : Second;
        if (offset == pass.TransferEndOffset)
        {
            upload = default;
            return false;
        }
        int index = offset - pass.TransferStartOffset;
        if (index < 0 || index % 8 != 0 || index >= pass.Uploads.Length * 8)
            throw new InvalidDataException(
                $"Crocomire melt transfer offset ${offset:X4} is not compiled.");
        upload = pass.Uploads.Span[index / 8];
        return true;
    }
}
