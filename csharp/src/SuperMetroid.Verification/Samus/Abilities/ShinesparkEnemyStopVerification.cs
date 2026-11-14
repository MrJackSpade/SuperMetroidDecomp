using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Samus.Movement;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks upward shinespark collision against nearby solid and frozen enemies.</summary>
internal static class ShinesparkEnemyStopVerification
{
    /// <summary>Checks exact crash position at a four-pixel gap and continued vertical flight at a twenty-pixel gap.</summary>
    internal static void VerifyShinesparkEnemyStop()
    {
        var level = CreateRoom(new(16, 16), new RoomTerrainBlock[256], new RoomBlockBehavior[256]);
        foreach (bool frozen in new[] { false, true })
        foreach (int gap in new[] { 4, 20 })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)), Health = 399 };
            samus.Shinespark.TryStoreFromSpeedBooster(SamusSpeedBoostCounter.AtFullStage);
            samus.Shinespark.BeginWindup(samus);
            samus.Shinespark.BeginDirectionalLaunch(samus, SamusPoseId.ShinesparkVerticalLeftPose);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(ushort.MaxValue);
            var enemy = new SolidEnemyFixture();
            samus.Kinematics.InteractiveEnemies =
            [new SolidEnemyCollisionBody(Enemy: enemy, Position: new(100, (ushort)(100 - 19 - 8 - gap)),
                Radius: new(8, 8), Solid: true)];
            FixedCoordinate initialY = samus.Kinematics.Position.Y;
            samus.Shinespark.Step(new SamusTerrain { Level = level }, samus, new ShinesparkFrame { FrameCounter = 0, Projectiles = new SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem(new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.SlotCount)) });
            if (gap == 4)
            {
                AssertEqual(initialY, samus.Kinematics.Position.Y, "spark enemy hit retains exact crash Y instead of advancing to boundary");
                AssertEqual(SignedSixteenSixteen.Zero, samus.Kinematics.Position.Y - initialY, "spark solid-enemy stop accepts no vertical displacement");
                AssertTrue(samus.Kinematics.DidCollideWithSolidEnemy(enemy), "spark stop retains enemy collision evidence");
                AssertEqual(ShinesparkPhase.Crash, samus.Shinespark.Phase, "spark enemy hit starts crash");
            }
            else
            {
                AssertTrue((samus.Kinematics.Position.Y - initialY).IsNegative, "clear spark enemy probe still moves upward");
                AssertEqual(ShinesparkPhase.Vertical, samus.Shinespark.Phase, "distant enemy does not force crash");
            }
        }
    }
}
