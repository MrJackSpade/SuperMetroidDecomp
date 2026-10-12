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
    int ChunkCount)
{
    internal CrocomireMeltingCopySequence Copies => new(this);
    internal CrocomireMeltingUploadSequence Uploads => new(this);
}

/// <summary>The two melt-pass header offsets from $A4:9BC5, Crocomire's serialized melt cursor.</summary>
internal enum CrocomireMeltingHeader : ushort
{
    /// <summary>First header's native offset from $A4:9BC5.</summary>
    First = 0x0000,
    /// <summary>Second header's native offset from $A4:9BC5.</summary>
    Second = 0x0054,
}

/// <summary>
/// The two complete mixed instruction/data records at $A4:9BC5-$A4:9C78.
/// Keeping native offsets preserves Crocomire's serialized melt cursor while removing
/// its runtime metadata reads from the cartridge.
/// </summary>
internal static class CrocomireMeltingTransferDefinitions
{
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

/// <summary>Native copy pairs: consecutive $0200-byte source/scratch strides.
/// The second pass includes $A4:B87D (palette and adjacent data) as its seventh
/// source. Preserve that native chunk and the caller's inclusive $0201-word copy.</summary>
internal readonly record struct CrocomireMeltingCopySequence(CrocomireMeltingPass Pass)
{
    internal int Length => Pass.ChunkCount;
    internal CrocomireMeltingCopy this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            int source = Pass.HeaderOffset == (ushort)CrocomireMeltingHeader.First ? 0xa07d : 0xac7d;
            return new((ushort)(source + index * 0x200), (ushort)(0x4000 + index * 0x200));
        }
    }

    public Enumerator GetEnumerator() => new(this);
    internal struct Enumerator(CrocomireMeltingCopySequence sequence)
    {
        private int index = -1;
        public bool MoveNext() => ++index < sequence.Length;
        public CrocomireMeltingCopy Current => sequence[index];
    }
}
/// <summary>Native uploads: $0160 bytes from successive $0200-byte scratch chunks
/// in bank $7E, to successive $0100-word VRAM destinations.</summary>
internal readonly record struct CrocomireMeltingUploadSequence(CrocomireMeltingPass Pass)
{
    internal int Length => Pass.ChunkCount;
    internal CrocomireMeltingUpload this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            return new(0x160, (ushort)(index * 0x100), 0x7e, (ushort)(0x4000 + index * 0x200));
        }
    }
}
/// <summary>Enumerates the two native melt passes without stored records.</summary>
internal readonly record struct CrocomireMeltingPassSequence()
{
    internal CrocomireMeltingPass this[int index] => index switch
    {
        0 => CrocomireMeltingTransferDefinitions.Header((ushort)CrocomireMeltingHeader.First),
        1 => CrocomireMeltingTransferDefinitions.Header((ushort)CrocomireMeltingHeader.Second),
        _ => throw new IndexOutOfRangeException(),
    };
}
