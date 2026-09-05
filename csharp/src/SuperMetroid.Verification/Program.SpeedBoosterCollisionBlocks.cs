using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Reproduces issue #297 at the real bank-$94 vertical and horizontal movement seams.
    /// The fixture uses type-$B terrain and the cartridge's exact BTS-to-PLM definitions;
    /// it does not call the block mutation routine directly.
    /// </summary>
    private static void VerifySpeedBoosterCollisionBlocks()
    {
        VerifyInactiveSpeedBlockRemainsSolid();
        VerifyBoostedFloorContactRunsRespawningPlm();
        VerifyBoostedWallContactRunsPermanentPlm();
        VerifyBrinstarAreaTableVariants();

        Console.WriteLine(
            "  Speed Booster blocks: contact gating, all five PLMs, sound, crumble, and respawn agree.");
    }

    private static void VerifyInactiveSpeedBlockRemainsSolid()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0e), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: false);

        BlockMoveResult result = SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            4 << 16,
            scanLeftToRight: true,
            plms: plms);

        AssertTrue(result.Collided,
            "inactive contact with BTS $0E retains the special-solid collision result");
        AssertEqual(RoomCollisionType.SpecialBlock,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "inactive contact does not mutate Speed Booster terrain");
        AssertEqual(0, plms.ActiveCount,
            "rejected setup synchronously releases its temporary PLM slot");
    }

    private static void VerifyBoostedFloorContactRunsRespawningPlm()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0e), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);

        BlockMoveResult result = SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            4 << 16,
            scanLeftToRight: true,
            plms: plms);

        AssertTrue(!result.Collided,
            "stage-four downward contact passes through the newly cleared floor block");
        AssertEqual(RoomCollisionType.Air,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "accepted setup clears the live collision nibble in the contact frame");
        AssertEqual(RoomPlmVisualBlockIndexes.SpeedBoosterParent,
            new RoomLevelWord(level.GetCollisionBlockByIndex(blockIndex).LevelWord).VisualBlockIndex,
            "accepted setup installs cartridge visual parent $0B6");
        RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
        AssertEqual(RoomPlmHeaders.SpeedBlockRespawning, slot.HeaderPointer,
            "BTS $0E selects the standard respawning header");
        AssertEqual(RoomPlmInstructionLists.SpeedBlockRespawning, slot.InstructionPointer,
            "BTS $0E selects list $C974");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        StepSpeedBoosterPlm(plms, bus, level, streamer);
        AssertTrue(plms.SoundRequests.Any(request =>
                request.SoundEffect == SoundEffectId.FromCartridge(
                    SoundEffectLibrary.Library2,
                    0x06) &&
                request.MaximumQueued == 1),
            "first handler pass queues crumble sound six with cartridge maximum one");

        for (int frame = 0; frame < 80 && plms.ActiveCount != 0; frame++)
            StepSpeedBoosterPlm(plms, bus, level, streamer);
        AssertEqual(0, plms.ActiveCount,
            "respawning speed-block list reaches its delete instruction");
        AssertEqual(RoomCollisionType.SpecialBlock,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "respawning list restores special-solid collision");
        AssertEqual(RoomPlmVisualBlockIndexes.SpeedBoosterParent,
            new RoomLevelWord(level.GetCollisionBlockByIndex(blockIndex).LevelWord).VisualBlockIndex,
            "respawning list restores synthesized visual parent $0B6");
    }

    private static void VerifyBoostedWallContactRunsPermanentPlm()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0f), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
        samus.XPosition = 34;
        samus.YPosition = 72;

        BlockMoveResult result = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            4 << 16,
            plms: plms);

        AssertTrue(!result.Collided,
            "stage-four horizontal contact passes through permanent speed terrain");
        AssertEqual(RoomPlmHeaders.SpeedBlockPermanent,
            plms.PopulationSlots.Single().HeaderPointer,
            "BTS $0F selects the standard permanent header");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        for (int frame = 0; frame < 12 && plms.ActiveCount != 0; frame++)
            StepSpeedBoosterPlm(plms, bus, level, streamer);
        AssertEqual(0, plms.ActiveCount,
            "permanent speed-block list deletes after its four crumble frames");
        AssertEqual(RoomCollisionType.Air,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "permanent speed-block list does not restore collision");
    }

    private static void VerifyBrinstarAreaTableVariants()
    {
        var cases = new (byte Bts, SpeedBoosterBlockPlmDefinition Definition)[]
        {
            (0x82, SpeedBoosterBlockPlmDefinitions.BrinstarSlowRespawning),
            (0x83, SpeedBoosterBlockPlmDefinitions.BrinstarSlowPermanent),
            (0x84, SpeedBoosterBlockPlmDefinitions.DachoraRespawning),
            (0x85, SpeedBoosterBlockPlmDefinitions.Permanent),
        };

        foreach ((byte bts, SpeedBoosterBlockPlmDefinition expected) in cases)
        {
            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
                CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(bts), AreaId.Brinstar);
            SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
            BlockMoveResult result = SamusBlockCollision.MoveVertical(
                bus,
                level,
                samus.Kinematics,
                4 << 16,
                scanLeftToRight: true,
                plms: plms);

            AssertTrue(!result.Collided,
                $"Brinstar BTS ${bts:X2} clears through the production collision path");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(expected.HeaderPointer, slot.HeaderPointer,
                $"Brinstar BTS ${bts:X2} selects its exact PLM header");
            AssertEqual(expected.InstructionPointer, slot.InstructionPointer,
                $"Brinstar BTS ${bts:X2} selects its exact instruction list");
            AssertEqual(RoomCollisionType.Air,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"Brinstar BTS ${bts:X2} clears collision synchronously");
        }
    }

    private static (TestAddressSpace Bus, RoomLevelData Level, RoomPlmSystem Plms, int BlockIndex)
        CreateSpeedBoosterBlockFixture(RoomBlockBehavior bts, AreaId area)
    {
        const int width = 8;
        const int height = 8;
        const int blockX = 2;
        const int blockY = 4;
        const ushort emptyPopulation = 0x9000;
        var bus = new TestAddressSpace();
        SeedSpeedBoosterInstructionLists(bus);
        bus.WriteBytes(0x8f0000 | emptyPopulation, [0x00, 0x00]);

        ushort[] words = new ushort[width * height];
        byte[] behaviors = new byte[words.Length];
        int blockIndex = blockY * width + blockX;
        words[blockIndex] = 0xb123;
        behaviors[blockIndex] = bts.Value;
        RoomLevelData level = CreateRoom(
            width,
            height,
            words,
            behaviors,
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        int parsed = plms.LoadRoomPopulation(
            bus,
            level,
            level.CreateBackgroundStreamer(),
            new SnesVram(),
            emptyPopulation,
            new Bank80SystemState(),
            area,
            getSamus: () => null,
            isAreaTorizoDefeated: () => false);
        AssertEqual(0, parsed, "speed-block fixture begins with an empty room PLM population");
        return (bus, level, plms, blockIndex);
    }

    private static SamusState CreateSpeedBoosterCollisionSamus(bool active)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 40,
            YPosition = 54,
        };
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 8;
        samus.HorizontalSpeed.SpeedBoostCounter = active
            ? SamusMovementRomData.HorizontalMotion.ActiveSpeedBoostStage
            : (ushort)0;
        return samus;
    }

    private static void SeedSpeedBoosterInstructionLists(TestAddressSpace bus)
    {
        const ushort drawPointer = 0xa000;
        WriteOneBlockDraw(bus, drawPointer, 0x00b6);

        // Exact opcode/timer structure of the three respawning lists. Their only
        // differences are the pre-respawn crumble delays, which remain ROM-driven here.
        bus.WriteBytes(0x84c951, CreateRespawningSpeedList(2, drawPointer));
        bus.WriteBytes(0x84c974, CreateRespawningSpeedList(1, drawPointer));
        bus.WriteBytes(0x84c997, CreateRespawningSpeedList(1, drawPointer));

        bus.WriteBytes(0x84c9cf, CreatePermanentSpeedList(2, drawPointer));
        bus.WriteBytes(0x84c9e4, CreatePermanentSpeedList(1, drawPointer));
    }

    private static byte[] CreateRespawningSpeedList(ushort crumbleDelay, ushort drawPointer)
    {
        var bytes = new List<byte> { 0x79, 0x8c, 0x06 };
        foreach (ushort duration in new ushort[]
                 { crumbleDelay, crumbleDelay, crumbleDelay, 48, 4, 4, 4 })
        {
            bytes.Add(unchecked((byte)duration));
            bytes.Add(unchecked((byte)(duration >> 8)));
            bytes.Add(unchecked((byte)drawPointer));
            bytes.Add(unchecked((byte)(drawPointer >> 8)));
        }
        bytes.Add(0x17);
        bytes.Add(0x8b);
        bytes.Add(0xbc);
        bytes.Add(0x86);
        return bytes.ToArray();
    }

    private static byte[] CreatePermanentSpeedList(ushort crumbleDelay, ushort drawPointer)
    {
        var bytes = new List<byte> { 0x79, 0x8c, 0x06 };
        foreach (ushort duration in new ushort[]
                 { crumbleDelay, crumbleDelay, crumbleDelay, 1 })
        {
            bytes.Add(unchecked((byte)duration));
            bytes.Add(unchecked((byte)(duration >> 8)));
            bytes.Add(unchecked((byte)drawPointer));
            bytes.Add(unchecked((byte)(drawPointer >> 8)));
        }
        bytes.Add(0xbc);
        bytes.Add(0x86);
        return bytes.ToArray();
    }

    private static void StepSpeedBoosterPlm(
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
}
