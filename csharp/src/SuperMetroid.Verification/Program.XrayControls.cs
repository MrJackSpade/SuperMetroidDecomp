using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class XrayControlsVerification
{
    internal static void VerifyXrayControls()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool remapped in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShutterRoom);
            runtime.Options.ControllerBindings = remapped
                ? ControllerBindings.Default.AssignAndSwap(0, ControllerBindings.Default.Dash)
                : ControllerBindings.Default;
            var samus = runtime.Player.Samus!;
            samus.InputLocked = false;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.Position = samus.Position.WithWhole(new(128, 139));
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
            samus.EquippedBeams = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.Options.ControllerBindings.ItemSelect);
            AssertEqual(SamusXrayRomData.SelectedHudItem, samus.SelectedHudItem, "ordinary input selects X-ray");
            runtime.StepFrame(runtime.Options.ControllerBindings.Shoot);
            AssertTrue(!samus.Xray.IsActive, "Shoot alone does not activate selected X-ray");
            AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is not null,
                "selected inactive X-ray falls through to ordinary beam producer when Run is released");
            for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
            SamusPoseId poseBeforeActivation = samus.PoseHistory.PreviousPose;
            ushort metadataBeforeActivation = samus.PoseHistory.PreviousDirectionAndMovement;
            // Construct an already-placed bomb, far from Samus, to isolate the shared
            // ten-slot update gate. Its long instruction delay avoids consuming data.
            var pendingBomb = runtime.BombProjectiles.Slots[0];
            pendingBomb.Type = (ushort)SamusProjectileFamily.Bomb;
            ushort bombData = SamusProjectileSelectionDefinitions.ReadWord(
                SamusProjectileRomData.NonBeam.DataPointers +
                ((ushort)SamusProjectileFamily.Bomb >> 8) * sizeof(ushort));
            pendingBomb.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
                (SamusProjectileRomData.Banks.Projectile | bombData) + sizeof(ushort));
            pendingBomb.SpritemapPointer = runtime.Artwork.ProjectileFrameBindings!.Resolve(pendingBomb.InstructionPointer);
            pendingBomb.InstructionTimer = 100;
            pendingBomb.BombTimer = 60;
            runtime.StepFrame((ushort)(runtime.Options.ControllerBindings.Dash | runtime.Options.ControllerBindings.Shoot));
            AssertEqual(60, pendingBomb.BombTimer, "X-ray activation freezes an existing bomb fuse immediately");
            AssertEqual(100, pendingBomb.InstructionTimer, "X-ray activation skips existing bomb animation immediately");
            pendingBomb.ClearFields();
            AssertTrue(samus.Xray.IsActive && runtime.Freeze.TimeIsFrozen,
                "configured Run takes priority over Shoot and activates scanning");
            AssertEqual(samus.Pose, samus.PoseHistory.PreviousPose, "X-ray activation commits interrupted pose");
            AssertEqual(SnesLittleEndian.Word(samus.PoseMetadata.ReadPoseXDirection(bus), (byte)samus.PoseMetadata.ReadMovementType(bus)),
                samus.PoseHistory.PreviousDirectionAndMovement, "X-ray activation commits interrupted metadata");
            AssertEqual(poseBeforeActivation, samus.PoseHistory.LastDifferentPose, "X-ray activation shifts prior pose exactly once");
            AssertEqual(metadataBeforeActivation, samus.PoseHistory.LastDifferentDirectionAndMovement,
                "X-ray activation shifts prior metadata exactly once");
            AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is null,
                "Run+Shoot does not allocate a beam on the activation frame");
            for (int frame = 0; frame < 90; frame++)
            {
                runtime.StepFrame((ushort)(runtime.Options.ControllerBindings.Dash | runtime.Options.ControllerBindings.Shoot));
                AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is null,
                    "active scanning does not fire ordinary beams");
                AssertEqual(poseBeforeActivation, samus.PoseHistory.LastDifferentPose,
                    "holding X-ray does not repeatedly commit activation history");
                AssertEqual(metadataBeforeActivation, samus.PoseHistory.LastDifferentDirectionAndMovement,
                    "holding X-ray retains prior transition metadata");
            }
            AssertEqual(XrayBeamPhase.Full, samus.Xray.BeamPhase, "configured Run keeps scan fully expanded");
            for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
            AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Run release tears down scanning");
            AssertEqual(SamusXrayRomData.SelectedHudItem, samus.SelectedHudItem, "release preserves X-ray selection");
            // $90:AC32 held the cooldown at $20 while time was frozen; it drains only now.
            AssertTrue(runtime.BombProjectiles.CooldownTimer is > 0 and <= 0x20,
                "frozen-time cooldown still delays the first post-scan shot");
            int cooldownFrames = 0;
            while (runtime.BombProjectiles.CooldownTimer != 0 && cooldownFrames++ <= 0x20)
                runtime.StepFrame(0);
            AssertEqual(0, (int)runtime.BombProjectiles.CooldownTimer, "frozen-time cooldown drains after scan release");
            runtime.StepFrame(runtime.Options.ControllerBindings.Shoot);
            AssertTrue(runtime.Projectiles.LastFiredProjectileSnapshot is not null,
                "Shoot resumes actual beam firing after scan release without deselecting scope");
            Console.WriteLine($"X-ray controls ({(remapped ? "swapped Run/Shoot" : "default")}): inactive shot, active suppression, release and resumed shot pass.");
        }
    }

    [VerificationCommand("--xray-controls")]
    internal static int XrayControlsCommand()
    {
        Suite(nameof(VerifyXrayControls), () => VerifyXrayControls());
        return 0;
    }
}
