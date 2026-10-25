using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyInstructionCodePointersVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks opening-cinematic ROM resource bounds, palette spans, and actor sound identities.</summary>
internal static class IntroCinematicRomDataVerification
{
    /// <summary>
    /// Verifies that the import's intro sources are mapped, fixed VRAM regions fit, palette spans are
    /// byte-aligned/in-range, and actor sounds retain their library-qualified identities. Since #1416 the
    /// sources are the import's (IntroCinematicSourceDefinitions) and the extents Core's definitions.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry, native VRAM words")]
    internal static void VerifyIntroCinematicRomData()
    {
        int[] resources =
        [
            IntroCinematicSourceDefinitions.Palette,
            IntroCinematicSourceDefinitions.BackgroundCharacters,
            IntroCinematicSourceDefinitions.FontOne,
            IntroCinematicSourceDefinitions.SamusHeadTilemap,
            IntroCinematicSourceDefinitions.BackgroundPageTilemaps,
            IntroCinematicSourceDefinitions.ObjectCharacters,
            IntroCinematicSourceDefinitions.FirstNarrationTilemap,
            IntroCinematicCheckSources.JapaneseFontTwo,
            IntroCinematicCheckSources.MotherBrainLevelData,
            IntroCinematicSourceDefinitions.IntroObjectCharacters,
            IntroCinematicSourceDefinitions.FinalTextLine,
        ];
        foreach (int address in resources)
            AssertTrue(address is >= 0x808000 and <= 0xffffff,
                $"intro resource ${address:X6} is mapped ROM");

        AssertEqual(IntroCinematicDefinitions.Tilemaps.TextTilemapWordCount,
            IntroCinematicDefinitions.Tilemaps.TilemapWidth * IntroCinematicDefinitions.Tilemaps.TilemapWidth,
            "intro text tilemap is 32x32 words");
        AssertTrue(
            (IntroVramLayout.CinematicObjectCharacters.Offset * 2) +
                IntroCinematicDefinitions.Artwork.ObjectCharacterBytes <= SnesVram.ByteCount,
            "intro object DMA fits VRAM");

        AssertIntroPaletteSpans(IntroCinematicDefinitions.Palette.Gameplay, "gameplay");
        AssertIntroPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear, "gameplay clear");
        AssertIntroPaletteSpans(IntroCinematicDefinitions.Palette.Narration, "narration");
        AssertIntroPaletteSpans(IntroCinematicDefinitions.Palette.Discovery, "discovery");
        AssertEqual(SoundEffectLibrary.Library2,
            IntroCinematicDefinitions.Objects.EggHatch.Library,
            "egg hatch sound library");
        AssertEqual(SoundEffectLibrary.Library3,
            IntroCinematicDefinitions.Objects.BabyCry1.Library,
            "Baby cry sound library");
        AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Library3, 0x23), IntroCinematicDefinitions.Objects.BabyCry1,
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
            .Concat(Enum.GetValues<CinematicSpriteInstruction>().Cast<object>().Select(Convert.ToUInt16))
            .Concat(new[] { IntroCinematicSourceDefinitions.PortraitTilemapDrawFunction })
            .Concat(Enum.GetValues<TitleSequenceInstruction>().Select(TitleNativeWords.Instructions.Word))
            .Concat(CinematicSpritePreInstruction.NativeAddresses)
            // The owner instructions the typed cinematic sprite programs (#641) turn into cues.
            .Concat(CinematicSpriteNativeListAddresses.OwnerInstructions.Keys)
            .Where(pointer => pointer != 0)
            .Distinct()
            .ToArray();
        // The native list of every typed cinematic sprite program (#641).
        ushort[] bank8BLists = [.. CinematicSpriteNativeListAddresses.Entries.Select(entry => entry.Entry)];
        // The narration pages' lists are named by the narration script definitions, not by loose constants.
        ushort[] bank8CLists =
        [
            .. GetUshortConstants(typeof(CinematicCodePointers.BackgroundLists))
                .Concat(GetUshortConstants(typeof(CinematicCodePointersIndirectData)))
                .Select(field => field.RequiredConstant<ushort>()),
            .. SuperMetroid.AssetExtraction.Frontend.Intro.IntroNarrationScriptDefinitions.Pages.Select(page => checked((ushort)page.InstructionPointer)),
        ];

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

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach (ushort pointer in bank8BCodePointers.Concat(bank8BLists))
            _ = CinematicCodePointerCatalogRomByte(bus, IntroCinematicCheckSources.CinematicCode | pointer);
        foreach (ushort pointer in bank8CLists)
            _ = CinematicCodePointerCatalogRomByte(bus, (int)new SnesAddress(0x8c, pointer));
    }

    /// <summary>Reads a cartridge byte to validate a named cinematic code or list pointer.</summary>
    private static byte CinematicCodePointerCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that a cinematic palette span is aligned, nonempty, and inside CGRAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void AssertIntroPaletteSpans(
        IReadOnlyList<IntroPaletteSpan> spans,
        string context)
    {
        AssertTrue(spans.Count != 0, $"intro {context} palette span list is nonempty");
        foreach (IntroPaletteSpan span in spans)
        {
            AssertTrue(span.FirstColor >= 0, $"intro {context} span starts inside CGRAM");
            AssertTrue(span.ColorCount > 0, $"intro {context} span is nonempty");
            AssertTrue(span.FirstColor + span.ColorCount <= SnesCgram.ColorCount,
                $"intro {context} span fits CGRAM");
        }
    }
}
