using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Checks a touching solid enemy during a zero-distance jump and the next hurt-pose radius update.</summary>
internal static class ZeroDistanceJumpContactVerification
{
    /// <summary>Confirms tangency clears Y subposition without fabricating movement, then checks crouch-to-hurt radius timing.</summary>
    internal static void VerifyZeroDistanceJumpContact()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0x8000).WithFractionY(0xffff);
            samus.Kinematics.InteractiveEnemies =
            [
                new SolidEnemyCollisionBody(Index: 0, Position: new((ushort)(100 + (left ? -1 : 1) * (10 + gap)), 100),
                    Radius: new(5, 8), FreezeTimer: (ushort)(frozen ? 1 : 0),
                    Properties: (ushort)(frozen ? default(EnemyProperties) : EnemyProperties.SolidToSamus)),
            ];
            uint initialX = unchecked((uint)samus.Kinematics.Position.X.RawValue);
            var result = SamusAerialMovement.StepNormalJump(level, samus, SnesButton.A, 0);
            AssertEqual(initialX, unchecked((uint)samus.Kinematics.Position.X.RawValue), "zero-distance jump leaves both X words unchanged");
            AssertEqual(100, samus.Position.Y.Whole, "zero-distance contact leaves whole Y unchanged");
            AssertEqual(gap == 0 ? 0 : 0xffff, samus.Kinematics.Position.Y.Fraction,
                "zero-distance jump preserves native enemy tangency fraction write and one-pixel miss");
            // The touching solid-enemy path's only trace is its STZ of Samus's Y subposition.
            AssertTrue((samus.Kinematics.Position.Y.Fraction == 0) == (gap == 0),
                "zero-distance jump reports actual touching enemy, not fabricated movement");
        }

        var crouched = new SamusState { Pose = SamusPoseId.CrouchingLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
        crouched.PoseMetadata.RefreshCollisionRadii();
        SamusKnockbackMovement.Start(crouched, 0, new KnockbackLaunch { KnockbackXDirection = 1, KnockbackTimer = 5 }, level: level);
        AssertEqual(SamusPoseId.KnockbackLeftPose, crouched.Pose, "crouch contact installs hurt pose");
        AssertEqual(16, crouched.Kinematics.Radius.Y, "hurt commit retains live crouch radius");
        crouched.PoseMetadata.RefreshCollisionRadii();
        AssertEqual(21, crouched.Kinematics.Radius.Y, "next alpha publishes hurt radius");
    }
}
