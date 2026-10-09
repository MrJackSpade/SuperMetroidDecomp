namespace SuperMetroid.Core.Game;

/// <summary>One source-to-scratch copy authored in Crocomire's melt table.</summary>
/// <param name="SourceWord">Bank-$A4 word address copied into the scratch buffer.</param>
/// <param name="DestinationWord">Scratch-memory word destination for the copied block.</param>
internal readonly record struct CrocomireMeltingCopy(ushort SourceWord, ushort DestinationWord);

/// <summary>One eight-byte scratch-to-VRAM transfer authored in the same table.</summary>
/// <param name="ByteCount">Number of source bytes transferred to VRAM.</param>
/// <param name="DestinationWord">Destination word address in VRAM.</param>
/// <param name="SourceBank">Bank containing the source bytes.</param>
/// <param name="SourceWord">Word address of the source bytes.</param>
internal readonly record struct CrocomireMeltingUpload(
    ushort ByteCount, ushort DestinationWord, byte SourceBank, ushort SourceWord);

/// <summary>
/// Fixed control and transfer metadata for one Crocomire dissolve pass. The graphics
/// bytes at the selected bank-$A4 sources remain presentation data, not mechanics.
/// </summary>
/// <param name="HeaderOffset">Byte offset of this pass header from the native table start.</param>
/// <param name="MaximumAdjustedDestinationY">Largest adjusted destination row accepted by this pass.</param>
/// <param name="DistortionEndY">Destination row where the melt distortion range ends.</param>
/// <param name="WordsToCopy">Native count of words copied into scratch memory.</param>
/// <param name="SourceBank">Bank supplying the upload bytes.</param>
/// <param name="TransferStartOffset">Offset of the first upload record in the native table.</param>
/// <param name="TransferEndOffset">Offset of the transfer-list terminator.</param>
/// <param name="NextHeaderOffset">Offset of the following pass header.</param>
/// <param name="ChunkCount">Number of copy and upload chunks in this pass.</param>
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
    /// <summary>Calculates the source and scratch destination pair for each chunk.</summary>
    internal CrocomireMeltingCopySequence Copies => new(this);
    /// <summary>Calculates the scratch-to-VRAM upload record for each chunk.</summary>
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

    /// <summary>Provides the two compiled pass headers in native execution order.</summary>
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

    /// <summary>Resolves a native upload-list start offset to its owning pass header.</summary>
    /// <param name="offset">Table-relative offset of the transfer list.</param>
    /// <returns>The pass whose upload records begin at that offset.</returns>
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
/// <param name="Pass">Pass metadata defining chunk count and source-table region.</param>
internal readonly record struct CrocomireMeltingCopySequence(CrocomireMeltingPass Pass)
{
    /// <summary>Number of source-to-scratch chunks in the selected pass.</summary>
    internal int Length => Pass.ChunkCount;
    /// <summary>Calculates the bank-$A4 source and scratch destination for one chunk.</summary>
    /// <param name="index">Zero-based chunk position.</param>
    internal CrocomireMeltingCopy this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new IndexOutOfRangeException();
            int source = Pass.HeaderOffset == CrocomireMeltingTransferDefinitions.FirstHeaderOffset ? 0xa07d : 0xac7d;
            return new((ushort)(source + index * 0x200), (ushort)(0x4000 + index * 0x200));
        }
    }

    /// <summary>Creates the value enumerator used by foreach over the calculated copy records.</summary>
    public Enumerator GetEnumerator() => new(this);
    /// <summary>Advances through copy records without allocating an iterator object.</summary>
    /// <param name="sequence">Calculated sequence whose chunks are enumerated.</param>
    internal struct Enumerator(CrocomireMeltingCopySequence sequence)
    {
        /// <summary>Index of the most recently yielded chunk, initialized before the first item.</summary>
        private int index = -1;
        /// <summary>Advances to the next copy record, returning false after the final chunk.</summary>
        public bool MoveNext() => ++index < sequence.Length;
        /// <summary>The source and scratch destination pair at the current index.</summary>
        public CrocomireMeltingCopy Current => sequence[index];
    }
}
/// <summary>Native uploads: $0160 bytes from successive $0200-byte scratch chunks
/// in bank $7E, to successive $0100-word VRAM destinations.</summary>
/// <param name="Pass">Pass metadata defining the number of upload chunks.</param>
internal readonly record struct CrocomireMeltingUploadSequence(CrocomireMeltingPass Pass)
{
    /// <summary>Number of scratch-to-VRAM transfers in the selected pass.</summary>
    internal int Length => Pass.ChunkCount;
    /// <summary>Calculates the transfer size, VRAM destination, and scratch source for one chunk.</summary>
    /// <param name="index">Zero-based upload position.</param>
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
    /// <summary>Gets one compiled pass in native order.</summary>
    /// <param name="index">Zero for the first pass or one for the second.</param>
    internal CrocomireMeltingPass this[int index] => index switch
    {
        0 => CrocomireMeltingTransferDefinitions.Header(CrocomireMeltingTransferDefinitions.FirstHeaderOffset),
        1 => CrocomireMeltingTransferDefinitions.Header(CrocomireMeltingTransferDefinitions.SecondHeaderOffset),
        _ => throw new IndexOutOfRangeException(),
    };
}
