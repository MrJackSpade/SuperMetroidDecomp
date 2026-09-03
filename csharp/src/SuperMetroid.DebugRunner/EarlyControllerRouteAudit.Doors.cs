using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class EarlyControllerRouteAudit
{
    /// <summary>
    /// Reproduces issue #62's exact area-$01 room-$0F to room-$0E transition and requires
    /// source-room OAM to be parked before the horizontal door begins moving.
    /// </summary>
    public static int RunConstructionZoneDoorGhostAudit(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ConstructionZone);

        CartridgeRoomHeader source = runtime.ActiveRoom ?? throw new InvalidDataException(
            "Construction Zone ghost audit did not load a source room.");
        if (source.AreaIndex != AreaId.Brinstar || source.RoomIndex != 0x0f)
        {
            throw new InvalidDataException(
                $"Expected source $01/$0F, got ${source.AreaIndex:X2}/${source.RoomIndex:X2} " +
                $"at $8F:{source.Pointer:X4}.");
        }

        // Two frames are required because OAM produced by the first gameplay pass becomes
        // PPU-visible at the following NMI. A zero count would let this audit pass without
        // ever reproducing the source-room objects reported by the player.
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        int sourceSpriteCount = runtime.DisplayedOam.LastFinalizedSpriteCount;
        if (sourceSpriteCount == 0 || runtime.Enemies.EnemyCount == 0)
        {
            throw new InvalidDataException(
                "Area $01/$0F did not publish its source-room enemy OAM before transition.");
        }

        PublishRetailDoor(runtime, bus, DoorPointers.MorphBallFromConstructionZone);
        CartridgeDoorHeader door = runtime.PendingDoorTransition ??
            throw new InvalidDataException("Construction Zone did not publish its Morph Ball door.");
        if (door.DestinationRoomPointer != RoomHeaderPointers.MorphBallRoom)
        {
            throw new InvalidDataException(
                $"Door $83:{door.Pointer:X4} targets $8F:{door.DestinationRoomPointer:X4}, " +
                $"not $01/$0E $8F:{RoomHeaderPointers.MorphBallRoom:X4}.");
        }
        var audio = new CartridgeAudioState();
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        bool observedOpeningScroll = false;
        for (int dispatcherFrame = 0;
            transition.IsActive && dispatcherFrame < 320;
            dispatcherFrame++)
        {
            transition.Step(runtime, audio, controllerInput: 0);
            if (transition.Phase != DoorTransitionPhase.WaitForDoorOpeningScroll)
                continue;

            observedOpeningScroll = true;
            if (runtime.DisplayedOam.LastFinalizedSpriteCount != 0)
            {
                throw new InvalidDataException(
                    $"Reproduced issue #62: door $83:{door.Pointer:X4} began scrolling with " +
                    $"{runtime.DisplayedOam.LastFinalizedSpriteCount} stale source-room OBJ entries.");
            }
        }

        if (transition.IsActive || !observedOpeningScroll)
            throw new InvalidDataException("Construction Zone ghost transition did not complete its scroll.");
        CartridgeRoomHeader destination = runtime.ActiveRoom ?? throw new InvalidDataException(
            "Construction Zone ghost audit lost the destination room.");
        if (destination.AreaIndex != AreaId.Brinstar || destination.RoomIndex != 0x0e)
        {
            throw new InvalidDataException(
                $"Expected destination $01/$0E, got ${destination.AreaIndex:X2}/" +
                $"${destination.RoomIndex:X2} at $8F:{destination.Pointer:X4}.");
        }

        Console.WriteLine(
            $"Construction Zone door-ghost audit passed: $01/$0F -> $01/$0E via " +
            $"$83:{door.Pointer:X4}; {sourceSpriteCount} source OBJ entries were cleared " +
            "before every visible door-scroll frame.");
        return 0;
    }

    /// <summary>
    /// Reproduces both horizontal door directions through the production transition and
    /// renderer using retail Landing Site/Parlor room data. Unlike the historical request-
    /// count check, this audit snapshots the actual BG1 VRAM ring before and after the first
    /// visible door-opening step and writes representative composed frames to disk.
    /// </summary>
    public static int RunDoorTransitionVisualAudit(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();

        var directHost = new ControllerRouteHost(
            StepFrame: input => runtime.StepFrame(input),
            LoadPendingDoor: () => runtime.LoadPendingDoorDestination());
        for (int frame = 0;
            runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted && frame < 1200;
            frame++)
        {
            directHost.StepFrame(0);
        }
        if (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted)
            throw new InvalidDataException("Door audit did not reach Landing Site control handoff.");

        _ = DriveUntilDoor(bus, runtime, directHost, "Landing Site", maximumFrames: 2400);
        AssertPendingDoor(
            runtime,
            DoorPointers.ParlorFromLandingSite,
            RoomHeaderPointers.ParlorAndAlcatraz,
            "door audit Landing Site -> Parlor");

        string outputDirectory = Path.Combine("csharp", "test-temp", "door-transition-visual");
        Directory.CreateDirectory(outputDirectory);
        DoorTransitionMutation left = CaptureDoorTransition(
            runtime,
            outputDirectory,
            "left-landing-to-parlor");

        // Reproduce the rightward case with the real First Missile room and its ordinary
        // return door into Construction Zone. This is the early-game transition where the
        // player reported the door passage looking one column short. Only the approach is
        // staged; the room, type-$9 collision, bank-$83 door, loader, scroll IRQ, and final
        // renderer are all the production cartridge path.
        runtime.LoadCartridgeRoomForDebug(
            RoomHeaderPointers.FirstMissileRoom,
            DoorTransitionAuditPositions.FirstMissileCameraX,
            DoorTransitionAuditPositions.FirstMissileCameraY);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Door audit lost Samus while staging the First Missile doorway.");
        samus.Kinematics.SetXFixed(DoorTransitionAuditPositions.FirstMissileSamusXFixed);
        samus.Kinematics.SetYFixed(DoorTransitionAuditPositions.FirstMissileSamusYFixed);
        PublishRetailDoor(runtime, bus, DoorPointers.ConstructionZoneFromFirstMissile);
        AssertPendingDoor(
            runtime,
            DoorPointers.ConstructionZoneFromFirstMissile,
            RoomHeaderPointers.ConstructionZone,
            "door audit First Missile -> Construction Zone");
        DoorTransitionMutation right = CaptureDoorTransition(
            runtime,
            outputDirectory,
            "right-first-missile-to-construction");

        DoorTransitionMutation ceresRightLower = CaptureStagedRetailDoor(
            runtime,
            bus,
            outputDirectory,
            "right-ceres-magnet-stairs-to-scientists",
            RoomHeaderPointers.CeresMagnetStairs,
            cameraX: DoorTransitionAuditPositions.Origin,
            cameraY: DoorTransitionAuditPositions.CeresMagnetStairsCameraY,
            samusXFixed: DoorTransitionAuditPositions.RightDoorSamusXFixed,
            samusYFixed: DoorTransitionAuditPositions.CeresMagnetStairsDoorSamusYFixed,
            DoorPointers.CeresDeadScientistFromMagnetStairs,
            RoomHeaderPointers.CeresDeadScientistRoom);
        DoorTransitionMutation ceresRightUpper = CaptureStagedRetailDoor(
            runtime,
            bus,
            outputDirectory,
            "right-ceres-scientists-to-final-hall",
            RoomHeaderPointers.CeresDeadScientistRoom,
            cameraX: DoorTransitionAuditPositions.SecondScreenCameraX,
            cameraY: DoorTransitionAuditPositions.Origin,
            samusXFixed: DoorTransitionAuditPositions.SecondScreenRightDoorSamusXFixed,
            samusYFixed: DoorTransitionAuditPositions.CeresCorridorDoorSamusYFixed,
            DoorPointers.CeresFinalHallwayFromDeadScientist,
            RoomHeaderPointers.CeresFinalHallway);
        DoorTransitionMutation ceresLeft = CaptureStagedRetailDoor(
            runtime,
            bus,
            outputDirectory,
            "left-ceres-final-hall-to-scientists",
            RoomHeaderPointers.CeresFinalHallway,
            cameraX: DoorTransitionAuditPositions.Origin,
            cameraY: DoorTransitionAuditPositions.Origin,
            samusXFixed: DoorTransitionAuditPositions.LeftDoorSamusXFixed,
            samusYFixed: DoorTransitionAuditPositions.CeresCorridorDoorSamusYFixed,
            DoorPointers.CeresDeadScientistFromFinalHallway,
            RoomHeaderPointers.CeresDeadScientistRoom);

        // At this point native code has run one four-pixel setup step. That step can replace
        // only one two-tile-wide, 32-tile-tall BG1 ring column: at most 64 words. Replacing
        // hundreds of words proves the source viewport was prematurely destroyed by a full
        // destination fill, which is the visible jumping/short-door failure reported by the
        // player. Keep the failure until the shared loader respects the native incremental
        // transition rather than accepting a nearby endpoint-only check.
        Console.WriteLine(
            $"Door visual audit: left changed {left.ChangedBg1Words} BG1 words; " +
            $"right changed {right.ChangedBg1Words}; representative frames are in " +
            $"{Path.GetFullPath(outputDirectory)}.");
        AssertInitialDoorMutation(left);
        AssertInitialDoorMutation(right);
        AssertInitialDoorMutation(ceresRightLower);
        AssertInitialDoorMutation(ceresRightUpper);
        AssertInitialDoorMutation(ceresLeft);
        AssertDoorSilhouette(left);
        AssertDoorSilhouette(right);
        AssertDoorSilhouette(ceresRightLower);
        AssertDoorSilhouette(ceresRightUpper);
        AssertDoorSilhouette(ceresLeft);
        return 0;
    }

    private static DoorTransitionMutation CaptureStagedRetailDoor(
        SuperMetroidRuntime runtime,
        ISnesAddressSpace bus,
        string outputDirectory,
        string name,
        ushort sourceRoomPointer,
        ushort cameraX,
        ushort cameraY,
        uint samusXFixed,
        uint samusYFixed,
        ushort doorPointer,
        ushort destinationRoomPointer)
    {
        runtime.LoadCartridgeRoomForDebug(sourceRoomPointer, cameraX, cameraY);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            $"{name} lost Samus while staging its real cartridge doorway.");
        samus.Kinematics.SetXFixed(samusXFixed);
        samus.Kinematics.SetYFixed(samusYFixed);
        PublishRetailDoor(runtime, bus, doorPointer);
        AssertPendingDoor(runtime, doorPointer, destinationRoomPointer, name);
        return CaptureDoorTransition(runtime, outputDirectory, name);
    }

    private static DoorTransitionMutation CaptureDoorTransition(
        SuperMetroidRuntime runtime,
        string outputDirectory,
        string name)
    {
        CartridgeDoorHeader door = runtime.PendingDoorTransition ??
            throw new InvalidDataException($"{name} began without a pending door.");
        ushort[] beforeOpening = Array.Empty<ushort>();
        ushort[] afterOpening = Array.Empty<ushort>();
        DoorSilhouette? middleSilhouette = null;
        var audio = new CartridgeAudioState();
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        int scrollFrame = 0;
        for (int dispatcherFrame = 0; transition.IsActive && dispatcherFrame < 320; dispatcherFrame++)
        {
            DoorTransitionPhase before = transition.Phase;
            if (before == DoorTransitionPhase.LoadMoreThingsAndOpenDoor)
                beforeOpening = SnapshotBg1Tilemap(runtime.Vram);

            transition.Step(runtime, audio, controllerInput: 0);

            if (before == DoorTransitionPhase.LoadMoreThingsAndOpenDoor)
            {
                afterOpening = SnapshotBg1Tilemap(runtime.Vram);
                WriteDoorFrame(runtime, outputDirectory, name, "opening-setup");
            }
            else if (before == DoorTransitionPhase.WaitForDoorOpeningScroll)
            {
                scrollFrame++;
                if (scrollFrame is 1 or 16 or 32 or 48 or 63)
                {
                    Rgba32[] pixels = WriteDoorFrame(
                        runtime,
                        outputDirectory,
                        name,
                        $"scroll-{scrollFrame:D2}");
                    if (scrollFrame == 32)
                        middleSilhouette = MeasureDoorSilhouette(pixels, name);
                }
            }
        }
        if (transition.IsActive)
            throw new InvalidDataException($"{name} did not finish within 320 dispatcher frames.");
        if (beforeOpening.Length == 0 || afterOpening.Length == 0)
            throw new InvalidDataException($"{name} never reached its opening-scroll setup.");

        var changedWords = new List<ushort>();
        for (int word = 0; word < beforeOpening.Length; word++)
        {
            if (beforeOpening[word] != afterOpening[word])
                changedWords.Add(unchecked((ushort)(DoorTransitionAuditVram.Bg1TilemapFirstWord + word)));
        }
        Console.WriteLine(
            $"  {name} BG1 mutations: {FormatWordRanges(changedWords)}");
        return new DoorTransitionMutation(
            name,
            door.Pointer,
            door.Orientation,
            changedWords.Count,
            middleSilhouette ?? throw new InvalidDataException(
                $"{name} did not capture its middle visible door frame."));
    }

    private static void PublishRetailDoor(
        SuperMetroidRuntime runtime,
        ISnesAddressSpace bus,
        ushort expectedDoorPointer)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "Door audit cannot find a reciprocal door without room level data.");
        byte pose = runtime.Samus?.Pose ?? 0;
        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType != RoomCollisionType.DoorBlock)
                    continue;
                CartridgeDoorHeader candidate = level.ResolveDoorCollision(
                    bus,
                    block.Behavior,
                    pose,
                    publishDoorSideEffects: false);
                if (candidate.Pointer != expectedDoorPointer)
                    continue;
                _ = level.ResolveDoorCollision(
                    bus,
                    block.Behavior,
                    pose,
                    publishDoorSideEffects: true);
                return;
            }
        }
        throw new InvalidDataException(
            $"Active retail room has no type-$9 block for door $83:{expectedDoorPointer:X4}.");
    }

    private static ushort[] SnapshotBg1Tilemap(SnesVram vram)
    {
        var words = new ushort[DoorTransitionAuditVram.Bg1TilemapWordCount];
        for (int index = 0; index < words.Length; index++)
            words[index] = vram.ReadWord(DoorTransitionAuditVram.Bg1TilemapFirstWord + index);
        return words;
    }

    private static Rgba32[] WriteDoorFrame(
        SuperMetroidRuntime runtime,
        string outputDirectory,
        string name,
        string suffix)
    {
        Rgba32[] pixels = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        PngWriter.WriteRgba(
            Path.Combine(outputDirectory, $"{name}.{suffix}.png"),
            FrontendFrame.Width,
            FrontendFrame.Height,
            pixels);
        return pixels;
    }

    private static void AssertInitialDoorMutation(DoorTransitionMutation mutation)
    {
        const int MaximumWordsWrittenByOneBg1Column = 64;
        if (mutation.ChangedBg1Words > MaximumWordsWrittenByOneBg1Column)
        {
            throw new InvalidDataException(
                $"Reproduced {mutation.Name}: horizontal door $83:{mutation.DoorPointer:X4} " +
                $"orientation {mutation.Orientation & 3} replaced {mutation.ChangedBg1Words} " +
                $"BG1 tilemap words before its first visible scroll frame; native one-column " +
                $"setup can replace at most {MaximumWordsWrittenByOneBg1Column}.");
        }
    }

    private static DoorSilhouette MeasureDoorSilhouette(Rgba32[] pixels, string name)
    {
        int minX = FrontendFrame.Width;
        int minY = FrontendFrame.Height;
        int maxX = -1;
        int maxY = -1;
        var columnTops = new int[FrontendFrame.Width];
        Array.Fill(columnTops, int.MaxValue);
        for (int y = SnesGameplayFrameRenderer.HudHeight; y < FrontendFrame.Height; y++)
        {
            for (int x = 0; x < FrontendFrame.Width; x++)
            {
                Rgba32 pixel = pixels[y * FrontendFrame.Width + x];
                if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                    continue;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
                columnTops[x] = Math.Min(columnTops[x], y);
            }
        }
        if (maxX < minX || maxY < minY)
            throw new InvalidDataException($"{name} middle frame contains no visible door pixels.");

        int highestColumnTop = int.MaxValue;
        int lowestColumnTop = int.MinValue;
        for (int x = minX; x <= maxX; x++)
        {
            if (columnTops[x] == int.MaxValue)
                continue;
            highestColumnTop = Math.Min(highestColumnTop, columnTops[x]);
            lowestColumnTop = Math.Max(lowestColumnTop, columnTops[x]);
        }
        var silhouette = new DoorSilhouette(
            minX,
            minY,
            maxX,
            maxY,
            lowestColumnTop - highestColumnTop);
        Console.WriteLine(
            $"  {name} middle silhouette: x={minX}..{maxX}, y={minY}..{maxY}, " +
            $"column-top spread={silhouette.ColumnTopSpread}.");
        return silhouette;
    }

    private static void AssertDoorSilhouette(DoorTransitionMutation mutation)
    {
        // The retail door art has small bevels, but no column begins an entire 16-pixel
        // block above another. The repeated left-door regression produced exactly that
        // isolated raised column while still satisfying camera and DMA-count assertions.
        if (mutation.MiddleSilhouette.ColumnTopSpread >= LevelBlockSizePixels)
        {
            throw new InvalidDataException(
                $"Reproduced {mutation.Name}: rendered door $83:{mutation.DoorPointer:X4} " +
                $"has column tops spread across {mutation.MiddleSilhouette.ColumnTopSpread} " +
                $"pixels, at least one complete 16-pixel block.");
        }
    }

    private static string FormatWordRanges(List<ushort> words)
    {
        if (words.Count == 0)
            return "none";
        var ranges = new List<string>();
        int start = words[0];
        int previous = start;
        for (int index = 1; index <= words.Count; index++)
        {
            int current = index < words.Count ? words[index] : -1;
            if (current == previous + 1)
            {
                previous = current;
                continue;
            }
            ranges.Add(start == previous
                ? $"${start:X4}"
                : $"${start:X4}-${previous:X4}");
            start = current;
            previous = current;
        }
        return string.Join(',', ranges);
    }

    private readonly record struct DoorTransitionMutation(
        string Name,
        ushort DoorPointer,
        byte Orientation,
        int ChangedBg1Words,
        DoorSilhouette MiddleSilhouette);

    private readonly record struct DoorSilhouette(
        int MinX,
        int MinY,
        int MaxX,
        int MaxY,
        int ColumnTopSpread);
}
