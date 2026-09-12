using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyHeroShotRuntimeCamera()
    {
        SamusProjectileSpawnSnapshot? stationaryLaunch = null;
        foreach (bool walkAfterShot in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            var level = runtime.LevelData!;
            // Construct a finite corridor within the loaded room. Do not alter
            // camera/scroll policies or projectile processing to make a shot live.
            for (int y = 16; y < 36; y++)
            for (int x = 16; x < 64; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                    y >= 32 || x == 54 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
                level.SetBehavior(index, 0);
            }
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = 512;
            samus.YPosition = 490;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            runtime.Camera!.SetPosition(400, 350);
            for (int frame = 0; frame < 64; frame++) runtime.StepFrame(0);
            ushort settledCameraX = runtime.Camera.XPosition;
            int impact = -1, deleted = -1;
            for (int frame = 0; frame < 100; frame++)
            {
                ushort input = frame == 0 ? runtime.ControllerBindings.Shoot
                    : walkAfterShot ? (ushort)SnesButton.Right : (ushort)0;
                runtime.StepFrame(input);
                var shot = runtime.Projectiles!.Slots[0];
                if (frame == 0)
                {
                    AssertTrue(shot.IsActive, "Runtime Hero fixture fires from normal gameplay input");
                    var launch = runtime.Projectiles.LastFiredProjectileSnapshot;
                    AssertTrue(launch.HasValue, "Runtime publishes launch evidence before movement");
                    if (!walkAfterShot) stationaryLaunch = launch;
                    else AssertTrue(launch == stationaryLaunch,
                        "Both input branches launch identical position, direction and velocity before walking diverges");
                }
                if (shot.PackedType.Family == SamusProjectileFamily.BeamExplosion)
                {
                    impact = frame;
                    AssertTrue(shot.XPosition >= 856 && shot.XPosition <= 880,
                        "Controller-followed shot impacts the constructed world-space target");
                    break;
                }
                if (!shot.IsActive) { deleted = frame; break; }
            }
            Console.WriteLine($"Runtime Hero walking={walkAfterShot}: impact={impact}, deletion={deleted}, SamusX={samus.XPosition}, cameraX={runtime.Camera.XPosition}.");
            if (walkAfterShot)
            {
                AssertTrue(impact >= 0 && deleted < 0,
                    "Ordinary Samus movement and camera tracking preserve the shot to the target");
                AssertTrue(samus.XPosition > 512 && runtime.Camera.XPosition > settledCameraX,
                    "The actual runtime movement and camera owners advance without test camera writes");
            }
            else
            {
                AssertTrue(deleted >= 0 && impact < 0,
                    "Without following movement the identical shot expires before the target");
                AssertEqual(settledCameraX, runtime.Camera.XPosition,
                    "Stationary control does not drift the camera during the shot lifetime");
            }
        }
    }
}
