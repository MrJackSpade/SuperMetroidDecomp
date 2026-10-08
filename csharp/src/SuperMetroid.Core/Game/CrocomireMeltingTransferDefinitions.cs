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

    internal static CrocomireMeltingPassSequence Passes => new();

    /// <summary>Six chunks in the first pass and seven in the second. Each header
    /// occupies eight bytes, each copy four, each upload eight, and each list ends
    /// with a two-byte sentinel. Derive serialized cursors from that layout.</summary>
    internal static CrocomireMeltingPass Header(ushort offset)
    {
        int chunks = offset switch
        {
            FirstHeaderOffset => 6,
            SecondHeaderOffset => 7,
            _ => throw new InvalidDataException($"Crocomire melt header offset ${offset:X4} is not compiled."),
        };
        ushort start = (ushort)(offset + 8 + 4 * chunks + 2);
        ushort end = (ushort)(start + 8 * chunks);
        return new(offset, 0x58, 0x30, 0x200, 0xa4, start, end, (ushort)(end + 2), chunks);
    }

    internal static CrocomireMeltingPass Transfers(ushort offset)
    {
        if (offset == Header(FirstHeaderOffset).TransferStartOffset) return Header(FirstHeaderOffset);
        if (offset == Header(SecondHeaderOffset).TransferStartOffset) return Header(SecondHeaderOffset);
        throw new InvalidDataException($"Crocomire melt transfer start ${offset:X4} is not compiled.");
    }

    /// <summary>Distinguishes the native transfer terminators from aligned records.</summary>
    internal static bool TryUpload(int offset, out CrocomireMeltingUpload upload)
    {
        var second = Header(SecondHeaderOffset);
        var pass = offset < second.TransferStartOffset ? Header(FirstHeaderOffset) : second;
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
            int source = Pass.HeaderOffset == CrocomireMeltingTransferDefinitions.FirstHeaderOffset ? 0xa07d : 0xac7d;
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
        0 => CrocomireMeltingTransferDefinitions.Header(CrocomireMeltingTransferDefinitions.FirstHeaderOffset),
        1 => CrocomireMeltingTransferDefinitions.Header(CrocomireMeltingTransferDefinitions.SecondHeaderOffset),
        _ => throw new IndexOutOfRangeException(),
    };
}
