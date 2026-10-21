using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks selected X-ray scope input against beam firing and frozen projectile updates.</summary>
internal static class XrayControlsVerification
{
    /// <summary>Checks default and remapped Run/Shoot bindings through activation, held scanning, release, and resumed fire.</summary>
    internal static void VerifyXrayControls()
    {
        foreach (bool remapped in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.XrayScope);
            runtime.Options.ControllerBindings = remapped
                ? ControllerBindings.Default.AssignAndSwap(0, ControllerBindings.Default.Dash)
                : ControllerBindings.Default;
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.Position = samus.Position.WithWhole(new(128, 139));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.EquippedItems = SamusEquipmentFlags.XrayScope;
            samus.EquippedBeams = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.Options.ControllerBindings.ItemSelect);
            AssertEqual(HudItem.XRay, samus.SelectedHudItem, "ordinary input selects X-ray");
            runtime.StepFrame(runtime.Options.ControllerBindings.Shoot);
            AssertTrue(!samus.Xray.IsActive, "Shoot alone does not activate selected X-ray");
            AssertTrue(runtime.Projectiles.LastFrameResult.QueuedSoundEffect is not null,
                "selected inactive X-ray falls through to ordinary beam producer when Run is released");
            for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
            SamusPoseId poseBeforeActivation = samus.PoseHistory.PreviousPose;
            int metadataBeforeActivation = samus.PoseHistory.PreviousDirectionAndMovement;
            // Construct an already-placed bomb, far from Samus, to isolate the shared
            // ten-slot update gate. Its long instruction delay avoids consuming data.
            var pendingBomb = runtime.BombProjectiles.Slots[0];
            pendingBomb.Type = (ushort)SamusProjectileFamily.Bomb;
            pendingBomb.Cursor = SamusProjectilePrograms.Start(SamusProjectileProgram.Bomb);
            pendingBomb.SpritemapPointer = (runtime.Artwork.ProjectileFrameBindings ?? throw new InvalidOperationException("Verification requires runtime.Artwork.ProjectileFrameBindings.")).Resolve(checked((ushort)pendingBomb.InstructionPointer));
            pendingBomb.InstructionTimer = WrappingCountdown.Start(100);
            pendingBomb.BombTimer = WrappingCountdown.Start(60);
            runtime.StepFrame((runtime.Options.ControllerBindings.Dash | runtime.Options.ControllerBindings.Shoot));
            AssertEqual(60, pendingBomb.BombTimer.RawValue, "X-ray activation freezes an existing bomb fuse immediately");
            AssertEqual(100, pendingBomb.InstructionTimer.RawValue, "X-ray activation skips existing bomb animation immediately");
            pendingBomb.ClearFields();
            AssertTrue(samus.Xray.IsActive && runtime.Freeze.TimeIsFrozen,
                "configured Run takes priority over Shoot and activates scanning");
            AssertEqual(samus.Pose, samus.PoseHistory.PreviousPose, "X-ray activation commits interrupted pose");
            AssertEqual(SnesLittleEndian.Word(SamusPoseMetadata.ReadPoseXDirection(samus), (byte)SamusPoseMetadata.ReadMovementType(samus)),
                samus.PoseHistory.PreviousDirectionAndMovement, "X-ray activation commits interrupted metadata");
            AssertEqual(poseBeforeActivation, samus.PoseHistory.LastDifferentPose, "X-ray activation shifts prior pose exactly once");
            AssertEqual(metadataBeforeActivation, samus.PoseHistory.LastDifferentDirectionAndMovement,
                "X-ray activation shifts prior metadata exactly once");
            AssertTrue(runtime.Projectiles.LastFrameResult.QueuedSoundEffect is null,
                "Run+Shoot does not allocate a beam on the activation frame");
            for (int frame = 0; frame < 90; frame++)
            {
                runtime.StepFrame((runtime.Options.ControllerBindings.Dash | runtime.Options.ControllerBindings.Shoot));
                AssertTrue(runtime.Projectiles.LastFrameResult.QueuedSoundEffect is null,
                    "active scanning does not fire ordinary beams");
                AssertEqual(poseBeforeActivation, samus.PoseHistory.LastDifferentPose,
                    "holding X-ray does not repeatedly commit activation history");
                AssertEqual(metadataBeforeActivation, samus.PoseHistory.LastDifferentDirectionAndMovement,
                    "holding X-ray retains prior transition metadata");
            }
            AssertEqual(XrayBeamPhase.Full, samus.Xray.BeamPhase, "configured Run keeps scan fully expanded");
            for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
            AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Run release tears down scanning");
            AssertEqual(HudItem.XRay, samus.SelectedHudItem, "release preserves X-ray selection");
            // $90:AC32 held the cooldown at $20 while time was frozen; it drains only now.
            AssertTrue(runtime.BombProjectiles.CooldownTimer.RawValue is > 0 and <= 0x20,
                "frozen-time cooldown still delays the first post-scan shot");
            int cooldownFrames = 0;
            while (!runtime.BombProjectiles.CooldownTimer.IsZero && cooldownFrames++ <= 0x20)
                runtime.StepFrame(0);
            AssertEqual(0, (int)runtime.BombProjectiles.CooldownTimer.RawValue, "frozen-time cooldown drains after scan release");
            runtime.StepFrame(runtime.Options.ControllerBindings.Shoot);
            AssertTrue(runtime.Projectiles.LastFrameResult.QueuedSoundEffect is not null,
                "Shoot resumes actual beam firing after scan release without deselecting scope");
            Console.WriteLine($"X-ray controls ({(remapped ? "swapped Run/Shoot" : "default")}): inactive shot, active suppression, release and resumed shot pass.");
        }
    }

    /// <summary>Runs the X-ray input and projectile-freeze verification.</summary>
    [VerificationCommand("--xray-controls")]
    internal static int XrayControlsCommand()
    {
        Suite(nameof(VerifyXrayControls), () => VerifyXrayControls());
        return 0;
    }
}
