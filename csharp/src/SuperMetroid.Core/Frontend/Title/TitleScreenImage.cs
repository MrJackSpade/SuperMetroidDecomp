using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Composes the title's published artwork into the memory image the backends draw (#1358): the interleaved Mode 7
/// characters and map, the OBJ characters, and the Baby Metroid page the animation currently shows.
/// </summary>
[PpuLayoutBoundary("codec: the title's Mode 7 planes, OBJ characters and Baby Metroid page at their native words")]
internal static class TitleScreenImage
{
    /// <summary>The memory image of <paramref name="artwork"/> with Baby Metroid source page <paramref name="babySourcePage"/> shown.</summary>
    internal static byte[] Compose(TitleGraphicsPresentation artwork, int babySourcePage)
    {
        ReadOnlySpan<byte> characters = artwork.Mode7Characters;
        ReadOnlySpan<byte> map = artwork.Mode7Map;
        ReadOnlySpan<byte> objects = artwork.ObjectCharacters;
        ReadOnlySpan<byte> baby = artwork.BabyCharacters;
        if (characters.Length < TitleSequenceRomData.Vram.Mode7CharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than its $4000-byte DMA.");
        if (map.Length < TitleSequenceRomData.Vram.Mode7MapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");
        int page = babySourcePage * TitleSequenceRomData.Vram.BabyCharacterPageByteCount;
        if (baby.Length < page + TitleSequenceRomData.Vram.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");

        var vram = new SnesVram();
        // DMA mode zero to $2119 writes only high bytes and increments VMADD afterward.
        // The following $2118 fill and map DMA populate the corresponding low bytes.
        var mode7 = new Mode7Vram(vram);
        mode7.FillMap(TitleSequenceRomData.Vram.Mode7InitialMapByte, TitleSequenceRomData.Vram.Mode7CharacterByteCount);
        mode7.LoadCharacters(characters[..TitleSequenceRomData.Vram.Mode7CharacterByteCount]);
        mode7.LoadMap(map[..TitleSequenceRomData.Vram.Mode7MapByteCount]);
        var title = new TitleVram(vram);
        title.ObjectCharacters.Load(objects[..Math.Min(TitleSequenceRomData.Vram.ObjectCharacterByteCount, objects.Length)]);
        // The instruction list cycles source pages 0,1,2,1 into Mode 7 destination word $3800, high byte only.
        title.LoadBabyMetroidPage(baby.Slice(page, TitleSequenceRomData.Vram.BabyCharacterPageByteCount));
        return vram.CaptureImage();
    }
}
