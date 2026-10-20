using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Frog Speedway ceiling-shot PLM saturation and native room traces.</summary>
internal static class CeilingWrapRoomVerification
{
    /// <summary>Compares production pose, movement, PLM count, and camera state against a supplied native trace.</summary>
    internal static void VerifyFrogSpeedwayRuntimeTrace(string path, ushort beams = 5, bool dash = true)
    {
        string[] native = File.ReadLines(path).Skip(1).ToArray();
        int expectedFrames = beams == 11 ? (dash ? 349 : 412) : 900;
        AssertEqual(expectedFrames, native.Length, "Native Speedway setup trace length");
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.FrogSpeedway);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.Position = samus.Position.WithWhole(new(1237, 139));
        samus.EquippedBeams = (SamusBeamFlags)beams; samus.EquippedItems = 0;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(1109, 0));
        int mismatches = 0;
        for (int frame = 0; frame < native.Length; frame++)
        {
            runtime.StepFrame((SnesButton.X | SnesButton.Left | SnesButton.R | (dash ? SnesButton.B : 0)));
            string actual = $"{frame},{samus.Position.X.Whole},{samus.Kinematics.Position.X.Fraction},{samus.Position.Y.Whole},{(int)samus.Pose:X2},{runtime.Plms.SlotTable.ActiveCount},{runtime.Room.LoadedCamera.PixelPosition.X}";
            if (actual != native[frame] && mismatches++ < 8)
                Console.WriteLine($"Speedway mismatch:\n{actual}\n{native[frame]}");
        }
        AssertEqual(0, mismatches, "Original CPU Speedway input, pose, movement, PLM pressure and camera");
        AssertEqual((ushort)(beams == 11 ? 799 : 842), samus.Position.X.Whole, "Native Speedway success/failure endpoint");
        Console.WriteLine($"PASS {native.Length} native Speedway frames: beams={beams} dash={dash} endpoint={samus.Position.X.Whole}.");
    }

    /// <summary>Checks that a full forty-slot PLM pool permits native speed-block entry without breaking the block.</summary>
    internal static void VerifyFrogSpeedwayPoolCollision()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.FrogSpeedway);
        var level = runtime.Room.LoadedLevelData;
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(1237, 139)),
            PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose, EquippedBeams = (SamusBeamFlags)5 };
        samus.Kinematics.Radius = new(5, 21);
        var plms = runtime.Plms;
        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        ushort blocker = level.GetCollisionBlock(new(76, 9)).PackedWord.Raw;
        var before = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = -65536 });
        AssertTrue(before.Collided, "Ordinary speedless contact blocks before ceiling overload");
        int firstFull = -1;
        for (int frame = 0; frame < 344; frame++)
        {
            SnesButton held = SnesButton.X;
            SnesButton pressed = frame == 0 ? held : SnesButton.None;
            bombs.StepFrame(new SamusTerrain { Level = level }, samus, held, pressed);
            shots.StepFrame(new SamusTerrain { Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = held, ControllerNewInput = pressed }, new(1109, 0), bombs);
            plms.Step(level);
            if (firstFull < 0 && plms.SlotTable.ActiveCount == 40) firstFull = frame;
        }
        AssertEqual(343, firstFull, "Authored room reaches full pool with live PLM timers");
        var after = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = -65536 });
        AssertTrue(!after.Collided, "Native full-pool collision remains carry clear without Speed Booster");
        AssertEqual((ushort)1236, samus.Position.X.Whole, "Samus enters first speed block without a clip or teleport");
        AssertEqual(blocker, level.GetCollisionBlock(new(76, 9)).PackedWord.Raw, "Exhaustion bypass does not break the block");
        AssertEqual(40, plms.SlotTable.ActiveCount, "Contact cannot allocate a forty-first slot");
        Console.WriteLine("PASS Frog Speedway: live ceiling-shot overload permits unchanged speed-block entry at frame 343.");
    }

    /// <summary>Runs the focused full-pool collision check.</summary>
    [VerificationCommand("--ceiling-wrap-room")]
    internal static int CeilingWrapRoomCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
        return 0;
    }

    /// <summary>Compares the original Frog Speedway runtime trace.</summary>
    [VerificationCommand("--ceiling-wrap-runtime")]
    internal static int CeilingWrapRuntimeCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv"));
        return 0;
    }

    /// <summary>Checks successful Speedway traces with and without dash held.</summary>
    [VerificationCommand("--ceiling-wrap-success")]
    internal static int CeilingWrapSuccessCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11, true));
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false));
        return 0;
    }
}
