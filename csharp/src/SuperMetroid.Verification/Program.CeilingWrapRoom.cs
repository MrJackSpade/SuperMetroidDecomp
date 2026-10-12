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

internal static class CeilingWrapRoomVerification
{
    internal static void VerifyFrogSpeedwayRuntimeTrace(string path, ushort beams = 5, bool dash = true)
    {
        string[] native = File.ReadLines(path).Skip(1).ToArray();
        int expectedFrames = beams == 11 ? (dash ? 349 : 412) : 900;
        AssertEqual(expectedFrames, native.Length, "Native Speedway setup trace length");
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.Position = samus.Position.WithWhole(new(1237, 139));
        samus.EquippedBeams = beams; samus.EquippedItems = 0;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.Room.Camera!.SetPosition(new Position16<RoomPixels>(1109, 0));
        int mismatches = 0;
        for (int frame = 0; frame < native.Length; frame++)
        {
            runtime.StepFrame((ushort)(SnesButton.X | SnesButton.Left | SnesButton.R | (dash ? SnesButton.B : 0)));
            string actual = $"{frame},{samus.Position.X.Whole},{samus.Kinematics.Position.X.Fraction},{samus.Position.Y.Whole},{(int)samus.Pose:X2},{runtime.Plms.SlotTable.ActiveCount},{runtime.Room.Camera.PixelPosition.X}";
            if (actual != native[frame] && mismatches++ < 8)
                Console.WriteLine($"Speedway mismatch:\n{actual}\n{native[frame]}");
        }
        AssertEqual(0, mismatches, "Original CPU Speedway input, pose, movement, PLM pressure and camera");
        AssertEqual((ushort)(beams == 11 ? 799 : 842), samus.Position.X.Whole, "Native Speedway success/failure endpoint");
        Console.WriteLine($"PASS {native.Length} native Speedway frames: beams={beams} dash={dash} endpoint={samus.Position.X.Whole}.");
    }

    internal static void VerifyFrogSpeedwayPoolCollision()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var level = runtime.Room.LevelData!;
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(1237, 139)),
            PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose, EquippedBeams = 5 };
        samus.Kinematics.Radius = new(5, 21);
        var plms = runtime.Plms;
        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        ushort blocker = level.GetCollisionBlock(new(76, 9)).LevelWord;
        var before = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = -65536 });
        AssertTrue(before.Collided, "Ordinary speedless contact blocks before ceiling overload");
        int firstFull = -1;
        for (int frame = 0; frame < 344; frame++)
        {
            ushort held = (ushort)SnesButton.X;
            ushort pressed = frame == 0 ? held : (ushort)0;
            bombs.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, held, pressed);
            shots.StepFrame(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = held, ControllerNewInput = pressed }, new(1109, 0), bombs);
            plms.Step(bus, level, runtime.Room.BackgroundStreamer!, new PlmView { Layer1Position = new(1109, 0), Bg1XOffset = 0 });
            if (firstFull < 0 && plms.SlotTable.ActiveCount == 40) firstFull = frame;
        }
        AssertEqual(343, firstFull, "Authored room reaches full pool with live PLM timers");
        var after = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = -65536 });
        AssertTrue(!after.Collided, "Native full-pool collision remains carry clear without Speed Booster");
        AssertEqual((ushort)1236, samus.Position.X.Whole, "Samus enters first speed block without a clip or teleport");
        AssertEqual(blocker, level.GetCollisionBlock(new(76, 9)).LevelWord, "Exhaustion bypass does not break the block");
        AssertEqual(40, plms.SlotTable.ActiveCount, "Contact cannot allocate a forty-first slot");
        Console.WriteLine("PASS Frog Speedway: live ceiling-shot overload permits unchanged speed-block entry at frame 343.");
    }

    [VerificationCommand("--ceiling-wrap-room")]
    internal static int CeilingWrapRoomCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
        return 0;
    }

    [VerificationCommand("--ceiling-wrap-runtime")]
    internal static int CeilingWrapRuntimeCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv"));
        return 0;
    }

    [VerificationCommand("--ceiling-wrap-success")]
    internal static int CeilingWrapSuccessCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11, true));
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false));
        return 0;
    }
}
