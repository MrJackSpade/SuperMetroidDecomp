using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private const int PostCeresLandingMaximumFrames = 700;

    private static void VerifyCompiledRoomDefinitionIntegration()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace oracleBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        HashSet<int> forbidden = BuildCompiledRoomDefinitionAddressSet(oracleBus);
        var guard = new CompiledRoomDefinitionReadGuard(
            SuperMetroidAddressSpace.LoadRetailRom(romPath), forbidden);

        var expected = new SuperMetroidRuntime(
            SuperMetroidAddressSpace.LoadRetailRom(romPath));
        var actual = new SuperMetroidRuntime(guard);
        InitialViewportResult expectedViewport = InitializePostCeresLanding(expected);
        InitialViewportResult actualViewport = InitializePostCeresLanding(actual);

        AssertEqual(expectedViewport, actualViewport,
            "guarded post-Ceres room entry retains initial viewport work");
        AssertEqual(expected.ActiveLoadStation, actual.ActiveLoadStation,
            "guarded post-Ceres room entry retains compiled station placement");
        AssertEqual(expected.ActiveDoor, actual.ActiveDoor,
            "guarded post-Ceres room entry retains compiled incoming door");
        AssertEqual(expected.ActiveRoom, actual.ActiveRoom,
            "guarded post-Ceres room entry retains compiled header, selection, and state");
        AssertTrue(expected.Camera!.Scrolls.Storage.SequenceEqual(
                actual.Camera!.Scrolls.Storage),
            "guarded post-Ceres room entry retains compiled scroll constraints");
        AssertEqual((expected.Camera.XPosition, expected.Camera.YPosition),
            (actual.Camera.XPosition, actual.Camera.YPosition),
            "guarded post-Ceres room entry retains camera placement");
        AssertEqual((expected.Samus!.Kinematics.XFixed, expected.Samus.Kinematics.YFixed),
            (actual.Samus!.Kinematics.XFixed, actual.Samus.Kinematics.YFixed),
            "guarded post-Ceres room entry retains Samus placement");

        ushort initialCameraY = actual.Camera.YPosition;
        int frames = 0;
        bool cameraMoved = false;
        while (actual.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               frames < PostCeresLandingMaximumFrames)
        {
            RuntimeFrameResult expectedFrame = expected.StepFrame(0);
            RuntimeFrameResult actualFrame = actual.StepFrame(0);
            AssertEqual(expectedFrame, actualFrame,
                $"guarded post-Ceres frame result {frames}");
            AssertEqual(
                (expected.Camera.XPosition, expected.Camera.XSubposition,
                    expected.Camera.YPosition, expected.Camera.YSubposition,
                    expected.Camera.IdealXPosition, expected.Camera.IdealYPosition),
                (actual.Camera.XPosition, actual.Camera.XSubposition,
                    actual.Camera.YPosition, actual.Camera.YSubposition,
                    actual.Camera.IdealXPosition, actual.Camera.IdealYPosition),
                $"guarded post-Ceres camera trajectory frame {frames}");
            AssertEqual(
                (expected.Samus!.Kinematics.XFixed, expected.Samus.Kinematics.YFixed),
                (actual.Samus!.Kinematics.XFixed, actual.Samus.Kinematics.YFixed),
                $"guarded post-Ceres carried-Samus trajectory frame {frames}");
            AssertEqual(expected.Enemies.LastGunshipEvent,
                actual.Enemies.LastGunshipEvent,
                $"guarded post-Ceres gunship boundary frame {frames}");
            cameraMoved |= actual.Camera.YPosition != initialCameraY;
            frames++;
        }

        AssertTrue(cameraMoved,
            "post-Ceres production trajectory moves the camera through Landing Site");
        AssertEqual(GunshipFrameEvent.LandingCompleted,
            actual.Enemies.LastGunshipEvent,
            "guarded post-Ceres production trajectory reaches landing completion");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production entry and camera trajectory perform no compiled-definition ROM reads");

        Console.WriteLine(
            $"Room-definition integration: {forbidden.Count} native definition bytes " +
            $"remain unread through production entry and {frames} Landing Site camera frames.");
    }

    private static InitialViewportResult InitializePostCeresLanding(
        SuperMetroidRuntime runtime)
    {
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        return runtime.InitializePostCeresZebesRoom();
    }

    private static HashSet<int> BuildCompiledRoomDefinitionAddressSet(
        ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        AddAddressRange(forbidden, LoadStationRomData.PointerTable,
            LoadStationRomData.DataEnd - LoadStationRomData.PointerTable);

        foreach (ushort doorPointer in EnumerateRetailDoorPointers()
                     .Append(DoorHeaderRomData.ElevatorPseudoDoorPointer)
                     .Distinct())
        {
            AddAddressRange(forbidden,
                DoorHeaderRomData.BankAddress | doorPointer,
                DoorHeaderRomData.RecordByteCount);
            CartridgeDoorHeader door = DoorDefinitions.Get(doorPointer);
            if (door.SetupCodePointer != 0)
            {
                forbidden.Add(RoomHeaderRomData.BankAddress |
                    door.SetupCodePointer);
            }
        }

        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .Order()
            .ToArray();
        var statePointers = new HashSet<ushort>();
        var tracingBus = new DefinitionAddressCollectingBus(source, forbidden);
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                statePointers.Add(CartridgeRoomHeader.Load(
                    tracingBus, roomPointer, context).State.Pointer);
            }

            RoomHeaderDefinition header = RoomHeaderDefinitions.Get(roomPointer);
            DoorListDefinition doors = DoorDefinitions.GetList(header.DoorListPointer);
            AddAddressRange(forbidden,
                RoomHeaderRomData.BankAddress | header.DoorListPointer,
                doors.DoorPointers.Length * sizeof(ushort));
        }

        foreach (ushort statePointer in statePointers)
        {
            CartridgeRoomState state = RoomStateDefinitions.Get(statePointer);
            if (unchecked((short)state.ScrollPointer) < 0)
            {
                AddAddressRange(forbidden,
                    RoomAssetRomData.Tilesets.DefinitionBank | state.ScrollPointer,
                    RoomScrollGrid.StorageByteCount);
            }
        }

        foreach (RoomMainCallback callback in Enum.GetValues<RoomMainCallback>())
        {
            ushort pointer = RoomCallbackDefinitions.PointerOf(callback);
            if (pointer != 0)
                forbidden.Add(RoomHeaderRomData.BankAddress | pointer);
        }
        foreach (RoomSetupCallback callback in Enum.GetValues<RoomSetupCallback>())
        {
            ushort pointer = RoomCallbackDefinitions.PointerOf(callback);
            if (pointer != 0)
                forbidden.Add(RoomHeaderRomData.BankAddress | pointer);
        }

        AssertEqual(RoomStateDefinitions.RetailStateCount, statePointers.Count,
            "combined definition guard reaches every retail room state");
        return forbidden;
    }

    private static void AddAddressRange(HashSet<int> addresses, int start, int count)
    {
        for (int offset = 0; offset < count; offset++)
            addresses.Add(start + offset);
    }

    private sealed class DefinitionAddressCollectingBus(
        ISnesAddressSpace source,
        HashSet<int> addresses) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            addresses.Add(address);
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);
    }

    private sealed class CompiledRoomDefinitionReadGuard(
        ISnesAddressSpace source,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production runtime read compiled room-definition byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);
    }
}
