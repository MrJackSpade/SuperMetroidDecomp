using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Regression for #473: spin HUD dispatch preserves charge on Shoot release.</summary>
    private static void VerifySpinChargePreservation(TestAddressSpace bus, RoomLevelData air)
    {
        foreach (byte pose in new[] { SamusPoseIds.SpinJumpRightPose, SamusPoseIds.SpinJumpLeftPose,
            SamusPoseIds.WallJumpRightPose, SamusPoseIds.WallJumpLeftPose })
        {
            SamusMovementType movement = pose is SamusPoseIds.SpinJumpRightPose or SamusPoseIds.SpinJumpLeftPose
                ? SamusMovementType.SpinJumping : SamusMovementType.WallJumping;
            WritePoseDefinition(bus, pose, [8, (byte)movement, 0, 2, 0, 0, 0, 0]);
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 96,
                EquippedBeams = (ushort)SamusBeamFlags.Charge };
            var projectiles = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 60; frame++)
                projectiles.StepFrame(bus, air, samus, (ushort)SnesButton.X, 0, 0, 0, bombs);
            AssertEqual(60, projectiles.FlareCounter, "spin charge fixture armed through real producer");
            samus.Pose = pose;
            foreach (ushort input in new ushort[] { (ushort)SnesButton.X, 0, 0 })
            {
                var result = projectiles.StepFrame(bus, air, samus, input, 0, 0, 0, bombs);
                AssertEqual(60, projectiles.FlareCounter, $"pose {pose:X2} preserves charge on hold/release");
                AssertEqual((int?)null, result.FiredSlot, $"pose {pose:X2} does not produce a release shot");
                AssertEqual(0, projectiles.ProjectileCounter, $"pose {pose:X2} keeps empty projectile slots");
            }
            // Leaving spin must restore normal release semantics, not lock charge forever.
            samus.Pose = 1;
            projectiles.StepFrame(bus, air, samus, 0, 0, 0, 0, bombs);
            AssertEqual(0, projectiles.FlareCounter, "ordinary pose consumes the retained charge");
        }
    }
}
