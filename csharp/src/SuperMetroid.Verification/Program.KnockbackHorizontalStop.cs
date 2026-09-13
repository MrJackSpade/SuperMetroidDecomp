using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKnockbackHorizontalStop()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        foreach (bool right in new[] { false, true })
        {
            var samus = new SamusState { Pose = SamusPoseIds.FacingLeftNormalPose, XPosition = 100, YPosition = 100 };
            samus.RefreshCollisionRadii(bus);
            SamusKnockbackMovement.Start(bus, samus, 0, (ushort)(right ? 1 : 0), level: level);
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.InteractiveEnemies =
            [new SolidEnemyCollisionBody(0, (ushort)(right ? 111 : 89), 100, 5, 8, 0, (ushort)EnemyProperties.SolidToSamus)];
            var clipped = SamusKnockbackMovement.Step(bus, level, samus, 0);
            AssertTrue(clipped.Horizontal is { Collided: true }, "knockback encounters nearby solid enemy");
            AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "clipped knockback clears base momentum immediately");
            AssertEqual(0, samus.HorizontalSpeed.ExtraRunSpeed, "clipped knockback clears extra whole momentum");
            AssertEqual(0, samus.HorizontalSpeed.ExtraRunSubspeed, "clipped knockback clears extra fractional momentum");
            samus.Kinematics.InteractiveEnemies = [];
            uint before = samus.Kinematics.XFixed;
            var clear = SamusKnockbackMovement.Step(bus, level, samus, 1);
            AssertEqual(right ? 98304 : -98304, clear.Horizontal!.Value.AcceptedDisplacement,
                "next clear knockback frame restarts at native 1.5 pixels instead of retaining clipped acceleration");
            AssertEqual(unchecked(before + (uint)clear.Horizontal.Value.AcceptedDisplacement), samus.Kinematics.XFixed,
                "knockback next-frame position uses the reset displacement");
        }
    }
}
