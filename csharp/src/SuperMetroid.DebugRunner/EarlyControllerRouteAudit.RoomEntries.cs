using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class EarlyControllerRouteAudit
{
    /// <summary>
    /// Reproduces the two repeatedly reported vertical-entry defects without traversing
    /// unrelated rooms: Morph Ball-to-elevator attachment/camera and Climb-to-Parlor's
    /// south-door floor clearance. Both use real bank-$83 headers and the production room
    /// loader; only the walk to each already-known doorway is replaced by staging.
    /// </summary>
    public static int RunVerticalRoomEntryAudit(string romPath, string? captureDirectory = null)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SuperMetroidRuntime runtime = CreateInitializedRuntime(bus);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Vertical-entry audit initialized without Samus.");

        samus.CollectedItems |= (ushort)SamusEquipmentFlags.MorphBall;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.MorphBall;
        samus.MaxMissiles = 5;
        samus.Missiles = 5;
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom);
        // The preceding downward journey leaves the global elevator status at one while
        // Samus explores Blue Brinstar. This persistent word is the entire native handoff
        // that tells the destination actor this is the return trip rather than a fresh room.
        runtime.ElevatorStatus = (ushort)ElevatorActorStatus.Departing;
        PublishRetailDoor(runtime, bus, DoorPointers.BlueBrinstarElevatorFromMorphBall);
        runtime.LoadPendingDoorDestination();
        AssertRoom(
            runtime,
            RoomHeaderPointers.BlueBrinstarElevatorRoom,
            RoomStatePointers.BlueBrinstarElevatorAfterItems,
            "focused upward elevator entry");
        AssertAscendingElevatorArrivalStartsAttached(runtime);
        if (captureDirectory is not null)
        {
            Directory.CreateDirectory(captureDirectory);
            PngWriter.WriteRgba(
                Path.Combine(captureDirectory, "issue-52-first-loaded.png"),
                256,
                224,
                SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }

        // The upward elevator actor owns movement after the door load. Wait only for its
        // own native return-to-rest handoff; walking to the Pit door would give an incorrect
        // camera an unrelated extra screen of travel in which to hide the reported offset.
        int elevatorFrames = 0;
        while (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive &&
               elevatorFrames < 1200)
        {
            runtime.StepFrame(0);
            elevatorFrames++;
        }
        if (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive)
            throw new InvalidDataException("Focused upward elevator never completed its return.");
        AssertAscendingElevatorCameraIsSynchronized(runtime);
        AssertElevatorPlatformSurvivesGameplayCompositor(runtime);
        if (captureDirectory is not null)
        {
            PngWriter.WriteRgba(
                Path.Combine(captureDirectory, "issue-52-settled.png"),
                256,
                224,
                SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }

        // The direct loader above isolates enemy/camera mechanics, but issue 52 was reported
        // after the playable state's complete vertical-door coroutine. Re-run the same bank-
        // $83 route through SuperMetroidGame so destination publication, the first visible
        // frame, the returning actor, and the final settled frame are observed together.
        ElevatorFrontendCapture frontendCapture = ReproduceAscendingElevatorThroughFrontend(
            bus,
            captureDirectory);
        ScrollBoundaryCamera directCamera = runtime.Camera
            ?? throw new InvalidOperationException("Focused elevator audit lost its camera.");
        if (frontendCapture.CameraY != directCamera.YPosition ||
            frontendCapture.SamusY != samus.YPosition)
        {
            throw new InvalidDataException(
                $"Playable elevator arrival diverged from the isolated cartridge path: " +
                $"frontend Samus/camera=${frontendCapture.SamusY:X4}/${frontendCapture.CameraY:X4}, " +
                $"direct=${samus.YPosition:X4}/${directCamera.YPosition:X4}.");
        }

        runtime.System.SetEvent((int)EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb);
        CartridgeDoorHeader parlorDoor = CartridgeDoorHeader.Load(
            bus,
            DoorPointers.ParlorFromClimb);
        // Vertical transitions retain the source X low byte. Stage it at the authored
        // destination door center so the focused transition has the same lane as gameplay.
        samus.Kinematics.SetXFixed(unchecked((uint)((parlorDoor.PlmX * 16 + 8) << 16)));
        samus.Kinematics.SetYFixed(0x0018_0000);
        PublishRetailDoor(runtime, bus, DoorPointers.ParlorFromClimb);
        runtime.LoadPendingDoorDestination();
        AssertRoom(
            runtime,
            RoomHeaderPointers.ParlorAndAlcatraz,
            RoomStatePointers.AwakenedParlor,
            "focused south Parlor entry");
        var directHost = new ControllerRouteHost(
            StepFrame: input => runtime.StepFrame(input),
            LoadPendingDoor: () => runtime.LoadPendingDoorDestination());
        AssertSouthParlorEntryAcceptsHorizontalMovement(runtime, directHost);

        Console.WriteLine(
            $"Vertical room-entry audit passed: upward elevator attached and synchronized " +
            $"in {elevatorFrames} frames; south Parlor entry accepted right-only movement.");
        return 0;
    }

    /// <summary>
    /// Drives the production outer dispatcher over the Morph Ball-room elevator door and
    /// returns the actual settled world/camera words. No unrelated room boundary is crossed;
    /// the source room is staged only to avoid replaying Ceres and the whole outbound route.
    /// </summary>
    private static ElevatorFrontendCapture ReproduceAscendingElevatorThroughFrontend(
        SuperMetroidAddressSpace bus,
        string? captureDirectory)
    {
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame frame = FrontendAuditDriver.EnterSelectedSlot(game);
        var acknowledgements = new byte[4];

        FrontendFrame StepAndAcknowledge(ushort input)
        {
            FrontendFrame next = game.Step(input);
            foreach (CartridgeAudioCommand command in next.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort)
                    acknowledgements[command.Port] = command.Value;
            }
            game.SetAudioAcknowledgements(new CartridgeAudioAcknowledgements(
                acknowledgements[0],
                acknowledgements[1],
                acknowledgements[2],
                acknowledgements[3]));
            return next;
        }

        for (int setupFrame = 0;
             setupFrame < 300 && frame.GameState != SuperMetroidGameState.MainGameplay;
             setupFrame++)
        {
            frame = StepAndAcknowledge(0);
        }
        if (frame.GameState != SuperMetroidGameState.MainGameplay)
        {
            throw new InvalidDataException(
                $"Elevator frontend setup stopped in {frame.GameState}.");
        }

        SuperMetroidRuntime runtime = game.RuntimeForVerification
            ?? throw new InvalidOperationException("Elevator frontend setup lost its runtime.");
        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException("Elevator frontend setup lost Samus.");
        samus.CollectedItems |= (ushort)SamusEquipmentFlags.MorphBall;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.MorphBall;
        samus.MaxMissiles = 5;
        samus.Missiles = 5;
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom);
        runtime.ElevatorStatus = (ushort)ElevatorActorStatus.Departing;
        PublishRetailDoor(runtime, bus, DoorPointers.BlueBrinstarElevatorFromMorphBall);

        ushort sourceRoom = runtime.ActiveRoom?.Pointer
            ?? throw new InvalidOperationException("Elevator frontend has no source room.");
        frame = StepAndAcknowledge(0);
        if (game.GameState != SuperMetroidGameState.HitDoorBlock)
        {
            throw new InvalidDataException(
                $"Elevator collision did not enter the production door state; got {game.GameState}.");
        }

        bool observedDestinationPublication = false;
        bool observedSourceRowRepair = false;
        bool observedSuppressedInitialFill = false;
        int destinationRowStreamFrames = 0;
        for (int doorFrame = 0;
             doorFrame < 512 && game.GameState != SuperMetroidGameState.MainGameplay;
             doorFrame++)
        {
            DoorTransitionPhase phaseBeforeStep = game.DoorTransitionPhaseForVerification;
            frame = StepAndAcknowledge(0);
            DoorTransitionPhase phaseAfterStep = game.DoorTransitionPhaseForVerification;
            if (phaseBeforeStep == DoorTransitionPhase.FixDoorsMovingUp)
                observedSourceRowRepair = runtime.LastBackgroundUpdateCount > 0;
            if (phaseBeforeStep == DoorTransitionPhase.LoadMoreThingsAndOpenDoor &&
                phaseAfterStep == DoorTransitionPhase.WaitForDoorOpeningScroll)
            {
                observedSuppressedInitialFill = runtime.LastBackgroundUpdateCount == 0;
            }
            if (phaseBeforeStep == DoorTransitionPhase.WaitForDoorOpeningScroll &&
                runtime.LastBackgroundUpdateCount > 0)
            {
                destinationRowStreamFrames++;
            }
            if (!observedDestinationPublication && runtime.ActiveRoom?.Pointer != sourceRoom)
            {
                observedDestinationPublication = true;
            }
        }
        if (game.GameState != SuperMetroidGameState.MainGameplay ||
            !observedDestinationPublication)
        {
            throw new InvalidDataException(
                $"Elevator frontend transition did not publish and enter the destination; " +
                $"state={game.GameState}, published={observedDestinationPublication}.");
        }
        if (!observedSourceRowRepair || !observedSuppressedInitialFill ||
            destinationRowStreamFrames < 12)
        {
            throw new InvalidDataException(
                $"Upward door omitted its cartridge tilemap-ring work: source repair=" +
                $"{observedSourceRowRepair}, initial fill suppressed=" +
                $"{observedSuppressedInitialFill}, destination row frames=" +
                $"{destinationRowStreamFrames}.");
        }
        frame = StepAndAcknowledge(0);
        if (captureDirectory is not null)
        {
            PngWriter.WriteRgba(
                Path.Combine(captureDirectory, "issue-52-frontend-first-visible.png"),
                FrontendFrame.Width,
                FrontendFrame.Height,
                frame.Pixels);
        }
        AssertRoom(
            runtime,
            RoomHeaderPointers.BlueBrinstarElevatorRoom,
            RoomStatePointers.BlueBrinstarElevatorAfterItems,
            "frontend upward elevator entry");
        AssertAscendingElevatorArrivalStartsAttached(runtime);

        int elevatorFrames = 0;
        while (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive &&
               elevatorFrames < 1200)
        {
            frame = StepAndAcknowledge(0);
            elevatorFrames++;
        }
        if (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive)
            throw new InvalidDataException("Frontend upward elevator never completed its return.");
        AssertAscendingElevatorCameraIsSynchronized(runtime);
        AssertElevatorPlatformSurvivesGameplayCompositor(runtime);
        frame = StepAndAcknowledge(0);
        if (captureDirectory is not null)
        {
            PngWriter.WriteRgba(
                Path.Combine(captureDirectory, "issue-52-frontend-settled.png"),
                FrontendFrame.Width,
                FrontendFrame.Height,
                frame.Pixels);
        }

        ScrollBoundaryCamera camera = runtime.Camera
            ?? throw new InvalidOperationException("Frontend elevator arrival lost its camera.");
        return new ElevatorFrontendCapture(
            samus.YPosition,
            camera.YPosition,
            camera.IdealYPosition,
            elevatorFrames);
    }

    /// <summary>
    /// Reproduces issue 57 at both cartridge-authored Parlor scroll blocks. A ball-to-body
    /// expansion straddles each special-air trigger so the prospective vertical probe must
    /// wake the resident <c>$B703</c> actor through the same collision dispatcher as play.
    /// </summary>
    public static int RunParlorScrollPoseProbeAudit(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach ((int blockIndex, int blockX, int blockY) in new[]
                 {
                     (1062, 0x16, 0x0d),
                     (1172, 0x34, 0x0e),
                 })
        {
            SuperMetroidRuntime runtime = CreateInitializedRuntime(bus);
            SamusState samus = runtime.Samus ?? throw new InvalidDataException(
                "Parlor scroll-pose audit initialized without Samus.");
            samus.CollectedItems |= (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.MorphBall;
            samus.MaxMissiles = 5;
            samus.Missiles = 5;
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ParlorAndAlcatraz);
            AssertRoom(
                runtime,
                RoomHeaderPointers.ParlorAndAlcatraz,
                RoomStatePointers.DefaultParlor,
                $"Parlor scroll block {blockIndex}");
            if (!runtime.Plms.ScrollPlms.Any(scroll => scroll.BlockIndex == blockIndex))
            {
                throw new InvalidDataException(
                    $"Parlor population did not retain the $B703 owner for block {blockIndex}.");
            }

            samus.Pose = SamusState.MorphBallGroundRightPose;
            samus.Kinematics.SetXFixed(unchecked((uint)((blockX * 16 + 8) << 16)));
            // Put the upward intermediate changed-pose scan exactly on the reported trigger.
            // Morph Ball has radius seven and the native first pass moves eight pixels, so a
            // centre 15 pixels below the trigger row makes its leading boundary enter that row.
            samus.Kinematics.SetYFixed(unchecked((uint)((blockY * 16 + 15) << 16)));
            samus.RefreshCollisionRadii(bus);
            if (!samus.TryApplyMorphTransition(
                    bus,
                    runtime.LevelData ?? throw new InvalidOperationException(
                        "Parlor scroll-pose audit has no level data."),
                    SamusState.UnmorphingTransitionRightPose,
                    runtime.NmiFrameCounter,
                    runtime.Plms))
            {
                throw new InvalidDataException(
                    $"Parlor scroll block {blockIndex} rejected an unobstructed unmorph probe.");
            }
            if (!runtime.Plms.ScrollPlms.Single(scroll => scroll.BlockIndex == blockIndex).Triggered)
            {
                throw new InvalidDataException(
                    $"Parlor pose-change collision did not notify $B703 owner {blockIndex}.");
            }
        }

        Console.WriteLine(
            "Parlor scroll-pose audit passed for native $B703 blocks 1062 and 1172.");
        return 0;
    }

    private static SuperMetroidRuntime CreateInitializedRuntime(ISnesAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        return runtime;
    }

    private readonly record struct ElevatorFrontendCapture(
        ushort SamusY,
        ushort CameraY,
        ushort IdealCameraY,
        int ElevatorFrames);
}
