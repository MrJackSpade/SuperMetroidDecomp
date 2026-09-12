using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyKronicGateBeamCollision()
    {
        VerifyGateScanTermination();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        var samus = new SamusState { XPosition = 140, YPosition = 360,
            PoseId = SamusPoseId.NormalJumpAimDiagonalUpLeftPose };
        var level = runtime.LevelData!;
        var plms = runtime.Plms;
        for (int warm = 0; warm < 2; warm++) plms.Step(bus, level, runtime.BackgroundStreamer!, 0, 224, 0);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ushort input = (ushort)SnesButton.X;
        bombs.StepFrame(bus, level, samus, input, input);
        shots.StepFrame(bus, level, samus, input, input, 0, 224, bombs, roomPlms: plms);
        var shot = shots.Slots[0];
        AssertEqual((ushort)0x8700, shot.Type, "Native beam impacts gate during producer collision");
        AssertEqual((ushort)113, shot.XPosition, "Native impact X, before any continuing flight");
        AssertEqual((ushort)329, shot.YPosition, "Native impact Y");
        AssertEqual((ushort)0xa00f, shot.InstructionPointer, "Same-frame native impact instruction advancement");
        AssertTrue(plms.PopulationSlots.Where(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate)
            .All(s => s.LoopTimer == 0), "Beam cannot wake the wrong-side switch through the gate body");
        Console.WriteLine("PASS Kronic Boost native initial gate-beam impact and no switch activation.");
    }

    private static void VerifyGateScanTermination()
    {
        // Two adjacent gate probes make an unwanted second scan observable as a
        // second allocation. Air before the first gate also tests forced collision.
        foreach (bool horizontal in new[] { false, true })
        foreach (bool wave in new[] { false, true })
        foreach (bool full in new[] { false, true })
        {
            var words = new ushort[256];
            var bts = new byte[256];
            int first = horizontal ? 33 : 18;
            int second = horizontal ? 49 : 19;
            words[first] = words[second] = 0xc000;
            bts[first] = bts[second] = 0x10;
            var level = CreateRoom(16, 16, words, bts);
            var plms = new RoomPlmSystem();
            if (full)
                for (int i = 0; i < 40; i++)
                    AssertTrue(plms.TrySpawnProjectileShotBlock(level, first, (byte)0x10, 0, true), "Fill gate probe pool");
            var shot = new SamusProjectileSlot(0) { XPosition = 33, YPosition = 33,
                XRadius = 17, YRadius = 17, XVelocity = -1, YVelocity = -1 };
            string method = $"Scan{(horizontal ? "Horizontal" : "Vertical")}{(wave ? "Wave" : "")}ShotReactions";
            object? result = typeof(SamusProjectileSystem).GetMethod(method,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(null, [level, shot, plms]);
            AssertEqual(full ? 40 : 1, plms.ActiveCount, "Gate setup terminates span after first allocation");
            if (!wave) AssertEqual(!full, (bool)result!, "Only executed gate setup overrides earlier air reaction");
            AssertEqual((ushort)0xc000, level.GetCollisionBlockByIndex(first).LevelWord, "Gate scan does not mutate terrain");
        }
    }
}
