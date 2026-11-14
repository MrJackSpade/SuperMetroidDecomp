using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Enemies.EnemyInstructionCodePointersVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks opening-cinematic ROM resource bounds, palette spans, and actor sound identities.</summary>
internal static class IntroCinematicRomDataVerification
{
    /// <summary>
    /// Verifies that the import's intro sources are mapped, fixed VRAM regions fit, palette spans are
    /// byte-aligned/in-range, and actor sounds retain their library-qualified identities. Since #1416 the
    /// sources are the import's (IntroCinematicSourceDefinitions) and the extents Core's definitions.
    /// </summary>
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

        AssertEqual(IntroCinematicDefinitionsPlanesConstants.TextTilemapWordCount,
            IntroCinematicDefinitions.Planes.Width * IntroCinematicDefinitions.Planes.Width,
            "intro text tilemap is 32x32 words");
        AssertTrue(
            (IntroVramLayout.CinematicObjectCharacters.Offset * 2) +
                IntroCinematicArtworkConstants.ObjectCharacterBytes <= SnesVram.ByteCount,
            "intro object DMA fits VRAM");

        AssertIntroCrossfadeScenes();
        AssertEqual(SoundEffectLibrary.Enemies,
            IntroCinematicDefinitions.Objects.EggHatch.Library,
            "egg hatch sound library");
        AssertEqual(SoundEffectLibrary.Room,
            IntroCinematicDefinitions.Objects.BabyCry1.Library,
            "Baby cry sound library");
        AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Room, 0x23), IntroCinematicDefinitions.Objects.BabyCry1,
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
            .Concat(Enum.GetValues<TitleCardCue>().Select(TitleNativeWords.Instructions.Word))
            .Concat(CinematicSpriteFrameBehavior.NativeAddresses)
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

        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach (ushort pointer in bank8BCodePointers.Concat(bank8BLists))
            _ = CinematicCodePointerCatalogRomByte(bus, IntroCinematicCheckSources.CinematicCode | pointer);
        foreach (ushort pointer in bank8CLists)
            _ = CinematicCodePointerCatalogRomByte(bus, (int)new SnesAddress(0x8c, pointer));
    }

    /// <summary>Reads a cartridge byte to validate a named cinematic code or list pointer.</summary>
    private static byte CinematicCodePointerCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Checks that the import places each crossfaded scene's colours as nonempty runs among the opening's background and
    /// sprite colours, which its crossfade shades recolour, and that no two scenes hold the same colour (#1512, #1597).
    /// </summary>
    private static void AssertIntroCrossfadeScenes()
    {
        const int ownerColours = ImageColourGroup.Count * ImageColourGroup.Colours;
        var backgrounds = new HashSet<int>();
        var sprites = new HashSet<int>();
        foreach (IntroCrossfadeScene scene in Enum.GetValues<IntroCrossfadeScene>())
        {
            int[] background = [.. IntroSheetInks.Background(scene)];
            AssertTrue(background.Length > 0 && background.All(ink => ink is >= 0 and < ownerColours),
                $"intro {scene} background colours are nonempty and inside the opening's colours");
            AssertTrue(background.All(backgrounds.Add), $"intro {scene} holds no background colour another scene holds");
            int[] sprite = [.. IntroSheetInks.Sprite(scene)];
            AssertTrue(sprite.Length > 0 && sprite.All(ink => ink is >= 0 and < ownerColours),
                $"intro {scene} sprite colours are nonempty and inside the opening's colours");
            AssertTrue(sprite.All(sprites.Add), $"intro {scene} holds no sprite colour another scene holds");
        }
    }
}
