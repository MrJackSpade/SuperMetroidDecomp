using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Samus.Abilities.SpeedBoosterPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Focused collision-path checks for issue #297's Speed Booster terrain and spawned PLM programs.</summary>
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

    /// <summary>Confirms an unboosted downward contact remains solid and releases the temporary setup slot without changing terrain.</summary>
    private static void VerifyInactiveSpeedBlockRemainsSolid()
    {
        (_, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0e), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: false);

        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });

        AssertTrue(result.Collided,
            "inactive contact with BTS $0E retains the special-solid collision result");
        AssertEqual(RoomCollisionType.SpecialBlock,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "inactive contact does not mutate Speed Booster terrain");
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "rejected setup synchronously releases its temporary PLM slot");
    }

    /// <summary>Confirms stage-four floor contact clears collision immediately, queues the crumble sound, and later restores solid terrain.</summary>
    private static void VerifyBoostedFloorContactRunsRespawningPlm()
    {
        (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0e), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);

        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });

        AssertTrue(!result.Collided,
            "stage-four downward contact passes through the newly cleared floor block");
        AssertEqual(RoomCollisionType.Air,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "accepted setup clears the live collision nibble in the contact frame");
        AssertEqual(RoomPlmVisualBlockDefinitions.SpeedBoosterParent,
            level.GetCollisionBlockByIndex(blockIndex).PackedWord.VisualBlockIndex,
            "accepted setup installs cartridge visual parent $0B6");
        RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
        AssertEqual(PlmHeaderId.SpeedBlockRespawning, slot.HeaderPointer,
            "BTS $0E selects the standard respawning header");
        AssertTrue(slot.Animation is SpeedBoosterBlockPlmAnimation { Program: SpeedBoosterBlockProgram.Respawning },
            "BTS $0E starts the respawning program");

        StepSpeedBoosterPlm(plms, level);
        AssertTrue(plms.Sounds.SoundRequests.Any(request =>
                request.SoundEffect == Library2Sound.ContactBlockBreak &&
                request.MaximumQueued.Requests == 1),
            "first handler pass queues crumble sound six with cartridge maximum one");

        for (int frame = 0; frame < 80 && plms.SlotTable.ActiveCount != 0; frame++)
            StepSpeedBoosterPlm(plms, level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "respawning speed-block list reaches its delete instruction");
        AssertEqual(RoomCollisionType.SpecialBlock,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "respawning list restores special-solid collision");
        AssertEqual(RoomPlmVisualBlockDefinitions.SpeedBoosterParent,
            level.GetCollisionBlockByIndex(blockIndex).PackedWord.VisualBlockIndex,
            "respawning list restores synthesized visual parent $0B6");
    }

    /// <summary>Confirms stage-four wall contact selects the permanent PLM and leaves its block as air after the crumble frames.</summary>
    private static void VerifyBoostedWallContactRunsPermanentPlm()
    {
        (_, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x0f), AreaId.Crateria);
        SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
        samus.Position = samus.Position.WithWhole(new(34, 72));

        BlockMoveResult result = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = 4 << 16 });

        AssertTrue(!result.Collided,
            "stage-four horizontal contact passes through permanent speed terrain");
        AssertEqual(PlmHeaderId.SpeedBlockPermanent,
            plms.PopulationSlots.Single().HeaderPointer,
            "BTS $0F selects the standard permanent header");

        for (int frame = 0; frame < 12 && plms.SlotTable.ActiveCount != 0; frame++)
            StepSpeedBoosterPlm(plms, level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "permanent speed-block list deletes after its four crumble frames");
        AssertEqual(RoomCollisionType.Air,
            level.GetCollisionBlockByIndex(blockIndex).CollisionType,
            "permanent speed-block list does not restore collision");
    }

    /// <summary>Confirms Brinstar BTS $82-$85 choose their distinct respawn, permanent, and Dachora PLM headers through real collision contact.</summary>
    private static void VerifyBrinstarAreaTableVariants()
    {
        var cases = new SpeedBoosterCollisionBlocksCase[]
        {
            new SpeedBoosterCollisionBlocksCase(0x82, SpeedBoosterBlockPlmDefinitions.BrinstarSlowRespawning),
            new SpeedBoosterCollisionBlocksCase(0x83, SpeedBoosterBlockPlmDefinitions.BrinstarSlowPermanent),
            new SpeedBoosterCollisionBlocksCase(0x84, SpeedBoosterBlockPlmDefinitions.DachoraRespawning),
            new SpeedBoosterCollisionBlocksCase(0x85, SpeedBoosterBlockPlmDefinitions.Permanent),
        };

        foreach ((byte bts, SpeedBoosterBlockPlmDefinition expected) in cases)
        {
            (_, RoomLevelData level, RoomPlmSystem plms, int blockIndex) =
                CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(bts), AreaId.Brinstar);
            SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
            BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = 4 << 16, ScanLeftToRight = true });

            AssertTrue(!result.Collided,
                $"Brinstar BTS ${bts:X2} clears through the production collision path");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(expected.Header, slot.HeaderPointer,
                $"Brinstar BTS ${bts:X2} selects its exact PLM header");
            AssertTrue(slot.Animation is SpeedBoosterBlockPlmAnimation animation && animation.Program == expected.Program,
                $"Brinstar BTS ${bts:X2} starts its exact typed program");
            AssertEqual(RoomCollisionType.Air,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"Brinstar BTS ${bts:X2} clears collision synchronously");
        }
    }

    /// <summary>Places one type-$B speed block at room cell (2,4) with an empty PLM population and bank-$84 instruction fixtures.</summary>
    internal static CreateSpeedBoosterBlockFixtureResult
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
        behaviors[blockIndex] = bts.ToBtsByte();
        RoomLevelData level = CreateRoom(new(width, height),
            words,
            behaviors,
            blockDefinitions: new byte[0x400 * 8]);
        var plms = PlmFixtures.Unbound();
        int parsed = PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), area), RoomPlmPopulationImporter.Read(bus, emptyPopulation), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertEqual(0, parsed, "speed-block fixture begins with an empty room PLM population");
        return new CreateSpeedBoosterBlockFixtureResult(bus, level, plms, blockIndex);
    }

    /// <summary>Positions Samus for the fixture contact and selects either stage-four boost or the inactive counter.</summary>
    internal static SamusState CreateSpeedBoosterCollisionSamus(bool active)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(40, 54)),
        };
        samus.Kinematics.Radius = new(5, 8);
        samus.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(active
            ? SamusSpeedBoostCounterFixtures.ActiveStageWord
            : (ushort)0);
        return samus;
    }

    /// <summary>Seeds the empty population, single-block draw, and five bank-$84 speed-block lists used by collision-spawned PLMs.</summary>
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

    /// <summary>Encodes sound, three crumble delays, a 48-frame wait, three return draws, and the restore/delete opcodes.</summary>
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

    /// <summary>Encodes sound, three crumble delays, one final draw frame, and deletion without restoring the block.</summary>
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

    /// <summary>Advances a fixture PLM one frame with stationary view and no controller or enemy-death events.</summary>
    private static void StepSpeedBoosterPlm(
        RoomPlmSystem plms,
        RoomLevelData level) =>
        plms.Step(level, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

    /// <summary>Runs the focused Speed Booster collision and PLM verification from its command-line switch.</summary>
    [VerificationCommand("--speed-booster-block-plms")]
    internal static int SpeedBoosterBlockPlmsCommand()
    {
        Suite(nameof(VerifySpeedBoosterCollisionBlocks), () => VerifySpeedBoosterCollisionBlocks());
        return 0;
    }
}
