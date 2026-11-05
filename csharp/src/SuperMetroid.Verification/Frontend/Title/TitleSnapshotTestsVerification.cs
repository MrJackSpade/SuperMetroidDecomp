using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Title.TitleFixtureVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleSequenceRomDataVerification;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
namespace SuperMetroid.Verification.Frontend.Title;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;

/// <summary>Checks that title-scene render snapshots draw the scene, stay repeatable and are retained across its advancement.</summary>
internal static class TitleSnapshotTestsVerification
{
    /// <summary>Checks constructed and available retail scenes across phase changes and retained snapshots.</summary>
    internal static void VerifyTitleRenderSnapshots()
    {
        Verify(CreateConstructedTitleRom(), "constructed");
        string path = Path.GetFullPath("Super Metroid.smc");
        if (File.Exists(path)) Verify(File.ReadAllBytes(path), "retail");
        else Console.WriteLine("  Title snapshot retail parity not run: ROM absent.");

        static void Verify(byte[] rom, string source)
        {
            var sourceBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            TitleGradientPresentation gradient = TitleGradientFile.Read(
                new MemoryStream(TitleGradientExtractor.Extract(sourceBus), writable: false));
            var scene = CreateTitleFixture(sourceBus, titleGradientPresentation: gradient);
            var control = CreateTitleFixture(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom),
                titleGradientPresentation: gradient);
            var phases = new HashSet<TitleSequencePhase>();
            int comparisons = 0;
            for (int frame = 0; frame < 2500; frame++)
            {
                TitleSequencePhase phase = scene.Phase;
                bool newPhase = phases.Add(phase);
                if (newPhase || frame % 17 == 0 || phase == TitleSequencePhase.TitleScreenFadeOut)
                {
                    // Compare at phase boundaries and throughout motion, not just black endpoints. The live scene's and the
                    // control's frames are compared with the held packet for the pixels they show, which draws them only when
                    // their descriptions differ (#1532).
                    FloorScene packet = scene.CaptureRenderSnapshot();
                    byte[] held = GraphDigest(packet);                    AssertTrue(ScenePixels.Same(new DisplayPacket(0, scene.CaptureRenderSnapshot()), new DisplayPacket((ushort)frame, packet)),
                        $"title snapshot {phase} frame {frame} shows the live scene's pixels");
                    FloorScene repeat = scene.CaptureRenderSnapshot();
                    AssertTrue(packet.Paint.SpriteParts().SequenceEqual(repeat.Paint.SpriteParts()), "repeat title OAM capture");
                    AssertTrue(ScenePixels.Same(new DisplayPacket(0, control.CaptureRenderSnapshot()), new DisplayPacket(0, packet)),
                        $"title control {phase} frame {frame} shows the same pixels");
                    comparisons++;

                    // Hold the packet across real scene/palette/graphics advancement.
                    ControllerButton input = phase == TitleSequencePhase.TitleScreen ? ControllerButton.Start : 0;
                    scene.Step(input);
                    control.Step(input);
                    // The held packet's whole description, every value it reaches through owned or shared state, is
                    // unchanged by the advancement, so it still shows the pixels it showed when captured (#1532).
                    AssertTrue(held.AsSpan().SequenceEqual(GraphDigest(packet)), $"title snapshot {source} {phase} frame {frame} survives the scene's advancement");
                }
                else
                {
                    scene.Step(0);
                    control.Step(0);
                }
                AssertEqual(control.Phase, scene.Phase, "capture does not advance title phase");
                AssertEqual(control.Brightness, scene.Brightness, "capture does not advance brightness");
                AssertEqual(control.Mode7MatrixScale, scene.Mode7MatrixScale, "capture does not advance zoom");
                if (scene.FileSelectRequested)
                {
                    AssertTrue(phases.Contains(TitleSequencePhase.SceneThreeZoom), "snapshot covers natural zoom");
                    Console.WriteLine($"  Title snapshots ({source}): {comparisons} exact frames, {phases.Count} phases and retained packets agree.");
                    return;
                }
            }
            throw new InvalidOperationException("Title snapshot fixture did not reach file select.");
        }
    }

    /// <summary>Runs title snapshot and sequence-data checks.</summary>
    [VerificationCommand("--title-fixtures")]
    internal static int TitleFixturesCommand()
    {
     VerifyTitleRenderSnapshots(); VerifyTitleSequenceRomData(); return 0; 
    }
}
