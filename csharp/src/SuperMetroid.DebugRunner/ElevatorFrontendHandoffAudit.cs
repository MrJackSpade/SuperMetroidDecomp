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
    public static int Run(string rom, bool retainedFlash = false)
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
        if (retainedFlash)
        {
            samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 10;
            samus.EquippedItems = samus.CollectedItems = 0;
            if (!runtime.TryBeginCrystalFlashFromPowerBombCleanup(0x470, 0x40))
                throw new InvalidDataException("Elevator setup rejected actual Flash admission.");
        }
        for (int frame = 0; runtime.PendingDoorTransition == null && frame < 1200; frame++)
            runtime.StepFrame(retainedFlash ? (ushort)(frame == 0 ? 0x470 : 0) : frame % 30 == 0 ? (ushort)SnesButton.Down : (ushort)0);
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
        {
            for (int frame = 0; runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive && frame < 1200; frame++)
                runtime.StepFrame(0);
            if (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive || samus.InputLocked ||
                samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive ||
                (retainedFlash ? samus.SharedShineTimer == 0 || samus.CrystalFlash.SpecialPaletteKind != SamusSpecialPaletteType.CrystalFlash
                               : samus.SharedShineTimer != 0 || samus.CrystalFlash.SpecialPaletteKind != SamusSpecialPaletteType.None))
                throw new InvalidDataException($"Elevator return did not preserve Flash and restore control: status={runtime.Enemies.ElevatorStatus}, lock={samus.InputLocked}, phase={samus.CrystalFlash.Phase}, timer={samus.SharedShineTimer}, palette={samus.CrystalFlash.SpecialPaletteType}.");
            bool spark = false;
            ushort startingHealth = samus.Health;
            // The elevator releases the front-facing pose; first turn into ordinary
            // standing movement, then supply a fresh jump edge.
            runtime.StepFrame((ushort)SnesButton.Right);
            runtime.StepFrame(0);
            for (int frame = 0; frame < 30; frame++)
            {
                runtime.StepFrame((ushort)(frame == 0 ? 0x80 : 0x880));
                spark |= samus.Shinespark.Phase == ShinesparkPhase.Vertical &&
                    samus.HorizontalSpeed.ContactDamageIndex == 2 && samus.Health < startingHealth;
            }
            if (spark != retainedFlash) throw new InvalidDataException("Post-elevator spark admission differs from earned Flash/control state.");
            Console.WriteLine($"Elevator Flash={retainedFlash}: real door handoff and arrival restore controls; damaging spark={spark}.");
        }
        Console.WriteLine($"Elevator frontend: {checkedFrames} frozen destination frames, then arrival resumes.");
        return 0;
    }
}
