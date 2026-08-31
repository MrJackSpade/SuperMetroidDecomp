using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Incremental controller-only playthrough from the completed Landing Site gunship scene.
/// </summary>
/// <remarks>
/// Unlike <see cref="EarlyRouteAudit"/>, this audit is forbidden from publishing a door
/// collision or changing Samus coordinates. It deliberately starts with just the first
/// surface exit; each subsequently proven segment will be appended until the complete
/// Bomb Torizo route runs from ordinary controller samples alone.
/// </remarks>
internal static class EarlyControllerRouteAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();

        int landingFrames = 0;
        while (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               landingFrames < 1200)
        {
            runtime.StepFrame(0);
            landingFrames++;
        }
        if (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted)
            throw new InvalidDataException("Controller route did not reach gunship control handoff.");

        PrintDoorBlocks(runtime, "Landing Site");
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Gunship landing did not retain Samus.");
        DriveResult landing = DriveUntilDoor(runtime, "Landing Site", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8916, 0x92fd, "Landing Site -> Parlor");

        // Loading a collision-published door is the normal outer game-state action. The
        // audit does not identify the destination to the loader and does not edit placement;
        // bank-$83's record supplies both, exactly as the desktop frontend does.
        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x92fd, 0x9314, "Parlor");
        PrintDoorBlocks(runtime, "Parlor");
        Console.WriteLine(
            $"  Parlor camera metadata: scrolls={string.Join(',', runtime.Camera!.Scrolls.Storage.ToArray().Take(runtime.Camera.Scrolls.LogicalCellCount).Select(value => value.ToString("X2")))} " +
            $"scrollers=(${runtime.ActiveRoom!.UpScroller:X2},${runtime.ActiveRoom.DownScroller:X2}) " +
            $"entry=(${runtime.Camera.XPosition:X4},${runtime.Camera.YPosition:X4}) " +
            $"main=$8F:{runtime.ActiveRoom.State.MainCodePointer:X4} setup=$8F:{runtime.ActiveRoom.State.SetupCodePointer:X4}.");
        PrintPlmPopulation(bus, runtime.ActiveRoom.State.PlmPointer);
        Console.WriteLine(
            $"  Loaded scroll PLMs: {string.Join(' ', runtime.Plms.ScrollPlms.Select(scroll => $"{scroll.BlockIndex}/${scroll.DataPointer:X4}"))}.");

        DriveResult parlor = DriveUntilDoor(runtime, "Parlor", maximumFrames: 3600);
        AssertPendingDoor(runtime, 0x898e, 0x96ba, "Parlor -> Climb");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x96ba, 0x96d1, "Climb");
        PrintDoorBlocks(runtime, "Climb");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult climb = DriveUntilDoor(runtime, "Climb", maximumFrames: 6000);
        AssertPendingDoor(runtime, 0x8b62, 0x975c, "Climb -> Pit");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x975c, 0x976d, "Pit");
        PrintDoorBlocks(runtime, "Pit");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult pit = DriveUntilDoor(runtime, "Pit", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8b86, 0x97b5, "Pit -> elevator room");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x97b5, 0x97c6, "Elevator to Blue Brinstar");
        PrintDoorBlocks(runtime, "Elevator to Blue Brinstar");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult elevator = DriveUntilDoor(
            runtime,
            "Elevator to Blue Brinstar",
            maximumFrames: 2400);
        CartridgeDoorHeader elevatorExit = runtime.LevelData?.PendingDoorTransition ??
            throw new InvalidDataException("Elevator departure ended without its pseudo-door.");
        Console.WriteLine(
            $"  Elevator pseudo-door: $83:{elevatorExit.Pointer:X4}, " +
            $"destination=${elevatorExit.DestinationRoomPointer:X4}, flags=${elevatorExit.BitFlags:X2}, " +
            $"orientation=${elevatorExit.Orientation:X2}, PLM=({elevatorExit.PlmX:X2},{elevatorExit.PlmY:X2}), " +
            $"screen=({elevatorExit.DestinationScreenX:X2},{elevatorExit.DestinationScreenY:X2}), " +
            $"distance=${elevatorExit.SamusDistance:X4}, setup=${elevatorExit.SetupCodePointer:X4}.");
        AssertPendingDoor(runtime, 0x8b9e, 0x9e9f, "Elevator -> Morph Ball room");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x9e9f, 0x9eb1, "Morph Ball room");
        PrintDoorBlocks(runtime, "Morph Ball room");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult morphBall = DriveUntilDoor(runtime, "Morph Ball room", maximumFrames: 5000);
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) ||
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
        {
            throw new InvalidDataException(
                "Morph Ball room exited without the cartridge collectible setting both item words.");
        }
        AssertPendingDoor(runtime, 0x8eb6, 0x97b5, "Morph Ball return elevator");

        Console.WriteLine(
            $"Controller route: gunship {landingFrames} frames; Landing Site -> Parlor " +
            $"after {landing.Frames} ordinary gameplay frames; Parlor -> Climb after " +
            $"{parlor.Frames} more frames; Climb -> Pit after {climb.Frames} more; Pit exit " +
            $"after {pit.Frames} more; elevator descent after {elevator.Frames} more; " +
            $"Morph Ball acquired and return elevator reached after {morphBall.Frames} more at Samus " +
            $"(${samus.XPosition:X4},${samus.YPosition:X4}).");
        return 0;
    }

    private static DriveResult DriveUntilDoor(
        SuperMetroidRuntime runtime,
        string roomName,
        int maximumFrames)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{roomName} controller drive began without Samus.");
        ushort previousX = samus.XPosition;
        ushort previousY = samus.YPosition;
        int stationaryFrames = 0;
        int horizontallyStationaryFrames = 0;
        int firedShots = 0;
        int collisionExplosions = 0;
        int jumpHoldFrames = 0;
        int floorHatchCycle = -1;
        // Landing Site, Parlor, and Climb begin by travelling left or descending
        // from a leftward approach. Pit is entered through its left cap and the
        // cartridge route continues to the right.
        SnesButton horizontalDirection = roomName is "Pit" or "Elevator to Blue Brinstar"
            ? SnesButton.Right
            : SnesButton.Left;
        var firedByDirection = new int[16];
        var collisionsByDirection = new int[16];
        var floorHatchShotTrace = new List<string>();
        int frame = 0;
        var routeTrace = new List<string>();
        int previousScreenX = -1;
        int previousScreenY = -1;
        int previousScrollPlmCount = runtime.Plms.ScrollPlms.Count;

        while (!runtime.HasPendingDoorTransition && frame < maximumFrames)
        {
            // The outbound new-game descent trends left in both Landing Site and Parlor.
            // A fresh shot every 24 frames opens encountered blue caps. If collision has
            // held both coordinates still for half a second, one ordinary Jump edge clears
            // low terrain; ROM pose tables and collision remain the only authorities that
            // can accept it. This script is intentionally simple so each room that needs a
            // different directional decision becomes an explicit later route segment.
            // Parlor's upper approach ends on a small left-wall ledge. Continuing to hold
            // left there cannot enter the descending zig-zag shaft: the player must cross
            // each ledge and walk off its opposite edge. Reverse only after the ordinary
            // horizontal collision code has held X fixed for twenty frames. This is an
            // input policy, not a coordinate/path shortcut; slopes, gravity, and walls are
            // still resolved by the translated game systems on every frame.
            if ((roomName is "Parlor" or "Climb") &&
                samus.YPosition >= 0x0100 &&
                horizontallyStationaryFrames == 20)
            {
                horizontalDirection = horizontalDirection == SnesButton.Left
                    ? SnesButton.Right
                    : SnesButton.Left;
            }

            bool aboveParlorFloorHatch = roomName == "Parlor" &&
                samus.YPosition >= 0x0480;
            bool approachingBlueBrinstarElevator =
                roomName == "Elevator to Blue Brinstar";
            bool crossingMorphBallRoom = roomName == "Morph Ball room";
            ushort input;
            if (aboveParlorFloorHatch)
            {
                // The Climb exit is a downward-facing blue cap. Center over its four-block
                // width, jump, hold Down to select the cartridge-authored downward aerial
                // pose, and pulse Fire into the cap. No audit-side PLM spawn is permitted.
                input = samus.XPosition switch
                {
                    < 0x0174 => (ushort)SnesButton.Right,
                    > 0x018c => (ushort)SnesButton.Left,
                    _ => 0,
                };
                bool centered = samus.XPosition is >= 0x0174 and <= 0x018c;
                floorHatchCycle = centered ? (floorHatchCycle + 1) % 60 : -1;
                if (floorHatchCycle is >= 0 and < 8)
                {
                    // Jump must be established before Down is introduced. Pressing both
                    // from a grounded pose selects crouch in the retail transition table.
                    input |= (ushort)SnesButton.A;
                }
                else if (floorHatchCycle is >= 8 and < 50)
                {
                    input |= (ushort)SnesButton.Down;
                    if (floorHatchCycle % 6 == 0)
                        input |= (ushort)SnesButton.X;
                }
            }
            else if (approachingBlueBrinstarElevator)
            {
                // The elevator actor waits for a newly-pressed direction only after the
                // floor's type-$9 pseudo-door collision has set $0E16. Walk onto the two
                // cartridge-authored platform columns, then pulse Down with intervening
                // released frames so the actor—not this audit—can accept the edge and
                // begin departure on its native frame-order boundary.
                input = samus.XPosition switch
                {
                    < 0x0074 => (ushort)SnesButton.Right,
                    > 0x008c => (ushort)SnesButton.Left,
                    _ => frame % 30 == 0 ? (ushort)SnesButton.Down : (ushort)0,
                };
            }
            else if (crossingMorphBallRoom)
            {
                bool hasMorphBall = samus.CollectedItems.HasAny(
                    SamusEquipmentFlags.MorphBall);
                if (!hasMorphBall)
                {
                    // Static face blocks and the ruined floor form several narrow lips on
                    // the westbound approach. A thirty-frame held Jump followed by a full
                    // release produces genuine new-button edges on each attempt and keeps
                    // Left held throughout; bank-$90 owns the resulting arc and collision.
                    input = (ushort)SnesButton.Left;
                    if (frame % 60 < 30)
                        input |= (ushort)SnesButton.A;
                }
                else
                {
                    RoomCollisionBlock returnBlock = runtime.LevelData!
                        .GetCollisionBlock(0x4c, 0x2c);
                    bool returnBlockStillSolid = returnBlock.CollisionType == 0x0c &&
                        returnBlock.Behavior == 0x04;
                    bool isMorphBall = SamusState.IsGroundedMorphBallPose(samus.Pose) ||
                        SamusState.IsAirborneMorphBallPose(samus.Pose);
                    bool centeredOnReturnElevator =
                        samus.XPosition is >= 0x0574 and <= 0x058c;
                    if (returnBlockStillSolid)
                    {
                        // The return tunnel begins with one permanent beam-break block at
                        // (4C,2C). Clear it before morphing: Right+Down selects the ROM's
                        // diagonal-down running aim and periodic X edges create ordinary
                        // projectiles whose block collision owns the terrain mutation.
                        input = (ushort)(SnesButton.Right | SnesButton.Down);
                        if (frame % 12 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                    else if (centeredOnReturnElevator)
                    {
                        // The west wall is BTS $09, intentionally Power-Bomb-only. The
                        // new-game route returns east after collecting Morph Ball. Pulse
                        // Up over the same two-column pseudo-door so the ordinary elevator
                        // actor owns the return trip to Crateria.
                        input = frame % 30 == 0
                            ? (ushort)SnesButton.Up
                            : (ushort)0;
                    }
                    else if (samus.XPosition >= 0x0530 && isMorphBall)
                    {
                        // The low tunnel ends at a one-block step. There is full standing
                        // headroom here, so use a clean release/Up cycle to invoke the ROM
                        // un-morph transition before attempting the ordinary jump.
                        input = frame % 60 < 20
                            ? (ushort)0
                            : (ushort)SnesButton.Up;
                    }
                    else if (samus.XPosition >= 0x0530)
                    {
                        input = (ushort)SnesButton.Right;
                        if (frame % 60 < 30)
                            input |= (ushort)SnesButton.A;
                    }
                    else if (isMorphBall)
                    {
                        input = (ushort)SnesButton.Right;
                    }
                    else
                    {
                        // Left+Down is an aimed-running command, not a morph command. Give
                        // the pose table a clean release window and then Down by itself so
                        // it can perform stand -> crouch -> morph. Repeating the cycle is
                        // necessary because the item-acquisition message temporarily owns
                        // Samus input; the first edge may legitimately occur during it.
                        input = frame % 60 < 20
                            ? (ushort)0
                            : (ushort)SnesButton.Down;
                    }
                }
            }
            else
            {
                // Climb's last two screens are a special case for the deliberately tiny
                // controller driver, not for gameplay. The long shaft alternates shallow
                // lips on its left and right walls; choosing the direction toward the
                // shaft centre makes Samus walk off whichever lip caught her. Once the
                // bottom screen is reached, the cartridge collision map puts the Pit door
                // at the lower-right edge, so ordinary Right input completes the segment.
                // Coordinates are inspected only to choose controller buttons: movement,
                // falling, collision, scrolling, and door publication all remain owned by
                // the translated runtime.
                bool descendingLowerClimb = roomName == "Climb" &&
                    samus.YPosition is >= 0x0700 and < 0x0800;
                bool approachingPitDoor = roomName == "Climb" &&
                    samus.YPosition >= 0x0800;
                if (descendingLowerClimb)
                {
                    input = samus.XPosition < 0x0180
                        ? (ushort)SnesButton.Right
                        : (ushort)SnesButton.Left;
                }
                else if (approachingPitDoor)
                {
                    input = (ushort)SnesButton.Right;
                }
                else
                {
                    input = (ushort)horizontalDirection;
                }
                if (frame % 24 == 0)
                    input |= (ushort)SnesButton.X;
                if ((roomName != "Parlor" || samus.YPosition < 0x0100) &&
                    stationaryFrames == 30 && jumpHoldFrames == 0)
                {
                    jumpHoldFrames = 24;
                }
                if (jumpHoldFrames > 0)
                {
                    input |= (ushort)SnesButton.A;
                    jumpHoldFrames--;
                }
            }

            byte poseBeforeStep = samus.Pose;
            ushort yBeforeStep = samus.YPosition;
            runtime.StepFrame(input);
            frame++;
            int currentScrollPlmCount = runtime.Plms.ScrollPlms.Count;
            if (currentScrollPlmCount != previousScrollPlmCount)
            {
                Console.WriteLine(
                    $"  {roomName} scroll PLMs changed {previousScrollPlmCount}->{currentScrollPlmCount} " +
                    $"at frame {frame}, Samus=(${samus.XPosition:X4},${samus.YPosition:X4}).");
                previousScrollPlmCount = currentScrollPlmCount;
            }

            if (aboveParlorFloorHatch &&
                (input & (ushort)SnesButton.X) != 0 &&
                floorHatchShotTrace.Count < 16)
            {
                floorHatchShotTrace.Add(
                    $"f{frame}:p${poseBeforeStep:X2}->${samus.Pose:X2}/" +
                    $"y${yBeforeStep:X4}->${samus.YPosition:X4}/" +
                    $"fire={runtime.Projectiles.LastFrameResult.FiredSlot?.ToString() ?? "-"}/" +
                    $"hit={runtime.Projectiles.LastFrameResult.CollisionStartedExplosion}/" +
                    $"deleted={runtime.Projectiles.LastFrameResult.ProjectileDeleted}/" +
                    $"spawn={FormatSpawn(runtime.Projectiles.LastFiredProjectileSnapshot)}/" +
                    $"cam=(${runtime.Camera?.XPosition:X4},${runtime.Camera?.YPosition:X4})/" +
                    $"slots={string.Join('|', runtime.Projectiles.Slots.Where(slot => slot.IsActive)
                        .Select(slot => $"{slot.SlotIndex}:d{slot.PackedDirection.DirectionIndex}@${slot.YPosition:X4}"))}");
            }

            // Retain only screen-boundary and one-second samples. A failed autonomous
            // route needs enough evidence to distinguish a bad directional decision from
            // collision or camera trouble, without producing a frame-by-frame log large
            // enough to bury the actual exception in CI output.
            int screenX = samus.XPosition >> 8;
            int screenY = samus.YPosition >> 8;
            if (screenX != previousScreenX || screenY != previousScreenY || frame % 60 == 0)
            {
                routeTrace.Add(
                    $"f{frame}:(${samus.XPosition:X4},${samus.YPosition:X4})/" +
                    $"p${samus.Pose:X2}/s({screenX},{screenY})");
                previousScreenX = screenX;
                previousScreenY = screenY;
            }
            if (runtime.Projectiles.LastFrameResult.FiredSlot is { } firedSlot)
            {
                firedShots++;
                int firedDirection = runtime.Projectiles.LastFiredProjectileSnapshot is { } spawn
                    ? new SamusProjectileDirectionWord(spawn.Direction).DirectionIndex
                    : runtime.Projectiles.Slots[firedSlot].PackedDirection.DirectionIndex;
                firedByDirection[firedDirection]++;
            }
            if (runtime.Projectiles.LastFrameResult.CollisionStartedExplosion)
            {
                collisionExplosions++;
                // Explosion setup preserves the direction nibble in its allocated slot.
                // Attribute the event before the slot's later animation deletes it.
                SamusProjectileSlot? collisionSlot = runtime.Projectiles.Slots.FirstOrDefault(
                    slot => slot.PackedDirection.HasLowByteLifecycleState);
                if (collisionSlot is not null)
                    collisionsByDirection[collisionSlot.PackedDirection.DirectionIndex]++;
            }

            bool moved = samus.XPosition != previousX || samus.YPosition != previousY;
            stationaryFrames = moved ? 0 : stationaryFrames + 1;
            horizontallyStationaryFrames = samus.XPosition != previousX
                ? 0
                : horizontallyStationaryFrames + 1;
            previousX = samus.XPosition;
            previousY = samus.YPosition;
        }

        if (!runtime.HasPendingDoorTransition)
        {
            Console.WriteLine($"  {roomName} route trace: {string.Join(' ', routeTrace)}");
            if (floorHatchShotTrace.Count != 0)
                Console.WriteLine($"  Floor-hatch shots: {string.Join(' ', floorHatchShotTrace)}");
            WriteCollisionMap(runtime, samus, roomName);
            PrintCollisionNeighborhood(runtime, samus);
            throw new InvalidDataException(
                $"Controller route did not leave {roomName} in {frame} frames; " +
                $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}, " +
                $"Y={samus.Kinematics.YDirection}:" +
                $"${samus.Kinematics.YSpeed:X4}.${samus.Kinematics.YSubspeed:X4}, " +
                $"stationary={stationaryFrames}, shots={firedShots}, " +
                $"collision explosions={collisionExplosions}, PLMs={runtime.Plms.ActiveCount}, " +
                $"shot directions=[{FormatCounts(firedByDirection)}], " +
                $"collision directions=[{FormatCounts(collisionsByDirection)}], " +
                $"projectiles=[{string.Join(", ", runtime.Projectiles.Slots
                    .Where(slot => slot.InstructionPointer != 0)
                    .Select(slot => $"d{slot.Direction}:(${slot.XPosition:X4},${slot.YPosition:X4})/t${slot.Type:X4}"))}].");
        }

        return new DriveResult(frame, firedShots, collisionExplosions);
    }

    private static string FormatCounts(IReadOnlyList<int> counts) => string.Join(
        ", ",
        counts.Select((count, direction) => (count, direction))
            .Where(entry => entry.count != 0)
            .Select(entry => $"{entry.direction}:{entry.count}"));

    private static string FormatSpawn(SamusProjectileSpawnSnapshot? snapshot) => snapshot is { } spawn
        ? $"d{new SamusProjectileDirectionWord(spawn.Direction).DirectionIndex}@" +
          $"(${spawn.XPosition:X4},${spawn.YPosition:X4})/" +
          $"v({spawn.XVelocity:X4},{spawn.YVelocity:X4})"
        : "-";

    private static void AssertPendingDoor(
        SuperMetroidRuntime runtime,
        ushort expectedDoorPointer,
        ushort expectedRoomPointer,
        string segmentName)
    {
        CartridgeDoorHeader door = runtime.LevelData?.PendingDoorTransition ??
            throw new InvalidDataException($"{segmentName} ended without a published door.");
        if (door.Pointer != expectedDoorPointer ||
            door.DestinationRoomPointer != expectedRoomPointer)
        {
            throw new InvalidDataException(
                $"{segmentName} reached $83:{door.Pointer:X4} -> " +
                $"$8F:{door.DestinationRoomPointer:X4}; expected " +
                $"$83:{expectedDoorPointer:X4} -> $8F:{expectedRoomPointer:X4}.");
        }
    }

    private static void AssertRoom(
        SuperMetroidRuntime runtime,
        ushort expectedRoomPointer,
        ushort expectedStatePointer,
        string roomName)
    {
        CartridgeRoomHeader room = runtime.ActiveRoom ?? throw new InvalidOperationException(
            $"{roomName} did not publish an active room.");
        if (room.Pointer != expectedRoomPointer || room.State.Pointer != expectedStatePointer)
        {
            throw new InvalidDataException(
                $"{roomName} loaded $8F:{room.Pointer:X4}/$8F:{room.State.Pointer:X4}; " +
                $"expected $8F:{expectedRoomPointer:X4}/$8F:{expectedStatePointer:X4}.");
        }
    }

    private static void PrintCollisionNeighborhood(
        SuperMetroidRuntime runtime,
        SamusState samus)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Controller route lost its level before the failure diagnostic.");
        int centerBlockX = samus.XPosition >> 4;
        int centerBlockY = samus.YPosition >> 4;
        Console.WriteLine(
            $"  Samus collision neighborhood (center block {centerBlockX:X2},{centerBlockY:X2}; " +
            $"radii {samus.Kinematics.XRadius},{samus.Kinematics.YRadius}):");

        // Print the exact packed collision metadata around the body. A door cap is a level
        // mutation owned by a PLM, whereas the edge trigger behind it is type $9. Showing
        // both nibbles here keeps the next failure tied to cartridge data rather than a
        // guess based on the composite screenshot.
        for (int y = Math.Max(0, centerBlockY - 3);
             y <= Math.Min(level.HeightInBlocks - 1, centerBlockY + 3);
             y++)
        {
            var row = new List<string>();
            for (int x = Math.Max(0, centerBlockX - 2);
                 x <= Math.Min(level.WidthInBlocks - 1, centerBlockX + 2);
                 x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                row.Add($"{x:X2}:{block.CollisionType:X1}/{block.Behavior:X2}");
            }
            Console.WriteLine($"    y={y:X2} {string.Join(' ', row)}");
        }
    }

    private static void WriteCollisionMap(
        SuperMetroidRuntime runtime,
        SamusState samus,
        string roomName)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Controller route lost its level before the collision-map diagnostic.");
        var pixels = new Rgba32[level.WidthInBlocks * level.HeightInBlocks];
        Rgba32[] collisionColors =
        [
            new(8, 8, 12),       // $0: air
            new(150, 150, 150),  // $1: slopes
            new(70, 100, 180),   // $2: air spikes
            new(100, 65, 45),    // $3: crumble
            new(90, 45, 130),    // $4: shootable
            new(35, 110, 130),   // $5: horizontal extension
            new(80, 60, 120),    // $6: unused/room-specific
            new(110, 80, 45),    // $7: bombable
            new(235, 235, 235),  // $8: solid
            new(0, 220, 255),    // $9: door trigger
            new(220, 80, 70),    // $A: spike
            new(160, 100, 30),   // $B: special collision
            new(40, 170, 80),    // $C: shootable BTS/door cap
            new(40, 130, 100),   // $D: vertical extension
            new(190, 70, 190),   // $E: grapple
            new(230, 170, 45),   // $F: bomb block
        ];

        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                pixels[y * level.WidthInBlocks + x] = collisionColors[block.CollisionType];
            }
        }

        // Magenta is reserved for Samus so the last autonomous position remains obvious
        // even when she overlaps a slope or door family with its own diagnostic color.
        int samusX = Math.Clamp(samus.XPosition >> 4, 0, level.WidthInBlocks - 1);
        int samusY = Math.Clamp(samus.YPosition >> 4, 0, level.HeightInBlocks - 1);
        pixels[samusY * level.WidthInBlocks + samusX] = new Rgba32(255, 0, 255);

        string safeName = roomName.Replace(' ', '-');
        string path = Path.Combine(
            "csharp", "test-temp", "early-controller", $"{safeName}-collision.png");
        PngWriter.WriteRgba(
            path,
            level.WidthInBlocks,
            level.HeightInBlocks,
            pixels,
            scale: 8);
        Console.WriteLine($"  Wrote collision map to {Path.GetFullPath(path)}.");
    }

    private static void PrintDoorBlocks(SuperMetroidRuntime runtime, string roomName)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            $"{roomName} has no level data.");
        var doors = new List<string>();
        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType == 9)
                    doors.Add($"({x:X2},{y:X2})=${block.Behavior:X2}");
            }
        }
        Console.WriteLine($"  {roomName} door blocks: {string.Join(' ', doors)}");
    }

    private static void PrintPlmPopulation(ISnesAddressSpace bus, ushort populationPointer)
    {
        var records = new List<string>();
        ushort cursor = populationPointer;
        for (int index = 0; index < 64; index++, cursor = unchecked((ushort)(cursor + 6)))
        {
            int address = 0x8f0000 | cursor;
            ushort header = (ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8));
            if (header == 0)
                break;
            byte x = bus.ReadByte(address + 2);
            byte y = bus.ReadByte(address + 3);
            ushort argument = (ushort)(bus.ReadByte(address + 4) | (bus.ReadByte(address + 5) << 8));
            records.Add($"${header:X4}@({x:X2},{y:X2})/${argument:X4}");
        }
        Console.WriteLine($"  PLM population $8F:{populationPointer:X4}: {string.Join(' ', records)}");
    }

    private readonly record struct DriveResult(
        int Frames,
        int FiredShots,
        int CollisionExplosions);
}
