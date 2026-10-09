using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Maximum simulated frames allowed for the post-Ceres Landing Site camera trajectory.</summary>
    private const int PostCeresLandingMaximumFrames = 700;

    /// <summary>Compares production room entry and the Landing Site camera trajectory with compiled definition reads forbidden.</summary>
    private static void VerifyCompiledRoomDefinitionIntegration()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace oracleBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        HashSet<int> forbidden = BuildCompiledRoomDefinitionAddressSet(oracleBus);
        var guard = new CompiledRoomDefinitionReadGuard(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath), forbidden);

        var expected = CreateRetailRuntimeFixture(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath));
        var actual = CreateRetailRuntimeFixture(guard);
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

    /// <summary>Initializes the Ceres starting room and transitions the runtime into the post-Ceres Landing Site room.</summary>
    /// <param name="runtime">Runtime fixture whose HUD, starting room, Samus, and timebomb event are initialized.</param>
    /// <returns>The initial viewport work reported by the post-Ceres room transition.</returns>
    private static InitialViewportResult InitializePostCeresLanding(
        SuperMetroidRuntime runtime)
    {
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        return runtime.InitializePostCeresZebesRoom();
    }

    /// <summary>Collects the native address ranges owned by compiled room, door, state, and callback definitions.</summary>
    /// <param name="source">Cartridge address space used while tracing room-header imports.</param>
    /// <returns>Addresses that production room loading must not reread after definitions are compiled.</returns>
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
                DoorHeaderRomDataTooling.BankAddress | doorPointer,
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
                statePointers.Add(SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(
                    tracingBus, roomPointer, context).State.Pointer);
            }

            RoomHeaderDefinition header = RoomHeaderDefinitions.Get(roomPointer);
            DoorListDefinition doors = DoorDefinitionsTooling.GetList(header.DoorListPointer);
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

    /// <summary>Adds each byte address in a contiguous definition range to the forbidden-address set.</summary>
    /// <param name="addresses">Set receiving the range's byte addresses.</param>
    /// <param name="start">First address in the range.</param>
    /// <param name="count">Number of consecutive bytes to include.</param>
    private static void AddAddressRange(HashSet<int> addresses, int start, int count)
    {
        for (int offset = 0; offset < count; offset++)
            addresses.Add(start + offset);
    }

    /// <summary>Wraps address reads to record every byte touched while importing room definitions.</summary>
    /// <param name="source">Underlying address space supplying collected bytes.</param>
    /// <param name="addresses">Set that receives each requested address.</param>
    private sealed class DefinitionAddressCollectingBus(
        ISnesAddressSpace source,
        HashSet<int> addresses) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Records the address and forwards a regular memory read.</summary>
        /// <param name="address">Address requested from the wrapped bus.</param>
        /// <returns>The byte returned by the underlying address space.</returns>
        public byte ReadByte(int address)
        {
            addresses.Add(address);
            return source.ReadByte(address);
        }

        /// <summary>Records and forwards a cartridge-import read through the import-source interface.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte returned by the wrapped cartridge source.</returns>
        public byte ReadCartridgeByte(int address)
        {
            addresses.Add(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Definition collection requires a cartridge import source."))
                .ReadCartridgeByte(address);
        }

        /// <summary>Forwards a memory write to the wrapped address space.</summary>
        /// <param name="address">Address being updated.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);
    }

    /// <summary>Rejects runtime reads from compiled definition addresses while forwarding other memory access.</summary>
    /// <param name="source">Underlying cartridge and mutable-memory source.</param>
    /// <param name="forbidden">Addresses that must be provided by compiled room definitions.</param>
    private sealed class CompiledRoomDefinitionReadGuard(
        ISnesAddressSpace source,
        HashSet<int> forbidden) :
        ISnesAddressSpace, ISnesMutableMemory, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempts to read an address owned by compiled definitions.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Reads an address unless the runtime would reread a compiled definition byte.</summary>
        /// <param name="address">SNES address to read.</param>
        /// <returns>The underlying byte for an address outside the forbidden set.</returns>
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

        /// <summary>Forwards a write to the wrapped mutable address space.</summary>
        /// <param name="address">SNES address being updated.</param>
        /// <param name="value">Byte written at that address.</param>
        public void WriteByte(int address, byte value) =>
            source.WriteByte(address, value);

        /// <summary>Checks a cartridge import address against the guard before forwarding the import read.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The byte returned by the wrapped import source when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address)
        {
            _ = ReadByte(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Room-definition guard requires a cartridge import source."))
                .ReadCartridgeByte(address);
        }

        /// <summary>Reads a WRAM byte from the wrapped mutable-memory source.</summary>
        /// <param name="address">WRAM address to read.</param>
        /// <returns>The byte at the requested WRAM address.</returns>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Room-definition guard requires WRAM."))
            .ReadWorkRamByte(address);

        /// <summary>Reads a save-RAM byte from the wrapped mutable-memory source.</summary>
        /// <param name="address">Save-RAM address to read.</param>
        /// <returns>The byte at the requested save-RAM address.</returns>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Room-definition guard requires SRAM."))
            .ReadSaveRamByte(address);
    }
}
