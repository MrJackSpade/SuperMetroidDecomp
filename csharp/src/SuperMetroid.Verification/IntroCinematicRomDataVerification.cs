using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using static EnemyInstructionCodePointersVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Title;

/// <summary>Checks opening-cinematic ROM resource bounds, palette spans, and actor sound identities.</summary>
internal static class IntroCinematicRomDataVerification
{
    /// <summary>
    /// Verifies that intro resources are mapped, fixed VRAM regions fit, palette spans are
    /// byte-aligned/in-range, and actor sounds retain their library-qualified identities.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry, native VRAM words")]
    internal static void VerifyIntroCinematicRomData()
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
            (IntroVramLayout.CinematicObjectCharacters.Offset * 2) +
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

        Suite(nameof(VerifyCinematicCodePointerCatalog), () => VerifyCinematicCodePointerCatalog());

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
        // Instruction entry points are owned by closed per-interpreter enums; an entry point
        // shared by two owners (a common delete or cry) is one bank-$8B address.
        ushort[] bank8BCodePointers = GetUshortConstants(typeof(CinematicCodePointers))
            .Where(field => field.Name != nameof(CinematicCodePointers.InstructionCommandBit))
            .Select(field => field.RequiredConstant<ushort>())
            .Concat(new[]
            {
                typeof(CinematicSpriteInstruction), typeof(CinematicBackgroundInstruction),
                typeof(ZebesTitleInstruction), typeof(IntroEggInstruction),
                typeof(ConfusedBabyInstruction), typeof(IntroScientistBabyInstruction),
                typeof(IntroRinkaSpawnerInstruction), typeof(IntroRinkaInstruction),
                typeof(IntroRinkaPreInstruction), typeof(ConfusedBabyPreInstruction),
                typeof(EndingSpriteInstruction), typeof(EndingLogoInstruction),
                typeof(EndingRewardGestureInstruction), typeof(EndingRewardJumpInstruction),
                typeof(TitleSequenceInstruction),
            }.SelectMany(domain => Enum.GetValues(domain).Cast<object>().Select(Convert.ToUInt16)))
            .Where(pointer => pointer != 0)
            .Distinct()
            .ToArray();
        ushort[] bank8BLists = GetUshortConstants(typeof(CinematicCodePointers.Lists))
            .Where(field => field.Name != nameof(CinematicCodePointersListsConstants.MetroidEggParticleStride))
            .Select(field => field.RequiredConstant<ushort>())
            .ToArray();
        ushort[] bank8CLists = GetUshortConstants(typeof(CinematicCodePointers.BackgroundLists))
            .Concat(GetUshortConstants(typeof(CinematicCodePointersIndirectData)))
            .Select(field => field.RequiredConstant<ushort>())
            .ToArray();

        AssertTrue(bank8BCodePointers.Length >= 50,
            "cinematic callback catalog covers translated bank-$8B code");
        AssertTrue(bank8BLists.Length >= 20,
            "cinematic list catalog covers translated bank-$8B streams");
        AssertTrue(bank8CLists.Length >= 9,
            "cinematic background catalog covers translated bank-$8C streams");
        AssertEqual(GetUshortConstants(typeof(CinematicCodePointers)).Count(),
            GetUshortConstants(typeof(CinematicCodePointers))
                .Select(field => field.RequiredConstant<ushort>()).Distinct().Count(),
            "cinematic callback constants are unique");
        AssertEqual(bank8BLists.Length, bank8BLists.Distinct().Count(),
            "cinematic bank-$8B list pointers are unique");
        AssertEqual(bank8CLists.Length, bank8CLists.Distinct().Count(),
            "cinematic bank-$8C list pointers are unique");
        foreach (ushort pointer in bank8BCodePointers.Concat(bank8BLists).Concat(bank8CLists))
            AssertTrue(pointer >= 0x8000, $"cinematic pointer ${pointer:X4} is mapped");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
            return;

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach (ushort pointer in bank8BCodePointers.Concat(bank8BLists))
            _ = CinematicCodePointerCatalogRomByte(bus, IntroCinematicRomData.Banks.CinematicCode | pointer);
        foreach (ushort pointer in bank8CLists)
            _ = CinematicCodePointerCatalogRomByte(bus, (int)new SnesAddress(0x8c, pointer));
    }

    /// <summary>Reads a cartridge byte to validate a named cinematic code or list pointer.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for cinematic code pointer catalog")]
    private static byte CinematicCodePointerCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that a cinematic palette span is aligned, nonempty, and inside CGRAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void AssertIntroPaletteSpans(
        IntroCinematicRomData.Palette.Regions spans,
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
