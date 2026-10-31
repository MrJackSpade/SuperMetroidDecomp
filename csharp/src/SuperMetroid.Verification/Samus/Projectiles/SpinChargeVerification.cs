using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
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
            WritePoseDefinition(bus, (int)pose, [8, (byte)movement, 0, 2, 0, 0, 0, 0]);
            var samus = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
                EquippedBeams = SamusBeamFlags.Charge };
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None, InstalledFixtures.SamusOverreads) { Animation = { FrameBindings = frames } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = frames };
            for (int frame = 0; frame < 60; frame++)
                projectiles.StepFrame(new SamusTerrain { Level = air }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = 0 }, new(0, 0), bombs);
            AssertEqual(60, projectiles.Charge.FlareCounter, "spin charge fixture armed through real producer");
            samus.Pose = pose;
            foreach (ControllerButton input in new ControllerButton[] { ControllerButton.X, 0, 0 })
            {
                projectiles.StepFrame(new SamusTerrain { Level = air }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = 0 }, new(0, 0), bombs);
                AssertEqual(60, projectiles.Charge.FlareCounter, $"pose {(int)pose:X2} preserves charge on hold/release");
                AssertEqual(0, projectiles.SlotTable.ProjectileCounter, $"pose {(int)pose:X2} keeps empty projectile slots");
            }
            // Leaving spin must restore normal release semantics, not lock charge forever.
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            projectiles.StepFrame(new SamusTerrain { Level = air }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), bombs);
            AssertEqual(0, projectiles.Charge.FlareCounter, "ordinary pose consumes the retained charge");
        }
    }
}
