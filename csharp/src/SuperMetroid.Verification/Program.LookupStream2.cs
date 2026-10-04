using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream2TubeRamp(ISnesAddressSpace rom)
    {
        PaletteRgb5[] Read(int address) => Enumerable.Range(0, 32).Select(index =>
        {
            ushort word = ReadVerificationWord(rom, address + index * 2);
            return new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }).ToArray();
        var original = new ChozoAndTubeColorDocument { Version = 1,
            TubeCracks = Read(ChozoAndTubeColorRomData.TubeCracksSource),
            WreckedShip = Read(ChozoAndTubeColorRomData.WreckedShipSource),
            LowerNorfair = Read(ChozoAndTubeColorRomData.LowerNorfairSource) };
        ChozoAndTubeColorCatalog Load(ChozoAndTubeColorDocument document) =>
            ChozoAndTubeColorCatalog.Load(new MemoryStream(ChozoAndTubeColorCatalog.Write(document), writable: false));
        var stock = Load(original);
        Check(stock, original);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertEqual(8, ((ushort[])typeof(ChozoAndTubeColorCatalog).GetField("tubeColorSeeds", flags)!.GetValue(stock)!).Length,
            "tube stores only eight unresolved independent colors");
        AssertEqual(0, ((System.Collections.IDictionary)typeof(ChozoAndTubeColorCatalog).GetField("tubeColorEdits", flags)!.GetValue(stock)!).Count,
            "stock tube ramp and repeated half need no residuals");
        for (int color = 0; color < 32; color++)
        {
            var changed = (PaletteRgb5[])original.TubeCracks.Clone();
            changed[color] = changed[color] with { Red = (changed[color].Red + 1) % 32 };
            var document = original with { TubeCracks = changed };
            Check(Load(document), document);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveTubeCracks(-1), "tube negative color");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveTubeCracks(32), "tube past last color");
        Console.WriteLine("Tube palette:32 native colors, full CGRAM writes,32 independent edits, unchanged statue palettes and canonical identity pass; eight seed colors remain unresolved.");

        static void Check(ChozoAndTubeColorCatalog catalog, ChozoAndTubeColorDocument document)
        {
            static ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            var cgram = new SnesCgram();
            catalog.ApplyTubeCracks(cgram);
            for (int color = 0; color < 32; color++)
            {
                ushort word = Word(document.TubeCracks[color]);
                AssertEqual(word, catalog.ResolveTubeCracks(color), "independent tube color preserved");
                AssertEqual(word, cgram.Colors[ChozoAndTubeColorRomData.Destination + color], "tube color reaches CGRAM");
                AssertEqual(Word(document.WreckedShip[color]), catalog.ResolveWreckedShip(color), "Wrecked Ship palette preserved");
                AssertEqual(Word(document.LowerNorfair[color]), catalog.ResolveLowerNorfair(color), "Lower Norfair palette preserved");
            }
            string expected = SelectedPresentationHash.Create("ChozoAndTubeColorCatalog-v1", content =>
            {
                content.AppendWords("tubeCracks", document.TubeCracks.Select(Word).ToArray());
                content.AppendWords("wreckedShip", document.WreckedShip.Select(Word).ToArray());
                content.AppendWords("lowerNorfair", document.LowerNorfair.Select(Word).ToArray());
            });
            AssertEqual(expected, catalog.ContentIdentity, "tube canonical identity preserved");
        }
    }
    private static void VerifyLookupStream2PauseOwnership(ISnesAddressSpace rom)
    {
        for (int cell = -1; cell <= PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            bool equipment = false;
            for (int index = 0; index < 14; index++)
            {
                int start = (ReadVerificationWord(rom, 0x82c06c + 2 * index) - 0x3800) / 2;
                equipment |= (uint)(cell - start) < 9;
            }
            bool reserve = (uint)(cell - PauseReserveUiDefinitions.DigitCell) < PauseReserveUiDefinitions.SupplyDigitPlaces;
            for (int index = 0; index < 2; index++)
            {
                int start = (ReadVerificationWord(rom, 0x82c068 + 2 * index) - 0x3800) / 2;
                reserve |= (uint)(cell - start) < 7;
            }
            int relative = cell - PauseWireframeDefinitions.DestinationByte / 2;
            bool wireframe = relative >= 0 && relative / (PauseWireframeDefinitions.DestinationStride / 2) < PauseWireframeDefinitions.Rows &&
                relative % (PauseWireframeDefinitions.DestinationStride / 2) < PauseWireframeDefinitions.Columns;
            AssertEqual(equipment, PauseEquipmentBaseDefinitions.IsEquipmentLabelCell(cell), "native equipment ownership footprint");
            AssertEqual(equipment || reserve || wireframe, PauseEquipmentBaseDefinitions.IsLiveOwnedCell(cell), "native live ownership footprint");
            AssertEqual((reserve || wireframe) && !equipment, PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell), "native noninventory footprint");
        }
        Console.WriteLine("Pause ownership: all1024 cells plus rejected outer bounds match native label/reserve destinations, including the nine-cell Plasma overlap.");
    }
    private static void VerifyLookupStream2DeadTorizoTransfers(ISnesAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        var state = new DeadTorizoEnemyState(enemies.Slots[0], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        var build = typeof(RoomEnemySystem).GetMethod("BuildDeadTorizoVramTransfers",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<DeadTorizoEnemyState>>(enemies);
        for (ushort phase = 0; phase < 2; phase++)
        {
            var rows = DeadTorizoVramTransferDefinitions.ForPhase(phase);
            ushort table = phase == 0 ? DeadTorizoVramTransferDefinitions.EvenTable : DeadTorizoVramTransferDefinitions.OddTable;
            AssertEqual(7, rows.Length, "Dead Torizo six body rows plus sand strip");
            int count = 0;
            foreach (var row in rows)
            {
                int address = 0xa90000 | table + 8 * count;
                AssertEqual(ReadVerificationWord(rom, address), row.SizeInBytes, "Dead Torizo native DMA size");
                AssertEqual(ReadVerificationWord(rom, address + 2), row.SourceBankWord, "Dead Torizo native DMA bank");
                AssertEqual(ReadVerificationWord(rom, address + 4), row.SourceOffset, "Dead Torizo native DMA source");
                AssertEqual(ReadVerificationWord(rom, address + 6), row.EncodedVramDestination, "Dead Torizo native DMA destination");
                AssertEqual(rows[count++], row, "Dead Torizo enumeration order");
            }
            AssertEqual(7, count, "Dead Torizo enumeration count");
            AssertEqual((ushort)0, ReadVerificationWord(rom, 0xa90000 | table + count * 8), "Dead Torizo native terminator");
            state.VramTransferPhase = unchecked((ushort)(phase - 1));
            build(state);
            AssertEqual(phase, state.VramTransferPhase, "Dead Torizo phase advancement");
            for (int index = 0; index < rows.Length; index++)
            {
                var actual = enemies.LastDeadTorizoVramTransfers[phase * 7 + index];
                AssertEqual(rows[index].SizeInBytes, actual.SizeInBytes, "Dead Torizo queued size");
                AssertEqual(rows[index].SourceAddress, actual.SourceAddress, "Dead Torizo queued live WRAM source");
                AssertEqual(rows[index].EncodedVramDestination, actual.EncodedVramDestination, "Dead Torizo queued VRAM destination");
                AssertEqual(rows[index], DeadTorizoVramTransferDefinitions.ForPhase((ushort)(phase + 2))[index], "Dead Torizo phase parity");
            }
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[-1]; }, "Dead Torizo negative row");
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[7]; }, "Dead Torizo past last row");
        }
        Console.WriteLine("Dead Torizo: all56 native descriptor fields, two terminators, actual fourteen queued live-WRAM transfers, phase parity, enumeration and bounds pass.");
    }
    private static void VerifyLookupStream2GunshipTransfers(ISnesAddressSpace rom)
    {
        var transfers = GunshipLiftoffTransferDefinitions.Frames;
        AssertEqual(5, transfers.Count, "Gunship calculated transfer count");
        var enemies = new RoomEnemySystem();
        var slot = enemies.Slots[0];
        var queue = new VramWriteQueue();
        var append = typeof(RoomEnemySystem).GetMethod("QueueGunshipTakeoffTiles",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, VramWriteQueue>>(enemies);
        int enumerated = 0;
        foreach (var transfer in transfers)
        {
            int source = 0x940000 | ReadVerificationWord(rom, 0xa2ac07 + 2 * enumerated);
            ushort destination = ReadVerificationWord(rom, 0xa2ac11 + 2 * enumerated);
            AssertEqual(source, transfer.SourceAddress, "Gunship original source");
            AssertEqual(destination, transfer.DestinationWord, "Gunship original destination");
            AssertEqual(VramAssetId.GunshipLiftoffFirstTiles + enumerated, transfer.Asset, "Gunship typed transfer identity");
            AssertEqual(transfers[enumerated], transfer, "Gunship enumeration and indexing agree");
            append(slot, queue);
            AssertEqual(destination, queue.Entries[enumerated].EncodedVramDestination, "Gunship actual queued destination");
            AssertEqual(source, queue.Entries[enumerated].SourceAddress, "Gunship actual queued source");
            enumerated++;
        }
        AssertEqual(5, enumerated, "Gunship enumerates every transfer");
        AssertEqual((ushort)0, slot.VariableB, "Gunship completes transfer phase");
        AssertEqual(GunshipCodePointers.FireUpEngines, slot.VariableF, "Gunship starts engines after fifth transfer");
        AssertThrows<IndexOutOfRangeException>(() => _ = transfers[-1], "Gunship lower transfer bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = transfers[5], "Gunship upper transfer bound");
        Console.WriteLine("Gunship transfers: all10 native source/destination fields, five typed identities, actual queued uploads/phase handoff, enumeration and bounds pass.");
    }
    private static void VerifyLookupStream2ReserveGeometry(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseReserveUiDocument>(source, MapPresentationFormat.JsonOptions)!;
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(source));
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertTrue(typeof(PauseReserveUiPresentation).GetField("digits", fields)!.GetValue(stock) is null, "Stock digits discard stored lookup");
        AssertTrue(typeof(PauseReserveUiPresentation).GetField("arrowOffsets", fields)!.GetValue(stock) is null, "Stock arrow discards stored offsets");
        Check(stock, document);
        for (int index = 0; index < 10; index++)
        {
            var digits = document.Digits.Cells.ToArray();
            digits[index] = digits[index] with { FlipY = !digits[index].FlipY };
            var changedDocument = document with { Digits = document.Digits with { Cells = digits, Anchor = new() { Column = 18, Row = 20 } } };
            using var encoded = new MemoryStream();
            PauseReserveUiPresentation.Write(encoded, changedDocument);
            encoded.Position = 0;
            Check(PauseReserveUiPresentation.Load(encoded), changedDocument);
            var cells = document.Arrow.Cells.ToArray();
            cells[index] = new() { Column = 20 + index, Row = 22 };
            changedDocument = document with { Arrow = document.Arrow with { Cells = cells, EnabledPalette = 3, DisabledPalette = 4 } };
            encoded.SetLength(0);
            PauseReserveUiPresentation.Write(encoded, changedDocument);
            encoded.Position = 0;
            Check(PauseReserveUiPresentation.Load(encoded), changedDocument);
        }
        Console.WriteLine("Reserve geometry: all10 stock digits/10 arrow cells, actual tilemap writes, all20 independent edits, supplied anchors/palettes and unrelated tile fields pass.");

        static void Check(PauseReserveUiPresentation presentation, PauseReserveUiDocument expected)
        {
            for (int value = 0; value < 10; value++)
            for (int place = 0; place < 3; place++)
            {
                byte[] tilemap = new byte[2048];
                presentation.ApplyDigit(tilemap, place, value);
                int offset = 2 * (32 * expected.Digits.Anchor.Row + expected.Digits.Anchor.Column + place);
                byte[] word = PauseTileGrid.Compile([expected.Digits.Cells[value]], "reserve verification");
                AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(word),
                    System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tilemap.AsSpan(offset)), "Digit actual complete tile word and supplied anchor");
            }
            foreach (bool enabled in new[] { false, true })
            {
                byte[] tilemap = new byte[2048];
                for (int cell = 0; cell < 1024; cell++)
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(tilemap.AsSpan(2 * cell), 0xe355);
                presentation.ApplyArrowTilePalettes(tilemap, enabled);
                for (int cell = 0; cell < 1024; cell++)
                {
                    bool arrow = expected.Arrow.Cells.Any(point => 32 * point.Row + point.Column == cell);
                    ushort word = arrow ? (ushort)((0xe355 & ~0x1c00) | ((enabled ? expected.Arrow.EnabledPalette : expected.Arrow.DisabledPalette) << 10)) : (ushort)0xe355;
                    AssertEqual(word, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tilemap.AsSpan(2 * cell)), "Arrow updates exactly supplied cells and preserves tile/flip/priority fields");
                }
            }
        }
    }
    private static void VerifyLookupStream2GhostAndNorfair(ISnesAddressSpace rom)
    {
        const System.Reflection.BindingFlags methods = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnWreckedShipGhost", methods)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState, ushort, ushort>>();
        var flicker = typeof(RoomEnemySystem).GetMethod("AdvanceWreckedShipGhostFlicker", methods)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new WreckedShipGhostEnemyState(slot);
        for (int index = 0; index < 9; index++)
        {
            short originalX = unchecked((short)ReadVerificationWord(rom, 0xa89aa8 + 4 * index));
            short originalY = unchecked((short)ReadVerificationWord(rom, 0xa89aaa + 4 * index));
            var calculated = WreckedShipGhostAppearanceDefinitions.SpawnOffset(index);
            AssertEqual(originalX, calculated.X, "Ghost original horizontal spawn offset");
            AssertEqual(originalY, calculated.Y, "Ghost original vertical spawn offset");
            state.HorizontalMovementClass = (ushort)(4 * (index % 3));
            state.VerticalMovementClass = (ushort)(12 * (index / 3));
            spawn(slot, state, 32, 65520);
            AssertEqual(unchecked((ushort)(32 + originalX)), slot.XPosition, "Ghost actual wrapped spawn X");
            AssertEqual(unchecked((ushort)(65520 + originalY)), slot.YPosition, "Ghost actual wrapped spawn Y");
            AssertEqual(WreckedShipGhostAiFunction.BrighteningAndFlickering, state.Function, "Ghost appearance phase");
            AssertEqual((ushort)64, state.StablePositionTimer, "Ghost appearance position timer");
            AssertEqual((ushort)16, state.StableDirectionTimer, "Ghost appearance direction timer");
        }
        for (int offset = 0; offset < 34; offset++)
        {
            short original = unchecked((short)ReadVerificationWord(rom, 0xa89acc + 2 * (offset / 2)));
            AssertEqual(original, WreckedShipGhostAppearanceDefinitions.FlickerDuration(offset / 2), "Ghost original flicker interval");
            slot.Properties = (ushort)(EnemyProperties.Invisible | EnemyProperties.ProcessOffScreen);
            state.PhaseTimer = 1;
            state.FlickerTableOffset = (ushort)offset;
            flicker(slot, state);
            AssertEqual(original < 0 ? (ushort)0 : (ushort)original, state.PhaseTimer, "Ghost actual next interval");
            AssertEqual(original < 0 ? (ushort)0 : (ushort)(offset + 2), state.FlickerTableOffset, "Ghost actual next offset including odd folding");
            bool remainsInvisible = original < 0 || (offset & 2) != 0;
            AssertEqual((ushort)(EnemyProperties.ProcessOffScreen | (remainsInvisible ? EnemyProperties.Invisible : EnemyProperties.None)), slot.Properties, "Ghost actual visibility preserves unrelated flags");
        }
        state.PhaseTimer = 2;
        state.FlickerTableOffset = 2;
        slot.Properties = (ushort)EnemyProperties.Invisible;
        flicker(slot, state);
        AssertEqual((ushort)1, state.PhaseTimer, "Ghost interval countdown");
        AssertEqual((ushort)2, state.FlickerTableOffset, "Ghost no early interval advance");
        AssertEqual((ushort)EnemyProperties.Invisible, slot.Properties, "Ghost no early visibility change");
        state.PhaseTimer = 0;
        flicker(slot, state);
        AssertEqual((ushort)EnemyProperties.None, slot.Properties, "Ghost terminal next call clears invisibility");
        state.PhaseTimer = 1;
        state.FlickerTableOffset = 34;
        AssertThrows<InvalidDataException>(() => flicker(slot, state), "Ghost malformed offset remains rejected");
        state.HorizontalMovementClass = 1;
        state.VerticalMovementClass = 0;
        AssertThrows<InvalidDataException>(() => spawn(slot, state, 0, 0), "Ghost unaligned spawn class remains rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(-1), "Ghost spawn lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(9), "Ghost spawn upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(-1), "Ghost flicker lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(17), "Ghost flicker upper bound");
        foreach (var definition in NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            int total = 0;
            for (int frame = 0; frame < 16; frame++)
            {
                ushort pointer = (ushort)(definition.FramePointer(frame) + (definition.PublishesHeatPhase ? 3 : 0));
                ushort original = ReadVerificationWord(rom, 0x8d0000 | pointer);
                AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort duration), "Norfair duration mechanic resolves");
                AssertEqual(original, duration, "Norfair original duration");
                total += duration;
            }
            AssertEqual(116, total, "Norfair exact cycle duration");
        }
        AssertThrows<IndexOutOfRangeException>(() => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(-1), "Norfair duration lower bound");
        AssertThrows<IndexOutOfRangeException>(() => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(16), "Norfair duration upper bound");
        Console.WriteLine("Ghost appearance: 18 native offset fields, 17 native intervals, actual wrapped positions/timing/visibility and malformed states pass; Norfair: 64 native durations and four 116-tick cycles pass.");
    }
    private static void VerifyLookupStream2PaletteMechanics(ISnesAddressSpace rom)
    {
        int upperWords = 0, oldWords = 0, bellyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort upper))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), upper, "Upper Crateria mechanics original word");
                upperWords++;
            }
            if (OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort old))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), old, "Old Tourian mechanics original word");
                oldWords++;
            }
            if (TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort belly))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), belly, "Torizo belly mechanics original word");
                bellyWords++;
            }
        }
        AssertEqual(32, upperWords, "Upper Crateria complete mechanics coverage");
        AssertEqual(60, oldWords, "Old Tourian complete mechanics coverage");
        AssertEqual(36, bellyWords, "Torizo belly complete mechanics coverage");
        int upperDuration = 0;
        for (int frame = 0; frame < 14; frame++)
        {
            ushort pointer = UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            AssertEqual((ushort)(0xfd01 + 18 * frame), pointer, "Upper Crateria frame address");
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort duration);
            upperDuration += duration;
            for (int color = 0; color < 8; color++)
            {
                int[] nativeOffsets = [2, 4, 6, 10, 12, 14, 16, 20];
                ushort actual = OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
                AssertEqual((ushort)(0xfa6d + 24 * frame + nativeOffsets[color]), actual, "Old Tourian color address around inline skips");
                AssertTrue(!OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(actual, out _), "Supplied old Tourian colors remain live");
            }
        }
        AssertEqual(63, upperDuration, "Upper Crateria cycle duration");
        var definitions = TorizoBellyPaletteFxProgramMechanicsDefinitions.All;
        AssertEqual(2, definitions.Count, "Both Torizo programs enumerated");
        for (int index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            AssertEqual((TorizoBellyPaletteOwner)index, definition.Owner, "Torizo program owner ordering");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.DefinitionPointer + 2), definition.ProgramStart, "Original Torizo definition list operand");
            int total = 0;
            for (int frame = 0; frame < 6; frame++)
            {
                definition.TryReadMechanicsWord(definition.FramePointer(frame), out ushort duration);
                total += duration;
                for (int color = 0; color < 3; color++)
                    AssertTrue(!definition.TryReadMechanicsWord(definition.ColorPointer(frame, color), out _), "Supplied Torizo colors remain live");
            }
            AssertEqual(52, total, "Torizo belly cycle duration");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(6), "Torizo frame upper bound");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(-1), "Torizo frame lower bound");
        }
        int enumerated = 0;
        foreach (var definition in definitions)
        {
            AssertEqual(definitions[enumerated].Owner, definition.Owner, "Torizo enumerated owner");
            enumerated++;
        }
        AssertEqual(2, enumerated, "Torizo definition enumeration");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[-1]; }, "Torizo definition lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[2]; }, "Torizo definition upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(-1), "Upper Crateria lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(14), "Upper Crateria upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(0, -1), "Old Tourian lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(0, 8), "Old Tourian upper color bound");
        Console.WriteLine("Stream 2 palette mechanics: original mechanics, inline color offsets, durations, dispatch and bounds pass.");
    }
    private static void VerifyLookupStream2CrystalBody(ISnesAddressSpace rom)
    {
        byte[] source = CrystalFlashColorExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<CrystalFlashColorDocument>(
            source, MapPresentationFormat.JsonOptions)!;
        var stock = CrystalFlashColorCatalog.Load(new MemoryStream(source));
        var storedBody = typeof(CrystalFlashColorCatalog).GetField("body",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        AssertTrue(storedBody.GetValue(stock) is null, "Original Crystal Flash body selects calculation without stored rows");
        var cgram = new SnesCgram();
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BodyRecords + frame * 4);
            stock.ApplyBody(cgram, frame);
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
                AssertEqual(expected, stock.ResolveBody(frame, color), "Original Crystal Flash body RGB5 word");
                AssertEqual(expected, cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + color], "Calculated body color reaches CGRAM");
                var rows = document.Body.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
                rows[frame][color] = rows[frame][color] with { Red = (rows[frame][color].Red + 1) % 32 };
                var changedDocument = document with { Body = rows };
                var changed = CrystalFlashColorCatalog.Load(new MemoryStream(CrystalFlashColorCatalog.Write(changedDocument)));
                AssertTrue(storedBody.GetValue(changed) is not null, "Independent supplied body edit retains its rows");
                for (int verifyFrame = 0; verifyFrame < rows.Length; verifyFrame++)
                {
                    changed.ApplyBody(cgram, verifyFrame);
                    for (int verifyColor = 0; verifyColor < rows[verifyFrame].Length; verifyColor++)
                    {
                        var rgb = rows[verifyFrame][verifyColor];
                        ushort word = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                        AssertEqual(word, changed.ResolveBody(verifyFrame, verifyColor), "All supplied body fields preserved");
                        AssertEqual(word, cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + verifyColor], "Supplied body color reaches CGRAM");
                    }
                }
                // Restore stock CGRAM for the remaining checks in this original frame.
                stock.ApplyBody(cgram, frame);
            }
        }
        var storedBubble = typeof(CrystalFlashColorCatalog).GetField("bubble",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var residuals = (Dictionary<int, ushort>)storedBubble.GetValue(stock)!;
        AssertEqual(1, residuals.Count, "One stock bubble residual remains pending");
        AssertEqual((ushort)0x7fff, residuals[30], "Final-frame leading white remains supplied data");
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BubblePointers + frame * 2);
            ushort original = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
            AssertEqual(original, stock.ResolveBubble(frame, color), "Original bubble payload preserved");
            stock.ApplyBubble(cgram, frame);
            AssertEqual(original, cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color], "Original bubble color reaches CGRAM");
            var rows = document.Bubble.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            rows[frame][color] = rows[frame][color] with { Green = (rows[frame][color].Green + 1) % 32 };
            var changedDocument = document with { Bubble = rows };
            var changed = CrystalFlashColorCatalog.Load(new MemoryStream(CrystalFlashColorCatalog.Write(changedDocument)));
            for (int verifyFrame = 0; verifyFrame < rows.Length; verifyFrame++)
            {
                changed.ApplyBubble(cgram, verifyFrame);
                for (int verifyColor = 0; verifyColor < rows[verifyFrame].Length; verifyColor++)
                {
                    var rgb = rows[verifyFrame][verifyColor];
                    ushort word = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                    AssertEqual(word, changed.ResolveBubble(verifyFrame, verifyColor), "All supplied bubble fields preserved");
                    AssertEqual(word, cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + verifyColor], "Supplied bubble color reaches CGRAM");
                }
            }
            AssertTrue(storedBody.GetValue(changed) is null, "Independent bubble edit preserves body calculation");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(-1, 0), "Bubble lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(6, 0), "Bubble upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, -1), "Bubble lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, 6), "Bubble upper color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(-1, 0), "Calculated body lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(10, 0), "Calculated body upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, -1), "Calculated body lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, 10), "Calculated body upper color bound");
        Console.WriteLine("Stream 2 Crystal Flash body: all100 body and36 bubble native colors, CGRAM application and every independent supplied edit pass; one bubble residual remains pending.");
    }    private static void VerifyLookupStream2KraidRamps(ISnesAddressSpace rom)
    {
        PaletteRgb5[] ReadSource(KraidPaletteSource source) => Enumerable.Range(0, KraidPaletteRomData.ColorCount(source))
            .Select(index =>
            {
                ushort word = ReadVerificationWord(rom, KraidPaletteRomData.SourceAddress(source) + 2 * index);
                return new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }).ToArray();
        var document = new KraidColorDocument
        {
            Version = KraidColorFormat.Version,
            RoomBackdrop = ReadSource(KraidPaletteSource.RoomBackdrop),
            InitialTarget = ReadSource(KraidPaletteSource.InitialTarget),
            Health = ReadSource(KraidPaletteSource.Health),
            Secondary = ReadSource(KraidPaletteSource.Secondary),
            DeathArm = ReadSource(KraidPaletteSource.DeathArm),
        };
        var stock = KraidColorCatalog.Load(new MemoryStream(KraidColorCatalog.Write(document)));
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object health = typeof(KraidColorCatalog).GetField("health", fields)!.GetValue(stock)!;
        object secondary = typeof(KraidColorCatalog).GetField("secondary", fields)!.GetValue(stock)!;
        AssertTrue(ReferenceEquals(health, secondary), "Identical Kraid secondary colors share calculated primary source");
        var deviations = (System.Collections.IDictionary)health.GetType().GetField("deviations", fields)!.GetValue(health)!;
        AssertEqual(2, deviations.Count, "Only the two original color-six deviations remain outside interpolation");
        Check(stock, document);
        string expectedIdentity = SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
        {
            foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            {
                content.Append("source", (int)source);
                content.AppendWords("colors", Source(document, source).Select(Pack).ToArray());
            }
        });
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Calculated Kraid colors preserve canonical content identity");
        foreach (KraidPaletteSource source in new[] { KraidPaletteSource.Health, KraidPaletteSource.Secondary })
        for (int index = 0; index < KraidPaletteRomData.ColorCount(source); index++)
        {
            var editedColors = (PaletteRgb5[])Source(document, source).Clone();
            editedColors[index] = editedColors[index] with { Red = (editedColors[index].Red + 1) % 32 };
            var editedDocument = source == KraidPaletteSource.Health
                ? document with { Health = editedColors } : document with { Secondary = editedColors };
            var edited = KraidColorCatalog.Load(new MemoryStream(KraidColorCatalog.Write(editedDocument)));
            Check(edited, editedDocument);
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Every independent Kraid color edit changes content identity");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Health, -1), "Kraid calculated lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Secondary, 144), "Kraid calculated upper bound");
        Console.WriteLine("Stream 2 Kraid health ramps: original336 colors, both288 edited cells, source independence and canonical identity pass; endpoints and2 deviations remain pending.");

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        static PaletteRgb5[] Source(KraidColorDocument value, KraidPaletteSource source) => source switch
        {
            KraidPaletteSource.RoomBackdrop => value.RoomBackdrop,
            KraidPaletteSource.InitialTarget => value.InitialTarget,
            KraidPaletteSource.Health => value.Health,
            KraidPaletteSource.Secondary => value.Secondary,
            KraidPaletteSource.DeathArm => value.DeathArm,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
        static void Check(KraidColorCatalog actual, KraidColorDocument expected)
        {
            foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            {
                var colors = Source(expected, source);
                for (int index = 0; index < colors.Length; index++)
                    AssertEqual(Pack(colors[index]), actual.Resolve(source, index), "Every original or independently supplied Kraid color");
            }
        }
    }}