using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks charge retention across spin and wall-jump Shoot release, then normal-pose consumption.</summary>
internal static class SpinChargeVerification
{
    /// <summary>Regression for #473: spin HUD dispatch preserves charge on Shoot release.</summary>
    internal static void VerifySpinChargePreservation(TestAddressSpace bus, RoomLevelData air,
        SuperMetroid.Core.Samus.Artwork.InstalledProjectileFrameBindings frames)
    {
        foreach (SamusPoseId pose in new[] { SamusPoseId.SpinJumpRightPose, SamusPoseId.SpinJumpLeftPose,
            SamusPoseId.WallJumpRightPose, SamusPoseId.WallJumpLeftPose })
        {
            SamusMovementType movement = pose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose
                ? SamusMovementType.SpinJumping : SamusMovementType.WallJumping;
            WritePoseDefinition(bus, SamusPoseNumbers.Number(pose), [8, SamusMovementTypeNativeWords.Byte(movement), 0, 2, 0, 0, 0, 0]);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
                EquippedBeams = SamusBeamFlags.Charge }.InPose(SamusPoseId.FacingRightNormalPose);
            var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: frames), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: frames));
            for (int frame = 0; frame < 60; frame++)
                projectiles.StepFrame(SamusTerrain.InRoom(air, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), bombs, new SharedGameState(new SharedRandom()));
            AssertEqual(60, projectiles.Charge.UpdatesHeld, "spin charge fixture armed through real producer");
            samus.ChangePoseKeepingAnimation(pose);
            foreach (GameplayAction input in new GameplayAction[] { GameplayAction.Shoot, 0, 0 })
            {
                projectiles.StepFrame(SamusTerrain.InRoom(air, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), bombs, new SharedGameState(new SharedRandom()));
                AssertEqual(60, projectiles.Charge.UpdatesHeld, $"pose {(int)SamusPoseNumbers.Number(pose):X2} preserves charge on hold/release");
                AssertEqual(0, projectiles.Pool.ShotCount.Count, $"pose {(int)SamusPoseNumbers.Number(pose):X2} keeps empty projectile slots");
            }
            // Leaving spin must restore normal release semantics, not lock charge forever.
            samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
            projectiles.StepFrame(SamusTerrain.InRoom(air, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), bombs, new SharedGameState(new SharedRandom()));
            AssertEqual(0, projectiles.Charge.UpdatesHeld, "ordinary pose consumes the retained charge");
        }
    }
}
