using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.ScenarioFixtures;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;
/// <summary>Compares the reported morph-only bomb approach to a native-checked shutter trajectory.</summary>
internal static class ShutterMorphApproachesVerification
{
    /// <summary>
    /// Issue #347's reported morph-only bomb/roll/return sequence follows its native-checked ascent,
    /// landing and shutter contact trajectory, and never enters a standing posture.
    /// </summary>
    internal static void VerifyShutterMorphBombArc()
    {
        string[] nativeArc = File.ReadAllLines(Path.Combine(ShutterBombArcScenario.FixtureDirectory, "bomb-arc.csv"));
        AssertEqual(ShutterBombArcScenario.FrameCount - ShutterBombArcScenario.FirstArcFrame, nativeArc.Length,
            "native-checked shutter trajectory covers frames 117..259");
        var scenario = ShutterBombArcScenario.Reproduced(CreateRetailRuntimeFixture(playerInvincibilityEnabled: true));
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

    /// <summary>Runs issue #347's 143-frame shutter approach reproduction from the verification command line.</summary>
    [VerificationCommand("--shutter-morph-repro")]
    internal static int ShutterMorphReproCommand()
    {
        Suite(nameof(VerifyShutterMorphBombArc), () => VerifyShutterMorphBombArc());
        return 0;
    }
}
