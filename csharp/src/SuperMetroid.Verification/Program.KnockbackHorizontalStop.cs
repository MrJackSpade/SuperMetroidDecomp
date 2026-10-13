using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static VerificationHelpersVerification;

/// <summary>Checks that enemy-clipped horizontal knockback clears momentum for the following frame.</summary>
internal static class KnockbackHorizontalStopVerification
{
    /// <summary>Exercises both directions against a nearby solid enemy, then checks the next clear step moves 1.5 pixels.</summary>
    internal static void VerifyKnockbackHorizontalStop()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(new(16, 16), new ushort[256], new byte[256]);
        foreach (bool right in new[] { false, true })
        {
            var samus = new SamusState { Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            SamusKnockbackMovement.Start(bus, samus, 0, new KnockbackLaunch { KnockbackXDirection = (ushort)(right ? 1 : 0) }, level: level);
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Kinematics.InteractiveEnemies =
            [new SolidEnemyCollisionBody(0, new((ushort)(right ? 111 : 89), 100), new(5, 8), 0, (ushort)EnemyProperties.SolidToSamus)];
            uint clippedBefore = samus.Kinematics.Position.X.RawValue;
            SamusKnockbackMovement.Step(bus, level, samus, 0);
            AssertTrue(Math.Abs(unchecked((int)(samus.Kinematics.Position.X.RawValue - clippedBefore))) < 98304,
                "knockback encounters nearby solid enemy and is clipped short of its 1.5-pixel move");
            AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "clipped knockback clears base momentum immediately");
            AssertEqual(0, samus.HorizontalSpeed.ExtraRunSpeed, "clipped knockback clears extra whole momentum");
            AssertEqual(0, samus.HorizontalSpeed.ExtraRunSubspeed, "clipped knockback clears extra fractional momentum");
            samus.Kinematics.InteractiveEnemies = [];
            uint before = samus.Kinematics.Position.X.RawValue;
            SamusKnockbackMovement.Step(bus, level, samus, 1);
            AssertEqual(right ? 98304 : -98304, unchecked((int)(samus.Kinematics.Position.X.RawValue - before)),
                "next clear knockback frame moves by the reset native 1.5 pixels instead of retaining clipped acceleration");
        }
    }
}
