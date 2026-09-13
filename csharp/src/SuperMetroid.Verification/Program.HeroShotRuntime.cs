using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyHeroShotRuntimeCamera(string? nativeTrace = null)
    {
        VerifyPoseCollisionCameraCheckpoint();
        var native = nativeTrace is null ? null : File.ReadLines(nativeTrace).Skip(1)
            .Select(line => line.Split(','))
            .ToDictionary(row => (int.Parse(row[0]), int.Parse(row[1])), row => row.Skip(2)
                .Select(value => Convert.ToUInt16(value, 16)).ToArray());
        int compared = 0;
        int mismatches = 0;
        var fieldMismatches = new int[15];
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
            for (int frame = -64; frame < 0; frame++)
            {
                runtime.StepFrame(0);
                CompareNative(frame);
            }
            ushort settledCameraX = runtime.Camera.XPosition;
            int impact = -1, deleted = -1;
            void CompareNative(int frame)
            {
                var shot = runtime.Projectiles!.Slots[0];
                if (native is not null)
                {
                    ushort[] actual = [samus.XPosition, samus.Kinematics.XSubposition,
                        samus.YPosition, samus.Kinematics.YSubposition, samus.Pose,
                        runtime.Camera.XPosition, runtime.Camera.YPosition,
                        shot.XPosition, shot.XSubposition, shot.YPosition, shot.YSubposition,
                        unchecked((ushort)shot.XVelocity), unchecked((ushort)shot.YVelocity), shot.Type, shot.InstructionPointer];
                    var expected = native[(walkAfterShot ? 1 : 0, frame)];
                    if (!actual.SequenceEqual(expected))
                    {
                        mismatches++;
                        for (int field = 0; field < actual.Length; field++)
                            if (actual[field] != expected[field]) fieldMismatches[field]++;
                        if (mismatches <= 8) Console.WriteLine(
                            $"Native runtime Hero walking={walkAfterShot}, frame={frame}: expected {string.Join(',', expected.Select(v => v.ToString("X4")))}; actual {string.Join(',', actual.Select(v => v.ToString("X4")))}");
                    }
                    compared++;
                }
            }
            for (int frame = 0; frame < 100; frame++)
            {
                ushort input = frame == 0 ? runtime.ControllerBindings.Shoot
                    : walkAfterShot ? (ushort)SnesButton.Right : (ushort)0;
                runtime.StepFrame(input);
                var shot = runtime.Projectiles!.Slots[0];
                CompareNative(frame);
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
        if (native is not null) AssertEqual(native.Count, compared, "All native runtime Hero frames are compared");
        if (mismatches != 0) Console.WriteLine($"Runtime Hero mismatches by CSV field: {string.Join(',', fieldMismatches)}");
        AssertEqual(0, mismatches, "Runtime Hero states match native movement/camera/projectile frames exactly");
    }

    private static void VerifyPoseCollisionCameraCheckpoint()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var blocks = new ushort[16 * 32];
        for (int x = 0; x < 16; x++) blocks[31 * 16 + x] = 0x8000;
        var level = CreateRoom(16, 32, blocks, new byte[blocks.Length]);
        var samus = new SamusState { Pose = 0x29, XPosition = 128, YPosition = 477 };
        samus.RefreshCollisionRadii(bus);
        AssertTrue(samus.TryApplyAerialLanding(bus, level, false, 0, 0),
            "Constructed landing reaches actual larger-pose collision correction");
        AssertEqual((ushort)475, samus.YPosition, "Landing correction preserves the old bottom boundary");
        AssertEqual(19, samus.Kinematics.YRadius, "landing commit retains falling radius");
        samus.RefreshCollisionRadii(bus);
        AssertEqual(21, samus.Kinematics.YRadius, "next alpha publishes landing radius");
        AssertEqual(475, samus.YPosition, "alpha does not repeat landing correction");
        var previous = new SamusCameraPoint(120, 0x1234, 477, 0xabcd);
        AssertEqual(previous with { YPosition = 475 }, samus.ApplyPoseCollisionCameraCheckpoint(previous),
            "Native pose correction changes only previous Y, preserving both X words and previous Y fraction");
        AssertEqual(previous, samus.ApplyPoseCollisionCameraCheckpoint(previous),
            "Checkpoint event is consumed once before normal scrolling replaces it");
        var unobstructed = new SamusState { Pose = 0x29, XPosition = 128, YPosition = 400 };
        unobstructed.RefreshCollisionRadii(bus);
        AssertTrue(unobstructed.TryApplyAerialLanding(bus, level, false, 0, 0),
            "Unobstructed pose expansion takes the no-correction branch");
        AssertEqual(previous, unobstructed.ApplyPoseCollisionCameraCheckpoint(previous),
            "No-collision pose expansion leaves the existing camera checkpoint untouched");
    }
}
