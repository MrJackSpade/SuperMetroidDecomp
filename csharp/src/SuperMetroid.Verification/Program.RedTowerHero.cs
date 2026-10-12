using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class RedTowerHeroVerification
{
    // Isolates the real target/PLM interaction with deliberately controlled camera
    // coordinates. This is NOT a successful controller-only Red Tower climb.
    internal static void VerifyControlledRedTowerHeroShot()
    {
        SamusProjectileSpawnSnapshot? stationaryLaunch = null;
        foreach (bool followShot in new[] { false, true })
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.RedTower);
            var level = runtime.Room.LevelData!;
            var samus = runtime.Player.Samus!;
            samus.InputLocked = false;
            samus.PoseId = SamusPoseId.StandingAimUpRightPose;
            samus.Position = samus.Position.WithWhole(new(116, 587));
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);

            // Preserve every retail enemy and terrain block. Activate the upper
            // Rippers before settling the lower viewport; otherwise their initial
            // offscreen positions intercept this shot regardless of the lower wait.
            for (int frame = 0; frame < 120; frame++)
            {
                runtime.Room.Camera!.SetPosition(new Position16<RoomPixels>(0, 160));
                runtime.StepFrame((ushort)SnesButton.Up);
            }
            runtime.Room.Camera!.SetPosition(new Position16<RoomPixels>(0, 450));
            for (int frame = 0; frame < 120; frame++) runtime.StepFrame((ushort)SnesButton.Up);
            AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlock(new(7, 10)).CollisionType,
                "Red Tower top target is still intact before firing");
            AssertEqual(587, samus.Position.Y.Whole, "Retail platform supports the launch position");
            int impact = -1, deleted = -1;
            for (int frame = 0; frame < 100; frame++)
            {
                ushort input = frame == 0
                    ? (ushort)((ushort)SnesButton.Up | runtime.Options.ControllerBindings.Shoot)
                    : (ushort)SnesButton.Up;
                if (followShot && frame > 0 && runtime.Projectiles!.Slots[0].IsActive)
                    runtime.Room.Camera.SetPosition(new Position16<RoomPixels>(0, (ushort)Math.Max(0, runtime.Projectiles.Slots[0].Position.Y.Whole - 160)));
                runtime.StepFrame(input);
                var shot = runtime.Projectiles!.Slots[0];
                if (frame == 0)
                {
                    AssertTrue(shot.IsActive, "Retail target probe launches its single shot");
                    var launch = runtime.Projectiles.LastFiredProjectileSnapshot;
                    AssertTrue(launch.HasValue, "Retail probe publishes launch evidence");
                    if (!followShot) stationaryLaunch = launch;
                    else AssertTrue(launch == stationaryLaunch, "Camera branches launch identical shots");
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

    [VerificationCommand("--red-tower-hero")]
    internal static int RedTowerHeroCommand()
    {
        Suite(nameof(VerifyControlledRedTowerHeroShot), () => VerifyControlledRedTowerHeroShot());
        return 0;
    }
}
