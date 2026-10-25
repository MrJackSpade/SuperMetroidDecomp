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
    /// <summary>$A4:9C79, the first melting illustration's 256-word tilemap.</summary>
    internal const int FirstTilemap = 0xa49c79;
    /// <summary>$A4:9E7B, the second melting illustration's 256-word tilemap.</summary>
    internal const int SecondTilemap = 0xa49e7b;
    /// <summary>Bytes between consecutive graphics chunks of one pass in the cartridge.</summary>
    internal const int ChunkStride = 512;
    /// <summary>The last word each chunk's copy moves, counted from its first: the cartridge's copy counts inclusively.</summary>
    private const int CopyLastWord = 0x200;
    /// <summary>
    /// Bytes each chunk's copy moves into the assembled image: its own row, the next row and one word more, which the next
    /// chunk's copy overwrites.
    /// </summary>
    internal const int CopyByteCount = (CopyLastWord + 1) * sizeof(ushort);
}
