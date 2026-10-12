using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;
using static WaveDoorBandsVerification;

internal static class GateBeamCollisionVerification
{
    internal static void VerifyKronicGateBeamCollision()
    {
        Suite(nameof(VerifyGateScanTermination), () => VerifyGateScanTermination());
        Suite(nameof(VerifyHorizontalWaveDoorBands), () => VerifyHorizontalWaveDoorBands());
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(140, 360)),
            PoseId = SamusPoseId.NormalJumpAimDiagonalUpLeftPose };
        var level = runtime.Room.LevelData!;
        var plms = runtime.Plms;
        for (int warm = 0; warm < 2; warm++) plms.Step(bus, level, runtime.Room.BackgroundStreamer!, new PlmView { Layer1Position = new(0, 224), Bg1XOffset = 0 });
        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        ushort input = (ushort)SnesButton.X;
        bombs.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, input, input);
        shots.StepFrame(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(0, 224), bombs);
        var shot = shots.Slots[0];
        AssertEqual((ushort)0x8700, shot.Type, "Native beam impacts gate during producer collision");
        AssertEqual((ushort)113, shot.Position.X.Whole, "Native impact X, before any continuing flight");
        AssertEqual((ushort)329, shot.Position.Y.Whole, "Native impact Y");
        AssertEqual((ushort)0xa00f, shot.InstructionPointer, "Same-frame native impact instruction advancement");
        AssertTrue(plms.PopulationSlots.Where(s => s.HeaderPointer == PlmHeaderId.DownwardGate)
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
            var level = CreateRoom(new(16, 16), words, bts);
            var plms = new RoomPlmSystem();
            if (full)
                for (int i = 0; i < 40; i++)
                    AssertTrue(plms.TrySpawnProjectileShotBlock(level, first, (byte)0x10, 0, true), "Fill gate probe pool");
            var shot = new SamusProjectileSlot(0) { Position = new(new(33, 0), new(33, 0)),
                Radius = new(17, 17), Velocity = new(-1, -1) };
            string method = $"Scan{(horizontal ? "Horizontal" : "Vertical")}{(wave ? "Wave" : "")}ShotReactions";
            object? result = typeof(ProjectileTerrainCollision).GetMethod(method,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(null, [level, shot, plms]);
            AssertEqual(full ? 40 : 1, plms.SlotTable.ActiveCount, "Gate setup terminates span after first allocation");
            if (!wave) AssertEqual(!full, (bool)result!, "Only executed gate setup overrides earlier air reaction");
            AssertEqual((ushort)0xc000, level.GetCollisionBlockByIndex(first).LevelWord, "Gate scan does not mutate terrain");
        }
    }

    [VerificationCommand("--gate-beam-collision")]
    internal static int GateBeamCollisionCommand()
    {
        Suite(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision());
        return 0;
    }
}
