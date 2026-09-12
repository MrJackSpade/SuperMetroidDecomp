using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyFrogSpeedwayRuntimeTrace(string path)
    {
        string[] native = File.ReadLines(path).Skip(1).ToArray();
        AssertEqual(900, native.Length, "Native Speedway unsuccessful-setup trace length");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.XPosition = 1237; samus.YPosition = 139;
        samus.EquippedBeams = 5; samus.EquippedItems = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(1109, 0);
        int mismatches = 0;
        for (int frame = 0; frame < native.Length; frame++)
        {
            runtime.StepFrame((ushort)(SnesButton.X | SnesButton.B | SnesButton.Left | SnesButton.R));
            string actual = $"{frame},{samus.XPosition},{samus.Kinematics.XSubposition},{samus.YPosition},{samus.Pose:X2},{runtime.Plms.ActiveCount},{runtime.Camera.XPosition}";
            if (actual != native[frame] && mismatches++ < 8)
                Console.WriteLine($"Speedway mismatch:\n{actual}\n{native[frame]}");
        }
        AssertEqual(0, mismatches, "Original CPU Speedway input, pose, movement, PLM pressure and camera");
        AssertEqual((ushort)842, samus.XPosition, "Native unsuccessful setup stops at the same speed block");
        Console.WriteLine("PASS 900 native Speedway input frames, including the unsuccessful crossing endpoint.");
    }

    private static void VerifyFrogSpeedwayPoolCollision()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var level = runtime.LevelData!;
        var samus = new SamusState { XPosition = 1237, YPosition = 139,
            PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose, EquippedBeams = 5 };
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 21;
        var plms = runtime.Plms;
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ushort blocker = level.GetCollisionBlock(76, 9).LevelWord;
        var before = SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, -65536, plms: plms);
        AssertTrue(before.Collided, "Ordinary speedless contact blocks before ceiling overload");
        int firstFull = -1;
        for (int frame = 0; frame < 344; frame++)
        {
            ushort held = (ushort)SnesButton.X;
            ushort pressed = frame == 0 ? held : (ushort)0;
            bombs.StepFrame(bus, level, samus, held, pressed);
            shots.StepFrame(bus, level, samus, held, pressed, 1109, 0, bombs, roomPlms: plms);
            plms.Step(bus, level, runtime.BackgroundStreamer!, 1109, 0, 0);
            if (firstFull < 0 && plms.ActiveCount == 40) firstFull = frame;
        }
        AssertEqual(343, firstFull, "Authored room reaches full pool with live PLM timers");
        var after = SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, -65536, plms: plms);
        AssertTrue(!after.Collided, "Native full-pool collision remains carry clear without Speed Booster");
        AssertEqual((ushort)1236, samus.XPosition, "Samus enters first speed block without a clip or teleport");
        AssertEqual(blocker, level.GetCollisionBlock(76, 9).LevelWord, "Exhaustion bypass does not break the block");
        AssertEqual(40, plms.ActiveCount, "Contact cannot allocate a forty-first slot");
        Console.WriteLine("PASS Frog Speedway: live ceiling-shot overload permits unchanged speed-block entry at frame 343.");
    }
}
