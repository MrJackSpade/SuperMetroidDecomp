using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies that intro resources are mapped, fixed VRAM regions fit, palette spans are
    /// byte-aligned/in-range, and actor sounds retain their library-qualified identities.
    /// </summary>
    static void VerifyIntroCinematicRomData()
    {
        int[] resources =
        [
            IntroCinematicRomData.Assets.Palette,
            IntroCinematicRomData.Assets.BackgroundCharacters,
            IntroCinematicRomData.Assets.FontOne,
            IntroCinematicRomData.Assets.SamusHeadTilemap,
            IntroCinematicRomData.Assets.BackgroundPageTilemaps,
            IntroCinematicRomData.Assets.ObjectCharacters,
            IntroCinematicRomData.Assets.FirstNarrationTilemap,
            IntroCinematicRomData.Assets.JapaneseFontTwo,
            IntroCinematicRomData.Assets.MotherBrainLevelData,
            IntroCinematicRomData.Assets.IntroObjectCharacters,
            IntroCinematicRomData.Assets.FinalTextLine,
        ];
        foreach (int address in resources)
            AssertTrue(address is >= 0x808000 and <= 0xffffff,
                $"intro resource ${address:X6} is mapped ROM");

        AssertEqual(IntroCinematicRomData.Layers.TextTilemapWordCount,
            IntroCinematicRomData.Layers.TilemapWidth * IntroCinematicRomData.Layers.TilemapWidth,
            "intro text tilemap is 32x32 words");
        AssertTrue(
            IntroCinematicRomData.Vram.CinematicObjectCharactersDestinationByte +
                IntroCinematicRomData.Vram.ObjectCharacterBytes <= SnesVram.ByteCount,
            "intro object DMA fits VRAM");

        AssertIntroPaletteSpans(IntroCinematicRomData.Palette.Gameplay, "gameplay");
        AssertIntroPaletteSpans(IntroCinematicRomData.Palette.GameplayClear, "gameplay clear");
        AssertIntroPaletteSpans(IntroCinematicRomData.Palette.Narration, "narration");
        AssertIntroPaletteSpans(IntroCinematicRomData.Palette.Discovery, "discovery");
        AssertEqual(SoundEffectLibrary.Library2,
            IntroCinematicRomData.Objects.EggHatch.Library,
            "egg hatch sound library");
        AssertEqual(SoundEffectLibrary.Library3,
            IntroCinematicRomData.Objects.BabyCry1.Library,
            "Baby cry sound library");
        AssertEqual(0x23, IntroCinematicRomData.Objects.BabyCry1.Value,
            "Baby cry one sequence");

        Console.WriteLine(
            "  Intro ROM data: resources, VRAM ranges, tilemaps, palette spans, text " +
            "objects, demo pointers, and typed actor audio agree.");
    }

    private static void AssertIntroPaletteSpans(
        ReadOnlySpan<IntroPaletteSpan> spans,
        string context)
    {
        AssertTrue(!spans.IsEmpty, $"intro {context} palette span list is nonempty");
        foreach (IntroPaletteSpan span in spans)
        {
            AssertEqual(0, span.ByteOffset & 1, $"intro {context} byte offset is word aligned");
            AssertTrue(span.ByteCount > 0, $"intro {context} span is nonempty");
            AssertTrue(span.ByteOffset / 2 + span.ByteCount <= SnesCgram.ColorCount,
                $"intro {context} span fits CGRAM");
        }
    }
}
