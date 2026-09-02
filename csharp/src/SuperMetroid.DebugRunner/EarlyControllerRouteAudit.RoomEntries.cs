using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
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
    public static int RunVerticalRoomEntryAudit(string romPath)
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
}
