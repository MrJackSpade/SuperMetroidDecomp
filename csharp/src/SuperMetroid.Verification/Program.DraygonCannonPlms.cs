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
using static DraygonCannonPlmProgramVerification;
using static SuiteTimingVerification;
using static SyntheticRoomCartridgeFixturesVerification;
using static VerificationHelpersVerification;

internal static class DraygonCannonPlmsVerification
{
    /// <summary>
    /// Reproduces the exact four retail cannon records in a constructed 2x2-screen room.
    /// Tiny draw lists isolate setup, hit filtering, destruction, slot order, and the shared
    /// bank-$84-to-bank-$A5 control-word writes from unrelated Draygon combat timing.
    /// </summary>
    internal static void VerifyDraygonCannonPlms()
    {
        Suite(nameof(VerifyDraygonCannonPlmProgram), () => VerifyDraygonCannonPlmProgram());
        const int roomWidth = 32;
        const int roomHeight = 32;
        var bus = new TestAddressSpace();
        SeedDraygonCannonFixture(bus);
        RoomLevelData level = CreateRoom(new(roomWidth, roomHeight),
            new ushort[roomWidth * roomHeight],
            new byte[roomWidth * roomHeight],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var disabledWords = new List<ushort>();
        var plms = new RoomPlmSystem();

        AssertEqual(4, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Maridia), RoomPlmPopulationImporter.Read(bus, pointer: 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { DraygonCannonDisabled = disabledWords.Add } }),
            "Draygon cannon population record count");

        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(4, slots.Length, "Draygon cannon native slot count");
        AssertEqual(39, slots[0].NativeSlotIndex, "pre-destroyed cannon receives first slot");
        AssertEqual(38, slots[1].NativeSlotIndex, "lower-left cannon receives second slot");
        AssertEqual(37, slots[2].NativeSlotIndex, "upper-right cannon receives third slot");
        AssertEqual(36, slots[3].NativeSlotIndex, "lower-right cannon receives fourth slot");

        int preDestroyedBlock = 11 * roomWidth + 2;
        int rightBlock = 18 * roomWidth + 2;
        int upperLeftBlock = 15 * roomWidth + 29;
        int lowerLeftBlock = 21 * roomWidth + 29;
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(rightBlock).CollisionType,
            "shielded right cannon setup installs type C");
        AssertEqual(0x44, level.GetCollisionBlockByIndex(rightBlock).Behavior,
            "shielded right cannon setup installs BTS 44");
        AssertEqual((int)RoomCollisionType.VerticalExtension,
            (int)level.GetCollisionBlockByIndex(rightBlock + roomWidth).CollisionType,
            "shielded right cannon setup installs its vertical extension");
        AssertEqual(0xff, level.GetCollisionBlockByIndex(rightBlock + roomWidth).Behavior,
            "shielded right cannon extension points to its origin");

        StepDraygonCannonPlms(plms, bus, level, streamer);
        AssertSequenceEqual(
            new ushort[] { (ushort)DraygonCannonControlWord.UpperLeft },
            disabledWords,
            "pre-destroyed cannon immediately writes its bank-$A5 control word");
        AssertDestroyedCannon(level, preDestroyedBlock, roomWidth, "pre-destroyed cannon");

        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(rightBlock, 0x0000),
            "beam collision reaches the right cannon PLM");
        StepDraygonCannonPlms(plms, bus, level, streamer);
        AssertEqual(1, disabledWords.Count, "beam cannot damage a Draygon cannon");

        for (int hit = 0; hit < 3; hit++)
        {
            AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(rightBlock, 0x0100),
                $"missile hit {hit + 1} reaches right cannon");
            SettleDraygonCannonHit(plms, bus, level, streamer);
        }
        AssertTrue(disabledWords.Contains((ushort)DraygonCannonControlWord.LowerLeft),
            "third missile disables the lower-left firing word");
        AssertDestroyedCannon(level, rightBlock, roomWidth, "right cannon");

        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(upperLeftBlock, 0x0200),
            "Super Missile collision reaches left cannon");
        SettleDraygonCannonHit(plms, bus, level, streamer);
        AssertTrue(disabledWords.Contains((ushort)DraygonCannonControlWord.UpperRight),
            "one Super Missile disables the upper-right firing word");
        AssertDestroyedCannon(level, upperLeftBlock, roomWidth, "upper-left cannon");
        AssertTrue(!disabledWords.Contains((ushort)DraygonCannonControlWord.LowerRight),
            "untouched lower-right cannon remains enabled");
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(lowerLeftBlock).CollisionType,
            "untouched lower-right cannon remains shootable");

        Console.WriteLine(
            "  Draygon cannons: retail slot order, setup, missile thresholds, terrain, and firing flags agree.");
    }

    private static void SeedDraygonCannonFixture(TestAddressSpace bus)
    {
        SeedRoomPlmPopulation(bus, 0x9000, [
            0x65, 0xdf, 2, 11, 0x02, 0x88,
            0x59, 0xdf, 2, 18, 0x04, 0x88,
            0x71, 0xdf, 29, 15, 0x06, 0x88,
            0x71, 0xdf, 29, 21, 0x08, 0x88,
            0, 0,
        ]);
        SeedPlmProgramWords(bus, unchecked((ushort)((ushort)PlmHeaderId.DraygonCannonFacingRight + 2)), 0x0400);
        SeedPlmProgramWords(bus, unchecked((ushort)((ushort)PlmHeaderId.DraygonCannonFacingRightDestroyed + 2)), 0x0440);
        SeedPlmProgramWords(bus, unchecked((ushort)((ushort)PlmHeaderId.DraygonCannonFacingLeft + 2)), 0x0500);

        SeedShieldedCannonList(bus, new ShieldedCannonLists { List = 0x0400, HitList = 0x0420, DestroyedList = 0x0440, DamageInstruction = (ushort)RoomPlmInstruction.DamageDraygonCannonFacingRight, IdleDraw = 0xf000, HitDraw = 0xf010, DestroyedDraw = 0xf020 });
        SeedShieldedCannonList(bus, new ShieldedCannonLists { List = 0x0500, HitList = 0x0520, DestroyedList = 0x0540, DamageInstruction = (ushort)RoomPlmInstruction.DamageDraygonCannonFacingLeft, IdleDraw = 0xf100, HitDraw = 0xf110, DestroyedDraw = 0xf120 });
    }

    private static void SeedShieldedCannonList(TestAddressSpace bus, ShieldedCannonLists lists)
    {
        SeedPlmProgramBytes(bus, lists.List, [
            0x24, 0x8a, unchecked((byte)lists.HitList), SnesLittleEndian.HighByte(lists.HitList),
            0xc1, 0x86, 0x64, 0xdb,
            0x01, 0x00, unchecked((byte)lists.IdleDraw), SnesLittleEndian.HighByte(lists.IdleDraw),
            0xb4, 0x86,
        ]);
        SeedPlmProgramBytes(bus, lists.HitList, [
            0xcd, 0x8a, 0x03,
                unchecked((byte)lists.DestroyedList), SnesLittleEndian.HighByte(lists.DestroyedList),
            0x01, 0x00, unchecked((byte)lists.HitDraw), SnesLittleEndian.HighByte(lists.HitDraw),
            0x24, 0x87, unchecked((byte)lists.List), SnesLittleEndian.HighByte(lists.List),
        ]);
        SeedPlmProgramWords(bus, lists.DestroyedList, lists.DamageInstruction);
        SeedPlmProgramBytes(bus, unchecked((ushort)(lists.DestroyedList + 2)), [
            0x01, 0x00, unchecked((byte)lists.DestroyedDraw), SnesLittleEndian.HighByte(lists.DestroyedDraw),
            0xb4, 0x86,
        ]);
        WriteOneBlockDraw(bus, lists.IdleDraw, 0xc044);
        WriteOneBlockDraw(bus, lists.HitDraw, 0xc044);
        WriteOneBlockDraw(bus, lists.DestroyedDraw, 0xa003);
    }

    private static void SettleDraygonCannonHit(
        RoomPlmSystem plms,
        TestAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer)
    {
        for (int frame = 0; frame < 3; frame++)
            StepDraygonCannonPlms(plms, bus, level, streamer);
    }

    private static void StepDraygonCannonPlms(
        RoomPlmSystem plms,
        TestAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer) =>
        plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

    private static void AssertDestroyedCannon(
        RoomLevelData level,
        int blockIndex,
        int roomWidth,
        string context)
    {
        foreach (int cell in new[] { blockIndex, blockIndex + roomWidth })
        {
            AssertEqual((int)RoomCollisionType.SpikeBlock,
                (int)level.GetCollisionBlockByIndex(cell).CollisionType,
                $"{context} replaces cell {cell} with type A");
            AssertEqual(0x03, level.GetCollisionBlockByIndex(cell).Behavior,
                $"{context} replaces cell {cell} with BTS 03");
        }
    }

    [VerificationCommand("--draygon-cannon-plms")]
    internal static int DraygonCannonPlmsCommand()
    {
        Suite(nameof(VerifyDraygonCannonPlms), () => VerifyDraygonCannonPlms());
        return 0;
    }
}
