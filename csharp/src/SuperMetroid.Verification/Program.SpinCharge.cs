using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static AssertionsVerification;
using static VerificationHelpersVerification;

/// <summary>Represents spin charge verification.</summary>
internal static class SpinChargeVerification
{
    /// <summary>Regression for #473: spin HUD dispatch preserves charge on Shoot release.</summary>
    internal static void VerifySpinChargePreservation(TestAddressSpace bus, RoomLevelData air,
        SuperMetroid.Core.Assets.ProjectileFrameBindingCatalog frames)
    {
        foreach (SamusPoseId pose in new[] { SamusPoseId.SpinJumpRightPose, SamusPoseId.SpinJumpLeftPose,
            SamusPoseId.WallJumpRightPose, SamusPoseId.WallJumpLeftPose })
        {
            SamusMovementType movement = pose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose
                ? SamusMovementType.SpinJumping : SamusMovementType.WallJumping;
            WritePoseDefinition(bus, (int)pose, [8, (byte)movement, 0, 2, 0, 0, 0, 0]);
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
                EquippedBeams = (ushort)SamusBeamFlags.Charge };
            var projectiles = new SamusProjectileSystem { Animation = { FrameBindings = frames } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = frames };
            for (int frame = 0; frame < 60; frame++)
                projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = air }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = 0 }, new(0, 0), bombs);
            AssertEqual(60, projectiles.Charge.FlareCounter, "spin charge fixture armed through real producer");
            samus.Pose = pose;
            foreach (ushort input in new ushort[] { (ushort)SnesButton.X, 0, 0 })
            {
                projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = air }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = 0 }, new(0, 0), bombs);
                AssertEqual(60, projectiles.Charge.FlareCounter, $"pose {(int)pose:X2} preserves charge on hold/release");
                AssertTrue(projectiles.LastFiredProjectileSnapshot is null, $"pose {(int)pose:X2} does not produce a release shot");
                AssertEqual(0, projectiles.SlotTable.ProjectileCounter, $"pose {(int)pose:X2} keeps empty projectile slots");
            }
            // Leaving spin must restore normal release semantics, not lock charge forever.
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = air }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), bombs);
            AssertEqual(0, projectiles.Charge.FlareCounter, "ordinary pose consumes the retained charge");
        }
    }
}
