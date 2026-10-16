using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Constructed-room checks for both eye-door facings, their linked PLMs and spawned enemy projectiles.</summary>
internal static class EyeDoorPlmsVerification
{
    /// <summary>
    /// Reproduces both mirrored eye-door families in constructed rooms. The fixture uses
    /// the retail instruction entry points but deliberately tiny frame lists, isolating the
    /// shared PLM state machine from unrelated room traversal and graphics streaming.
    /// </summary>
    internal static void VerifyEyeDoorPlms()
    {
        Suite(nameof(VerifyEyeDoorOrientation), () => VerifyEyeDoorOrientation(EyeDoorOrientation.Right, useSuperMissile: false));
        Suite(nameof(VerifyEyeDoorOrientation), () => VerifyEyeDoorOrientation(EyeDoorOrientation.Left, useSuperMissile: true));
        Suite(nameof(VerifyEyeDoorEnemyProjectiles), () => VerifyEyeDoorEnemyProjectiles());
        Console.WriteLine(
            "  Eye doors: mirrored setup, slot order, effects, shot filters, persistence, and blue-door conversion agree.");
    }

    /// <summary>Checks eye attack direction, open-door cancellation, sweat velocity and smoke RNG placement.</summary>
    private static void VerifyEyeDoorEnemyProjectiles()
    {
        var bus = new TestAddressSpace();
        SeedEmptyRoomEnemyPopulation(bus, 0x9600);
        RoomLevelData level = CreateRoom(new(16, 16),
            new ushort[16 * 16],
            new byte[16 * 16],
            blockDefinitions: new byte[0x400 * 8]);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(220, 80)) };
        int randomAdvances = 0;
        var enemies = new RoomEnemySystem();
        enemies.Load(new RoomEnemyLoadRequest(bus, 0x9600, 0, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => { randomAdvances++; return 0x7777; }) { ReadRandom = () => 0x3412 }) { Level = level, Samus = samus });
        var system = new Bank80SystemState();
        int plmBlock = 4 * 16 + 7;

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.Projectile,
                Parameter: 0,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot attack = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorProjectile);
        AssertEqual((ushort)104, attack.Position.X.Whole,
            "eye-door attack initializer applies cartridge PLM X offset");
        AssertEqual((ushort)80, attack.Position.Y.Whole,
            "eye-door attack initializer applies cartridge PLM Y offset");
        for (int frame = 0; frame < 10; frame++)
            enemies.StepEnemyProjectiles(level, samus);
        AssertEqual((ushort)0x0080, attack.Variable0,
            "direction opcode stores twice the rightward byte angle");
        AssertEqual((ushort)0x0100, attack.Velocity.X.RawValue,
            "direction opcode seeds rightward signed 8.8 X velocity");
        AssertEqual((ushort)0, attack.Velocity.Y.RawValue,
            "direction opcode seeds zero Y velocity for a level target");
        system.SetOpenedDoorBit(5);
        enemies.StepEnemyProjectiles(level, samus);
        AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170,
            attack.PreInstruction,
            "opened door impact executes its native clear-pre-instruction command");

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.Sweat,
                Parameter: 4,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot sweat = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertEqual((ushort)64, sweat.Velocity.X.RawValue,
            "mirrored sweat parameter selects positive cartridge X velocity");
        AssertEqual((ushort)512, sweat.Velocity.Y.RawValue,
            "sweat parameter retains cartridge falling velocity");

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.Smoke,
                Parameter: 0x030a,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot smoke = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
        AssertTrue(smoke.Animation is MiscDustProjectileAnimation { Cursor.Program: var smokeProgram } &&
                smokeProgram == MiscDustProjectileDefinitions.Program((MiscDustAnimation)0x0a),
            "smoke low parameter byte selects the misc-dust program");
        AssertEqual((ushort)114, smoke.Position.X.Whole,
            "smoke X uses low random byte and selected mask/base tuple");
        AssertEqual((ushort)76, smoke.Position.Y.Whole,
            "smoke Y uses high random byte and selected mask/base tuple");
        AssertEqual(1, randomAdvances,
            "smoke advances cartridge RNG once after sampling current seed");
    }

    /// <summary>Exercises one mirrored eye-door family through setup, shot gating, persistence and blue-door conversion.</summary>
    private static void VerifyEyeDoorOrientation(
        EyeDoorOrientation orientation,
        bool useSuperMissile)
    {
        const int roomWidth = 16;
        const int roomHeight = 16;
        const byte eyeX = 7;
        const byte eyeY = 4;
        const ushort doorBit = 5;
        int eyeBlock = eyeY * roomWidth + eyeX;

        var bus = new TestAddressSpace();
        SeedEyeDoorFixtureRom(bus, orientation);
        RoomLevelData level = CreateRoom(new(roomWidth, roomHeight),
            new ushort[roomWidth * roomHeight],
            new byte[roomWidth * roomHeight],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(eyeX * 16 + 8, eyeY * 16 + 8)),
        };
        var effects = new List<EyeDoorProjectileRequest>();
        var plms = new RoomPlmSystem();
        AssertEqual(3, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(level, streamer, new SnesVram(), system, AreaId.Brinstar), RoomPlmPopulationImporter.Read(bus, pointer: 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { EyeDoorProjectile = effects.Add } }),
            $"{orientation} eye-door population record count");

        AssertEqual(3, plms.SlotTable.ActiveCount, $"{orientation} eye door retains three PLM slots");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(39, slots[0].NativeSlotIndex,
            $"{orientation} eye receives the first highest native slot");
        AssertEqual(38, slots[1].NativeSlotIndex,
            $"{orientation} main door receives the second native slot");
        AssertEqual(37, slots[2].NativeSlotIndex,
            $"{orientation} bottom receives the third native slot");
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(eyeBlock).CollisionType,
            $"{orientation} eye setup installs collision type C");
        AssertEqual(0x44, level.GetCollisionBlockByIndex(eyeBlock).Behavior,
            $"{orientation} eye setup installs BTS 44");
        AssertEqual((int)RoomCollisionType.SpikeBlock,
            (int)level.GetCollisionBlockByIndex(eyeBlock + roomWidth * 2).CollisionType,
            $"{orientation} main component setup installs collision type A");

        // Three blocks in front of the eye: near enough to open it, too far to make it close.
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new((eyeX + 3) * 16 + 8, eyeY * 16 + 8));
        for (int frame = 0; frame < 256 && !effects.Any(effect => effect.DefinitionPointer == EyeDoorProjectileDefinition.Projectile); frame++)
            StepEyeDoorPlms(plms, level, streamer);
        EyeDoorProjectileRequest shot = effects.First(effect => effect.DefinitionPointer == EyeDoorProjectileDefinition.Projectile);
        AssertEqual(EyeDoorPlmProgramDefinitions.ShotParameterOf(orientation), shot.Parameter, $"{orientation} attack operand");
        AssertEqual(eyeBlock, shot.PlmBlockIndex, $"{orientation} attack leaves the eye's block");
        AssertEqual(doorBit, shot.DoorBit, $"{orientation} attack carries the door bit");
        AssertEqual((ushort)EyeDoorPreInstruction.MissileHit,
            plms.EyeDoorSnapshots.Single(door => door.Component == EyeDoorComponent.Eye).PreInstruction,
            $"{orientation} open eye arms its missile wake");

        AssertTrue(plms.TryNotifyColoredDoorHit(
                eyeBlock,
                new SamusProjectileTypeWord(0x0000)),
            $"{orientation} beam collision reaches resident eye");
        StepEyeDoorPlms(plms, level, streamer);
        AssertTrue(plms.Sounds.SoundRequests.Any(request =>
                request.SoundEffect == SoundEffectId.FromCartridge(
                    SoundEffectLibrary.Library2,
                    EyeDoorPlmRomData.RejectedShotSound)),
            $"{orientation} rejected beam queues cartridge dud sound");
        AssertTrue(!system.HasOpenedDoorBit(doorBit),
            $"{orientation} beam cannot open eye door");

        int acceptedHits = useSuperMissile ? 1 : 3;
        ushort projectileWord = useSuperMissile ? (ushort)0x0200 : (ushort)0x0100;
        for (int hit = 0; hit < acceptedHits; hit++)
        {
            AssertTrue(plms.TryNotifyColoredDoorHit(
                    eyeBlock,
                    new SamusProjectileTypeWord(projectileWord)),
                $"{orientation} missile-family hit {hit + 1} reaches resident eye");
            StepEyeDoorPlms(plms, level, streamer);
            if (hit + 1 < acceptedHits && !effects.Any(effect => effect.DefinitionPointer == EyeDoorProjectileDefinition.Sweat))
            {
                // A hit that leaves the door shut ends its flinch with the eye's sweat.
                for (int frame = 0; frame < 128 && !effects.Any(effect => effect.DefinitionPointer == EyeDoorProjectileDefinition.Sweat); frame++)
                    StepEyeDoorPlms(plms, level, streamer);
                AssertEqual(EyeDoorPlmProgramDefinitions.SweatParameterOf(orientation),
                    effects.First(effect => effect.DefinitionPointer == EyeDoorProjectileDefinition.Sweat).Parameter,
                    $"{orientation} sweat operand");
            }
        }

        AssertTrue(system.HasOpenedDoorBit(doorBit),
            $"{orientation} accepted hit threshold persists door bit");
        for (int frame = 0; frame < 256 && plms.SlotTable.ActiveCount != 0; frame++)
            StepEyeDoorPlms(plms, level, streamer);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            $"{orientation} eye and both passive components complete together");
        AssertEqual(2, effects.Count(effect => effect.DefinitionPointer == EyeDoorProjectileDefinition.Smoke &&
                effect.Parameter == EyeDoorPlmRomData.CenteredSmokeParameter),
            $"{orientation} the destroyed eye puffs two centred smoke clouds");

        int capBlock = eyeBlock - roomWidth;
        RoomCollisionBlock cap = level.GetCollisionBlockByIndex(capBlock);
        AssertEqual((int)RoomCollisionType.ShootableBlock, (int)cap.CollisionType,
            $"{orientation} opening produces ordinary blue-door cap type");
        AssertEqual(
            orientation == EyeDoorOrientation.Right
                ? RoomBlockBehaviorValues.BlueDoorFacingRight.Value
                : RoomBlockBehaviorValues.BlueDoorFacingLeft.Value,
            cap.Behavior,
            $"{orientation} opening produces correct blue-door BTS");
        for (int row = 1; row <= EyeDoorPlmRomData.BlueDoorExtensionCount; row++)
        {
            RoomCollisionBlock extension = level.GetCollisionBlockByIndex(
                capBlock + row * roomWidth);
            AssertEqual((int)RoomCollisionType.VerticalExtension,
                (int)extension.CollisionType,
                $"{orientation} blue-door extension row {row} type");
            AssertEqual(unchecked((byte)-row), extension.Behavior,
                $"{orientation} blue-door extension row {row} BTS");
        }
    }

    /// <summary>Installs a compact eye, door and bottom-piece PLM population with native instruction entry points for either facing.</summary>
    internal static void SeedEyeDoorFixtureRom(
        TestAddressSpace bus,
        EyeDoorOrientation orientation)
    {
        ushort eyeHeader;
        ushort doorHeader;
        ushort bottomHeader;
        ushort convertInstruction;
        if (orientation == EyeDoorOrientation.Right)
        {
            eyeHeader = (ushort)PlmHeaderId.EyeDoorEyeFacingRight;
            doorHeader = (ushort)PlmHeaderId.EyeDoorFacingRight;
            bottomHeader = (ushort)PlmHeaderId.EyeDoorBottomFacingRight;
            convertInstruction = (ushort)RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight;
        }
        else
        {
            eyeHeader = (ushort)PlmHeaderId.EyeDoorEyeFacingLeft;
            doorHeader = (ushort)PlmHeaderId.EyeDoorFacingLeft;
            bottomHeader = (ushort)PlmHeaderId.EyeDoorBottomFacingLeft;
            convertInstruction = (ushort)RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft;
        }

        SeedRoomPlmPopulation(bus, 0x9000, [
            unchecked((byte)eyeHeader), SnesLittleEndian.HighByte(eyeHeader), 7, 4, 5, 0,
            unchecked((byte)doorHeader), SnesLittleEndian.HighByte(doorHeader), 7, 6, 5, 0,
            unchecked((byte)bottomHeader), SnesLittleEndian.HighByte(bottomHeader), 7, 8, 5, 0,
            0, 0,
        ]);
        SeedPlmProgramWords(bus, unchecked((ushort)(eyeHeader + 2)), 0x0400);
        SeedPlmProgramWords(bus, unchecked((ushort)(doorHeader + 2)), 0x0500);
        SeedPlmProgramWords(bus, unchecked((ushort)(bottomHeader + 2)), 0x0600);

        SeedPlmProgramBytes(bus, 0x0400, [
            0x72, 0x8a, 0x80, 0x04,
            0x24, 0x8a, 0x40, 0x04,
            0xc1, 0x86, 0x50, 0xbd,
            0x7a, 0xd7, 0x00, 0x00,
            0x90, 0xd7, 0x00, 0x00,
            0x9f, 0xd7,
            0xb6, 0xd7,
            0x01, 0x00, 0x00, 0xf0,
            0xb4, 0x86,
        ]);
        SeedPlmProgramBytes(bus, 0x0440, [
            0x91, 0x8a, 0x03, 0x80, 0x04,
            0x24, 0x87, 0x00, 0x04,
        ]);
        SeedPlmProgramWords(bus, 0x0480, convertInstruction);
        SeedPlmProgramWords(bus, 0x0482, (ushort)RoomPlmInstruction.Delete);

        SeedPassiveEyeDoorList(bus, 0x0500, 0x0540, 0xf010);
        SeedPassiveEyeDoorList(bus, 0x0600, 0x0640, 0xf020);
        WriteOneBlockDraw(bus, 0xf000, 0x8001);
        WriteOneBlockDraw(bus, 0xf010, 0x8002);
        WriteOneBlockDraw(bus, 0xf020, 0x8003);
    }

    /// <summary>Seeds one passive door-piece list that redirects on the opened bit and otherwise repeats its frame.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one passive eye-door instruction list")]
    private static void SeedPassiveEyeDoorList(
        TestAddressSpace bus,
        ushort list,
        ushort openedTarget,
        ushort draw)
    {
        int address = 0x840000 | list;
        bus.WriteBytes(address, [
            0x72, 0x8a, unchecked((byte)openedTarget), SnesLittleEndian.HighByte(openedTarget),
            0x24, 0x8a, unchecked((byte)openedTarget), SnesLittleEndian.HighByte(openedTarget),
            0xc1, 0x86, 0x53, 0xd7,
            0x01, 0x00, unchecked((byte)draw), SnesLittleEndian.HighByte(draw),
            0xb4, 0x86,
        ]);
        SeedPlmProgramWords(bus, openedTarget, (ushort)RoomPlmInstruction.Delete);
    }

    /// <summary>Advances linked PLMs with a fixed camera and no unrelated room event or input.</summary>
    private static void StepEyeDoorPlms(
        RoomPlmSystem plms,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer) =>
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

    /// <summary>Runs both facing fixtures and projectile behavior from the eye-door verification command.</summary>
    [VerificationCommand("--eye-door-plms")]
    internal static int EyeDoorPlmsCommand()
    {
        Suite(nameof(VerifyEyeDoorPlms), () => VerifyEyeDoorPlms());
        return 0;
    }
}
