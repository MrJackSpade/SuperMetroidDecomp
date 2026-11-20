using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Speed Booster contact damage while a mockball remains in motion.</summary>
internal static class MockballBoostContactVerification
{
    /// <summary>Confirms 500 damage across moving Morph and Spring Ball frames, then threshold and stop behavior.</summary>
    internal static void VerifyMockballBoostContact()
    {
        // The report concerns the retained boost after a mockball has landed. Exercise
        // the real moving-ball handler, then the enemy contact pass that consumes its
        // publication on the following frame, as the native alpha/beta ordering does.
        foreach (SamusPoseId pose in new[] { SamusPoseId.MorphBallMovingRightPose, SamusPoseId.SpringBallMovingRightPose })
        {
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 57)),
                EquippedItems = (SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.MorphBall),
            };
            var fixture = CreateEnemyDropFixture(samus, [1]);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            RoomTerrainBlock[] blocks = new RoomTerrainBlock[32 * 8];
            for (int x = 0; x < 32; x++) blocks[4 * 32 + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
            var floor = CreateRoom(new(32, 8), blocks, new RoomBlockBehavior[blocks.Length]);
            var speed = samus.HorizontalSpeed;
            speed.HasRunningMomentum = true;
            speed.ExtraRunSpeed = speed.ExtraRunSpeed.WithWhole(7);
            speed.SpeedBoostCounter = new SamusSpeedBoostCounter(4, 1);
            var enemy = fixture.System.Places.InUpdateOrder[0];
            enemy.Bind(EnemyPlaceFixtures.StandIn, enemy.KeptDefinition);
            enemy.Bind(enemy.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank with
            {
                Touch = EnemyReaction.OrdinaryTouch,
            });
            enemy.Radius = new(8, 8);
            enemy.Frame = EnemyFrameId.SkulteraSwimmingLeftTailStraight; // Contact admits only an enemy showing a frame.
            for (int frame = 0; frame < 3; frame++)
            {
                speed.BodyAttack = SamusBodyAttack.None; // Runtime beta clears before movement.
                SamusMorphBallMovement.StepGrounded(floor, samus, (ushort)frame, roomObjects: SamusRoomObjectFixtures.Uninstalled);
                enemy.Position = enemy.Position.WithWhole(samus.Position.Whole);
                enemy.Health = 1000;
                samus.InvincibilityTimer = WrappingCountdown.Start(1); // Non-attacking control cannot hurt Samus.
                fixture.System.ProcessingWindow.DetermineWhichEnemiesToProcess(Position16<RoomPixels>.Zero);
                fixture.System.Collision.ResolveOrdinarySamusContact(samus, RoomEnemyFrameFixtures.StandalonePseudoScrew(), 0, floor, enemy.HeldOccupant(), Position16<RoomPixels>.Zero);
                AssertEqual(500, enemy.Health, $"pose {(int)SamusPoseNumbers.Number(pose):X2} mockball frame {frame} inflicts native 500 boost damage");
                AssertEqual(SamusBodyAttack.SpeedBooster, speed.BodyAttack, "moving ball republishes Speed Booster contact attack");
                AssertEqual(7, speed.ExtraRunSpeed.Whole, "moving ball retains boost momentum");
            }
            speed.SpeedBoostCounter = new SamusSpeedBoostCounter(3, 1);
            speed.BodyAttack = SamusBodyAttack.None;
            SamusMorphBallMovement.StepGrounded(floor, samus, 4, roomObjects: SamusRoomObjectFixtures.Uninstalled);
            AssertEqual(SamusBodyAttack.None, speed.BodyAttack, "sub-threshold boost does not grant a ball contact attack");
            speed.SpeedBoostCounter = new SamusSpeedBoostCounter(4, 1);
            speed.BodyAttack = SamusBodyAttack.None;
            samus.Pose = pose == SamusPoseId.MorphBallMovingRightPose
                ? SamusPoseId.MorphBallGroundRightPose : SamusPoseId.SpringBallGroundRightPose;
            speed.AccelerationMode = SamusAccelerationMode.Accelerating;
            SamusMorphBallMovement.StepGrounded(floor, samus, 5, roomObjects: SamusRoomObjectFixtures.Uninstalled);
            AssertEqual(SamusBodyAttack.None, speed.BodyAttack, "stationary ball does not run the moving-ball boost publication");
            AssertEqual(new SamusSpeedBoostCounter(0, 0), speed.SpeedBoostCounter, "stopping retains native boost cancellation");
        }
        Console.WriteLine("Mockball boost: moving Morph/Spring Ball republishes contact damage for consecutive frames, deals 500 damage, and respects boost threshold and stopping.");
    }

    /// <summary>Runs the focused mockball contact-damage regression.</summary>
    [VerificationCommand("--mockball-boost-contact")]
    internal static int MockballBoostContactCommand()
    {
     VerifyMockballBoostContact(); return 0; 
    }
}
