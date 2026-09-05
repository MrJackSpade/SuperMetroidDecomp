using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Reproduces the exact four retail cannon records in a constructed 2x2-screen room.
    /// Tiny draw lists isolate setup, hit filtering, destruction, slot order, and the shared
    /// bank-$84-to-bank-$A5 control-word writes from unrelated Draygon combat timing.
    /// </summary>
    private static void VerifyDraygonCannonPlms()
    {
        const int roomWidth = 32;
        const int roomHeight = 32;
        var bus = new TestAddressSpace();
        SeedDraygonCannonFixture(bus);
        RoomLevelData level = CreateRoom(
            roomWidth,
            roomHeight,
            new ushort[roomWidth * roomHeight],
            new byte[roomWidth * roomHeight],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var disabledWords = new List<ushort>();
        var plms = new RoomPlmSystem();

        AssertEqual(4, plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            populationPointer: 0x9000,
            new Bank80SystemState(),
            AreaId.Maridia,
            getSamus: () => null,
            isAreaTorizoDefeated: () => false,
            disableDraygonCannon: disabledWords.Add),
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
            new ushort[] { DraygonCannonData.UpperLeftDisabledWord },
            disabledWords,
            "pre-destroyed cannon immediately writes its bank-$A5 control word");
        AssertDestroyedCannon(level, preDestroyedBlock, roomWidth, "pre-destroyed cannon");

        AssertTrue(plms.TryNotifyResidentProjectileHit(rightBlock, 0x0000),
            "beam collision reaches the right cannon PLM");
        StepDraygonCannonPlms(plms, bus, level, streamer);
        AssertEqual(1, disabledWords.Count, "beam cannot damage a Draygon cannon");

        for (int hit = 0; hit < 3; hit++)
        {
            AssertTrue(plms.TryNotifyResidentProjectileHit(rightBlock, 0x0100),
                $"missile hit {hit + 1} reaches right cannon");
            SettleDraygonCannonHit(plms, bus, level, streamer);
        }
        AssertTrue(disabledWords.Contains(DraygonCannonData.LowerLeftDisabledWord),
            "third missile disables the lower-left firing word");
        AssertDestroyedCannon(level, rightBlock, roomWidth, "right cannon");

        AssertTrue(plms.TryNotifyResidentProjectileHit(upperLeftBlock, 0x0200),
            "Super Missile collision reaches left cannon");
        SettleDraygonCannonHit(plms, bus, level, streamer);
        AssertTrue(disabledWords.Contains(DraygonCannonData.UpperRightDisabledWord),
            "one Super Missile disables the upper-right firing word");
        AssertDestroyedCannon(level, upperLeftBlock, roomWidth, "upper-left cannon");
        AssertTrue(!disabledWords.Contains(DraygonCannonData.LowerRightDisabledWord),
            "untouched lower-right cannon remains enabled");
        AssertEqual((int)RoomCollisionType.ShootableBlock,
            (int)level.GetCollisionBlockByIndex(lowerLeftBlock).CollisionType,
            "untouched lower-right cannon remains shootable");

        Console.WriteLine(
            "  Draygon cannons: retail slot order, setup, missile thresholds, terrain, and firing flags agree.");
    }

    private static void SeedDraygonCannonFixture(TestAddressSpace bus)
    {
        bus.WriteBytes(0x8f9000, [
            0x65, 0xdf, 2, 11, 0x02, 0x88,
            0x59, 0xdf, 2, 18, 0x04, 0x88,
            0x71, 0xdf, 29, 15, 0x06, 0x88,
            0x71, 0xdf, 29, 21, 0x08, 0x88,
            0, 0,
        ]);
        WriteWord(bus, 0x840000 | RoomPlmHeaders.DraygonCannonFacingRight + 2, 0xe000);
        WriteWord(bus, 0x840000 | RoomPlmHeaders.DraygonCannonFacingRightDestroyed + 2, 0xe040);
        WriteWord(bus, 0x840000 | RoomPlmHeaders.DraygonCannonFacingLeft + 2, 0xe100);

        SeedShieldedCannonList(
            bus,
            list: 0xe000,
            hitList: 0xe020,
            destroyedList: 0xe040,
            damageInstruction: RoomPlmInstructionCodes.DamageDraygonCannonFacingRight,
            idleDraw: 0xf000,
            hitDraw: 0xf010,
            destroyedDraw: 0xf020);
        SeedShieldedCannonList(
            bus,
            list: 0xe100,
            hitList: 0xe120,
            destroyedList: 0xe140,
            damageInstruction: RoomPlmInstructionCodes.DamageDraygonCannonFacingLeft,
            idleDraw: 0xf100,
            hitDraw: 0xf110,
            destroyedDraw: 0xf120);
    }

    private static void SeedShieldedCannonList(
        TestAddressSpace bus,
        ushort list,
        ushort hitList,
        ushort destroyedList,
        ushort damageInstruction,
        ushort idleDraw,
        ushort hitDraw,
        ushort destroyedDraw)
    {
        bus.WriteBytes(0x840000 | list, [
            0x24, 0x8a, unchecked((byte)hitList), unchecked((byte)(hitList >> 8)),
            0xc1, 0x86, 0x64, 0xdb,
            0x01, 0x00, unchecked((byte)idleDraw), unchecked((byte)(idleDraw >> 8)),
            0xb4, 0x86,
        ]);
        bus.WriteBytes(0x840000 | hitList, [
            0xcd, 0x8a, 0x03,
                unchecked((byte)destroyedList), unchecked((byte)(destroyedList >> 8)),
            0x01, 0x00, unchecked((byte)hitDraw), unchecked((byte)(hitDraw >> 8)),
            0x24, 0x87, unchecked((byte)list), unchecked((byte)(list >> 8)),
        ]);
        WriteWord(bus, 0x840000 | destroyedList, damageInstruction);
        bus.WriteBytes(0x840000 | unchecked((ushort)(destroyedList + 2)), [
            0x01, 0x00, unchecked((byte)destroyedDraw), unchecked((byte)(destroyedDraw >> 8)),
            0xb4, 0x86,
        ]);
        WriteOneBlockDraw(bus, idleDraw, 0xc044);
        WriteOneBlockDraw(bus, hitDraw, 0xc044);
        WriteOneBlockDraw(bus, destroyedDraw, 0xa003);
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
}
