using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Room-local #474 reproduction of the actual elevator door coroutine.</summary>
internal static class ElevatorFrontendHandoffAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializeDebugGroundedSamus(128, 0, 16);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GreenBrinstarElevatorRoom);
        var samus = runtime.Samus!;
        var platform = runtime.Enemies.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.XPosition = platform.XPosition;
        samus.YPosition = (ushort)(platform.YPosition - ElevatorActorDefinitions.SamusYOffset);
        runtime.Enemies.PublishElevatorDoorContact();
        for (int frame = 0; runtime.PendingDoorTransition == null && frame < 1200; frame++)
            runtime.StepFrame(frame % 30 == 0 ? (ushort)SnesButton.Down : (ushort)0);
        if (runtime.PendingDoorTransition?.Pointer != DoorPointers.GreenBrinstarMainShaftFromElevator)
            throw new InvalidDataException("Elevator failed to reach the expected retail door.");

        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        uint? initialPlatformY = null;
        int checkedFrames = 0;
        for (int frame = 0; transition.IsActive && frame < 600; frame++)
        {
            var phase = transition.Phase;
            transition.Step(runtime, audio, (ushort)(SnesButton.A | SnesButton.Left));
            if (runtime.ActiveRoom!.Pointer != RoomHeaderPointers.GreenBrinstarMainShaft)
                continue;
            platform = runtime.Enemies.Slots.Single(slot =>
                slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
            uint y = ((uint)platform.YPosition << 16) | platform.YSubposition;
            initialPlatformY ??= y;
            // $A3:952A returns while $0795 is nonzero. $82:E737 clears it only
            // after the last fade step, without another EnemyMain in that call.
            if (y != initialPlatformY || !samus.InputLocked || samus.Pose != 0)
                throw new InvalidDataException($"Elevator moved/unlocked during {phase}: Y={y:X8}, initial={initialPlatformY:X8}, lock={samus.InputLocked}, pose={samus.Pose:X2}.");
            checkedFrames++;
        }
        if (transition.IsActive || checkedFrames == 0)
            throw new InvalidDataException("Elevator frontend transition did not complete.");
        runtime.StepFrame((ushort)(SnesButton.A | SnesButton.Left));
        uint resumedY = ((uint)platform.YPosition << 16) | platform.YSubposition;
        if (resumedY == initialPlatformY)
            throw new InvalidDataException("Elevator failed to resume after the door fade.");
        Console.WriteLine($"Elevator frontend: {checkedFrames} frozen destination frames, then arrival resumes.");
        return 0;
    }
}
