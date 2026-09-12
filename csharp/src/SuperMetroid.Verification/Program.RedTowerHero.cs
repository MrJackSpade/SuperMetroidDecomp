using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // Isolates the real target/PLM interaction with deliberately controlled camera
    // coordinates. This is NOT a successful controller-only Red Tower climb.
    private static void VerifyControlledRedTowerHeroShot()
    {
        SamusProjectileSpawnSnapshot? stationaryLaunch = null;
        foreach (bool followShot in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.RedTower);
            var level = runtime.LevelData!;
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.PoseId = SamusPoseId.StandingAimUpRightPose;
            samus.XPosition = 116;
            samus.YPosition = 587;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);

            // Preserve every retail enemy and terrain block. Activate the upper
            // Rippers before settling the lower viewport; otherwise their initial
            // offscreen positions intercept this shot regardless of the lower wait.
            for (int frame = 0; frame < 120; frame++)
            {
                runtime.Camera!.SetPosition(0, 160);
                runtime.StepFrame((ushort)SnesButton.Up);
            }
            runtime.Camera!.SetPosition(0, 450);
            for (int frame = 0; frame < 120; frame++) runtime.StepFrame((ushort)SnesButton.Up);
            AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlock(7, 10).CollisionType,
                "Red Tower top target is still intact before firing");
            AssertEqual(587, samus.YPosition, "Retail platform supports the launch position");
            int impact = -1, deleted = -1;
            for (int frame = 0; frame < 100; frame++)
            {
                ushort input = frame == 0
                    ? (ushort)((ushort)SnesButton.Up | runtime.ControllerBindings.Shoot)
                    : (ushort)SnesButton.Up;
                if (followShot && frame > 0 && runtime.Projectiles!.Slots[0].IsActive)
                    runtime.Camera.SetPosition(0, (ushort)Math.Max(0, runtime.Projectiles.Slots[0].YPosition - 160));
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
                    AssertEqual(118, shot.XPosition, "Shot impacts the target column, not a distant enemy");
                    AssertEqual(174, shot.YPosition, "Shot impacts the underside of the retail top block");
                    break;
                }
                if (!shot.IsActive) { deleted = frame; break; }
            }
            AssertEqual(99, samus.Health, "Enemy damage does not interrupt either controlled branch");
            AssertEqual(followShot ? RoomCollisionType.Air : RoomCollisionType.ShootableBlock,
                level.GetCollisionBlock(7, 10).CollisionType,
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
}
