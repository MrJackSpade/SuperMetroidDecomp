using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Tooling;
using System.Runtime.CompilerServices;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyInstructionCodePointersVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.NorfairGlowVerification;
using static SuperMetroid.Verification.Fx.Artwork.RoomPaletteFxPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleGradientVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Checks compiled palette-FX instruction vocabularies and program mechanics against retail bank $8D.</summary>
internal static class PaletteFxInstructionCodesVerification
{
    /// <summary>
    /// Keeps the complete translated palette-FX dispatcher vocabulary explicit. This
    /// catches accidental duplicate callbacks and verifies that every executable pointer
    /// remains in mapped bank-$8D cartridge space.
    /// </summary>
    internal static void VerifyPaletteFxInstructionCodeCatalogs()
    {
        AssertPaletteFxCatalog(nameof(PaletteFxInstruction), CatalogWords(PaletteFxInstructionCodesType()), expectedCount: 19);
        // The setups' and pre-instructions' values are arbitrary (#1415): their routine words are the native view's.
        AssertPaletteFxCatalog(nameof(PaletteFxSetup), NativeWords(FxNativeWords.PaletteFxSetups), expectedCount: 4);
        AssertPaletteFxCatalog(nameof(PaletteFxPreInstruction), NativeWords(FxNativeWords.PaletteFxPreInstructions), expectedCount: 9);
        AssertPaletteFxCatalog(nameof(PaletteFxHeatData), CatalogWords(typeof(PaletteFxHeatData)), expectedCount: 2, requireMappedPointers: false);

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Palette FX: 32 instruction, setup and pre-instruction " +
                "pointers are structurally valid; retail ROM reads skipped.");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        ushort[][] catalogs =
        [
            CatalogWords(PaletteFxInstructionCodesType()),
            NativeWords(FxNativeWords.PaletteFxSetups),
            NativeWords(FxNativeWords.PaletteFxPreInstructions),
        ];
        foreach (ushort[] catalog in catalogs)
            ReadPaletteFxCatalogPointerTargets(bus, catalog);

        Suite(nameof(VerifyBeaconSoundInstruction), () => VerifyBeaconSoundInstruction(bus));
        Suite(nameof(VerifyPaletteFxDeleteDefinitions), () => VerifyPaletteFxDeleteDefinitions(bus));
        Suite(nameof(VerifyTitleLogoFadePaletteFxColorDefinitions), () => VerifyTitleLogoFadePaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyNintendoLogoFadePaletteFxColorDefinitions), () => VerifyNintendoLogoFadePaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyCeresCinematicLightPaletteFxColorDefinitions), () => VerifyCeresCinematicLightPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyExplodingZebesFadePaletteFxColorDefinitions), () => VerifyExplodingZebesFadePaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionForegroundPaletteFxColorDefinitions), () => VerifyZebesExplosionForegroundPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionFinalePaletteFxColorDefinitions), () => VerifyZebesExplosionFinalePaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionWhiteoutPaletteFxColorDefinitions), () => VerifyZebesExplosionWhiteoutPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionGunshipPaletteFxColorDefinitions), () => VerifyZebesExplosionGunshipPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyUnusedCinematicFadePaletteFxColorDefinitions), () => VerifyUnusedCinematicFadePaletteFxColorDefinitions(bus));
        Suite(nameof(VerifySamusLoadingSuitPaletteFxColorDefinitions), () => VerifySamusLoadingSuitPaletteFxColorDefinitions());
        Suite(nameof(VerifyPostCreditsIconGlarePaletteFxColorDefinitions), () => VerifyPostCreditsIconGlarePaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyPaletteFxHeatInstructionListDefinitions), () => VerifyPaletteFxHeatInstructionListDefinitions(bus));
        Suite(nameof(VerifyWreckedShipGreenLightPaletteFxColorDefinitions), () => VerifyWreckedShipGreenLightPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyTourianStatueGreyPaletteFxColorDefinitions), () => VerifyTourianStatueGreyPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyRedBrinstarGlowPaletteFxColorDefinitions), () => VerifyRedBrinstarGlowPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyTourianGlowPaletteFxColorDefinitions), () => VerifyTourianGlowPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyBeaconPaletteFxColorDefinitions), () => VerifyBeaconPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyTourianEscapeSharedRedFlashPaletteFxColorDefinitions), () => VerifyTourianEscapeSharedRedFlashPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyOldTourianEscapeRedFlashPaletteFxColorDefinitions), () => VerifyOldTourianEscapeRedFlashPaletteFxColorDefinitions(bus));
        Suite(nameof(VerifyUpperCrateriaEscapeRedFlashPaletteFxColorDefinitions), () => VerifyUpperCrateriaEscapeRedFlashPaletteFxColorDefinitions(bus));
        var guardedHeatBus = NewPaletteFxMechanicsForbiddenBus(bus, out _);
        Suite(nameof(VerifyNorfairHeatPaletteHandshake), () => VerifyNorfairHeatPaletteHandshake(guardedHeatBus));
        AssertEqual(0, guardedHeatBus.ForbiddenReadAttempts,
            "Norfair heat pre-instruction performs no selector-table ROM reads");
        Suite(nameof(VerifyNorfairGlowCycles), () => VerifyNorfairGlowCycles(bus));
        Suite(nameof(VerifyExtractedRoomPaletteFxPresentation), () => VerifyExtractedRoomPaletteFxPresentation(bus));
        Suite(nameof(VerifyTitleGradientTables), () => VerifyTitleGradientTables(bus));

        Console.WriteLine(
            "  Palette FX: all 37 code/list pointers are ROM-readable; 48 heat selectors " +
            "plus 1717 control words and 33 byte operands are compiled; all four audio opcodes " +
            "and retail $F781's byte/cursor handoff agree.");
    }

    /// <summary>Reads each pointer's first byte so an unmapped bank-$8D cartridge target fails loudly.</summary>
    private static void ReadPaletteFxCatalogPointerTargets(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace cartridge, ushort[] pointers)
    {
        foreach (ushort pointer in pointers)
            _ = cartridge.ReadCartridgeByte(0x8d0000 | pointer);
    }

    /// <summary>The ushort constants a catalog type declares, in declaration order.</summary>
    private static ushort[] CatalogWords(Type catalog) =>
        GetUshortConstants(catalog).Select(field => field.RequiredConstant<ushort>()).ToArray();

    /// <summary>The cartridge word naming each member of an identity whose values are arbitrary.</summary>
    private static ushort[] NativeWords<TIdentity>(SuperMetroid.AssetExtraction.Cartridge.NativeWordIdentities<TIdentity> words)
        where TIdentity : struct, Enum =>
        Enum.GetValues<TIdentity>().Select(words.Word).ToArray();

    /// <summary>Checks both standalone delete programs' compiled words and cartridge locations.</summary>
    private static void VerifyPaletteFxDeleteDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            PaletteFxObject.Nothing,
            equippedItems: 0);
        AssertTrue(paletteFx.IsDefinitionActive(
                PaletteFxObject.Nothing),
            "empty room palette-FX definition occupies a slot before its first handler pass");
        paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertTrue(!paletteFx.IsDefinitionActive(
                PaletteFxObject.Nothing),
            "empty room palette-FX definition deletes itself on its first handler pass");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "empty room palette-FX deletion performs no mechanics ROM reads");
        AssertEqual(0, guardedPresentationReads.Value,
            "empty room palette-FX deletion has no presentation payload");
    }


    /// <summary>Checks the boot-logo fade through its copyright goto against bank-$8D words.</summary>
    private static void VerifyNintendoLogoFadePaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        NintendoLogoEntry[] nintendoLogoEntries =
        [
            new NintendoLogoEntry("BootLogo",
                NintendoLogoFadePaletteFxColorDefinitions.BootLogoDefinitionPointer),
            new NintendoLogoEntry("Copyright",
                NintendoLogoFadePaletteFxColorDefinitions.CopyrightDefinitionPointer),
        ];
        AssertEqual(nintendoLogoEntries.Length, NintendoLogoFadePaletteFxColorDefinitionsTooling.All.Count,
            "Nintendo-logo fade exposes one definition per entry");
        foreach ((string owner, ushort definitionPointer) in nintendoLogoEntries)
        {
            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            paletteFx.SpawnDefinition(definitionPointer, 0);
            for (int step = 0;
                 step < NintendoLogoFadePaletteFxColorDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            }
            AssertTrue(paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} fade remains active through its final hold");
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            AssertTrue(!paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} fade deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{owner} fade avoids mechanics ROM reads");
            AssertEqual(
                NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount *
                NintendoLogoFadePaletteFxColorDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guardedPresentationReads.Value,
                $"{owner} fade retains every live color");
        }
    }

    /// <summary>Checks the title-logo fade program through its delete instruction against cartridge words.</summary>
    private static void VerifyTitleLogoFadePaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            TitleLogoFadePaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step < TitleLogoFadePaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(paletteFx.IsDefinitionActive(
                TitleLogoFadePaletteFxColorDefinitions.DefinitionPointer),
            "title-logo fade remains active through its final hold");
        paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertTrue(!paletteFx.IsDefinitionActive(
                TitleLogoFadePaletteFxColorDefinitions.DefinitionPointer),
            "title-logo fade deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "title-logo fade avoids mechanics ROM reads");
        AssertEqual(
            TitleLogoFadePaletteFxColorDefinitionsConstants.FrameCount *
            TitleLogoFadePaletteFxColorDefinitions.ColorsPerFrame * sizeof(ushort),
            guardedPresentationReads.Value,
            "title-logo fade retains every live color");
    }

    /// <summary>Checks the post-credits glare's color setup, definition entry, and frame sequence against native data.</summary>
    internal static void VerifyPostCreditsIconGlarePaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        ushort Word(int pointer) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8d0000 | pointer);
        AssertEqual((ushort)0xc655, Word(0xdf94), "native glare set-color opcode");
        AssertEqual((ushort)0x01e0, Word(0xdf96), "native glare destination operand");
        AssertEqual(PostCreditsIconGlarePaletteFxColorDefinitionsConstants.ProgramStart, Word(0xe202), "native glare definition entry");
        AssertEqual(FxNativeWords.PaletteFxSetups.Word(PaletteFxSetup.Null), Word(0xe200), "native glare setup callback");
        foreach (int invalid in new[] { int.MinValue, -1, 14, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => PostCreditsIconGlarePaletteFxColorDefinitions.FramePointer(invalid), "glare frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => PostCreditsIconGlarePaletteFxColorDefinitions.ColorPointer(invalid, 0), "glare color frame bounds");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PostCreditsIconGlarePaletteFxColorDefinitions.ColorPointer(0, invalid), "glare color column bounds");
        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            PostCreditsIconGlarePaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step < PostCreditsIconGlarePaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(paletteFx.IsDefinitionActive(
                PostCreditsIconGlarePaletteFxColorDefinitions.DefinitionPointer),
            "post-credits icon glare remains active through its final hold");
        paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertTrue(!paletteFx.IsDefinitionActive(
                PostCreditsIconGlarePaletteFxColorDefinitions.DefinitionPointer),
            "post-credits icon glare deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "post-credits icon glare avoids mechanics ROM reads");
        AssertEqual(
            PostCreditsIconGlarePaletteFxColorDefinitions.FrameCount *
            PostCreditsIconGlarePaletteFxColorDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guardedPresentationReads.Value,
            "post-credits icon glare retains every live color");
    }

    /// <summary>Checks Power, Varia, and Gravity loading-suit programs in roster order and against native mechanics.</summary>
    internal static void VerifySamusLoadingSuitPaletteFxColorDefinitions()
    {
        var programs = SamusLoadingSuitPaletteFxColorDefinitions.All;
        AssertEqual(3, programs.Count, "Loading roster count");
        AssertTrue(programs.Select(program => program.Owner).SequenceEqual(new[]
        {
            SamusLoadingSuitPaletteFxProgramOwner.PowerSuit,
            SamusLoadingSuitPaletteFxProgramOwner.VariaSuit,
            SamusLoadingSuitPaletteFxProgramOwner.GravitySuit,
        }), "Original loading roster order");
        for (int index = 0; index < 3; index++)
            AssertTrue(ReferenceEquals(programs[index], programs.ElementAt(index)), "Indexed and enumerated named owner agree");
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = programs[invalid], "Loading roster retains index bounds");
    }

    /// <summary>Checks the unused cinematic fade's compiled program through its delete instruction.</summary>
    private static void VerifyUnusedCinematicFadePaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            UnusedCinematicFadePaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= UnusedCinematicFadePaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                UnusedCinematicFadePaletteFxColorDefinitions.DefinitionPointer),
            "unused cinematic fade deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "unused cinematic fade avoids mechanics ROM reads");
        AssertEqual(
            UnusedCinematicFadePaletteFxColorDefinitionsConstants.FrameCount *
            UnusedCinematicFadePaletteFxColorDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guardedPresentationReads.Value,
            "unused cinematic fade retains every live color");
    }

    /// <summary>Checks the Zebes explosion gunship reveal's color setup, definition entry, and frame sequence.</summary>
    internal static void VerifyZebesExplosionGunshipPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        ushort Word(int pointer) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8d0000 | pointer);
        AssertEqual((ushort)0xc655, Word(0xd6ba), "native gunship reveal set-color opcode");
        AssertEqual((ushort)0x00a0, Word(0xd6bc), "native gunship reveal destination operand");
        AssertEqual(ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.ProgramStart, Word(0xe1e6), "native gunship reveal definition entry");
        AssertEqual(FxNativeWords.PaletteFxSetups.Word(PaletteFxSetup.Null), Word(0xe1e4), "native gunship reveal setup callback");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionGunshipPaletteFxColorDefinitions.FramePointer(invalid), "gunship reveal frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionGunshipPaletteFxColorDefinitions.ColorPointer(invalid, 0), "gunship reveal color frame bounds");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionGunshipPaletteFxColorDefinitions.ColorPointer(0, invalid), "gunship reveal color column bounds");
        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            ZebesExplosionGunshipPaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step < ZebesExplosionGunshipPaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(paletteFx.IsDefinitionActive(
                ZebesExplosionGunshipPaletteFxColorDefinitions.DefinitionPointer),
            "ending gunship reveal remains active through its final hold");
        paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertTrue(!paletteFx.IsDefinitionActive(
                ZebesExplosionGunshipPaletteFxColorDefinitions.DefinitionPointer),
            "ending gunship reveal deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "ending gunship reveal avoids mechanics ROM reads");
        AssertEqual(
            ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount *
            ZebesExplosionGunshipPaletteFxColorDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guardedPresentationReads.Value,
            "ending gunship reveal retains every live color");
    }



    /// <summary>Checks the wide explosion whiteout from its background entry through final delete.</summary>
    private static void VerifyZebesExplosionWhiteoutPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        NintendoLogoEntry[] whiteoutEntries =
        [
            new NintendoLogoEntry("WideExplosionBackground",
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.WideExplosionBackgroundDefinitionPointer),
            new NintendoLogoEntry("SpaceWhiteout",
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.SpaceWhiteoutDefinitionPointer),
        ];
        AssertEqual(whiteoutEntries.Length, ZebesExplosionWhiteoutPaletteFxColorDefinitionsTooling.All.Count,
            "Zebes explosion whiteout exposes one definition per entry");
        foreach ((string owner, ushort definitionPointer) in whiteoutEntries)
        {
            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            paletteFx.SpawnDefinition(definitionPointer, 0);
            for (int step = 0;
                 step <= ZebesExplosionWhiteoutPaletteFxColorDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            }
            AssertTrue(!paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} whiteout deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{owner} whiteout avoids mechanics ROM reads");
            AssertEqual(
                ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.FrameCount *
                ZebesExplosionWhiteoutPaletteFxColorDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guardedPresentationReads.Value,
                $"{owner} whiteout retains every live color");
        }
    }

    /// <summary>Checks the Zebes explosion finale's compiled mechanics through its delete opcode.</summary>
    private static void VerifyZebesExplosionFinalePaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            ZebesExplosionFinalePaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= ZebesExplosionFinalePaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                ZebesExplosionFinalePaletteFxColorDefinitions.DefinitionPointer),
            "Zebes explosion finale deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Zebes explosion finale avoids mechanics ROM reads");
        AssertEqual(
            ZebesExplosionFinalePaletteFxColorDefinitionsConstants.FrameCount *
            ZebesExplosionFinalePaletteFxColorDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guardedPresentationReads.Value,
            "Zebes explosion finale retains every live color");
    }

    /// <summary>Checks the foreground explosion palette program through its native delete instruction.</summary>
    internal static void VerifyZebesExplosionForegroundPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            ZebesExplosionForegroundPaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                ZebesExplosionForegroundPaletteFxColorDefinitions.DefinitionPointer),
            "Zebes explosion foreground deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Zebes explosion foreground avoids mechanics ROM reads");
        AssertEqual(
            ZebesExplosionForegroundPaletteFxColorDefinitionsConstants.FrameCount *
            ZebesExplosionForegroundPaletteFxColorDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guardedPresentationReads.Value,
            "Zebes explosion foreground retains every live color");
    }

    /// <summary>Checks the exploding-Zebes fade program's compiled bytes and terminal delete against the cartridge.</summary>
    private static void VerifyExplodingZebesFadePaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            ExplodingZebesFadePaletteFxColorDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= ExplodingZebesFadePaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                ExplodingZebesFadePaletteFxColorDefinitions.DefinitionPointer),
            "exploding-Zebes fade deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "exploding-Zebes fade avoids mechanics ROM reads");
        AssertEqual(
            ExplodingZebesFadePaletteFxColorDefinitionsConstants.FrameCount *
            ExplodingZebesFadePaletteFxColorDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guardedPresentationReads.Value,
            "exploding-Zebes fade retains every live color");
    }



    /// <summary>Checks Ceres engine and navigation-light palette records across their native bank-$8D span.</summary>
    private static void VerifyCeresCinematicLightPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        CeresLightEntry[] ceresLightEntries =
        [
            new CeresLightEntry(CeresCinematicLightPaletteFxProgramOwner.GunshipEngine,
                CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineDefinitionPointer),
            new CeresLightEntry(CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights,
                CeresCinematicLightPaletteFxColorDefinitions.SpriteNavigationLightsDefinitionPointer),
            new CeresLightEntry(CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights,
                CeresCinematicLightPaletteFxColorDefinitions.BackgroundNavigationLightsDefinitionPointer),
        ];
        AssertEqual(ceresLightEntries.Length, CeresCinematicLightPaletteFxColorDefinitionsTooling.All.Count,
            "Ceres cinematic lights expose one definition per entry");
        foreach ((CeresCinematicLightPaletteFxProgramOwner owner, ushort definitionPointer) in ceresLightEntries)
        {
            bool isGunship = owner ==
                CeresCinematicLightPaletteFxProgramOwner.GunshipEngine;
            int cycleFrames = isGunship
                ? CeresCinematicLightPaletteFxColorDefinitions
                    .GunshipEngineCycleFrames
                : CeresCinematicLightPaletteFxColorDefinitions
                    .NavigationLightsCycleFrames;
            int frameCount = isGunship
                ? CeresCinematicLightPaletteFxColorDefinitionsConstants
                    .GunshipEngineFrameCount
                : CeresCinematicLightPaletteFxColorDefinitionsConstants
                    .NavigationLightsFrameCount;
            int colorsPerFrame = isGunship
                ? CeresCinematicLightPaletteFxColorDefinitions
                    .GunshipEngineColorsPerFrame
                : CeresCinematicLightPaletteFxColorDefinitions
                    .NavigationLightsColorsPerFrame;

            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            paletteFx.SpawnDefinition(definitionPointer, 0);
            for (int step = 0; step <= cycleFrames; step++)
                paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            AssertTrue(paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} completes and repeats its cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{owner} avoids mechanics ROM reads");
            AssertEqual((frameCount + 1) * colorsPerFrame * sizeof(ushort),
                guardedPresentationReads.Value,
                $"{owner} retains every live color");
        }
    }


    /// <summary>Checks the upper-Crateria red flash against both the room catalog and native loop words.</summary>
    private static void VerifyUpperCrateriaEscapeRedFlashPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.DefinitionPointer, 0);
        for (int step = 0; step <= UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.CycleFrames; step++)
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertTrue(paletteFx.IsDefinitionActive(UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.DefinitionPointer),
            "upper-Crateria escape flash completes and repeats its cycle");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "upper-Crateria escape flash avoids mechanics ROM reads");
        AssertEqual((UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount + 1) *
            UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame * sizeof(ushort),
            guardedPresentationReads.Value, "upper-Crateria escape flash retains every live color");
    }


    /// <summary>Checks the old-Tourian red flash's compiled mechanics against its native loop span.</summary>
    private static void VerifyOldTourianEscapeRedFlashPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        paletteFx.SpawnDefinition(
            OldTourianEscapeRedFlashPaletteFxColorDefinitions.DefinitionPointer,
            equippedItems: 0);
        for (int step = 0;
             step <= OldTourianEscapeRedFlashPaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }

        AssertTrue(paletteFx.IsDefinitionActive(
                OldTourianEscapeRedFlashPaletteFxColorDefinitions.DefinitionPointer),
            "old-Tourian red flash completes and repeats its full cycle");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "old-Tourian red flash avoids mechanics ROM reads");
        AssertEqual(
            (OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount + 1) *
                OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame *
                sizeof(ushort),
            guardedPresentationReads.Value,
            "old-Tourian red flash retains every live color through its cycle");
    }

    /// <summary>Checks the Tourian shared red flash from its general-level entry through the loop operand.</summary>
    private static void VerifyTourianEscapeSharedRedFlashPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        foreach (TourianEscapeSharedRedFlashPaletteFxColorDefinition definition in
                 TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions.All)
        {
            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            paletteFx.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0;
                 step <= TourianEscapeSharedRedFlashPaletteFxColorDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} completes and repeats the shared cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                (TourianEscapeSharedRedFlashPaletteFxColorDefinitionsConstants.FrameCount + 1) *
                    TourianEscapeSharedRedFlashPaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guardedPresentationReads.Value,
                $"{definition.Owner} retains every live color through the shared cycle");
        }
    }



    /// <summary>Checks the beacon palette cycle from setup through its loop operand.</summary>
    private static void VerifyBeaconPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        var cgram = new SamusColourRig();
        paletteFx.SpawnDefinition(
            BeaconPaletteFxColorDefinitions.DefinitionPointer,
            equippedItems: 0);
        int soundCount = 0;
        for (int step = 0;
             step <= BeaconPaletteFxColorDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(cgram, new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            if (paletteFx.SoundRequests.Count == 0)
                continue;
            AssertEqual(1, paletteFx.SoundRequests.Count,
                "beacon publishes one sound at its midpoint");
            AssertEqual(SoundEffectLibrary.Library2,
                paletteFx.SoundRequests[0].SoundEffect.Library,
                "beacon selects sound library two");
            AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Library2, BeaconPaletteFxColorDefinitionsConstants.SoundId),
                paletteFx.SoundRequests[0].SoundEffect,
                "beacon retains its compiled sound ID");
            soundCount++;
        }

        AssertEqual(1, soundCount, "beacon queues its sound once per complete cycle");
        AssertTrue(paletteFx.IsDefinitionActive(
                BeaconPaletteFxColorDefinitions.DefinitionPointer),
            "beacon remains active after its complete cycle");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "beacon avoids mechanics ROM reads");
        AssertEqual(
            (BeaconPaletteFxColorDefinitionsConstants.FrameCount + 1) *
                BeaconPaletteFxColorDefinitions.ColorsPerFrame * 2,
            guardedPresentationReads.Value,
            "beacon retains every live color read through its complete cycle");
        AssertEqual(0x02bf, cgram.Colors[0x71],
            "beacon writes its first live color");
        AssertEqual(0, cgram.Colors[0x74],
            "beacon preserves the first skipped CGRAM color");
        AssertEqual(0, cgram.Colors[0x7c],
            "beacon preserves the ninth skipped CGRAM color");
        AssertEqual(0x7fff, cgram.Colors[0x7d],
            "beacon resumes after the native eighteen-byte skip");
    }

    /// <summary>Checks the Tourian glow clone and loop mechanics at their word-aligned native addresses.</summary>
    private static void VerifyTourianGlowPaletteFxColorDefinitions(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {

        foreach (ushort definition in new ushort[]
                 {
                     TourianGlowPaletteFxColorDefinitions.LiveDefinitionPointer,
                     TourianGlowPaletteFxColorDefinitions.CloneDefinitionPointer,
                 })
        {
            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            var cgram = new SamusColourRig();
            paletteFx.SpawnDefinition(definition, equippedItems: 0);
            for (int step = 0;
                 step <= TourianGlowPaletteFxColorDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(cgram, new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition),
                $"Tourian glow definition $8D:{definition:X4} completes its loop");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"Tourian glow definition $8D:{definition:X4} avoids mechanics ROM reads");
            AssertEqual(
                (TourianGlowPaletteFxColorDefinitionsConstants.FrameCount + 1) *
                    TourianGlowPaletteFxColorDefinitions.ColorsPerFrame * 2,
                guardedPresentationReads.Value,
                $"Tourian glow definition $8D:{definition:X4} retains all live colors");
            AssertEqual(0x5294, cgram.Colors[0x74],
                "Tourian glow writes its isolated first color");
            AssertEqual(0, cgram.Colors[0x75],
                "Tourian glow preserves first skipped CGRAM color");
            AssertEqual(0, cgram.Colors[0x76],
                "Tourian glow preserves second skipped CGRAM color");
            AssertEqual(0, cgram.Colors[0x77],
                "Tourian glow preserves third skipped CGRAM color");
            AssertEqual(0x0019, cgram.Colors[0x78],
                "Tourian glow resumes after the native six-byte skip");
        }

        var deletionFxSession = new PaletteFxSessionState();
        var deletionFx = new RoomPaletteFxSystem(deletionFxSession);
        deletionFx.SpawnDefinition(TourianGlowPaletteFxColorDefinitions.LiveDefinitionPointer, 0);
        deletionFx.SpawnDefinition(0xf795, 0);
        deletionFx.SpawnDefinition(0xf799, 0);
        deletionFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(bus), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        deletionFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(bus), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertTrue(!deletionFx.IsDefinitionActive(
                TourianGlowPaletteFxColorDefinitions.LiveDefinitionPointer),
            "Tourian glow pre-instruction deletes its owner when two later slots exist");
    }



    /// <summary>Checks Red Brinstar glow color-index setup and loop mechanics against their retail words.</summary>
    private static void VerifyRedBrinstarGlowPaletteFxColorDefinitions(
        ISnesAddressSpace bus)
    {
        var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        var cgram = new SamusColourRig();
        const ushort definition =
            RedBrinstarGlowPaletteFxColorDefinitionsConstants.DefinitionPointer;
        paletteFx.SpawnDefinition(definition, equippedItems: 0);
        int cycleFrames =
            RedBrinstarGlowPaletteFxColorDefinitionsConstants.FrameCount * 10;
        for (int step = 0; step <= cycleFrames; step++)
        {
            paletteFx.Step(cgram, new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        }

        AssertTrue(paletteFx.IsDefinitionActive(definition),
            "Red Brinstar glow program loops after fourteen frames");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Red Brinstar glow program avoids mechanics ROM reads");
        AssertEqual(
            (RedBrinstarGlowPaletteFxColorDefinitionsConstants.FrameCount + 1) *
                RedBrinstarGlowPaletteFxColorDefinitions.ColorsPerFrame *
                sizeof(ushort),
            guardedPresentationReads.Value,
            "Red Brinstar glow program retains live colors through loop");
    }



    /// <summary>Checks statue greying programs with both goto and delete endings against native words.</summary>
    private static void VerifyTourianStatueGreyPaletteFxColorDefinitions(
        ISnesAddressSpace bus)
    {
        foreach (TourianStatueGreyPaletteFxColorDefinition definition in
                 TourianStatueGreyPaletteFxColorToolingDefinitions.All)
        {
            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            paletteFx.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0;
                 step < TourianStatueGreyPaletteFxColorDefinitions.FramesThroughDeletion;
                 step++)
            {
                paletteFx.Step(new SamusColourRig(), new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            }

            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"palette-FX definition $8D:{definition.DefinitionPointer:X4} reaches " +
                "compiled deletion");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"palette-FX definition $8D:{definition.DefinitionPointer:X4} avoids statue " +
                "mechanics ROM reads");
            AssertEqual(
                TourianStatueGreyPaletteFxColorDefinitions.FrameCount *
                    TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guardedPresentationReads.Value,
                $"palette-FX definition $8D:{definition.DefinitionPointer:X4} retains statue " +
                "color reads");
        }
    }

    /// <summary>Checks the Wrecked Ship green light's color index and repeating palette mechanics.</summary>
    private static void VerifyWreckedShipGreenLightPaletteFxColorDefinitions(
        ISnesAddressSpace bus)
    {
        foreach (ushort definition in new ushort[] { 0xf76d, 0xf771 })
        {
            var guarded = NewPaletteFxMechanicsForbiddenBus(bus, out StrongBox<int> guardedPresentationReads);
            var paletteFxSession = new PaletteFxSessionState();
            var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
            var cgram = new SamusColourRig();
            paletteFx.SpawnDefinition(definition, equippedItems: 0);
            int steps = 1 +
                WreckedShipGreenLightPaletteFxColorDefinitionsConstants.FrameCount * 10;
            for (int step = 0; step <= steps; step++)
            {
                paletteFx.Step(cgram, new ReferencePaletteFxColorSource(guarded), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"palette-FX definition $8D:{definition:X4} avoids green-light mechanics ROM reads");
            AssertEqual(
                (WreckedShipGreenLightPaletteFxColorDefinitionsConstants.FrameCount + 1) *
                    WreckedShipGreenLightPaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guardedPresentationReads.Value,
                $"palette-FX definition $8D:{definition:X4} retains green-light color reads");
        }
    }


    /// <summary>Checks each suit's heat source table from the native LDA operand and its compiled instruction list.</summary>
    internal static void VerifyPaletteFxHeatInstructionListDefinitions(
        ISnesAddressSpace bus)
    {
        // Native LDA operands select each original table, independently of production
        // source-address helpers and the replacement program-stride calculation.
        var sources = new PaletteFxInstructionCodesSource[]
        {
            new PaletteFxInstructionCodesSource(PaletteFxHeatSuit.Power, 0x8de3d6),
            new PaletteFxInstructionCodesSource(PaletteFxHeatSuit.Varia, 0x8de3d1),
            new PaletteFxInstructionCodesSource(PaletteFxHeatSuit.Gravity, 0x8de3c7),
        };
        foreach (var (suit, operand) in sources)
        {
            int table = 0x8d0000 | ReadVerificationWord(bus, operand);
            for (ushort phase = 0; phase < 16; phase++)
            {
                ushort expected = ReadVerificationWord(bus, table + 2 * phase);
                AssertEqual(expected, PaletteFxHeatInstructionListDefinitions.Resolve(suit, phase),
                    "Every original heat selector word");
                AssertEqual((ushort)(table + 2 * phase),
                    PaletteFxHeatInstructionListDefinitions.NativeSourceAddress(suit, phase),
                    "Verification source alias preserves original pointer location");
                AssertEqual((ushort)PaletteFxInstruction.Wait, ReadVerificationWord(bus, 0x8d0000 | (expected + 32)),
                    "Native frame contains duration, fifteen color words, then Done");
            }
            foreach (ushort phase in new ushort[] { 16, 17, ushort.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(
                    () => PaletteFxHeatInstructionListDefinitions.Resolve(suit, phase), "Reject invalid heat phase");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => PaletteFxHeatInstructionListDefinitions.Resolve((PaletteFxHeatSuit)invalid, 0), "Reject unknown suit");
        ushort gravityMask = ReadVerificationWord(bus, 0x8de3c2);
        ushort variaMask = ReadVerificationWord(bus, 0x8de3cc);
        // Every equipment word Samus's items can record: each word whose set bits are all item bits.
        ushort itemBits = Enum.GetValues<SamusEquipmentFlags>().Aggregate((all, item) => all | item).ToEquipmentWord();
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            if ((word & ~itemBits) != 0)
                continue;
            ushort equipment = (ushort)word;
            int operand = (equipment & gravityMask) != 0 ? 0x8de3c7 :
                (equipment & variaMask) != 0 ? 0x8de3d1 : 0x8de3d6;
            int table = 0x8d0000 | ReadVerificationWord(bus, operand);
            for (ushort phase = 0; phase < 16; phase++)
                AssertEqual(ReadVerificationWord(bus, table + 2 * phase),
                    PaletteFxHeatInstructionListDefinitionsConstants.ResolveForEquippedItems(SamusEquipmentWordCodec.FromEquipmentWord(equipment), phase),
                    "Every recordable equipment word preserves native Gravity-before-Varia selection at every phase");
        }
    }

    /// <summary>
    /// Reproduces issue #253 with the two retail definitions active in native allocation
    /// order. `$F785` must publish its byte-sized heat index without losing cursor
    /// alignment; `$F761` must then consume that index, add quarter-energy subdamage, and
    /// queue the native environmental-damage sound on an eight-frame boundary.
    /// </summary>
    private static void VerifyNorfairHeatPaletteHandshake(ISnesAddressSpace bus)
    {
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        var cgram = new SamusColourRig();
        var samus = new SamusState { Health = 99 };
        paletteFx.SpawnDefinition(definition: 0xf761, equippedItems: 0);
        paletteFx.SpawnDefinition(definition: 0xf785, equippedItems: 0);

        int damageSoundCount = 0;
        for (ushort frame = 0; frame <= 17; frame++)
        {
            paletteFx.Step(cgram, new ReferencePaletteFxColorSource(bus), new PaletteFxConditions { SamusY = samus.Position.Y.Whole, EquippedItems = samus.EquippedItems, EnemyZeroIsDead = false, AreaMiniBossDefeated = false, Samus = samus.RoomView, FrameCounter = frame });
            damageSoundCount += paletteFx.SoundRequests.Count;
            foreach (SoundRequest request in paletteFx.SoundRequests)
            {
                AssertEqual(Library3Sound.IncrementalEnergy,
                    request.SoundEffect,
                    "retail Norfair heat owner publishes native library-three sound");
                AssertEqual(PaletteFxHeatData.DamageSoundMaximumQueued,
                    request.MaximumQueued,
                    "retail Norfair heat owner uses native Max6 queue");
            }
            if (frame == 16)
            {
                AssertEqual(1, paletteFxSession.SamusInHeatPaletteIndex,
                    "$F785 publishes its second heat phase on its own slot pass");
                AssertEqual(0, paletteFxSession.PreviousSamusInHeatPaletteIndex,
                    "shared heat owner has not consumed the later slot's phase in the same frame");
            }
        }

        AssertEqual(0x0004, samus.LiquidPhysics.PeriodicDamage,
            "retail Norfair heat owner carries quarter-units into whole damage");
        AssertEqual(0x4000, samus.LiquidPhysics.PeriodicSubDamage,
            "retail Norfair heat owner retains exact 16.16 fractional carry");
        AssertEqual(1, paletteFxSession.PreviousSamusInHeatPaletteIndex,
            "shared heat owner consumes $F785 phase on following frame");
        AssertEqual(2, damageSoundCount,
            "retail Norfair heat owner publishes on frame-eight boundaries");
    }

    /// <summary>Rejects runtime palette-FX mechanics reads while counting permitted live presentation-table reads.</summary>
    internal static CartridgeReadGuard NewPaletteFxMechanicsForbiddenBus(
        ISnesAddressSpace inner, out StrongBox<int> presentationReads)
    {
        var reads = new StrongBox<int>();
        presentationReads = reads;
        return new CartridgeReadGuard(inner)
            .Forbid(address => ClassifyPaletteFxRead(address).Forbidden is not null,
                address => (ClassifyPaletteFxRead(address).Forbidden ?? throw new InvalidOperationException("Verification requires ClassifyPaletteFxRead(address).Forbidden.")))
            .Observe(_ => true, address => reads.Value += ClassifyPaletteFxRead(address).PresentationReads);
    }

    /// <summary>
    /// Classifies one palette-FX bus read: the failure message for a compiled mechanic or an
    /// unclassified bank-$8D byte, and how many live presentation color tables the byte belongs to.
    /// </summary>
    private static ClassifyPaletteFxReadResult ClassifyPaletteFxRead(int address)
    {
        int presentationReads = 0;
        int presentationReadsBefore = presentationReads;
        SnesAddress source = SnesAddress.FromBusAddress(address);

        if (source.Bank == 0x8d)
        {
            foreach (TitleScreenAmbientPaletteFxColorDefinition definition in
                     TitleScreenAmbientPaletteFxColorDefinitions.All)
            {
                for (int frame = 0; frame < definition.FrameCount; frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < NintendoLogoFadePaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    NintendoLogoFadePaletteFxColorDefinitions.FramePointer(frame) +
                    sizeof(ushort)));
                if ((uint)offset <
                    NintendoLogoFadePaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < TitleLogoFadePaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    TitleLogoFadePaletteFxColorDefinitions.FramePointer(frame) +
                    sizeof(ushort)));
                if ((uint)offset <
                    TitleLogoFadePaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < CeresCinematicLightPaletteFxColorDefinitionsConstants
                     .GunshipEngineFrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    CeresCinematicLightPaletteFxColorDefinitions
                        .GunshipEngineFramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    CeresCinematicLightPaletteFxColorDefinitions
                        .GunshipEngineColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < CeresCinematicLightPaletteFxColorDefinitionsConstants
                     .NavigationLightsFrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    CeresCinematicLightPaletteFxColorDefinitions
                        .NavigationLightsFramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    CeresCinematicLightPaletteFxColorDefinitions
                        .NavigationLightsColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (PlanetZebesTextPaletteFxColorDefinition definition in
                     PlanetZebesTextPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < PlanetZebesTextPaletteFxColorDefinitions.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            foreach (CinematicGlowPaletteFxColorDefinition definition in
                     CinematicGlowPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < CinematicGlowPaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < ExplodingZebesFadePaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    ExplodingZebesFadePaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    ExplodingZebesFadePaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < ZebesExplosionForegroundPaletteFxColorDefinitionsConstants
                     .FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    ZebesExplosionForegroundPaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    ZebesExplosionForegroundPaletteFxColorDefinitions
                        .ColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < ZebesExplosionFinalePaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    ZebesExplosionFinalePaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    ZebesExplosionFinalePaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants
                     .FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    ZebesExplosionWhiteoutPaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    ZebesExplosionWhiteoutPaletteFxColorDefinitions
                        .ColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (ZebesExplosionAmbientPaletteFxColorDefinition definition in
                     ZebesExplosionAmbientPaletteFxColorDefinitions.All)
            {
                for (int frame = 0; frame < definition.FrameCount; frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            foreach (ZebesExplosionLayerFadePaletteFxColorDefinition definition in
                     ZebesExplosionLayerFadePaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < ZebesExplosionLayerFadePaletteFxColorDefinitionsConstants
                         .FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        ZebesExplosionLayerFadePaletteFxColorDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < ZebesExplosionGunshipPaletteFxColorDefinitions
                     .FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    ZebesExplosionGunshipPaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    ZebesExplosionGunshipPaletteFxColorDefinitions
                        .ColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < UnusedCinematicFadePaletteFxColorDefinitionsConstants
                     .FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    UnusedCinematicFadePaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    UnusedCinematicFadePaletteFxColorDefinitions
                        .ColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (SamusLoadingSuitPaletteFxColorDefinition definition in
                     SamusLoadingSuitPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < SamusLoadingSuitPaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        SamusLoadingSuitPaletteFxColorDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < PostCreditsIconGlarePaletteFxColorDefinitions
                     .FrameCount;
                 frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    PostCreditsIconGlarePaletteFxColorDefinitions
                        .FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset <
                    PostCreditsIconGlarePaletteFxColorDefinitions
                        .ColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (PaletteFxHeatProgramDefinition definition in
                     PaletteFxHeatProgramMechanicsDefinitions.All)
            {
                foreach (var frame in definition.Frames)
                {
                    int colorOffset = source.Offset - frame.FirstColorPointer;
                    if ((uint)colorOffset <
                        PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < WreckedShipGreenLightPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    WreckedShipGreenLightPaletteFxColorDefinitions.FramePointer(frame) +
                    sizeof(ushort)));
                int colorOffset = source.Offset - firstColor;
                if ((uint)colorOffset <
                    WreckedShipGreenLightPaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < TourianStatueGreyPaletteFxColorDefinitions.FrameCount;
                 frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    TourianStatueGreyPaletteFxColorDefinitions.FramePointer(frame) +
                    sizeof(ushort)));
                int colorOffset = source.Offset - firstColor;
                if ((uint)colorOffset <
                    TourianStatueGreyPaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (TorizoBellyPaletteFxColorDefinition definition in
                     TorizoBellyPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < TorizoBellyPaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    ushort firstColor = definition.ColorPointer(frame, 0);
                    int colorOffset = source.Offset - firstColor;
                    if ((uint)colorOffset <
                        TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            foreach (BrinstarBlueSporePaletteFxColorDefinition definition in
                     BrinstarBlueSporePaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < BrinstarBlueSporePaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    ushort firstColor = definition.ColorPointer(frame, 0);
                    int colorOffset = source.Offset - firstColor;
                    if ((uint)colorOffset <
                        BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < RedBrinstarGlowPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                ushort firstColor =
                    RedBrinstarGlowPaletteFxColorDefinitions.ColorPointer(
                        frame, 0);
                int colorOffset = source.Offset - firstColor;
                if ((uint)colorOffset <
                    RedBrinstarGlowPaletteFxColorDefinitions.ColorsPerFrame *
                    sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (CrateriaLightningPaletteFxColorDefinition definition in
                     CrateriaLightningPaletteFxColorDefinitions.All)
            {
                foreach (var frame in definition.Frames)
                {
                    int colorOffset = source.Offset - frame.FirstColorPointer;
                    if ((uint)colorOffset < frame.ColorCount * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            foreach (MaridiaEnvironmentalPaletteFxColorDefinition definition in
                     MaridiaEnvironmentalPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < definition.FrameCount;
                     frame++)
                {
                    ushort firstColor = unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    int colorOffset = source.Offset - firstColor;
                    if ((uint)colorOffset <
                        definition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < TourianGlowPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                bool presentationByte = false;
                for (int color = 0;
                     color < TourianGlowPaletteFxColorDefinitions.ColorsPerFrame;
                     color++)
                {
                    int colorOffset = source.Offset -
                        TourianGlowPaletteFxColorDefinitions.ColorPointer(
                            frame, color);
                    if ((uint)colorOffset >= sizeof(ushort))
                        continue;
                    presentationByte = true;
                    presentationReads++;
                    break;
                }
                if (presentationByte)
                    break;
            }

            for (int frame = 0;
                 frame < BeaconPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                ushort framePointer =
                    BeaconPaletteFxColorDefinitions.FramePointer(frame);
                int leadingColorOffset = source.Offset - unchecked((ushort)(framePointer + 2));
                if ((uint)leadingColorOffset < 3 * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
                int finalColorOffset = source.Offset - unchecked((ushort)(framePointer + 10));
                if ((uint)finalColorOffset < sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (NorfairEnvironmentalPaletteFxColorDefinition definition in
                     NorfairEnvironmentalPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < NorfairEnvironmentalPaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    bool presentationByte = false;
                    for (int color = 0;
                         color < NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame;
                         color++)
                    {
                        int colorOffset = source.Offset - definition.ColorPointer(frame, color);
                        if ((uint)colorOffset >= sizeof(ushort))
                            continue;
                        presentationByte = true;
                        presentationReads++;
                        break;
                    }
                    if (presentationByte)
                        break;
                }
            }

            foreach (TourianEscapeRedFlashPaletteFxColorDefinition definition in
                     TourianEscapeRedFlashPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < TourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0;
                 frame < TourianEscapeSharedRedFlashPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                ushort framePointer =
                    TourianEscapeSharedRedFlashPaletteFxColorDefinitions.FramePointer(frame);
                int leadingOffset = source.Offset - unchecked((ushort)(
                    framePointer + sizeof(ushort)));
                if ((uint)leadingOffset < 6 * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
                int trailingOffset = source.Offset - unchecked((ushort)(
                    framePointer + 16));
                if ((uint)trailingOffset < sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            for (int frame = 0;
                 frame < OldTourianEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount;
                 frame++)
            {
                ushort framePointer =
                    OldTourianEscapeRedFlashPaletteFxColorDefinitions.FramePointer(frame);
                int firstOffset = source.Offset - unchecked((ushort)(framePointer + 2));
                int secondOffset = source.Offset - unchecked((ushort)(framePointer + 10));
                int thirdOffset = source.Offset - unchecked((ushort)(framePointer + 20));
                if ((uint)firstOffset < 3 * sizeof(ushort) ||
                    (uint)secondOffset < 4 * sizeof(ushort) ||
                    (uint)thirdOffset < sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (OldTourianEscapeAccentPaletteFxColorDefinition definition in
                     OldTourianEscapeAccentPaletteFxColorDefinitions.All)
            {
                for (int frame = 0;
                     frame < OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        OldTourianEscapeAccentPaletteFxColorDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            for (int frame = 0; frame < UpperCrateriaEscapeRedFlashPaletteFxColorDefinitionsConstants.FrameCount; frame++)
            {
                int offset = source.Offset - unchecked((ushort)(
                    UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FramePointer(frame) + sizeof(ushort)));
                if ((uint)offset < UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.ColorsPerFrame * sizeof(ushort))
                {
                    presentationReads++;
                    break;
                }
            }

            foreach (CrateriaEscapeLightningPaletteFxColorDefinition definition in
                     CrateriaEscapeLightningPaletteFxColorDefinitions.All)
            {
                for (int frame = 0; frame < CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.FrameCount; frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(definition.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                    {
                        presentationReads++;
                        break;
                    }
                }
            }

            bool livePayloadRead = presentationReads != presentationReadsBefore;
            if (!livePayloadRead)
            {
                return new ClassifyPaletteFxReadResult($"Production read unclassified palette-FX byte ${address:X6}.", presentationReads);
            }
        }

        return new ClassifyPaletteFxReadResult(null, presentationReads);
    }

    /// <summary>
    /// Reproduces the reported $F781 crash against its real retail bytecode. The assertion
    /// covers the exact native side effect and the odd one-byte operand/cursor advancement:
    /// if the interpreter accidentally advances a word, the following timed record fails.
    /// </summary>
    private static void VerifyBeaconSoundInstruction(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        var paletteFxSession = new PaletteFxSessionState();
        var paletteFx = new RoomPaletteFxSystem(paletteFxSession);
        var cgram = new SamusColourRig();
        paletteFx.SpawnDefinition(definition: 0xf781, equippedItems: 0);

        int soundFrame = -1;
        SoundRequest soundRequest = default;
        for (int frame = 0; frame < 256; frame++)
        {
            paletteFx.Step(cgram, new ReferencePaletteFxColorSource(bus), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            if (paletteFx.SoundRequests.Count == 0)
                continue;
            AssertEqual(1, paletteFx.SoundRequests.Count,
                "$F781 beacon publishes one palette-FX sound call");
            soundFrame = frame;
            soundRequest = paletteFx.SoundRequests[0];
            break;
        }

        AssertTrue(soundFrame >= 0, "$F781 beacon reaches its sound opcode");
        AssertEqual(SoundEffectLibrary.Library2, soundRequest.SoundEffect.Library,
            "$F781 beacon sound uses library two");
        AssertEqual<SoundEffectId>(Library2Sound.Beacon, soundRequest.SoundEffect,
            "$F781 beacon sound is the beacon (operand $18)");
        AssertEqual(PaletteFxAudioQueueLimits.SoundEffects, soundRequest.MaximumQueued,
            "$F781 beacon sound uses QueueSfx2_Max6");

        // The next call must parse the timed palette record immediately after the byte
        // operand. This is the observable consequence of returning Y+1 at $8D:C67A.
        paletteFx.Step(cgram, new ReferencePaletteFxColorSource(bus), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        AssertEqual(0, paletteFx.SoundRequests.Count,
            "$F781 advances beyond its one-byte sound operand");
    }

    /// <summary>Preserves the catalog's name in failures without colliding with the verification command name.</summary>
    private static Type PaletteFxInstructionCodesType() => typeof(PaletteFxInstruction);

    /// <summary>Checks catalog size, unique callback values, and mapped bank-$8D pointers where required.</summary>
    private static void AssertPaletteFxCatalog(
        string catalog,
        ushort[] pointers,
        int expectedCount,
        bool requireMappedPointers = true)
    {
        AssertEqual(expectedCount, pointers.Length, $"{catalog} exhaustive entry count");
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{catalog} contains no duplicate callbacks");
        if (requireMappedPointers)
        {
            foreach (ushort pointer in pointers)
                AssertTrue(pointer >= 0x8000, $"{catalog} pointer ${pointer:X4} is mapped");
        }
    }

    /// <summary>Runs the palette-FX instruction-catalog verification command.</summary>
    [VerificationCommand("--palette-fx-instruction-codes")]
    internal static int PaletteFxInstructionCodesCommand()
    {
        Suite(nameof(VerifyPaletteFxInstructionCodeCatalogs), () => VerifyPaletteFxInstructionCodeCatalogs());
        return 0;
    }
}
