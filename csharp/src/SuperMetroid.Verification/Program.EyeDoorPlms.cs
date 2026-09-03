using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Reproduces both mirrored eye-door families in constructed rooms. The fixture uses
    /// the retail instruction entry points but deliberately tiny frame lists, isolating the
    /// shared PLM state machine from unrelated room traversal and graphics streaming.
    /// </summary>
    private static void VerifyEyeDoorPlms()
    {
        VerifyEyeDoorOrientation(EyeDoorOrientation.Right, useSuperMissile: false);
        VerifyEyeDoorOrientation(EyeDoorOrientation.Left, useSuperMissile: true);
        VerifyEyeDoorEnemyProjectiles();
        Console.WriteLine(
            "  Eye doors: mirrored setup, slot order, effects, shot filters, persistence, and blue-door conversion agree.");
    }

    private static void VerifyEyeDoorEnemyProjectiles()
    {
        var bus = new TestAddressSpace();
        bus.WriteBytes(0xa19600, [0xff, 0xff]);
        SeedEyeDoorEnemyProjectileRom(bus);
        RoomLevelData level = CreateRoom(
            16,
            16,
            new ushort[16 * 16],
            new byte[16 * 16],
            blockDefinitions: new byte[0x400 * 8]);
        var samus = new SamusState { XPosition = 220, YPosition = 80 };
        int randomAdvances = 0;
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            populationPointer: 0x9600,
            tilesetPointer: 0,
            new SnesVram(),
            new SnesCgram(),
            nextRandom: () => { randomAdvances++; return 0x7777; },
            readRandomNumber: () => 0x3412,
            level: level,
            samus: samus);
        var system = new Bank80SystemState();
        int plmBlock = 4 * 16 + 7;

        enemies.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorEnemyProjectileRomData.ProjectileDefinition,
                Parameter: 0,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot attack = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorProjectile);
        AssertEqual((ushort)104, attack.XPosition,
            "eye-door attack initializer applies cartridge PLM X offset");
        AssertEqual((ushort)80, attack.YPosition,
            "eye-door attack initializer applies cartridge PLM Y offset");
        enemies.StepEnemyProjectiles(level, samus);
        AssertEqual((ushort)0x0080, attack.Variable0,
            "direction opcode stores twice the rightward byte angle");
        AssertEqual((ushort)0x0100, attack.XVelocity,
            "direction opcode seeds rightward signed 8.8 X velocity");
        AssertEqual((ushort)0, attack.YVelocity,
            "direction opcode seeds zero Y velocity for a level target");
        system.SetOpenedDoorBit(5);
        enemies.StepEnemyProjectiles(level, samus);
        AssertEqual(EyeDoorEnemyProjectileRomData.SmokeInertPreInstruction,
            attack.PreInstruction,
            "opened door switches attack into its impact animation");

        enemies.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorEnemyProjectileRomData.SweatDefinition,
                Parameter: 4,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot sweat = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertEqual((ushort)64, sweat.XVelocity,
            "mirrored sweat parameter selects positive cartridge X velocity");
        AssertEqual((ushort)512, sweat.YVelocity,
            "sweat parameter retains cartridge falling velocity");

        enemies.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorEnemyProjectileRomData.SmokeDefinition,
                Parameter: 0x030a,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot smoke = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
        AssertEqual((ushort)0xb800, smoke.InstructionPointer,
            "smoke low parameter byte indexes the cartridge instruction-list pointer table");
        AssertEqual((ushort)123, smoke.XPosition,
            "smoke X uses low random byte and selected mask/base tuple");
        AssertEqual((ushort)74, smoke.YPosition,
            "smoke Y uses high random byte and selected mask/base tuple");
        AssertEqual(1, randomAdvances,
            "smoke advances cartridge RNG once after sampling current seed");
    }

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
        RoomLevelData level = CreateRoom(
            roomWidth,
            roomHeight,
            new ushort[roomWidth * roomHeight],
            new byte[roomWidth * roomHeight],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            XPosition = eyeX * 16 + 8,
            YPosition = eyeY * 16 + 8,
        };
        var effects = new List<EyeDoorProjectileRequest>();
        var plms = new RoomPlmSystem();
        AssertEqual(3, plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            populationPointer: 0x9000,
            system,
            AreaId.Brinstar,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false,
            spawnEyeDoorProjectile: effects.Add),
            $"{orientation} eye-door population record count");

        AssertEqual(3, plms.ActiveCount, $"{orientation} eye door retains three PLM slots");
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

        StepEyeDoorPlms(plms, bus, level, streamer);
        AssertEqual(5, effects.Count,
            $"{orientation} active eye emits attack, sweat, and three smoke requests");
        AssertEqual(EyeDoorEnemyProjectileRomData.ProjectileDefinition,
            effects[0].DefinitionPointer, $"{orientation} attack definition");
        AssertEqual(EyeDoorEnemyProjectileRomData.SweatDefinition,
            effects[1].DefinitionPointer, $"{orientation} sweat definition");
        AssertEqual(3, effects.Count(effect =>
                effect.DefinitionPointer == EyeDoorEnemyProjectileRomData.SmokeDefinition),
            $"{orientation} smoke request count");

        AssertTrue(plms.TryNotifyColoredDoorHit(
                eyeBlock,
                new SamusProjectileTypeWord(0x0000)),
            $"{orientation} beam collision reaches resident eye");
        StepEyeDoorPlms(plms, bus, level, streamer);
        AssertTrue(plms.SoundRequests.Any(request =>
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
            StepEyeDoorPlms(plms, bus, level, streamer);
        }

        AssertTrue(system.HasOpenedDoorBit(doorBit),
            $"{orientation} accepted hit threshold persists door bit");
        AssertEqual(0, plms.ActiveCount,
            $"{orientation} eye and both passive components complete together");

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

    private static void SeedEyeDoorFixtureRom(
        TestAddressSpace bus,
        EyeDoorOrientation orientation)
    {
        ushort eyeHeader;
        ushort doorHeader;
        ushort bottomHeader;
        ushort convertInstruction;
        if (orientation == EyeDoorOrientation.Right)
        {
            eyeHeader = RoomPlmHeaders.EyeDoorEyeFacingRight;
            doorHeader = RoomPlmHeaders.EyeDoorFacingRight;
            bottomHeader = RoomPlmHeaders.EyeDoorBottomFacingRight;
            convertInstruction = RoomPlmInstructionCodes.MoveUpAndMakeBlueDoorFacingRight;
        }
        else
        {
            eyeHeader = RoomPlmHeaders.EyeDoorEyeFacingLeft;
            doorHeader = RoomPlmHeaders.EyeDoorFacingLeft;
            bottomHeader = RoomPlmHeaders.EyeDoorBottomFacingLeft;
            convertInstruction = RoomPlmInstructionCodes.MoveUpAndMakeBlueDoorFacingLeft;
        }

        bus.WriteBytes(0x8f9000, [
            unchecked((byte)eyeHeader), unchecked((byte)(eyeHeader >> 8)), 7, 4, 5, 0,
            unchecked((byte)doorHeader), unchecked((byte)(doorHeader >> 8)), 7, 6, 5, 0,
            unchecked((byte)bottomHeader), unchecked((byte)(bottomHeader >> 8)), 7, 8, 5, 0,
            0, 0,
        ]);
        WriteWord(bus, 0x840000 | eyeHeader + 2, 0xe000);
        WriteWord(bus, 0x840000 | doorHeader + 2, 0xe100);
        WriteWord(bus, 0x840000 | bottomHeader + 2, 0xe200);

        bus.WriteBytes(0x84e000, [
            0x72, 0x8a, 0x80, 0xe0,
            0x24, 0x8a, 0x40, 0xe0,
            0xc1, 0x86, 0x50, 0xbd,
            0x7a, 0xd7, 0x00, 0x00,
            0x90, 0xd7, 0x00, 0x00,
            0x9f, 0xd7,
            0xb6, 0xd7,
            0x01, 0x00, 0x00, 0xf0,
            0xb4, 0x86,
        ]);
        bus.WriteBytes(0x84e040, [
            0x91, 0x8a, 0x03, 0x80, 0xe0,
            0x24, 0x87, 0x00, 0xe0,
        ]);
        WriteWord(bus, 0x84e080, convertInstruction);
        WriteWord(bus, 0x84e082, RoomPlmInstructionCodes.Delete);

        SeedPassiveEyeDoorList(bus, 0xe100, 0xe140, 0xf010);
        SeedPassiveEyeDoorList(bus, 0xe200, 0xe240, 0xf020);
        WriteOneBlockDraw(bus, 0xf000, 0x8001);
        WriteOneBlockDraw(bus, 0xf010, 0x8002);
        WriteOneBlockDraw(bus, 0xf020, 0x8003);
    }

    private static void SeedPassiveEyeDoorList(
        TestAddressSpace bus,
        ushort list,
        ushort openedTarget,
        ushort draw)
    {
        int address = 0x840000 | list;
        bus.WriteBytes(address, [
            0x72, 0x8a, unchecked((byte)openedTarget), unchecked((byte)(openedTarget >> 8)),
            0x24, 0x8a, unchecked((byte)openedTarget), unchecked((byte)(openedTarget >> 8)),
            0xc1, 0x86, 0x53, 0xd7,
            0x01, 0x00, unchecked((byte)draw), unchecked((byte)(draw >> 8)),
            0xb4, 0x86,
        ]);
        WriteWord(bus, 0x840000 | openedTarget, RoomPlmInstructionCodes.Delete);
    }

    private static void SeedEyeDoorEnemyProjectileRom(TestAddressSpace bus)
    {
        // Minimal definitions preserve the retail initializer identities while routing all
        // animation into a tiny deterministic list owned by this constructed-room test.
        bus.WriteBytes(0x86b743, [
            0x2d, 0xb6, 0x00, 0x00, 0x00, 0xb8, 0x04, 0x04,
            0x00, 0x00, 0x00, 0x00, 0xf3, 0xb5,
        ]);
        bus.WriteBytes(0x86b751, [
            0x83, 0xb6, 0x14, 0xb7, 0x00, 0xb8, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00, 0xfc, 0x84,
        ]);
        bus.WriteBytes(0x86e517, [
            0xa6, 0xe4, 0x08, 0xe5, 0x00, 0xb8, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0xfc, 0x84,
        ]);
        bus.WriteBytes(0x86b800, [
            0x61, 0x81, 0xb9, 0xb6,
            0xa5, 0x82,
            0x10, 0x00, 0x00, 0x80,
        ]);
        bus.WriteBytes(0x86b5f3, [0x10, 0x00, 0x00, 0x80]);

        // Rightward angle $40 reads sine[$40] for X and sine[$00] for Y.
        WriteWord(bus, EnemyRomTablePointers.Common.SignedSineCosineWords, 0);
        WriteWord(bus, EnemyRomTablePointers.Common.SignedSineCosineWords + 0x80, 0x0100);

        WriteWord(bus,
            EyeDoorEnemyProjectileRomData.BankBase |
            (EyeDoorEnemyProjectileRomData.SmokeInstructionListTable + 0x0a * 2),
            0xb800);
        int smokeTuple = EyeDoorEnemyProjectileRomData.BankBase |
            (EyeDoorEnemyProjectileRomData.SmokeOffsetTable + 3 * 8);
        WriteWord(bus, smokeTuple, 0x000f);
        WriteWord(bus, smokeTuple + 2, 0x0007);
        WriteWord(bus, smokeTuple + 4, 1);
        WriteWord(bus, smokeTuple + 6, unchecked((ushort)-2));
    }

    private static void StepEyeDoorPlms(
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
