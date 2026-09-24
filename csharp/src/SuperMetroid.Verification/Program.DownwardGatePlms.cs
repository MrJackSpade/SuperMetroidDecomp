using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDownwardGateShotBlockDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

        for (ushort roomArgument = 0; roomArgument <= 14; roomArgument += 2)
        {
            DownwardGateShotBlockDefinition definition =
                DownwardGateShotBlockDefinitions.Resolve(roomArgument);
            AssertEqual(
                ReadWord(rom, DownwardGateShotBlockDefinitions.InstructionListTableAddress + roomArgument),
                definition.InstructionList,
                $"gate row ${roomArgument:X2} instruction list matches the cartridge");
            AssertEqual(
                ReadWord(rom, DownwardGateShotBlockDefinitions.LeftBlockWordTableAddress + roomArgument),
                definition.LeftBlockWord,
                $"gate row ${roomArgument:X2} left block matches the cartridge");
            AssertEqual(
                ReadWord(rom, DownwardGateShotBlockDefinitions.RightBlockWordTableAddress + roomArgument),
                definition.RightBlockWord,
                $"gate row ${roomArgument:X2} right block matches the cartridge");
        }

        AssertThrows<InvalidDataException>(
            () => DownwardGateShotBlockDefinitions.Resolve(1),
            "odd downward-gate table offsets fail loudly");
        AssertThrows<InvalidDataException>(
            () => DownwardGateShotBlockDefinitions.Resolve(16),
            "out-of-range downward-gate table offsets fail loudly");

        // The synthetic address space intentionally omits all three source tables. Running
        // every row through production setup proves room loading no longer reads them.
        VerifyDownwardGatePlms();
    }

    /// <summary>
    /// Reproduces all eight gate-trigger dispatches in constructed rooms and checks the
    /// resident gate plus bank-$86 actor handoff independently of any long controller route.
    /// </summary>
    private static void VerifyDownwardGatePlms()
    {
        VerifyDownwardGateHeaderDefinitions();
        VerifyDownwardGateProgramDefinitions();
        VerifyDownwardGateDrawDefinitions();
        VerifyDownwardGateVisuals();
        VerifyDownwardGateSetupAndProjectile();

        var cases = new (DownwardGateTriggerBehavior Trigger, ushort Projectile, bool Accepted)[]
        {
            (DownwardGateTriggerBehavior.BlueLeft, 0x0004, true),
            (DownwardGateTriggerBehavior.BlueRight, 0x0300, false),
            (DownwardGateTriggerBehavior.GreenLeft, 0x0200, true),
            (DownwardGateTriggerBehavior.GreenRight, 0x0000, false),
            (DownwardGateTriggerBehavior.RedLeft, 0x0100, true),
            (DownwardGateTriggerBehavior.RedRight, 0x0200, true),
            (DownwardGateTriggerBehavior.YellowLeft, 0x0300, true),
            // This counterintuitive inequality is present in the retail right-hand routine.
            (DownwardGateTriggerBehavior.YellowRight, 0x0000, true),
        };
        foreach ((DownwardGateTriggerBehavior trigger, ushort projectile, bool accepted) in cases)
            VerifyDownwardGateTrigger(trigger, projectile, accepted);

        Console.WriteLine(
            "  Downward gates: setup, slot order, actor handoff, and all eight shot filters agree.");
    }

    private static void VerifyDownwardGateHeaderDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort header in new ushort[]
                 { RoomPlmHeaders.DownwardGate, RoomPlmHeaders.DownwardGateShotBlock })
        {
            AssertTrue(DownwardGatePlmHeaderDefinitions.TryGetInitialInstruction(
                    header, out ushort compiled),
                $"gate header $84:{header:X4} has a compiled initial list");
            ushort address = checked((ushort)(header + 2));
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled,
                $"gate header $84:{header:X4} first instruction matches ROM");
        }
        AssertTrue(!DownwardGatePlmHeaderDefinitions.TryGetInitialInstruction(
                RoomPlmHeaders.ElevatorPlatform, out _),
            "gate header catalog does not claim unrelated room objects");
    }

    private static void VerifyDownwardGateDrawDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        int count = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 DownwardGatePlmDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                ushort directionAndCount = unchecked((ushort)(
                    rom.ReadByte(0x840000 | cursor) |
                    rom.ReadByte(0x840000 | (cursor + 1)) << 8));
                AssertEqual(run.DirectionAndCount, directionAndCount,
                    $"gate draw ${list.Pointer:X4} direction/count matches ROM");
                cursor += 2;
                foreach (ushort word in run.LevelWords.Span)
                {
                    ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                        rom.ReadByte(0x840000 | (cursor + 1)) << 8));
                    AssertEqual(word, native,
                        $"gate draw ${list.Pointer:X4} physical level word matches ROM");
                    cursor += 2;
                }

                AssertEqual(unchecked((byte)run.NextX), rom.ReadByte(0x840000 | cursor++),
                    $"gate draw ${list.Pointer:X4} next X matches ROM");
                AssertEqual(unchecked((byte)run.NextY), rom.ReadByte(0x840000 | cursor++),
                    $"gate draw ${list.Pointer:X4} next Y matches ROM");
            }

            count++;
        }
        AssertEqual(14, count, "all six resident and eight shot-trigger gate draws are compiled");
        AssertTrue(!DownwardGatePlmDrawDefinitions.TryGet(0xa518, out _),
            "adjacent payload bytes cannot alias a complete gate draw list");
    }

    private static void VerifyDownwardGateProgramDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        int wordCount = 0;
        foreach ((ushort address, ushort compiled) in
                 DownwardGatePlmProgramDefinitions.MechanicsWords)
        {
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled,
                $"downward-gate program word $84:{address:X4} matches ROM");
            wordCount++;
        }
        AssertEqual(62, wordCount,
            "resident and eight trigger gate streams have all compiled words");

        int byteCount = 0;
        foreach ((ushort address, byte compiled) in
                 DownwardGatePlmProgramDefinitions.MechanicsBytes)
        {
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"downward-gate sound byte $84:{address:X4} matches ROM");
            byteCount++;
        }
        AssertEqual(2, byteCount, "both resident gate sound operands are compiled");
        AssertTrue(!DownwardGatePlmProgramDefinitions.TryReadMechanicsWord(0xbc61, out _),
            "adjacent non-gate list bytes are not claimed");
    }

    private static void VerifyDownwardGateSetupAndProjectile()
    {
        (TestAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer,
            RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                DownwardGateTriggerBehavior.BlueLeft);

        AssertEqual(2, plms.ActiveCount, "gate and shot-block records retain separate PLM slots");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(39, slots[0].NativeSlotIndex, "gate receives the first highest native slot");
        AssertEqual(RoomPlmHeaders.DownwardGate, slots[0].HeaderPointer,
            "first resident slot is the closed downward gate");
        AssertEqual(38, slots[1].NativeSlotIndex, "shot block receives the next native slot");
        AssertEqual(RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft,
            slots[1].InstructionPointer, "argument zero selects the retail blue-left list");

        for (int row = 0; row < DownwardGatePlmRomData.GateHeightInBlocks; row++)
        {
            AssertEqual(DownwardGatePlmRomData.ClosedGateBts,
                level.GetCollisionBlockByIndex(gateBlockIndex + row * level.WidthInBlocks).Behavior,
                $"gate setup writes BTS $10 to row {row}");
        }
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(gateBlockIndex - 1).CollisionType,
            "left trigger table installs a shootable block");
        AssertEqual((int)DownwardGateTriggerBehavior.BlueLeft,
            level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior,
            "argument zero installs retail blue-left BTS $46");

        DownwardGateProjectileRequest request = plms.TakeDownwardGateProjectileRequests().Single();
        AssertEqual(DownwardGateProjectileOperation.Spawn, request.Operation,
            "closed gate setup publishes one spawn");
        AssertEqual((ushort)RoomEnemyProjectileKind.DownwardGateClosed,
            request.DefinitionPointer, "closed gate setup uses definition $E659");

        // The production room loader consumes this request after Enemies.Load has cleared
        // the shared projectile pool. Recreate that exact boundary and inspect initializer E.
        bus.WriteBytes(0xa19600, [0xff, 0xff]);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            populationPointer: 0x9600,
            tilesetPointer: 0,
            new SnesVram(),
            new SnesCgram(),
            nextRandom: () => 0,
            level: level,
            samus: new SamusState());
        enemies.ApplyDownwardGateProjectileRequest(request, level.WidthInBlocks);
        RoomEnemyProjectileSlot actor = enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
        AssertEqual(RoomEnemyProjectileKind.DownwardGateClosed, actor.Kind,
            "bank-$86 pool owns the closed gate actor");
        AssertEqual((ushort)(gateBlockIndex * 2), actor.Variable0,
            "gate actor variable E retains native PLM byte index");
        AssertEqual((ushort)96, actor.YPosition,
            "closed gate actor begins four tiles beneath a row-two PLM origin");
        enemies.StepEnemyProjectiles(level, samus: null);
        enemies.StepEnemyProjectiles(level, samus: null);
        AssertEqual(DownwardGateEnemyProjectileRomData.ClosedSleepInstruction,
            actor.InstructionPointer,
            "closed gate actor reaches its cartridge sleep before a shot can wake it");

        // The closed resident list first draws its collision state and then sleeps under
        // $BB6B. The Green Hill Zone fixture uses room argument zero, so an ordinary beam
        // must wake its blue-left gate exactly as it does in room $8F:9E52.
        StepDownwardGatePlm(plms, bus, level, streamer);
        StepDownwardGatePlm(plms, bus, level, streamer);
        AssertEqual((ushort)0xc0ff,
            level.GetCollisionBlockByIndex(gateBlockIndex + level.WidthInBlocks).LevelWord,
            "compiled initial gate draw installs the second solid column word");
        AssertTrue(plms.TrySpawnDownwardGateTrigger(
                level,
                gateBlockIndex - 1,
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Bts,
                0x0004),
            "power beam reaches the sleeping blue gate");
        StepDownwardGatePlm(plms, bus, level, streamer);
        DownwardGateProjectileRequest wake = plms.TakeDownwardGateProjectileRequests().Single();
        AssertEqual(DownwardGateProjectileOperation.Wake, wake.Operation,
            "closed gate publishes a wake request");
        AssertTrue(plms.SoundRequests.Any(sound =>
                sound.SoundEffect == SoundEffectId.FromCartridge(
                    SoundEffectLibrary.Library3,
                    DownwardGatePlmRomData.MovementSound)),
            "opening gate queues its cartridge movement sound");
        enemies.ApplyDownwardGateProjectileRequest(wake, level.WidthInBlocks);

        // Run the PLM and bank-$86 actor together until the open-state coroutine sleeps.
        // This exercises all four 16-pixel movement segments and the list's final goto.
        for (int frame = 0; frame < 96; frame++)
        {
            StepDownwardGatePlm(plms, bus, level, streamer);
            enemies.StepEnemyProjectiles(level, samus: null);
        }
        AssertEqual(0, enemies.ActiveEnemyProjectileCount,
            "opened gate actor retracts four tiles and deletes");
        AssertEqual((ushort)0x00ff,
            level.GetCollisionBlockByIndex(gateBlockIndex + level.WidthInBlocks).LevelWord,
            "compiled final open draw clears the second column collision word");
        RoomPlmSlotSnapshot openedGate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        AssertEqual(DownwardGatePreInstructionCodes.WakeIfTriggered,
            openedGate.PreInstruction, "open gate sleeps under shot-only callback");

        AssertTrue(plms.TrySpawnDownwardGateTrigger(
                level,
                gateBlockIndex - 1,
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Bts,
                0x0004),
            "second power beam reaches the sleeping open blue gate");
        // The open-state collision image remains for sixteen frames before $BBE1 spawns
        // the downward-moving actor, exactly as list $BC13 specifies.
        for (int frame = 0; frame < 17; frame++)
            StepDownwardGatePlm(plms, bus, level, streamer);
        DownwardGateProjectileRequest close = plms.TakeDownwardGateProjectileRequests().Single();
        AssertEqual(DownwardGateProjectileOperation.Spawn, close.Operation,
            "open gate publishes a moving close actor");
        AssertEqual((ushort)RoomEnemyProjectileKind.DownwardGateMoving,
            close.DefinitionPointer, "close request uses definition $E64B");
        enemies.ApplyDownwardGateProjectileRequest(close, level.WidthInBlocks);
        for (int frame = 0; frame < 80; frame++)
            enemies.StepEnemyProjectiles(level, samus: null);
        RoomEnemyProjectileSlot closedActor = enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
        AssertEqual((ushort)96, closedActor.YPosition,
            "moving gate travels four tiles down and remains parked closed");
    }

    private static void VerifyDownwardGateTrigger(
        DownwardGateTriggerBehavior trigger,
        ushort projectile,
        bool accepted)
    {
        (TestAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer,
            RoomPlmSystem plms, int gateBlockIndex) =
            CreateDownwardGateFixture(trigger);
        int triggerBlockIndex = ((byte)trigger & 1) == 0
            ? gateBlockIndex - 1
            : gateBlockIndex + 1;
        RoomBlockBehavior bts = level.GetCollisionBlockByIndex(triggerBlockIndex).Bts;
        AssertTrue(plms.TrySpawnDownwardGateTrigger(level, triggerBlockIndex, bts, projectile),
            $"{trigger} is claimed by the gate dispatcher");
        RoomPlmSlotSnapshot gate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        AssertEqual(accepted ? 1 : 0, gate.LoopTimer,
            $"{trigger} projectile ${projectile:X4} acceptance matches cartridge");
        StepDownwardGatePlm(plms, bus, level, streamer);
        StepDownwardGatePlm(plms, bus, level, streamer);
        int color = ((byte)trigger - (byte)DownwardGateTriggerBehavior.BlueLeft) / 2;
        ushort expectedTriggerWord = ((byte)trigger & 1) == 0
            ? checked((ushort)(0xc0db - color))
            : checked((ushort)(0xc4db - color));
        AssertEqual(expectedTriggerWord,
            level.GetCollisionBlockByIndex(triggerBlockIndex).LevelWord,
            $"{trigger} compiled shot-trigger draw installs its physical block word");
        AssertTrue(plms.PopulationSlots.All(slot =>
                slot.HeaderPointer != RoomPlmHeaders.DownwardGateShotBlock),
            $"{trigger} trigger list draws once and deletes without ROM control bytes");
    }

    private static (TestAddressSpace Bus, RoomLevelData Level,
        BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms, int GateBlockIndex)
        CreateDownwardGateFixture(DownwardGateTriggerBehavior trigger,
            RoomPlmDownwardGateVisualCatalog? visuals = null)
    {
        var bus = new TestAddressSpace();
        const ushort populationPointer = 0x9400;
        const byte gateX = 6;
        const byte gateY = 2;
        const int roomWidth = 16;
        ushort argument = unchecked((ushort)(((byte)trigger - 0x46) * 2));
        bus.WriteBytes(0x8f0000 | populationPointer, [
            0x2a, 0xc8, gateX, gateY, 0x00, 0x00,
            0x36, 0xc8, gateX, gateY, unchecked((byte)argument), 0x00,
            0x00, 0x00,
        ]);
        // Gate header first-list words are intentionally absent; the population
        // allocator must use the compiled definitions before gate setup/dispatch.
        WriteWord(bus, 0x84aae3, RoomPlmInstructionCodes.Delete);
        SeedDownwardGateProjectileRom(bus);

        var blockDefinitions = new byte[0x400 * 8];
        for (int tile = 0; tile < 4; tile++)
        {
            blockDefinitions[0x0ff * 8 + tile * 2] = 0x0f;
            blockDefinitions[0x053 * 8 + tile * 2] = 0x53;
        }
        RoomLevelData level = CreateRoom(
            roomWidth,
            16,
            new ushort[roomWidth * 16],
            new byte[roomWidth * 16],
            blockDefinitions: blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem { DownwardGateVisuals = visuals };
        int parsed = plms.LoadRoomPopulation(
            new DownwardGateHeaderReadGuard(bus),
            level,
            streamer,
            new SnesVram(),
            populationPointer,
            new Bank80SystemState(),
            AreaId.Brinstar,
            getSamus: () => null,
            isAreaTorizoDefeated: () => false);
        AssertEqual(2, parsed, $"{trigger} synthetic population parses both records");
        return (bus, level, streamer, plms, gateY * roomWidth + gateX);
    }

    private sealed class DownwardGateHeaderReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= DownwardGatePlmHeaderDefinitions.ResidentInitialInstructionAddress and
                <= DownwardGatePlmHeaderDefinitions.ResidentInitialInstructionAddress + 1 or
                >= DownwardGatePlmHeaderDefinitions.ShotBlockInitialInstructionAddress and
                <= DownwardGatePlmHeaderDefinitions.ShotBlockInitialInstructionAddress + 1)
                throw new InvalidOperationException(
                    $"Gate population read compiled header word ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private static void StepDownwardGatePlm(
        RoomPlmSystem plms,
        TestAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer) =>
        plms.Step(
            bus,
            level,
            streamer,
            layer1XPosition: 0,
            layer1YPosition: 0,
            bg1XOffset: 0,
            scrolls: null,
            enemyDeaths: 0,
            enemyDeathQuota: 0,
            controllerNewInput: 0);

    private static void SeedDownwardGateProjectileRom(TestAddressSpace bus)
    {
        // Both exact fourteen-byte definitions share the same inert initial preinstruction.
        bus.WriteBytes(0x86e64b, [
            0xd0, 0xe5, 0x04, 0xe6, 0x3c, 0xe5, 0x00, 0x00,
            0x00, 0x20, 0x00, 0x00, 0xfc, 0x84,
            0xd5, 0xe5, 0x04, 0xe6, 0x5e, 0xe5, 0x00, 0x00,
            0x00, 0x20, 0x00, 0x00, 0xfc, 0x84,
        ]);
        // Exact list/code span $86:E533-$E585. The first nine bytes are the native opcode
        // routine itself; list $E53C begins immediately afterward.
        bus.WriteBytes(0x86e533, [
            0xb9, 0x00, 0x00, 0x9d, 0xdb, 0x1a, 0xc8, 0xc8,
            0x60, 0x33, 0xe5, 0x00, 0x01, 0x61, 0x81, 0x05,
            0xe6, 0x01, 0x00, 0xee, 0xb4, 0x59, 0x81, 0x01,
            0x00, 0xf5, 0xb4, 0x59, 0x81, 0x01, 0x00, 0x01,
            0xb5, 0x59, 0x81, 0x01, 0x00, 0x12, 0xb5, 0x59,
            0x81, 0x6a, 0x81, 0x33, 0xe5, 0x00, 0xff, 0x01,
            0x00, 0x12, 0xb5, 0x59, 0x81, 0x61, 0x81, 0x05,
            0xe6, 0x01, 0x00, 0x12, 0xb5, 0x59, 0x81, 0x01,
            0x00, 0x01, 0xb5, 0x59, 0x81, 0x01, 0x00, 0xf5,
            0xb4, 0x59, 0x81, 0x01, 0x00, 0xee, 0xb4, 0x59,
            0x81, 0x54, 0x81,
        ]);
    }
}
