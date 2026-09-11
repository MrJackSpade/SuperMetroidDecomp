using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>#552: constructed retail door boundary, with charge acquired through held input.</summary>
internal static class DoorChargeAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(SpinDoorFixtureDefinitions.SourceRoom);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InputLocked = false;
        samus.XPosition = 600; samus.YPosition = SpinDoorFixtureDefinitions.SourceY;
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Charge;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus); samus.PrimeGraphics(bus);
        for (int frame = 0; frame < 100; frame++) runtime.StepFrame((ushort)SnesButton.X);
        if (runtime.Projectiles.FlareCounter < 60) throw new InvalidDataException("Door fixture never acquired full charge.");
        samus.XPosition = SpinDoorFixtureDefinitions.SourceX;
        samus.YPosition = SpinDoorFixtureDefinitions.SourceY;
        var level = runtime.LevelData!;
        foreach (int index in Enumerable.Range(0, level.ForegroundEntries.Length))
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, false);
            if (door.DestinationRoomPointer != SpinDoorFixtureDefinitions.DestinationRoom) continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
            break;
        }
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        for (int frame = 0; frame < 300 && transition.IsActive; frame++)
        {
            var phase = transition.Phase;
            ushort before = runtime.Projectiles.FlareCounter;
            transition.Step(runtime, audio, (ushort)SnesButton.X);
            if (runtime.Projectiles.FlareCounter < before)
                throw new InvalidDataException($"Held charge lost during {phase}: {before} -> {runtime.Projectiles.FlareCounter}.");
            if (phase is DoorTransitionPhase.HandleTransition or DoorTransitionPhase.FadeInDestinationPalette &&
                runtime.Projectiles.Slots.Any(slot => slot.IsActive))
                throw new InvalidDataException($"A projectile fired during draw-only door phase {phase}.");
        }
        if (transition.IsActive) throw new InvalidDataException("Charged door transition timed out.");
        runtime.StepFrame((ushort)SnesButton.X);
        if (runtime.Projectiles.FlareCounter < 60) throw new InvalidDataException("Held charge lost on resumed gameplay.");
        if (runtime.LastBeamChargePaletteStep.Action != SamusBeamChargePaletteAction.ChargeCycle)
            throw new InvalidDataException("Resumed held charge did not resume its visible palette cycle.");
        runtime.StepFrame(0);
        var fired = runtime.Projectiles.LastFiredProjectileSnapshot
            ?? throw new InvalidDataException("Releasing Fire after arrival did not fire the retained charge.");
        if (!runtime.Projectiles.Slots[fired.SlotIndex].PackedType.IsChargedBeam || runtime.Projectiles.FlareCounter != 0)
            throw new InvalidDataException("Released shot was not charged or charge did not clear afterward.");
        Console.WriteLine("Held charge survives door loading/fade, resumes its palette, and fires a charged shot only on release.");
        return 0;
    }
}
