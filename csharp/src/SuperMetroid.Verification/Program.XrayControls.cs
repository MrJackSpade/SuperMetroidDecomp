using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyXrayControls()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool remapped in new[] { false, true })
        {
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom);
            runtime.ControllerBindings = remapped
                ? ControllerBindings.Default.AssignAndSwap(0, ControllerBindings.Default.Dash)
                : ControllerBindings.Default;
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = 128;
            samus.YPosition = 139;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
            samus.EquippedBeams = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            AssertEqual(SamusXrayRomData.SelectedHudItem, samus.SelectedHudItem, "ordinary input selects X-ray");
            runtime.StepFrame(runtime.ControllerBindings.Shoot);
            AssertTrue(!samus.Xray.IsActive, "Shoot alone does not activate selected X-ray");
            AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is not null,
                "selected inactive X-ray falls through to ordinary beam producer when Run is released");
            for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
            ushort poseBeforeActivation = samus.PoseHistory.PreviousPose;
            ushort metadataBeforeActivation = samus.PoseHistory.PreviousDirectionAndMovement;
            // Construct an already-placed bomb, far from Samus, to isolate the shared
            // ten-slot update gate. Its long instruction delay avoids consuming data.
            var pendingBomb = runtime.BombProjectiles.Slots[0];
            pendingBomb.Type = (ushort)SamusProjectileFamily.Bomb;
            pendingBomb.InstructionPointer = 0x9000;
            pendingBomb.InstructionTimer = 100;
            pendingBomb.BombTimer = 60;
            runtime.StepFrame((ushort)(runtime.ControllerBindings.Dash | runtime.ControllerBindings.Shoot));
            AssertEqual(60, pendingBomb.BombTimer, "X-ray activation freezes an existing bomb fuse immediately");
            AssertEqual(100, pendingBomb.InstructionTimer, "X-ray activation skips existing bomb animation immediately");
            pendingBomb.ClearFields();
            AssertTrue(samus.Xray.IsActive && runtime.TimeIsFrozen,
                "configured Run takes priority over Shoot and activates scanning");
            AssertEqual(samus.Pose, samus.PoseHistory.PreviousPose, "X-ray activation commits interrupted pose");
            AssertEqual(samus.ReadPoseXDirection(bus) | ((byte)samus.ReadMovementType(bus) << 8),
                samus.PoseHistory.PreviousDirectionAndMovement, "X-ray activation commits interrupted metadata");
            AssertEqual(poseBeforeActivation, samus.PoseHistory.LastDifferentPose, "X-ray activation shifts prior pose exactly once");
            AssertEqual(metadataBeforeActivation, samus.PoseHistory.LastDifferentDirectionAndMovement,
                "X-ray activation shifts prior metadata exactly once");
            AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is null,
                "Run+Shoot does not allocate a beam on the activation frame");
            for (int frame = 0; frame < 90; frame++)
            {
                runtime.StepFrame((ushort)(runtime.ControllerBindings.Dash | runtime.ControllerBindings.Shoot));
                AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is null,
                    "active scanning does not fire ordinary beams");
                AssertEqual(poseBeforeActivation, samus.PoseHistory.LastDifferentPose,
                    "holding X-ray does not repeatedly commit activation history");
                AssertEqual(metadataBeforeActivation, samus.PoseHistory.LastDifferentDirectionAndMovement,
                    "holding X-ray retains prior transition metadata");
            }
            AssertEqual(XrayBeamPhase.Full, samus.Xray.BeamPhase, "configured Run keeps scan fully expanded");
            for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
            AssertTrue(!samus.Xray.IsActive && !runtime.TimeIsFrozen, "Run release tears down scanning");
            AssertEqual(SamusXrayRomData.SelectedHudItem, samus.SelectedHudItem, "release preserves X-ray selection");
            runtime.StepFrame(runtime.ControllerBindings.Shoot);
            AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is not null,
                "Shoot resumes actual beam firing after scan release without deselecting scope");
            Console.WriteLine($"X-ray controls ({(remapped ? "swapped Run/Shoot" : "default")}): inactive shot, active suppression, release and resumed shot pass.");
        }
    }
}
