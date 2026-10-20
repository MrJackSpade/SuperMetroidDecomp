using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the retail Red Tower upper shootable block under two controlled camera paths.</summary>
internal static class RedTowerHeroVerification
{
    /// <summary>
    /// Isolates the real target and PLM interaction with controlled camera coordinates.
    /// The fixed camera expires the shot; following it lets the same launch hit the block.
    /// This fixture does not demonstrate a controller-only climb.
    /// </summary>
    internal static void VerifyControlledRedTowerHeroShot()
    {
        foreach (bool followShot in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.RedTower);
            var level = runtime.Room.LoadedLevelData;
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.PoseId = SamusPoseId.StandingAimUpRightPose;
            samus.Position = samus.Position.WithWhole(new(116, 587));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);

            // Preserve every retail enemy and terrain block. Activate the upper
            // Rippers before settling the lower viewport; otherwise their initial
            // offscreen positions intercept this shot regardless of the lower wait.
            for (int frame = 0; frame < 120; frame++)
            {
                runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 160));
                runtime.StepFrame(SnesButton.Up);
            }
            runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 450));
            for (int frame = 0; frame < 120; frame++) runtime.StepFrame(SnesButton.Up);
            AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlock(new(7, 10)).CollisionType,
                "Red Tower top target is still intact before firing");
            AssertEqual(587, samus.Position.Y.Whole, "Retail platform supports the launch position");
            int impact = -1, deleted = -1;
            for (int frame = 0; frame < 100; frame++)
            {
                SnesButton input = frame == 0
                    ? SnesButton.Up | runtime.Options.ControllerBindings.Shoot
                    : SnesButton.Up;
                if (followShot && frame > 0 && runtime.Projectiles.Slots[0].IsActive)
                    runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, (ushort)Math.Max(0, runtime.Projectiles.Slots[0].Position.Y.Whole - 160)));
                runtime.StepFrame(input);
                var shot = runtime.Projectiles.Slots[0];
                if (frame == 0)
                {
                    AssertTrue(shot.IsActive, "Retail target probe launches its single shot");
                }
                if (shot.PackedType.Family == SamusProjectileFamily.BeamExplosion)
                {
                    impact = frame;
                    AssertEqual(118, shot.Position.X.Whole, "Shot impacts the target column, not a distant enemy");
                    AssertEqual(174, shot.Position.Y.Whole, "Shot impacts the underside of the retail top block");
                    break;
                }
                if (!shot.IsActive) { deleted = frame; break; }
            }
            AssertEqual(99, samus.Health, "Enemy damage does not interrupt either controlled branch");
            AssertEqual(followShot ? RoomCollisionType.Air : RoomCollisionType.ShootableBlock,
                level.GetCollisionBlock(new(7, 10)).CollisionType,
                "Only the camera-retained shot actually clears the retail shootable block");
            if (followShot) AssertEqual(64, impact, "Retail target breaks on the captured impact frame");
            else
            {
                AssertEqual(36, deleted, "Fixed-camera shot expires on the captured boundary frame");
                AssertEqual(-1, impact, "Fixed-camera shot expires without an impact");
            }
            Console.WriteLine($"Red Tower controlled camera={followShot}: impact={impact}, deletion={deleted}.");
        }
    }

    /// <summary>Runs the two retail Red Tower camera branches.</summary>
    [VerificationCommand("--red-tower-hero")]
    internal static int RedTowerHeroCommand()
    {
        Suite(nameof(VerifyControlledRedTowerHeroShot), () => VerifyControlledRedTowerHeroShot());
        return 0;
    }
}
