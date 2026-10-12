using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyFrogSpeedwayRuntimeTrace(string path, ushort beams = 5, bool dash = true)
    {
        string[] native = File.ReadLines(path).Skip(1).ToArray();
        int expectedFrames = beams == 11 ? (dash ? 349 : 412) : 900;
        AssertEqual(expectedFrames, native.Length, "Native Speedway setup trace length");
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.Position = samus.Position.WithWhole(new(1237, 139));
        samus.EquippedBeams = beams; samus.EquippedItems = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(new Position16<RoomPixels>(1109, 0));
        int mismatches = 0;
        for (int frame = 0; frame < native.Length; frame++)
        {
            runtime.StepFrame((ushort)(SnesButton.X | SnesButton.Left | SnesButton.R | (dash ? SnesButton.B : 0)));
            string actual = $"{frame},{samus.Position.X.Whole},{samus.Kinematics.Position.X.Fraction},{samus.Position.Y.Whole},{(int)samus.Pose:X2},{runtime.Plms.ActiveCount},{runtime.Camera.PixelPosition.X}";
            if (actual != native[frame] && mismatches++ < 8)
                Console.WriteLine($"Speedway mismatch:\n{actual}\n{native[frame]}");
        }
        AssertEqual(0, mismatches, "Original CPU Speedway input, pose, movement, PLM pressure and camera");
        AssertEqual((ushort)(beams == 11 ? 799 : 842), samus.Position.X.Whole, "Native Speedway success/failure endpoint");
        Console.WriteLine($"PASS {native.Length} native Speedway frames: beams={beams} dash={dash} endpoint={samus.Position.X.Whole}.");
    }

    private static void VerifyFrogSpeedwayPoolCollision()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var level = runtime.LevelData!;
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
            bombs.StepFrame(bus, level, samus, held, pressed);
            shots.StepFrame(bus, level, samus, held, pressed, new(1109, 0), bombs, roomPlms: plms);
            plms.Step(bus, level, runtime.BackgroundStreamer!, new PlmView { Layer1Position = new(1109, 0), Bg1XOffset = 0 });
            if (firstFull < 0 && plms.ActiveCount == 40) firstFull = frame;
        }
        AssertEqual(343, firstFull, "Authored room reaches full pool with live PLM timers");
        var after = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = -65536 });
        AssertTrue(!after.Collided, "Native full-pool collision remains carry clear without Speed Booster");
        AssertEqual((ushort)1236, samus.Position.X.Whole, "Samus enters first speed block without a clip or teleport");
        AssertEqual(blocker, level.GetCollisionBlock(new(76, 9)).LevelWord, "Exhaustion bypass does not break the block");
        AssertEqual(40, plms.ActiveCount, "Contact cannot allocate a forty-first slot");
        Console.WriteLine("PASS Frog Speedway: live ceiling-shot overload permits unchanged speed-block entry at frame 343.");
    }
}
