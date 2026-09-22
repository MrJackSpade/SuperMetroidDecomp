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
        VerifyConstructedAudioInstructions();

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Palette FX: 37 instruction, setup, pre-instruction, and list " +
                "pointers are structurally valid; retail ROM reads skipped.");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
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

        VerifyBeaconSoundInstruction(bus);
        VerifyPaletteFxHeatInstructionListDefinitions(bus);
        VerifyPaletteFxHeatProgramMechanicsDefinitions(bus);
        VerifyWreckedShipGreenLightPaletteFxProgramMechanicsDefinitions(bus);
        VerifyTourianStatueGreyPaletteFxProgramMechanicsDefinitions(bus);
        VerifyTorizoBellyPaletteFxProgramMechanicsDefinitions(bus);
        VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions(bus);
        VerifyRedBrinstarGlowPaletteFxProgramMechanicsDefinitions(bus);
        VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions(bus);
        var guardedHeatBus = new PaletteFxMechanicsForbiddenBus(bus);
        VerifyNorfairHeatPaletteHandshake(guardedHeatBus);
        AssertEqual(0, guardedHeatBus.ForbiddenReadAttempts,
            "Norfair heat pre-instruction performs no selector-table ROM reads");
        VerifyNorfairGlowCycles(bus);
        VerifyTitleGradientTables(bus);

        Console.WriteLine(
            "  Palette FX: all 37 code/list pointers are ROM-readable; 48 heat selectors " +
            "plus 377 control words and four timer bytes are compiled; all four audio opcodes " +
            "and retail $F781's byte/cursor handoff agree.");
    }

    private static void VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions(
        SuperMetroidAddressSpace bus)
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
                        bus,
                        RoomFxRomData.Banks.PaletteFx | word.Pointer),
                    $"{definition.Owner} cartridge word $8D:{word.Pointer:X4}");
                mechanicsWords++;
            }
            foreach (PaletteFxMechanicsByte item in definition.MechanicsBytes)
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
            foreach (CrateriaLightningPaletteFrame frame in definition.Frames)
            {
                for (int color = 0; color < frame.ColorCount; color++)
                {
                    ushort pointer = unchecked((ushort)(
                        frame.FirstColorPointer + color * sizeof(ushort)));
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
                    cgram,
                    samusY: 0x0380,
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
            resetFx.Step(resetGuard, new SnesCgram(), 0x0380, 0, false, false);
            resetFx.Step(resetGuard, new SnesCgram(), 0x037f, 0, false, false);
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
                ushort presentationPointer = unchecked((ushort)(
                    pointer + sizeof(ushort) + color * sizeof(ushort)));
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
                    bus,
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"Red Brinstar glow cartridge word $8D:{pointer:X4}");
        }

        var guarded = new PaletteFxMechanicsForbiddenBus(bus);
        var paletteFx = new RoomPaletteFxSystem();
        var cgram = new SnesCgram();
        const ushort definition = 0xf77d;
        paletteFx.SpawnDefinition(guarded, definition, equippedItems: 0);
        const int cycleFrames =
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FrameCount * 10;
        for (int step = 0; step <= cycleFrames; step++)
        {
            paletteFx.Step(
                guarded,
                cgram,
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
                    ushort presentationPointer = unchecked((ushort)(
                        pointer + sizeof(ushort) + color * sizeof(ushort)));
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
                        bus,
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
                    cgram,
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
                cgram,
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
                    ushort presentationPointer = unchecked((ushort)(
                        pointer + sizeof(ushort) + color * sizeof(ushort)));
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
                        bus,
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
                    cgram,
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
                cgram,
                samusY: 0,
                equippedItems: 0,
                enemyZeroIsDead: true,
                areaMiniBossDefeated: false);
            AssertTrue(!paletteFx.IsDefinitionActive(definition.DefinitionPointer),
                $"{definition.Owner} belly pre-instruction deletes when enemy zero dies");
        }

        AssertEqual(36, mechanicsWords, "compiled Torizo belly mechanics words");
        AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(0xe2e0, out _),
            "Torizo belly owner rejects adjacent pre-instruction code");
        AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(0xe379, out _),
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
                ushort presentationPointer = unchecked((ushort)(
                    pointer + sizeof(ushort) + color * sizeof(ushort)));
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
                    bus,
                    RoomFxRomData.Banks.PaletteFx | pointer),
                $"Tourian statue grey cartridge word $8D:{pointer:X4}");
        }

        ushort[] definitions = [0xf749, 0xf74d, 0xf751, 0xf755];
        foreach (ushort definition in definitions)
        {
            var guarded = new PaletteFxMechanicsForbiddenBus(bus);
            var paletteFx = new RoomPaletteFxSystem();
            paletteFx.SpawnDefinition(guarded, definition, equippedItems: 0);
            for (int step = 0; step <= 1 +
                 TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount * 8; step++)
            {
                paletteFx.Step(
                    guarded,
                    new SnesCgram(),
                    samusY: 0,
                    equippedItems: 0,
                    enemyZeroIsDead: false,
                    areaMiniBossDefeated: false);
            }

            AssertTrue(!paletteFx.IsDefinitionActive(definition),
                $"palette-FX definition $8D:{definition:X4} reaches compiled deletion");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"palette-FX definition $8D:{definition:X4} avoids statue mechanics ROM reads");
            AssertEqual(
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FrameCount *
                    TourianStatueGreyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame *
                    sizeof(ushort),
                guarded.PresentationReadCount,
                $"palette-FX definition $8D:{definition:X4} retains statue color reads");
        }

        AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(0xe2e0, out _),
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
                    bus,
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
                    cgram,
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

        AssertTrue(!RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(0xeae0, out _),
            "Wrecked Ship green-light owner rejects adjacent code");
    }

    private static void VerifyPaletteFxHeatProgramMechanicsDefinitions(
        ISnesAddressSpace bus)
    {
        int mechanicsWords = 0;
        foreach (PaletteFxHeatProgramDefinition definition in
                 PaletteFxHeatProgramMechanicsDefinitions.All)
        {
            var expected = new Dictionary<ushort, ushort>
            {
                [definition.ProgramStart] = PaletteFxInstructionCodes.SetPreInstruction,
                [unchecked((ushort)(definition.ProgramStart + 2))] =
                    PaletteFxPreInstructionCodes.Heat,
                [unchecked((ushort)(definition.ProgramStart + 4))] =
                    PaletteFxInstructionCodes.SetColorIndex,
                [unchecked((ushort)(definition.ProgramStart + 6))] = 0x0182,
                [definition.LoopInstructionPointer] = PaletteFxInstructionCodes.Goto,
                [unchecked((ushort)(definition.LoopInstructionPointer + 2))] =
                    definition.Frames[0].InstructionPointer,
            };
            foreach (PaletteFxHeatProgramFrameDefinition frame in definition.Frames)
            {
                expected.Add(frame.InstructionPointer, frame.Duration);
                expected.Add(frame.WaitInstructionPointer, PaletteFxInstructionCodes.Wait);
                for (int color = 0;
                     color < PaletteFxHeatProgramDefinition.ColorsPerFrame;
                     color++)
                {
                    ushort presentationPointer = unchecked((ushort)(
                        frame.FirstColorPointer + color * sizeof(ushort)));
                    AssertTrue(!PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(
                            presentationPointer,
                            out _),
                        $"{definition.Suit} heat color ${presentationPointer:X4} remains presentation-owned");
                }
            }

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
                        bus,
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
            int steps = 1 + definition.Frames.Sum(frame => frame.Duration);
            for (int step = 0; step <= steps; step++)
            {
                paletteFx.Step(
                    guarded,
                    cgram,
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

        _ = PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(0xe45e, out _);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        ushort checksum = 0;
        for (int iteration = 0; iteration < 65_536; iteration++)
        {
            if (PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(
                    (ushort)(0xe45e + (iteration & 0x7ff)),
                    out ushort value))
            {
                checksum ^= value;
            }
        }
        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(checksum);
        AssertEqual(0L, allocatedAfter - allocatedBefore,
            "warmed Norfair heat program lookup allocates no managed memory");
    }

    private static void VerifyPaletteFxHeatInstructionListDefinitions(
        ISnesAddressSpace bus)
    {
        PaletteFxHeatSuit[] suits = Enum.GetValues<PaletteFxHeatSuit>();
        AssertEqual(3, suits.Length, "Norfair heat suit domain count");
        foreach (PaletteFxHeatSuit suit in suits)
        {
            for (ushort phase = 0;
                 phase < PaletteFxHeatInstructionListDefinitions.PhaseCount;
                 phase++)
            {
                ushort source = PaletteFxHeatInstructionListDefinitions.NativeSourceAddress(
                    suit,
                    phase);
                ushort expected = RomDataReader.ReadWordFixedBank(
                    bus,
                    RoomFxRomData.Banks.PaletteFx | source);
                AssertEqual(expected,
                    PaletteFxHeatInstructionListDefinitions.Resolve(suit, phase),
                    $"{suit} heat program phase {phase}");
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => PaletteFxHeatInstructionListDefinitions.Resolve(
                PaletteFxHeatSuit.Power,
                PaletteFxHeatInstructionListDefinitions.PhaseCount),
            "Norfair heat selector rejects phase sixteen");
        AssertThrows<ArgumentOutOfRangeException>(
            () => PaletteFxHeatInstructionListDefinitions.Resolve(
                (PaletteFxHeatSuit)3,
                0),
            "Norfair heat selector rejects unknown suit");
        AssertEqual(
            PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Power, 7),
            PaletteFxHeatInstructionListDefinitions.ResolveForEquippedItems(0, 7),
            "Norfair heat selector uses Power Suit without protection bits");
        AssertEqual(
            PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Varia, 7),
            PaletteFxHeatInstructionListDefinitions.ResolveForEquippedItems(
                (ushort)SamusEquipmentFlags.VariaSuit,
                7),
            "Norfair heat selector uses Varia Suit when equipped");
        AssertEqual(
            PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Gravity, 7),
            PaletteFxHeatInstructionListDefinitions.ResolveForEquippedItems(
                (ushort)SamusEquipmentFlags.GravitySuit,
                7),
            "Norfair heat selector uses Gravity Suit when equipped");
        AssertEqual(
            PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Gravity, 7),
            PaletteFxHeatInstructionListDefinitions.ResolveForEquippedItems(
                (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
                7),
            "Norfair heat selector gives Gravity Suit native priority");

        _ = PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Power, 0);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        ushort checksum = 0;
        for (int iteration = 0; iteration < 65_536; iteration++)
        {
            checksum ^= PaletteFxHeatInstructionListDefinitions.Resolve(
                (PaletteFxHeatSuit)(iteration % 3),
                (ushort)(iteration & 15));
        }
        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(checksum);
        AssertEqual(0L, allocatedAfter - allocatedBefore,
            "warmed Norfair heat selector allocates no managed memory");
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
                cgram,
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
        : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
        public int PresentationReadCount { get; private set; }

        public byte ReadByte(int address)
        {
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
                        ushort firstColor = unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
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
                        ushort firstColor = unchecked((ushort)(
                            definition.FramePointer(frame) + sizeof(ushort)));
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
                    ushort firstColor = unchecked((ushort)(
                        RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.FramePointer(frame) +
                        sizeof(ushort)));
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
            }

            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }

    private static void VerifyConstructedAudioInstructions()
    {
        VerifyConstructedSoundInstruction(
            PaletteFxInstructionCodes.QueueSfx1,
            SoundEffectLibrary.Library1,
            sound: 0x11);
        VerifyConstructedSoundInstruction(
            PaletteFxInstructionCodes.QueueSfx2,
            SoundEffectLibrary.Library2,
            sound: 0x22);
        VerifyConstructedSoundInstruction(
            PaletteFxInstructionCodes.QueueSfx3,
            SoundEffectLibrary.Library3,
            sound: 0x33);

        (RoomPaletteFxSystem paletteFx, TestAddressSpace bus) =
            CreateSingleAudioInstruction(PaletteFxInstructionCodes.QueueMusic, operand: 0x05);
        paletteFx.Step(bus, new SnesCgram(), 0, 0, false, false);
        AssertEqual(1, paletteFx.MusicRequests.Count,
            "constructed palette-FX music opcode publishes once");
        AssertEqual(MusicCommand.SelectTrack(5), paletteFx.MusicRequests[0].Command,
            "constructed palette-FX music opcode preserves its byte operand");
        AssertEqual(MusicCommandDelay.EightFrames, paletteFx.MusicRequests[0].Delay,
            "constructed palette-FX music opcode selects QueueMusic_Delayed8");
    }

    private static void VerifyConstructedSoundInstruction(
        ushort instruction,
        SoundEffectLibrary expectedLibrary,
        byte sound)
    {
        (RoomPaletteFxSystem paletteFx, TestAddressSpace bus) =
            CreateSingleAudioInstruction(instruction, sound);
        paletteFx.Step(bus, new SnesCgram(), 0, 0, false, false);
        AssertEqual(1, paletteFx.SoundRequests.Count,
            $"constructed {expectedLibrary} palette-FX sound opcode publishes once");
        AssertEqual(new SoundEffectId(expectedLibrary, sound),
            paletteFx.SoundRequests[0].SoundEffect,
            $"constructed {expectedLibrary} palette-FX opcode preserves its byte operand");
        AssertEqual(PaletteFxAudioQueueLimits.SoundEffects,
            paletteFx.SoundRequests[0].MaximumQueued,
            $"constructed {expectedLibrary} palette-FX opcode selects Max6");
    }

    private static (RoomPaletteFxSystem PaletteFx, TestAddressSpace Bus)
        CreateSingleAudioInstruction(ushort instruction, byte operand)
    {
        const ushort instructionList = 0x8f00;
        var bus = new TestAddressSpace();
        bus.WriteBytes(
            0x8d0000 | instructionList,
            [
                unchecked((byte)instruction),
                (byte)(instruction >> 8),
                operand,
                0x01, 0x00,
                unchecked((byte)PaletteFxInstructionCodes.Wait),
                (byte)(PaletteFxInstructionCodes.Wait >> 8),
            ]);

        var paletteFx = new RoomPaletteFxSystem();
        paletteFx.SpawnConstructedProgramForVerification(instructionList);
        return (paletteFx, bus);
    }

    /// <summary>
    /// Reproduces the reported $F781 crash against its real retail bytecode. The assertion
    /// covers the exact native side effect and the odd one-byte operand/cursor advancement:
    /// if the interpreter accidentally advances a word, the following timed record fails.
    /// </summary>
    private static void VerifyBeaconSoundInstruction(SuperMetroidAddressSpace bus)
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
                cgram,
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
            cgram,
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
