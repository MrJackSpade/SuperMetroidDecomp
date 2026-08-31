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
        PrintCollisionFeatureRows(runtime, 0x00, 0x0f, 0x00, 0x1a);
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

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x9f11, 0x9f23, "Pre-Missiles return");
        Console.WriteLine(
            $"  Returned to pre-Missiles at Samus (${samus.XPosition:X4},${samus.YPosition:X4}); " +
            $"colored doors: {string.Join(' ', runtime.Plms.ColoredDoors)}.");
        DriveResult constructionZoneReturn = DriveUntilDoor(
            bus,
            runtime,
            "Pre-Missiles room",
            maximumFrames: 3600);
        AssertPendingDoor(runtime, 0x8ec2, 0x9e9f, "Pre-Missiles return -> Morph Ball");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x9e9f, 0x9eb1, "Morph Ball return");
        DriveResult morphBallReturn = DriveUntilDoor(
            bus,
            runtime,
            "Morph Ball return",
            maximumFrames: 6000);
        AssertPendingDoor(runtime, 0x8eb6, 0x97b5, "Morph Ball return -> elevator");

        runtime.LoadPendingDoorDestination();
        // The room's event/item state selector deliberately chooses $97E0 on the upward
        // trip after Morph Ball and the first Missile; $97C6 is only the initial descent.
        AssertRoom(runtime, 0x97b5, 0x97e0, "Elevator return");
        DriveResult elevatorReturn = DriveUntilDoor(
            bus,
            runtime,
            "Elevator to Blue Brinstar",
            maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8b92, 0x975c, "Elevator return -> Pit");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x975c, 0x9787, "Pit return");
        DriveResult pitReturn = DriveUntilDoor(bus, runtime, "Pit", maximumFrames: 2400);
        AssertPendingDoor(runtime, 0x8b7a, 0x96ba, "Pit return -> Climb");

        runtime.LoadPendingDoorDestination();
        AssertRoom(runtime, 0x96ba, 0x96d1, "Climb return");
        DriveResult climbReturn = DriveUntilDoor(bus, runtime, "Climb", maximumFrames: 12000);
        AssertPendingDoor(runtime, 0x8b3e, 0x92fd, "Climb return -> Parlor");

        Console.WriteLine(
            $"Controller route: gunship {landingFrames} frames; Landing Site -> Parlor " +
            $"after {landing.Frames} ordinary gameplay frames; Parlor -> Climb after " +
            $"{parlor.Frames} more frames; Climb -> Pit after {climb.Frames} more; Pit exit " +
            $"after {pit.Frames} more; elevator descent after {elevator.Frames} more; " +
            $"Morph Ball acquired and the pre-Missiles door reached after " +
            $"{morphBall.Frames} more; pre-Missiles exited after " +
            $"{preMissiles.Frames} more; first Missile collected and room exited after " +
            $"{firstMissile.Frames} more; climbed Construction Zone and returned to the " +
            $"Morph Ball door after {constructionZoneReturn.Frames} more at Samus " +
            $"(${samus.XPosition:X4},${samus.YPosition:X4}); returned to the elevator " +
            $"after {morphBallReturn.Frames} more; elevator/Pit/Climb return took " +
            $"{elevatorReturn.Frames}/{pitReturn.Frames}/{climbReturn.Frames} frames.");
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
        bool climbTargetIsDoorApproach = false;
        bool climbUsingWallJumps = false;
        bool climbWallTargetIsRight = true;
        SnesButton climbWallContactDirection = 0;
        int climbWallJumpButtonHoldFrames = 0;
        bool climbHoldingTriggeredWallJump = false;
        bool climbWallLandingEnabled = false;
        bool climbWaitingAboveWallLanding = false;
        int climbWallLandingX = 0;
        int climbWallLandingSurfaceY = 0;
        bool climbRequiresGroundApproach = false;
        bool climbLaunchTowardApproach = false;
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
        int preMissilesAscentStage = 0;
        int preMissilesStageEnteredFrame = 0;

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
            bool returningAcrossMorphBallRoom = roomName == "Morph Ball return";
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
            else if (returningAcrossMorphBallRoom)
            {
                // The acquired-item return enters at the east cap and traverses the same
                // cartridge room westward to the elevator at columns $57/$58. Repeated
                // held/released jumps clear ruined-floor lips; only once Samus is centred
                // over the real platform do pulsed Up edges request its upward departure.
                if (samus.XPosition > 0x0590)
                {
                    input = (ushort)SnesButton.Left;
                    if (frame % 60 < 30)
                        input |= (ushort)SnesButton.A;
                }
                else if (samus.XPosition < 0x0568)
                {
                    input = (ushort)SnesButton.Right;
                }
                else
                {
                    input = frame % 30 == 0 ? (ushort)SnesButton.Up : (ushort)0;
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
                if (samus.MaxMissiles != 0)
                {
                    int previousAscentStage = preMissilesAscentStage;
                    if (samus.Kinematics.YDirection == 0 && samus.YPosition < 0x0170)
                    {
                        if (samus.YPosition < 0x0090)
                            preMissilesAscentStage = 3;
                        else if (samus.XPosition >= 0x00a0)
                            preMissilesAscentStage = 1;
                        else if (samus.YPosition < 0x0130 && samus.XPosition <= 0x0080)
                            preMissilesAscentStage = 2;
                    }
                    else if (samus.Kinematics.YDirection == 0 && samus.YPosition >= 0x0170)
                    {
                        // A missed shelf returns to the physical lower floor. Restart the
                        // three-transfer policy instead of applying an upper-shelf steering
                        // decision to a new bottom jump.
                        preMissilesAscentStage = 0;
                    }
                    if (preMissilesAscentStage != previousAscentStage)
                        preMissilesStageEnteredFrame = frame;
                    input = BuildPreMissilesReturnInput(
                        bus,
                        samus,
                        frame,
                        preMissilesAscentStage,
                        frame - preMissilesStageEnteredFrame);
                }
                else if (samus.YPosition < 0x0100)
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
                        if (!waitingForLandingAnimation && climbTargetX is null &&
                            !climbUsingWallJumps)
                        {
                            ClimbPlatformTarget? nextPlatform = FindNextClimbPlatformCenter(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus);
                            if (nextPlatform is null)
                            {
                                // The return ascent contains cartridge-authored gaps whose
                                // next floor exceeds the ordinary spin-jump envelope. Switch
                                // the input driver to the game's native wall-jump handshake;
                                // neither position nor velocity is patched by the audit.
                                climbUsingWallJumps = true;
                                climbWallTargetIsRight = samus.XPosition >= 0x0180;
                                ClimbPlatformTarget landingAfterWallJump =
                                    FindNextClimbPlatformCenter(
                                        bus,
                                        runtime.LevelData!,
                                        runtime.Plms,
                                        samus,
                                        maximumRise: 224) ??
                                    throw new InvalidDataException(
                                        "Climb wall-jump gap has no cartridge floor or top-door " +
                                        "approach within 224 pixels.");
                                climbWallLandingX = landingAfterWallJump.LandingX;
                                climbWallLandingSurfaceY = landingAfterWallJump.SurfaceY;
                            }
                            if (nextPlatform is not { } platform)
                                goto BuildClimbWallJumpInput;
                            climbTargetX = platform.LandingX;
                            climbTargetSurfaceY = platform.SurfaceY;
                            climbTargetIsDoorApproach = platform.IsDoorApproach;
                            int launchDistance = Math.Abs(platform.LandingX - samus.XPosition);
                            // Samus is only ten pixels wide, but a square slope's solid
                            // quadrant can project much farther than the block selected as
                            // the eventual landing sample. Treat transfers under three
                            // blocks as under-ledged and clear the whole underside first.
                            int targetBlockLeft = platform.ShadowLeftX;
                            int targetBlockRight = platform.ShadowRightX;
                            bool directlyUnderTarget = !platform.IsDoorApproach &&
                                samus.XPosition >=
                                targetBlockLeft - samus.Kinematics.XRadius &&
                                samus.XPosition <= targetBlockRight + samus.Kinematics.XRadius;
                            // A normal jump cannot pass through the selected ledge. The
                            // approach coordinate is the nearest X whose complete body is
                            // just outside its tile shadow. If Samus begins under that
                            // shadow, reach the coordinate on the ground first. Otherwise
                            // jump immediately and build horizontal speed toward it during
                            // ascent; the hardest retail gap offers only one pixel of
                            // vertical clearance at the apex.
                            climbApproachX = platform.IsDoorApproach
                                ? platform.LandingX
                                : FindClearClimbAscentX(
                                    bus,
                                    runtime.LevelData!,
                                    runtime.Plms,
                                    samus,
                                    platform);
                            bool currentLaneIsClear = platform.IsDoorApproach ||
                                IsClimbAscentLaneClear(
                                    bus,
                                    runtime.LevelData!,
                                    runtime.Plms,
                                    samus,
                                    platform,
                                    samus.XPosition);
                            // A tiny supporting lip may end before the safe outside lane.
                            // Trace the proposed run-up with the real downward dispatcher.
                            // If cartridge collision cannot support that walk, launch toward
                            // the outside lane immediately and clear the overhang in the air.
                            // This derives the decision from room geometry instead of naming
                            // one particular shelf or installing a route-only position fix.
                            bool canWalkToApproach = HasClimbGroundSupportToApproach(
                                bus,
                                runtime.LevelData!,
                                runtime.Plms,
                                samus,
                                climbApproachX);
                            climbLaunchTowardApproach = directlyUnderTarget &&
                                !canWalkToApproach;
                            climbRequiresGroundApproach = !climbLaunchTowardApproach &&
                                (directlyUnderTarget || !currentLaneIsClear);
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

                    BuildClimbWallJumpInput:
                        if (climbTargetIsDoorApproach &&
                            IsClimbTopDoorCapClosed(runtime.LevelData!))
                        {
                            int currentSurfaceY =
                                samus.YPosition + samus.Kinematics.YRadius;
                            if (currentSurfaceY < 0x0080)
                            {
                                // The row-$7 post is only one block wide and cannot provide
                                // a centred firing stance. Walk off its inner edge onto the
                                // broad row-$8 ledge; gravity and floor collision own the drop.
                                input = (ushort)SnesButton.Right;
                            }
                            else if (samus.XPosition < 0x0174)
                            {
                                input = (ushort)SnesButton.Right;
                            }
                            else if (samus.XPosition > 0x018c)
                            {
                                input = (ushort)SnesButton.Left;
                            }
                            else
                            {
                                // Row $2 is a type-$C main block followed by type-$5
                                // extensions. Aim upward and send distinct beam edges until
                                // the translated shot-block owner removes that cartridge cap.
                                input = (ushort)SnesButton.Up;
                                if (frame % 20 < 4)
                                    input |= (ushort)SnesButton.X;
                            }
                            goto ClimbInputBuilt;
                        }

                        if (climbUsingWallJumps)
                        {
                            // Begin each first wall approach with a real running spin jump.
                            // The slope lip supporting this gap is only a few usable pixels
                            // wide: trying to manufacture a long run-up simply walks Samus
                            // off it. One direction-only frame establishes running, and the
                            // following fresh Jump edge selects the native spin pose.
                            SnesButton towardWall = climbWallTargetIsRight
                                ? SnesButton.Right
                                : SnesButton.Left;
                            input = (ushort)(towardWall | SnesButton.B);
                            climbGroundedRunupFrames++;
                            if (climbGroundedRunupFrames >= 2 && climbJumpReady)
                            {
                                // Hold for the full native arc unless wall proximity below
                                // asks for an early release. This gives the short lip enough
                                // flight time to reach the shaft wall, while still ensuring
                                // contact precedes a newly-pressed A edge.
                                input |= (ushort)SnesButton.A;
                                climbWallJumpButtonHoldFrames = 48;
                                climbJumpReady = false;
                            }
                            climbAlignedForJump = false;
                        }
                        else
                        {
                        int groundTargetX = climbApproachX;
                        bool reachedLaunchX = Math.Abs(
                            samus.XPosition - groundTargetX) <= 1;
                        int launchTargetX = climbLaunchTowardApproach
                            ? climbApproachX
                            : climbTargetX ?? 0x0178;
                        SnesButton launchDirection = samus.XPosition < launchTargetX
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
                    }
                    else
                    {
                        climbAirborneFrames++;
                        if (climbUsingWallJumps)
                        {
                            SnesButton towardWall = climbWallTargetIsRight
                                ? SnesButton.Right
                                : SnesButton.Left;
                            SnesButton awayFromWall = climbWallTargetIsRight
                                ? SnesButton.Left
                                : SnesButton.Right;
                            // These are the body-centre limits produced by the shaft walls,
                            // including the asymmetric four-pixel square-slope stop on the
                            // left. Releasing A only at contact preserves jump height; the
                            // real bank-$94 probe below remains the authority on whether a
                            // wall contact and subsequent wall jump actually exist.
                            bool nearWall = climbWallTargetIsRight
                                ? samus.XPosition >= 0x01cf
                                : samus.XPosition <= 0x012e;
                            bool contactedLastFrame = runtime.LastAerialSamusMovement is
                                { WallContact: true, WallJumpTriggered: false };

                            if (climbWaitingAboveWallLanding)
                            {
                                // The target shelf projects leftward from the right wall.
                                // Cancel horizontal base motion and rise outside its shadow;
                                // crossing toward its center any earlier hits the underside
                                // and drops Samus back onto the lower slope.
                                bool feetAboveLanding = samus.YPosition +
                                    samus.Kinematics.YRadius < climbWallLandingSurfaceY;
                                if (feetAboveLanding)
                                {
                                    climbWaitingAboveWallLanding = false;
                                    climbWallLandingEnabled = true;
                                }
                                else
                                {
                                    input = (ushort)SnesButton.B;
                                    if (climbWallJumpButtonHoldFrames > 0)
                                    {
                                        input |= (ushort)SnesButton.A;
                                        climbWallJumpButtonHoldFrames--;
                                    }
                                    goto ClimbInputBuilt;
                                }
                            }

                            if (climbWallLandingEnabled)
                            {
                                // The second wall launch is now above the otherwise
                                // unreachable shelf. Stop crossing the shaft and center
                                // over the floor resolved by the same bank-$94 probe used
                                // by ordinary targets, so downward collision owns landing.
                                input = (ushort)SnesButton.B;
                                if (samus.XPosition < climbWallLandingX - 1)
                                    input |= (ushort)SnesButton.Right;
                                else if (samus.XPosition > climbWallLandingX + 1)
                                    input |= (ushort)SnesButton.Left;
                                if (climbWallJumpButtonHoldFrames > 0)
                                {
                                    input |= (ushort)SnesButton.A;
                                    climbWallJumpButtonHoldFrames--;
                                }
                                goto ClimbInputBuilt;
                            }

                            // CheckBlockWallJump probes the wall behind the newly-facing
                            // spin pose. Thus Left tests the right wall and Right tests the
                            // left wall. First make one contact frame with A released; on
                            // the following frame add A to create the edge required by
                            // `$90:9E7F`. After a successful launch, keep A held for native
                            // variable-height physics while travelling to the opposite wall.
                            if (climbWallJumpButtonHoldFrames > 0 &&
                                (climbHoldingTriggeredWallJump || !nearWall))
                            {
                                input = (ushort)(towardWall | SnesButton.A | SnesButton.B);
                                climbWallJumpButtonHoldFrames--;
                                if (climbWallJumpButtonHoldFrames == 0)
                                    climbHoldingTriggeredWallJump = false;
                            }
                            else if (contactedLastFrame)
                            {
                                // Repeat the exact directional probe that made contact.
                                // The direction names the wall *behind* the facing pose
                                // (Right probes left, Left probes right); recomputing it
                                // from the intended travel wall can reverse the pose and
                                // discard ApplyWallContactAnimationRewind's eligible frame.
                                input = (ushort)(climbWallContactDirection |
                                    SnesButton.A | SnesButton.B);
                            }
                            else
                            {
                                if (nearWall)
                                    climbWallJumpButtonHoldFrames = 0;
                                input = (ushort)((nearWall ? awayFromWall : towardWall) |
                                    SnesButton.B);
                            }
                            goto ClimbInputBuilt;
                        }
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
            ClimbInputBuilt:
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
            if (runtime.LastAerialSamusMovement is
                { WallContact: true, WallJumpTriggered: false })
            {
                climbWallContactDirection = (input & (ushort)SnesButton.Left) != 0
                    ? SnesButton.Left
                    : SnesButton.Right;
            }
            if (roomName == "Climb" && returningWithMorphBallAtEntry)
            {
                if (runtime.LastAerialSamusMovement is { WallJumpTriggered: true })
                {
                    // The trigger frame installs `$83/$84` and its upward speed, but the
                    // following wall-jump movement frames still use ordinary variable-jump
                    // cutoff. Continue holding A or native quite correctly truncates the
                    // launch to a tiny hop on the very next frame.
                    if (climbUsingWallJumps)
                    {
                        // Right input probes a wall on Samus' left and launches toward the
                        // right; Left is its mirror. Aim at the opposite shaft boundary.
                        climbWallTargetIsRight = climbWallContactDirection == SnesButton.Right;
                        climbWallJumpButtonHoldFrames = 48;
                        climbHoldingTriggeredWallJump = true;
                        if (samus.YPosition <= climbWallLandingSurfaceY + 0x30)
                            climbWaitingAboveWallLanding = true;
                    }
                    else
                    {
                        climbJumpHoldFrames = 48;
                    }
                }
                bool climbIsAirborne = samus.Kinematics.YDirection != 0;
                if (climbWasAirborne && !climbIsAirborne)
                {
                    if (climbUsingWallJumps)
                    {
                        // A wall-jump arc may terminate on one of the narrow side lips.
                        // Launch across the shaft from whichever half caught Samus; aiming
                        // back into that adjacent wall produces only a one-frame hop.
                        climbWallTargetIsRight = samus.XPosition < 0x0180;
                        if (climbWallLandingEnabled)
                        {
                            // Landing may be either the requested floor or an intermediate
                            // authored slope below it. In both cases the oversized gap is
                            // finished; resume the ordinary platform selector from this
                            // genuine support rather than chaining synthetic wall logic.
                            climbUsingWallJumps = false;
                            climbWallLandingEnabled = false;
                            climbWaitingAboveWallLanding = false;
                        }
                    }
                    // A variable-height hold belongs only to the arc that created it. If
                    // its remaining frames leak across landing, the next launch never has
                    // a newly-pressed A edge and the ROM table produces a tiny neutral hop.
                    climbJumpHoldFrames = 0;
                    climbJumpReady = true;
                    climbGroundedRunupFrames = 0;
                    climbAirborneFrames = 0;
                    climbTargetX = null;
                    climbWallJumpButtonHoldFrames = 0;
                    climbHoldingTriggeredWallJump = false;
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
            bool detailedPreMissilesReturnSample = descendingPreMissiles &&
                samus.MaxMissiles != 0 &&
                frame <= 420 && frame % 10 == 0;
            if (screenX != previousScreenX || screenY != previousScreenY ||
                frame % 60 == 0 || detailedReturnedClimbSample ||
                detailedPreMissilesReturnSample)
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
                    (detailedPreMissilesReturnSample
                        ? $"/stage={preMissilesAscentStage}/" +
                          $"ydir={samus.Kinematics.YDirection}/" +
                          $"move={samus.ReadMovementKind(bus)}/in=${input:X4}"
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

    private static ClimbPlatformTarget? FindNextClimbPlatformCenter(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        int maximumRise = 112)
    {
        const int shaftLeftBlock = 0x12;
        const int shaftRightBlock = 0x1d;
        const int topDoorCapRow = 0x02;
        const int upwardSearchRows = 12;
        const int topDoorCenterX = 0x0178;
        const int minimumSurfaceRise = 8;

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
        if (currentSurfaceY <= 0x0080)
        {
            // Once Samus reaches the broad top ledge (or either one-block post above it),
            // no further landing target is required. The remaining geometry is a breakable
            // cap at row $2 and the upward type-$9 door at row $0. A terminal target keeps
            // steering inside that open four-block shaft while ordinary projectile and
            // vertical collision code perform both state changes.
            return new ClimbPlatformTarget(
                topDoorCenterX,
                SurfaceY: topDoorCapRow * 16,
                ShadowLeftX: 0x0160,
                ShadowRightX: 0x019f,
                IsDoorApproach: true);
        }
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
                RoomCollisionBlock candidate = level.GetCollisionBlock(centerX >> 4, blockY);
                // Climb's repeating shaft uses square slopes, but its final visible ledges
                // are ordinary solid blocks. Admit both candidate families, then let the
                // direction-aware bank-$94 probe below decide whether Samus can stand at
                // this exact body coordinate. Skipping air here is important: a 17-pixel
                // probe from an empty row can otherwise rediscover the floor below it and
                // incorrectly widen a narrow platform's usable centre range.
                if (candidate.CollisionType is not (1 or 8))
                    continue;
                if (candidate.CollisionType == 8 && blockY > 0 &&
                    level.GetCollisionBlock(centerX >> 4, blockY - 1).CollisionType == 8)
                {
                    // The side walls are long vertical stacks of type-$8 blocks. Their top
                    // edge is technically a downward collision surface, but it is not an
                    // in-shaft landing: the block immediately above is solid too. Reject
                    // those wall columns while retaining isolated type-$8 top ledges.
                    continue;
                }

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
                // A square slope is commonly paired with an adjacent mirrored record.
                // Samus' real-width probe samples both feet, and bank $94 may therefore
                // report the neighboring member even though this exact centre coordinate
                // is supported by the same authored platform. Collision plus the resolved
                // surface is authoritative; nominal block-index identity is not.
                if (!result.Collided)
                    continue;

                int surfaceY = probe.YPosition + probe.YRadius;
                int rise = currentSurfaceY - surfaceY;
                if (rise < minimumSurfaceRise || rise > maximumRise)
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

        return null;
    }

    private static bool IsClimbTopDoorCapClosed(RoomLevelData level)
    {
        const int capRow = 0x02;
        const int capLeftBlock = 0x16;
        const int capRightBlock = 0x19;

        // The main type-$C block owns the mutation; the three type-$5 records extend its
        // collision across the four-tile shaft. Consult all four so the controller waits
        // through any multi-frame removal state instead of assuming one header byte is the
        // complete door-cap lifecycle.
        for (int blockX = capLeftBlock; blockX <= capRightBlock; blockX++)
        {
            if (level.GetCollisionBlock(blockX, capRow).CollisionType is 0x0c or 0x05)
                return true;
        }
        return false;
    }

    private static bool HasClimbGroundSupportToApproach(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms,
        SamusState samus,
        int approachX)
    {
        const int horizontalSampleSpacing = 2;
        const int probeClearanceAboveExpectedSurface = 8;
        const int maximumSurfaceChangePerSample = 4;
        const int maximumDownwardProbeDistance =
            probeClearanceAboveExpectedSurface + maximumSurfaceChangePerSample;

        int expectedSurfaceY = samus.YPosition + samus.Kinematics.YRadius;
        int direction = Math.Sign(approachX - samus.XPosition);
        if (direction == 0)
            return true;

        // A safe ground run-up needs continuous support for Samus' complete body, not just
        // a solid-looking BTS tile beneath its centre. Sample every two horizontal pixels
        // and let bank $94 resolve the exact square-slope surface under the real body radii.
        // Each sample begins above the preceding surface and descends one pixel at a time;
        // this follows legitimate shallow slopes while rejecting a lip, gap, or large drop.
        for (int centerX = samus.XPosition;
             centerX != approachX;
             centerX = direction > 0
                 ? Math.Min(centerX + horizontalSampleSpacing, approachX)
                 : Math.Max(centerX - horizontalSampleSpacing, approachX))
        {
            int sampledX = direction > 0
                ? Math.Min(centerX + horizontalSampleSpacing, approachX)
                : Math.Max(centerX - horizontalSampleSpacing, approachX);
            var probe = new SamusKinematicsState
            {
                XPosition = unchecked((ushort)sampledX),
                YPosition = unchecked((ushort)(
                    expectedSurfaceY - samus.Kinematics.YRadius -
                    probeClearanceAboveExpectedSurface)),
                XRadius = samus.Kinematics.XRadius,
                YRadius = samus.Kinematics.YRadius,
                YDirection = 2,
                HorizontalSlopeCollisionEnable =
                    samus.Kinematics.HorizontalSlopeCollisionEnable,
            };

            bool foundSupport = false;
            for (int pixel = 0; pixel < maximumDownwardProbeDistance; pixel++)
            {
                BlockMoveResult result = SamusBlockCollision.MoveVertical(
                    bus,
                    level,
                    probe,
                    displacement: 1 << 16,
                    scanLeftToRight: true,
                    includeSolidEnemies: false,
                    plms: plms,
                    publishDoorSideEffects: false);
                if (!result.Collided)
                    continue;

                int resolvedSurfaceY = probe.YPosition + probe.YRadius;
                if (Math.Abs(resolvedSurfaceY - expectedSurfaceY) >
                    maximumSurfaceChangePerSample)
                {
                    return false;
                }
                expectedSurfaceY = resolvedSurfaceY;
                foundSupport = true;
                break;
            }

            if (!foundSupport)
                return false;
        }

        return true;
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

    private static ushort BuildPreMissilesReturnInput(
        ISnesAddressSpace bus,
        SamusState samus,
        int frame,
        int ascentStage,
        int framesInStage)
    {
        SamusMovementType movement = samus.ReadMovementKind(bus);
        bool morphed = movement is SamusMovementType.MorphBallGround or
            SamusMovementType.MorphBallFalling;

        if (samus.YPosition >= 0x0170)
        {
            // The return begins in the west chamber below the divider. Morph through the
            // same one-tile passage used outbound, but travel east to the only vertical
            // opening at columns $08/$09. This remains controller-only: these thresholds
            // choose buttons while native posture/collision decides whether motion occurs.
            if (samus.XPosition < 0x0080)
            {
                if (!morphed)
                    return frame % 60 < 20 ? (ushort)0 : (ushort)SnesButton.Down;
                return (ushort)SnesButton.Right;
            }

            if (morphed || movement == SamusMovementType.PostureTransition)
                return (ushort)SnesButton.Up;

            if (samus.Kinematics.YDirection == 0)
            {
                // A failed upper transfer can deposit Samus against the passage's east
                // wall at X=$9B. Running farther right there can never establish the spin
                // jump required by the shaft. Walk back to its centre before beginning a
                // fresh launch; this is recovery policy in the audit, not gameplay state.
                if (samus.XPosition > 0x0094)
                    return (ushort)SnesButton.Left;
                if (samus.XPosition < 0x0084)
                    return (ushort)SnesButton.Right;

                // A standing A press selects normal-jump pose $4D and its larger body
                // tops out one row short of this narrow shaft. Establish running with the
                // configured Run button, then add a fresh Jump edge to select spin $19.
                ushort launch = (ushort)(SnesButton.Right | SnesButton.B);
                if (movement == SamusMovementType.Running)
                    launch |= (ushort)SnesButton.A;
                return launch;
            }
        }

        if (ascentStage == 2)
        {
            // Rows $0C and $08 contain cartridge type-$C/BTS-$00 shot blocks directly
            // above the row-$10 floor. Stand under their shared X=$06..$09 chimney and
            // send two distinct upward shots through it. The first dies on row $0C; the
            // second reaches row $08 after the first block has become air.
            if (samus.Kinematics.YDirection == 0 && framesInStage < 48)
            {
                if (samus.XPosition < 0x0076)
                    return (ushort)SnesButton.Right;
                if (samus.XPosition > 0x0079)
                    return (ushort)SnesButton.Left;

                ushort clearingInput = (ushort)SnesButton.Up;
                if (framesInStage % 20 < 4)
                    clearingInput |= (ushort)SnesButton.X;
                return clearingInput;
            }

            // Once both blocks have received a beam, hold a plain Jump through their
            // temporary air cells. Keeping Up held permits another shot if the leading
            // edge happened to meet a block on its restoration frame.
            // Jump is edge-triggered. Give every retry a release interval so a collision
            // with a just-restored block cannot leave the audit holding an inert A button
            // forever. X continues to pulse during both halves and keeps the column open.
            int jumpCycle = (framesInStage - 48) % 60;
            ushort chimneyInput = (ushort)SnesButton.Up;
            if (jumpCycle >= 12)
                chimneyInput |= (ushort)SnesButton.A;
            if (framesInStage % 20 < 4)
                chimneyInput |= (ushort)SnesButton.X;
            return chimneyInput;
        }

        if (ascentStage >= 3)
        {
            // Above row $08, the restored shot blocks become the top corridor's floor.
            // Walk west and pulse Fire only near the blue cap. Door collision and the
            // transition itself remain owned by the normal projectile/PLM/door systems.
            ushort corridorInput = (ushort)SnesButton.Left;
            if (samus.XPosition <= 0x0030 && frame % 24 < 4)
                corridorInput |= (ushort)SnesButton.X;
            return corridorInput;
        }

        if (samus.YPosition >= 0x00f0)
        {
            // The shaft is intentionally taller than one jump. Rise between its two walls,
            // then cross onto the right wall's row-$15 top only after Samus' feet clear it.
            // A second spin jump from that genuine collision surface reaches the upper room.
            bool feetAboveRightPlatform =
                samus.YPosition + samus.Kinematics.YRadius < 0x0150;
            if (samus.Kinematics.YDirection == 0)
            {
                // Construction Zone alternates real landing shelves: right at row $15,
                // then left at row $12. Run away from each adjoining wall before adding
                // Jump so every transfer begins with the cartridge's spin pose.
                SnesButton platformDirection = ascentStage == 1
                    ? SnesButton.Left
                    : SnesButton.Right;
                ushort platformLaunch = (ushort)(platformDirection | SnesButton.B);
                if (movement == SamusMovementType.Running)
                    platformLaunch |= (ushort)SnesButton.A;
                return platformLaunch;
            }
            ushort input = (ushort)(SnesButton.A | SnesButton.B);
            if (ascentStage == 1)
                input |= (ushort)SnesButton.Left;
            else if (ascentStage >= 2)
                input |= (ushort)SnesButton.Right;
            else if (feetAboveRightPlatform)
                input |= (ushort)SnesButton.Right;
            else if (samus.XPosition < 0x0084)
                input |= (ushort)SnesButton.Right;
            else if (samus.XPosition > 0x0094)
                input |= (ushort)SnesButton.Left;
            return input;
        }

        // Normal progression returns west to the Morph Ball room. The red east cap leads
        // to the optional Blue Brinstar ceiling E-tank hall and must not be mistaken for
        // the route simply because the first Missile can now satisfy it.
        ushort upperInput;
        if (ascentStage == 2)
        {
            // Preserve leftward spin through the short gap between the bridge and the
            // west door floor. Landing there advances the audit to stage 3 above.
            upperInput = (ushort)(SnesButton.Left | SnesButton.B | SnesButton.A);
        }
        else if (samus.Kinematics.YDirection != 0)
        {
            // Land on the restored row-$10 shot-block bridge before walking west. Holding
            // Left throughout the second arc carries Samus past its block-$06 edge while
            // still airborne and drops her all the way back into the shaft.
            upperInput = samus.Kinematics.YDirection == 2
                ? samus.XPosition > 0x0080
                    ? (ushort)SnesButton.Left
                    : (ushort)SnesButton.Right
                : samus.XPosition > 0x00a0
                    ? (ushort)SnesButton.Left
                    : (ushort)SnesButton.Right;
            if (samus.Kinematics.YDirection == 1)
                upperInput |= (ushort)SnesButton.A;
        }
        else
        {
            upperInput = (ushort)SnesButton.Left;
        }
        if (samus.XPosition <= 0x0030 && frame % 24 == 0)
            upperInput |= (ushort)SnesButton.X;
        return upperInput;
    }

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
        int ShadowRightX,
        bool IsDoorApproach = false);

    private readonly record struct ClimbLandingSample(
        int X,
        int SurfaceY);
}
