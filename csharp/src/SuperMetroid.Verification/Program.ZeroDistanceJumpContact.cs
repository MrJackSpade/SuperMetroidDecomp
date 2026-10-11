using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyZeroDistanceJumpContact()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(new(16, 16), new ushort[256], new byte[256]);
        foreach (bool left in new[] { false, true })
        foreach (bool frozen in new[] { false, true })
        foreach (int gap in new[] { 0, 1 })
        {
            var samus = new SamusState
            {
                Pose = left ? SamusPoseId.NeutralJumpTransitionLeftPose : SamusPoseId.NeutralJumpTransitionRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
            };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x8000).WithFractionY(0xffff);
            samus.Kinematics.InteractiveEnemies =
            [
                new SolidEnemyCollisionBody(Index: 0, Position: new((ushort)(100 + (left ? -1 : 1) * (10 + gap)), 100),
                    Radius: new(5, 8), FreezeTimer: (ushort)(frozen ? 1 : 0),
                    Properties: (ushort)(frozen ? default(EnemyProperties) : EnemyProperties.SolidToSamus)),
            ];
            uint initialX = unchecked((uint)samus.Kinematics.Position.X.RawValue);
            var result = SamusAerialMovement.StepNormalJump(bus, level, samus, (ushort)SnesButton.A, 0);
            AssertEqual(initialX, unchecked((uint)samus.Kinematics.Position.X.RawValue), "zero-distance jump leaves both X words unchanged");
            AssertEqual(100, samus.Position.Y.Whole, "zero-distance contact leaves whole Y unchanged");
            AssertEqual(gap == 0 ? 0 : 0xffff, samus.Kinematics.Position.Y.Fraction,
                "zero-distance jump preserves native enemy tangency fraction write and one-pixel miss");
            // The touching solid-enemy path's only trace is its STZ of Samus's Y subposition.
            AssertTrue((samus.Kinematics.Position.Y.Fraction == 0) == (gap == 0),
                "zero-distance jump reports actual touching enemy, not fabricated movement");
        }

        var crouched = new SamusState { Pose = SamusPoseId.CrouchingLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
        crouched.RefreshCollisionRadii(bus);
        SamusKnockbackMovement.Start(bus, crouched, 0, 1, 5, level: level);
        AssertEqual(SamusPoseId.KnockbackLeftPose, crouched.Pose, "crouch contact installs hurt pose");
        AssertEqual(16, crouched.Kinematics.Radius.Y, "hurt commit retains live crouch radius");
        crouched.RefreshCollisionRadii(bus);
        AssertEqual(21, crouched.Kinematics.Radius.Y, "next alpha publishes hurt radius");
    }
}
