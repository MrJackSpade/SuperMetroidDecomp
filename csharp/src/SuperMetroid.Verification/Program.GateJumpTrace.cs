using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGateJumpTraces()
    {
        foreach ((int shootFrame, int aimFrame, bool releaseLeft) in new[]
            { (7, 0, false), (8, 0, false), (9, 0, false), (16, 0, false), (8, 7, false),
              (4, 4, true), (5, 4, true), (6, 4, true) })
        {
            string suffix = releaseLeft ? $"spin-release-{shootFrame}" :
                aimFrame == 0 ? shootFrame.ToString() : $"spin-{shootFrame}-{aimFrame}";
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
            bool activated = false;
            for (int frame = -60; frame < 80; frame++)
            {
                ushort input = frame < 0 ? (ushort)0 : (ushort)(SnesButton.A | SnesButton.Left);
                if (aimFrame > 0 && frame == -1) input = (ushort)SnesButton.Left;
                if (frame >= aimFrame) input |= (ushort)SnesButton.R;
                if (releaseLeft && frame >= aimFrame) input &= unchecked((ushort)~(ushort)SnesButton.Left);
                if (frame == shootFrame) input |= (ushort)SnesButton.X;
                runtime.StepFrame(input);
                var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate);
                activated |= gate.LoopTimer != 0;
                string actual = $"{frame},{input:X4},{samus.XPosition},{samus.Kinematics.XSubposition},{samus.YPosition},{samus.Pose:X2},{gate.LoopTimer},{gate.InstructionPointer:X4}";
                AssertEqual(expected[frame + 60], actual, $"Native gate jump shot={shootFrame}, frame={frame}");
            }
            AssertEqual(shootFrame == 8 && aimFrame == 0, samus.XPosition < 112, "Only native successful timing crosses opened gate");
            AssertEqual((aimFrame == 0 && shootFrame == 8) || (releaseLeft && shootFrame == 5),
                activated, "Native successful ordinary/spin timings activate the switch independently of crossing");
        }
        Console.WriteLine("PASS 1120 original-CPU gate jump frames: ordinary/spin successes, adjacent misses and falling control.");
    }
}
