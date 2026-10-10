using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDownwardGateShotBlockDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDownwardGateListSelection), () => VerifyDownwardGateListSelection(rom));
        Suite(nameof(VerifyDownwardGateLeftSelection), () => VerifyDownwardGateLeftSelection(rom));
        Suite(nameof(VerifyDownwardGateRightSelection), () => VerifyDownwardGateRightSelection(rom));

        // The synthetic address space intentionally omits all three source tables. Running
        // every row through production setup proves room loading no longer reads them.
        Suite(nameof(VerifyDownwardGatePlms), () => VerifyDownwardGatePlms());
    }

    /// <summary>
    /// Reproduces all eight gate-trigger dispatches in constructed rooms and checks the
    /// resident gate plus bank-$86 actor handoff independently of any long controller route.
    /// </summary>
    private static void VerifyDownwardGatePlms()
    {
        Suite(nameof(VerifyDownwardGateHeaderDefinitions), () => VerifyDownwardGateHeaderDefinitions());
        Suite(nameof(VerifyDownwardGateProgramDefinitions), () => VerifyDownwardGateProgramDefinitions());
        Suite(nameof(VerifyDownwardGateDrawDefinitions), () => VerifyDownwardGateDrawDefinitions());
        Suite(nameof(VerifyDownwardGateVisuals), () => VerifyDownwardGateVisuals());
        Suite(nameof(VerifyDownwardGateSetupAndProjectile), () => VerifyDownwardGateSetupAndProjectile());

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
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate header oracle revision");
        foreach ((PlmHeaderId header, int source) in new[] { (PlmHeaderId.DownwardGate, 0x84c82c), (PlmHeaderId.DownwardGateShotBlock, 0x84c838) })
            AssertEqual(ReadSamusEaterPlmWord(rom, source), RoomPlmHeaderDefinitions.Get(header).InitialInstruction,
                "Gate compiled-population first instruction");
    }

    private static void VerifyDownwardGateDrawDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate draw oracle revision");
        Suite(nameof(VerifyDownwardGateDrawGeometry), () => VerifyDownwardGateDrawGeometry(rom));
        Suite(nameof(VerifyDownwardGateDrawCollision), () => VerifyDownwardGateDrawCollision(rom));
        Suite(nameof(VerifyDownwardGateDrawVisuals), () => VerifyDownwardGateDrawVisuals(rom));
    }

    private static void VerifyDownwardGateProgramDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate program oracle revision");
        Suite(nameof(VerifyDownwardGateProgramControls), () => VerifyDownwardGateProgramControls(rom));
        Suite(nameof(VerifyDownwardGateProgramDraws), () => VerifyDownwardGateProgramDraws(rom));
        Suite(nameof(VerifyDownwardGateProgramOperands), () => VerifyDownwardGateProgramOperands(rom));
        Suite(nameof(VerifyDownwardGateProgramSounds), () => VerifyDownwardGateProgramSounds(rom));
    }

    private static void VerifyDownwardGateSetupAndProjectile()
    {
        (TestAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer,
            RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                DownwardGateTriggerBehavior.BlueLeft);

        AssertEqual(2, plms.ActiveCount, "gate and shot-block records retain separate PLM slots");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(39, slots[0].NativeSlotIndex, "gate receives the first highest native slot");
        AssertEqual(PlmHeaderId.DownwardGate, slots[0].HeaderPointer,
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
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
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
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
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
                slot.HeaderPointer != PlmHeaderId.DownwardGateShotBlock),
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
        // Supply imported placements directly. Cartridge header reads belong to import;
        // the guard below prohibits them only while the production runtime sets up slots.
        var population = new RoomPlmPopulationDefinition(populationPointer,
        [
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.DownwardGate),
                gateX, gateY, 0),
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(PlmHeaderId.DownwardGateShotBlock),
                gateX, gateY, argument),
        ]);
        WriteWord(bus, 0x84aae3, (ushort)RoomPlmInstruction.Delete);
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
            population,
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
            // The resident and shot-block gate headers' first-instruction words.
            if (address is >= 0x84c82c and <= 0x84c82d or >= 0x84c838 and <= 0x84c839)
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
