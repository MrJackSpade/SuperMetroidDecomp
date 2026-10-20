namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Native uploads: $0160 bytes from successive $0200-byte scratch chunks
/// in bank $7E, to successive $0100-word VRAM destinations.</summary>
internal readonly record struct CrocomireMeltingUploadSequence(CrocomireMeltingPass Pass)
{
    /// <summary>Number of scratch chunks uploaded in this melt pass.</summary>
    internal int Length => Pass.ChunkCount;
    /// <summary>Transfers $0160 bytes from one bank-$7E scratch chunk to its matching VRAM destination.</summary>
    internal CrocomireMeltingUpload this[int index]
    {
        get
        {
            if ((uint)index >= Length) throw new ArgumentOutOfRangeException(nameof(index));
            return new(CrocomireMeltingTransferDefinitions.UploadByteCount,
                index * CrocomireMeltingTransferDefinitions.UploadVramWordStride,
                CrocomireMeltingTransferDefinitions.ScratchBank,
                CrocomireMeltingTransferDefinitions.ScratchStart + index * CrocomireMeltingTransferDefinitions.ChunkByteStride);
        }
    }
}
