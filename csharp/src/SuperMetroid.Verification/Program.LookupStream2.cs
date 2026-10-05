using System.Reflection;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream2KagoFrameGeometry(ISnesAddressSpace rom)
    {
        int count = 0;
        foreach (var frame in KagoVisualDefinitions.Frames())
        {
            ushort native = ReadVerificationWord(rom, 0xa8ab20 + 4 * count);
            AssertEqual((byte)0xa8, frame.Bank, "Kago native frame bank");
            AssertEqual(native, frame.Pointer, "Kago native animation selects generated identity");
            AssertEqual((ushort)4, ReadVerificationWord(rom, 0xa80000 | native), "Kago native four-part record width");
            AssertEqual($"kago_cycle_{count}", frame.Name, "Kago installed resource name");
            AssertTrue(EnemySpritemapDefinitions.Frames.Contains(frame), "Generated Kago identity reaches installed catalog");
            count++;
        }
        AssertEqual(3, count, "Kago frame enumeration count");
        Console.WriteLine("Kago frames:three native selectors/four-part headers, stable names and installed catalog membership pass.");
    }
    private static void VerifyLookupStream2KraidLintInitialization(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 1));
        var finish = typeof(RoomEnemySystem).GetMethod("FinishKraidGrowth", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies);
        var align = typeof(RoomEnemySystem).GetMethod("AlignKraidPart", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidPartState>>(enemies);
        var body = enemies.Slots[0];
        body.XPosition = 200; body.YPosition = 300;
        for (int slot = 2; slot <= 4; slot++) enemies.Slots[slot].VariableB = 17;
        finish(body, state);
        for (int slot = 2; slot <= 4; slot++)
        {
            var lint = enemies.Slots[slot];
            ushort expected = Word(0xa7a916 + (slot - 2) * 2);
            AssertEqual(expected, KraidLintInitializationDefinitions.InitialDelayForSlot(slot), "Native per-part initial delay");
            AssertEqual(expected, lint.VariableF, "Actual post-growth timer write");
            AssertEqual((ushort)0, lint.VariableB, "Actual lint growth resets extension");
            AssertEqual(KraidAiFunction.LintProduce, state.Parts[slot].NextFunction, "Actual lint continuation");
            for (int remaining = expected - 1; remaining >= 0; remaining--)
            {
                align(lint, state.Parts[slot]);
                AssertEqual((ushort)remaining, lint.VariableF, "Actual per-part countdown");
                AssertEqual((ushort)(remaining == 0 ? KraidAiFunction.LintProduce : KraidAiFunction.AlignPartToKraid),
                    lint.VariableA, "Actual lint transition occurs on its own final tick");
                AssertEqual(unchecked((ushort)(body.XPosition - lint.XRadius)), lint.XPosition, "Alignment continues during delay");
            }
        }
        foreach (int invalid in new[] { -1, 0, 1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidLintInitializationDefinitions.InitialDelayForSlot(invalid), "Only lint slots have launch policy");
        Console.WriteLine("Kraid lint initialization: three native policies, actual slot writes and all 512 countdown transitions pass.");
    }
    private static void VerifyLookupStream2DropSelection(SuperMetroidAddressSpace rom)
    {
        // Native first record weights60,60,60,5,60,10 yield cumulative thresholds
        //60,120,180,185,245,255 when all resources are eligible.
        ushort[] randomValues = [1, 61, 121, 181, 186, 246];
        for (int column = 0; column < 6; column++)
        {
            EnemyPickupKind expected = (EnemyPickupKind)rom.ReadByte(0x86f25e + column);
            AssertEqual(expected, EnemyDropSelectionDefinitions.ForProbabilityColumn(column),
                "Native probability-column pickup identity");
            var samus = CreateDropTestSamus();
            samus.Health = 50;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 5;
            var fixture = CreateEnemyDropFixture(samus, [randomValues[column]]);
            var projectile = fixture.System.EnemyProjectiles[0];
            projectile.ItemDropChancesPointerOverride = NativeDropChancePointer;
            AssertEqual(expected, fixture.System.SelectRandomEnemyDrop(projectile),
                "Actual cumulative selection preserves each semantic column");
        }
        VerifyEnemyDropSelectionRules();
        AssertThrows<IndexOutOfRangeException>(() => EnemyDropSelectionDefinitions.ForProbabilityColumn(-1),
            "Drop column lower bound");
        AssertThrows<IndexOutOfRangeException>(() => EnemyDropSelectionDefinitions.ForProbabilityColumn(6),
            "Drop column upper bound");
        Console.WriteLine("Enemy drop selection:six native identities and actual cumulative selections, RNG-zero reroll, energy hysteresis and full-resource eligibility pass.");
    }
    private static void VerifyLookupStream2PipeBugFormation(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeNorfairPipeBug", flags)!;
        MethodInfo wait = typeof(RoomEnemySystem).GetMethod("RunNorfairPipeBugSamusWait", flags)!;
        for (int member = 0; member < 5; member++)
        {
            AssertEqual(Word(0xb38c65 + member * 7), PipeBugDefinitions.NorfairStaggerTarget(member),
                "Native rank-derived formation release counter");
            AssertEqual(Word(0xb38c88 + member * 6), (ushort)PipeBugDefinitions.NorfairPostRiseFunction(member),
                "Native role-specific formation function");
        }
        foreach (ushort samusX in new ushort[] { 120, 136 })
        {
            var enemies = new RoomEnemySystem();
            for (int member = 0; member < 5; member++)
            {
                var slot = enemies.Slots[member];
                slot.EnemyDefinitionPointer = PipeBugDefinitions.NorfairEnemyDefinition;
                slot.XPosition = 128; slot.YPosition = 128; slot.Parameter2 = 64;
                initialize.Invoke(enemies, [slot]);
                slot.InstructionTimer = (ushort)(member + 4);
                slot.Timer = (ushort)(member + 8);
            }
            var samus = new SamusState { XPosition = samusX, YPosition = 112 };
            wait.Invoke(enemies, [enemies.Slots[0], enemies.PipeBugStates[0]!, samus]);
            for (int member = 0; member < 5; member++)
            {
                var state = enemies.PipeBugStates[member]!;
                AssertEqual(Word(0xb38c65 + member * 7), state.StaggerTarget,
                    "Actual formation writes native release counter");
                AssertEqual(Word(0xb38c88 + member * 6), (ushort)state.NorfairPostRiseFunction,
                    "Actual formation writes native role function");
                AssertEqual(PipeBugEnemyFunction.NorfairRise, state.Function, "Actual formation begins rising");
                AssertEqual(samusX < 128 ? NorfairPipeBugInstructionProgramDefinitions.RisingLeft
                    : NorfairPipeBugInstructionProgramDefinitions.RisingRight,
                    enemies.Slots[member].CurrentInstruction, "Actual formation selects facing");
                AssertEqual((ushort)(member == 0 ? 1 : member + 4), enemies.Slots[member].InstructionTimer,
                    "Only leader instruction timer resets");
                AssertEqual((ushort)(member == 0 ? 0 : member + 8), enemies.Slots[member].Timer,
                    "Only leader loop counter resets");
            }
        }
        AssertThrows<IndexOutOfRangeException>(() => PipeBugDefinitions.NorfairStaggerTarget(-1), "Formation lower bound");
        AssertThrows<IndexOutOfRangeException>(() => PipeBugDefinitions.NorfairPostRiseFunction(5), "Formation upper bound");
        Console.WriteLine("Norfair Pipe Bug formation:ten native immediates, both actual facing formations and leader-only timer resets pass.");
    }
    private static void VerifyLookupStream2GoldenControl(SuperMetroidAddressSpace rom)
    {
        VerifyGoldenTorizoJumpLandingDefinitions(rom);
        int address = 0xc9cb;
        for (int index = 0; index < 7; index++)
        {
            GoldenTorizoInitialMechanicsWord actual =
                GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual((ushort)address, actual.Address, "Golden initial native instruction cursor");
            ushort native = (ushort)(rom.ReadByte(0xaa0000 | address) |
                rom.ReadByte(0xaa0000 | (address + 1)) << 8);
            AssertEqual(native, actual.Value, "Golden initial native control word");
            AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions.TryReadMechanicsWord(
                actual.Address, out ushort selected) && selected == native,
                "Golden initial runtime word dispatch");
            for (int byteIndex = 0; byteIndex < 2; byteIndex++)
                AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions.IsCompiledMechanicsByte(
                    0xaa0000 | (address + byteIndex)), "Golden initial control byte ownership");
            address += native == 0x814b ? 9 : native < 0x8000 ? 4 : 2;
        }
        AssertEqual(0xc9e2, address, "Golden initial terminal sleep boundary");
        ushort operand = GoldenTorizoInitialInstructionProgramDefinitions.InitialFrameOperand;
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xaa, operand, out ushort visual),
            "Golden initial installed sprite selector");
        AssertEqual((ushort)(rom.ReadByte(0xaa0000 | operand) |
            rom.ReadByte(0xaa0000 | (operand + 1)) << 8), visual,
            "Golden initial exact native sprite selection");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions.TryReadMechanicsWord(operand, out _),
            "Golden initial visual operand excluded from mechanics");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions.TryReadMechanicsWord((ushort)address, out _),
            "Golden initial next program excluded from mechanics");
        AssertThrows<IndexOutOfRangeException>(() => GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord(7),
            "Golden initial control index boundary");
        AssertThrows<IndexOutOfRangeException>(() => GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWord(20),
            "Golden landing control index boundary");
        Console.WriteLine("Golden Torizo control dispatch:7 initial words,20 landing words, native instruction positions, exact installed initial sprite and bounded ownership pass.");
    }
    private static void VerifyLookupStream2TrailPrograms(ISnesAddressSpace bus)
    {
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        var catalog = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var encountered = new HashSet<ushort>();
        foreach (ushort start in new[] { ProjectileTrailDefinitions.LeftIce, ProjectileTrailDefinitions.RightIce, ProjectileTrailDefinitions.Wave, ProjectileTrailDefinitions.Missile })
        {
            ushort cursor = start;
            while (true)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Banks.Movement | cursor);
                if (word == 0) break;
                if (word < 0x8000) { encountered.Add(cursor); cursor += 4; }
                else { cursor += 2; }
            }
            var nativeSystem = new SamusProjectileSystem(); var authoredSystem = new SamusProjectileSystem();
            foreach (var system in new[] { nativeSystem, authoredSystem })
            {
                var pair = system.TrailSlots[0];
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.InstructionTimer = 1; side.XPosition = 100; side.YPosition = 100; }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer(); var authoredOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                StepNative(nativeSystem.TrailSlots[0], nativeSystem.TrailSlots[0].Left, nativeOam, frozenFrame);
                StepNative(nativeSystem.TrailSlots[0], nativeSystem.TrailSlots[0].Right, nativeOam, frozenFrame);
                authoredSystem.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), authoredOam, 0, 0, frozenFrame, catalog);
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { (nativeSystem.TrailSlots[0].Left, authoredSystem.TrailSlots[0].Left), (nativeSystem.TrailSlots[0].Right, authoredSystem.TrailSlots[0].Right) })
                {
                    AssertEqual(sides.Item1.InstructionPointer, sides.Item2.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.Item1.InstructionTimer, sides.Item2.InstructionTimer, "Trail artwork cannot change live timing");
                    AssertEqual(sides.Item1.YPosition, sides.Item2.YPosition, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.ToArray()), "Independent native stream walk finds exactly the catalog's appearance records");
        int programWords = 0;
        for (int address = 0x90b4c8; address <= 0x90b5b3; address++)
        {
            if (ProjectileTrailProgramDefinitions.TryRead(address, out _))
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailProgramDefinitions.Read(bus, address), "Compiled trail program preserves every authored mechanics word");
                programWords++;
            }
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(bus, rejectedAddress), "Trail program rejects presentation gaps, odd addresses and unrelated high-bank words");
            }
        }
        AssertEqual(67, programWords, "All 42 durations, 20 movement commands and five terminators are compiled");
        AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(bus, 0x91b4c9), "Trail program rejects a wrong-bank alias");
        VerifyTrailMutableAlias();
        AssertThrows<IndexOutOfRangeException>(() => _ = ProjectileTrailVisualDefinitions.Frames[-1], "Calculated trail frame lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = ProjectileTrailVisualDefinitions.Frames[42], "Calculated trail frame upper bound");
        Console.WriteLine("Stream 2 trail programs: 67 native mechanics words,42 visual record addresses, all four actual paired trail lifetimes/freeze states and mutable alias pass.");
        void StepNative(SamusProjectileTrailSlot pair, SamusProjectileTrailSide side, OamBuffer oam, bool frozen)
        {
            if (side.InstructionTimer == 0) return;
            if (!frozen && --side.InstructionTimer == 0)
            {
                ushort cursor = side.InstructionPointer;
                while (true)
                {
                    ushort word = ReadVerificationWord(bus, 0x900000 | cursor);
                    if (word < 0x8000)
                    {
                        side.InstructionTimer = word;
                        if (word == 0) return;
                        side.TileNumberAttributes = ReadVerificationWord(bus, 0x900000 | (cursor + 2));
                        side.InstructionPointer = (ushort)(cursor + 4);
                        break;
                    }
                    cursor += 2;
                    if (word == SamusProjectileRomData.Trails.MoveLeftDown) pair.Left.YPosition++;
                    else if (word == SamusProjectileRomData.Trails.MoveRightDown) pair.Right.YPosition++;
                    else throw new InvalidDataException($"Unexpected native trail command {word:X4}.");
                }
            }
            oam.AddProjectileTrailSprite((byte)side.XPosition, (byte)side.YPosition, side.TileNumberAttributes);
        }
    }

    private static void VerifyLookupStream2TrailSelectors(ISnesAddressSpace bus)
    {

        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address), "Trail selectors retain every reachable aligned native word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end), "Trail selector rejects the following word");
        var spawn = typeof(SamusProjectileSystem).GetMethod("SpawnTrail", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (int selection = 0; selection < 64; selection++)
        {
            var projectiles = new SamusProjectileSystem();
            var projectile = new SamusProjectileSlot(0)
            {
                Type = (ushort)selection,
                InstructionPointer = 0x86e3,
                XPosition = 100,
                YPosition = 200,
            };
            spawn.Invoke(projectiles, [new TrailSelectorGuard(start, end), projectile]);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.XPosition, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }

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