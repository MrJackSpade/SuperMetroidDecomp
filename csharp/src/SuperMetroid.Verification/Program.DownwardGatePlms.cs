using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Reproduces all eight gate-trigger dispatches in constructed rooms and checks the
    /// resident gate plus bank-$86 actor handoff independently of any long controller route.
    /// </summary>
    private static void VerifyDownwardGatePlms()
    {
        VerifyDownwardGateSetupAndProjectile();

        var cases = new (DownwardGateTriggerBehavior Trigger, ushort Projectile, bool Accepted)[]
        {
            (DownwardGateTriggerBehavior.GreenLeft, 0x0200, true),
            (DownwardGateTriggerBehavior.GreenRight, 0x0000, false),
            (DownwardGateTriggerBehavior.RedLeft, 0x0100, true),
            (DownwardGateTriggerBehavior.RedRight, 0x0200, true),
            (DownwardGateTriggerBehavior.BlueLeft, 0x0300, false),
            (DownwardGateTriggerBehavior.BlueRight, 0x0010, true),
            (DownwardGateTriggerBehavior.YellowLeft, 0x0300, true),
            // This counterintuitive inequality is present in the retail right-hand routine.
            (DownwardGateTriggerBehavior.YellowRight, 0x0000, true),
        };
        foreach ((DownwardGateTriggerBehavior trigger, ushort projectile, bool accepted) in cases)
            VerifyDownwardGateTrigger(trigger, projectile, accepted);

        Console.WriteLine(
            "  Downward gates: setup, slot order, actor handoff, and all eight shot filters agree.");
    }

    private static void VerifyDownwardGateSetupAndProjectile()
    {
        (TestAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer,
            RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                DownwardGateTriggerBehavior.GreenLeft);

        AssertEqual(2, plms.ActiveCount, "gate and shot-block records retain separate PLM slots");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(39, slots[0].NativeSlotIndex, "gate receives the first highest native slot");
        AssertEqual(RoomPlmHeaders.DownwardGate, slots[0].HeaderPointer,
            "first resident slot is the closed downward gate");
        AssertEqual(38, slots[1].NativeSlotIndex, "shot block receives the next native slot");
        AssertEqual(RoomPlmInstructionLists.DownwardGateShotBlockGreenLeft,
            slots[1].InstructionPointer, "shot block selects its ROM table instruction list");

        for (int row = 0; row < DownwardGatePlmRomData.GateHeightInBlocks; row++)
        {
            AssertEqual(DownwardGatePlmRomData.ClosedGateBts,
                level.GetCollisionBlockByIndex(gateBlockIndex + row * level.WidthInBlocks).Behavior,
                $"gate setup writes BTS $10 to row {row}");
        }
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(gateBlockIndex - 1).CollisionType,
            "left trigger table installs a shootable block");
        AssertEqual((int)DownwardGateTriggerBehavior.GreenLeft,
            level.GetCollisionBlockByIndex(gateBlockIndex - 1).Behavior,
            "left trigger table installs green-left BTS");

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
        // $BB6B. A valid left-green super hit wakes both the PLM and its associated actor.
        StepDownwardGatePlm(plms, bus, level, streamer);
        StepDownwardGatePlm(plms, bus, level, streamer);
        AssertTrue(plms.TrySpawnDownwardGateTrigger(
                level,
                gateBlockIndex - 1,
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Bts,
                0x0200),
            "super missile reaches the sleeping closed gate");
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
        RoomPlmSlotSnapshot openedGate = plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        AssertEqual(DownwardGatePreInstructionCodes.WakeIfTriggered,
            openedGate.PreInstruction, "open gate sleeps under shot-only callback");

        AssertTrue(plms.TrySpawnDownwardGateTrigger(
                level,
                gateBlockIndex - 1,
                level.GetCollisionBlockByIndex(gateBlockIndex - 1).Bts,
                0x0200),
            "second super missile reaches the sleeping open gate");
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
        (_, RoomLevelData level, _, RoomPlmSystem plms, int gateBlockIndex) =
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
    }

    private static (TestAddressSpace Bus, RoomLevelData Level,
        BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms, int GateBlockIndex)
        CreateDownwardGateFixture(DownwardGateTriggerBehavior trigger)
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
        WriteWord(bus, 0x84c82c, RoomPlmInstructionLists.DownwardGateOpening);
        WriteWord(bus, 0x84c838, RoomPlmInstructionLists.Delete);
        WriteWord(bus, 0x84aae3, RoomPlmInstructionCodes.Delete);
        SeedDownwardGateInstructionLists(bus);
        SeedDownwardGateProjectileRom(bus);

        ushort[] lists =
        [
            RoomPlmInstructionLists.DownwardGateShotBlockGreenLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockGreenRight,
            RoomPlmInstructionLists.DownwardGateShotBlockRedLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockRedRight,
            RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockBlueRight,
            RoomPlmInstructionLists.DownwardGateShotBlockYellowLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockYellowRight,
        ];
        for (int index = 0; index < lists.Length; index++)
        {
            WriteWord(bus, 0x84c70a + index * 2, lists[index]);
            WriteWord(bus, 0x84c71a + index * 2,
                (index & 1) == 0 ? unchecked((ushort)(0xc000 | 0x46 + index)) : (ushort)0);
            WriteWord(bus, 0x84c72a + index * 2,
                (index & 1) != 0 ? unchecked((ushort)(0xc000 | 0x46 + index)) : (ushort)0);
        }

        RoomLevelData level = CreateRoom(
            roomWidth,
            16,
            new ushort[roomWidth * 16],
            new byte[roomWidth * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        int parsed = plms.LoadRoomPopulation(
            bus,
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

    private static void SeedDownwardGateInstructionLists(TestAddressSpace bus)
    {
        // Exact retail bytes from $84:BC13-$BC60. The odd byte after each $8C19 sound
        // opcode is intentional and proves the following duration remains aligned.
        bus.WriteBytes(0x84bc13, [
            0x01, 0x00, 0x17, 0xa5, 0xdd, 0xbb, 0xc1, 0x86,
            0x52, 0xbb, 0xb4, 0x86, 0x10, 0x00, 0x17, 0xa5,
            0xe1, 0xbb, 0x4b, 0xe6, 0x19, 0x8c, 0x0e, 0x10,
            0x00, 0x25, 0xa5, 0x10, 0x00, 0x33, 0xa5, 0x10,
            0x00, 0x41, 0xa5, 0x18, 0x00, 0x4f, 0xa5, 0x01,
            0x00, 0x5d, 0xa5, 0xdd, 0xbb, 0xc1, 0x86, 0x6b,
            0xbb, 0xb4, 0x86, 0xf0, 0xbb, 0x66, 0xe5, 0x19,
            0x8c, 0x0e, 0x10, 0x00, 0x4f, 0xa5, 0x10, 0x00,
            0x41, 0xa5, 0x10, 0x00, 0x33, 0xa5, 0x18, 0x00,
            0x25, 0xa5, 0x24, 0x87, 0x13, 0xbc,
        ]);
        foreach (ushort drawPointer in new ushort[]
                 { 0xa517, 0xa525, 0xa533, 0xa541, 0xa54f, 0xa55d })
        {
            WriteOneBlockDraw(bus, drawPointer, 0x8000);
        }

        ushort[] triggerLists =
        [
            RoomPlmInstructionLists.DownwardGateShotBlockGreenLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockGreenRight,
            RoomPlmInstructionLists.DownwardGateShotBlockRedLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockRedRight,
            RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockBlueRight,
            RoomPlmInstructionLists.DownwardGateShotBlockYellowLeft,
            RoomPlmInstructionLists.DownwardGateShotBlockYellowRight,
        ];
        ushort[] triggerDraws =
            [0xa5d7, 0xa5e3, 0xa5eb, 0xa5f7, 0xa5ff, 0xa60b, 0xa613, 0xa61f];
        for (int index = 0; index < triggerLists.Length; index++)
        {
            WriteWord(bus, 0x840000 | triggerLists[index], 1);
            WriteWord(bus, 0x840000 | triggerLists[index] + 2, triggerDraws[index]);
            WriteWord(bus, 0x840000 | triggerLists[index] + 4, RoomPlmInstructionCodes.Delete);
            WriteOneBlockDraw(bus, triggerDraws[index], 0x8000);
        }
    }

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
