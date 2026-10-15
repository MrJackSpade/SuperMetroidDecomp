using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Compares gate-jump trajectories and switch activation with recorded native CPU traces.</summary>
internal static class GateJumpTraceVerification
{
    /// <summary>Checks ordinary and spin timings across 1120 frames, including adjacent misses and falling control.</summary>
    internal static void VerifyGateJumpTraces()
    {
        foreach ((int shootFrame, int aimFrame, bool releaseLeft) in new[]
            { (7, 0, false), (8, 0, false), (9, 0, false), (16, 0, false), (8, 7, false),
              (4, 4, true), (5, 4, true), (6, 4, true) })
        {
            string suffix = releaseLeft ? $"spin-release-{shootFrame}" :
                aimFrame == 0 ? shootFrame.ToString() : $"spin-{shootFrame}-{aimFrame}";
            var expected = File.ReadLines($"csharp/test-fixtures/movement-release/gate-jump-403-{suffix}.csv").Skip(1).ToArray();
            AssertEqual(140, expected.Length, "Complete native gate jump trace");
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
            var samus = runtime.Player.IntroducedSamus;
            samus.InputLocked = false;
            samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
            samus.Position = samus.Position.WithWhole(new(140, 379));
            samus.EquippedItems = samus.EquippedBeams = 0;
            samus.SelectedHudItem = 1;
            samus.Missiles = samus.MaxMissiles = 10;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 224));
            bool activated = false;
            for (int frame = -60; frame < 80; frame++)
            {
                SnesButton input = frame < 0 ? (ushort)0 : (SnesButton.A | SnesButton.Left);
                if (aimFrame > 0 && frame == -1) input = SnesButton.Left;
                if (frame >= aimFrame) input |= SnesButton.R;
                if (releaseLeft && frame >= aimFrame) input &= ~SnesButton.Left;
                if (frame == shootFrame) input |= SnesButton.X;
                runtime.StepFrame(input);
                var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == PlmHeaderId.DownwardGate);
                activated |= gate.LoopTimer != 0;
                string actual = $"{frame},{input:X},{samus.Position.X.Whole},{samus.Kinematics.Position.X.Fraction},{samus.Position.Y.Whole},{(int)samus.Pose:X2},{gate.LoopTimer},{NativeGateInstruction(bus, gate):X4}";
                AssertEqual(expected[frame + 60], actual, $"Native gate jump shot={shootFrame}, frame={frame}");
            }
            AssertEqual(shootFrame == 8 && aimFrame == 0, samus.Position.X.Whole < 112, "Only native successful timing crosses opened gate");
            AssertEqual((aimFrame == 0 && shootFrame == 8) || (releaseLeft && shootFrame == 5),
                activated, "Native successful ordinary/spin timings activate the switch independently of crossing");
        }
        Console.WriteLine("PASS 1120 original-CPU gate jump frames: ordinary/spin successes, adjacent misses and falling control.");
    }

    /// <summary>The gate's native instruction pointer, or the address its typed program stands for.</summary>
    private static ushort NativeGateInstruction(ISnesAddressSpace bus, RoomPlmSlotSnapshot gate) =>
        gate.Animation is DownwardGatePlmAnimation typed
            ? RoomPlmProgramsVerification.DownwardGateNativeInstruction(bus, typed)
            : gate.InstructionPointer;

    /// <summary>Runs the focused gate-jump trace verification command.</summary>
    [VerificationCommand("--gate-jump-traces")]
    internal static int GateJumpTracesCommand()
    {
        Suite(nameof(VerifyGateJumpTraces), () => VerifyGateJumpTraces());
        return 0;
    }
}
