using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Checks $90:DCE0's pose gate before cooldown, HUD selection and weapon production.</summary>
internal static class ForwardFacingProjectileAudit
{
    public static int Run(string romPath)
    {
        foreach (byte pose in new[] { SamusPoseIds.ForwardFacingPowerSuitPose, SamusPoseIds.ForwardFacingSuitedPose })
        foreach (ushort item in new ushort[] { 0, 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 0, 0);
            var samus = runtime.Samus!;
            samus.XPosition = 128;
            samus.YPosition = 128;
            samus.Pose = pose;
            samus.InputLocked = true;
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            samus.SelectedHudItem = item;
            samus.Missiles = samus.MaxMissiles = 5;
            samus.SuperMissiles = samus.MaxSuperMissiles = 5;
            for (int tick = 0; tick < 90; tick++)
            {
                runtime.StepFrame((ushort)SnesButton.X);
                if (runtime.Projectiles.FlareCounter != 0 || runtime.Projectiles.Slots.Any(slot => slot.IsActive) ||
                    samus.Missiles != 5 || samus.SuperMissiles != 5)
                    throw new InvalidDataException($"Facing-forward pose {pose:X2}, HUD {item}, tick {tick}: charge={runtime.Projectiles.FlareCounter}, missiles={samus.Missiles}/{samus.SuperMissiles}.");
            }
        }
        VerifyExistingProjectileAdvances(romPath);
        Console.WriteLine("Forward-facing projectile gate: both elevator poses suppress Charge/beam/missile production across 540 runtime frames.");
        return 0;
    }

    private static void VerifyExistingProjectileAdvances(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 0, 0);
        var level = runtime.LevelData!;
        for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++) level.SetForegroundEntry(block, 0);
        var samus = runtime.Samus!;
        samus.XPosition = samus.YPosition = 128;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InputLocked = false;
        samus.InitializeAnimation(bus);
        // First prove ordinary weapon input still works and produce a real moving shot.
        runtime.StepFrame((ushort)SnesButton.X);
        var shot = runtime.Projectiles.Slots.FirstOrDefault(slot => slot.IsActive)
            ?? throw new InvalidDataException("Standing control fixture failed to fire an ordinary beam.");
        ushort beforeX = shot.XPosition;
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.InputLocked = true;
        runtime.BombProjectiles.SetSharedCooldown(7);
        runtime.StepFrame((ushort)SnesButton.X);
        if (!shot.IsActive || shot.XPosition <= beforeX || runtime.BombProjectiles.CooldownTimer != 7)
            throw new InvalidDataException("Facing-forward branch froze existing projectiles or decremented the bypassed cooldown.");
        Console.WriteLine("Forward-facing bypass: standing still fires, existing beam keeps moving, shared cooldown remains untouched.");
    }
}
