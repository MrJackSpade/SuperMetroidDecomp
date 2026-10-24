namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where Crocomire's melt graphics live in the cartridge: extraction-only data that
/// <see cref="CrocomireMeltingSources"/> reads to locate each graphics chunk.
/// </summary>
internal static class CrocomireMeltingSourceDefinitions
{
    /// <summary>Bank $A4, which holds both passes' melt graphics.</summary>
    internal const int GraphicsBank = 0xa4;
    /// <summary>$A4:A07D, the first pass's first graphics chunk.</summary>
    internal const int FirstPassFirstChunk = 0xa07d;
    /// <summary>$A4:AC7D, the second pass's first graphics chunk.</summary>
    internal const int SecondPassFirstChunk = 0xac7d;
    /// <summary>Bytes between consecutive graphics chunks of one pass in the cartridge.</summary>
    internal const int ChunkStride = 512;
    /// <summary>
    /// Bytes each chunk's copy moves into the assembled image: one word more than the stride, as the cartridge's copy
    /// counts its word total inclusively.
    /// </summary>
    internal const int CopyByteCount = ChunkStride + sizeof(ushort);
}
