using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The two complete mixed instruction/data records at $A4:9BC5-$A4:9C78.
/// Keeping native offsets preserves Crocomire's serialized melt cursor while removing
/// its runtime metadata reads from the cartridge.
/// </summary>
internal static class CrocomireMeltingTransferDefinitions
{
    /// <summary>Bank $7E: the melt's graphics scratch image lives in work RAM.</summary>
    internal const byte ScratchBank = 0x7e;
    /// <summary><c>$7E:4000</c>: the first byte of the melt's graphics scratch image.</summary>
    internal const ushort ScratchStart = 0x4000;
    /// <summary>Projects the two native melt passes without storing duplicate transfer records.</summary>
    internal static CrocomireMeltingPassSequence Passes => new();

    /// <summary>Six chunks in the first pass and seven in the second. Each header
    /// occupies eight bytes, each copy four, each upload eight, and each list ends
    /// with a two-byte sentinel. Derive serialized cursors from that layout.</summary>
    internal static CrocomireMeltingPass Header(ushort offset)
    {
        int chunks = ClosedNativeWords.Decode<CrocomireMeltingHeader>(offset, "Crocomire melt header offset") switch
        {
            CrocomireMeltingHeader.First => 6,
            CrocomireMeltingHeader.Second => 7,
            _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireMeltingHeader)} {offset:X4}."),
        };
        ushort start = (ushort)(offset + 8 + 4 * chunks + 2);
        ushort end = (ushort)(start + 8 * chunks);
        return new(offset, 0x58, 0x30, 0x200, 0xa4, start, end, (ushort)(end + 2), chunks);
    }

    /// <summary>Accepts only the transfer-list start of one of the two compiled melt headers.</summary>
    internal static CrocomireMeltingPass Transfers(ushort offset)
    {
        if (offset == Header((ushort)CrocomireMeltingHeader.First).TransferStartOffset) return Header((ushort)CrocomireMeltingHeader.First);
        if (offset == Header((ushort)CrocomireMeltingHeader.Second).TransferStartOffset) return Header((ushort)CrocomireMeltingHeader.Second);
        throw new InvalidDataException($"Crocomire melt transfer start ${offset:X4} is not compiled.");
    }

    /// <summary>Distinguishes the native transfer terminators from aligned records.</summary>
    internal static bool TryUpload(int offset, out CrocomireMeltingUpload upload)
    {
        var second = Header((ushort)CrocomireMeltingHeader.Second);
        var pass = offset < second.TransferStartOffset ? Header((ushort)CrocomireMeltingHeader.First) : second;
        if (offset == pass.TransferEndOffset)
        {
            upload = default;
            return false;
        }
        int index = offset - pass.TransferStartOffset;
        if (index < 0 || index % 8 != 0 || index >= pass.ChunkCount * 8)
            throw new InvalidDataException($"Crocomire melt transfer offset ${offset:X4} is not compiled.");
        upload = pass.Uploads[index / 8];
        return true;
    }
}
