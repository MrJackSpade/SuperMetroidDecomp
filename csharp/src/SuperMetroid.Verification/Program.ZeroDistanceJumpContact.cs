using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyZeroDistanceJumpContact()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        foreach (bool left in new[] { false, true })
        foreach (bool frozen in new[] { false, true })
        foreach (int gap in new[] { 0, 1 })
        {
            var samus = new SamusState
            {
                Pose = left ? SamusPoseIds.NeutralJumpTransitionLeftPose : SamusPoseIds.NeutralJumpTransitionRightPose,
                XPosition = 100, YPosition = 100,
            };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.XSubposition = 0x8000;
            samus.Kinematics.YSubposition = 0xffff;
            samus.Kinematics.InteractiveEnemies =
            [
                new SolidEnemyCollisionBody(Index: 0, XPosition: (ushort)(100 + (left ? -1 : 1) * (10 + gap)),
                    YPosition: 100, XRadius: 5, YRadius: 8, FreezeTimer: (ushort)(frozen ? 1 : 0),
                    Properties: (ushort)(frozen ? EnemyProperties.None : EnemyProperties.SolidToSamus)),
            ];
            uint initialX = unchecked((uint)samus.Kinematics.XFixed);
            var result = SamusAerialMovement.StepNormalJump(bus, level, samus, (ushort)SnesButton.A, 0);
            AssertEqual(initialX, unchecked((uint)samus.Kinematics.XFixed), "zero-distance jump leaves both X words unchanged");
            AssertEqual(100, samus.YPosition, "zero-distance contact leaves whole Y unchanged");
            AssertEqual(gap == 0 ? 0 : 0xffff, samus.Kinematics.YSubposition,
                "zero-distance jump preserves native enemy tangency fraction write and one-pixel miss");
            AssertTrue(result.Horizontal.EnemyCollision is { WasTouching: true } == (gap == 0),
                "zero-distance jump reports actual touching enemy, not fabricated movement");
            AssertTrue(result.Vertical is null, "zero-distance contact does not invent vertical movement");
        }

        var crouched = new SamusState { Pose = SamusPoseIds.CrouchingLeftPose, XPosition = 100, YPosition = 100 };
        crouched.RefreshCollisionRadii(bus);
        SamusKnockbackMovement.Start(bus, crouched, 0, 1, 5, level: level);
        AssertEqual(SamusPoseIds.KnockbackLeftPose, crouched.Pose, "crouch contact installs hurt pose");
        AssertEqual(16, crouched.Kinematics.YRadius, "hurt commit retains live crouch radius");
        crouched.RefreshCollisionRadii(bus);
        AssertEqual(21, crouched.Kinematics.YRadius, "next alpha publishes hurt radius");
    }
}
