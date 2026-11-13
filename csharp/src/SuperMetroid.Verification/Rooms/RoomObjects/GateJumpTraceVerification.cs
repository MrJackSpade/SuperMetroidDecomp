using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;

/// <summary>Compares gate-jump trajectories and switch activation with recorded native CPU traces.</summary>
internal static class GateJumpTraceVerification
{
    /// <summary>Checks ordinary and spin timings across 1120 frames, including adjacent misses and falling control.</summary>
    internal static void VerifyGateJumpTraces()
    {
        foreach ((int shootFrame, int aimFrame, bool releaseLeft) in new[]
            { new VerifyGateJumpTracesCase(7, 0, false), new VerifyGateJumpTracesCase(8, 0, false), new VerifyGateJumpTracesCase(9, 0, false), new VerifyGateJumpTracesCase(16, 0, false), new VerifyGateJumpTracesCase(8, 7, false),
              new VerifyGateJumpTracesCase(4, 4, true), new VerifyGateJumpTracesCase(5, 4, true), new VerifyGateJumpTracesCase(6, 4, true) })
        {
            string suffix = releaseLeft ? $"spin-release-{shootFrame}" :
                aimFrame == 0 ? shootFrame.ToString() : $"spin-{shootFrame}-{aimFrame}";
            var expected = File.ReadLines($"csharp/test-fixtures/movement-release/gate-jump-403-{suffix}.csv").Skip(1).ToArray();
            AssertEqual(140, expected.Length, "Complete native gate jump trace");
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.KronicBoost);
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
            samus.Position = samus.Position.WithWhole(new(140, 379));
            samus.EquippedItems = 0; samus.EquippedBeams = 0;
            samus.SelectedHudItem = HudItem.Missile;
            samus.Missiles = samus.MaxMissiles = 10;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 224));
            bool activated = false;
            for (int frame = -60; frame < 80; frame++)
            {
                ControllerButton input = frame < 0 ? 0 : (ControllerButton.A | ControllerButton.Left);
                if (aimFrame > 0 && frame == -1) input = ControllerButton.Left;
                if (frame >= aimFrame) input |= ControllerButton.R;
                if (releaseLeft && frame >= aimFrame) input &= ~ControllerButton.Left;
                if (frame == shootFrame) input |= ControllerButton.X;
                runtime.StepFrame(input);
                var gate = runtime.RoomObjects.PopulationSlots.Single(s => s.Kind == RoomObjectKind.DownwardGate);
                activated |= gate.Behaviour is DownwardGateObject { Triggered: true };
                string actual = $"{frame},{ControllerWordCodec.ToJoypadWord(input):X4},{samus.Position.X.Whole},{samus.Kinematics.Position.X.Fraction},{samus.Position.Y.Whole},{(int)SamusPoseNumbers.Number(samus.Pose):X2},{(gate.Behaviour is DownwardGateObject { Triggered: true } ? 1 : 0)},{NativeGateInstruction(bus, gate):X4}";
                AssertEqual(expected[frame + 60], actual, $"Native gate jump shot={shootFrame}, frame={frame}");
            }
            AssertEqual(shootFrame == 8 && aimFrame == 0, samus.Position.X.Whole < 112, "Only native successful timing crosses opened gate");
            AssertEqual((aimFrame == 0 && shootFrame == 8) || (releaseLeft && shootFrame == 5),
                activated, "Native successful ordinary/spin timings activate the switch independently of crossing");
        }
        Console.WriteLine("PASS 1120 original-CPU gate jump frames: ordinary/spin successes, adjacent misses and falling control.");
    }

    /// <summary>
    /// The native instruction the gate's typed state stands for (#1554): the trace's instruction column is compared as
    /// the gate's own phase and image, translated through the ROM oracle's walk of the gate's lists.
    /// </summary>
    private static ushort NativeGateInstruction(ISnesAddressSpace bus, RoomObjectSlotSnapshot gate) =>
        RoomObjectProgramsVerification.DownwardGateNativeInstruction(bus, gate.Behaviour as DownwardGateObject
            ?? throw new InvalidDataException($"Gate at block {gate.Block} is no typed gate."));

    /// <summary>Runs the focused gate-jump trace verification command.</summary>
    [VerificationCommand("--gate-jump-traces")]
    internal static int GateJumpTracesCommand()
    {
        Suite(nameof(VerifyGateJumpTraces), () => VerifyGateJumpTraces());
        return 0;
    }
}
