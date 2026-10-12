using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static PostCeresLandingHandOffVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static DoorDefinitionsVerification;
using static RetailRuntimeFixtureVerification;
using static RoomMainCodeCatalogVerification;

/// <summary>Represents room definition integration verification.</summary>
internal static class RoomDefinitionIntegrationVerification
{
    /// <summary>Stores post ceres landing maximum frames.</summary>
    internal const int PostCeresLandingMaximumFrames = 700;

    /// <summary>Verifies compiled room definition integration.</summary>
    internal static void VerifyCompiledRoomDefinitionIntegration()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace oracleBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        HashSet<int> forbidden = BuildCompiledRoomDefinitionAddressSet(oracleBus);
        var guard = NewCompiledRoomDefinitionReadGuard(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath), forbidden);

        var expected = CreateRetailRuntimeFixture(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath));
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
        AssertTrue(expected.Room.Camera!.Scrolls.Storage.SequenceEqual(
                actual.Room.Camera!.Scrolls.Storage),
            "guarded post-Ceres room entry retains compiled scroll constraints");
        AssertEqual((expected.Room.Camera.PixelPosition.X, expected.Room.Camera.PixelPosition.Y),
            (actual.Room.Camera.PixelPosition.X, actual.Room.Camera.PixelPosition.Y),
            "guarded post-Ceres room entry retains camera placement");
        AssertEqual((expected.Player.Samus!.Kinematics.Position.X.RawValue, expected.Player.Samus.Kinematics.Position.Y.RawValue),
            (actual.Player.Samus!.Kinematics.Position.X.RawValue, actual.Player.Samus.Kinematics.Position.Y.RawValue),
            "guarded post-Ceres room entry retains Samus placement");

        ushort initialCameraY = actual.Room.Camera.PixelPosition.Y;
        int frames = 0;
        bool cameraMoved = false;
        while (actual.Enemies.GunshipBehavior.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               frames < PostCeresLandingMaximumFrames)
        {
            RuntimeFrameResult expectedFrame = expected.StepFrame(0);
            RuntimeFrameResult actualFrame = actual.StepFrame(0);
            AssertEqual(expectedFrame, actualFrame,
                $"guarded post-Ceres frame result {frames}");
            AssertEqual(
                (expected.Room.Camera.PixelPosition.X, expected.Room.Camera.Position.X.Fraction,
                    expected.Room.Camera.PixelPosition.Y, expected.Room.Camera.Position.Y.Fraction,
                    expected.Room.Camera.IdealPosition.X, expected.Room.Camera.IdealPosition.Y),
                (actual.Room.Camera.PixelPosition.X, actual.Room.Camera.Position.X.Fraction,
                    actual.Room.Camera.PixelPosition.Y, actual.Room.Camera.Position.Y.Fraction,
                    actual.Room.Camera.IdealPosition.X, actual.Room.Camera.IdealPosition.Y),
                $"guarded post-Ceres camera trajectory frame {frames}");
            AssertEqual(
                (expected.Player.Samus!.Kinematics.Position.X.RawValue, expected.Player.Samus.Kinematics.Position.Y.RawValue),
                (actual.Player.Samus!.Kinematics.Position.X.RawValue, actual.Player.Samus.Kinematics.Position.Y.RawValue),
                $"guarded post-Ceres carried-Samus trajectory frame {frames}");
            AssertEqual(expected.Enemies.GunshipBehavior.LastGunshipEvent,
                actual.Enemies.GunshipBehavior.LastGunshipEvent,
                $"guarded post-Ceres gunship boundary frame {frames}");
            cameraMoved |= actual.Room.Camera.PixelPosition.Y != initialCameraY;
            frames++;
        }

        AssertTrue(cameraMoved,
            "post-Ceres production trajectory moves the camera through Landing Site");
        AssertEqual(GunshipFrameEvent.LandingCompleted,
            actual.Enemies.GunshipBehavior.LastGunshipEvent,
            "guarded post-Ceres production trajectory reaches landing completion");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production entry and camera trajectory perform no compiled-definition ROM reads");

        Console.WriteLine(
            $"Room-definition integration: {forbidden.Count} native definition bytes " +
            $"remain unread through production entry and {frames} Landing Site camera frames.");
    }

    /// <summary>Initializes post ceres landing.</summary>
    private static InitialViewportResult InitializePostCeresLanding(
        SuperMetroidRuntime runtime)
    {
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        return runtime.RoomLoader.InitializePostCeresZebesRoom();
    }

    /// <summary>Builds compiled room definition address set.</summary>
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
            .Select(pointer => pointer!.Value)
            .Distinct()
            .Order()
            .ToArray();
        var statePointers = new HashSet<ushort>();
        var tracingBus = NewDefinitionAddressCollectingBus(source, forbidden);
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

    /// <summary>Adds address range.</summary>
    private static void AddAddressRange(HashSet<int> addresses, int start, int count)
    {
        for (int offset = 0; offset < count; offset++)
            addresses.Add(start + offset);
    }

    /// <summary>Creates definition address collecting bus.</summary>
    private static CartridgeReadGuard NewDefinitionAddressCollectingBus(ISnesAddressSpace source, HashSet<int> addresses) =>
        new CartridgeReadGuard(source).Observe(_ => true, address => addresses.Add(address));

    /// <summary>Creates compiled room definition read guard.</summary>
    private static MutableCartridgeReadGuard NewCompiledRoomDefinitionReadGuard(ISnesAddressSpace source, HashSet<int> forbidden)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains,
            address => $"Production runtime read compiled room-definition byte ${address:X6}.");
        return guard;
    }

    /// <summary>Room definition integration command.</summary>
    [VerificationCommand("--room-definition-integration")]
    internal static int RoomDefinitionIntegrationCommand()
    {
        Suite(nameof(VerifyCompiledRoomDefinitionIntegration), () => VerifyCompiledRoomDefinitionIntegration());
        Suite(nameof(VerifyPostCeresLandingHandOff), () => VerifyPostCeresLandingHandOff());
        return 0;
    }
}
