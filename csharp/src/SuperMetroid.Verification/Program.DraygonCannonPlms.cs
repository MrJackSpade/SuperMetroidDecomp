using SuperMetroid.AssetExtraction;
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
        Suite(nameof(VerifyDraygonCannonPlmProgram), () => VerifyDraygonCannonPlmProgram());
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
            RoomPlmPopulationImporter.Read(bus, pointer: 0x9000),
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

    /// <summary>Writes the four retail-order cannon population records and seeds their facing-specific instruction and draw lists.</summary>
    /// <param name="bus">Fixture address space receiving the room population and PLM program bytes.</param>
    private static void SeedDraygonCannonFixture(TestAddressSpace bus)
    {
        bus.WriteBytes(0x8f9000, [
            0x65, 0xdf, 2, 11, 0x02, 0x88,
            0x59, 0xdf, 2, 18, 0x04, 0x88,
            0x71, 0xdf, 29, 15, 0x06, 0x88,
            0x71, 0xdf, 29, 21, 0x08, 0x88,
            0, 0,
        ]);
        WriteWord(bus, 0x840000 | RoomPlmHeaders.DraygonCannonFacingRight + 2, 0x0400);
        WriteWord(bus, 0x840000 | RoomPlmHeaders.DraygonCannonFacingRightDestroyed + 2, 0x0440);
        WriteWord(bus, 0x840000 | RoomPlmHeaders.DraygonCannonFacingLeft + 2, 0x0500);

        SeedShieldedCannonList(
            bus,
            list: 0x0400,
            hitList: 0x0420,
            destroyedList: 0x0440,
            damageInstruction: RoomPlmInstructionCodes.DamageDraygonCannonFacingRight,
            idleDraw: 0xf000,
            hitDraw: 0xf010,
            destroyedDraw: 0xf020);
        SeedShieldedCannonList(
            bus,
            list: 0x0500,
            hitList: 0x0520,
            destroyedList: 0x0540,
            damageInstruction: RoomPlmInstructionCodes.DamageDraygonCannonFacingLeft,
            idleDraw: 0xf100,
            hitDraw: 0xf110,
            destroyedDraw: 0xf120);
    }

    /// <summary>Builds a shielded cannon's idle, hit, and destroyed programs with one-block draw lists.</summary>
    /// <param name="bus">Fixture address space that receives the instruction and draw bytes.</param>
    /// <param name="list">Address of the shielded cannon's idle instruction list.</param>
    /// <param name="hitList">Address of the list entered after a qualifying missile hit.</param>
    /// <param name="destroyedList">Address of the terminal list entered when the cannon is destroyed.</param>
    /// <param name="damageInstruction">Facing-specific callback that disables the corresponding cannon firing word.</param>
    /// <param name="idleDraw">Draw-list address used by the intact cannon.</param>
    /// <param name="hitDraw">Draw-list address used while the cannon reacts to a hit.</param>
    /// <param name="destroyedDraw">Draw-list address used for the destroyed cannon tiles.</param>
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

    /// <summary>Advances the PLM system for the hit animation's three frames so its damage callback can settle.</summary>
    /// <param name="plms">PLM system containing the struck cannon.</param>
    /// <param name="bus">Address space used by PLM execution.</param>
    /// <param name="level">Room collision and block state modified by the cannon.</param>
    /// <param name="streamer">Background tilemap streamer used during each PLM step.</param>
    private static void SettleDraygonCannonHit(
        RoomPlmSystem plms,
        TestAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer)
    {
        for (int frame = 0; frame < 3; frame++)
            StepDraygonCannonPlms(plms, bus, level, streamer);
    }

    /// <summary>Runs one PLM update with fixed zero scroll, no enemy deaths, and no new controller input.</summary>
    /// <param name="plms">PLM system to update.</param>
    /// <param name="bus">Address space supplying the active PLM instructions.</param>
    /// <param name="level">Room state supplied to the update.</param>
    /// <param name="streamer">Background tilemap streamer supplied to the update.</param>
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

    /// <summary>Checks that a destroyed cannon replaces its two vertical room cells with native spike-block collision.</summary>
    /// <param name="level">Room level whose collision cells are inspected.</param>
    /// <param name="blockIndex">Index of the cannon's upper cell in the room block array.</param>
    /// <param name="roomWidth">Number of blocks per row, used to locate the lower cell.</param>
    /// <param name="context">Label included in assertion failures to identify the cannon state.</param>
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
