using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Private-ROM smoke test for the exact new-game door graph from Landing Site to Bomb Torizo.
/// </summary>
/// <remarks>
/// This deliberately consumes each room's bank-$8F door list through the same type-$9
/// collision resolver used by live Samus. It is not a substitute for controller traversal;
/// it is the narrow diagnostic underneath that eventual playthrough, ensuring every room
/// can be constructed through its real incoming bank-$83 record before path-finding noise is
/// introduced. The sequence and expected pointers below are transcribed from the retail door
/// tables, while the values actually loaded are always read back from the user's cartridge.
/// </remarks>
internal static class EarlyRouteAudit
{
    private const ushort LandingSiteRoom = 0x91f8;
    private const ushort LandingSiteDefaultState = 0x9213;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);

        // Reuse the real post-Ceres station loader to obtain a fresh Samus and Landing Site
        // state. The audit skips only the already-verified gunship clock; it restores the
        // ordinary standing endpoint that GunshipTop_7 publishes when landing completes.
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Post-Ceres Landing Site did not retain Samus.");
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);

        AssertRoom(runtime, LandingSiteRoom, LandingSiteDefaultState, "Landing Site");

        // Outbound new-game descent: ship -> Parlor -> Climb -> Pit -> Crateria elevator
        // room -> the long Blue Brinstar Morph Ball room.
        Traverse(runtime, bus, 0, 0x8916, 0x92fd, 0x9314, "Parlor");
        Traverse(runtime, bus, 4, 0x898e, 0x96ba, 0x96d1, "Climb");
        Traverse(runtime, bus, 3, 0x8b62, 0x975c, 0x976d, "Pit Room");
        Traverse(runtime, bus, 1, 0x8b86, 0x97b5, 0x97c6, "Blue Brinstar elevator top");
        Traverse(runtime, bus, 1, 0x8b9e, 0x9e9f, 0x9eb1, "Morph Ball Room");

        // Return to Crateria and take Parlor's lower-right branch into the Flyway and the
        // one-door Bomb Torizo arena. The Morph Ball pickup itself remains the next live
        // controller/PLM milestone, so the cartridge correctly continues selecting all
        // pre-awakening room states here.
        Traverse(runtime, bus, 2, 0x8eb6, 0x97b5, 0x97c6, "Blue Brinstar elevator top (return)");
        Traverse(runtime, bus, 0, 0x8b92, 0x975c, 0x976d, "Pit Room (return)");
        Traverse(runtime, bus, 0, 0x8b7a, 0x96ba, 0x96d1, "Climb (return)");
        Traverse(runtime, bus, 0, 0x8b3e, 0x92fd, 0x9314, "Parlor (return)");
        Traverse(runtime, bus, 3, 0x8982, 0x9879, 0x9890, "Flyway");
        Traverse(runtime, bus, 1, 0x8bc2, 0x9804, 0x981b, "Bomb Torizo");

        // Prove that subsequent loads consume live progression rather than silently
        // returning to each header's default branch. These writes model facts produced by
        // the encounter/item systems; the room choice itself remains the retail selector.
        runtime.System.SetBossBits(areaIndex: AreaId.Crateria, BossBits.AreaTorizo);
        Traverse(runtime, bus, 0, 0x8baa, 0x9879, 0x98aa, "defeated Bomb Torizo Flyway");
        runtime.System.SetEvent((int)EventNumber.ZebesAwake);
        Traverse(runtime, bus, 0, 0x8bb6, 0x92fd, 0x932e, "awakened Parlor");

        // $8F:E652 requires both the collected Morph Ball bit and nonzero missile capacity.
        // Testing only either half would incorrectly select the untouched Pit/elevator art.
        samus.CollectedItems = samus.CollectedItems.With(SamusEquipmentFlags.MorphBall);
        samus.EquippedItems = samus.EquippedItems.With(SamusEquipmentFlags.MorphBall);
        samus.MaxMissiles = 5;
        Traverse(runtime, bus, 4, 0x898e, 0x96ba, 0x96eb, "awakened Climb");
        Traverse(runtime, bus, 3, 0x8b62, 0x975c, 0x9787, "post-Morph Pit Room");
        Traverse(runtime, bus, 1, 0x8b86, 0x97b5, 0x97e0, "post-Morph elevator top");

        // Power-bomb state selection has higher priority than event zero in Landing Site.
        // Walk back through real reverse doors so this assertion covers the shared loader,
        // not a special direct-room helper.
        samus.MaxPowerBombs = 5;
        Traverse(runtime, bus, 0, 0x8b92, 0x975c, 0x9787, "post-Morph Pit Room (return)");
        Traverse(runtime, bus, 0, 0x8b7a, 0x96ba, 0x96eb, "awakened Climb (return)");
        Traverse(runtime, bus, 0, 0x8b3e, 0x92fd, 0x932e, "awakened Parlor (return)");
        Traverse(runtime, bus, 1, 0x896a, 0x91f8, 0x9247, "power-bomb Landing Site");

        VerifyLiveElevatorJourney(romPath);

        Console.WriteLine(
            "Early route audit: 20 cartridge-authored door loads covered the new-game " +
            "route and all progression selectors used by its rooms.");
        return 0;
    }

    /// <summary>
    /// Drives the ordinary bank-$A3 elevator actor across a real room-load boundary.
    /// </summary>
    private static void VerifyLiveElevatorJourney(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Elevator audit could not initialize Samus.");
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);

        Traverse(runtime, bus, 0, 0x8916, 0x92fd, 0x9314, "elevator audit Parlor");
        Traverse(runtime, bus, 4, 0x898e, 0x96ba, 0x96d1, "elevator audit Climb");
        Traverse(runtime, bus, 3, 0x8b62, 0x975c, 0x976d, "elevator audit Pit");
        Traverse(runtime, bus, 1, 0x8b86, 0x97b5, 0x97c6, "elevator audit top");

        ElevatorEnemyState elevator = runtime.Enemies.ElevatorStates
            .FirstOrDefault(state => state is not null)
            ?? throw new InvalidDataException("Crateria elevator room did not load elevator $D73F.");
        RoomEnemySlot elevatorSlot = runtime.Enemies.Slots
            .First(slot => slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);

        // The focused actor audit begins from the exact resting carrier coordinates. The
        // full route harness will reach this point with ordinary controller motion; this
        // placement isolates the cross-room elevator contract while that navigation layer
        // is still under construction.
        samus.XPosition = elevatorSlot.XPosition;
        samus.YPosition = unchecked((ushort)(elevatorSlot.YPosition - 26));
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        _ = elevator;

        // Door-list entry two is the retail pseudo-door word $0000. Its collision sets
        // elevator_properties without scheduling an ordinary room transition; a fresh Down
        // edge on the following enemy frame is what starts bank-$A3's actor.
        CartridgeDoorHeader pseudoDoor = runtime.LevelData!.ResolveDoorCollision(
            bus,
            behavior: 2,
            samus.Pose);
        if (pseudoDoor.Pointer != 0x88fc || runtime.HasPendingDoorTransition)
            throw new InvalidDataException("Elevator pseudo-door was treated as an ordinary exit.");

        runtime.StepFrame((ushort)SnesButton.Down);
        if (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Departing ||
            runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.DepartureStarted)
        {
            throw new InvalidDataException(
                "Down input did not start the cartridge elevator departure.");
        }

        int departureFrames = 0;
        while (!runtime.HasPendingDoorTransition && departureFrames < 240)
        {
            runtime.StepFrame(0);
            departureFrames++;
        }
        if (!runtime.HasPendingDoorTransition)
        {
            throw new InvalidDataException(
                $"Departing elevator did not reach its real destination door in " +
                $"{departureFrames} frames (Samus Y=${samus.YPosition:X4}).");
        }

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x9e9f, 0x9eb1, "Morph Ball elevator arrival");
        if (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.BeginArrivalReturn)
        {
            throw new InvalidDataException(
                "Destination elevator initializer did not retain arrival status two.");
        }

        int arrivalFrames = 0;
        while (runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.ArrivalCompleted &&
               arrivalFrames < 520)
        {
            runtime.StepFrame(0);
            arrivalFrames++;
        }
        if (runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.ArrivalCompleted ||
            runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive ||
            samus.InputLocked)
        {
            RoomEnemySlot arrivalSlot = runtime.Enemies.Slots.First(
                slot => slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
            throw new InvalidDataException(
                $"Morph Ball elevator arrival did not restore control after {arrivalFrames} frames: " +
                $"status {runtime.Enemies.ElevatorStatus}, event {runtime.Enemies.LastElevatorEvent}, " +
                $"actor Y=${arrivalSlot.YPosition:X4}.{arrivalSlot.YSubposition:X4}, " +
                $"rest=${runtime.Enemies.ElevatorStates[arrivalSlot.SlotIndex]!.RestingYPosition:X4}, " +
                $"Samus Y=${samus.YPosition:X4}, locked={samus.InputLocked}.");
        }

        Console.WriteLine(
            $"  Elevator journey: departure {departureFrames} frames, arrival " +
            $"{arrivalFrames} frames, controls restored at Samus Y=${samus.YPosition:X4}.");
    }

    private static void Traverse(
        SuperMetroidRuntime runtime,
        ISnesAddressSpace bus,
        byte doorListIndex,
        ushort expectedDoorPointer,
        ushort expectedRoomPointer,
        ushort expectedStatePointer,
        string roomName)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            $"{roomName} traversal began without active level data.");
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{roomName} traversal began without Samus.");

        // The BTS byte's low seven bits are the word index in the active room's door list.
        // Passing it to the collision owner proves the list entry rather than directly
        // constructing the expected door named beside it in this audit.
        CartridgeDoorHeader resolved = level.ResolveDoorCollision(
            bus,
            doorListIndex,
            samus.Pose);
        if (resolved.Pointer != expectedDoorPointer)
        {
            throw new InvalidDataException(
                $"Door index {doorListIndex} resolved $83:{resolved.Pointer:X4}; " +
                $"expected $83:{expectedDoorPointer:X4} before {roomName}.");
        }

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, expectedRoomPointer, expectedStatePointer, roomName);

        // Two accepted gameplay passes execute initialization-time AI/PLM work and make
        // the newly queued graphics visible. Most unsupported early-route translations
        // therefore fail at the room that owns them rather than several doors later.
        runtime.StepFrame(0);
        runtime.StepFrame(0);
    }

    private static void AssertRoom(
        SuperMetroidRuntime runtime,
        ushort expectedRoomPointer,
        ushort expectedStatePointer,
        string roomName)
    {
        CartridgeRoomHeader room = runtime.ActiveRoom ?? throw new InvalidOperationException(
            $"{roomName} did not publish an active cartridge room.");
        if (room.Pointer != expectedRoomPointer || room.State.Pointer != expectedStatePointer)
        {
            throw new InvalidDataException(
                $"{roomName} loaded room/state $8F:{room.Pointer:X4}/$8F:{room.State.Pointer:X4}; " +
                $"expected $8F:{expectedRoomPointer:X4}/$8F:{expectedStatePointer:X4}.");
        }
    }
}
