using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Keeps the complete translated palette-FX dispatcher vocabulary explicit. This
    /// catches accidental duplicate callbacks and verifies that every executable pointer
    /// remains in mapped bank-$8D cartridge space.
    /// </summary>
    static void VerifyPaletteFxInstructionCodeCatalogs()
    {
        AssertPaletteFxCatalog(PaletteFxInstructionCodesType(), expectedCount: 19);
        AssertPaletteFxCatalog(typeof(PaletteFxSetupCodes), expectedCount: 4);
        AssertPaletteFxCatalog(typeof(PaletteFxPreInstructionCodes), expectedCount: 9);
        AssertPaletteFxCatalog(typeof(PaletteFxInstructionListPointers), expectedCount: 5);
        AssertPaletteFxCatalog(typeof(PaletteFxHeatData), expectedCount: 2, requireMappedPointers: false);

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Palette FX: 37 instruction, setup, pre-instruction, and list " +
                "pointers are structurally valid; retail ROM reads skipped.");
            return;
        }

        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(romPath);
        Type[] catalogs =
        [
            PaletteFxInstructionCodesType(),
            typeof(PaletteFxSetupCodes),
            typeof(PaletteFxPreInstructionCodes),
            typeof(PaletteFxInstructionListPointers),
        ];
        foreach (Type catalog in catalogs)
        {
            foreach (FieldInfo field in GetUshortConstants(catalog))
            {
                ushort pointer = (ushort)field.GetRawConstantValue()!;
                _ = bus.ReadByte(0x8d0000 | pointer);
            }
        }

        Suite(nameof(VerifyBeaconSoundInstruction), () => VerifyBeaconSoundInstruction(bus));
        Suite(nameof(VerifyPaletteFxDeleteProgramMechanicsDefinitions), () => VerifyPaletteFxDeleteProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTitleLogoFadePaletteFxProgramMechanicsDefinitions), () => VerifyTitleLogoFadePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyNintendoLogoFadePaletteFxProgramMechanicsDefinitions), () => VerifyNintendoLogoFadePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTitleScreenAmbientPaletteFxProgramMechanicsDefinitions), () => VerifyTitleScreenAmbientPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyCeresCinematicLightPaletteFxProgramMechanicsDefinitions), () => VerifyCeresCinematicLightPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyPlanetZebesTextPaletteFxProgramMechanicsDefinitions), () => VerifyPlanetZebesTextPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyCinematicGlowPaletteFxProgramMechanicsDefinitions), () => VerifyCinematicGlowPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyExplodingZebesFadePaletteFxProgramMechanicsDefinitions), () => VerifyExplodingZebesFadePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionFinalePaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionFinalePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyUnusedCinematicFadePaletteFxProgramMechanicsDefinitions), () => VerifyUnusedCinematicFadePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifySamusLoadingSuitPaletteFxProgramMechanicsDefinitions), () => VerifySamusLoadingSuitPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyPostCreditsIconGlarePaletteFxProgramMechanicsDefinitions), () => VerifyPostCreditsIconGlarePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyPaletteFxHeatInstructionListDefinitions), () => VerifyPaletteFxHeatInstructionListDefinitions(bus));
        Suite(nameof(VerifyPaletteFxHeatProgramMechanicsDefinitions), () => VerifyPaletteFxHeatProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyWreckedShipGreenLightPaletteFxProgramMechanicsDefinitions), () => VerifyWreckedShipGreenLightPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTourianStatueGreyPaletteFxProgramMechanicsDefinitions), () => VerifyTourianStatueGreyPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTorizoBellyPaletteFxProgramMechanicsDefinitions), () => VerifyTorizoBellyPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions), () => VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyRedBrinstarGlowPaletteFxProgramMechanicsDefinitions), () => VerifyRedBrinstarGlowPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions), () => VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyMaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions), () => VerifyMaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTourianGlowPaletteFxProgramMechanicsDefinitions), () => VerifyTourianGlowPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyBeaconPaletteFxProgramMechanicsDefinitions), () => VerifyBeaconPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyNorfairEnvironmentalPaletteFxProgramMechanicsDefinitions), () => VerifyNorfairEnvironmentalPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions), () => VerifyTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyTourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions), () => VerifyTourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyOldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions), () => VerifyOldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions), () => VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyUpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions), () => VerifyUpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions(bus));
        Suite(nameof(VerifyCrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions), () => VerifyCrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions(bus));
        var guardedHeatBus = new PaletteFxMechanicsForbiddenBus(bus);
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

    private static void VerifyPaletteFxDeleteProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        ushort[] pointers =
        [
            PaletteFxDeleteProgramMechanicsDefinitions.CinematicDelete,
            PaletteFxDeleteProgramMechanicsDefinitions.EmptyRoomEffect,
        ];
        foreach (ushort pointer in pointers)
        {
            AssertTrue(PaletteFxDeleteProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value),
                $"standalone palette-FX delete catalogs $8D:{pointer:X4}");
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"standalone palette-FX delete is registered at $8D:{pointer:X4}");
            AssertEqual(PaletteFxInstructionCodes.Delete, value,
                $"standalone palette-FX delete value at $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"standalone palette-FX compiled word at $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"standalone palette-FX cartridge word at $8D:{pointer:X4}");
        }

        AssertTrue(!PaletteFxDeleteProgramMechanicsDefinitions.TryReadMechanicsWord(
                unchecked((ushort)(PaletteFxDeleteProgramMechanicsDefinitions.CinematicDelete -
                    sizeof(ushort))),
                out _),
            "standalone delete owner rejects the preceding cinematic payload word");
        AssertTrue(!PaletteFxDeleteProgramMechanicsDefinitions.TryReadMechanicsWord(
                unchecked((ushort)(PaletteFxDeleteProgramMechanicsDefinitions.EmptyRoomEffect +
                    sizeof(ushort))),
                out _),
            "standalone delete owner rejects the following statue setup command");

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            PaletteFxDeleteProgramMechanicsDefinitions.EmptyRoomEffectDefinition,
            equippedItems: 0);
        AssertTrue(paletteFx.IsDefinitionActive(
                PaletteFxDeleteProgramMechanicsDefinitions.EmptyRoomEffectDefinition),
            "empty room palette-FX definition occupies a slot before its first handler pass");
        paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        AssertTrue(!paletteFx.IsDefinitionActive(
                PaletteFxDeleteProgramMechanicsDefinitions.EmptyRoomEffectDefinition),
            "empty room palette-FX definition deletes itself on its first handler pass");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "empty room palette-FX deletion performs no mechanics ROM reads");
        AssertEqual(0, guarded.PresentationReadCount,
            "empty room palette-FX deletion has no presentation payload");
    }

    private static void VerifyTitleScreenAmbientPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in
                 TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= definition.LoopInstructionPointer + sizeof(ushort);
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} compiled word $8D:{pointer:X4}");
                AssertEqual(value,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(definition.FrameCount * 2 + 4, actualWords,
                $"{definition.Owner} mechanics word count");

            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            unchecked((ushort)(firstColor + color * sizeof(ushort))),
                            out _),
                        $"{definition.Owner} colors remain presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0; step < definition.CycleFrames * 2; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} repeats after two complete cycles");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                definition.FrameCount * definition.ColorsPerFrame * sizeof(ushort) * 2,
                guarded.PresentationReadCount,
                $"{definition.Owner} retains every live color across two cycles");
        }
        AssertEqual(28, mechanicsWords,
            "compiled title-screen ambient mechanics words");
    }

    private static void VerifyNintendoLogoFadePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoEntry;
             pointer <= NintendoLogoFadePaletteFxProgramMechanicsDefinitions
                 .CopyrightGotoInstructionPointer + sizeof(ushort);
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!NintendoLogoFadePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Nintendo-logo fade catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Nintendo-logo fade compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"Nintendo-logo fade cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(23, mechanicsWords,
            "compiled Nintendo-logo fade mechanics words");

        for (int frame = 0;
             frame < NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            for (int color = 0;
                 color < NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        NintendoLogoFadePaletteFxProgramMechanicsDefinitions
                            .ColorPointer(frame, color),
                        out _),
                    "Nintendo-logo fade colors remain presentation-owned");
            }
        }

        (string Owner, ushort DefinitionPointer)[] nintendoLogoEntries =
        [
            ("BootLogo",
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoDefinitionPointer),
            ("Copyright",
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CopyrightDefinitionPointer),
        ];
        AssertEqual(nintendoLogoEntries.Length, NintendoLogoFadePaletteFxProgramMechanicsDefinitions.All.Count,
            "Nintendo-logo fade exposes one definition per entry");
        foreach ((string owner, ushort definitionPointer) in nintendoLogoEntries)
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definitionPointer, 0);
            for (int step = 0;
                 step < NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }
            AssertTrue(paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} fade remains active through its final hold");
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(!paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} fade deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{owner} fade avoids mechanics ROM reads");
            AssertEqual(
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount *
                NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guarded.PresentationReadCount,
                $"{owner} fade retains every live color");
        }
    }

    private static void VerifyTitleLogoFadePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = TitleLogoFadePaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= TitleLogoFadePaletteFxProgramMechanicsDefinitions
                 .DeleteInstructionPointer;
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!TitleLogoFadePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"title-logo fade catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"title-logo fade compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"title-logo fade cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(19, mechanicsWords,
            "compiled title-logo fade mechanics words");

        for (int frame = 0;
             frame < TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            for (int color = 0;
                 color < TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        TitleLogoFadePaletteFxProgramMechanicsDefinitions
                            .ColorPointer(frame, color),
                        out _),
                    "title-logo fade colors remain presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step < TitleLogoFadePaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(paletteFx.IsDefinitionActive(
                TitleLogoFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "title-logo fade remains active through its final hold");
        paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        AssertTrue(!paletteFx.IsDefinitionActive(
                TitleLogoFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "title-logo fade deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "title-logo fade avoids mechanics ROM reads");
        AssertEqual(
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount *
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame * sizeof(ushort),
            guarded.PresentationReadCount,
            "title-logo fade retains every live color");
    }

    private static void VerifyPostCreditsIconGlarePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        ushort Word(int pointer) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8d0000 | pointer);
        AssertEqual((ushort)0xc655, Word(0xdf94), "native glare set-color opcode");
        AssertEqual((ushort)0x01e0, Word(0xdf96), "native glare destination operand");
        AssertEqual(PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ProgramStart, Word(0xe202), "native glare definition entry");
        AssertEqual(PaletteFxSetupCodes.Null, Word(0xe200), "native glare setup callback");
        var original = new Dictionary<ushort, ushort> { [0xdf94] = Word(0xdf94), [0xdf96] = Word(0xdf96) };
        int cursor = 0xdf98, frame = 0;
        while (true)
        {
            ushort operation = Word(cursor);
            original.Add((ushort)cursor, operation);
            if (operation == 0xc5cf) break;
            AssertEqual((ushort)1, operation, "native glare frame duration");
            AssertEqual((ushort)cursor, PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FramePointer(frame), "decoded glare frame pointer");
            cursor += 2;
            int color = 0;
            while (Word(cursor) < 0x8000)
            {
                AssertTrue(color < 16, "native glare record has bounded color payload");
                AssertEqual((ushort)cursor, PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color), "decoded glare color pointer");
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord((ushort)cursor, out _), "glare colors remain presentation-owned");
                color++;
                cursor += 2;
            }
            AssertEqual(16, color, "native glare color count");
            AssertEqual((ushort)0xc595, Word(cursor), "native glare wait opcode");
            original.Add((ushort)cursor, Word(cursor));
            cursor += 2;
            frame++;
            AssertTrue(frame <= 14, "native glare has bounded frame count");
        }
        AssertEqual(14, frame, "decoded glare frame count");
        AssertEqual(0xe190, cursor, "decoded glare delete address");
        AssertEqual(31, original.Count, "decoded glare control-word count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            bool owned = original.TryGetValue(pointer, out ushort expected);
            AssertEqual(owned, PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort actual), "complete glare control ownership");
            AssertEqual(expected, actual, "glare control value and rejected-pointer zero result");
            if (owned)
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort compiled), "glare registered control word");
                AssertEqual(expected, compiled, "glare registered native control value");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 14, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FramePointer(invalid), "glare frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(invalid, 0), "glare color frame bounds");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(0, invalid), "glare color column bounds");
        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(paletteFx.IsDefinitionActive(
                PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "post-credits icon glare remains active through its final hold");
        paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        AssertTrue(!paletteFx.IsDefinitionActive(
                PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "post-credits icon glare deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "post-credits icon glare avoids mechanics ROM reads");
        AssertEqual(
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount *
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guarded.PresentationReadCount,
            "post-credits icon glare retains every live color");
    }

    private static void VerifySamusLoadingSuitPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        var programs = SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All;
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
            AssertThrows<ArgumentOutOfRangeException>(() => _ = programs[invalid], "Loading roster retains index bounds");
        int mechanicsWords = 0;
        int mechanicsBytes = 0;
        foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        {
            ushort header = definition.Owner switch
            {
                SamusLoadingSuitPaletteFxProgramOwner.PowerSuit => 0xe1f4,
                SamusLoadingSuitPaletteFxProgramOwner.VariaSuit => 0xe1f8,
                SamusLoadingSuitPaletteFxProgramOwner.GravitySuit => 0xe1fc,
                _ => throw new InvalidOperationException(),
            };
            AssertEqual(header, definition.DefinitionPointer, "Native loading header identity");
            int start = ReadVerificationWord(bus, 0x8d0000 | (header + 2));
            AssertEqual((ushort)start, definition.ProgramStart, "Independent native loading entry");
            var original = new Dictionary<ushort, ushort>();
            var originalBytes = new Dictionary<ushort, byte>();
            int cursor = start;
            ushort Word()
            {
                ushort value = ReadVerificationWord(bus, 0x8d0000 | cursor);
                original.Add((ushort)cursor, value);
                cursor += 2;
                return value;
            }
            AssertEqual((ushort)0xc655, Word(), "Native loading color-index instruction");
            AssertEqual((ushort)0x0180, Word(), "Native loading CGRAM origin");
            void Frame(int frame)
            {
                AssertEqual((ushort)cursor, definition.FramePointer(frame), "Calculated native loading frame cursor");
                AssertEqual(frame == 8 ? (ushort)1 : (ushort)3, Word(), "Original native loading frame duration");
                for (int color = 0; color < 16; color++, cursor += 2)
                    AssertEqual((ushort)cursor, definition.ColorPointer(frame, color), "Every calculated native color address");
                AssertEqual((ushort)0xc595, Word(), "Native loading wait instruction");
            }
            for (int group = 0; group < 4; group++)
            {
                AssertEqual((ushort)0xc648, Word(), "Native timer command starts each counted group");
                originalBytes.Add((ushort)cursor, bus.ReadByte(0x8d0000 | cursor));
                AssertEqual((ushort)cursor, definition.GroupTimerBytePointer(group), "Calculated timer byte pointer");
                cursor++;
                int groupStart = cursor;
                AssertEqual((ushort)(cursor - start), SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupStartOffset(group),
                    "Calculated group stride matches independently decoded command stream");
                Frame(group * 2);
                Frame(group * 2 + 1);
                AssertEqual((ushort)0xc639, Word(), "Original group replay instruction");
                AssertEqual((ushort)groupStart, Word(), "Original replay target");
            }
            Frame(8);
            AssertEqual((ushort)cursor, definition.DeleteInstructionPointer, "Calculated final deletion pointer");
            AssertEqual((ushort)0xc5cf, Word(), "Original final delete instruction");
            for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            {
                bool wordOwned = original.TryGetValue((ushort)pointer, out ushort expectedWord);
                AssertEqual(wordOwned, definition.TryReadMechanicsWord((ushort)pointer, out ushort actualWord), "Complete loading word ownership");
                AssertEqual(expectedWord, actualWord, "Original word or unowned zero");
                bool byteOwned = originalBytes.TryGetValue((ushort)pointer, out byte expectedByte);
                AssertEqual(byteOwned, definition.TryReadMechanicsByte((ushort)pointer, out byte actualByte), "Complete loading timer-byte ownership");
                AssertEqual(expectedByte, actualByte, "Original timer byte or unowned zero");
            }
            foreach (int invalid in new[] { -1, 4, int.MinValue, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupStartOffset(invalid),
                    "Group offset retains strict bounds");
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= definition.DeleteInstructionPointer;
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} loading catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} loading compiled word $8D:{pointer:X4}");
                AssertEqual(value,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} loading cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(33, actualWords,
                $"{definition.Owner} loading mechanics word count");

            for (int group = 0;
                 group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
                 group++)
            {
                ushort pointer = definition.GroupTimerBytePointer(group);
                AssertTrue(definition.TryReadMechanicsByte(pointer, out byte value),
                    $"{definition.Owner} loading owns timer byte $8D:{pointer:X4}");
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                        pointer,
                        out byte compiled),
                    $"{definition.Owner} loading catalogs timer byte $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} loading compiled timer byte $8D:{pointer:X4}");
                AssertEqual(value,
                    bus.ReadByte(RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} loading cartridge timer byte $8D:{pointer:X4}");
                mechanicsBytes++;
            }

            for (int frame = 0;
                 frame < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0;
                     color < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
                         .ColorsPerFrame;
                     color++)
                {
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            unchecked((ushort)(firstColor + color * sizeof(ushort))),
                            out _),
                        $"{definition.Owner} loading colors remain presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0;
                 step < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }
            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} loading remains active through its final hold");
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} loading deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} loading avoids mechanics ROM reads");
            AssertEqual(
                (SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(0) +
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(1) +
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(2) +
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(3)) *
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup *
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort) +
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} loading retains every replayed live color");
        }
        AssertEqual(99, mechanicsWords,
            "compiled Samus-loading mechanics words");
        AssertEqual(12, mechanicsBytes,
            "compiled Samus-loading byte operands");
    }

    private static void VerifyUnusedCinematicFadePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                 .ProgramStart;
             pointer <= UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                 .DeleteInstructionPointer;
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                    .TryReadMechanicsWord(pointer, out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"unused cinematic fade catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"unused cinematic fade compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"unused cinematic fade cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(25, mechanicsWords,
            "compiled unused cinematic-fade mechanics words");

        for (int frame = 0;
             frame < UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            for (int color = 0;
                 color < UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                     .ColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                            .ColorPointer(frame, color),
                        out _),
                    "unused cinematic-fade colors remain presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "unused cinematic fade deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "unused cinematic fade avoids mechanics ROM reads");
        AssertEqual(
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.FrameCount *
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guarded.PresentationReadCount,
            "unused cinematic fade retains every live color");
    }

    private static void VerifyZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        ushort Word(int pointer) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8d0000 | pointer);
        AssertEqual((ushort)0xc655, Word(0xd6ba), "native gunship reveal set-color opcode");
        AssertEqual((ushort)0x00a0, Word(0xd6bc), "native gunship reveal destination operand");
        AssertEqual(ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ProgramStart, Word(0xe1e6), "native gunship reveal definition entry");
        AssertEqual(PaletteFxSetupCodes.Null, Word(0xe1e4), "native gunship reveal setup callback");
        var original = new Dictionary<ushort, ushort> { [0xd6ba] = Word(0xd6ba), [0xd6bc] = Word(0xd6bc) };
        int cursor = 0xd6be, frame = 0;
        while (true)
        {
            ushort operation = Word(cursor);
            original.Add((ushort)cursor, operation);
            if (operation == 0xc5cf) break;
            AssertEqual((ushort)24, operation, "native gunship reveal frame duration");
            AssertEqual((ushort)cursor, ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FramePointer(frame), "decoded gunship reveal frame pointer");
            cursor += 2;
            int color = 0;
            while (Word(cursor) < 0x8000)
            {
                AssertTrue(color < 16, "native gunship reveal record has bounded color payload");
                AssertEqual((ushort)cursor, ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color), "decoded gunship reveal color pointer");
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord((ushort)cursor, out _), "gunship reveal colors remain presentation-owned");
                color++;
                cursor += 2;
            }
            AssertEqual(16, color, "native gunship reveal color count");
            AssertEqual((ushort)0xc595, Word(cursor), "native gunship reveal wait opcode");
            original.Add((ushort)cursor, Word(cursor));
            cursor += 2;
            frame++;
            AssertTrue(frame <= 16, "native gunship reveal has bounded frame count");
        }
        AssertEqual(16, frame, "decoded gunship reveal frame count");
        AssertEqual(0xd8fe, cursor, "decoded gunship reveal delete address");
        AssertEqual(35, original.Count, "decoded gunship reveal control-word count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            bool owned = original.TryGetValue(pointer, out ushort expected);
            AssertEqual(owned, ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort actual), "complete gunship reveal control ownership");
            AssertEqual(expected, actual, "gunship reveal control value and rejected-pointer zero result");
            if (owned)
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort compiled), "gunship reveal registered control word");
                AssertEqual(expected, compiled, "gunship reveal registered native control value");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FramePointer(invalid), "gunship reveal frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(invalid, 0), "gunship reveal color frame bounds");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(0, invalid), "gunship reveal color column bounds");
        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(paletteFx.IsDefinitionActive(
                ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "ending gunship reveal remains active through its final hold");
        paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        AssertTrue(!paletteFx.IsDefinitionActive(
                ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "ending gunship reveal deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "ending gunship reveal avoids mechanics ROM reads");
        AssertEqual(
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount *
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guarded.PresentationReadCount,
            "ending gunship reveal retains every live color");
    }

    private static void VerifyZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition definition in
                 ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= definition.DeleteInstructionPointer;
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} explosion fade catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} explosion fade compiled word $8D:{pointer:X4}");
                AssertEqual(value,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} explosion fade cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(19, actualWords,
                $"{definition.Owner} explosion fade mechanics word count");

            for (int frame = 0;
                 frame < ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0;
                     color < ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions
                         .ColorsPerFrame;
                     color++)
                {
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            unchecked((ushort)(firstColor + color * sizeof(ushort))),
                            out _),
                        $"{definition.Owner} explosion fade colors remain presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} explosion fade deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} explosion fade avoids mechanics ROM reads");
            AssertEqual(
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount *
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} explosion fade retains every live color");
        }
        AssertEqual(38, mechanicsWords,
            "compiled Zebes explosion layer-fade mechanics words");
    }

    private static void VerifyZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition definition in
                 ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} explosion ambience catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} explosion ambience compiled word $8D:{pointer:X4}");
                AssertEqual(value,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} explosion ambience cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(2 * definition.FrameCount + 4, actualWords,
                $"{definition.Owner} explosion ambience mechanics word count");

            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            unchecked((ushort)(firstColor + color * sizeof(ushort))),
                            out _),
                        $"{definition.Owner} explosion ambience colors remain presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} explosion ambience completes and repeats its cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} explosion ambience avoids mechanics ROM reads");
            AssertEqual(
                (definition.FrameCount + 1) * definition.ColorsPerFrame * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} explosion ambience retains every live color");
        }
        AssertEqual(40, mechanicsWords, "compiled Zebes explosion ambient mechanics words");
    }

    private static void VerifyZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                 .WideExplosionBackgroundProgramStart;
             pointer <= ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                 .DeleteInstructionPointer;
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                    .TryReadMechanicsWord(pointer, out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Zebes explosion whiteout catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Zebes explosion whiteout compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"Zebes explosion whiteout cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(37, mechanicsWords, "compiled Zebes explosion whiteout mechanics words");

        for (int frame = 0;
             frame < ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort color = unchecked((ushort)(
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                    .FramePointer(frame) + sizeof(ushort)));
            AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    color,
                    out _),
                "Zebes explosion whiteout colors remain presentation-owned");
        }

        (string Owner, ushort DefinitionPointer)[] whiteoutEntries =
        [
            ("WideExplosionBackground",
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.WideExplosionBackgroundDefinitionPointer),
            ("SpaceWhiteout",
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.SpaceWhiteoutDefinitionPointer),
        ];
        AssertEqual(whiteoutEntries.Length, ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.All.Count,
            "Zebes explosion whiteout exposes one definition per entry");
        foreach ((string owner, ushort definitionPointer) in whiteoutEntries)
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definitionPointer, 0);
            for (int step = 0;
                 step <= ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }
            AssertTrue(!paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} whiteout deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{owner} whiteout avoids mechanics ROM reads");
            AssertEqual(
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.FrameCount *
                ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guarded.PresentationReadCount,
                $"{owner} whiteout retains every live color");
        }
    }

    private static void VerifyZebesExplosionFinalePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer =
                 ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions
                 .DeleteInstructionPointer;
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions
                    .TryReadMechanicsWord(pointer, out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Zebes explosion finale catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Zebes explosion finale compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"Zebes explosion finale cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(93, mechanicsWords, "compiled Zebes explosion finale mechanics words");

        for (int frame = 0;
             frame < ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort firstColor = unchecked((ushort)(
                ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FramePointer(frame) +
                sizeof(ushort)));
            for (int color = 0;
                 color < ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions
                     .ColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        unchecked((ushort)(firstColor + color * sizeof(ushort))),
                        out _),
                    "Zebes explosion finale colors remain presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "Zebes explosion finale deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Zebes explosion finale avoids mechanics ROM reads");
        AssertEqual(
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount *
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guarded.PresentationReadCount,
            "Zebes explosion finale retains every live color");
    }

    private static void VerifyZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer =
                 ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                 .DeleteInstructionPointer;
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                    .TryReadMechanicsWord(pointer, out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Zebes explosion foreground catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Zebes explosion foreground compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"Zebes explosion foreground cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(35, mechanicsWords,
            "compiled Zebes explosion foreground mechanics words");

        for (int frame = 0;
             frame < ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort firstColor = unchecked((ushort)(
                ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                    .FramePointer(frame) + sizeof(ushort)));
            for (int color = 0;
                 color < ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                     .ColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        unchecked((ushort)(firstColor + color * sizeof(ushort))),
                        out _),
                    "Zebes explosion foreground colors remain presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "Zebes explosion foreground deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Zebes explosion foreground avoids mechanics ROM reads");
        AssertEqual(
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.FrameCount *
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guarded.PresentationReadCount,
            "Zebes explosion foreground retains every live color");
    }

    private static void VerifyExplodingZebesFadePaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= ExplodingZebesFadePaletteFxProgramMechanicsDefinitions
                 .DeleteInstructionPointer;
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"exploding-Zebes fade catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"exploding-Zebes fade compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"exploding-Zebes fade cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(17, mechanicsWords, "compiled exploding-Zebes fade mechanics words");

        for (int frame = 0;
             frame < ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            for (int color = 0;
                 color < ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        ExplodingZebesFadePaletteFxProgramMechanicsDefinitions
                            .ColorPointer(frame, color),
                        out _),
                    "exploding-Zebes fade colors remain presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            0);
        for (int step = 0;
             step <= ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }
        AssertTrue(!paletteFx.IsDefinitionActive(
                ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "exploding-Zebes fade deletes after its final hold");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "exploding-Zebes fade avoids mechanics ROM reads");
        AssertEqual(
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount *
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
            sizeof(ushort),
            guarded.PresentationReadCount,
            "exploding-Zebes fade retains every live color");
    }

    private static void VerifyCinematicGlowPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (CinematicGlowPaletteFxProgramDefinition definition in
                 CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} glow catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} glow compiled word $8D:{pointer:X4}");
                AssertEqual(value,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} glow cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(32, actualWords, $"{definition.Owner} glow mechanics word count");

            for (int frame = 0;
                 frame < CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            definition.ColorPointer(frame, color),
                            out _),
                        $"{definition.Owner} glow colors remain presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} glow completes and repeats its cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} glow avoids mechanics ROM reads");
            AssertEqual(
                (CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                definition.ColorsPerFrame * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} glow retains every live color");
        }
        AssertEqual(64, mechanicsWords, "compiled cinematic-glow mechanics words");
    }

    private static void VerifyPlanetZebesTextPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (PlanetZebesTextPaletteFxProgramDefinition definition in
                 PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= definition.DeleteInstructionPointer;
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} PLANET ZEBES fade catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} PLANET ZEBES fade compiled word $8D:{pointer:X4}");
                AssertEqual(value,
                    RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} PLANET ZEBES fade cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(19, actualWords,
                $"{definition.Owner} PLANET ZEBES fade mechanics word count");

            for (int frame = 0;
                 frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                for (int color = 0;
                     color < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                     color++)
                {
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            definition.ColorPointer(frame, color),
                            out _),
                        $"{definition.Owner} PLANET ZEBES colors remain presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0;
                 step <= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }
            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} PLANET ZEBES fade deletes after its final hold");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} PLANET ZEBES fade avoids mechanics ROM reads");
            AssertEqual(
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount *
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} PLANET ZEBES fade retains every live color");
        }
        AssertEqual(38, mechanicsWords,
            "compiled PLANET ZEBES text-fade mechanics words");
    }

    private static void VerifyCeresCinematicLightPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer =
                 CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineProgramStart;
             pointer <= unchecked((ushort)(
                 CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                     .BackgroundNavigationLightsProgramStart + 6));
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!CeresCinematicLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Ceres cinematic lights catalog word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Ceres cinematic lights compiled word $8D:{pointer:X4}");
            AssertEqual(value,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"Ceres cinematic lights cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(44, mechanicsWords, "compiled Ceres cinematic-light mechanics words");

        for (int frame = 0;
             frame < CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                 .GunshipEngineFrameCount;
             frame++)
        {
            ushort color = CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineColorPointer(frame, 0);
            AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    color,
                    out _),
                "gunship-engine colors remain presentation-owned");
        }

        for (int frame = 0;
             frame < CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                 .NavigationLightsFrameCount;
             frame++)
        {
            for (int color = 0;
                 color < CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                     .NavigationLightsColorsPerFrame;
                 color++)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                            .NavigationLightsColorPointer(frame, color),
                        out _),
                    "Ceres navigation-light colors remain presentation-owned");
            }
        }

        (CeresCinematicLightPaletteFxProgramOwner Owner, ushort DefinitionPointer)[] ceresLightEntries =
        [
            (CeresCinematicLightPaletteFxProgramOwner.GunshipEngine,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineDefinitionPointer),
            (CeresCinematicLightPaletteFxProgramOwner.SpriteNavigationLights,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions.SpriteNavigationLightsDefinitionPointer),
            (CeresCinematicLightPaletteFxProgramOwner.BackgroundNavigationLights,
                CeresCinematicLightPaletteFxProgramMechanicsDefinitions.BackgroundNavigationLightsDefinitionPointer),
        ];
        AssertEqual(ceresLightEntries.Length, CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All.Count,
            "Ceres cinematic lights expose one definition per entry");
        foreach ((CeresCinematicLightPaletteFxProgramOwner owner, ushort definitionPointer) in ceresLightEntries)
        {
            bool isGunship = owner ==
                CeresCinematicLightPaletteFxProgramOwner.GunshipEngine;
            int cycleFrames = isGunship
                ? CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .GunshipEngineCycleFrames
                : CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .NavigationLightsCycleFrames;
            int frameCount = isGunship
                ? CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .GunshipEngineFrameCount
                : CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .NavigationLightsFrameCount;
            int colorsPerFrame = isGunship
                ? CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .GunshipEngineColorsPerFrame
                : CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                    .NavigationLightsColorsPerFrame;

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definitionPointer, 0);
            for (int step = 0; step <= cycleFrames; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(paletteFx.IsDefinitionActive(definitionPointer),
                $"{owner} completes and repeats its cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{owner} avoids mechanics ROM reads");
            AssertEqual((frameCount + 1) * colorsPerFrame * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{owner} retains every live color");
        }
    }

    private static void VerifyCrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition definition in
                 CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled, $"{definition.Owner} compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(26, actualWords, $"{definition.Owner} mechanics word count");

            for (int frame = 0; frame < CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
            {
                ushort firstColor = unchecked((ushort)(definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            unchecked((ushort)(firstColor + color * sizeof(ushort))), out _),
                        $"{definition.Owner} colors remain presentation-owned");
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, 0);
            for (int step = 0; step <= CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.CycleFrames; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} completes and repeats its cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual((CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                definition.ColorsPerFrame * sizeof(ushort), guarded.PresentationReadCount,
                $"{definition.Owner} retains every live color");
        }
        AssertEqual(52, mechanicsWords, "compiled late-Crateria escape mechanics words");
    }

    private static void VerifyUpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= unchecked((ushort)(UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2));
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort value))
                continue;
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort compiled),
                $"upper-Crateria escape flash catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled, $"upper-Crateria escape flash compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomData.Banks.PaletteFx | pointer),
                $"upper-Crateria escape flash cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(32, mechanicsWords, "compiled upper-Crateria escape mechanics words");

        for (int frame = 0; frame < UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
        {
            ushort firstColor = unchecked((ushort)(UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame) + sizeof(ushort)));
            for (int color = 0; color < UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame; color++)
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(unchecked((ushort)(firstColor + color * sizeof(ushort))), out _),
                    "upper-Crateria escape colors remain presentation-owned");
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(guarded, UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer, 0);
        for (int step = 0; step <= UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.CycleFrames; step++)
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        AssertTrue(paletteFx.IsDefinitionActive(UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "upper-Crateria escape flash completes and repeats its cycle");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "upper-Crateria escape flash avoids mechanics ROM reads");
        AssertEqual((UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame * sizeof(ushort),
            guarded.PresentationReadCount, "upper-Crateria escape flash retains every live color");
    }

    private static void VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in
                 OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(34, actualWords, $"{definition.Owner} mechanics word count");

            for (int frame = 0;
                 frame < OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0;
                     color < OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                     color++)
                {
                    ushort pointer = unchecked((ushort)(firstColor + color * sizeof(ushort)));
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            pointer,
                            out _),
                        $"{definition.Owner} color $8D:{pointer:X4} remains presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0;
                 step <= OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} completes and repeats its full cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                (OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} retains every live color through its cycle");
        }

        AssertEqual(68, mechanicsWords,
            "compiled old-Tourian escape accent mechanics words");
    }

    private static void VerifyOldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer =
                 OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= unchecked((ushort)(
                 OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2));
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions
                    .TryReadMechanicsWord(pointer, out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"old-Tourian red flash catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"old-Tourian red flash compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"old-Tourian red flash cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(60, mechanicsWords, "compiled old-Tourian red-flash mechanics words");

        for (int frame = 0;
             frame < OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort framePointer =
                OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            ushort[] colorPointers =
            [
                unchecked((ushort)(framePointer + 2)),
                unchecked((ushort)(framePointer + 4)),
                unchecked((ushort)(framePointer + 6)),
                unchecked((ushort)(framePointer + 10)),
                unchecked((ushort)(framePointer + 12)),
                unchecked((ushort)(framePointer + 14)),
                unchecked((ushort)(framePointer + 16)),
                unchecked((ushort)(framePointer + 20)),
            ];
            foreach (ushort pointer in colorPointers)
            {
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out _),
                    $"old-Tourian color $8D:{pointer:X4} remains presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnDefinition(
            guarded,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            equippedItems: 0);
        for (int step = 0;
             step <= OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
        }

        AssertTrue(paletteFx.IsDefinitionActive(
                OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "old-Tourian red flash completes and repeats its full cycle");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "old-Tourian red flash avoids mechanics ROM reads");
        AssertEqual(
            (OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
            guarded.PresentationReadCount,
            "old-Tourian red flash retains every live color through its cycle");
    }

    private static void VerifyTourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer =
                 TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.GeneralLevelProgramStart;
             pointer <= unchecked((ushort)(
                 TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2));
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions
                    .TryReadMechanicsWord(pointer, out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"shared Tourian escape red flash catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"shared Tourian escape red flash compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"shared Tourian escape red flash cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(50, mechanicsWords,
            "compiled shared Tourian escape red-flash mechanics words");

        for (int frame = 0;
             frame < TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort framePointer =
                TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            for (int color = 0;
                 color < TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                ushort pointer = color < 6
                    ? unchecked((ushort)(framePointer + sizeof(ushort) + color * sizeof(ushort)))
                    : unchecked((ushort)(framePointer + 16));
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out _),
                    $"shared Tourian escape color $8D:{pointer:X4} remains presentation-owned");
            }
        }

        foreach (TourianEscapeSharedRedFlashPaletteFxProgramDefinition definition in
                 TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0;
                 step <= TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} completes and repeats the shared cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                (TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} retains every live color through the shared cycle");
        }
    }

    private static void VerifyTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in
                 TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(32, actualWords, $"{definition.Owner} mechanics word count");

            for (int frame = 0;
                 frame < TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort firstColor = unchecked((ushort)(
                    definition.FramePointer(frame) + sizeof(ushort)));
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    ushort pointer = unchecked((ushort)(firstColor + color * sizeof(ushort)));
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            pointer,
                            out _),
                        $"{definition.Owner} color $8D:{pointer:X4} remains presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} completes and repeats its full cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                (TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    definition.ColorsPerFrame * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} retains every live color through its cycle");
        }

        AssertEqual(64, mechanicsWords,
            "compiled early Tourian escape red-flash mechanics words");
    }

    private static void VerifyNorfairEnvironmentalPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        int mechanicsBytes = 0;
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                 NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            int actualWords = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 1)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        bus,
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} cartridge word $8D:{pointer:X4}");
                actualWords++;
                mechanicsWords++;
            }
            AssertEqual(definition.PublishesHeatPhase ? 68 : 52, actualWords,
                $"{definition.Owner} mechanics word count");

            for (int frame = 0;
                 frame < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort framePointer = definition.FramePointer(frame);
                int durationOffset = definition.PublishesHeatPhase ? 3 : 0;
                if (definition.PublishesHeatPhase)
                {
                    ushort bytePointer = unchecked((ushort)(framePointer + 2));
                    AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                            bytePointer,
                            out byte compiledByte),
                        $"{definition.Owner} catalogs phase byte $8D:{bytePointer:X4}");
                    AssertEqual((byte)frame, compiledByte,
                        $"{definition.Owner} compiled phase byte {frame}");
                    AssertEqual((byte)frame,
                        bus.ReadByte(RoomFxRomData.Banks.PaletteFx | bytePointer),
                        $"{definition.Owner} cartridge phase byte {frame}");
                    mechanicsBytes++;
                }

                for (int color = 0;
                     color < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                     color++)
                {
                    ushort pointer = definition.ColorPointer(frame, color);
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            pointer,
                            out _),
                        $"{definition.Owner} color $8D:{pointer:X4} remains presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0;
                 step <= NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
                paletteFx.Step(guarded, new SnesCgram(), new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} completes and repeats its full cycle");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                (NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame * 2,
                guarded.PresentationReadCount,
                $"{definition.Owner} retains every live color through its cycle");
            if (definition.PublishesHeatPhase)
            {
                AssertEqual((ushort)0, paletteFx.SamusInHeatPaletteIndex,
                    "Norfair phase publisher repeats phase zero after a complete cycle");
            }
        }

        AssertEqual(224, mechanicsWords, "compiled Norfair environmental mechanics words");
        AssertEqual(16, mechanicsBytes, "compiled Norfair heat-phase bytes");
    }

    private static void VerifyBeaconPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = BeaconPaletteFxProgramMechanicsDefinitions.ProgramStart;
             pointer <= unchecked((ushort)(
                 BeaconPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2));
             pointer = unchecked((ushort)(pointer + 1)))
        {
            if (!BeaconPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"beacon program catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled, $"beacon compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    bus,
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"beacon cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(35, mechanicsWords, "compiled beacon mechanics words");
        AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                BeaconPaletteFxProgramMechanicsDefinitions.SoundOperandPointer,
                out byte soundId),
            "beacon sound ID is compiled");
        AssertEqual(BeaconPaletteFxProgramMechanicsDefinitions.SoundId, soundId,
            "beacon compiled sound ID");
        AssertEqual(bus.ReadByte(RoomFxRomData.Banks.PaletteFx |
                BeaconPaletteFxProgramMechanicsDefinitions.SoundOperandPointer),
            soundId,
            "beacon sound ID matches cartridge");

        for (int frame = 0;
             frame < BeaconPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            for (int color = 0;
                 color < BeaconPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                ushort pointer = BeaconPaletteFxProgramMechanicsDefinitions.ColorPointer(
                    frame, color);
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out _),
                    $"beacon color $8D:{pointer:X4} remains presentation-owned");
            }
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        var cgram = new SnesCgram();
        paletteFx.SpawnDefinition(
            guarded,
            BeaconPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            equippedItems: 0);
        int soundCount = 0;
        for (int step = 0;
             step <= BeaconPaletteFxProgramMechanicsDefinitions.CycleFrames;
             step++)
        {
            paletteFx.Step(guarded, cgram, new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            if (paletteFx.SoundRequests.Count == 0)
                continue;
            AssertEqual(1, paletteFx.SoundRequests.Count,
                "beacon publishes one sound at its midpoint");
            AssertEqual(SoundEffectLibrary.Library2,
                paletteFx.SoundRequests[0].SoundEffect.Library,
                "beacon selects sound library two");
            AssertEqual(BeaconPaletteFxProgramMechanicsDefinitions.SoundId,
                paletteFx.SoundRequests[0].SoundEffect.Value,
                "beacon retains its compiled sound ID");
            soundCount++;
        }

        AssertEqual(1, soundCount, "beacon queues its sound once per complete cycle");
        AssertTrue(paletteFx.IsDefinitionActive(
                BeaconPaletteFxProgramMechanicsDefinitions.DefinitionPointer),
            "beacon remains active after its complete cycle");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "beacon avoids mechanics ROM reads");
        AssertEqual(
            (BeaconPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                BeaconPaletteFxProgramMechanicsDefinitions.ColorsPerFrame * 2,
            guarded.PresentationReadCount,
            "beacon retains every live color read through its complete cycle");
        AssertEqual((ushort)0x02bf, cgram.Colors[0x71],
            "beacon writes its first live color");
        AssertEqual((ushort)0, cgram.Colors[0x74],
            "beacon preserves the first skipped CGRAM color");
        AssertEqual((ushort)0, cgram.Colors[0x7c],
            "beacon preserves the ninth skipped CGRAM color");
        AssertEqual((ushort)0x7fff, cgram.Colors[0x7d],
            "beacon resumes after the native eighteen-byte skip");
    }

    private static void VerifyTourianGlowPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        for (ushort pointer = TourianGlowPaletteFxProgramMechanicsDefinitions.CloneProgramStart;
             pointer <= unchecked((ushort)(
                 TourianGlowPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2));
             pointer = unchecked((ushort)(pointer + 2)))
        {
            if (!TourianGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort value))
            {
                continue;
            }
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Tourian glow catalogs word $8D:{pointer:X4}");
            AssertEqual(value, compiled, $"Tourian glow compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    bus,
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"Tourian glow cartridge word $8D:{pointer:X4}");
            mechanicsWords++;
        }
        AssertEqual(43, mechanicsWords, "compiled Tourian glow mechanics words");

        for (int frame = 0;
             frame < TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort firstColor =
                TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, 0);
            AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    firstColor,
                    out _),
                $"Tourian glow first color $8D:{firstColor:X4} remains presentation-owned");
            for (int color = 1;
                 color < TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                ushort pointer =
                    TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out _),
                    $"Tourian glow color $8D:{pointer:X4} remains presentation-owned");
            }
        }

        foreach (ushort definition in new ushort[]
                 {
                     TourianGlowPaletteFxProgramMechanicsDefinitions.LiveDefinitionPointer,
                     TourianGlowPaletteFxProgramMechanicsDefinitions.CloneDefinitionPointer,
                 })
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            paletteFx.SpawnDefinition(guarded, definition, equippedItems: 0);
            for (int step = 0;
                 step <= TourianGlowPaletteFxProgramMechanicsDefinitions.CycleFrames;
                 step++)
            {
                paletteFx.Step(guarded, cgram, new ReferencePaletteFxColorSource(guarded), 0, 0, false, false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition),
                $"Tourian glow definition $8D:{definition:X4} completes its loop");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"Tourian glow definition $8D:{definition:X4} avoids mechanics ROM reads");
            AssertEqual(
                (TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame * 2,
                guarded.PresentationReadCount,
                $"Tourian glow definition $8D:{definition:X4} retains all live colors");
            AssertEqual((ushort)0x5294, cgram.Colors[0x74],
                "Tourian glow writes its isolated first color");
            AssertEqual((ushort)0, cgram.Colors[0x75],
                "Tourian glow preserves first skipped CGRAM color");
            AssertEqual((ushort)0, cgram.Colors[0x76],
                "Tourian glow preserves second skipped CGRAM color");
            AssertEqual((ushort)0, cgram.Colors[0x77],
                "Tourian glow preserves third skipped CGRAM color");
            AssertEqual((ushort)0x0019, cgram.Colors[0x78],
                "Tourian glow resumes after the native six-byte skip");
        }

        var deletionFx = new RoomPaletteFxSystem();
        deletionFx.SpawnDefinition(bus,
            TourianGlowPaletteFxProgramMechanicsDefinitions.LiveDefinitionPointer, 0);
        deletionFx.SpawnDefinition(bus, 0xf795, 0);
        deletionFx.SpawnDefinition(bus, 0xf799, 0);
        deletionFx.Step(bus, new SnesCgram(), new ReferencePaletteFxColorSource(bus), 0, 0, false, false);
        deletionFx.Step(bus, new SnesCgram(), new ReferencePaletteFxColorSource(bus), 0, 0, false, false);
        AssertTrue(!deletionFx.IsDefinitionActive(
                TourianGlowPaletteFxProgramMechanicsDefinitions.LiveDefinitionPointer),
            "Tourian glow pre-instruction deletes its owner when two later slots exist");
    }

    private static void VerifyMaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                 MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            int expectedWordCount = 4 + definition.FrameCount * 2;
            int actualWordCount = 0;
            for (ushort pointer = definition.ProgramStart;
                 pointer <= unchecked((ushort)(definition.LoopInstructionPointer + 2));
                 pointer = unchecked((ushort)(pointer + 2)))
            {
                if (!definition.TryReadMechanicsWord(pointer, out ushort value))
                    continue;
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        bus,
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} cartridge word $8D:{pointer:X4}");
                actualWordCount++;
                mechanicsWords++;
            }
            AssertEqual(expectedWordCount, actualWordCount,
                $"{definition.Owner} mechanics word count");

            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                ushort framePointer = definition.FramePointer(frame);
                for (int color = 0; color < definition.ColorsPerFrame; color++)
                {
                    ushort pointer = unchecked((ushort)(
                        framePointer + sizeof(ushort) + color * sizeof(ushort)));
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            pointer,
                            out _),
                        $"{definition.Owner} color $8D:{pointer:X4} remains presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(
                guarded,
                definition.DefinitionPointer,
                equippedItems: 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
            {
                paletteFx.Step(
                    guarded,
                    new SnesCgram(), new ReferencePaletteFxColorSource(guarded),
                    samusY: 0,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} loops through its complete program");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            AssertEqual(
                (definition.FrameCount + 1) * definition.ColorsPerFrame * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} retains all live colors through its complete cycle");
            AssertTrue(!definition.TryReadMechanicsWord(
                    unchecked((ushort)(definition.ProgramStart - 2)),
                    out _),
                $"{definition.Owner} rejects preceding code");
            AssertTrue(!definition.TryReadMechanicsWord(
                    unchecked((ushort)(definition.LoopInstructionPointer + 4)),
                    out _),
                $"{definition.Owner} rejects following code");
        }

        AssertEqual(44, mechanicsWords, "compiled Maridia environmental mechanics words");
    }

    private static void VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        int mechanicsWords = 0;
        int mechanicsBytes = 0;
        foreach (CrateriaLightningPaletteFxProgramDefinition definition in
                 CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            int expectedWordCount = definition.Owner ==
                CrateriaLightningPaletteOwner.SurfaceLightning ? 38 : 40;
            AssertEqual(expectedWordCount, definition.MechanicsWords.Count,
                $"{definition.Owner} mechanics word count");
            AssertEqual(2, definition.MechanicsBytes.Count,
                $"{definition.Owner} mechanics byte count");

            foreach (PaletteFxMechanicsWord word in definition.MechanicsWords)
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        word.Pointer,
                        out ushort compiled),
                    $"{definition.Owner} catalogs word $8D:{word.Pointer:X4}");
                AssertEqual(word.Value, compiled,
                    $"{definition.Owner} compiled word $8D:{word.Pointer:X4}");
                AssertEqual(word.Value, RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | word.Pointer),
                    $"{definition.Owner} cartridge word $8D:{word.Pointer:X4}");
                mechanicsWords++;
            }
            foreach ((ushort Pointer, byte Value) item in definition.TimerMechanicsBytes)
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                        item.Pointer,
                        out byte compiled),
                    $"{definition.Owner} catalogs byte $8D:{item.Pointer:X4}");
                AssertEqual(item.Value, compiled,
                    $"{definition.Owner} compiled byte $8D:{item.Pointer:X4}");
                AssertEqual(item.Value,
                    bus.ReadByte(RoomFxRomData.Banks.PaletteFx | item.Pointer),
                    $"{definition.Owner} cartridge byte $8D:{item.Pointer:X4}");
                mechanicsBytes++;
            }
            for (int frameIndex = 0; frameIndex < definition.Frames.Count; frameIndex++)
            {
                CrateriaLightningPaletteFrame frame = definition.Frames[frameIndex];
                for (int color = 0; color < frame.ColorCount; color++)
                {
                    ushort pointer = definition.ColorPointer(frameIndex, color);
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            pointer,
                            out _),
                        $"{definition.Owner} color $8D:{pointer:X4} remains presentation-owned");
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                            pointer,
                            out _),
                        $"{definition.Owner} color byte $8D:{pointer:X4} remains presentation-owned");
                }
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0; step <= definition.CycleFrames; step++)
            {
                paletteFx.Step(
                    guarded,
                    cgram, new ReferencePaletteFxColorSource(guarded),
                    samusY:
                        CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} loops through its complete timer program");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} avoids mechanics ROM reads");
            int colorsPerRecord = definition.Frames[0].ColorCount;
            AssertEqual(
                definition.DisplayedRecordsPerCycle * colorsPerRecord * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} retains all live colors through its complete cycle");

            var resetGuard = new PaletteFxMechanicsForbiddenBus(bus);
            var resetFx = new RoomPaletteFxSystem();
            resetFx.SpawnDefinition(resetGuard, definition.DefinitionPointer, equippedItems: 0);
            resetFx.Step(
                resetGuard,
                new SnesCgram(), new ReferencePaletteFxColorSource(resetGuard),
                CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY,
                0,
                false,
                false);
            resetFx.Step(
                resetGuard,
                new SnesCgram(), new ReferencePaletteFxColorSource(resetGuard),
                unchecked((ushort)(
                    CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY -
                    1)),
                0,
                false,
                false);
            AssertEqual(2 * colorsPerRecord * sizeof(ushort),
                resetGuard.PresentationReadCount,
                $"{definition.Owner} low-Samus pre-instruction restarts its neutral frame");
            AssertEqual(0, resetGuard.ForbiddenReadAttempts,
                $"{definition.Owner} restart avoids mechanics ROM reads");
        }

        AssertEqual(78, mechanicsWords, "compiled Crateria lightning mechanics words");
        AssertEqual(4, mechanicsBytes, "compiled Crateria lightning mechanics bytes");
    }

    private static void VerifyRedBrinstarGlowPaletteFxProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        var expected = new Dictionary<ushort, ushort>
        {
            [RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ProgramStart] =
                PaletteFxInstructionCodes.SetColorIndex,
            [unchecked((ushort)(
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ProgramStart + 2))] =
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            [RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer] =
                PaletteFxInstructionCodes.Goto,
            [unchecked((ushort)(
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2))] =
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FirstFramePointer,
        };
        for (int frame = 0;
             frame < RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort pointer = RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            expected.Add(pointer, 10);
            expected.Add(
                unchecked((ushort)(pointer +
                    RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameByteCount - 2)),
                PaletteFxInstructionCodes.Wait);
            for (int color = 0;
                 color < RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                ushort presentationPointer =
                    RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorPointer(
                        frame, color);
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        presentationPointer,
                        out _),
                    $"Red Brinstar glow color $8D:{presentationPointer:X4} remains presentation-owned");
            }
        }

        AssertEqual(32, expected.Count, "Red Brinstar glow mechanics word count");
        foreach ((ushort pointer, ushort value) in expected)
        {
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Red Brinstar glow program catalogs $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Red Brinstar glow compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"Red Brinstar glow cartridge word $8D:{pointer:X4}");
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        var cgram = new SnesCgram();
        const ushort definition =
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
        paletteFx.SpawnDefinition(guarded, definition, equippedItems: 0);
        const int cycleFrames =
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount * 10;
        for (int step = 0; step <= cycleFrames; step++)
        {
            paletteFx.Step(
                guarded,
                cgram, new ReferencePaletteFxColorSource(guarded),
                samusY: 0,
                equippedItems: 0,
                enemyZeroIsDead: false,
                areaMiniBossDefeated: false);
        }

        AssertTrue(paletteFx.IsDefinitionActive(definition),
            "Red Brinstar glow program loops after fourteen frames");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Red Brinstar glow program avoids mechanics ROM reads");
        AssertEqual(
            (RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                sizeof(ushort),
            guarded.PresentationReadCount,
            "Red Brinstar glow program retains live colors through loop");
    }

    private static void VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in
                 BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All)
        {
            var expected = new Dictionary<ushort, ushort>();
            if (definition.DeletesWithAreaMiniBoss)
            {
                expected.Add(definition.ProgramStart,
                    PaletteFxInstructionCodes.SetPreInstruction);
                expected.Add(unchecked((ushort)(definition.ProgramStart + 2)),
                    PaletteFxPreInstructionCodes.DeleteWhenAreaMiniBossDies);
                expected.Add(unchecked((ushort)(definition.ProgramStart + 4)),
                    PaletteFxInstructionCodes.SetColorIndex);
                expected.Add(unchecked((ushort)(definition.ProgramStart + 6)),
                    BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex);
            }
            else
            {
                expected.Add(definition.ProgramStart, PaletteFxInstructionCodes.SetColorIndex);
                expected.Add(unchecked((ushort)(definition.ProgramStart + 2)),
                    BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex);
            }

            expected.Add(definition.LoopInstructionPointer, PaletteFxInstructionCodes.Goto);
            expected.Add(unchecked((ushort)(definition.LoopInstructionPointer + 2)),
                definition.FirstFramePointer);
            for (int frame = 0;
                 frame < BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort pointer = definition.FramePointer(frame);
                expected.Add(pointer, 10);
                expected.Add(
                    unchecked((ushort)(pointer +
                        BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameByteCount - 2)),
                    PaletteFxInstructionCodes.Wait);
                for (int color = 0;
                     color < BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                     color++)
                {
                    ushort presentationPointer = definition.ColorPointer(frame, color);
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            presentationPointer,
                            out _),
                        $"{definition.Owner} blue-spore color $8D:{presentationPointer:X4} remains presentation-owned");
                }
            }

            int expectedWordCount = definition.DeletesWithAreaMiniBoss ? 34 : 32;
            AssertEqual(expectedWordCount, expected.Count,
                $"{definition.Owner} blue-spore mechanics word count");
            foreach ((ushort pointer, ushort value) in expected)
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} blue-spore program catalogs $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} blue-spore compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} blue-spore cartridge word $8D:{pointer:X4}");
                mechanicsWords++;
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            paletteFx.SpawnDefinition(
                guarded,
                definition.DefinitionPointer,
                equippedItems: 0,
                areaMiniBossDefeated: false);
            const int cycleFrames =
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount * 10;
            for (int step = 0; step <= cycleFrames; step++)
            {
                paletteFx.Step(
                    guarded,
                    cgram, new ReferencePaletteFxColorSource(guarded),
                    samusY: 0,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} blue-spore program loops after fourteen frames");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} blue-spore program avoids mechanics ROM reads");
            AssertEqual(
                (BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} blue-spore program retains live colors through loop");

            paletteFx.Step(
                guarded,
                cgram, new ReferencePaletteFxColorSource(guarded),
                samusY: 0,
                equippedItems: 0,
                enemyZeroIsDead: false,
                areaMiniBossDefeated: true);
            AssertEqual(!definition.DeletesWithAreaMiniBoss,
                paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} blue-spore mini-boss death ownership");
        }

        AssertEqual(66, mechanicsWords, "compiled Brinstar blue-spore mechanics words");
    }

    private static void VerifyTorizoBellyPaletteFxProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (TorizoBellyPaletteFxProgramDefinition definition in
                 TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
        {
            var expected = new Dictionary<ushort, ushort>
            {
                [definition.ProgramStart] = PaletteFxInstructionCodes.SetColorIndex,
                [unchecked((ushort)(definition.ProgramStart + 2))] =
                    TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
                [unchecked((ushort)(definition.ProgramStart + 4))] =
                    PaletteFxInstructionCodes.SetPreInstruction,
                [unchecked((ushort)(definition.ProgramStart + 6))] =
                    PaletteFxPreInstructionCodes.DeleteWhenEnemyZeroDies,
                [definition.LoopInstructionPointer] = PaletteFxInstructionCodes.Goto,
                [unchecked((ushort)(definition.LoopInstructionPointer + 2))] =
                    definition.FirstFramePointer,
            };
            for (int frame = 0;
                 frame < TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount;
                 frame++)
            {
                ushort pointer = definition.FramePointer(frame);
                expected.Add(pointer,
                    TorizoBellyPaletteFxProgramMechanicsDefinitions.Duration(frame));
                expected.Add(
                    unchecked((ushort)(pointer +
                        TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameByteCount - 2)),
                    PaletteFxInstructionCodes.Wait);
                for (int color = 0;
                     color < TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                     color++)
                {
                    ushort presentationPointer = definition.ColorPointer(frame, color);
                    AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                            presentationPointer,
                            out _),
                        $"{definition.Owner} belly color $8D:{presentationPointer:X4} remains presentation-owned");
                }
            }

            AssertEqual(18, expected.Count, $"{definition.Owner} belly mechanics word count");
            foreach ((ushort pointer, ushort value) in expected)
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Owner} belly program catalogs $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Owner} belly compiled word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Owner} belly cartridge word $8D:{pointer:X4}");
                mechanicsWords++;
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            const int cycleFrames = 10 + 8 + 8 + 10 + 8 + 8;
            for (int step = 0; step <= cycleFrames; step++)
            {
                paletteFx.Step(
                    guarded,
                    cgram, new ReferencePaletteFxColorSource(guarded),
                    samusY: 0,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertTrue(paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} belly program loops after six frames");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Owner} belly program avoids mechanics ROM reads");
            AssertEqual(
                (TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Owner} belly program retains live colors through loop");

            paletteFx.Step(
                guarded,
                cgram, new ReferencePaletteFxColorSource(guarded),
                samusY: 0,
                equippedItems: 0,
                enemyZeroIsDead: true,
                areaMiniBossDefeated: false);
            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} belly pre-instruction deletes when enemy zero dies");
        }

        AssertEqual(36, mechanicsWords, "compiled Torizo belly mechanics words");
        AssertTrue(!TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                0xe2e0,
                out _),
            "Torizo belly owner rejects adjacent pre-instruction code");
        AssertTrue(!TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                0xe379,
                out _),
            "Torizo belly owner rejects adjacent heat pre-instruction code");
    }

    private static void VerifyTourianStatueGreyPaletteFxProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        var expected = new Dictionary<ushort, ushort>();
        foreach (TourianStatueGreyPaletteFxProgramDefinition definition in
                 TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All)
        {
            expected.Add(definition.ProgramStart, PaletteFxInstructionCodes.SetColorIndex);
            expected.Add(unchecked((ushort)(definition.ProgramStart + 2)),
                definition.ColorByteIndex);
            if (definition.UsesGoto)
            {
                expected.Add(unchecked((ushort)(definition.ProgramStart + 4)),
                    PaletteFxInstructionCodes.Goto);
                expected.Add(unchecked((ushort)(definition.ProgramStart + 6)),
                    TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FirstFramePointer);
            }
        }

        for (int frame = 0;
             frame < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort pointer =
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            expected.Add(pointer, 8);
            expected.Add(
                unchecked((ushort)(pointer +
                    TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameByteCount - 2)),
                PaletteFxInstructionCodes.Wait);

            for (int color = 0;
                 color < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                ushort presentationPointer =
                    TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorPointer(
                        frame, color);
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        presentationPointer,
                        out _),
                    $"Tourian statue color $8D:{presentationPointer:X4} remains presentation-owned");
            }
        }
        expected.Add(
            TourianStatueGreyPaletteFxProgramMechanicsDefinitions.DeleteInstructionPointer,
            PaletteFxInstructionCodes.Delete);

        AssertEqual(31, expected.Count, "Tourian statue grey mechanics word count");
        foreach ((ushort pointer, ushort value) in expected)
        {
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Tourian statue grey program catalogs $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Tourian statue grey compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"Tourian statue grey cartridge word $8D:{pointer:X4}");
        }

        foreach (TourianStatueGreyPaletteFxProgramDefinition definition in
                 TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All)
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            for (int step = 0;
                 step < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FramesThroughDeletion;
                 step++)
            {
                paletteFx.Step(
                    guarded,
                    new SnesCgram(), new ReferencePaletteFxColorSource(guarded),
                    samusY: 0,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"palette-FX definition $8D:{definition.DefinitionPointer:X4} reaches " +
                "compiled deletion");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"palette-FX definition $8D:{definition.DefinitionPointer:X4} avoids statue " +
                "mechanics ROM reads");
            AssertEqual(
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount *
                    TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"palette-FX definition $8D:{definition.DefinitionPointer:X4} retains statue " +
                "color reads");
        }

        AssertTrue(!TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                0xe2e0,
                out _),
            "Tourian statue grey owner rejects adjacent pre-instruction code");
    }

    private static void VerifyWreckedShipGreenLightPaletteFxProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        var expected = new Dictionary<ushort, ushort>
        {
            [WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ProgramStart] =
                PaletteFxInstructionCodes.SetColorIndex,
            [unchecked((ushort)(
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ProgramStart + 2))] =
                0x0098,
            [WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer] =
                PaletteFxInstructionCodes.Goto,
            [unchecked((ushort)(
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.LoopInstructionPointer + 2))] =
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FirstFramePointer,
        };
        for (int frame = 0;
             frame < WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort pointer =
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            expected.Add(pointer, 10);
            expected.Add(
                unchecked((ushort)(pointer +
                    WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameByteCount - 2)),
                PaletteFxInstructionCodes.Wait);
            for (int color = 0;
                 color < WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                 color++)
            {
                ushort presentationPointer = unchecked((ushort)(
                    pointer + sizeof(ushort) + color * sizeof(ushort)));
                AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        presentationPointer,
                        out _),
                    $"Wrecked Ship green-light color $8D:{presentationPointer:X4} remains presentation-owned");
            }
        }

        AssertEqual(20, expected.Count, "Wrecked Ship green-light mechanics word count");
        foreach ((ushort pointer, ushort value) in expected)
        {
            AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    pointer,
                    out ushort compiled),
                $"Wrecked Ship green-light program catalogs $8D:{pointer:X4}");
            AssertEqual(value, compiled,
                $"Wrecked Ship green-light compiled word $8D:{pointer:X4}");
            AssertEqual(value, RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"Wrecked Ship green-light cartridge word $8D:{pointer:X4}");
        }

        foreach (ushort definition in new ushort[] { 0xf76d, 0xf771 })
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            paletteFx.SpawnDefinition(guarded, definition, equippedItems: 0);
            int steps = 1 +
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount * 10;
            for (int step = 0; step <= steps; step++)
            {
                paletteFx.Step(
                    guarded,
                    cgram, new ReferencePaletteFxColorSource(guarded),
                    samusY: 0,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"palette-FX definition $8D:{definition:X4} avoids green-light mechanics ROM reads");
            AssertEqual(
                (WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount + 1) *
                    WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"palette-FX definition $8D:{definition:X4} retains green-light color reads");
        }

        AssertTrue(!WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.
                TryReadMechanicsWord(0xeae0, out _),
            "Wrecked Ship green-light owner rejects adjacent code");
    }

    private static void VerifyPaletteFxHeatProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (PaletteFxHeatProgramDefinition definition in
                 PaletteFxHeatProgramMechanicsDefinitions.All)
        {
            ushort nativeStart = definition.Suit switch
            {
                PaletteFxHeatSuit.Power => 0xe45e,
                PaletteFxHeatSuit.Varia => 0xe68a,
                PaletteFxHeatSuit.Gravity => 0xe8b6,
                _ => throw new InvalidOperationException(),
            };
            AssertEqual(nativeStart, definition.ProgramStart, "Native suit setup entry");
            var expected = new Dictionary<ushort, ushort>();
            void Capture(int pointer) => expected.Add((ushort)pointer, ReadVerificationWord(bus, 0x8d0000 | pointer));
            for (int setup = nativeStart; setup < nativeStart + 8; setup += 2) Capture(setup);
            int cursor = nativeStart + 8;
            AssertEqual(16, definition.Frames.Count, "Native phase count");
            for (int phase = 0; phase < 16; phase++)
            {
                var frame = definition.Frames[phase];
                Capture(cursor);
                AssertEqual((ushort)cursor, frame.InstructionPointer, "Calculated frame address");
                AssertTrue(definition.TryReadMechanicsWord(frame.InstructionPointer, out ushort duration),
                    "Frame duration word is mechanics");
                AssertEqual(expected[(ushort)cursor], duration, "Original duration schedule");
                cursor += 2;
                AssertEqual((ushort)cursor, frame.FirstColorPointer, "Calculated first color address");
                for (int color = 0; color < 15; color++, cursor += 2)
                    AssertTrue(!definition.TryReadMechanicsWord((ushort)cursor, out _), "Color word is not mechanics");
                AssertEqual((ushort)cursor, frame.WaitInstructionPointer, "Calculated wait address");
                Capture(cursor);
                AssertEqual((ushort)0xc595, expected[(ushort)cursor], "Native terminal wait");
                cursor += 2;
            }
            AssertEqual((ushort)cursor, definition.LoopInstructionPointer, "Calculated terminal goto address");
            Capture(cursor);
            Capture(cursor + 2);
            for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            {
                bool owned = expected.TryGetValue((ushort)pointer, out ushort native);
                AssertEqual(owned, definition.TryReadMechanicsWord((ushort)pointer, out ushort actual),
                    "Every byte pointer has exact native mechanics ownership");
                AssertEqual(owned ? native : (ushort)0, actual, "Every owned value/unowned zero matches original");
            }
            foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = definition.Frames[invalid], "Frame list index bounds");

            AssertEqual(38, expected.Count, $"{definition.Suit} heat mechanics word count");
            foreach ((ushort pointer, ushort value) in expected)
            {
                AssertTrue(PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(
                        pointer,
                        out ushort compiled),
                    $"{definition.Suit} heat program catalogs $8D:{pointer:X4}");
                AssertEqual(value, compiled,
                    $"{definition.Suit} compiled heat word $8D:{pointer:X4}");
                AssertEqual(value, RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer),
                    $"{definition.Suit} cartridge heat word $8D:{pointer:X4}");
                mechanicsWords++;
            }

            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            ushort equippedItems = definition.Suit switch
            {
                PaletteFxHeatSuit.Power => 0,
                PaletteFxHeatSuit.Varia => (ushort)SamusEquipmentFlags.VariaSuit,
                PaletteFxHeatSuit.Gravity => (ushort)SamusEquipmentFlags.GravitySuit,
                _ => throw new InvalidOperationException(),
            };
            paletteFx.SpawnDefinition(guarded, definition: 0xf761, equippedItems);
            int steps = 1 + definition.Frames.Sum(frame =>
                definition.TryReadMechanicsWord(frame.InstructionPointer, out ushort duration)
                    ? duration
                    : throw new InvalidOperationException($"{definition.Suit} heat frame ${frame.InstructionPointer:X4} has no duration word"));
            for (int step = 0; step <= steps; step++)
            {
                paletteFx.Step(
                    guarded,
                    cgram, new ReferencePaletteFxColorSource(guarded),
                    samusY: 0,
                    equippedItems,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{definition.Suit} heat program performs no mechanics ROM reads");
            AssertEqual(
                (definition.Frames.Count + 1) *
                    PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort),
                guarded.PresentationReadCount,
                $"{definition.Suit} heat program retains live BGR555 reads through loop");
        }

        AssertEqual(114, mechanicsWords, "compiled Norfair heat program mechanics words");
        AssertTrue(!PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(0xe45c, out _),
            "Norfair heat owner rejects adjacent setup code");

    }

    private static void VerifyPaletteFxHeatInstructionListDefinitions(
        ISnesAddressSpace bus)
    {
        // Native LDA operands select each original table, independently of production
        // source-address helpers and the replacement program-stride calculation.
        var sources = new (PaletteFxHeatSuit Suit, int Operand)[]
        {
            (PaletteFxHeatSuit.Power, 0x8de3d6),
            (PaletteFxHeatSuit.Varia, 0x8de3d1),
            (PaletteFxHeatSuit.Gravity, 0x8de3c7),
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
                AssertEqual(PaletteFxInstructionCodes.Wait, ReadVerificationWord(bus, 0x8d0000 | (expected + 32)),
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
        for (int equipment = 0; equipment <= ushort.MaxValue; equipment++)
        {
            int operand = (equipment & gravityMask) != 0 ? 0x8de3c7 :
                (equipment & variaMask) != 0 ? 0x8de3d1 : 0x8de3d6;
            int table = 0x8d0000 | ReadVerificationWord(bus, operand);
            for (ushort phase = 0; phase < 16; phase++)
                AssertEqual(ReadVerificationWord(bus, table + 2 * phase),
                    PaletteFxHeatInstructionListDefinitions.ResolveForEquippedItems((ushort)equipment, phase),
                    "All equipment words preserve native Gravity-before-Varia selection at every phase");
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
        var paletteFx = new RoomPaletteFxSystem();
        var cgram = new SnesCgram();
        var samus = new SamusState { Health = 99 };
        paletteFx.SpawnDefinition(bus, definition: 0xf761, equippedItems: 0);
        paletteFx.SpawnDefinition(bus, definition: 0xf785, equippedItems: 0);

        int damageSoundCount = 0;
        for (ushort frame = 0; frame <= 17; frame++)
        {
            paletteFx.Step(
                bus,
                cgram, new ReferencePaletteFxColorSource(bus),
                samusY: samus.YPosition,
                equippedItems: samus.EquippedItems,
                enemyZeroIsDead: false,
                areaMiniBossDefeated: false,
                samus,
                nmiFrameCounter: frame);
            damageSoundCount += paletteFx.SoundRequests.Count;
            foreach (PaletteFxSoundRequest request in paletteFx.SoundRequests)
            {
                AssertEqual(SoundEffectLibrary3Sounds.EnvironmentalDamage,
                    request.SoundEffect,
                    "retail Norfair heat owner publishes native library-three sound");
                AssertEqual(PaletteFxHeatData.DamageSoundMaximumQueued,
                    request.MaximumQueued,
                    "retail Norfair heat owner uses native Max6 queue");
            }
            if (frame == 16)
            {
                AssertEqual((ushort)1, paletteFx.SamusInHeatPaletteIndex,
                    "$F785 publishes its second heat phase on its own slot pass");
                AssertEqual((ushort)0, paletteFx.PreviousSamusInHeatPaletteIndex,
                    "shared heat owner has not consumed the later slot's phase in the same frame");
            }
        }

        AssertEqual((ushort)0x0004, samus.LiquidPhysics.PeriodicDamage,
            "retail Norfair heat owner carries quarter-units into whole damage");
        AssertEqual((ushort)0x4000, samus.LiquidPhysics.PeriodicSubDamage,
            "retail Norfair heat owner retains exact 16.16 fractional carry");
        AssertEqual((ushort)1, paletteFx.PreviousSamusInHeatPaletteIndex,
            "shared heat owner consumes $F785 phase on following frame");
        AssertEqual(2, damageSoundCount,
            "retail Norfair heat owner publishes on frame-eight boundaries");
    }

    private sealed class PaletteFxMechanicsForbiddenBus(ISnesAddressSpace inner)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }
        public int PresentationReadCount { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            int presentationReadsBefore = PresentationReadCount;
            int first = RoomFxRomData.Banks.PaletteFx |
                PaletteFxHeatInstructionListDefinitions.GravitySourceTable;
            int lastExclusive = RoomFxRomData.Banks.PaletteFx |
                unchecked((ushort)(PaletteFxHeatInstructionListDefinitions.PowerSourceTable +
                    PaletteFxHeatInstructionListDefinitions.PhaseCount * sizeof(ushort)));
            SnesAddress source = SnesAddress.FromBusAddress(address);
            bool compiledProgramByte = source.Bank == 0x8d &&
                (RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    source.Offset,
                    out _) ||
                 RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                    unchecked((ushort)(source.Offset - 1)),
                    out _) ||
                 RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                    source.Offset,
                    out _));
            if (address >= first && address < lastExclusive || compiledProgramByte)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled palette-FX mechanic ${address:X6}.");
            }


            if (source.Bank == 0x8d)
            {
                foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in
                         TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0; frame < definition.FrameCount; frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FramePointer(frame) +
                        sizeof(ushort)));
                    if ((uint)offset <
                        NintendoLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < TitleLogoFadePaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        TitleLogoFadePaletteFxProgramMechanicsDefinitions.FramePointer(frame) +
                        sizeof(ushort)));
                    if ((uint)offset <
                        TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                         .GunshipEngineFrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                            .GunshipEngineFramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                            .GunshipEngineColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                         .NavigationLightsFrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                            .NavigationLightsFramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                            .NavigationLightsColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (PlanetZebesTextPaletteFxProgramDefinition definition in
                         PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset <
                            PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                            sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                foreach (CinematicGlowPaletteFxProgramDefinition definition in
                         CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        ExplodingZebesFadePaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                         .FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                         .FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (ZebesExplosionAmbientPaletteFxProgramDefinition definition in
                         ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0; frame < definition.FrameCount; frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition definition in
                         ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions
                             .FrameCount;
                         frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset <
                            ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions
                                .ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions
                         .FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                         .FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        UnusedCinematicFadePaletteFxProgramMechanicsDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in
                         SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset <
                            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
                                .ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions
                         .FrameCount;
                     frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions
                            .FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset <
                        PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions
                            .ColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (PaletteFxHeatProgramDefinition definition in
                         PaletteFxHeatProgramMechanicsDefinitions.All)
                {
                    foreach (PaletteFxHeatProgramFrameDefinition frame in definition.Frames)
                    {
                        int colorOffset = source.Offset - frame.FirstColorPointer;
                        if ((uint)colorOffset <
                            PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }


                for (int frame = 0;
                     frame < WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    ushort firstColor = unchecked((ushort)(
                        WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FramePointer(frame) +
                        sizeof(ushort)));
                    int colorOffset = source.Offset - firstColor;
                    if ((uint)colorOffset <
                        WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    ushort firstColor = unchecked((ushort)(
                        TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FramePointer(frame) +
                        sizeof(ushort)));
                    int colorOffset = source.Offset - firstColor;
                    if ((uint)colorOffset <
                        TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (TorizoBellyPaletteFxProgramDefinition definition in
                         TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        ushort firstColor = definition.ColorPointer(frame, 0);
                        int colorOffset = source.Offset - firstColor;
                        if ((uint)colorOffset <
                            TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                            sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in
                         BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        ushort firstColor = definition.ColorPointer(frame, 0);
                        int colorOffset = source.Offset - firstColor;
                        if ((uint)colorOffset <
                            BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                            sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    ushort firstColor =
                        RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorPointer(
                            frame, 0);
                    int colorOffset = source.Offset - firstColor;
                    if ((uint)colorOffset <
                        RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                        sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (CrateriaLightningPaletteFxProgramDefinition definition in
                         CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
                {
                    foreach (CrateriaLightningPaletteFrame frame in definition.Frames)
                    {
                        int colorOffset = source.Offset - frame.FirstColorPointer;
                        if ((uint)colorOffset < frame.ColorCount * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in
                         MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
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
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < TourianGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    bool presentationByte = false;
                    for (int color = 0;
                         color < TourianGlowPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                         color++)
                    {
                        int colorOffset = source.Offset -
                            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorPointer(
                                frame, color);
                        if ((uint)colorOffset >= sizeof(ushort))
                            continue;
                        presentationByte = true;
                        PresentationReadCount++;
                        break;
                    }
                    if (presentationByte)
                        break;
                }

                for (int frame = 0;
                     frame < BeaconPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    ushort framePointer =
                        BeaconPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
                    int leadingColorOffset = source.Offset - unchecked((ushort)(framePointer + 2));
                    if ((uint)leadingColorOffset < 3 * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                    int finalColorOffset = source.Offset - unchecked((ushort)(framePointer + 10));
                    if ((uint)finalColorOffset < sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in
                         NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        bool presentationByte = false;
                        for (int color = 0;
                             color < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
                             color++)
                        {
                            int colorOffset = source.Offset - definition.ColorPointer(frame, color);
                            if ((uint)colorOffset >= sizeof(ushort))
                                continue;
                            presentationByte = true;
                            PresentationReadCount++;
                            break;
                        }
                        if (presentationByte)
                            break;
                    }
                }

                foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in
                         TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0;
                     frame < TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    ushort framePointer =
                        TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
                    int leadingOffset = source.Offset - unchecked((ushort)(
                        framePointer + sizeof(ushort)));
                    if ((uint)leadingOffset < 6 * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                    int trailingOffset = source.Offset - unchecked((ushort)(
                        framePointer + 16));
                    if ((uint)trailingOffset < sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                for (int frame = 0;
                     frame < OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
                     frame++)
                {
                    ushort framePointer =
                        OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
                    int firstOffset = source.Offset - unchecked((ushort)(framePointer + 2));
                    int secondOffset = source.Offset - unchecked((ushort)(framePointer + 10));
                    int thirdOffset = source.Offset - unchecked((ushort)(framePointer + 20));
                    if ((uint)firstOffset < 3 * sizeof(ushort) ||
                        (uint)secondOffset < 4 * sizeof(ushort) ||
                        (uint)thirdOffset < sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in
                         OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0;
                         frame < OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount;
                         frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset <
                            OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                            sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                for (int frame = 0; frame < UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
                {
                    int offset = source.Offset - unchecked((ushort)(
                        UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame) + sizeof(ushort)));
                    if ((uint)offset < UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorsPerFrame * sizeof(ushort))
                    {
                        PresentationReadCount++;
                        break;
                    }
                }

                foreach (CrateriaEscapeLightningPaletteFxProgramDefinition definition in
                         CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
                {
                    for (int frame = 0; frame < CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
                    {
                        int offset = source.Offset - unchecked((ushort)(definition.FramePointer(frame) + sizeof(ushort)));
                        if ((uint)offset < definition.ColorsPerFrame * sizeof(ushort))
                        {
                            PresentationReadCount++;
                            break;
                        }
                    }
                }

                bool livePayloadRead = PresentationReadCount != presentationReadsBefore;
                if (!livePayloadRead)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Production read unclassified palette-FX byte ${address:X6}.");
                }
            }

            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }

    /// <summary>
    /// Reproduces the reported $F781 crash against its real retail bytecode. The assertion
    /// covers the exact native side effect and the odd one-byte operand/cursor advancement:
    /// if the interpreter accidentally advances a word, the following timed record fails.
    /// </summary>
    private static void VerifyBeaconSoundInstruction(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        var paletteFx = new RoomPaletteFxSystem();
        var cgram = new SnesCgram();
        paletteFx.SpawnDefinition(bus, definition: 0xf781, equippedItems: 0);

        int soundFrame = -1;
        PaletteFxSoundRequest soundRequest = default;
        for (int frame = 0; frame < 256; frame++)
        {
            paletteFx.Step(
                bus,
                cgram, new ReferencePaletteFxColorSource(bus),
                samusY: 0,
                equippedItems: 0,
                enemyZeroIsDead: false,
                areaMiniBossDefeated: false);
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
        AssertEqual((byte)0x18, soundRequest.SoundEffect.Value,
            "$F781 beacon sound preserves byte operand $18");
        AssertEqual(PaletteFxAudioQueueLimits.SoundEffects, soundRequest.MaximumQueued,
            "$F781 beacon sound uses QueueSfx2_Max6");

        // The next call must parse the timed palette record immediately after the byte
        // operand. This is the observable consequence of returning Y+1 at $8D:C67A.
        paletteFx.Step(
            bus,
            cgram, new ReferencePaletteFxColorSource(bus),
            samusY: 0,
            equippedItems: 0,
            enemyZeroIsDead: false,
            areaMiniBossDefeated: false);
        AssertEqual(0, paletteFx.SoundRequests.Count,
            "$F781 advances beyond its one-byte sound operand");
    }

    // Keeping this tiny helper avoids a name collision between the method and the catalog
    // in diagnostic stack traces while retaining the catalog's precise domain name.
    private static Type PaletteFxInstructionCodesType() => typeof(PaletteFxInstructionCodes);

    private static void AssertPaletteFxCatalog(
        Type catalog,
        int expectedCount,
        bool requireMappedPointers = true)
    {
        FieldInfo[] fields = GetUshortConstants(catalog);
        AssertEqual(expectedCount, fields.Length, $"{catalog.Name} exhaustive entry count");
        ushort[] pointers = fields
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{catalog.Name} contains no duplicate callbacks");
        if (requireMappedPointers)
        {
            foreach (ushort pointer in pointers)
                AssertTrue(pointer >= 0x8000, $"{catalog.Name} pointer ${pointer:X4} is mapped");
        }
    }
}
