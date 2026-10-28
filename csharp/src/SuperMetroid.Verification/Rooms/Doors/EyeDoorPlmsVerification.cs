using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;

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
            new byte[16 * 16]);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(220, 80)) };
        int randomAdvances = 0;
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: bus, services: new StandaloneRoomEnemyServices(() => { randomAdvances++; return 0x7777; }) { ReadRandom = () => 0x3412 });
        enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(0x9600), TestAddressSpace.UnusedGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new GameplayColours(new SpriteColourSets())) { Level = level, Samus = samus });
        var system = new SharedGameState();
        int plmBlock = 4 * 16 + 7;

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.Projectile,
                Facing: EyeDoorOrientation.Left,
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
        AssertEqual(0x0080, attack.Variable0,
            "direction opcode stores twice the rightward byte angle");
        AssertEqual((ushort)0x0100, attack.Velocity.X.RawValue,
            "direction opcode seeds rightward signed 8.8 X velocity");
        AssertEqual((ushort)0, attack.Velocity.Y.RawValue,
            "direction opcode seeds zero Y velocity for a level target");
        system.SetOpenedDoorBit(5);
        enemies.StepEnemyProjectiles(level, samus);
        AssertEqual(EnemyProjectilePreInstruction.RTS_868170,
            attack.PreInstruction,
            "opened door impact executes its native clear-pre-instruction command");

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.Sweat,
                Facing: EyeDoorOrientation.Right,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot sweat = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorSweat);
        AssertEqual((ushort)64, sweat.Velocity.X.RawValue,
            "a right-facing eye launches its sweat with positive cartridge X velocity");
        AssertEqual((ushort)512, sweat.Velocity.Y.RawValue,
            "the sweat keeps the cartridge falling velocity");

        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.ScatteredSmoke,
                Facing: EyeDoorOrientation.Left,
                PlmBlockIndex: plmBlock,
                DoorBit: 5),
            roomWidthInBlocks: 16,
            system);
        RoomEnemyProjectileSlot smoke = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
        AssertTrue(smoke.Animation is MiscDustProjectileAnimation { Cursor.Program: var smokeProgram } &&
                smokeProgram == MiscDustProjectiles.Program(MiscDustAnimation.CorpseDustCloud),
            "scattered smoke shows the corpse dust cloud, the cartridge's misc-dust selector ten");
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
            new byte[roomWidth * roomHeight]);
        var system = new SharedGameState();
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(eyeX * 16 + 8, eyeY * 16 + 8)),
        };
        var effects = new List<EyeDoorProjectileRequest>();
        var plms = PlmFixtures.Unbound();
        AssertEqual(3, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Brinstar), RoomPlmPopulationImporter.Read(bus, pointer: 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { EyeDoorProjectile = effects.Add } }),
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
        AssertEqual(0x44, level.GetCollisionBlockByIndex(eyeBlock).Bts.Value,
            $"{orientation} eye setup installs BTS 44");
        AssertEqual((int)RoomCollisionType.SpikeBlock,
            (int)level.GetCollisionBlockByIndex(eyeBlock + roomWidth * 2).CollisionType,
            $"{orientation} main component setup installs collision type A");

        // Three blocks in front of the eye: near enough to open it, too far to make it close.
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new((eyeX + 3) * 16 + 8, eyeY * 16 + 8));
        for (int frame = 0; frame < 256 && !effects.Any(effect => effect.Definition == EyeDoorProjectileDefinition.Projectile); frame++)
            StepEyeDoorPlms(plms, level);
        EyeDoorProjectileRequest shot = effects.First(effect => effect.Definition == EyeDoorProjectileDefinition.Projectile);
        AssertEqual(orientation, shot.Facing, $"{orientation} attack carries the eye's facing");
        AssertEqual(eyeBlock, shot.PlmBlockIndex, $"{orientation} attack leaves the eye's block");
        AssertEqual(doorBit, shot.DoorBit, $"{orientation} attack carries the door bit");
        AssertEqual(DoorsNativeWords.EyeDoorPreInstructions.Word(EyeDoorPreInstruction.MissileHit),
            plms.EyeDoorSnapshots.Single(door => door.Component == EyeDoorComponent.Eye).PreInstruction,
            $"{orientation} open eye arms its missile wake");

        AssertTrue(plms.TryNotifyColoredDoorHit(
                eyeBlock,
                new SamusProjectileTypeWord(0x0000)),
            $"{orientation} beam collision reaches resident eye");
        StepEyeDoorPlms(plms, level);
        AssertTrue(plms.Sounds.SoundRequests.Any(request =>
                request.SoundEffect == EyeDoorDefinitions.RejectedShotSound),
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
            StepEyeDoorPlms(plms, level);
            if (hit + 1 < acceptedHits && !effects.Any(effect => effect.Definition == EyeDoorProjectileDefinition.Sweat))
            {
                // A hit that leaves the door shut ends its flinch with the eye's sweat.
                for (int frame = 0; frame < 128 && !effects.Any(effect => effect.Definition == EyeDoorProjectileDefinition.Sweat); frame++)
                    StepEyeDoorPlms(plms, level);
                AssertEqual(orientation,
                    effects.First(effect => effect.Definition == EyeDoorProjectileDefinition.Sweat).Facing,
                    $"{orientation} sweat carries the eye's facing");
            }
        }

        AssertTrue(system.HasOpenedDoorBit(doorBit),
            $"{orientation} accepted hit threshold persists door bit");
        for (int frame = 0; frame < 256 && plms.SlotTable.ActiveCount != 0; frame++)
            StepEyeDoorPlms(plms, level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            $"{orientation} eye and both passive components complete together");
        AssertEqual(2, effects.Count(effect => effect.Definition == EyeDoorProjectileDefinition.CenteredSmoke),
            $"{orientation} the destroyed eye puffs two centred smoke clouds");

        int capBlock = eyeBlock - roomWidth;
        RoomCollisionBlock cap = level.GetCollisionBlockByIndex(capBlock);
        AssertEqual((int)RoomCollisionType.ShootableBlock, (int)cap.CollisionType,
            $"{orientation} opening produces ordinary blue-door cap type");
        AssertEqual(
            orientation == EyeDoorOrientation.Right
                ? RoomBlockBehaviorValues.BlueDoorFacingRight.Value
                : RoomBlockBehaviorValues.BlueDoorFacingLeft.Value,
            cap.Bts.Value,
            $"{orientation} opening produces correct blue-door BTS");
        for (int row = 1; row <= EyeDoorDefinitions.BlueDoorExtensionCount; row++)
        {
            RoomCollisionBlock extension = level.GetCollisionBlockByIndex(
                capBlock + row * roomWidth);
            AssertEqual((int)RoomCollisionType.VerticalExtension,
                (int)extension.CollisionType,
                $"{orientation} blue-door extension row {row} type");
            AssertEqual(unchecked((byte)-row), extension.Bts.Value,
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
            eyeHeader = PlmNativeWords.Word(PlmHeaderId.EyeDoorEyeFacingRight);
            doorHeader = PlmNativeWords.Word(PlmHeaderId.EyeDoorFacingRight);
            bottomHeader = PlmNativeWords.Word(PlmHeaderId.EyeDoorBottomFacingRight);
            convertInstruction = (ushort)RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight;
        }
        else
        {
            eyeHeader = PlmNativeWords.Word(PlmHeaderId.EyeDoorEyeFacingLeft);
            doorHeader = PlmNativeWords.Word(PlmHeaderId.EyeDoorFacingLeft);
            bottomHeader = PlmNativeWords.Word(PlmHeaderId.EyeDoorBottomFacingLeft);
            convertInstruction = (ushort)RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft;
        }

        SeedRoomPlmPopulation(bus, 0x9000, [
            unchecked((byte)eyeHeader), LittleEndianBytes.HighByte(eyeHeader), 7, 4, 5, 0,
            unchecked((byte)doorHeader), LittleEndianBytes.HighByte(doorHeader), 7, 6, 5, 0,
            unchecked((byte)bottomHeader), LittleEndianBytes.HighByte(bottomHeader), 7, 8, 5, 0,
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
    private static void SeedPassiveEyeDoorList(
        TestAddressSpace bus,
        ushort list,
        ushort openedTarget,
        ushort draw)
    {
        int address = 0x840000 | list;
        bus.WriteBytes(address, [
            0x72, 0x8a, unchecked((byte)openedTarget), LittleEndianBytes.HighByte(openedTarget),
            0x24, 0x8a, unchecked((byte)openedTarget), LittleEndianBytes.HighByte(openedTarget),
            0xc1, 0x86, 0x53, 0xd7,
            0x01, 0x00, unchecked((byte)draw), LittleEndianBytes.HighByte(draw),
            0xb4, 0x86,
        ]);
        SeedPlmProgramWords(bus, openedTarget, (ushort)RoomPlmInstruction.Delete);
    }

    /// <summary>Advances linked PLMs with a fixed camera and no unrelated room event or input.</summary>
    private static void StepEyeDoorPlms(
        RoomPlmSystem plms,
        RoomLevelData level) =>
        plms.Step(level, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

    /// <summary>Runs both facing fixtures and projectile behavior from the eye-door verification command.</summary>
    [VerificationCommand("--eye-door-plms")]
    internal static int EyeDoorPlmsCommand()
    {
        Suite(nameof(VerifyEyeDoorPlms), () => VerifyEyeDoorPlms());
        return 0;
    }
}
