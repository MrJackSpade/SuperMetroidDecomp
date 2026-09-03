using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Executes every translated pure-scroll door callback against the bytes in the private
/// retail cartridge and compares its complete 50-byte result with the C# definition table.
/// </summary>
internal static class DoorSetupCallbackAudit
{
    private const int ScratchScrollSource = 0x7e2000;
    private const int ExpectedPureScrollCallbackCount = 74;
    private const int ExpectedRetailCallbackCount = 82;
    private const int ExpectedConfiguredDoorHeaderCount = 94;
    private const byte UnwrittenSentinel = 0x7f;
    private const ushort WreckedShipEntranceRoom = 0xca08;
    private const ushort MaridiaElevatubeRoom = 0xd408;
    private const ushort OasisRoom = 0xd48e;
    private const ushort PlasmaSparkRoom = 0xd340;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ushort[] pointers = DoorScrollPrograms.Pointers.Order().ToArray();
        if (pointers.Length != ExpectedPureScrollCallbackCount)
        {
            throw new InvalidDataException(
                $"Translated {pointers.Length} pure door callbacks; the retail catalog contains " +
                $"{ExpectedPureScrollCallbackCount}.");
        }

        VerifyRetailHeaderInventory(bus, pointers);
        foreach (ushort pointer in pointers)
            VerifyProgram(bus, pointer);

        VerifyReportedFlywayHeader(bus);
        VerifyCeresMode7Callbacks(bus);
        VerifyWreckedShipTreadmillCallbacks(bus);
        VerifyMaridiaElevatubeCallbacks(bus);

        Console.WriteLine(
            $"Door setup callback audit passed all {ExpectedConfiguredDoorHeaderCount} configured " +
            $"headers and {ExpectedRetailCallbackCount} retail callbacks: " +
            $"{pointers.Length} pure scroll, 2 Ceres Mode-7, " +
            "2 Wrecked Ship treadmill, and 4 Maridia elevatube programs.");
        return 0;
    }

    private static void VerifyRetailHeaderInventory(
        SuperMetroidAddressSpace bus,
        IReadOnlyCollection<ushort> pureScrollPointers)
    {
        ushort[] headerPointers = RetailDoorHeaderCatalog.EnumeratePointers().ToArray();
        if (headerPointers.Length != RetailDoorHeaderCatalog.HeaderCount)
        {
            throw new InvalidDataException(
                $"Retail door ranges produced {headerPointers.Length} headers, expected " +
                $"{RetailDoorHeaderCatalog.HeaderCount}.");
        }

        CartridgeDoorHeader[] configuredHeaders = headerPointers
            .Select(pointer => CartridgeDoorHeader.Load(bus, pointer))
            .Where(header => header.SetupCodePointer != 0)
            .ToArray();
        if (configuredHeaders.Length != ExpectedConfiguredDoorHeaderCount)
        {
            throw new InvalidDataException(
                $"Retail door inventory contains {configuredHeaders.Length} configured headers, " +
                $"expected {ExpectedConfiguredDoorHeaderCount}.");
        }

        ushort[] configuredCallbacks = configuredHeaders
            .Select(header => header.SetupCodePointer)
            .Distinct()
            .Order()
            .ToArray();
        if (configuredCallbacks.Length != ExpectedRetailCallbackCount)
        {
            throw new InvalidDataException(
                $"Configured retail headers reference {configuredCallbacks.Length} callbacks, " +
                $"expected {ExpectedRetailCallbackCount}.");
        }

        ushort[] translatedCallbacks = pureScrollPointers
            .Concat(StatefulCallbackPointers)
            .Distinct()
            .Order()
            .ToArray();
        if (!configuredCallbacks.SequenceEqual(translatedCallbacks))
        {
            string missing = string.Join(", ", configuredCallbacks
                .Except(translatedCallbacks)
                .Select(pointer => $"$8F:{pointer:X4}"));
            string extra = string.Join(", ", translatedCallbacks
                .Except(configuredCallbacks)
                .Select(pointer => $"$8F:{pointer:X4}"));
            throw new InvalidDataException(
                $"Door callback catalog mismatch; missing=[{missing}], extra=[{extra}].");
        }

        foreach (CartridgeDoorHeader header in configuredHeaders)
        {
            RoomScrollGrid scratch = CreateScratchScrollGrid(bus);
            // This is the exact production interpreter call made before the runtime's
            // stateful dispatch. Unknown pointers throw, so every one of the 94 headers
            // must reach a translated branch rather than merely sharing a known address.
            DoorSetupCodeInterpreter.ApplyScrollWrites(
                header.SetupCodePointer,
                header.Pointer,
                scratch);
        }
    }

    private static readonly ushort[] StatefulCallbackPointers =
    [
        DoorCodes.DoorASM_ToCeresElevatorShaft,
        DoorCodes.DoorASM_FromCeresElevatorShaft,
        DoorCodes.DoorASM_StartWreckedShipTreadmillWestEntrance,
        DoorCodes.DoorASM_StartWreckedShipTreadmillEastEntrance,
        DoorCodes.DoorASM_SetupElevatubeFromSouth,
        DoorCodes.DoorASM_SetupElevatubeFromNorth,
        DoorCodes.DoorASM_ResetElevatubeOnNorthExit,
        DoorCodes.DoorASM_ResetElevatubeOnSouthExit,
    ];

    private static void VerifyReportedFlywayHeader(SuperMetroidAddressSpace bus)
    {
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, DoorPointers.ParlorFromFlyway);
        if (door.SetupCodePointer != DoorCodes.DoorASM_Scroll_4_Red_8_Green)
        {
            throw new InvalidDataException(
                $"Issue #63 door $83:{door.Pointer:X4} names setup $8F:" +
                $"{door.SetupCodePointer:X4}, expected $8F:" +
                $"{DoorCodes.DoorASM_Scroll_4_Red_8_Green:X4}.");
        }

        RoomScrollGrid scrolls = CreateScratchScrollGrid(bus);
        DoorSetupCodeInterpreter.ApplyScrollWrites(
            door.SetupCodePointer,
            door.Pointer,
            scrolls);
        for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        {
            byte expected = index switch
            {
                4 => (byte)RoomScrollState.RedBoundary,
                8 => (byte)RoomScrollState.Green,
                _ => UnwrittenSentinel,
            };
            if (scrolls.ReadStorage(index) != expected)
            {
                throw new InvalidDataException(
                    $"Issue #63 door wrote scroll[{index:X2}]=" +
                    $"${scrolls.ReadStorage(index):X2}, expected ${expected:X2}.");
            }
        }
    }

    private static void VerifyCeresMode7Callbacks(SuperMetroidAddressSpace bus)
    {
        SuperMetroidRuntime runtime = CreateRuntime(bus);
        CartridgeDoorHeader intoShaft = CartridgeDoorHeader.Load(
            bus,
            DoorPointers.ToCeresElevatorShaft);
        runtime.LoadCartridgeRoomThroughDoorForVerification(intoShaft);
        if (runtime.ActiveSamusMode7Transform is null ||
            runtime.DisplayedSamusMode7Transform is null ||
            !runtime.CeresElevatorShaft.IsActive)
        {
            throw new InvalidDataException(
                "Ceres entry callback $8F:E4E0 did not establish Mode-7 shaft state.");
        }

        CartridgeDoorHeader fromShaft = CartridgeDoorHeader.Load(
            bus,
            DoorPointers.FromCeresElevatorShaft);
        runtime.LoadCartridgeRoomThroughDoorForVerification(fromShaft);
        if (runtime.ActiveSamusMode7Transform is not null ||
            runtime.DisplayedSamusMode7Transform is not null ||
            runtime.CeresElevatorShaft.IsActive)
        {
            throw new InvalidDataException(
                "Ceres exit callback $8F:E513 did not restore ordinary Mode-1 state.");
        }
    }

    private static void VerifyWreckedShipTreadmillCallbacks(SuperMetroidAddressSpace bus)
    {
        VerifyWreckedShipTreadmill(
            bus,
            DoorPointers.WreckedShipEntranceFromWestOcean,
            WreckedShipTreadmillDirection.Rightwards,
            RoomPlmHeaders.WreckedShipEntranceTreadmillFromWest,
            WreckedShipTreadmillPlmRomData.RightwardsBehavior,
            [
                WreckedShipTreadmillRomData.Frame0Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame3Source,
            ]);
        VerifyWreckedShipTreadmill(
            bus,
            DoorPointers.WreckedShipEntranceFromMainShaft,
            WreckedShipTreadmillDirection.Leftwards,
            RoomPlmHeaders.WreckedShipEntranceTreadmillFromEast,
            WreckedShipTreadmillPlmRomData.LeftwardsBehavior,
            [
                WreckedShipTreadmillRomData.Frame3Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame0Source,
            ]);
    }

    private static void VerifyWreckedShipTreadmill(
        SuperMetroidAddressSpace bus,
        ushort doorPointer,
        WreckedShipTreadmillDirection direction,
        ushort expectedPlmHeader,
        byte expectedBts,
        ReadOnlySpan<int> expectedSources)
    {
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, doorPointer);
        if (door.DestinationRoomPointer != WreckedShipEntranceRoom)
        {
            throw new InvalidDataException(
                $"Treadmill door $83:{doorPointer:X4} targets ${door.DestinationRoomPointer:X4}, " +
                $"not Wrecked Ship entrance ${WreckedShipEntranceRoom:X4}.");
        }

        // First prove the pre-Phantoon branch: setup always clears all 56 entries, while
        // the first PLM handler pass deletes without installing type-three treadmill BTS.
        SuperMetroidRuntime inactiveRuntime = CreateRuntime(bus);
        inactiveRuntime.LoadCartridgeRoomForDebug(WreckedShipEntranceRoom);
        inactiveRuntime.RunDoorSetupForVerification(door);
        AssertTreadmillWords(
            inactiveRuntime,
            WreckedShipTreadmillPlmRomData.BlankAirWord,
            expectedBts: null);
        AssertResidentHeader(inactiveRuntime.Plms, expectedPlmHeader);
        StepPlms(inactiveRuntime);
        if (inactiveRuntime.Plms.PopulationSlots.Any(slot => slot.HeaderPointer == expectedPlmHeader))
            throw new InvalidDataException($"Treadmill PLM ${expectedPlmHeader:X4} did not delete.");
        AssertTreadmillWords(
            inactiveRuntime,
            WreckedShipTreadmillPlmRomData.BlankAirWord,
            expectedBts: null);

        // Then select the powered Wrecked Ship state before room load and prove both the
        // collision row and the bank-$87 source order produced by the same door callback.
        SuperMetroidRuntime poweredRuntime = CreateRuntime(bus);
        poweredRuntime.System.SetBossBits(areaIndex: AreaId.WreckedShip, BossBits.AreaBoss);
        poweredRuntime.LoadCartridgeRoomForDebug(WreckedShipEntranceRoom);
        poweredRuntime.RunDoorSetupForVerification(door);
        if (!poweredRuntime.WreckedShipTreadmill.IsActive ||
            poweredRuntime.WreckedShipTreadmill.Direction != direction)
        {
            throw new InvalidDataException(
                $"Door $83:{doorPointer:X4} did not spawn {direction} animated tiles.");
        }
        AssertTreadmillWords(
            poweredRuntime,
            WreckedShipTreadmillPlmRomData.BlankAirWord,
            expectedBts: null);
        StepPlms(poweredRuntime);
        AssertTreadmillWords(
            poweredRuntime,
            WreckedShipTreadmillPlmRomData.ActiveTreadmillWord,
            expectedBts);

        for (int frame = 0; frame < expectedSources.Length; frame++)
        {
            poweredRuntime.WreckedShipTreadmill.Step(
                areaBossDefeated: true,
                poweredRuntime.VramWrites);
            if (poweredRuntime.WreckedShipTreadmill.LastSourceAddress != expectedSources[frame])
            {
                throw new InvalidDataException(
                    $"Door $83:{doorPointer:X4} treadmill frame {frame} selected " +
                    $"${poweredRuntime.WreckedShipTreadmill.LastSourceAddress:X6}, expected " +
                    $"${expectedSources[frame]:X6}.");
            }
            VramWriteEntry transfer = poweredRuntime.VramWrites.Entries[^1];
            if (transfer.SizeInBytes != WreckedShipTreadmillRomData.TransferByteCount ||
                transfer.EncodedVramDestination !=
                    WreckedShipTreadmillRomData.EncodedVramDestination)
            {
                throw new InvalidDataException(
                    $"Door $83:{doorPointer:X4} published a malformed treadmill VRAM transfer.");
            }
        }
    }

    private static void VerifyMaridiaElevatubeCallbacks(SuperMetroidAddressSpace bus)
    {
        VerifyMaridiaElevatubeEntry(
            bus,
            DoorPointers.MaridiaElevatubeFromSouth,
            fromSouth: true,
            expectedPosition: MaridiaElevatubeRomData.SouthStartingPosition,
            expectedVelocity: MaridiaElevatubeRomData.SouthStartingVelocity,
            expectedAcceleration: MaridiaElevatubeRomData.SouthAcceleration,
            expectedPositionAfterStep: 0x09bf,
            expectedVelocityAfterStep: 0xfee0);
        VerifyMaridiaElevatubeEntry(
            bus,
            DoorPointers.MaridiaElevatubeFromNorth,
            fromSouth: false,
            expectedPosition: MaridiaElevatubeRomData.NorthStartingPosition,
            expectedVelocity: MaridiaElevatubeRomData.NorthStartingVelocity,
            expectedAcceleration: MaridiaElevatubeRomData.NorthAcceleration,
            expectedPositionAfterStep: 0x0041,
            expectedVelocityAfterStep: 0x0120);

        VerifyMaridiaElevatubeExit(
            bus,
            DoorPointers.MaridiaElevatubeSouthExit,
            OasisRoom,
            expectsGreenLeadingScrolls: true);
        VerifyMaridiaElevatubeExit(
            bus,
            DoorPointers.MaridiaElevatubeNorthExit,
            PlasmaSparkRoom,
            expectsGreenLeadingScrolls: false);
    }

    private static void VerifyMaridiaElevatubeEntry(
        SuperMetroidAddressSpace bus,
        ushort doorPointer,
        bool fromSouth,
        ushort expectedPosition,
        ushort expectedVelocity,
        ushort expectedAcceleration,
        ushort expectedPositionAfterStep,
        ushort expectedVelocityAfterStep)
    {
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, doorPointer);
        if (door.DestinationRoomPointer != MaridiaElevatubeRoom)
        {
            throw new InvalidDataException(
                $"Elevatube entry $83:{doorPointer:X4} targets " +
                $"${door.DestinationRoomPointer:X4}, not ${MaridiaElevatubeRoom:X4}.");
        }

        SuperMetroidRuntime runtime = CreateRuntime(bus);
        runtime.LoadCartridgeRoomForDebug(MaridiaElevatubeRoom);
        runtime.RunDoorSetupForVerification(door);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Elevatube entry lost Samus.");
        MaridiaElevatubeRoomMainState state = runtime.MaridiaElevatube;
        if (!state.IsActive || !samus.InputLocked || state.PositionSubposition != 0 ||
            state.Position != expectedPosition || state.Velocity != expectedVelocity ||
            state.Acceleration != expectedAcceleration)
        {
            throw new InvalidDataException(
                $"Elevatube {(fromSouth ? "south" : "north")} setup words do not match ROM.");
        }
        AssertResidentHeader(runtime.Plms, RoomPlmHeaders.MaridiaElevatube);

        // Place Samus in open tube space so this assertion isolates $E2B6's arithmetic
        // rather than a doorway boundary inherited from direct debug loading.
        samus.XPosition = 0x0040;
        samus.Kinematics.XSubposition = 0xffff;
        samus.YPosition = 0x0500;
        samus.Kinematics.YSubposition = 0;
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "Elevatube room has no level data.");
        _ = state.Step(bus, level, samus, nmiFrameCounter: 0, runtime.Plms);
        if (samus.XPosition != MaridiaElevatubeRomData.SamusCenterX ||
            samus.Kinematics.XSubposition != 0 || state.Position != expectedPositionAfterStep ||
            state.PositionSubposition != 0 || state.Velocity != expectedVelocityAfterStep)
        {
            throw new InvalidDataException(
                $"Elevatube {(fromSouth ? "south" : "north")} first room-main step diverged.");
        }

        bool soundObserved = false;
        for (int frame = 0; frame < 17; frame++)
        {
            StepPlms(runtime);
            soundObserved |= runtime.Plms.SoundRequests.Any(
                sound => sound is { Library: 2, SoundId: 0x15, MaximumQueued: 6 });
        }
        if (!soundObserved || runtime.Plms.PopulationSlots.Any(
                slot => slot.HeaderPointer == RoomPlmHeaders.MaridiaElevatube))
        {
            throw new InvalidDataException(
                "Elevatube PLM did not queue library-two sound $15 and delete after 16 frames.");
        }
    }

    private static void VerifyMaridiaElevatubeExit(
        SuperMetroidAddressSpace bus,
        ushort doorPointer,
        ushort destinationRoom,
        bool expectsGreenLeadingScrolls)
    {
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, doorPointer);
        if (door.DestinationRoomPointer != destinationRoom)
        {
            throw new InvalidDataException(
                $"Elevatube exit $83:{doorPointer:X4} targets ${door.DestinationRoomPointer:X4}, " +
                $"expected ${destinationRoom:X4}.");
        }

        SuperMetroidRuntime runtime = CreateRuntime(bus);
        runtime.LoadCartridgeRoomForDebug(destinationRoom);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Elevatube exit lost Samus.");
        samus.InputLocked = true;
        runtime.RunDoorSetupForVerification(door);
        if (samus.InputLocked || runtime.MaridiaElevatube.IsActive)
            throw new InvalidDataException($"Elevatube exit $83:{doorPointer:X4} did not unlock Samus.");
        if (expectsGreenLeadingScrolls)
        {
            RoomScrollGrid scrolls = runtime.Camera?.Scrolls ?? throw new InvalidDataException(
                "Elevatube south exit has no scroll grid.");
            if (scrolls.ReadStorage(0) != (byte)RoomScrollState.Green ||
                scrolls.ReadStorage(1) != (byte)RoomScrollState.Green)
            {
                throw new InvalidDataException(
                    "Elevatube south exit did not reproduce the cartridge's $0202 scroll write.");
            }
        }
    }

    private static SuperMetroidRuntime CreateRuntime(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        return runtime;
    }

    private static void StepPlms(SuperMetroidRuntime runtime)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "Door callback audit has no active level.");
        BackgroundTilemapStreamer streamer = runtime.BackgroundStreamer ??
            throw new InvalidDataException("Door callback audit has no background streamer.");
        ScrollBoundaryCamera camera = runtime.Camera ?? throw new InvalidDataException(
            "Door callback audit has no camera.");
        _ = runtime.Plms.Step(
            runtime.AddressSpace,
            level,
            streamer,
            camera.XPosition,
            camera.YPosition,
            runtime.BackgroundScroll.Bg1XOffset,
            camera.Scrolls);
    }

    private static void AssertResidentHeader(RoomPlmSystem plms, ushort header)
    {
        if (!plms.PopulationSlots.Any(slot => slot.HeaderPointer == header))
            throw new InvalidDataException($"Door callback did not spawn PLM header $84:{header:X4}.");
    }

    private static void AssertTreadmillWords(
        SuperMetroidRuntime runtime,
        ushort expectedLevelWord,
        byte? expectedBts)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "Treadmill callback has no active level.");
        int origin = level.GetBlockIndex(
            WreckedShipTreadmillPlmRomData.BlockX,
            WreckedShipTreadmillPlmRomData.BlockY);
        for (int offset = 0; offset < WreckedShipTreadmillPlmRomData.BlockCount; offset++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(origin + offset);
            if (block.LevelWord != expectedLevelWord ||
                (expectedBts is byte bts && block.Behavior != bts))
            {
                throw new InvalidDataException(
                    $"Treadmill block {offset} is word ${block.LevelWord:X4}/BTS " +
                    $"${block.Behavior:X2}; expected ${expectedLevelWord:X4}/" +
                    (expectedBts is byte value ? $"${value:X2}." : "unchanged."));
            }
        }
    }

    private static void VerifyProgram(SuperMetroidAddressSpace bus, ushort pointer)
    {
        var expected = Enumerable.Repeat(
            UnwrittenSentinel,
            RoomScrollGrid.StorageByteCount).ToArray();
        ExecuteCartridgeScrollProgram(bus, pointer, expected);

        RoomScrollGrid actual = CreateScratchScrollGrid(bus);

        if (!DoorScrollPrograms.TryApply(pointer, actual))
            throw new InvalidDataException($"Door callback $8F:{pointer:X4} was not recognized.");

        for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        {
            if (actual.ReadStorage(index) == expected[index])
                continue;
            throw new InvalidDataException(
                $"Door callback $8F:{pointer:X4} produced scroll[{index:X2}]=" +
                $"${actual.ReadStorage(index):X2}; cartridge instructions produce " +
                $"${expected[index]:X2}.");
        }
    }

    private static RoomScrollGrid CreateScratchScrollGrid(SuperMetroidAddressSpace bus)
    {
        for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
            bus.WriteByte(ScratchScrollSource + index, UnwrittenSentinel);
        return RoomScrollGrid.LoadExplicit(
            bus,
            ScratchScrollSource,
            widthInScreens: 10,
            heightInScreens: 5);
    }

    /// <summary>
    /// Interprets only the straight-line instruction subset used by retail's pure scroll
    /// callbacks. Encountering any other opcode fails the audit instead of quietly blessing
    /// a C# definition after the cartridge routine gained an unmodeled side effect.
    /// </summary>
    private static void ExecuteCartridgeScrollProgram(
        SuperMetroidAddressSpace bus,
        ushort pointer,
        Span<byte> scrolls)
    {
        int pc = 0x8f0000 | pointer;
        bool accumulatorIsEightBit = false;
        ushort accumulator = 0;

        for (int instruction = 0; instruction < 32; instruction++)
        {
            byte opcode = bus.ReadByte(pc++);
            switch (opcode)
            {
                case 0x08: // PHP
                case 0x28: // PLP
                    break;

                case 0xe2: // SEP #$20
                    byte sepMask = bus.ReadByte(pc++);
                    if (sepMask != 0x20)
                        throw Unsupported(pointer, opcode, pc - 2);
                    accumulatorIsEightBit = true;
                    break;

                case 0xa9: // LDA immediate
                    accumulator = bus.ReadByte(pc++);
                    if (!accumulatorIsEightBit)
                        accumulator |= unchecked((ushort)(bus.ReadByte(pc++) << 8));
                    break;

                case 0x8f: // STA long
                    int destination = bus.ReadByte(pc) |
                        (bus.ReadByte(pc + 1) << 8) |
                        (bus.ReadByte(pc + 2) << 16);
                    pc += 3;
                    int storageIndex = destination - RoomScrollGrid.WorkRamAddress;
                    if ((uint)storageIndex >= RoomScrollGrid.StorageByteCount)
                        throw Unsupported(pointer, opcode, pc - 4);
                    scrolls[storageIndex] = unchecked((byte)accumulator);
                    if (!accumulatorIsEightBit)
                    {
                        if (storageIndex + 1 >= scrolls.Length)
                            throw Unsupported(pointer, opcode, pc - 4);
                        scrolls[storageIndex + 1] = unchecked((byte)(accumulator >> 8));
                    }
                    break;

                case 0x60: // RTS
                    return;

                default:
                    throw Unsupported(pointer, opcode, pc - 1);
            }
        }

        throw new InvalidDataException(
            $"Door callback $8F:{pointer:X4} did not return within 32 instructions.");
    }

    private static InvalidDataException Unsupported(
        ushort pointer,
        byte opcode,
        int opcodeAddress) =>
        new(
            $"Door callback $8F:{pointer:X4} uses unsupported reference-audit opcode " +
            $"${opcode:X2} at ${opcodeAddress >> 16:X2}:{opcodeAddress & 0xffff:X4}.");
}
