using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using System.Reflection;

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

        VerifyCinematicCodePointerCatalog();

        Console.WriteLine(
            "  Intro ROM data: resources, VRAM ranges, tilemaps, palette spans, text " +
            "objects, demo pointers, and typed actor audio agree.");
    }

    /// <summary>
    /// Checks every translated bank-$8B callback and every named bank-$8B/$8C list cursor.
    /// When the private cartridge is present, touching each address also catches an
    /// accidentally transcribed bank or an out-of-ROM pointer before a cinematic reaches it.
    /// </summary>
    private static void VerifyCinematicCodePointerCatalog()
    {
        ushort[] bank8BCodePointers = GetUshortConstants(typeof(CinematicCodePointers))
            .Where(field => field.Name != nameof(CinematicCodePointers.InstructionCommandBit))
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();
        ushort[] bank8BLists = GetUshortConstants(typeof(CinematicCodePointers.Lists))
            .Where(field => field.Name != nameof(CinematicCodePointers.Lists.MetroidEggParticleStride))
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();
        ushort[] bank8CLists = GetUshortConstants(typeof(CinematicCodePointers.BackgroundLists))
            .Concat(GetUshortConstants(typeof(CinematicCodePointers.IndirectData)))
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();

        AssertTrue(bank8BCodePointers.Length >= 50,
            "cinematic callback catalog covers translated bank-$8B code");
        AssertTrue(bank8BLists.Length >= 20,
            "cinematic list catalog covers translated bank-$8B streams");
        AssertTrue(bank8CLists.Length >= 9,
            "cinematic background catalog covers translated bank-$8C streams");
        AssertEqual(bank8BCodePointers.Length, bank8BCodePointers.Distinct().Count(),
            "cinematic callback pointers are unique");
        AssertEqual(bank8BLists.Length, bank8BLists.Distinct().Count(),
            "cinematic bank-$8B list pointers are unique");
        AssertEqual(bank8CLists.Length, bank8CLists.Distinct().Count(),
            "cinematic bank-$8C list pointers are unique");
        foreach (ushort pointer in bank8BCodePointers.Concat(bank8BLists).Concat(bank8CLists))
            AssertTrue(pointer >= 0x8000, $"cinematic pointer ${pointer:X4} is mapped");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
            return;

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (ushort pointer in bank8BCodePointers.Concat(bank8BLists))
            _ = bus.ReadByte(IntroCinematicRomData.Banks.CinematicCode | pointer);
        foreach (ushort pointer in bank8CLists)
            _ = bus.ReadByte((int)new SnesAddress(0x8c, pointer));
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
