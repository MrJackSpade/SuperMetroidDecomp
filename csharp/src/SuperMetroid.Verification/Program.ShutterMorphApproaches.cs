using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using static RetailRuntimeFixtureVerification;
internal static class ShutterMorphApproachesVerification
{
    /// <summary>
    /// Issue #347's reported morph-only bomb/roll/return sequence follows its native-checked ascent,
    /// landing and shutter contact trajectory, and never enters a standing posture.
    /// </summary>
    internal static void VerifyShutterMorphBombArc()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        string[] nativeArc = File.ReadAllLines(Path.Combine(ShutterBombArcScenario.FixtureDirectory, "bomb-arc.csv"));
        AssertEqual(ShutterBombArcScenario.FrameCount - ShutterBombArcScenario.FirstArcFrame, nativeArc.Length,
            "native-checked shutter trajectory covers frames 117..259");
        var scenario = ShutterBombArcScenario.Reproduced(CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true), bus);
        for (int frame = 0; frame < ShutterBombArcScenario.FrameCount; frame++)
        {
            scenario.Runtime.StepFrame(scenario.Input(frame));
            if (frame >= ShutterBombArcScenario.FirstArcFrame)
                AssertEqual(nativeArc[frame - ShutterBombArcScenario.FirstArcFrame], scenario.ArcRow(frame),
                    $"native-checked shutter ascent/landing/contact trajectory frame {frame}");
            AssertTrue(scenario.Samus.Kinematics.Radius.Y == 7, "morph-only approach never enters a standing/unmorph posture");
        }
        Console.WriteLine("143 native-checked shutter trajectory frames agree, including cartridge-permitted ceiling overlap.");
    }

    [VerificationCommand("--shutter-morph-repro")]
    internal static int ShutterMorphReproCommand()
    {
        Suite(nameof(VerifyShutterMorphBombArc), () => VerifyShutterMorphBombArc());
        return 0;
    }
}
