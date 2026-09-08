using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class EarlyControllerRouteAudit
{
    /// <summary>
    /// Reproduces the player's exact ordinary-elevator pair between area-$00 room-$19 and
    /// area-$01 room-$00. Both directions use the real room data, type-$9 pseudo-door,
    /// bank-$83 header, state-$0B transition, destination enemy, camera, BG streaming, and
    /// gameplay compositor. Optional captures make tile-ring and OBJ alignment inspectable.
    /// </summary>
    public static int RunGreenBrinstarElevatorAudit(
        string romPath,
        string? captureDirectory = null)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SuperMetroidRuntime runtime = CreateInitializedRuntime(bus);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Green Brinstar elevator audit initialized without Samus.");
        samus.CollectedItems |= (ushort)SamusEquipmentFlags.MorphBall;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.MorphBall;
        samus.MaxMissiles = 5;
        samus.Missiles = 5;

        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GreenBrinstarElevatorRoom);
        RoomEnemySlot sourceElevator = runtime.Enemies.Slots.Single(
            slot => slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.XPosition = sourceElevator.XPosition;
        samus.YPosition = unchecked((ushort)(sourceElevator.YPosition - ElevatorActorDefinitions.SamusYOffset));
        runtime.Enemies.PublishElevatorDoorContact();
        for (int departureFrame = 0;
             runtime.PendingDoorTransition is null && departureFrame < 1200;
             departureFrame++)
        {
            ushort input = departureFrame % 30 == 0 ? (ushort)SnesButton.Down : (ushort)0;
            runtime.StepFrame(input);
            runtime.RunNmi(controller1Input: input, mainLoopRequestedNmi: true);
        }
        if (runtime.PendingDoorTransition?.Pointer != DoorPointers.GreenBrinstarMainShaftFromElevator)
            throw new InvalidDataException("Green Brinstar downward ride did not reach its retail door.");
        RunGreenBrinstarElevatorTransition(
            runtime,
            RoomHeaderPointers.GreenBrinstarMainShaft,
            RoomStatePointers.GreenBrinstarMainShaft,
            "downward",
            captureDirectory);

        int downwardReturnFrames = RunElevatorToRest(
            runtime,
            "downward Green Brinstar arrival",
            captureDirectory,
            "issue-234-235-downward");
        (int DownBg1, int DownBg2, int DownShift) = CompareWithDirectElevatorReference(
            bus,
            runtime,
            RoomHeaderPointers.GreenBrinstarMainShaft,
            captureDirectory,
            "issue-234-235-downward-reference.png");

        // Ride back to the source boundary instead of publishing a door at the resting
        // platform. The native transition retains the source PPU phase, so teleporting
        // straight into the door coroutine fabricates a different tile-ring alignment.
        runtime.Enemies.PublishElevatorDoorContact();
        for (int departureFrame = 0;
             runtime.PendingDoorTransition is null && departureFrame < 1200;
             departureFrame++)
        {
            ushort input = departureFrame % 30 == 0 ? (ushort)SnesButton.Up : (ushort)0;
            runtime.StepFrame(input);
            runtime.RunNmi(controller1Input: input, mainLoopRequestedNmi: true);
        }
        if (runtime.PendingDoorTransition?.Pointer != DoorPointers.GreenBrinstarElevatorFromMainShaft)
            throw new InvalidDataException($"Green Brinstar upward ride did not reach its retail door: Samus={samus.XPosition:X4}/{samus.YPosition:X4}, pose={samus.Pose:X2}, status={runtime.ElevatorStatus}, flags={runtime.Enemies.ElevatorFlags}, pending={runtime.PendingDoorTransition?.Pointer:X4}.");
        RunGreenBrinstarElevatorTransition(
            runtime,
            RoomHeaderPointers.GreenBrinstarElevatorRoom,
            RoomStatePointers.GreenBrinstarElevator,
            "upward",
            captureDirectory);
        int upwardReturnFrames = RunElevatorToRest(
            runtime,
            "upward Green Brinstar arrival",
            captureDirectory,
            "issue-233-upward",
            requireStableTopAlignment: true);
        (int UpBg1, int UpBg2, int UpShift) = CompareWithDirectElevatorReference(
            bus,
            runtime,
            RoomHeaderPointers.GreenBrinstarElevatorRoom,
            captureDirectory,
            "issue-233-upward-reference.png");

        ScrollBoundaryCamera camera = runtime.Camera ?? throw new InvalidDataException(
            "Green Brinstar upward arrival lost its camera.");
        GameplayPpuRenderSnapshot displayed = runtime.DisplayedGameplayPpu;
        Console.WriteLine(
            $"Green Brinstar elevator audit: down={downwardReturnFrames} frames, " +
            $"up={upwardReturnFrames} frames; settled Samus/camera/layer/display=" +
            $"${samus.YPosition:X4}/${camera.YPosition:X4}/" +
            $"${runtime.BackgroundScroll.Layer1YPosition:X4}/${displayed.Layer1YPosition:X4}; " +
            $"BG1VOFS/offset=${runtime.BackgroundScroll.Bg1VerticalScroll:X4}/" +
            $"${runtime.BackgroundScroll.Bg1YOffset:X4}; " +
            $"visible BG1 mismatches down/up={DownBg1}/{UpBg1}; " +
            $"BG2={DownBg2}/{UpBg2}; best row shifts={DownShift}/{UpShift}.");

        // The reference fills from the same live level words, including elevator PLM
        // mutations, so every visible tile must match without tolerating a shifted row.
        if (DownBg1 != 0 || UpBg1 != 0 ||
            DownBg2 != 0 || UpBg2 != 0 || DownShift != 0 || UpShift != 0)
        {
            throw new InvalidDataException(
                $"Green Brinstar elevator left incorrect visible tiles: " +
                $"down BG1/BG2={DownBg1}/{DownBg2}, " +
                $"up={UpBg1}/{UpBg2}, best row shifts={DownShift}/{UpShift}.");
        }
        return 0;
    }

    private static void RunGreenBrinstarElevatorTransition(
        SuperMetroidRuntime runtime,
        ushort expectedRoom,
        ushort expectedState,
        string direction,
        string? captureDirectory)
    {
        CartridgeDoorHeader pendingDoor = runtime.PendingDoorTransition
            ?? throw new InvalidDataException($"Green Brinstar {direction} transition has no door.");
        Console.WriteLine(
            $"  {direction} door ${pendingDoor.Pointer:X4}: orientation=" +
            $"${pendingDoor.Orientation:X2}, destination=" +
            $"${pendingDoor.DestinationScreenX:X2}/${pendingDoor.DestinationScreenY:X2}.");
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        int openingFrame = 0;
        for (int frame = 0; transition.IsActive && frame < 400; frame++)
        {
            DoorTransitionPhase phaseBefore = transition.Phase;
            transition.Step(runtime, audio, controllerInput: 0);
            if (phaseBefore == DoorTransitionPhase.LoadMoreThingsAndOpenDoor)
            {
                ushort destinationY = unchecked((ushort)(pendingDoor.DestinationScreenY << 8));
                ushort expectedInitialCameraY = (pendingDoor.Orientation & 3) switch
                {
                    2 => unchecked((ushort)(destinationY - 224)),
                    3 => unchecked((ushort)(destinationY + 251)),
                    _ => throw new InvalidDataException(
                        $"Green Brinstar {direction} audit received a horizontal door."),
                };
                if (runtime.Camera?.YPosition != expectedInitialCameraY)
                {
                    throw new InvalidDataException(
                        $"Green Brinstar {direction} opening clamped its native modular " +
                        $"camera Y: expected ${expectedInitialCameraY:X4}, observed " +
                        $"${runtime.Camera?.YPosition:X4}.");
                }
                Console.WriteLine(
                    $"  {direction} opening start: cameraY=${runtime.Camera?.YPosition:X4}, " +
                    $"layer2Y=${runtime.BackgroundScroll.Layer2YPosition:X4}, " +
                    $"BG1VOFS/offset=${runtime.BackgroundScroll.Bg1VerticalScroll:X4}/" +
                    $"${runtime.BackgroundScroll.Bg1YOffset:X4}, " +
                    $"blocks L1/BG1/prev=${runtime.BackgroundScroll.Layer1YBlock:X4}/" +
                    $"${runtime.BackgroundScroll.Bg1YBlock:X4}/" +
                    $"${runtime.BackgroundScroll.PreviousLayer1YBlock:X4}.");
            }
            if (phaseBefore == DoorTransitionPhase.WaitForDoorOpeningScroll)
            {
                openingFrame++;
                if (runtime.LastBackgroundUpdateCount > 0)
                {
                    Console.WriteLine(
                        $"    {direction} IRQ {openingFrame:D2}: " +
                        $"L1Y/BG1Y/prev=${runtime.BackgroundScroll.Layer1YBlock:X4}/" +
                        $"${runtime.BackgroundScroll.Bg1YBlock:X4}/" +
                        $"${runtime.BackgroundScroll.PreviousLayer1YBlock:X4}; " +
                        $"scroll=${runtime.BackgroundScroll.Bg1VerticalScroll:X4}.");
                }
                if (captureDirectory is not null && openingFrame is 1 or 16 or 32 or 48 or 56)
                {
                    WriteElevatorFrame(
                        runtime,
                        captureDirectory,
                        $"green-brinstar-{direction}-door-{openingFrame:D2}.png");
                }
            }
        }

        if (transition.IsActive)
            throw new InvalidDataException($"Green Brinstar {direction} transition did not finish.");
        AssertRoom(runtime, expectedRoom, expectedState, $"Green Brinstar {direction} transition");
    }

    private static int RunElevatorToRest(
        SuperMetroidRuntime runtime,
        string description,
        string? captureDirectory,
        string capturePrefix,
        bool requireStableTopAlignment = false)
    {
        int frames = 0;
        int captureIndex = 0;
        var trackingCamera = runtime.Camera ?? throw new InvalidDataException("Elevator return has no camera.");
        ushort previousCameraY = trackingCamera.YPosition;
        bool reachedTopAlignment = previousCameraY == 0;
        Console.WriteLine($"  {description} return begins: camera={previousCameraY:X4}, Samus={runtime.Samus!.YPosition:X4}");
        while (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive && frames < 1200)
        {
            runtime.StepFrame(0);
            runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
            frames++;
            // The arrival handler carries Samus without running her terrain mover. Once
            // blue scrolling has aligned this one-screen room, riding past the scroll
            // trigger must not wake it and send the camera back down toward her.
            if (requireStableTopAlignment &&
                (trackingCamera.Scrolls.Storage[0] != (byte)RoomScrollState.Blue ||
                 (reachedTopAlignment && trackingCamera.YPosition != 0)))
            {
                throw new InvalidDataException(
                    $"Elevator arrival lost top alignment at frame {frames}: " +
                    $"camera=${trackingCamera.YPosition:X4}, scroll={trackingCamera.Scrolls.Storage[0]}.");
            }
            reachedTopAlignment |= trackingCamera.YPosition == 0;
            if (trackingCamera.YPosition != previousCameraY)
            {
                if (captureDirectory is not null)
                    Console.WriteLine($"  {description} tracking frame {frames}: camera={previousCameraY:X4}->{trackingCamera.YPosition:X4}, Samus={runtime.Samus!.YPosition:X4}, direction={runtime.Samus.Kinematics.YDirection}, scrolls={Convert.ToHexString(trackingCamera.Scrolls.Storage)}, status={runtime.Enemies.ElevatorStatus}");
                previousCameraY = trackingCamera.YPosition;
            }
            int elevatorActors = runtime.Enemies.Slots.Count(
                enemy => enemy.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
            if (elevatorActors != 1)
            {
                throw new InvalidDataException(
                    $"{description} rendered {elevatorActors} elevator actors on frame {frames}; " +
                    "the platform must have exactly one OBJ owner.");
            }
            if (captureDirectory is not null && frames % 96 == 0)
            {
                RoomEnemySlot? elevator = runtime.Enemies.Slots.FirstOrDefault(
                    enemy => enemy.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
                Console.WriteLine(
                    $"  {capturePrefix} frame {frames}: SamusY=${runtime.Samus?.YPosition:X4}, " +
                    $"cameraY=${runtime.Camera?.YPosition:X4}, " +
                    $"BG1VOFS=${runtime.BackgroundScroll.Bg1VerticalScroll:X4}, " +
                    $"elevatorY=${elevator?.YPosition:X4}");
                WriteElevatorFrame(
                    runtime,
                    captureDirectory,
                    $"{capturePrefix}-{++captureIndex:D2}.png");
            }
        }

        if (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive)
            throw new InvalidDataException($"{description} did not reach the resting platform.");

        runtime.StepFrame(0);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        if (captureDirectory is not null)
            WriteElevatorFrame(runtime, captureDirectory, $"{capturePrefix}-settled.png");
        return frames;
    }

    private static void WriteElevatorFrame(
        SuperMetroidRuntime runtime,
        string captureDirectory,
        string fileName)
    {
        Directory.CreateDirectory(captureDirectory);
        PngWriter.WriteRgba(
            Path.Combine(captureDirectory, fileName),
            FrontendFrame.Width,
            FrontendFrame.Height,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
    }

    private static (int Bg1, int Bg2, int BestBg1Shift) CompareWithDirectElevatorReference(
        ISnesAddressSpace bus,
        SuperMetroidRuntime observed,
        ushort roomPointer,
        string? captureDirectory,
        string fileName)
    {
        ScrollBoundaryCamera observedCamera = observed.Camera ?? throw new InvalidDataException(
            "Elevator reference has no observed camera.");
        SamusState observedSamus = observed.Samus ?? throw new InvalidDataException(
            "Elevator reference has no observed Samus.");
        SuperMetroidRuntime reference = CreateInitializedRuntime(bus);
        reference.LoadCartridgeRoomForDebug(
            roomPointer,
            observedCamera.XPosition,
            observedCamera.YPosition);
        RoomLevelData liveLevel = observed.LevelData ?? throw new InvalidDataException("Missing live elevator level.");
        RoomLevelData referenceLevel = reference.LevelData ?? throw new InvalidDataException("Missing reference elevator level.");
        for (int block = 0; block < liveLevel.WidthInBlocks * liveLevel.HeightInBlocks; block++)
        {
            referenceLevel.SetForegroundEntry(block, liveLevel.GetCollisionBlockByIndex(block).LevelWord);
            reference.BackgroundStreamer!.SetLevelEntry(block, liveLevel.GetCollisionBlockByIndex(block).LevelWord);
        }
        reference.BackgroundScroll.ConfigureDoorOpeningOffsets(
            observed.BackgroundScroll.Bg1XOffset,
            observed.BackgroundScroll.Bg1YOffset,
            stagedLayer1X: 0,
            stagedLayer1Y: 0);
        // Rebuild an authoritative full viewport using the transition's retained PPU
        // phase. Comparing against a zero-offset debug load is invalid: the cartridge
        // intentionally carries BG offsets across doors and maps world blocks into a
        // different part of the circular tilemap while preserving the same picture.
        reference.BackgroundScroll.Layer1XPosition = observedCamera.XPosition;
        reference.BackgroundScroll.Layer1YPosition = observedCamera.YPosition;
        reference.ExecuteBackgroundStreamRequests(
            reference.BackgroundScroll.BuildInitialViewportRequests(),
            "Green Brinstar offset-preserving reference fill");
        reference.BackgroundScroll.PrimePreviousBlocks();
        SamusState referenceSamus = reference.Samus ?? throw new InvalidDataException(
            "Elevator reference room lost Samus.");
        referenceSamus.Pose = observedSamus.Pose;
        referenceSamus.XPosition = observedSamus.XPosition;
        referenceSamus.YPosition = observedSamus.YPosition;
        referenceSamus.RefreshCollisionRadii(bus);
        referenceSamus.InitializeAnimation(bus);
        reference.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        if (captureDirectory is not null)
            WriteElevatorFrame(reference, captureDirectory, fileName);

        ushort[] observedBg1 = SnapshotVisibleBgTiles(observed, backgroundLayer: false);
        ushort[] referenceBg1 = SnapshotVisibleBgTiles(reference, backgroundLayer: false);
        ushort[] observedBg2 = SnapshotVisibleBgTiles(observed, backgroundLayer: true);
        ushort[] referenceBg2 = SnapshotVisibleBgTiles(reference, backgroundLayer: true);
        int bg1Mismatches = 0;
        int bg2Mismatches = 0;
        for (int tile = 0; tile < observedBg1.Length; tile++)
        {
            if (observedBg1[tile] != referenceBg1[tile])
            {
                bg1Mismatches++;
                if (bg1Mismatches <= 12)
                    Console.WriteLine($"  tile mismatch {tile % 32}/{tile / 32}: {observedBg1[tile]:X4} != {referenceBg1[tile]:X4}");
            }
            if (observedBg2[tile] != referenceBg2[tile])
                bg2Mismatches++;
        }
        Console.WriteLine(
            $"  {fileName}: BG1 mismatched rows=" +
            string.Join(',', Enumerable.Range(0, 24)
                .Select(row => Enumerable.Range(0, 32).Count(column =>
                    observedBg1[row * 32 + column] != referenceBg1[row * 32 + column]))) +
            "; BG2=" +
            string.Join(',', Enumerable.Range(0, 24)
                .Select(row => Enumerable.Range(0, 32).Count(column =>
                    observedBg2[row * 32 + column] != referenceBg2[row * 32 + column]))));
        int[] shiftMismatches = Enumerable.Range(-4, 9)
            .Select(shift =>
                {
                    int count = 0;
                    for (int row = Math.Max(0, -shift); row < Math.Min(24, 24 - shift); row++)
                    {
                        for (int column = 0; column < 32; column++)
                        {
                            if (observedBg1[row * 32 + column] !=
                                referenceBg1[(row + shift) * 32 + column])
                                count++;
                        }
                    }
                    return count;
                })
            .ToArray();
        int bestBg1Shift = Enumerable.Range(-4, 9)
            .MinBy(shift => shiftMismatches[shift + 4]);
        Console.WriteLine(
            "    BG1 shift mismatches=" + string.Join(',', Enumerable.Range(-4, 9)
                .Select(shift => $"{shift}:{shiftMismatches[shift + 4]}")));
        return (bg1Mismatches, bg2Mismatches, bestBg1Shift);
    }

    private static ushort[] SnapshotVisibleBgTiles(
        SuperMetroidRuntime runtime,
        bool backgroundLayer)
    {
        const int tileColumns = FrontendFrame.Width / 8;
        const int tileRows = (FrontendFrame.Height - SnesGameplayFrameRenderer.HudHeight) / 8;
        var result = new ushort[tileColumns * tileRows];
        ushort horizontal = backgroundLayer
            ? runtime.BackgroundScroll.Bg2HorizontalScroll
            : runtime.BackgroundScroll.Bg1HorizontalScroll;
        ushort vertical = unchecked((ushort)(
            (backgroundLayer
                ? runtime.BackgroundScroll.Bg2VerticalScroll
                : runtime.BackgroundScroll.Bg1VerticalScroll) +
            SnesGameplayFrameRenderer.HudHeight));
        ushort tilemapBase = backgroundLayer
            ? SnesPpuLayout.GameplayBg2TilemapWord
            : SnesPpuLayout.GameplayBg1TilemapWord;

        for (int row = 0; row < tileRows; row++)
        {
            int tileY = ((vertical + row * 8) & 0x00ff) >> 3;
            for (int column = 0; column < tileColumns; column++)
            {
                int tileX = ((horizontal + column * 8) & 0x01ff) >> 3;
                int screenWordOffset = (tileX >> 5) * 0x0400;
                int mapWord = tilemapBase + screenWordOffset +
                    tileY * 32 + (tileX & 31);
                result[row * tileColumns + column] = runtime.Vram.ReadWord(mapWord);
            }
        }
        return result;
    }

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

            samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
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
                    SamusPoseIds.UnmorphingTransitionRightPose,
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
