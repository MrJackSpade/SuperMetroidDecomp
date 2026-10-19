using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Locates Crocomire's melt graphics in the cartridge. The simulation names each graphics
/// chunk only by its pass and its position within that pass; this extraction-side
/// translation turns that identity into the bank and offset the chunk is read from when
/// the stock melt images are extracted, using <see cref="CrocomireMeltingSourceDefinitions"/>.
/// </summary>
internal static class CrocomireMeltingSources
{
    /// <summary>The cartridge bank a melt pass's graphics chunks are read from.</summary>
    extension(CrocomireMeltingPass pass)
    {
        /// <summary>Bank of every graphics chunk the pass copies; both passes' graphics live in the same bank.</summary>
        internal int SourceBank => Graphics((CrocomireMeltingHeader)pass.HeaderOffset).Bank;
    }

    /// <summary>The cartridge offset a melt copy's graphics chunk is read from.</summary>
    extension(CrocomireMeltingCopy copy)
    {
        /// <summary>Offset within its pass's bank of the copied chunk: the pass's first chunk plus one stride per earlier chunk.</summary>
        internal int SourceWord => Graphics(copy.Pass).FirstChunk + copy.Chunk * CrocomireMeltingSourceDefinitions.ChunkStride;
    }

    /// <summary>The bank and the offset of the first graphics chunk of <paramref name="pass"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="pass"/> is not a defined melt pass.</exception>
    private static (int Bank, int FirstChunk) Graphics(CrocomireMeltingHeader pass) => pass switch
    {
        CrocomireMeltingHeader.First =>
            (CrocomireMeltingSourceDefinitions.GraphicsBank, CrocomireMeltingSourceDefinitions.FirstPassFirstChunk),
        CrocomireMeltingHeader.Second =>
            (CrocomireMeltingSourceDefinitions.GraphicsBank, CrocomireMeltingSourceDefinitions.SecondPassFirstChunk),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireMeltingHeader)} {pass}."),
    };
}
