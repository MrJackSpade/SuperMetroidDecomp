using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Composes the title's published artwork into the memory image the backends draw (#1358): the interleaved Mode 7
/// characters and map, the OBJ characters, and the Baby Metroid page the animation currently shows.
/// </summary>
[PpuLayoutBoundary("exact hardware test: the title's Mode 7 planes, OBJ characters and Baby Metroid page at their native words")]
internal static class TitleScreenMemory
{
    /// <summary>The memory image of <paramref name="artwork"/> with Baby Metroid source page <paramref name="babySourcePage"/> shown.</summary>
    internal static byte[] Compose(TitleGraphicsPresentation artwork, int babySourcePage)
    {
        ReadOnlySpan<byte> characters = artwork.FloorCharacters;
        ReadOnlySpan<byte> map = artwork.FloorMap;
        ReadOnlySpan<byte> objects = artwork.ObjectCharacters;
        ReadOnlySpan<byte> baby = artwork.BabyCharacters;
        if (characters.Length < TitleSequenceRomData.Transfers.FloorCharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than its $4000-byte DMA.");
        if (map.Length < TitleSequenceRomData.Transfers.FloorMapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");
        int page = babySourcePage * TitleSequenceRomData.Transfers.BabyCharacterPageByteCount;
        if (baby.Length < page + TitleSequenceRomData.Transfers.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");

        var vram = new SnesVram();
        // DMA mode zero to $2119 writes only high bytes and increments VMADD afterward.
        // The following $2118 fill and map DMA populate the corresponding low bytes.
        var mode7 = new Mode7Vram(vram);
        mode7.FillMap(TitleSequenceRomData.Transfers.FloorInitialMapByte, TitleSequenceRomData.Transfers.FloorCharacterByteCount);
        mode7.LoadCharacters(characters[..TitleSequenceRomData.Transfers.FloorCharacterByteCount]);
        mode7.LoadMap(map[..TitleSequenceRomData.Transfers.FloorMapByteCount]);
        var title = new TitleVram(vram);
        title.ObjectCharacters.Load(objects[..Math.Min(TitleSequenceRomData.Transfers.ObjectCharacterByteCount, objects.Length)]);
        // The instruction list cycles source pages 0,1,2,1 into Mode 7 destination word $3800, high byte only.
        title.LoadBabyMetroidPage(baby.Slice(page, TitleSequenceRomData.Transfers.BabyCharacterPageByteCount));
        return vram.CaptureImage();
    }

    /// <summary>The memory image <paramref name="title"/> shows at its current Baby Metroid frame.</summary>
    internal static byte[] Compose(TitleSequenceState title) =>
        Compose(title.TitleGraphics ?? throw new InvalidOperationException("Title rendering requires installed title artwork."),
            TitleSequenceRomData.Transfers.BabyAnimationSourcePages[title.BabyFrame]);
}
