using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Runtime;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks a fired beam's lifetime against Samus movement, camera tracking, and a native trace.</summary>
internal static class HeroShotRuntimeVerification
{
    /// <summary>Runs stationary and walking shot paths through production gameplay and compares recorded state fields.</summary>
    internal static void VerifyHeroShotRuntimeCamera(string? nativeTrace = null)
    {
        Suite(nameof(VerifyPoseCollisionCameraCheckpoint), () => VerifyPoseCollisionCameraCheckpoint());
        var native = nativeTrace is null ? null : File.ReadLines(nativeTrace).Skip(1)
            .Select(line => line.Split(','))
            .ToDictionary(row => new HeroShotRuntimeNative(int.Parse(row[0]), int.Parse(row[1])), row => row.Skip(2)
                .Select(value => Convert.ToUInt16(value, 16)).ToArray());
        int compared = 0;
        int mismatches = 0;
        var fieldMismatches = new int[15];
        foreach (bool walkAfterShot in new[] { false, true })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
            var level = runtime.Room.LoadedLevelData;
            // Construct a finite corridor within the loaded room. Do not alter
            // camera/scroll policies or projectile processing to make a shot live.
            for (int y = 16; y < 36; y++)
            for (int x = 16; x < 64; x++)
            {
                int index = y * level.Size.X + x;
                level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                    y >= 32 || x == 54 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
                level.SetBehavior(index, 0);
            }
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.Position = samus.Position.WithWhole(new(512, 490));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(400, 350));
            for (int frame = -64; frame < 0; frame++)
            {
                runtime.StepFrame(0);
                CompareNative(frame);
            }
            ushort settledCameraX = runtime.Room.LoadedCamera.PixelPosition.X;
            int impact = -1, deleted = -1;
            void CompareNative(int frame)
            {
                var shot = runtime.Projectiles.Slots[0];
                if (native is not null)
                {
                    ushort[] actual = [samus.Position.X.Whole, samus.Kinematics.Position.X.Fraction,
                        samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction, (ushort)samus.Pose,
                        runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y,
                        shot.Position.X.Whole, shot.Position.X.Fraction, shot.Position.Y.Whole, shot.Position.Y.Fraction,
                        unchecked((ushort)shot.Velocity.X.SignedRawValue), unchecked((ushort)shot.Velocity.Y.SignedRawValue), shot.PackedType.Raw, checked((ushort)shot.InstructionPointer)];
                    var expected = native[new HeroShotRuntimeNative(walkAfterShot ? 1 : 0, frame)];
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
                SnesButton input = frame == 0 ? runtime.Options.ControllerBindings.Shoot
                    : walkAfterShot ? SnesButton.Right : 0;
                runtime.StepFrame(input);
                var shot = runtime.Projectiles.Slots[0];
                CompareNative(frame);
                if (frame == 0)
                {
                    AssertTrue(shot.IsActive, "Runtime Hero fixture fires from normal gameplay input");
                }
                if (shot.PackedType.Family == SamusProjectileFamily.BeamExplosion)
                {
                    impact = frame;
                    AssertTrue(shot.Position.X.Whole is >= 856 and <= 880,
                        "Controller-followed shot impacts the constructed world-space target");
                    break;
                }
                if (!shot.IsActive) { deleted = frame; break; }
            }
            Console.WriteLine($"Runtime Hero walking={walkAfterShot}: impact={impact}, deletion={deleted}, SamusX={samus.Position.X.Whole}, cameraX={runtime.Room.LoadedCamera.PixelPosition.X}.");
            if (walkAfterShot)
            {
                AssertTrue(impact >= 0 && deleted < 0,
                    "Ordinary Samus movement and camera tracking preserve the shot to the target");
                AssertTrue(samus.Position.X.Whole > 512 && runtime.Room.LoadedCamera.PixelPosition.X > settledCameraX,
                    "The actual runtime movement and camera owners advance without test camera writes");
            }
            else
            {
                AssertTrue(deleted >= 0 && impact < 0,
                    "Without following movement the identical shot expires before the target");
                AssertEqual(settledCameraX, runtime.Room.LoadedCamera.PixelPosition.X,
                    "Stationary control does not drift the camera during the shot lifetime");
            }
        }
        if (native is not null) AssertEqual(native.Count, compared, "All native runtime Hero frames are compared");
        if (mismatches != 0) Console.WriteLine($"Runtime Hero mismatches by CSV field: {string.Join(',', fieldMismatches)}");
        AssertEqual(0, mismatches, "Runtime Hero states match native movement/camera/projectile frames exactly");
    }

    /// <summary>Checks that landing pose correction changes previous Y once without disturbing camera X or fractions.</summary>
    private static void VerifyPoseCollisionCameraCheckpoint()
    {
        var blocks = new ushort[16 * 32];
        for (int x = 0; x < 16; x++) blocks[31 * 16 + x] = 0x8000;
        var level = CreateRoom(new(16, 32), blocks, new byte[blocks.Length]);
        var samus = new SamusState { Pose = SamusPoseId.FallingRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 477)) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        AssertTrue(SamusPoseTransitions.TryApplyAerialLanding(samus, level, false, 0, 0),
            "Constructed landing reaches actual larger-pose collision correction");
        AssertEqual((ushort)475, samus.Position.Y.Whole, "Landing correction preserves the old bottom boundary");
        AssertEqual(19, samus.Kinematics.Radius.Y, "landing commit retains falling radius");
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        AssertEqual(21, samus.Kinematics.Radius.Y, "next alpha publishes landing radius");
        AssertEqual(475, samus.Position.Y.Whole, "alpha does not repeat landing correction");
        var previous = FixedPosition<RoomPixels>.FromWords(120, 0x1234, 477, 0xabcd);
        AssertEqual(previous.WithY(previous.Y.WithWhole(475)), samus.ApplyPreviousPositionWrites(previous),
            "Native pose correction changes only previous Y, preserving both X words and previous Y fraction");
        AssertEqual(previous, samus.ApplyPreviousPositionWrites(previous),
            "Checkpoint event is consumed once before normal scrolling replaces it");
        var unobstructed = new SamusState { Pose = SamusPoseId.FallingRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 400)) };
        SamusPoseMetadata.RefreshCollisionRadii(unobstructed);
        AssertTrue(SamusPoseTransitions.TryApplyAerialLanding(unobstructed, level, false, 0, 0),
            "Unobstructed pose expansion takes the no-correction branch");
        AssertEqual(previous, unobstructed.ApplyPreviousPositionWrites(previous),
            "No-collision pose expansion leaves the existing camera checkpoint untouched");
    }

    /// <summary>Dispatches the Hero shot runtime comparison with its checked-in native trace.</summary>
    [VerificationCommand("--hero-shot-runtime")]
    internal static int HeroShotRuntimeCommand()
    {
        Suite(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv"));
        return 0;
    }

    /// <summary>Dispatches the Hero shot runtime comparison with a supplied native trace path.</summary>
    [VerificationCommand("--hero-shot-runtime", "nativeHeroRuntimeTrace")]
    internal static int HeroShotRuntimeCommand2(string nativeHeroRuntimeTrace)
    {
        Suite(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera(nativeHeroRuntimeTrace));
        return 0;
    }
}
