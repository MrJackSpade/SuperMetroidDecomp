using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static AssertionsVerification;
using static CollectiblesVerification;
using static SpeedBoosterPlmProgramsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>Represents speed booster collision blocks verification.</summary>
internal static class SpeedBoosterCollisionBlocksVerification
{
    /// <summary>
    /// Reproduces issue #297 at the real bank-$94 vertical and horizontal movement seams.
    /// The fixture uses type-$B terrain and the cartridge's exact BTS-to-PLM definitions;
    /// it does not call the block mutation routine directly.
    /// </summary>
    internal static void VerifySpeedBoosterCollisionBlocks()
    {
        Suite(nameof(VerifyInactiveSpeedBlockRemainsSolid), () => VerifyInactiveSpeedBlockRemainsSolid());
        Suite(nameof(VerifyBoostedFloorContactRunsRespawningPlm), () => VerifyBoostedFloorContactRunsRespawningPlm());
        Suite(nameof(VerifyBoostedWallContactRunsPermanentPlm), () => VerifyBoostedWallContactRunsPermanentPlm());
        Suite(nameof(VerifyBrinstarAreaTableVariants), () => VerifyBrinstarAreaTableVariants());
        Suite(nameof(VerifyCompiledSpeedBoosterPlmPrograms), () => VerifyCompiledSpeedBoosterPlmPrograms());

        Console.WriteLine(
            "  Speed Booster blocks: contact gating, all five PLMs, sound, crumble, and respawn agree.");
    }

    /// <summary>Verifies inactive speed block remains solid.</summary>
    private static void VerifyInactiveSpeedBlockRemainsSolid()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0e), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: false);

        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });

        AssertTrue(result.Collided,
            "inactive contact with BTS $0E retains the special-solid collision result");
        AssertEqual(RoomCollisionType.SpecialBlock,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "inactive contact does not mutate Speed Booster terrain");
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "rejected setup synchronously releases its temporary PLM slot");
    }

    /// <summary>Verifies boosted floor contact runs respawning plm.</summary>
    private static void VerifyBoostedFloorContactRunsRespawningPlm()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0e), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);

        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });

        AssertTrue(!result.Collided,
            "stage-four downward contact passes through the newly cleared floor block");
        AssertEqual(RoomCollisionType.Air,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "accepted setup clears the live collision nibble in the contact frame");
        AssertEqual(RoomPlmVisualBlockIndexes.SpeedBoosterParent,
            new RoomLevelWord(level.GetCollisionBlockByIndex(blockIndex).LevelWord).VisualBlockIndex,
            "accepted setup installs cartridge visual parent $0B6");
        RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
        AssertEqual(PlmHeaderId.SpeedBlockRespawning, slot.HeaderPointer,
            "BTS $0E selects the standard respawning header");
        AssertEqual(RoomPlmInstructionLists.SpeedBlockRespawning, slot.InstructionPointer,
            "BTS $0E selects list $C974");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        StepSpeedBoosterPlm(plms, bus, level, streamer);
        AssertTrue(plms.Sounds.SoundRequests.Any(request =>
                request.SoundEffect == SoundEffectId.FromCartridge(
                    SoundEffectLibrary.Library2,
                    0x06) &&
                request.MaximumQueued == 1),
            "first handler pass queues crumble sound six with cartridge maximum one");

        for (int frame = 0; frame < 80 && plms.SlotTable.ActiveCount != 0; frame++)
            StepSpeedBoosterPlm(plms, bus, level, streamer);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "respawning speed-block list reaches its delete instruction");
        AssertEqual(RoomCollisionType.SpecialBlock,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "respawning list restores special-solid collision");
        AssertEqual(RoomPlmVisualBlockIndexes.SpeedBoosterParent,
            new RoomLevelWord(level.GetCollisionBlockByIndex(blockIndex).LevelWord).VisualBlockIndex,
            "respawning list restores synthesized visual parent $0B6");
    }

    /// <summary>Verifies boosted wall contact runs permanent plm.</summary>
    private static void VerifyBoostedWallContactRunsPermanentPlm()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0f), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
        samus.Position = samus.Position.WithWhole(new(34, 72));

        BlockMoveResult result = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = 4 << 16 });

        AssertTrue(!result.Collided,
            "stage-four horizontal contact passes through permanent speed terrain");
        AssertEqual(PlmHeaderId.SpeedBlockPermanent,
            plms.PopulationSlots.Single().HeaderPointer,
            "BTS $0F selects the standard permanent header");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        for (int frame = 0; frame < 12 && plms.SlotTable.ActiveCount != 0; frame++)
            StepSpeedBoosterPlm(plms, bus, level, streamer);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "permanent speed-block list deletes after its four crumble frames");
        AssertEqual(RoomCollisionType.Air,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "permanent speed-block list does not restore collision");
    }

    /// <summary>Verifies brinstar area table variants.</summary>
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
            BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });

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

    /// <summary>Creates speed booster block fixture.</summary>
    internal static (TestAddressSpace Bus, RoomLevelData Level, RoomPlmSystem Plms, int BlockIndex)
        CreateSpeedBoosterBlockFixture(RoomBlockBehavior bts, AreaId area)
    {
        const int width = 8;
        const int height = 8;
        const int blockX = 2;
        const int blockY = 4;
        const ushort emptyPopulation = 0x9000;
        var bus = new TestAddressSpace();
        SeedSpeedBoosterBlockCartridge(bus, emptyPopulation);

        ushort[] words = new ushort[width * height];
        byte[] behaviors = new byte[words.Length];
        int blockIndex = blockY * width + blockX;
        words[blockIndex] = 0xb123;
        behaviors[blockIndex] = bts.Value;
        RoomLevelData level = CreateRoom(new(width, height),
            words,
            behaviors,
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        int parsed = plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, level.CreateBackgroundStreamer(), new SnesVram(), new Bank80SystemState(), area), RoomPlmPopulationImporter.Read(bus, emptyPopulation), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertEqual(0, parsed, "speed-block fixture begins with an empty room PLM population");
        return (bus, level, plms, blockIndex);
    }

    /// <summary>Creates speed booster collision samus.</summary>
    internal static SamusState CreateSpeedBoosterCollisionSamus(bool active)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(40, 54)),
        };
        samus.Kinematics.Radius = new(5, 8);
        samus.HorizontalSpeed.SpeedBoostCounter = active
            ? SamusMovementRomData.HorizontalMotion.ActiveSpeedBoostStage
            : (ushort)0;
        return samus;
    }

    /// <summary>Seed speed booster block cartridge.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Speed Booster block PLM instruction lists and an empty room population")]
    private static void SeedSpeedBoosterBlockCartridge(TestAddressSpace bus, ushort emptyPopulation)
    {
        bus.WriteBytes(0x8f0000 | emptyPopulation, [0x00, 0x00]);
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

    /// <summary>Creates respawning speed list.</summary>
    private static byte[] CreateRespawningSpeedList(ushort crumbleDelay, ushort drawPointer)
    {
        var bytes = new List<byte> { 0x79, 0x8c, 0x06 };
        foreach (ushort duration in new ushort[]
                 { crumbleDelay, crumbleDelay, crumbleDelay, 48, 4, 4, 4 })
        {
            bytes.Add(unchecked((byte)duration));
            bytes.Add(SnesLittleEndian.HighByte(duration));
            bytes.Add(unchecked((byte)drawPointer));
            bytes.Add(SnesLittleEndian.HighByte(drawPointer));
        }
        bytes.Add(0x17);
        bytes.Add(0x8b);
        bytes.Add(0xbc);
        bytes.Add(0x86);
        return bytes.ToArray();
    }

    /// <summary>Creates permanent speed list.</summary>
    private static byte[] CreatePermanentSpeedList(ushort crumbleDelay, ushort drawPointer)
    {
        var bytes = new List<byte> { 0x79, 0x8c, 0x06 };
        foreach (ushort duration in new ushort[]
                 { crumbleDelay, crumbleDelay, crumbleDelay, 1 })
        {
            bytes.Add(unchecked((byte)duration));
            bytes.Add(SnesLittleEndian.HighByte(duration));
            bytes.Add(unchecked((byte)drawPointer));
            bytes.Add(SnesLittleEndian.HighByte(drawPointer));
        }
        bytes.Add(0xbc);
        bytes.Add(0x86);
        return bytes.ToArray();
    }

    /// <summary>Steps speed booster plm.</summary>
    private static void StepSpeedBoosterPlm(
        RoomPlmSystem plms,
        TestAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer) =>
        plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

    /// <summary>Speed booster block plms command.</summary>
    [VerificationCommand("--speed-booster-block-plms")]
    internal static int SpeedBoosterBlockPlmsCommand()
    {
        Suite(nameof(VerifySpeedBoosterCollisionBlocks), () => VerifySpeedBoosterCollisionBlocks());
        return 0;
    }
}
