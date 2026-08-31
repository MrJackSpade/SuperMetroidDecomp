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

        PrintDoorBlocks(bus, runtime, "Landing Site");
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Gunship landing did not retain Samus.");
        DriveResult landing = DriveUntilDoor(bus, runtime, "Landing Site", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8916, 0x92fd, "Landing Site -> Parlor");

        // Loading a collision-published door is the normal outer game-state action. The
        // audit does not identify the destination to the loader and does not edit placement;
        // bank-$83's record supplies both, exactly as the desktop frontend does.
        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x92fd, 0x9314, "Parlor");
        PrintDoorBlocks(bus, runtime, "Parlor");
        Console.WriteLine(
            $"  Parlor camera metadata: scrolls={string.Join(',', runtime.Camera!.Scrolls.Storage.ToArray().Take(runtime.Camera.Scrolls.LogicalCellCount).Select(value => value.ToString("X2")))} " +
            $"scrollers=(${runtime.ActiveRoom!.UpScroller:X2},${runtime.ActiveRoom.DownScroller:X2}) " +
            $"entry=(${runtime.Camera.XPosition:X4},${runtime.Camera.YPosition:X4}) " +
            $"main=$8F:{runtime.ActiveRoom.State.MainCodePointer:X4} setup=$8F:{runtime.ActiveRoom.State.SetupCodePointer:X4}.");
        PrintPlmPopulation(bus, runtime.ActiveRoom.State.PlmPointer);
        Console.WriteLine(
            $"  Loaded scroll PLMs: {string.Join(' ', runtime.Plms.ScrollPlms.Select(scroll => $"{scroll.BlockIndex}/${scroll.DataPointer:X4}"))}.");

        DriveResult parlor = DriveUntilDoor(bus, runtime, "Parlor", maximumFrames: 3600);
        AssertPendingDoor(runtime, 0x898e, 0x96ba, "Parlor -> Climb");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x96ba, 0x96d1, "Climb");
        PrintDoorBlocks(bus, runtime, "Climb");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult climb = DriveUntilDoor(bus, runtime, "Climb", maximumFrames: 6000);
        AssertPendingDoor(runtime, 0x8b62, 0x975c, "Climb -> Pit");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x975c, 0x976d, "Pit");
        PrintDoorBlocks(bus, runtime, "Pit");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult pit = DriveUntilDoor(bus, runtime, "Pit", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8b86, 0x97b5, "Pit -> elevator room");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x97b5, 0x97c6, "Elevator to Blue Brinstar");
        PrintDoorBlocks(bus, runtime, "Elevator to Blue Brinstar");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        DriveResult elevator = DriveUntilDoor(
            bus,
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
        PrintDoorBlocks(bus, runtime, "Morph Ball room");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        Console.WriteLine(
            $"  Morph Ball scrolls: {string.Join(',', runtime.Camera!.Scrolls.Storage[..runtime.Camera.Scrolls.LogicalCellCount].ToArray().Select(value => value.ToString("X2")))}.");
        PrintCollisionRegion(runtime, 0x40, 0x59, 0x20, 0x2d);
        DriveResult morphBall = DriveUntilDoor(
            bus,
            runtime,
            "Morph Ball room",
            maximumFrames: 5000);
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) ||
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.MorphBall))
        {
            throw new InvalidDataException(
                "Morph Ball room exited without the cartridge collectible setting both item words.");
        }
        // The elevator at X=$57 is an optional return to Crateria. Normal new-game
        // progression keeps travelling east through the same room and enters the
        // cartridge's pre-Missiles room through door-list entry one.
        AssertPendingDoor(runtime, 0x8eaa, 0x9f11, "Morph Ball -> pre-Missiles");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x9f11, 0x9f23, "Pre-Missiles room");
        PrintDoorBlocks(bus, runtime, "Pre-Missiles room");
        PrintPlmPopulation(bus, runtime.ActiveRoom!.State.PlmPointer);
        PrintCollisionFeatureRows(runtime, 0x00, 0x0f, 0x12, 0x1a);
        DriveResult preMissiles = DriveUntilDoor(
            bus,
            runtime,
            "Pre-Missiles room",
            maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8eda, 0xa107, "Pre-Missiles -> first Missile");

        runtime.LoadPendingDoorDestination();
        CartridgeRoomHeader firstMissileRoom = runtime.ActiveRoom ??
            throw new InvalidDataException("First Missile door did not publish its room.");
        AssertRoom(runtime, 0xa107, 0xa114, "First Missile room");
        PrintDoorBlocks(bus, runtime, "First Missile room");
        PrintPlmPopulation(bus, firstMissileRoom.State.PlmPointer);
        DriveResult firstMissile = DriveUntilDoor(
            bus,
            runtime,
            "First Missile room",
            maximumFrames: 2400);
        if (samus.MaxMissiles != 5 || samus.Missiles != 5)
        {
            throw new InvalidDataException(
                $"First Missile collectible left ammo at {samus.Missiles}/{samus.MaxMissiles}; " +
                "expected the cartridge's initial 5/5 capacity.");
        }
        AssertPendingDoor(runtime, 0x8fa6, 0x9f11, "First Missile -> pre-Missiles");

        Console.WriteLine(
            $"Controller route: gunship {landingFrames} frames; Landing Site -> Parlor " +
            $"after {landing.Frames} ordinary gameplay frames; Parlor -> Climb after " +
            $"{parlor.Frames} more frames; Climb -> Pit after {climb.Frames} more; Pit exit " +
            $"after {pit.Frames} more; elevator descent after {elevator.Frames} more; " +
            $"Morph Ball acquired and the pre-Missiles door reached after " +
            $"{morphBall.Frames} more; pre-Missiles exited after " +
            $"{preMissiles.Frames} more; first Missile collected and room exited after " +
            $"{firstMissile.Frames} more at Samus " +
            $"(${samus.XPosition:X4},${samus.YPosition:X4}).");
        return 0;
    }

    private static DriveResult DriveUntilDoor(
        ISnesAddressSpace bus,
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
        int climbJumpHoldFrames = 0;
        int climbGroundedRunupFrames = 0;
        int climbAirborneFrames = 0;
        int climbSteeringDelayFrames = 0;
        int climbApproachX = 0x0178;
        int climbTargetSurfaceY = 0;
        int? climbTargetX = null;
        bool climbRequiresGroundApproach = false;
        bool climbJumpReady = true;
        bool climbWasAirborne = false;
        // Landing Site, Parlor, and Climb begin by travelling left or descending
        // from a leftward approach. Pit is entered through its left cap and the
        // cartridge route continues to the right.
        bool returningWithMorphBallAtEntry = samus.CollectedItems.HasAny(
            SamusEquipmentFlags.MorphBall);
        SnesButton horizontalDirection = roomName switch
        {
            "Pit" when returningWithMorphBallAtEntry => SnesButton.Left,
            "Pit" or "Elevator to Blue Brinstar" or "Pre-Missiles room" => SnesButton.Right,
            _ => SnesButton.Left,
        };
        var firedByDirection = new int[16];
        var collisionsByDirection = new int[16];
        var floorHatchShotTrace = new List<string>();
        var morphBallShotTrace = new List<string>();
        var morphBallCollisionTrace = new List<string>();
        int frame = 0;
        var routeTrace = new List<string>();
        int previousScreenX = -1;
        int previousScreenY = -1;
        int previousScrollPlmCount = runtime.Plms.ScrollPlms.Count;
        ElevatorActorStatus previousElevatorStatus = runtime.Enemies.ElevatorStatus;

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
                !(roomName == "Climb" && returningWithMorphBallAtEntry) &&
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
            bool descendingPreMissiles = roomName == "Pre-Missiles room";
            bool collectingFirstMissile = roomName == "First Missile room";
            bool climbAlignedForJump = false;
            bool climbStartingJump = false;
            SnesButton climbJumpDirection = 0;
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
                bool returningWithMorphBall = samus.CollectedItems.HasAny(
                    SamusEquipmentFlags.MorphBall);
                if (returningWithMorphBall)
                {
                    // On the return trip the destination actor first carries locked Samus
                    // back to its rest point. Left input is harmless during that ownership
                    // window and becomes ordinary movement toward Pit as soon as the actor
                    // restores controls. Periodic shots reopen any cartridge door cap.
                    input = (ushort)SnesButton.Left;
                    if (frame % 24 == 0)
                        input |= (ushort)SnesButton.X;
                }
                else
                {
                    // The elevator actor waits for a newly-pressed direction only after
                    // the floor's type-$9 pseudo-door collision has set $0E16. Walk onto
                    // the two cartridge-authored platform columns, then pulse Down with
                    // released frames so the actor owns the departure.
                    input = samus.XPosition switch
                    {
                        < 0x0074 => (ushort)SnesButton.Right,
                        > 0x008c => (ushort)SnesButton.Left,
                        _ => frame % 30 == 0 ? (ushort)SnesButton.Down : (ushort)0,
                    };
                }
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
                    if (samus.XPosition > 0x0470)
                    {
                        input = (ushort)SnesButton.Left;
                        if (frame % 60 < 30)
                            input |= (ushort)SnesButton.A;
                    }
                    else
                    {
                        // The item sits above a small solid pedestal. Once horizontally
                        // aligned, counter any carried running momentum from either side,
                        // then use repeatable neutral jumps through its collision box. The
                        // cartridge's acceleration/deceleration still owns the correction.
                        input = samus.XPosition switch
                        {
                            < 0x0440 => (ushort)SnesButton.Right,
                            > 0x0458 => (ushort)SnesButton.Left,
                            _ => frame % 60 < 25
                                ? (ushort)SnesButton.A
                                : (ushort)0,
                        };
                    }
                }
                else
                {
                    RoomCollisionBlock returnBlock = runtime.LevelData!
                        .GetCollisionBlock(0x4c, 0x2c);
                    bool returnBlockStillSolid = returnBlock.CollisionType == 0x0c &&
                        returnBlock.Behavior == 0x04;
                    bool isMorphBall = SamusState.IsGroundedMorphBallPose(samus.Pose) ||
                        SamusState.IsAirborneMorphBallPose(samus.Pose);
                    if (returnBlockStillSolid)
                    {
                        // The return tunnel passes underneath the four-column wall at
                        // X=$4C..$4F. Its cartridge-authored beam block is the floor-level
                        // western cap at (4C,2C), so Samus must first cross the ruined
                        // pedestal pieces and stand immediately west of the wall. Shooting
                        // diagonally from the item pedestal is not equivalent: that line
                        // correctly intersects the wall's ordinary solids several rows
                        // above the tunnel. The retail default maps Aim Down to L (R is
                        // Aim Up), and the projectile/terrain systems alone decide whether
                        // this nearby downward shot actually opens the cap.
                        input = samus.XPosition switch
                        {
                            < 0x04a0 => (ushort)SnesButton.Right,
                            > 0x04b0 => (ushort)SnesButton.Left,
                            _ when samus.IsFacingLeft(bus) => (ushort)SnesButton.Right,
                            _ => (ushort)SnesButton.L,
                        };
                        // Ordinary held jumps clear the two one-block pedestal fragments.
                        // Stop jumping once aligned with the tunnel so the downward beam
                        // begins from the grounded pose expected by the room geometry.
                        if (samus.XPosition < 0x04a0 && frame % 60 < 30)
                            input |= (ushort)SnesButton.A;
                        if (samus.XPosition is >= 0x04a0 and <= 0x04b0 && frame % 12 == 0)
                            input |= (ushort)SnesButton.X;
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
                        // The east exit is an ordinary blue cap at X=$7E. Keep the same
                        // periodic Fire policy used elsewhere on the route once Samus is
                        // in its final screen; the projectile/PLM collision path remains
                        // solely responsible for opening it.
                        if (samus.XPosition >= 0x0780 && frame % 24 == 0)
                            input |= (ushort)SnesButton.X;
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
            else if (descendingPreMissiles)
            {
                if (samus.YPosition < 0x0100)
                {
                    // Permanent shot blocks cover the opening at row $0A, preceded by a
                    // one-block lip at X=$04. Use a complete jump/release cycle to mount
                    // the lip, then establish an aerial pose before holding Down for the
                    // cartridge's straight-down shot. No block is removed here.
                    input = samus.XPosition < 0x0060
                        ? (ushort)SnesButton.Right
                        : samus.XPosition > 0x0090
                            ? (ushort)SnesButton.Left
                            : (ushort)0;
                    int floorShotCycle = frame % 60;
                    if (floorShotCycle < 8)
                        input |= (ushort)SnesButton.A;
                    else if (floorShotCycle < 50)
                    {
                        input |= (ushort)SnesButton.Down;
                        if (floorShotCycle % 6 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                }
                else
                {
                    // The west door is not reachable over the ledge. Cartridge collision
                    // rows $15-$18 contain a three-block dividing wall, with the only drop
                    // at X=$08/$09. Walk right off that ledge, let gravity put Samus below
                    // the wall, then walk left through the lower passage. These thresholds
                    // choose buttons only; the runtime remains responsible for the fall.
                    bool belowDividingWall = samus.YPosition >= 0x0170;
                    SamusMovementType movement = samus.ReadMovementKind(bus);
                    bool morphed = movement is SamusMovementType.MorphBallGround or
                        SamusMovementType.MorphBallFalling;
                    bool touchingWestDoorCap = samus.XPosition <= 0x0030;
                    if (!belowDividingWall)
                    {
                        input = (ushort)SnesButton.Right;
                    }
                    else if (!morphed && !touchingWestDoorCap)
                    {
                        // The lower passage is one block tall, so this is the route's first
                        // mandatory Morph Ball use. A release interval followed by Down
                        // supplies the same stand -> crouch -> morph edges as player input.
                        input = frame % 60 < 20
                            ? (ushort)0
                            : (ushort)SnesButton.Down;
                    }
                    else if (morphed && !touchingWestDoorCap)
                    {
                        input = (ushort)SnesButton.Left;
                    }
                    else if (morphed || movement == SamusMovementType.PostureTransition)
                    {
                        // The arm cannon is unavailable in Morph Ball form. Unmorph in the
                        // west chamber's full-height clearance before addressing the cap.
                        input = (ushort)SnesButton.Up;
                    }
                    else
                    {
                        // Face left and pulse Fire at the cartridge-authored
                        // type-$C/BTS-$41 blue cap.
                        input = (ushort)SnesButton.Left;
                        if (frame % 24 == 0)
                            input |= (ushort)SnesButton.X;
                    }
                }
            }
            else if (collectingFirstMissile)
            {
                // Enter from the east, touch the sole cartridge Missile PLM at block
                // (4,7), wait through its message-owned input lock, and return east. The
                // capacity word—not a timer or coordinate—selects the return leg.
                input = samus.MaxMissiles == 0
                    ? (ushort)SnesButton.Left
                    : (ushort)SnesButton.Right;

                // Header $EF2F is the Missile entry in the Chozo-orb table, not the
                // exposed-item table. Fire opens the shell; continued Left then touches
                // the visible tank after the cartridge's three burst frames.
                if (samus.MaxMissiles == 0 && frame % 24 == 0)
                    input |= (ushort)SnesButton.X;
                else if (samus.MaxMissiles != 0 && frame % 24 == 0)
                    input |= (ushort)SnesButton.X;
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
                bool returningUpClimb = roomName == "Climb" &&
                    returningWithMorphBallAtEntry;
                bool descendingLowerClimb = roomName == "Climb" &&
                    !returningUpClimb &&
                    samus.YPosition is >= 0x0700 and < 0x0800;
                bool approachingPitDoor = roomName == "Climb" &&
                    !returningUpClimb &&
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
                else if (returningUpClimb)
                {
                    // Select the nearest authored floor-slope span above the current
                    // support. The driver still supplies buttons only; slope quadrants,
                    // acceleration, gravity, landings, PLM mutation, and the eventual door
                    // trigger remain entirely inside the translated game systems.
                    bool airborne = samus.Kinematics.YDirection != 0;
                    bool waitingForLandingAnimation = !airborne &&
                        (SamusState.IsRightFacingLandingPose(samus.Pose) ||
                         SamusState.IsLeftFacingLandingPose(samus.Pose));
                    if (!airborne)
                    {
                        if (!waitingForLandingAnimation && climbTargetX is null)
                        {
                            ClimbPlatformTarget platform = FindNextClimbPlatformCenter(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus);
                            climbTargetX = platform.LandingX;
                            climbTargetSurfaceY = platform.SurfaceY;
                            int launchDistance = Math.Abs(platform.LandingX - samus.XPosition);
                            // Samus is only ten pixels wide, but a square slope's solid
                            // quadrant can project much farther than the block selected as
                            // the eventual landing sample. Treat transfers under three
                            // blocks as under-ledged and clear the whole underside first.
                            int targetBlockLeft = platform.ShadowLeftX;
                            int targetBlockRight = platform.ShadowRightX;
                            bool directlyUnderTarget = samus.XPosition >=
                                targetBlockLeft - samus.Kinematics.XRadius &&
                                samus.XPosition <= targetBlockRight + samus.Kinematics.XRadius;
                            // A normal jump cannot pass through the selected ledge. The
                            // approach coordinate is the nearest X whose complete body is
                            // just outside its tile shadow. If Samus begins under that
                            // shadow, reach the coordinate on the ground first. Otherwise
                            // jump immediately and build horizontal speed toward it during
                            // ascent; the hardest retail gap offers only one pixel of
                            // vertical clearance at the apex.
                            climbApproachX = FindClearClimbAscentX(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus,
                                platform);
                            bool currentLaneIsClear = IsClimbAscentLaneClear(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus,
                                platform,
                                samus.XPosition);
                            climbRequiresGroundApproach = directlyUnderTarget ||
                                !currentLaneIsClear;
                            // Long transfers need immediate acceleration. On short transfers
                            // that same input reaches the platform's vertical side before
                            // Samus' feet clear its sloped surface, so defer steering until
                            // later in the cartridge-authored arc.
                            climbSteeringDelayFrames = Math.Clamp(
                                28 - launchDistance / 3,
                                0,
                                24);
                            if (directlyUnderTarget)
                                climbSteeringDelayFrames = 0;
                        }
                        int groundTargetX = climbApproachX;
                        bool reachedLaunchX = Math.Abs(
                            samus.XPosition - groundTargetX) <= 1;
                        SnesButton launchDirection = samus.XPosition <
                            (climbTargetX ?? 0x0178)
                            ? SnesButton.Right
                            : SnesButton.Left;
                        horizontalDirection = reachedLaunchX
                            ? launchDirection
                            : samus.XPosition < groundTargetX
                                ? SnesButton.Right
                                : SnesButton.Left;

                        // Jump during `$A4/$A5/$A6/$A7` legitimately selects a landing-
                        // interrupt target. Let that animation settle before issuing the
                        // fresh directional edge that selects spin-jump art.
                        input = waitingForLandingAnimation
                            ? (ushort)0
                            : (ushort)(horizontalDirection | SnesButton.B);
                        if (!waitingForLandingAnimation)
                            climbGroundedRunupFrames++;
                        climbAlignedForJump = !waitingForLandingAnimation &&
                            (!climbRequiresGroundApproach || reachedLaunchX) &&
                            // One direction-only frame establishes running state before
                            // the new Jump edge. Pressing both on the first supported frame
                            // selects normal-jump `$4B/$4D`; the authored Climb gaps rely on
                            // spin-jump `$19/$1A` and its twelve-pixel vertical radius.
                            climbGroundedRunupFrames >= 2;
                        climbJumpDirection = launchDirection;
                    }
                    else
                    {
                        climbAirborneFrames++;
                        int targetX = climbTargetX ?? 0x0178;
                        // Ascent first stays outside the ledge's tile shadow. Once Samus'
                        // feet are above its probed surface, crossing over it is physically
                        // possible and should begin immediately; waiting for the apex left
                        // too little horizontal travel before the landing row.
                        bool feetAboveTargetSurface =
                            samus.YPosition + samus.Kinematics.YRadius < climbTargetSurfaceY;
                        int steeringTargetX = samus.Kinematics.YDirection == 1 &&
                            !feetAboveTargetSurface
                            ? climbApproachX
                            : targetX;
                        bool horizontallyAligned = Math.Abs(
                            steeringTargetX - samus.XPosition) <= 1;
                        if (!horizontallyAligned)
                        {
                            horizontalDirection = samus.XPosition < steeringTargetX
                                ? SnesButton.Right
                                : SnesButton.Left;
                        }

                        // A one-frame directional launch selects spin art. Releasing the
                        // direction then clears base X speed through `$90:9078`; reapply it
                        // only after the distance-derived delay so a nearby ledge is reached
                        // around the apex rather than struck from underneath.
                        input = (ushort)SnesButton.B;
                        if (climbAirborneFrames > climbSteeringDelayFrames &&
                            !horizontallyAligned)
                        {
                            input |= (ushort)horizontalDirection;
                        }
                    }

                }
                else
                {
                    input = (ushort)horizontalDirection;
                }
                // Fire cancels spin into normal-jump gun art, which cannot execute the
                // block wall-jump check. Climb's upward exit is an exposed type-$9 trigger,
                // so the ascent deliberately preserves spin instead of firing at it.
                if (frame % 24 == 0 && !returningUpClimb)
                    input |= (ushort)SnesButton.X;
                if (returningUpClimb && samus.Kinematics.YDirection == 0 &&
                    climbAlignedForJump && climbJumpReady)
                {
                    // A room-global modulo pulse made later jumps arbitrarily short: a
                    // landing near the end of the pulse could receive only a handful of
                    // held frames, invoking the game's variable-height cutoff. Start one
                    // complete button hold from the actual supported launch instead.
                    climbJumpHoldFrames = 48;
                    climbJumpReady = false;
                    climbStartingJump = true;
                }
                if (returningUpClimb && climbJumpHoldFrames > 0)
                {
                    // The directional chord is needed only on the new Jump edge to select
                    // spin-jump art and seed its horizontal motion. On later frames the
                    // target-X steering above must be free to reverse direction around a
                    // ledge; holding the launch direction here would press Left+Right at
                    // once and let the ROM priority table defeat that correction.
                    input |= (ushort)SnesButton.A;
                    if (climbStartingJump)
                        input |= (ushort)climbJumpDirection;
                    climbJumpHoldFrames--;
                }
                if ((roomName != "Parlor" || samus.YPosition < 0x0100) &&
                    !returningUpClimb &&
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
            if (roomName == "Climb" && returningWithMorphBallAtEntry)
            {
                if (runtime.LastAerialSamusMovement is { WallJumpTriggered: true })
                {
                    // The trigger frame installs `$83/$84` and its upward speed, but the
                    // following wall-jump movement frames still use ordinary variable-jump
                    // cutoff. Continue holding A or native quite correctly truncates the
                    // launch to a tiny hop on the very next frame.
                    climbJumpHoldFrames = 48;
                }
                bool climbIsAirborne = samus.Kinematics.YDirection != 0;
                if (climbWasAirborne && !climbIsAirborne)
                {
                    // A variable-height hold belongs only to the arc that created it. If
                    // its remaining frames leak across landing, the next launch never has
                    // a newly-pressed A edge and the ROM table produces a tiny neutral hop.
                    climbJumpHoldFrames = 0;
                    climbJumpReady = true;
                    climbGroundedRunupFrames = 0;
                    climbAirborneFrames = 0;
                    climbTargetX = null;
                }
                climbWasAirborne = climbIsAirborne;
            }
            if (approachingBlueBrinstarElevator &&
                (runtime.Enemies.ElevatorStatus != previousElevatorStatus ||
                 runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.None ||
                 runtime.HasPendingDoorTransition))
            {
                Console.WriteLine(
                    $"  Elevator f{frame}: status={previousElevatorStatus}->" +
                    $"{runtime.Enemies.ElevatorStatus}, flags=${runtime.Enemies.ElevatorFlags:X4}, " +
                    $"event={runtime.Enemies.LastElevatorEvent}, input=${input:X4}, " +
                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), " +
                    $"pending={runtime.LevelData?.PendingDoorTransition?.Pointer.ToString("X4") ?? "-"}.");
                previousElevatorStatus = runtime.Enemies.ElevatorStatus;
            }
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
            bool detailedReturnedClimbSample = roomName == "Climb" &&
                returningWithMorphBallAtEntry &&
                frame <= 260 && frame % 10 == 0;
            if (screenX != previousScreenX || screenY != previousScreenY ||
                frame % 60 == 0 || detailedReturnedClimbSample)
            {
                routeTrace.Add(
                    $"f{frame}:(${samus.XPosition:X4},${samus.YPosition:X4})/" +
                    $"p${samus.Pose:X2}/s({screenX},{screenY})" +
                    (roomName == "Climb" && returningWithMorphBallAtEntry
                        ? $"/lock={samus.InputLocked}/" +
                          $"target={climbTargetX?.ToString("X4") ?? "-"}/" +
                          $"surface={climbTargetSurfaceY:X4}/r={samus.Kinematics.YRadius}/" +
                          $"approach={climbApproachX:X4}/dir={horizontalDirection}/in=${input:X4}/" +
                          $"vcol={runtime.LastAerialSamusMovement?.Vertical?.Collided}/" +
                          $"vdisp={runtime.LastAerialSamusMovement?.Vertical?.AcceptedDisplacement}/" +
                          $"land={runtime.LastAerialSamusMovement?.Landed}"
                        : "") +
                    (detailedReturnedClimbSample
                        ? $"/af{samus.AnimationFrame}/in${input:X4}/" +
                          $"wall={runtime.LastAerialSamusMovement?.WallContact}/" +
                          $"hcol={runtime.LastAerialSamusMovement?.Horizontal.Collided}/" +
                          $"yd={samus.Kinematics.YDirection}/" +
                          $"ys=${samus.Kinematics.YSpeed:X4}.${samus.Kinematics.YSubspeed:X4}"
                        : ""));
                previousScreenX = screenX;
                previousScreenY = screenY;
            }
            if (runtime.Projectiles.LastFrameResult.FiredSlot is { } firedSlot)
            {
                firedShots++;
                if (crossingMorphBallRoom && morphBallShotTrace.Count < 24)
                {
                    morphBallShotTrace.Add(
                        $"f{frame}:samus=(${samus.XPosition:X4},${samus.YPosition:X4})/" +
                        $"p${samus.Pose:X2}/spawn={FormatSpawn(runtime.Projectiles.LastFiredProjectileSnapshot)}/" +
                        $"cam=(${runtime.Camera?.XPosition:X4},${runtime.Camera?.YPosition:X4})/" +
                        $"hit={runtime.Projectiles.LastFrameResult.CollisionStartedExplosion}");
                }
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
                if (crossingMorphBallRoom && morphBallCollisionTrace.Count < 24)
                {
                    SamusProjectileSlot? impact = runtime.Projectiles.Slots.FirstOrDefault(
                        slot => slot.PackedType.Family == SamusProjectileFamily.BeamExplosion);
                    if (impact is not null)
                    {
                        RoomCollisionBlock impactBlock = runtime.LevelData!.GetCollisionBlockAtPixel(
                            impact.XPosition,
                            impact.YPosition);
                        morphBallCollisionTrace.Add(
                            $"f{frame}:d{impact.PackedDirection.DirectionIndex}@" +
                            $"(${impact.XPosition:X4},${impact.YPosition:X4})/" +
                            $"r({impact.XRadius},{impact.YRadius})/" +
                            $"block={impactBlock.Index}:" +
                            $"{impactBlock.CollisionType:X1}/{impactBlock.Behavior:X2}");
                    }
                }
            }

            bool moved = samus.XPosition != previousX || samus.YPosition != previousY;
            stationaryFrames = moved ? 0 : stationaryFrames + 1;
            horizontallyStationaryFrames = samus.XPosition != previousX
                ? 0
                : horizontallyStationaryFrames + 1;
            previousX = samus.XPosition;
            previousY = samus.YPosition;
        }

        if (roomName == "Climb" && returningWithMorphBallAtEntry)
            Console.WriteLine($"  Returned Climb route trace: {string.Join(' ', routeTrace)}");

        if (!runtime.HasPendingDoorTransition)
        {
            Console.WriteLine($"  {roomName} route trace: {string.Join(' ', routeTrace)}");
            if (floorHatchShotTrace.Count != 0)
                Console.WriteLine($"  Floor-hatch shots: {string.Join(' ', floorHatchShotTrace)}");
            if (morphBallShotTrace.Count != 0)
                Console.WriteLine($"  Morph-Ball shots: {string.Join(' ', morphBallShotTrace)}");
            if (morphBallCollisionTrace.Count != 0)
                Console.WriteLine($"  Morph-Ball impacts: {string.Join(' ', morphBallCollisionTrace)}");
            WriteCollisionMap(runtime, samus, roomName);
            PrintCollisionNeighborhood(runtime, samus);
            throw new InvalidDataException(
                $"Controller route did not leave {roomName} in {frame} frames; " +
                $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}, " +
                $"inputLocked={samus.InputLocked}, " +
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

    private static ClimbPlatformTarget FindNextClimbPlatformCenter(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus)
    {
        const int shaftLeftBlock = 0x12;
        const int shaftRightBlock = 0x1d;
        const int highestOrdinaryPlatformRow = 0x06;
        const int upwardSearchRows = 12;
        const int topDoorCenterX = 0x0178;
        const int minimumSurfaceRise = 8;
        const int maximumNormalJumpRise = 112;

        // BTS bit seven is a slope mirror bit, not a universal floor/ceiling flag. Its
        // meaning depends on the low five-bit shape index, so classifying `$82` as a
        // ceiling and `$02` as a floor (or vice versa) eventually chooses an underside.
        // Ask the actual bank-$94 downward dispatcher instead. More importantly, probe
        // every possible Samus center pixel rather than each block center. Square slopes
        // deliberately make only one quadrant solid; a block-center target can therefore
        // put one foot over the hollow half even though the block itself is a valid floor.
        // The disposable body has Samus' real radii, so any accepted X coordinate already
        // includes the horizontal clearance that a genuine landing requires.
        int currentSurfaceY = samus.YPosition + samus.Kinematics.YRadius;
        int supportRow = currentSurfaceY >> 4;
        if (supportRow <= highestOrdinaryPlatformRow)
            return new ClimbPlatformTarget(
                topDoorCenterX,
                SurfaceY: highestOrdinaryPlatformRow * 16,
                ShadowLeftX: 0x0160,
                ShadowRightX: 0x019f);

        int bestCenter = 0;
        int bestSurfaceY = 0;
        int bestShadowLeftX = 0;
        int bestShadowRightX = 0;
        int bestRise = int.MaxValue;
        int bestHorizontalDistance = int.MaxValue;
        int firstRow = supportRow - 1;
        int lastRow = Math.Max(0, supportRow - upwardSearchRows);
        for (int blockY = firstRow; blockY >= lastRow; blockY--)
        {
            var rowSamples = new List<ClimbLandingSample>();
            int firstCenterX = shaftLeftBlock * 16 + samus.Kinematics.XRadius;
            int lastCenterX = (shaftRightBlock + 1) * 16 - samus.Kinematics.XRadius - 1;
            for (int centerX = firstCenterX; centerX <= lastCenterX; centerX++)
            {
                int blockX = centerX >> 4;
                RoomCollisionBlock candidate = level.GetCollisionBlock(blockX, blockY);
                // Bit seven mirrors the slope shape; it does not mean "ceiling." Climb
                // uses mirrored records for required intermediate ledges, so no BTS-bit
                // heuristic belongs here. The direction-aware bank-$94 probe below is the
                // authority on whether Samus can stand at this exact body coordinate.
                if (candidate.CollisionType != 1)
                    continue;

                var probe = new SamusKinematicsState
                {
                    XPosition = unchecked((ushort)centerX),
                    YPosition = unchecked((ushort)(blockY * 16 - samus.Kinematics.YRadius - 1)),
                    XRadius = samus.Kinematics.XRadius,
                    YRadius = samus.Kinematics.YRadius,
                    YDirection = 2,
                    HorizontalSlopeCollisionEnable = samus.Kinematics.HorizontalSlopeCollisionEnable,
                };
                BlockMoveResult result = SamusBlockCollision.MoveVertical(
                    bus,
                    level,
                    probe,
                    // Bank $94 samples the destination row rather than sweeping every
                    // crossed row. End with the leading boundary in this exact block;
                    // extending one pixel farther would test the row below and falsely
                    // reject a perfectly valid slope candidate.
                    displacement: 17 << 16,
                    scanLeftToRight: true,
                    includeSolidEnemies: false,
                    plms: plms,
                    publishDoorSideEffects: false);
                if (!result.Collided || result.CollisionBlock?.Index != candidate.Index)
                    continue;

                int surfaceY = probe.YPosition + probe.YRadius;
                int rise = currentSurfaceY - surfaceY;
                if (rise < minimumSurfaceRise || rise > maximumNormalJumpRise)
                    continue;
                rowSamples.Add(new ClimbLandingSample(centerX, surfaceY));
            }

            // A square-slope platform commonly spans two BTS records and changes its
            // surface Y across the run. Comparing individual pixels chooses the lowest
            // extreme of that slope. Collapse consecutive accepted body centers into one
            // authored platform and aim at its middle, where both feet have useful margin.
            for (int runStart = 0; runStart < rowSamples.Count;)
            {
                int runEnd = runStart;
                while (runEnd + 1 < rowSamples.Count &&
                    rowSamples[runEnd + 1].X == rowSamples[runEnd].X + 1)
                {
                    runEnd++;
                }

                ClimbLandingSample middle = rowSamples[(runStart + runEnd) / 2];
                int middleRise = currentSurfaceY - middle.SurfaceY;
                int horizontalDistance = Math.Abs(middle.X - samus.XPosition);
                if (middleRise < bestRise ||
                    (middleRise == bestRise &&
                     horizontalDistance < bestHorizontalDistance))
                {
                    bestCenter = middle.X;
                    bestSurfaceY = middle.SurfaceY;
                    bestShadowLeftX = (rowSamples[runStart].X >> 4) * 16;
                    bestShadowRightX = (rowSamples[runEnd].X >> 4) * 16 + 15;
                    bestRise = middleRise;
                    bestHorizontalDistance = horizontalDistance;
                }

                runStart = runEnd + 1;
            }
        }

        if (bestRise != int.MaxValue)
        {
            return new ClimbPlatformTarget(
                bestCenter,
                bestSurfaceY,
                bestShadowLeftX,
                bestShadowRightX);
        }

        throw new InvalidDataException(
            $"Climb collision dispatcher found no reachable floor within " +
            $"{upwardSearchRows} rows above surface ${currentSurfaceY:X4} at " +
            $"Samus X ${samus.XPosition:X4}.");
    }

    private static int FindClearClimbAscentX(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        ClimbPlatformTarget platform)
    {
        const int shaftLeftBlock = 0x12;
        const int shaftRightBlock = 0x1d;
        int firstCenterX = shaftLeftBlock * 16 + samus.Kinematics.XRadius;
        int lastCenterX = (shaftRightBlock + 1) * 16 - samus.Kinematics.XRadius - 1;
        int bestX = 0;
        int bestDistance = int.MaxValue;
        for (int centerX = firstCenterX; centerX <= lastCenterX; centerX++)
        {
            // Stay wholly outside the destination platform until Samus' feet rise above
            // it. The upward probe also rejects any intervening mirrored slope, wall, or
            // PLM extension that would turn this nominal edge into a blocked ascent lane.
            bool outsideDestinationShadow =
                centerX + samus.Kinematics.XRadius < platform.ShadowLeftX ||
                centerX - samus.Kinematics.XRadius > platform.ShadowRightX;
            if (!outsideDestinationShadow || !IsClimbAscentLaneClear(
                    bus,
                    level,
                    plms,
                    samus,
                    platform,
                    centerX))
            {
                continue;
            }

            int distance = Math.Abs(centerX - samus.XPosition);
            if (distance < bestDistance)
            {
                bestX = centerX;
                bestDistance = distance;
            }
        }

        if (bestDistance != int.MaxValue)
            return bestX;

        throw new InvalidDataException(
            $"Climb found platform at (${platform.LandingX:X4}," +
            $"${platform.SurfaceY:X4}) but no collision-clear ascent lane.");
    }

    private static bool IsClimbAscentLaneClear(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        ClimbPlatformTarget platform,
        int centerX)
    {
        const ushort spinJumpVerticalRadius = 12;
        int destinationCenterY = platform.SurfaceY - spinJumpVerticalRadius - 1;
        int pixelDisplacement = destinationCenterY - samus.YPosition;
        if (pixelDisplacement >= 0)
            return true;

        var probe = new SamusKinematicsState
        {
            XPosition = unchecked((ushort)centerX),
            YPosition = samus.YPosition,
            XRadius = samus.Kinematics.XRadius,
            YRadius = spinJumpVerticalRadius,
            YDirection = 1,
            HorizontalSlopeCollisionEnable = samus.Kinematics.HorizontalSlopeCollisionEnable,
        };
        // MoveVertical intentionally mirrors bank $94 and samples the destination row; it
        // is not a swept-volume API. A single 112-pixel diagnostic request could jump over
        // an intervening ceiling that real five-pixel-per-frame motion must hit. Walk the
        // disposable body upward in four-pixel slices so every crossed block row is tested.
        while (probe.YPosition > destinationCenterY)
        {
            int stepPixels = Math.Min(4, probe.YPosition - destinationCenterY);
            BlockMoveResult result = SamusBlockCollision.MoveVertical(
                bus,
                level,
                probe,
                displacement: -(stepPixels << 16),
                scanLeftToRight: true,
                includeSolidEnemies: false,
                plms: plms,
                publishDoorSideEffects: false);
            if (result.Collided)
                return false;
        }
        return true;
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

    private static void PrintCollisionRegion(
        SuperMetroidRuntime runtime,
        int left,
        int right,
        int top,
        int bottom)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Collision-region diagnostic requires active room data.");
        Console.WriteLine(
            $"  Collision types X={left:X2}..{right:X2}, Y={top:X2}..{bottom:X2}:");
        for (int y = top; y <= bottom; y++)
        {
            var types = new char[right - left + 1];
            for (int x = left; x <= right; x++)
                types[x - left] = "0123456789ABCDEF"[level.GetCollisionBlock(x, y).CollisionType];
            Console.WriteLine($"    {y:X2}: {new string(types)}");
        }
    }

    private static void PrintCollisionFeatureRows(
        SuperMetroidRuntime runtime,
        int left,
        int right,
        int top,
        int bottom)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Collision-feature diagnostic requires active room data.");
        Console.WriteLine(
            $"  Collision features X={left:X2}..{right:X2}, Y={top:X2}..{bottom:X2}:");
        for (int y = top; y <= bottom; y++)
        {
            var features = new List<string>();
            for (int x = left; x <= right; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType != 0)
                    features.Add($"{x:X2}:{block.CollisionType:X1}/{block.Behavior:X2}");
            }
            if (features.Count != 0)
                Console.WriteLine($"    {y:X2}: {string.Join(' ', features)}");
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

    private static void PrintDoorBlocks(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        string roomName)
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
                {
                    CartridgeDoorHeader definition = level.ResolveDoorCollision(
                        bus,
                        block.Behavior,
                        runtime.Samus?.Pose ?? 0,
                        publishDoorSideEffects: false);
                    doors.Add(
                        $"({x:X2},{y:X2})=${block.Behavior:X2}" +
                        $"->$83:{definition.Pointer:X4}/$8F:{definition.DestinationRoomPointer:X4}");
                }
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

    private readonly record struct ClimbPlatformTarget(
        int LandingX,
        int SurfaceY,
        int ShadowLeftX,
        int ShadowRightX);

    private readonly record struct ClimbLandingSample(
        int X,
        int SurfaceY);
}
