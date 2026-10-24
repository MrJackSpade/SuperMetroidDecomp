using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Locates Crocomire's melt graphics in the cartridge and the image the cartridge assembles from them. The simulation names
/// each melt image only as its illustration and each row of it as a band; this extraction-side translation turns a band into
/// the bank and offset its graphics are read from, and the bytes of the image its copy fills, using
/// <see cref="CrocomireMeltingSourceDefinitions"/>.
/// </summary>
internal static class CrocomireMeltingSources
{
    /// <summary>The bank and the offset of the first graphics chunk of <paramref name="illustration"/>'s image.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="illustration"/> is not a melt image.</exception>
    internal static MeltingGraphicsSource Graphics(CrocomireMeltingIllustration illustration) => illustration switch
    {
        CrocomireMeltingIllustration.First =>
            new MeltingGraphicsSource(CrocomireMeltingSourceDefinitions.GraphicsBank, CrocomireMeltingSourceDefinitions.FirstPassFirstChunk),
        CrocomireMeltingIllustration.Second =>
            new MeltingGraphicsSource(CrocomireMeltingSourceDefinitions.GraphicsBank, CrocomireMeltingSourceDefinitions.SecondPassFirstChunk),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireMeltingIllustration)} {illustration}."),
    };

    /// <summary>Offset within its image's bank of <paramref name="band"/>'s graphics chunk: the first chunk plus one stride per earlier band.</summary>
    internal static int SourceWord(CrocomireMeltingIllustration illustration, int band) =>
        Graphics(illustration).FirstChunk + band * CrocomireMeltingSourceDefinitions.ChunkStride;

    /// <summary>
    /// The byte of the assembled image <paramref name="band"/>'s copy starts at. Each copy moves one word more than a row,
    /// so a band's copy also fills the first word of the next row until the next band's copy replaces it.
    /// </summary>
    internal static int ImageByte(int band) => band * CrocomireMeltingSourceDefinitions.ChunkStride;
}
