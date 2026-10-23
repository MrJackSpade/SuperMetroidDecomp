using SuperMetroid.Core.Frontend.Intro.Artwork;
namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The intro tilemaps that change as it runs (#1358): the Samus portrait BG2 tilemap, whose eye rectangle the object
/// system redraws, and the narration BG3 tilemap the staging map is uploaded into. The capture places them.
/// </summary>
internal sealed class IntroTilemaps
{
    /// <summary>The four scene pages' words, page after page.</summary>
    private readonly ushort[] scenePages = new ushort[IntroCinematicArtworkFormat.BackgroundPageCount * SnesPpuLayout.TilemapPageWordCount];

    /// <summary>The portrait tilemap's words.</summary>
    private readonly ushort[] portrait = new ushort[SnesPpuLayout.TilemapPageWordCount];

    /// <summary>The narration tilemap's words.</summary>
    private readonly ushort[] narration = new ushort[IntroCinematicRomData.Layers.TextTilemapWordCount];

    /// <summary>The portrait tilemap.</summary>
    internal Span<ushort> Portrait => portrait;

    /// <summary>The narration tilemap.</summary>
    internal Span<ushort> Narration => narration;

    /// <summary>The tilemap of one scene page.</summary>
    internal ReadOnlySpan<ushort> ScenePage(IntroScenePage page) =>
        scenePages.AsSpan((int)page * SnesPpuLayout.TilemapPageWordCount, SnesPpuLayout.TilemapPageWordCount);

    /// <summary>Loads the installed scene pages.</summary>
    internal void LoadScenePages(ReadOnlySpan<byte> littleEndianWords) =>
        Load(scenePages, littleEndianWords[..(scenePages.Length * sizeof(ushort))]);

    /// <summary>Loads the installed portrait image.</summary>
    internal void LoadPortrait(ReadOnlySpan<byte> littleEndianWords) =>
        Load(portrait, littleEndianWords[..IntroCinematicRomData.Transfers.SamusHeadTilemapBytes]);

    /// <summary>Loads the installed first narration card.</summary>
    internal void LoadNarration(ReadOnlySpan<byte> littleEndianWords) =>
        Load(narration, littleEndianWords[..IntroCinematicRomData.Transfers.NarrationTilemapBytes]);

    /// <summary>Loads little-endian words into the start of <paramref name="destination"/>.</summary>
    private static void Load(Span<ushort> destination, ReadOnlySpan<byte> littleEndianWords)
    {
        for (int word = 0; word < littleEndianWords.Length / sizeof(ushort); word++)
            destination[word] = SnesLittleEndian.Word(littleEndianWords[word * 2], littleEndianWords[word * 2 + 1]);
    }
}
