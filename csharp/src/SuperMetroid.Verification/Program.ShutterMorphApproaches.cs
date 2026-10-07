internal static partial class Program
{
    /// <summary>
    /// Issue #347's reported morph-only bomb/roll/return sequence follows its native-checked ascent,
    /// landing and shutter contact trajectory, and never enters a standing posture.
    /// </summary>
    private static void VerifyShutterMorphBombArc()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
            AssertTrue(scenario.Samus.Kinematics.YRadius == 7, "morph-only approach never enters a standing/unmorph posture");
        }
        Console.WriteLine("143 native-checked shutter trajectory frames agree, including cartridge-permitted ceiling overlap.");
    }
}
