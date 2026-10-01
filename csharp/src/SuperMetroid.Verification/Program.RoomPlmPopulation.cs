using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyElevatorPlatformPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort Word(ISnesAddressSpace bus, ushort address) =>
            unchecked((ushort)(bus.ReadByte(0x840000 | address) |
                bus.ReadByte(0x840000 | unchecked((ushort)(address + 1))) << 8));

        foreach ((ushort address, ushort value) in ElevatorPlatformPlmDefinitions.ProgramWords)
        {
            AssertEqual(Word(rom, address), value,
                $"elevator platform program word $84:{address:X4} matches cartridge");
            AssertTrue(ElevatorPlatformPlmDefinitions.TryReadMechanicsWord(
                    address, out ushort installed) && installed == value,
                $"elevator platform program word $84:{address:X4} is installed");
        }
        AssertTrue(!ElevatorPlatformPlmDefinitions.TryReadMechanicsWord(
                unchecked((ushort)(ElevatorPlatformPlmDefinitions.InstructionLoop - 2)),
                out _),
            "elevator program does not claim preceding cartridge data");

        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                 ElevatorPlatformPlmDefinitions.DrawLists)
        {
            ushort cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertEqual(Word(rom, cursor), run.DirectionAndCount,
                    $"elevator draw ${draw.Pointer:X4} run count at ${cursor:X4}");
                cursor += 2;
                foreach (ushort levelWord in run.LevelWords.Span)
                {
                    AssertEqual(Word(rom, cursor), levelWord,
                        $"elevator draw ${draw.Pointer:X4} level word at ${cursor:X4}");
                    cursor += 2;
                }
                AssertEqual(rom.ReadByte(0x840000 | cursor),
                    unchecked((byte)run.NextX),
                    $"elevator draw ${draw.Pointer:X4} next X at ${cursor:X4}");
                AssertEqual(rom.ReadByte(0x840000 | unchecked((ushort)(cursor + 1))),
                    unchecked((byte)run.NextY),
                    $"elevator draw ${draw.Pointer:X4} next Y at ${cursor + 1:X4}");
                cursor += 2;
            }
            AssertEqual((ushort)0, Word(rom, unchecked((ushort)(cursor - 2))),
                $"elevator draw ${draw.Pointer:X4} terminates with a zero relative offset");
        }
        Console.WriteLine("Elevator platform PLM: native instruction loop and three complete draw lists match cartridge.");
    }

    private static void VerifyDoorClosingPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

        for (byte direction = 0; direction < DoorClosingPlmRomData.DirectionCount; direction++)
        {
            DoorClosingPlmDefinition definition =
                DoorClosingPlmRomData.GetDefinition(direction);
            ushort expectedHeader = ReadWord(
                rom,
                DoorClosingPlmRomData.HeaderTableAddress + direction * sizeof(ushort));
            AssertEqual(expectedHeader, definition.Header,
                $"door-closing direction {direction} header matches cartridge");
            ushort expectedList = expectedHeader == 0
                ? (ushort)0
                : ReadWord(rom, 0x840000 | unchecked((ushort)(expectedHeader + 2)));
            AssertEqual(expectedList, definition.InitialInstructionList,
                $"door-closing direction {direction} initial list matches cartridge");

            var plms = new RoomPlmSystem();
            RoomLevelData level = CreateRoom(
                4,
                4,
                new ushort[16],
                new byte[16],
                blockDefinitions: new byte[0x400 * 8]);
            var door = new CartridgeDoorHeader(
                Pointer: 0,
                DestinationRoomPointer: 0,
                BitFlags: 0,
                Orientation: direction,
                PlmX: 1,
                PlmY: 1,
                DestinationScreenX: 0,
                DestinationScreenY: 0,
                SamusDistance: 0,
                SetupCodePointer: 0);
            bool spawned = plms.TrySpawnDoorClosingPlm(
                new TestAddressSpace(), level, door, new Bank80SystemState());
            AssertEqual(expectedHeader != 0, spawned,
                $"door-closing direction {direction} fallback admission matches cartridge");
            if (expectedHeader == 0)
                continue;
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(expectedHeader, slot.HeaderPointer,
                $"door-closing direction {direction} installs compiled header");
            AssertEqual(expectedList, slot.InstructionPointer,
                $"door-closing direction {direction} installs compiled initial list");
        }

        AssertThrows<InvalidDataException>(
            () => DoorClosingPlmRomData.GetDefinition(DoorClosingPlmRomData.DirectionCount),
            "out-of-range door-closing direction fails loudly");
        VerifyResidentDoorClosingDefinitions(rom);
        VerifyMotherBrainEscapeGateCompiledDefinitions(rom);
        VerifyEscapeGateVisuals(rom);
        VerifySequentialRoomPlmPopulationLoader();
        Console.WriteLine(
            "Door-closing definitions: all twelve fallback and eighteen resident selections match.");
    }

    /// <summary>
    /// Verifies the fixed header+4 relationship for every resident grey/coloured door,
    /// then exercises the real room-population and transition redirect with that source
    /// word deliberately absent from the sparse bus.
    /// </summary>
    private static void VerifyResidentDoorClosingDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

        AssertEqual(ResidentDoorClosingDefinitions.Count,
            ResidentDoorClosingDefinitions.All.Length,
            "resident-door definition count");

        int definitionIndex = 0;
        foreach (ResidentDoorClosingDefinition definition in
                 ResidentDoorClosingDefinitions.All)
        {
            ushort expected = ReadWord(
                rom,
                0x840000 | unchecked((ushort)(definition.Header + 4)));
            AssertEqual(expected, definition.ClosingInstructionList,
                $"resident door $84:{definition.Header:X4} closing list matches cartridge");
            AssertEqual(expected,
                ResidentDoorClosingDefinitions.Resolve(definition.Header),
                $"resident door $84:{definition.Header:X4} resolves by identity");

            // Typed placement metadata supplies the first list only. The transition
            // must obtain its second list from the resident-door definition catalog,
            // not from an arbitrary address space or a header+4 fixture byte.
            var bus = new TestAddressSpace();
            const ushort population = 0x9000;
            byte blockX = (byte)(1 + definitionIndex % 4);
            byte blockY = (byte)(1 + definitionIndex / 4);
            var decoded = new RoomPlmPopulationDefinition(population,
            [
                // Two directional caps are not placed in any retail room population,
                // so they are absent from the 70-header population catalog. Decode
                // their setup/first-list metadata here, in the import-side oracle.
                new RoomPlmPlacement(new RoomPlmHeaderDefinition(definition.Header,
                    ReadWord(rom, 0x840000 | definition.Header),
                    ReadWord(rom, 0x840000 | unchecked((ushort)(definition.Header + 2)))),
                    blockX, blockY, 0x8000), // Negative argument always closes.
            ]);

            RoomLevelData level = CreateRoom(
                8,
                8,
                new ushort[64],
                new byte[64],
                blockDefinitions: new byte[0x400 * 8]);
            var system = new Bank80SystemState();
            var plms = new RoomPlmSystem();
            AssertEqual(1, plms.LoadRoomPopulation(
                    bus,
                    level,
                    level.CreateBackgroundStreamer(),
                    new SnesVram(),
                    decoded,
                    system,
                    AreaId.Crateria,
                    () => new SamusState(),
                    () => false),
                $"resident door $84:{definition.Header:X4} loads through production setup");

            var enteringDoor = new CartridgeDoorHeader(
                Pointer: 0,
                DestinationRoomPointer: 0,
                BitFlags: 0,
                Orientation: 5,
                PlmX: blockX,
                PlmY: blockY,
                DestinationScreenX: 0,
                DestinationScreenY: 0,
                SamusDistance: 0,
                SetupCodePointer: 0);
            AssertTrue(plms.TrySpawnDoorClosingPlm(bus, level, enteringDoor, system),
                $"resident door $84:{definition.Header:X4} accepts transition redirect");
            AssertEqual(expected, plms.PopulationSlots.Single().InstructionPointer,
                $"resident door $84:{definition.Header:X4} installs compiled closing list");
            definitionIndex++;
        }

        AssertThrows<InvalidDataException>(
            () => ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.BlueDoorFacingLeft),
            "nonresident blue-door collision header is outside resident closing domain");
    }

    private static void VerifySpeedBoosterEscapeStageDefinitions(
        SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

        for (int index = 0; index < SpeedBoosterEscapePlmProgramDefinitions.WordCount; index++)
        {
            ushort address = checked((ushort)(SpeedBoosterEscapePlmProgramDefinitions.Start + index * 2));
            AssertTrue(SpeedBoosterEscapePlmProgramDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"Speed Booster escape instruction ${address:X4} is compiled");
            AssertEqual(ReadWord(rom, 0x840000 | address), compiled,
                $"Speed Booster escape instruction ${address:X4} matches cartridge");
        }
        AssertTrue(!SpeedBoosterEscapePlmProgramDefinitions.TryReadMechanicsWord(
                checked((ushort)(SpeedBoosterEscapePlmProgramDefinitions.Start - 2)), out _),
            "Speed Booster escape instruction owner excludes the preceding routine");
        AssertTrue(!SpeedBoosterEscapePlmProgramDefinitions.TryReadMechanicsWord(
                checked((ushort)(SpeedBoosterEscapePlmProgramDefinitions.Start +
                    SpeedBoosterEscapePlmProgramDefinitions.WordCount * 2)), out _),
            "Speed Booster escape instruction owner excludes the following setup routine");

        for (ushort offset = 0;
             offset < SpeedBoosterEscapeStageDefinitions.TerminatorOffset;
             offset += SpeedBoosterEscapeStageDefinitions.RecordByteCount)
        {
            SpeedBoosterEscapeStageDefinition definition =
                SpeedBoosterEscapeStageDefinitions.Resolve(offset)!.Value;
            int source = SpeedBoosterEscapeStageDefinitions.TableAddress + offset;
            AssertEqual(ReadWord(rom, source), definition.TargetSamusX,
                $"Speed Booster escape stage ${offset:X2} target X matches cartridge");
            AssertEqual(ReadWord(rom, source + 2), definition.MaximumFxY,
                $"Speed Booster escape stage ${offset:X2} maximum FX Y matches cartridge");
            AssertEqual(ReadWord(rom, source + 4), definition.PackedYVelocity,
                $"Speed Booster escape stage ${offset:X2} velocity matches cartridge");
        }

        AssertEqual(SpeedBoosterEscapeStageDefinitions.Terminator,
            ReadWord(rom, SpeedBoosterEscapeStageDefinitions.TableAddress +
                SpeedBoosterEscapeStageDefinitions.TerminatorOffset),
            "Speed Booster escape terminal word matches cartridge");
        AssertTrue(SpeedBoosterEscapeStageDefinitions.Resolve(
                SpeedBoosterEscapeStageDefinitions.TerminatorOffset) is null,
            "Speed Booster escape terminal offset resolves to event completion");
        AssertThrows<InvalidDataException>(
            () => SpeedBoosterEscapeStageDefinitions.Resolve(1),
            "unaligned Speed Booster escape stage offset fails loudly");
        AssertThrows<InvalidDataException>(
            () => SpeedBoosterEscapeStageDefinitions.Resolve(24),
            "out-of-range Speed Booster escape stage offset fails loudly");

        // This fresh bus contains the PLM program and FX fixture, but deliberately omits
        // $84:B876-$B889. The production controller must finish all three physical stages.
        VerifySpeedBoosterEscapePlm(new TestAddressSpace());
        Console.WriteLine(
            "Speed Booster escape definitions: three physical stages and the terminal event match.");
    }

    /// <summary>
    /// Locks the single bank-$8F parser, descending physical IDs, synchronous slot reuse,
    /// elevator animation, and all requested station setup/contact families to bounded
    /// synthetic rooms. No assertion crosses a door or depends on frontend choreography.
    /// </summary>
    private static void VerifySequentialRoomPlmPopulationLoader()
    {
        var bus = new TestAddressSpace();
        SeedRoomPlmPopulationRom(bus);
        bus.WriteBytes(0x8f9100, [0x80]); // Dormant scroll fixture owns an empty terminated program.
        const ushort population = 0x9000;
        bus.WriteBytes(0x8f0000 | population, [
            // First resident takes native slot 39.
            0x03, 0xb7, 0x08, 0x04, 0x00, 0x91,
            // Setup deletes this temporary extension synchronously.
            0x3b, 0xb6, 0x09, 0x04, 0x00, 0x80,
            // It must reuse slot 38, not receive a slot based on family load order.
            0x0b, 0xb7, 0x0c, 0x08, 0x00, 0x00,
            // The following resident then receives slot 37.
            0xdf, 0xb6, 0x14, 0x08, 0x00, 0x00,
            0x00, 0x00,
        ]);

        const int width = 32;
        const int height = 16;
        RoomLevelData level = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 25,
            MaxHealth = 199,
            Missiles = 1,
            MaxMissiles = 15,
        };
        var plms = new RoomPlmSystem();
        int parsed = plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            RoomPlmPopulationImporter.Read(bus, population),
            system,
            areaIndex: AreaId.Crateria,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false);

        AssertEqual(4, parsed, "single parser consumes every six-byte record once");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(3, slots.Length, "extension setup deletes its preallocated actor");
        AssertEqual(39, slots[0].NativeSlotIndex, "first ROM record receives highest native slot");
        AssertEqual(0xb703, slots[0].HeaderPointer, "first resident preserves record identity");
        AssertEqual(38, slots[1].NativeSlotIndex, "post-delete record reuses next native slot");
        AssertEqual(0xb70b, slots[1].HeaderPointer, "elevator follows ROM order, not family order");
        AssertEqual(37, slots[2].NativeSlotIndex, "later station receives descending slot");
        AssertEqual(0xb6df, slots[2].HeaderPointer, "station header remains observable");
        AssertEqual(1, plms.ElevatorPlatforms.Count, "elevator platform family is resident");
        AssertEqual(0x0000, level.GetCollisionBlock(12, 8).LevelWord,
            "elevator setup clears collision bits through shared setup dispatcher");

        // Energy right access is parent+1 and retains solid type B while publishing the
        // parent trigger. The next PLM pass refills through live Samus state and emits the
        // cartridge message ID; it does not require a room-name callback.
        AssertEqual(0xb000, level.GetCollisionBlock(21, 8).LevelWord,
            "energy setup installs type-B right access");
        AssertEqual(0x49, level.GetCollisionBlock(21, 8).Behavior,
            "energy setup installs BTS $49");
        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(21, 8), 0x49),
            "generic BTS dispatcher locates energy-station parent");
        AssertTrue(samus.InputLocked,
            "station access setup runs command six before its first PLM handler pass");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertTrue(samus.InputLocked,
            "station access command locks Samus during six-plus-$60 insertion");
        AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x37), 6)),
            "station access begins with cartridge extension sound $37");
        for (int frame = 1; frame < 102; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(199, samus.Health, "energy station restores health to cartridge maximum");
        AssertEqual(1, plms.StationActivationEvents.Count,
            "energy station publishes one shared message request");
        AssertEqual(GameplayMessageIds.EnergyRechargeCompleted,
            plms.StationActivationEvents[0].MessageBoxIndex,
            "energy station uses cartridge message $15");

        VerifyOtherStationFamilies(bus);
        VerifySaveStationConfirmation(bus);
        VerifyNativeOutOfBoundsPopulationSetupOrder(bus);
        VerifyMetroidsClearedStatePlm(bus);
        VerifyShaktoolRoomPlm();
        VerifyMotherBrainEscapeRoomGate(bus);
        VerifyBombTorizoGreyDoorClosingReentry(bus);
        VerifyStandardGreyDoorClosingFallthrough(bus);
        VerifySpeedBoosterEscapePlm(bus);
        VerifyWreckedShipAtticPlm(bus);
        VerifyUnsupportedPopulationContext(bus);
        Console.WriteLine(
            "  Room PLM population: one-pass native slots, synchronous reuse, elevator, and station families agree.");
    }

    /// <summary>
    /// Reproduces the `$BAF4` resident-door path from room `$9804` using the
    /// compiled retail instructions with a synthetic population/header. Bombs
    /// admit the closing animation, then its final Goto returns to `$BA7F`.
    /// </summary>
    private static void VerifyBombTorizoGreyDoorClosingReentry(TestAddressSpace bus)
    {
        const ushort population = 0x9690;
        const int width = 16;
        const int height = 16;
        const int doorX = 1;
        const int doorY = 6;
        const ushort roomArgument = 0x081b;
        const ushort initialList = 0xba7f;
        const ushort closingList = 0xba4c;

        WriteWord(bus, 0x840000 | RoomPlmHeaders.BombTorizoGreyDoor, 0xc794);
        WriteWord(bus,
            0x840000 | unchecked((ushort)(RoomPlmHeaders.BombTorizoGreyDoor + 2)),
            initialList);
        WriteWord(bus,
            0x840000 | unchecked((ushort)(RoomPlmHeaders.BombTorizoGreyDoor + 4)),
            closingList);

        // The PLM program and its draw pointers are compiled cartridge data. Do
        // not inject a shortened stand-in at their native addresses: the timing
        // and Bombs callback need to execute the same lists used in gameplay.

        bus.WriteBytes(0x8f0000 | population,
        [
            0xf4, 0xba, doorX, doorY,
            unchecked((byte)roomArgument), unchecked((byte)(roomArgument >> 8)),
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            CollectedItems = SamusEquipmentFlags.Bombs.ToNativeWord(),
        };
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, population),
                system,
                AreaId.Crateria,
                () => samus,
                () => false),
            "Bomb Torizo grey door loads through the shared room population");

        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0x8bc2,
            DestinationRoomPointer: RoomHeaderPointers.BombTorizoRoom,
            BitFlags: 0,
            Orientation: 5,
            PlmX: doorX,
            PlmY: doorY,
            DestinationScreenX: 0,
            DestinationScreenY: 0,
            SamusDistance: 0x8000,
            SetupCodePointer: 0);
        AssertTrue(plms.TrySpawnDoorClosingPlm(bus, level, enteringDoor, system),
            "Bomb Torizo entry redirects the resident door to its secondary list");
        AssertEqual(GreyDoorPhase.Closing, plms.GreyDoors.Single().Phase,
            "redirected actor retains explicit grey-door ownership while closing");

        plms.Step(
            bus, level, streamer, 0, 0, 0,
            scrolls: null,
            enemyDeaths: 0,
            enemyDeathQuota: 0,
            controllerNewInput: 0,
            collectedItems: samus.CollectedItems);
        AssertEqual(0x0482, level.GetCollisionBlock(doorX, doorY).LevelWord,
            "collected Bombs admit the secondary list's closing draw");

        for (int frame = 0; frame < 80 &&
             plms.GreyDoors.Single().Phase == GreyDoorPhase.Closing; frame++)
            plms.Step(
                bus, level, streamer, 0, 0, 0,
                scrolls: null,
                enemyDeaths: 0,
                enemyDeathQuota: 0,
                controllerNewInput: 0,
                collectedItems: samus.CollectedItems);
        AssertEqual(GreyDoorPhase.Locked, plms.GreyDoors.Single().Phase,
            "closing Goto hands the resident actor back to its grey-door family");
        AssertEqual(initialList, plms.PopulationSlots.Single().InstructionPointer,
            "rejoined actor retains the cartridge first-list pointer");
        AssertEqual(0xc4ae, level.GetCollisionBlock(doorX, doorY).LevelWord,
            "re-entry draws the locked grey-door frame in the same handler pass");
        AssertEqual(1, plms.ActiveCount,
            "rejoined Bomb Torizo door remains resident for the battle condition");
    }

    /// <summary>
    /// Reproduces issue 236's retail <c>$84:C842</c> grey door in room
    /// <c>$8F:9B9D</c>. Its closing list occupies <c>$BE59..$BE6F</c> and falls directly
    /// into first list <c>$BE70</c>; it has no Goto like Bomb Torizo's special door.
    /// </summary>
    private static void VerifyStandardGreyDoorClosingFallthrough(TestAddressSpace bus)
    {
        const ushort population = 0x96d0;
        const int width = 48;
        const int height = 16;
        const int doorX = 0x2e;
        const int doorY = 6;
        const ushort roomArgument = 0x0c25;
        const ushort initialList = 0xbe70;
        const ushort closingList = 0xbe59;
        const ushort closedBlueList = 0xc100;
        const ushort activationList = 0xbe84;
        const ushort openTriggerList = 0xbea8;
        const ushort openingList = 0xbead;
        const ushort closingDraw = 0xa600;
        const ushort lockedDraw = 0xa610;

        WriteWord(bus, 0x840000 | RoomPlmHeaders.GreyDoorFacingLeft, 0xc794);
        WriteWord(bus,
            0x840000 | unchecked((ushort)(RoomPlmHeaders.GreyDoorFacingLeft + 2)),
            initialList);
        WriteWord(bus,
            0x840000 | unchecked((ushort)(RoomPlmHeaders.GreyDoorFacingLeft + 4)),
            closingList);
        WriteWord(bus, 0x840000 | unchecked((ushort)(initialList + 2)), closedBlueList);
        WriteWord(bus, 0x840000 | unchecked((ushort)(initialList + 6)), activationList);
        WriteWord(bus, 0x840000 | unchecked((ushort)(initialList + 12)), lockedDraw);
        WriteWord(bus, 0x840000 | unchecked((ushort)(activationList + 2)), openTriggerList);
        bus.WriteBytes(0x840000 | unchecked((ushort)(openTriggerList + 2)),
        [
            0x01,
            unchecked((byte)openingList),
            unchecked((byte)(openingList >> 8)),
        ]);

        // Preserve the exact byte widths of BE59: five timer/draw pairs with an odd-sized
        // queue-sound instruction between the second and third pair. The final pair ends
        // at BE70, the start of the resident door's ordinary first list.
        WriteWord(bus, 0x840000 | closingList, 2);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 2)), closingDraw);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 4)), 2);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 6)), closingDraw);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 8)),
            RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6);
        bus.WriteBytes(0x840000 | unchecked((ushort)(closingList + 10)), [0x08]);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 11)), 2);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 13)), closingDraw);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 15)), 2);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 17)), closingDraw);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 19)), 1);
        WriteWord(bus, 0x840000 | unchecked((ushort)(closingList + 21)), closingDraw);
        WriteOneBlockDraw(bus, closingDraw, 0xc123);
        WriteOneBlockDraw(bus, lockedDraw, 0xc123);

        bus.WriteBytes(0x8f0000 | population,
        [
            unchecked((byte)RoomPlmHeaders.GreyDoorFacingLeft),
            unchecked((byte)(RoomPlmHeaders.GreyDoorFacingLeft >> 8)),
            doorX,
            doorY,
            unchecked((byte)roomArgument),
            unchecked((byte)(roomArgument >> 8)),
            0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, population),
                system,
                AreaId.Brinstar,
                () => new SamusState(),
                () => false),
            "Brinstar pre-map grey door loads through the shared population path");

        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0x8cb2,
            DestinationRoomPointer: 0x9b9d,
            BitFlags: 0,
            Orientation: 5,
            PlmX: doorX,
            PlmY: doorY,
            DestinationScreenX: 0,
            DestinationScreenY: 0,
            SamusDistance: 0x8000,
            SetupCodePointer: 0);
        AssertTrue(plms.TrySpawnDoorClosingPlm(bus, level, enteringDoor, system),
            "room $9B9D redirects its resident grey door to closing list $BE59");

        for (int frame = 0; frame < 10; frame++)
        {
            plms.Step(
                bus, level, streamer, 0, 0, 0,
                scrolls: null,
                enemyDeaths: 0,
                enemyDeathQuota: 1,
                controllerNewInput: 0,
                collectedItems: 0);
        }

        AssertEqual(GreyDoorPhase.Locked, plms.GreyDoors.Single().Phase,
            "fall-through at $BE70 returns to the resident grey-door owner");
        AssertEqual(initialList, plms.PopulationSlots.Single().InstructionPointer,
            "door remains parked at its first list instead of dispatching $8A72 generically");
        AssertEqual(1, plms.ActiveCount,
            "room $9B9D grey door remains resident after its entry-closing animation");
    }

    /// <summary>
    /// Reproduces the complete $BB05 lifecycle in a bounded synthetic room. Although its
    /// callback is intentionally inert, the actor must retain a native slot, install the
    /// exact $BAFA pre-instruction from ROM, and remain asleep without mutating terrain.
    /// </summary>
    private static void VerifyWreckedShipAtticPlm(TestAddressSpace bus)
    {
        const ushort population = 0x9640;
        const int width = 8;
        const int height = 8;
        const int blockX = 3;
        const int blockY = 4;

        WriteWord(bus,
            0x840000 | unchecked((ushort)(RoomPlmHeaders.WreckedShipAttic + 2)),
            RoomPlmInstructionLists.WreckedShipAttic);
        SuperMetroidAddressSpace? rom = File.Exists("Super Metroid.smc")
            ? SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")
            : null;
        for (int index = 0; index < 3; index++)
        {
            ushort address = checked((ushort)(RoomPlmInstructionLists.WreckedShipAttic + index * 2));
            AssertTrue(WreckedShipAtticPlmRomData.TryReadInstructionWord(address,
                    out ushort compiled),
                $"Wrecked Ship attic list word ${address:X4} is compiled");
            if (rom is not null)
            {
                int source = 0x840000 | address;
                AssertEqual((ushort)(rom.ReadByte(source) | rom.ReadByte(source + 1) << 8),
                    compiled,
                    $"Wrecked Ship attic list word ${address:X4} matches cartridge");
            }
            WriteWord(bus, 0x840000 | address, 0xdead);
        }
        AssertTrue(!WreckedShipAtticPlmRomData.TryReadInstructionWord(
                checked((ushort)(RoomPlmInstructionLists.WreckedShipAttic + 6)), out _),
            "Wrecked Ship attic list owner excludes its adjacent header");
        bus.WriteBytes(0x8f0000 | population,
        [
            0x05, 0xbb, blockX, blockY, 0x34, 0x12,
            0x00, 0x00,
        ]);

        ushort[] levelWords = new ushort[width * height];
        levelWords[blockY * width + blockX] = 0x9123;
        RoomLevelData level = CreateRoom(
            width,
            height,
            levelWords,
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(
                bus,
                level,
                level.CreateBackgroundStreamer(),
                new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, population),
                new Bank80SystemState(),
                AreaId.WreckedShip,
                () => new SamusState(),
                () => false),
            "Wrecked Ship attic record is accepted by sequential loader");
        RoomPlmSlotSnapshot loaded = plms.PopulationSlots.Single();
        AssertEqual(39, loaded.NativeSlotIndex,
            "Wrecked Ship attic actor receives the native highest free slot");
        AssertEqual(RoomPlmInstructionLists.WreckedShipAttic, loaded.InstructionPointer,
            "Wrecked Ship attic actor starts at cartridge list BAFF");
        AssertEqual(0, loaded.PreInstruction,
            "Wrecked Ship attic callback is not installed before its first handler pass");

        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(bus, level, streamer, 0, 0, 0);
        RoomPlmSlotSnapshot sleeping = plms.PopulationSlots.Single();
        AssertEqual(WreckedShipAtticPlmRomData.NoOpCallback, sleeping.PreInstruction,
            "Wrecked Ship attic list installs exact BAFA pre-instruction");
        AssertEqual(
            unchecked((ushort)(RoomPlmInstructionLists.WreckedShipAttic + 4)),
            sleeping.InstructionPointer,
            "Wrecked Ship attic actor sleeps permanently at list word BB03");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(1, plms.ActiveCount,
            "inert attic callback leaves its resident actor alive");
        AssertEqual(0x9123, level.GetCollisionBlock(blockX, blockY).LevelWord,
            "inert attic callback leaves authored terrain unchanged");

        // A saved/corrupt live state can still supply an unsupported callback even
        // though the compiled retail list cannot. Inject that state after install so
        // the family dispatcher remains loudly exhaustive without reopening ROM reads.
        Array slots = (Array)typeof(RoomPlmSystem).GetField("_slots",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)!.GetValue(plms)!;
        object liveSlot = slots.GetValue(39)!;
        liveSlot.GetType().GetProperty("PreInstruction")!.SetValue(liveSlot,
            (ushort)0x9876);
        AssertThrows<InvalidDataException>(
            () => plms.Step(bus, level, streamer, 0, 0, 0),
            "unknown Wrecked Ship attic pre-instruction fails loudly");
    }

    /// <summary>
    /// Exercises setup $B89C and all three callbacks installed by the cartridge's $B88A
    /// list. The fixture writes the explicit shared FX motion words and compiled stage records,
    /// so the test observes the same shared FX/event/earthquake owners as production.
    /// </summary>
    private static void VerifySpeedBoosterEscapePlm(TestAddressSpace bus)
    {
        const ushort population = 0x9620;
        const int width = 16;
        const int height = 16;

        WriteWord(bus, 0x84b8ae, RoomPlmInstructionLists.SpeedBoosterEscape);
        // Poison the old cartridge path: all three real handoffs must now use
        // the compiled list even when a synthetic address space disagrees.
        for (int index = 0; index < SpeedBoosterEscapePlmProgramDefinitions.WordCount; index++)
        {
            WriteWord(bus,
                0x840000 | SpeedBoosterEscapePlmProgramDefinitions.Start + index * 2,
                0xdead);
        }
        bus.WriteBytes(0x8f0000 | population,
        [
            0xac, 0xb8, 0x01, 0x01, 0x00, 0x00,
            0x00, 0x00,
        ]);
        RoomLevelData CreateLevel() => CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);

        RoomLayer3FxState CreateFx(ushort baseY, ushort targetY, ushort velocity, byte timer)
        {
            var fx = new RoomLayer3FxState();
            fx.ApplyCartridgeMotionWrites(baseY, targetY, velocity, timer);
            return fx;
        }

        RoomPlmSystem Load(
            Bank80SystemState system,
            SamusState samus,
            RoomLayer3FxState fx,
            RoomLevelData level,
            Action<ushort> writeEarthquake)
        {
            var plms = new RoomPlmSystem();
            AssertEqual(1, plms.LoadRoomPopulation(
                    bus,
                    level,
                    level.CreateBackgroundStreamer(),
                    new SnesVram(),
                    RoomPlmPopulationImporter.Read(bus, population),
                    system,
                    AreaId.Norfair,
                    () => samus,
                    () => false,
                    hasEvent: system.HasEvent,
                    setEvent: system.SetEvent,
                    roomFx: fx,
                    setEarthquakeTimer: writeEarthquake),
                "Speed Booster escape record is accepted by sequential loader");
            return plms;
        }

        var completedSystem = new Bank80SystemState();
        completedSystem.SetEvent(EventNumber.OutranSpeedBoosterLavaquake);
        RoomLevelData completedLevel = CreateLevel();
        RoomPlmSystem completed = Load(
            completedSystem,
            new SamusState(),
            CreateFx(0x0300, 0x0100, 0, 5),
            completedLevel,
            _ => { });
        AssertEqual(0, completed.ActiveCount,
            "event-$15 setup deletes B8AC synchronously");

        var unequippedSystem = new Bank80SystemState();
        var unequippedSamus = new SamusState();
        RoomLayer3FxState unequippedFx = CreateFx(0x0300, 0x0100, 0x1234, 5);
        ushort earthquakeTimer = 7;
        RoomLevelData unequippedLevel = CreateLevel();
        RoomPlmSystem unequipped = Load(
            unequippedSystem,
            unequippedSamus,
            unequippedFx,
            unequippedLevel,
            value => earthquakeTimer = value);
        BackgroundTilemapStreamer unequippedStreamer = unequippedLevel.CreateBackgroundStreamer();
        unequipped.Step(bus, unequippedLevel, unequippedStreamer, 0, 0, 0);
        unequipped.Step(bus, unequippedLevel, unequippedStreamer, 0, 0, 0);
        AssertEqual(0, unequipped.ActiveCount,
            "missing Speed Booster deletes the resident controller");
        AssertEqual(ushort.MaxValue, unequippedFx.TargetYPosition,
            "missing Speed Booster disables the FX target");
        AssertEqual(0, unequippedFx.PackedYVelocity,
            "missing Speed Booster clears packed FX velocity");
        AssertEqual(0, unequippedFx.Timer,
            "missing Speed Booster clears FX timer");
        AssertEqual(0, earthquakeTimer,
            "missing Speed Booster clears global earthquake timer");

        var disabledSystem = new Bank80SystemState();
        var disabledSamus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.SpeedBooster,
        };
        RoomLevelData disabledLevel = CreateLevel();
        RoomPlmSystem disabled = Load(
            disabledSystem,
            disabledSamus,
            CreateFx(0x0300, ushort.MaxValue, 0, 0),
            disabledLevel,
            _ => { });
        BackgroundTilemapStreamer disabledStreamer = disabledLevel.CreateBackgroundStreamer();
        disabled.Step(bus, disabledLevel, disabledStreamer, 0, 0, 0);
        disabled.Step(bus, disabledLevel, disabledStreamer, 0, 0, 0);
        AssertEqual(0, disabled.ActiveCount,
            "negative FX target deletes collected-Speed-Booster controller");

        var activeSystem = new Bank80SystemState();
        var activeSamus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.SpeedBooster,
            XPosition = 0x0ae1,
        };
        RoomLayer3FxState activeFx = CreateFx(0x0300, 0x0100, 0, 0);
        RoomLevelData activeLevel = CreateLevel();
        RoomPlmSystem active = Load(
            activeSystem,
            activeSamus,
            activeFx,
            activeLevel,
            _ => { });
        BackgroundTilemapStreamer activeStreamer = activeLevel.CreateBackgroundStreamer();
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(SpeedBoosterEscapePlmRomData.InitialLavaquakeVelocity,
            activeFx.PackedYVelocity,
            "collected Speed Booster starts the regional lavaquake velocity");
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(0, activeFx.Timer,
            "Samus right of $0AE0 leaves FX motion sleeping");
        activeSamus.XPosition = SpeedBoosterEscapePlmRomData.StartFxMotionSamusX;
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(1, activeFx.Timer,
            "Samus reaching $0AE0 starts FX motion");

        activeSamus.XPosition = 0x072c;
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(0x0300, activeFx.BaseYPosition,
            "first lava stage waits while Samus remains right of its threshold");
        activeSamus.XPosition = 0x072b;
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(0x01bf, activeFx.BaseYPosition,
            "first lava stage clamps maximum FX Y");
        AssertEqual(0xff50, activeFx.PackedYVelocity,
            "first lava stage installs ROM velocity");
        activeSamus.XPosition = 0x050a;
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(0x0167, activeFx.BaseYPosition,
            "second lava stage clamps maximum FX Y");
        activeSamus.XPosition = 0x0244;
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertEqual(0x0100, activeFx.BaseYPosition,
            "third lava stage clamps maximum FX Y");
        active.Step(bus, activeLevel, activeStreamer, 0, 0, 0);
        AssertTrue(activeSystem.HasEvent(EventNumber.OutranSpeedBoosterLavaquake),
            "stage-table terminator marks event $15");
        AssertEqual(1, active.ActiveCount,
            "native terminator leaves B8AC resident until room reload");
    }

    /// <summary>
    /// Reproduces the retail Fireflea/Colosseum defect pattern: a colored door can have a
    /// byte coordinate below the native allocation ceiling but outside the authored room.
    /// Native allocates the slot and runs setup against level_data anyway. The translation
    /// must preserve that harmless order without weakening bounds for PLMs whose setup
    /// genuinely consumes logical terrain.
    /// </summary>
    private static void VerifyNativeOutOfBoundsPopulationSetupOrder(TestAddressSpace bus)
    {
        const ushort population = 0x96c0;
        const ushort malformedPopulation = 0x96e0;
        const int width = 16;
        const int height = 32;
        const int offRoomX = 1;
        const int offRoomY = 38;
        const int inRoomX = 2;
        const int inRoomY = 2;

        var decoded = new RoomPlmPopulationDefinition(population,
        [
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreenDoorFacingRight),
                offRoomX, offRoomY, 0x27),
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreenDoorFacingLeft),
                inRoomX, inRoomY, 0x28),
        ]);

        RoomLevelData level = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        AssertEqual(2, plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                decoded,
                new Bank80SystemState(),
                AreaId.Brinstar,
                () => new SamusState(),
                () => false),
            "off-room colored door and following record both run setup");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(39, slots[0].NativeSlotIndex,
            "off-room record is allocated before its setup callback");
        AssertEqual(offRoomY * width + offRoomX, slots[0].BlockIndex,
            "off-room record retains native unchecked row-major block index");
        AssertEqual(38, slots[1].NativeSlotIndex,
            "following record retains descending native slot order");
        AssertEqual(0, level.GetCollisionBlockByIndex(width * height - 1).LevelWord,
            "off-room setup cannot corrupt the final authored host block");

        // Both resident actors execute their first draw. The off-room write remains in the
        // bounded native tail while the ordinary record updates logical terrain normally.
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(0xc004, level.GetCollisionBlock(inRoomX, inRoomY).LevelWord,
            "in-room record following the harmless tail actor still draws normally");

        var malformed = new RoomPlmPopulationDefinition(malformedPopulation,
        [
            new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ScrollTrigger),
                offRoomX, offRoomY, 0x9800, new byte[] { 0x80 }),
        ]);
        AssertThrows<ArgumentOutOfRangeException>(
            () => new RoomPlmSystem().LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                malformed,
                new Bank80SystemState(),
                AreaId.Brinstar,
                () => new SamusState(),
                () => false),
            "off-room scroll owner fails when setup consumes a required logical block");
    }

    /// <summary>
    /// Exercises both native uses of header $C8CA: an ordinary room load leaves its
    /// six-frame closed gate list intact, while Mother Brain's direction-nine entrance
    /// redirects that same physical slot to the header's second, three-frame closing list.
    /// </summary>
    private static void VerifyMotherBrainEscapeRoomGate(TestAddressSpace bus)
    {
        const ushort population = 0x9680;
        const int width = 16;
        const int height = 16;
        const int gateX = 7;
        const int gateY = 2;
        const ushort originalGateWord = 0xf123;
        const ushort deactivatedGateWord = 0x8123;
        const ushort openGateWord = 0x80ff;
        const ushort halfClosedGateTopWord = 0x830f;
        const ushort closedGateMiddleWord = 0x8ae8;

        WriteWord(bus, 0x84c8ca, 0xb3c1);
        WriteWord(bus, 0x84c8cc, RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosed);
        WriteWord(bus, 0x84c8ce, RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing);
        WriteWord(bus, 0x84c8d0, 0xb3c1);
        bus.WriteBytes(0x8f0000 | population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);

        WriteWord(bus, 0x84bb34, 6);
        WriteWord(bus, 0x84bb36, 0x948b);
        WriteWord(bus, 0x84bb38, RoomPlmInstructionCodes.Delete);
        WriteWord(bus, 0x84bb44, 2);
        WriteWord(bus, 0x84bb46, 0x9473);
        WriteWord(bus, 0x84bb48, 2);
        WriteWord(bus, 0x84bb4a, 0x947f);
        WriteWord(bus, 0x84bb4c, 2);
        WriteWord(bus, 0x84bb4e, 0x948b);
        WriteWord(bus, 0x84bb50, RoomPlmInstructionCodes.Delete);
        WriteVerticalPlmDraw(bus, 0x9473, [0x80ff, 0x80ff, 0x80ff, 0x80ff]);
        WriteVerticalPlmDraw(bus, 0x947f, [0x830f, 0x80ff, 0x80ff, 0x830f]);
        WriteVerticalPlmDraw(bus, 0x948b, [0x830f, 0x8ae8, 0x82e8, 0x830f]);
        var guarded = new MotherBrainEscapeGateReadGuard(bus);

        RoomLevelData ordinaryLevel = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        int gateBlock = ordinaryLevel.GetBlockIndex(gateX, gateY);
        ordinaryLevel.SetForegroundEntry(gateBlock, originalGateWord);
        BackgroundTilemapStreamer ordinaryStreamer = ordinaryLevel.CreateBackgroundStreamer();
        var ordinary = new RoomPlmSystem();
        var system = new Bank80SystemState();
        AssertEqual(1, ordinary.LoadRoomPopulation(
                guarded,
                ordinaryLevel,
                ordinaryStreamer,
                new SnesVram(),
                RoomPlmPopulationImporter.Read(guarded, population),
                system,
                AreaId.Tourian,
                () => new SamusState(),
                () => false),
            "escape gate population record is accepted by the sequential loader");
        AssertEqual(deactivatedGateWord, ordinaryLevel.GetCollisionBlockByIndex(gateBlock).LevelWord,
            "C8CA setup clears collision class bits without deleting its resident slot");
        AssertEqual(39, ordinary.PopulationSlots.Single().NativeSlotIndex,
            "escape gate retains the highest native population slot");
        AssertEqual(RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosed,
            ordinary.PopulationSlots.Single().InstructionPointer,
            "ordinary entry retains C8CA's first closed-gate list");
        ordinary.Step(guarded, ordinaryLevel, ordinaryStreamer, 0, 0, 0);
        AssertEqual(closedGateMiddleWord,
            ordinaryLevel.GetCollisionBlock(gateX, gateY + 1).LevelWord,
            "non-escape entry draws the cartridge's already-closed gate");
        for (int frame = 0; frame < 6; frame++)
            ordinary.Step(guarded, ordinaryLevel, ordinaryStreamer, 0, 0, 0);
        AssertEqual(0, ordinary.ActiveCount,
            "non-escape closed-gate actor releases its slot after six authored frames");

        RoomLevelData escapeLevel = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        escapeLevel.SetForegroundEntry(gateBlock, originalGateWord);
        BackgroundTilemapStreamer escapeStreamer = escapeLevel.CreateBackgroundStreamer();
        var escape = new RoomPlmSystem();
        escape.LoadRoomPopulation(
            guarded,
            escapeLevel,
            escapeStreamer,
            new SnesVram(),
            RoomPlmPopulationImporter.Read(guarded, population),
            system,
            AreaId.Tourian,
            () => new SamusState(),
            () => false);
        var motherBrainExit = new CartridgeDoorHeader(
            Pointer: 0xaa8c,
            DestinationRoomPointer: 0xde4d,
            BitFlags: 0,
            Orientation: 9,
            PlmX: gateX,
            PlmY: gateY,
            DestinationScreenX: 0,
            DestinationScreenY: 0,
            SamusDistance: 0x8000,
            SetupCodePointer: 0);
        AssertTrue(escape.TrySpawnDoorClosingPlm(guarded, escapeLevel, motherBrainExit, system),
            "direction-nine Mother Brain exit redirects the resident gate");
        AssertEqual(1, escape.ActiveCount,
            "resident escape gate is redirected in place rather than duplicated");
        AssertEqual(39, escape.PopulationSlots.Single().NativeSlotIndex,
            "redirect preserves native room-population slot ordering");
        AssertEqual(RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing,
            escape.PopulationSlots.Single().InstructionPointer,
            "door transition selects C8CA's second instruction list");

        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(gateX, gateY).LevelWord,
            "closing frame one draws the fully open gate");
        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(gateX, gateY).LevelWord,
            "two-frame gate timer retains the open image on its countdown frame");
        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(halfClosedGateTopWord, escapeLevel.GetCollisionBlock(gateX, gateY).LevelWord,
            "closing frame two draws the half-closed gate");
        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(closedGateMiddleWord, escapeLevel.GetCollisionBlock(gateX, gateY + 1).LevelWord,
            "closing frame three installs the final solid gate collision");
        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        escape.Step(guarded, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(0, escape.ActiveCount,
            "escape gate releases its slot after the final two-frame hold");

        RoomLevelData fallbackLevel = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        fallbackLevel.SetForegroundEntry(gateBlock, originalGateWord);
        var fallback = new RoomPlmSystem();
        AssertTrue(fallback.TrySpawnDoorClosingPlm(guarded, fallbackLevel, motherBrainExit, system),
            "special door without a resident cap spawns C8D0 fallback");
        AssertEqual(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            fallback.PopulationSlots.Single().HeaderPointer,
            "fallback actor uses the cartridge's dedicated C8D0 header");
        AssertEqual(deactivatedGateWord, fallbackLevel.GetCollisionBlockByIndex(gateBlock).LevelWord,
            "fallback C8D0 executes the shared deactivate setup");

        var nonClosingDoor = motherBrainExit with { Orientation = 0 };
        var nonClosing = new RoomPlmSystem();
        AssertTrue(!nonClosing.TrySpawnDoorClosingPlm(guarded, fallbackLevel, nonClosingDoor, system),
            "directions zero through three retain the native no-closing-PLM branch");
        AssertEqual(0, nonClosing.ActiveCount,
            "non-closing door does not consume a PLM slot");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Mother Brain escape gate never rereads compiled programs or physical draws");
    }

    private sealed class MotherBrainEscapeGateReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x84bb34 and <= 0x84bb51 or
                >= 0x849473 and <= 0x849496)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Mother Brain escape gate reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private static void VerifyMotherBrainEscapeGateCompiledDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int address = MotherBrainEscapeGatePlmProgramDefinitions.FirstAddress;
             address <= MotherBrainEscapeGatePlmProgramDefinitions.LastAddress; address++)
        {
            AssertTrue(MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsByte(
                    checked((ushort)address), out byte compiled),
                $"escape-gate program claims byte $84:{address:X4}");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"escape-gate program byte $84:{address:X4} matches ROM");
            if (address == MotherBrainEscapeGatePlmProgramDefinitions.LastAddress)
                continue;
            AssertTrue(MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsWord(
                    checked((ushort)address), out ushort compiledWord),
                $"escape-gate program claims word $84:{address:X4}");
            ushort native = (ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8);
            AssertEqual(native, compiledWord,
                $"escape-gate program word $84:{address:X4} matches ROM");
        }
        AssertTrue(!MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsByte(0xbb33, out _),
            "escape-gate program excludes preceding PLM header");
        AssertTrue(!MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsByte(0xbb52, out _),
            "escape-gate program excludes following pre-instruction machine code");

        AssertEqual(3, MotherBrainEscapeGatePlmDrawDefinitions.All.Count(),
            "escape gate owns open, half-closed and closed physical draws");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 MotherBrainEscapeGatePlmDrawDefinitions.All)
        {
            AssertEqual(1, list.Runs.Length,
                $"escape-gate draw ${list.Pointer:X4} has one vertical run");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            AssertEqual(run.DirectionAndCount,
                (ushort)(rom.ReadByte(0x840000 | list.Pointer) |
                    rom.ReadByte(0x840000 | (list.Pointer + 1)) << 8),
                $"escape-gate draw ${list.Pointer:X4} direction and count match ROM");
            AssertEqual(4, run.LevelWords.Length,
                $"escape-gate draw ${list.Pointer:X4} has four physical words");
            for (int block = 0; block < run.LevelWords.Length; block++)
            {
                int address = 0x840000 | (list.Pointer + 2 + block * 2);
                ushort native = (ushort)(rom.ReadByte(address) |
                    rom.ReadByte(address + 1) << 8);
                AssertEqual(native, run.LevelWords.Span[block],
                    $"escape-gate draw ${list.Pointer:X4} block {block} matches ROM");
            }
            int terminator = 0x840000 | (list.Pointer + 10);
            AssertEqual((ushort)0,
                (ushort)(rom.ReadByte(terminator) | rom.ReadByte(terminator + 1) << 8),
                $"escape-gate draw ${list.Pointer:X4} has zero offset terminator");
        }
    }

    private static void WriteVerticalPlmDraw(
        TestAddressSpace bus,
        ushort pointer,
        ReadOnlySpan<ushort> levelWords)
    {
        WriteWord(bus, 0x840000 | pointer, unchecked((ushort)(0x8000 | levelWords.Length)));
        for (int index = 0; index < levelWords.Length; index++)
            WriteWord(bus, 0x840000 | unchecked((ushort)(pointer + 2 + index * 2)), levelWords[index]);
        WriteWord(bus, 0x840000 | unchecked((ushort)(pointer + 2 + levelWords.Length * 2)), 0);
    }

    /// <summary>
    /// Exercises all semantic branches in `$DB44`'s thirteen-entry room-argument table:
    /// no-op, below-quota, four event writes, persistent sleep, and malformed input.
    /// </summary>
    private static void VerifyMetroidsClearedStatePlm(TestAddressSpace bus)
    {
        const ushort population = 0x9500;
        WriteWord(bus, 0x84db46, RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired);
        WriteWord(bus, 0x84db42, 0xdead);
        AssertTrue(MetroidsClearedPlmRomData.TryReadInstructionWord(
                RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired,
                out ushort compiledInstruction),
            "Metroids-cleared resident list is compiled");
        AssertEqual(RoomPlmInstructionCodes.Sleep, compiledInstruction,
            "Metroids-cleared resident list sleeps instead of reading poisoned fixture data");
        AssertTrue(!MetroidsClearedPlmRomData.TryReadInstructionWord(
                checked((ushort)(RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired + 2)),
                out _),
            "Metroids-cleared instruction owner excludes adjacent PLM header");
        if (File.Exists("Super Metroid.smc"))
        {
            SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
                "Super Metroid.smc");
            int source = 0x840000 | RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired;
            ushort nativeInstruction = (ushort)(rom.ReadByte(source) |
                rom.ReadByte(source + 1) << 8);
            AssertEqual(nativeInstruction, compiledInstruction,
                "Metroids-cleared one-word list matches pinned cartridge");
        }
        bus.WriteBytes(0x8f0000 | population,
        [
            0x44, 0xdb, 0x01, 0x01, 0x10, 0x00,
            0x44, 0xdb, 0x02, 0x01, 0x12, 0x00,
            0x44, 0xdb, 0x03, 0x01, 0x14, 0x00,
            0x44, 0xdb, 0x04, 0x01, 0x16, 0x00,
            0x44, 0xdb, 0x05, 0x01, 0x18, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(
            8,
            8,
            new ushort[64],
            new byte[64],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(5, plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, population),
                system,
                AreaId.Tourian,
                () => new SamusState(),
                () => false,
                hasEvent: system.HasEvent,
                setEvent: system.SetEvent),
            "Metroids-cleared population loads all argument families");

        plms.Step(bus, level, streamer, 0, 0, 0, enemyDeaths: 2, enemyDeathQuota: 3);
        foreach (EventNumber eventNumber in new[]
                 {
                     EventNumber.FirstMetroidHallCleared,
                     EventNumber.FirstMetroidShaftCleared,
                     EventNumber.SecondMetroidHallCleared,
                     EventNumber.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(!system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} remains clear below quota");
        }

        plms.Step(bus, level, streamer, 0, 0, 0, enemyDeaths: 3, enemyDeathQuota: 3);
        foreach (EventNumber eventNumber in new[]
                 {
                     EventNumber.FirstMetroidHallCleared,
                     EventNumber.FirstMetroidShaftCleared,
                     EventNumber.SecondMetroidHallCleared,
                     EventNumber.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} is marked at quota");
        }
        AssertEqual(5, plms.ActiveCount,
            "Metroids-cleared observers remain resident on their sleep instruction");
        AssertTrue(plms.PopulationSlots.All(slot =>
                slot.InstructionPointer ==
                    RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired),
            "Metroids-cleared observers retain the cartridge sleep list");

        const ushort invalidPopulation = 0x9580;
        bus.WriteBytes(0x8f0000 | invalidPopulation,
        [
            0x44, 0xdb, 0x01, 0x01, 0x01, 0x00,
            0x00, 0x00,
        ]);
        AssertThrows<InvalidDataException>(
            () => new RoomPlmSystem().LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, invalidPopulation),
                new Bank80SystemState(),
                AreaId.Tourian,
                () => new SamusState(),
                () => false),
            "odd Metroids-cleared room argument fails loudly");
    }

    /// <summary>
    /// The hardcoded room-setup spawn and actual PLM handler must install the
    /// compiled $B8D6 callback before the power-bomb and path-clear branches.
    /// </summary>
    private static void VerifyShaktoolRoomPlm()
    {
        var bus = new TestAddressSpace();
        SuperMetroidAddressSpace? rom = File.Exists("Super Metroid.smc")
            ? SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")
            : null;
        for (int index = 0; index < 3; index++)
        {
            ushort address = checked((ushort)(ShaktoolRoomPlmRomData.InstructionList + index * 2));
            AssertTrue(ShaktoolRoomPlmRomData.TryReadInstructionWord(address, out ushort compiled),
                $"Shaktool room list word ${address:X4} is compiled");
            if (rom is not null)
            {
                int source = 0x840000 | address;
                AssertEqual((ushort)(rom.ReadByte(source) | rom.ReadByte(source + 1) << 8),
                    compiled,
                    $"Shaktool room list word ${address:X4} matches pinned cartridge");
            }
            WriteWord(bus, 0x840000 | address, 0xdead);
        }
        AssertTrue(!ShaktoolRoomPlmRomData.TryReadInstructionWord(
                checked((ushort)(ShaktoolRoomPlmRomData.InstructionList + 6)), out _),
            "Shaktool room list owner excludes adjacent setup code");

        const ushort emptyPopulation = 0x9780;
        WriteWord(bus, 0x8f0000 | emptyPopulation, 0);
        RoomLevelData level = CreateRoom(32, 32, new ushort[32 * 32],
            new byte[32 * 32], blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        RoomScrollGrid scrolls = RoomScrollGrid.LoadCompiled(bus,
            new byte[RoomScrollGrid.StorageByteCount], 2, 2);
        var samus = new SamusState();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(0, plms.LoadRoomPopulation(bus, level, streamer,
                new SnesVram(), RoomPlmPopulationImporter.Read(bus, emptyPopulation), system, AreaId.Maridia,
                () => samus, () => false,
                hasEvent: system.HasEvent, setEvent: system.SetEvent),
            "empty fixture reserves no PLM before Shaktool setup spawn");
        AssertTrue(plms.TrySpawnShaktoolRoomController(scrolls),
            "Shaktool room setup spawns its resident controller");
        AssertEqual(RoomScrollState.Blue, scrolls.ReadState(0),
            "Shaktool setup opens the first scroll cell");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadState(3),
            "Shaktool setup keeps later scroll cells red");

        plms.Step(bus, level, streamer, 0, 0, 0,
            scrolls: scrolls, enemyDeaths: 0, enemyDeathQuota: 0,
            controllerNewInput: 0);
        RoomPlmSlotSnapshot sleeping = plms.PopulationSlots.Single();
        AssertEqual(ShaktoolRoomPlmRomData.PreInstruction, sleeping.PreInstruction,
            "compiled list installs Shaktool room pre-instruction");
        AssertEqual(checked((ushort)(ShaktoolRoomPlmRomData.InstructionList + 4)),
            sleeping.InstructionPointer,
            "compiled list sleeps on its last instruction");

        plms.Step(bus, level, streamer, 0, 0, 0,
            scrolls: scrolls, enemyDeaths: 0, enemyDeathQuota: 0,
            controllerNewInput: 0, powerBombExplosionStatus: 1);
        for (int cell = 0; cell < ShaktoolRoomPlmRomData.ScrollCellCount; cell++)
            AssertEqual(RoomScrollState.Blue, scrolls.ReadState(cell),
                $"Shaktool power bomb opens scroll cell {cell}");
        AssertEqual(1, plms.ActiveCount,
            "Shaktool controller remains resident until Samus passes the threshold");

        samus.XPosition = checked((ushort)(ShaktoolRoomPlmRomData.ClearedPathXBoundary + 1));
        plms.Step(bus, level, streamer, 0, 0, 0,
            scrolls: scrolls, enemyDeaths: 0, enemyDeathQuota: 0,
            controllerNewInput: 0);
        AssertTrue(system.HasEvent(EventNumber.ShaktoolClearedPath),
            "Shaktool path event follows the native X threshold");
        AssertEqual(0, plms.ActiveCount,
            "Shaktool controller deletes itself when the path event is set");
    }

    private static void VerifyOtherStationFamilies(TestAddressSpace bus,
        RoomPlmStationVisualCatalog? visuals = null)
    {
        const ushort population = 0x9200;
        bus.WriteBytes(0x8f0000 | population, [
            0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00,
            0xeb, 0xb6, 0x0c, 0x06, 0x00, 0x00,
            0x6f, 0xb7, 0x12, 0x06, 0x03, 0x00,
            0x00, 0x00,
        ]);
        const int width = 32;
        RoomLevelData level = CreateRoom(
            width,
            16,
            new ushort[width * 16],
            new byte[width * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState { Health = 99, MaxHealth = 99, Missiles = 0, MaxMissiles = 10 };
        var plms = new RoomPlmSystem { StationVisuals = visuals };
        plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            RoomPlmPopulationImporter.Read(bus, population),
            system,
            areaIndex: AreaId.Norfair,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false);

        AssertEqual(3, plms.Stations.Count, "map, missile, and save parents share one pool");
        AssertEqual(0x47, level.GetCollisionBlock(7, 6).Behavior,
            "map right access setup uses BTS $47");
        AssertEqual(0x48, level.GetCollisionBlock(4, 6).Behavior,
            "map left access setup uses native two-block offset");
        AssertEqual(0x4b, level.GetCollisionBlock(13, 6).Behavior,
            "missile right access setup uses BTS $4B");
        AssertEqual(0x4c, level.GetCollisionBlock(11, 6).Behavior,
            "missile left access setup uses BTS $4C");
        AssertEqual(0x4d, level.GetCollisionBlock(18, 6).Behavior,
            "save station setup installs trigger BTS $4D");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(7, 6), 0x47),
            "map access resolves parent");
        AssertTrue(samus.InputLocked,
            "map access setup locks Samus before the resident PLM advances");
        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(13, 6), 0x4b),
            "missile access resolves parent");
        for (int frame = 0; frame < 102; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(0xb859, level.GetCollisionBlock(18, 6).LevelWord,
            "compiled save-pod idle draw installs its physical floor word");
        AssertEqual(0x005b, level.GetCollisionBlock(18, 2).LevelWord,
            "compiled save-pod draw extends its shaft above the trigger");
        AssertEqual(0x8059, level.GetCollisionBlock(18, 1).LevelWord,
            "compiled save-pod draw installs its upper cap");
        AssertTrue(system.HasAreaMap(2), "map station marks current area acquired");
        AssertEqual(10, samus.Missiles, "missile station restores missiles");
        AssertEqual(2, plms.StationActivationEvents.Count,
            "two station parents publish two explicit activations");
        AssertTrue(samus.InputLocked,
            "station access remains locked while bank-$85 owns the completion message");

        // The runtime freezes this PLM while the message is open. Once bank $85 returns,
        // the original instruction lists execute three six-frame phases: post-message
        // hold, retract, and final hold. Both simultaneously active fixtures must release
        // their shared Samus owner at the native endpoint instead of looping access sound.
        for (int frame = 0; frame < 18; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(0x8128, level.GetCollisionBlock(7, 6).LevelWord,
            "map access retracts to its compiled right-side parent word");
        AssertEqual(0x8528, level.GetCollisionBlock(4, 6).LevelWord,
            "map access retracts its linked left-side block");
        AssertEqual(0xb4c3, level.GetCollisionBlock(13, 6).LevelWord,
            "missile access retracts to its compiled resource word");
        AssertTrue(!samus.InputLocked,
            "map/resource stations unlock Samus after post-message retraction");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(18, 6), 0x4d),
            "save trigger resolves its same-block parent");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(StationKind.Save, plms.StationActivationEvents.Single().Kind,
            "save trigger publishes typed confirmation request");
        AssertEqual(3, plms.StationActivationEvents.Single().StationIndex,
            "save request preserves native room argument/station index");

        StationActivationEvent saveRequest = plms.StationActivationEvents.Single();
        samus.XPosition = 0x0127;
        AssertTrue(plms.ResolveSaveStationConfirmation(bus, saveRequest, accepted: true),
            "accepted save resumes the sleeping cartridge PLM");
        AssertEqual(0x0120, samus.XPosition,
            "save animation centers Samus on the station's 16-pixel boundary");
        AssertTrue(SamusState.IsForwardFacingPose(samus.Pose),
            "save animation applies shared MakeSamusFaceForward setup");
        AssertTrue(samus.InputLocked, "save animation owns Samus input");
        AssertEqual(level.GetBlockIndex(18, 6), saveRequest.BlockIndex,
            "save activation retains the executing PLM block for bank-$86 electricity");
        VerifySaveStationElectricity(bus, level, saveRequest.BlockIndex);

        StationActivationEvent completion = default;
        bool completionPublished = false;
        int completionFrame = -1;
        int maximumSaveAnimationFrames =
            SaveStationAnimationDefinitions.SaveAnimationLoops * 8 + 8;
        for (int frame = 0; frame < maximumSaveAnimationFrames && !completionPublished; frame++)
        {
            plms.Step(bus, level, streamer, 0, 0, 0);
            if (frame == 0)
            {
                AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2e), 6)),
                    "save animation queues cartridge library-one sound $2E");
            }
            if (plms.StationActivationEvents.Count != 0)
            {
                completion = plms.StationActivationEvents.Single();
                completionPublished = true;
                completionFrame = frame;
            }
        }
        AssertTrue(completionPublished, "save animation reaches completion message");
        AssertEqual(SaveStationAnimationDefinitions.SaveAnimationLoops * 8 - 4,
            completionFrame,
            "save animation publishes completion after all 21 alternating native loops");
        AssertEqual(GameplayMessageIds.SaveCompleted, completion.MessageBoxIndex,
            "save animation publishes game-saved message only after its draw loop");
        plms.CompleteSaveStation(completion);
        AssertTrue(!samus.InputLocked, "game-saved message return unlocks Samus");
        AssertTrue(plms.Stations.Single(station => station.Kind == StationKind.Save)
            .SaveStationLockedOut,
            "save completion installs once-per-room-entry lockout");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(18, 6), 0x4d),
            "locked save trigger remains a solid station block");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.StationActivationEvents.Count,
            "locked save station cannot reopen until room re-entry");
    }

    private static void VerifySaveStationElectricity(
        TestAddressSpace bus,
        RoomLevelData level,
        int saveBlockIndex)
    {
        // The native control stream is compiled. Constructed artwork is supplied by
        // operand identity, rather than shortening a bank-$86 program on a fake bus.
        bus.WriteBytes(0xa19600, [0xff, 0xff]);
        bus.WriteBytes(0x86e6d2,
        [
            0xad, 0xe6,
            0xd1, 0xe6,
            0x83, 0xe6,
            0x00, 0x00,
            0x00, 0x30,
            0x00, 0x00,
            0xfc, 0x84,
        ]);
        bus.WriteBytes(0x86e683,
        [
            0xd5, 0x81, 0x14, 0x00,
            0x01, 0x00, 0x62, 0xb5,
            0x54, 0x81,
        ]);
        // Enemy-projectile animation lists live in bank $86, but their frame maps are
        // consumed by the shared bank-$8D spritemap writer.
        bus.WriteBytes(0x8db562,
        [
            0x01, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x20,
        ]);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            populationPointer: 0x9600,
            tilesetPointer: 0,
            new SnesVram(),
            new SnesCgram(),
            nextRandom: () => 0x4040,
            level: level,
            samus: new SamusState());
        enemies.SpawnSaveStationElectricity(saveBlockIndex, level.WidthInBlocks);

        var visible = new SpriteVisualPart
        {
            OffsetX = 0, OffsetY = 0, TileColumn = 1, TileRow = 0,
            Size = 8, Priority = 1, Palette = 0, FlipX = false, FlipY = false,
        };
        HashSet<ushort> electricityOperands = Enumerable.Range(0,
            SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount)
            .Select(SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress).ToHashSet();
        var artworkDocument = new EnemyProjectileSpritemapDocument
        {
            Version = EnemyProjectileSpritemapDefinitions.Version,
            Frames = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(frame => frame.Item2, _ => Array.Empty<SpriteVisualPart>()),
            ProgramFrames = EnemyProjectilePresentationFrameDefinitions.All.ToArray().ToDictionary(
                frame => frame.Name, frame => electricityOperands.Contains(frame.OperandAddress) ? new[] { visible } : Array.Empty<SpriteVisualPart>()),
        };
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
            projectileSpritemaps: EnemyProjectileSpritemapCatalog.Load(new MemoryStream(
                EnemyProjectileSpritemapCatalog.Write(artworkDocument))));

        RoomEnemyProjectileSlot electricity = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertEqual(0x0130, electricity.XPosition,
            "save electricity starts one block right of the station PLM");
        AssertEqual(0x0040, electricity.YPosition,
            "save electricity starts two blocks above the station PLM");

        enemies.StepEnemyProjectiles(level, samus: null);
        AssertEqual(SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(0),
            electricity.PresentationOperandAddress, "save electricity selects its first installed frame through the compiled interpreter");
        AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, electricity.SpritemapPointer,
            "installed program artwork does not retain a cartridge spritemap pointer");
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawEnemyProjectiles(oam, cameraX: 0x0100, cameraY: 0);
        oam.FinalizeFrame();
        AssertTrue(Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .Any(entry => entry.X == 0x0030 && entry.Y == 0x40),
            "save electricity reaches the ordinary room enemy-projectile draw path");
    }

    private static void VerifyUnsupportedPopulationContext(TestAddressSpace bus)
    {
        const ushort population = 0x9400;
        const ushort unsupportedHeader = 0xb123;
        WriteWord(bus, 0x840000 | unsupportedHeader, 0x9876);
        WriteWord(bus, 0x840000 | unsupportedHeader + 2, 0x9abc);
        bus.WriteBytes(0x8f0000 | population, [
            0x23, 0xb1, 0x03, 0x04, 0x55, 0xaa, 0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(
            8,
            8,
            new ushort[64],
            new byte[64],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        NotSupportedException? error = null;
        try
        {
            plms.LoadRoomPopulation(
                bus,
                level,
                level.CreateBackgroundStreamer(),
                new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, population),
                new Bank80SystemState(),
                0,
                () => new SamusState(),
                () => false);
        }
        catch (NotSupportedException caught)
        {
            error = caught;
        }
        AssertTrue(error is not null,
            "unsupported room-population header fails at load time");
        AssertTrue(error!.Message.Contains("$8F:9400", StringComparison.Ordinal) &&
            error.Message.Contains("record 0", StringComparison.Ordinal) &&
            error.Message.Contains("$84:B123", StringComparison.Ordinal) &&
            error.Message.Contains("(3,4)", StringComparison.Ordinal) &&
            error.Message.Contains("$AA55", StringComparison.Ordinal),
            "unsupported PLM failure includes population/header/record/coordinate/argument context");
    }

    private static void VerifySaveStationConfirmation(TestAddressSpace bus)
    {
        GameplayMessageTitleCell[] Cells(int count, ushort first) => Enumerable.Range(0, count)
            .Select(index => new GameplayMessageTitleCell { Raw = unchecked((ushort)(first + index)) }).ToArray();
        var notices = new GameplayMessageNoticeDocument
        {
            Version = GameplayMessageNoticeDefinitions.Version,
            Border = Cells(GameplayMessageRomData.Layout.TilemapWidth, 0x3801),
            Notices = GameplayMessageNoticeDefinitions.MessageIds.ToArray().ToDictionary(id => id.ToString(), id =>
                new GameplayMessageNotice
                {
                    RowCount = GameplayMessageNoticeDefinitions.ContentRows(id),
                    Template = Cells(GameplayMessageNoticeDefinitions.ContentRows(id) * GameplayMessageRomData.Layout.TilemapWidth, 0x2800),
                    Text = [new GameplayMessageTextRegion { Row = 0, Column = 0, Width = 4,
                        Alignment = GameplayMessageNoticeDefinitions.LeftAlignment, Text = "TEST", Palette = 0 }],
                    YesSelection = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id) ? Cells(32, 0x5100) : null,
                    NoSelection = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id) ? Cells(32, 0x5200) : null,
                }),
        };
        using var noticeJson = new MemoryStream();
        GameplayMessageNoticePresentation.Write(noticeJson, notices);
        noticeJson.Position = 0;
        GameplayMessageNoticePresentation presentation = GameplayMessageNoticePresentation.Load(noticeJson);
        const int definitions = 0x85869b;
        int message23 = definitions + (0x17 - 1) * 6;
        WriteWord(bus, message23, 0x8436);
        WriteWord(bus, message23 + 2, 0x8289);
        WriteWord(bus, message23 + 4, 0x9800);
        WriteWord(bus, message23 + 6, 0x8436);
        WriteWord(bus, message23 + 8, 0x8289);
        WriteWord(bus, message23 + 10, 0x9900);
        WriteWord(bus, message23 + 12, 0x8436);
        WriteWord(bus, message23 + 14, 0x8289);
        WriteWord(bus, message23 + 16, 0x9940);
        for (int word = 0; word < 32; word++)
        {
            WriteWord(bus, 0x858040 + word * 2, 0x3801);
            WriteWord(bus, 0x859581 + (32 + word) * 2,
                unchecked((ushort)(0x5100 + word)));
            WriteWord(bus, 0x859581 + (64 + word) * 2,
                unchecked((ushort)(0x5200 + word)));
        }
        for (int word = 0; word < 128; word++)
            WriteWord(bus, 0x859800 + word * 2, unchecked((ushort)(0x2800 + word)));
        for (int word = 0; word < 32; word++)
            WriteWord(bus, 0x859900 + word * 2, unchecked((ushort)(0x2a00 + word)));

        var message = new GameplayMessageBoxState();
        message.BindPresentation(null, notices: presentation);
        message.Begin(bus, GameplayMessageIds.SaveConfirmation);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "save message enters shared yes/no selection phase");
        AssertEqual(0x5100, message.Tilemap[128],
            "save message begins with native selected-YES row");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "save cursor changes the shared confirmation selection");
        AssertEqual(0x5200, message.Tilemap[128],
            "save cursor redraws the native selected-NO row");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "holding a direction does not toggle the newly-pressed save cursor repeatedly");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(false, message.ConsumeConfirmationResult(),
            "save confirmation publishes selected no result after close");

        message.Begin(bus, GameplayMessageIds.SaveConfirmation);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(true, message.ConsumeConfirmationResult(),
            "save confirmation defaults to yes and publishes acceptance");
    }

    private static void SeedRoomPlmPopulationRom(TestAddressSpace bus)
    {
        SeedPoseOneSamusData(bus);
        // Header instruction-list fields consumed by the generic allocator.
        WriteWord(bus, 0x84b705, 0xaf86);
        WriteWord(bus, 0x84b63d, 0xafa4);
        WriteWord(bus, 0x84b70d, 0xafb6);
        WriteWord(bus, 0x84b6e1, 0xadc2);
        WriteWord(bus, 0x84b6d5, 0xad62);
        WriteWord(bus, 0x84b6ed, 0xae4c);
        WriteWord(bus, 0x84b771, 0xafe8);
        // Elevator's four synthetic frames. Station timing, pointer selection, and
        // draw payloads are compiled, so this sparse bus carries none of those bytes.
        SeedTimedDrawLoop(bus, 0xafb6, 4, 4, 0xa100);
        WriteWord(bus, 0x84afc6, 0x8724);
        WriteWord(bus, 0x84afc8, 0xafb6);
    }

    private static void SeedTimedDrawLoop(
        TestAddressSpace bus,
        ushort list,
        ushort timer,
        int frameCount,
        ushort firstDraw)
    {
        for (int frame = 0; frame < frameCount; frame++)
        {
            ushort draw = unchecked((ushort)(firstDraw + frame * 6));
            WriteWord(bus, 0x840000 | unchecked((ushort)(list + frame * 4)), timer);
            WriteWord(bus, 0x840000 | unchecked((ushort)(list + frame * 4 + 2)), draw);
            WriteOneBlockDraw(bus, draw, unchecked((ushort)(0x8000 | frame)));
        }
    }
}
