using static SuperMetroid.Verification.Areas.Ceres.PostCeresLandingHandOffVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled room definitions through the production post-Ceres landing path.</summary>
internal static class RoomDefinitionIntegrationVerification
{
    /// <summary>Bound on the Landing Site trajectory needed to observe gunship landing completion.</summary>
    internal const int PostCeresLandingMaximumFrames = 700;

    /// <summary>Compares guarded and cartridge-backed room entry, camera, Samus, and gunship updates until landing completes.</summary>
    internal static void VerifyCompiledRoomDefinitionIntegration()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace oracleBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        HashSet<int> forbidden = BuildCompiledRoomDefinitionAddressSet(oracleBus);
        var guard = NewCompiledRoomDefinitionReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath), forbidden);

        var expected = CreateRetailRuntimeFixture(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath));
        var actual = CreateRetailRuntimeFixture(guard);
        InitialViewportResult expectedViewport = InitializePostCeresLanding(expected);
        InitialViewportResult actualViewport = InitializePostCeresLanding(actual);

        AssertEqual(expectedViewport, actualViewport,
            "guarded post-Ceres room entry retains initial viewport work");
        AssertEqual(expected.Room.ActiveLoadStation, actual.Room.ActiveLoadStation,
            "guarded post-Ceres room entry retains compiled station placement");
        AssertEqual(expected.Room.ActiveDoor, actual.Room.ActiveDoor,
            "guarded post-Ceres room entry retains compiled incoming door");
        AssertEqual(expected.Room.ActiveRoom, actual.Room.ActiveRoom,
            "guarded post-Ceres room entry retains compiled header, selection, and state");
        AssertTrue(expected.Room.LoadedCamera.Scrolls.Storage.SequenceEqual(
                actual.Room.LoadedCamera.Scrolls.Storage),
            "guarded post-Ceres room entry retains compiled scroll constraints");
        AssertEqual((expected.Room.LoadedCamera.PixelPosition.X, expected.Room.LoadedCamera.PixelPosition.Y),
            (actual.Room.LoadedCamera.PixelPosition.X, actual.Room.LoadedCamera.PixelPosition.Y),
            "guarded post-Ceres room entry retains camera placement");
        AssertEqual((expected.Player.IntroducedSamus.Kinematics.Position.X.RawValue, expected.Player.IntroducedSamus.Kinematics.Position.Y.RawValue),
            (actual.Player.IntroducedSamus.Kinematics.Position.X.RawValue, actual.Player.IntroducedSamus.Kinematics.Position.Y.RawValue),
            "guarded post-Ceres room entry retains Samus placement");

        ushort initialCameraY = actual.Room.LoadedCamera.PixelPosition.Y;
        int frames = 0;
        bool cameraMoved = false;
        while (actual.Enemies.LastEnemyFrame.Gunship != GunshipFrameEvent.LandingCompleted &&
               frames < PostCeresLandingMaximumFrames)
        {
            RuntimeFrameResult expectedFrame = expected.StepFrame(0);
            RuntimeFrameResult actualFrame = actual.StepFrame(0);
            AssertEqual(expectedFrame, actualFrame,
                $"guarded post-Ceres frame result {frames}");
            AssertEqual(
                (expected.Room.LoadedCamera.PixelPosition.X, expected.Room.LoadedCamera.Position.X.Fraction,
                    expected.Room.LoadedCamera.PixelPosition.Y, expected.Room.LoadedCamera.Position.Y.Fraction,
                    expected.Room.LoadedCamera.IdealPosition.X, expected.Room.LoadedCamera.IdealPosition.Y),
                (actual.Room.LoadedCamera.PixelPosition.X, actual.Room.LoadedCamera.Position.X.Fraction,
                    actual.Room.LoadedCamera.PixelPosition.Y, actual.Room.LoadedCamera.Position.Y.Fraction,
                    actual.Room.LoadedCamera.IdealPosition.X, actual.Room.LoadedCamera.IdealPosition.Y),
                $"guarded post-Ceres camera trajectory frame {frames}");
            AssertEqual(
                (expected.Player.IntroducedSamus.Kinematics.Position.X.RawValue, expected.Player.IntroducedSamus.Kinematics.Position.Y.RawValue),
                (actual.Player.IntroducedSamus.Kinematics.Position.X.RawValue, actual.Player.IntroducedSamus.Kinematics.Position.Y.RawValue),
                $"guarded post-Ceres carried-Samus trajectory frame {frames}");
            AssertEqual(expected.Enemies.LastEnemyFrame.Gunship,
                actual.Enemies.LastEnemyFrame.Gunship,
                $"guarded post-Ceres gunship boundary frame {frames}");
            cameraMoved |= actual.Room.LoadedCamera.PixelPosition.Y != initialCameraY;
            frames++;
        }

        AssertTrue(cameraMoved,
            "post-Ceres production trajectory moves the camera through Landing Site");
        AssertEqual(GunshipFrameEvent.LandingCompleted,
            actual.Enemies.LastEnemyFrame.Gunship,
            "guarded post-Ceres production trajectory reaches landing completion");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production entry and camera trajectory perform no compiled-definition ROM reads");

        Console.WriteLine(
            $"Room-definition integration: {forbidden.Count} native definition bytes " +
            $"remain unread through production entry and {frames} Landing Site camera frames.");
    }

    /// <summary>Starts the Ceres handoff and enters Landing Site with the timebomb event set.</summary>
    private static InitialViewportResult InitializePostCeresLanding(
        SuperMetroidRuntime runtime)
    {
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        return runtime.RoomLoader.InitializePostCeresZebesRoom();
    }

    /// <summary>Collects native bytes owned by compiled load stations, doors, room headers/states, scroll grids, and callbacks.</summary>
    private static HashSet<int> BuildCompiledRoomDefinitionAddressSet(
        ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        AddAddressRange(forbidden, LoadStationRomData.PointerTable,
            LoadStationRomData.DataEnd - LoadStationRomData.PointerTable);

        foreach (ushort doorPointer in EnumerateRetailDoorPointers()
                     .Append((ushort)ElevatorPseudoDoorPointer.Shared)
                     .Distinct())
        {
            AddAddressRange(forbidden,
                DoorHeaderRomDataTooling.BankAddress | doorPointer,
                DoorHeaderRomDataConstants.RecordByteCount);
            // The pseudo-door's remaining bytes overlap $88FE; it has no setup code of its own.
            if (doorPointer == (ushort)ElevatorPseudoDoorPointer.Shared)
                continue;
            CartridgeDoorHeader door = DoorDefinitions.Get(doorPointer);
            if (door.SetupCodePointer != DoorSetupCode.None)
            {
                forbidden.Add(RoomHeaderRomData.BankAddress |
                    (ushort)door.SetupCodePointer);
            }
        }

        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .Order()
            .ToArray();
        var statePointers = new HashSet<ushort>();
        var tracingBus = NewDefinitionAddressCollectingBus(source, forbidden);
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                statePointers.Add(SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(
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

    /// <summary>Adds every byte in a native definition range to the forbidden-read set.</summary>
    private static void AddAddressRange(HashSet<int> addresses, int start, int count)
    {
        for (int offset = 0; offset < count; offset++)
            addresses.Add(start + offset);
    }

    /// <summary>Observes importer reads to capture exact room-header and state-definition byte ownership.</summary>
    private static CartridgeReadGuard NewDefinitionAddressCollectingBus(ISnesAddressSpace source, HashSet<int> addresses) =>
        new CartridgeReadGuard(source).Observe(_ => true, address => addresses.Add(address));

    /// <summary>Rejects production access to any cartridge byte claimed by compiled room definitions.</summary>
    private static MutableCartridgeReadGuard NewCompiledRoomDefinitionReadGuard(ISnesAddressSpace source, HashSet<int> forbidden)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains,
            address => $"Production runtime read compiled room-definition byte ${address:X6}.");
        return guard;
    }

    /// <summary>Runs room-definition integration and the post-Ceres landing handoff check.</summary>
    [VerificationCommand("--room-definition-integration")]
    internal static int RoomDefinitionIntegrationCommand()
    {
        Suite(nameof(VerifyCompiledRoomDefinitionIntegration), () => VerifyCompiledRoomDefinitionIntegration());
        Suite(nameof(VerifyPostCeresLandingHandOff), () => VerifyPostCeresLandingHandOff());
        return 0;
    }
}
