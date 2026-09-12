using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGateJumpTraces()
    {
        foreach ((int shootFrame, int aimFrame) in new[] { (7, 0), (8, 0), (9, 0), (8, 7) })
        {
            string suffix = aimFrame == 0 ? shootFrame.ToString() : $"spin-{shootFrame}-{aimFrame}";
            var expected = File.ReadLines($"csharp/test-fixtures/movement-release/gate-jump-403-{suffix}.csv").Skip(1).ToArray();
            AssertEqual(140, expected.Length, "Complete native gate jump trace");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
            samus.XPosition = 140; samus.YPosition = 379;
            samus.EquippedItems = samus.EquippedBeams = 0;
            samus.SelectedHudItem = 1;
            samus.Missiles = samus.MaxMissiles = 10;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            runtime.Camera!.SetPosition(0, 224);
            for (int frame = -60; frame < 80; frame++)
            {
                ushort input = frame < 0 ? (ushort)0 : (ushort)(SnesButton.A | SnesButton.Left);
                if (aimFrame > 0 && frame == -1) input = (ushort)SnesButton.Left;
                if (frame >= aimFrame) input |= (ushort)SnesButton.R;
                if (frame == shootFrame) input |= (ushort)SnesButton.X;
                runtime.StepFrame(input);
                var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate);
                string actual = $"{frame},{input:X4},{samus.XPosition},{samus.Kinematics.XSubposition},{samus.YPosition},{samus.Pose:X2},{gate.LoopTimer},{gate.InstructionPointer:X4}";
                AssertEqual(expected[frame + 60], actual, $"Native gate jump shot={shootFrame}, frame={frame}");
            }
            AssertEqual(shootFrame == 8 && aimFrame == 0, samus.XPosition < 112, "Only native successful timing crosses opened gate");
        }
        Console.WriteLine("PASS 560 original-CPU gate jump frames: successful timing, adjacent misses and spin control.");
    }
}
