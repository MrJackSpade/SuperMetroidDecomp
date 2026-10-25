using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Title.TitleFixtureVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleSequenceRomDataVerification;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Frontend.Title;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

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
            TitleGradientPresentation gradient = TitleGradientPresentation.Load(
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
                    // Compare at phase boundaries and throughout motion, not just black endpoints.
                    RgbaPixel[] expected = scene.Render();
                    FloorScene packet = scene.CaptureRenderSnapshot();
                    Match(expected, DisplayRendering.Render(
                        new DisplayPacket(new(frame + 1, 1, (ushort)frame), packet)), frame, phase);
                    Match(expected, DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), packet)), frame, phase);
                    Match(expected, DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), packet)), frame, phase);
                    FloorScene repeat = scene.CaptureRenderSnapshot();
                    AssertTrue(packet.Paint.SpriteParts().SequenceEqual(repeat.Paint.SpriteParts()), "repeat title OAM capture");
                    Match(control.Render(), expected, frame, phase);
                    comparisons++;

                    // Hold the packet across real scene/palette/graphics advancement.
                    ControllerButton input = phase == TitleSequencePhase.TitleScreen ? ControllerButton.Start : 0;
                    scene.Step(input);
                    control.Step(input);
                    Match(expected, DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), packet)), frame, phase);
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
                    Console.WriteLine($"  Title snapshots ({source}): {comparisons} exact frames, {phases.Count} phases, retained packets and repeat rendering agree.");
                    return;
                }
            }
            throw new InvalidOperationException("Title snapshot fixture did not reach file select.");
        }

        static void Match(RgbaPixel[] expected, RgbaPixel[] actual, int frame, TitleSequencePhase phase)
        {
            AssertEqual(expected.Length, actual.Length, "title snapshot frame size");
            for (int index = 0; index < expected.Length; index++)
                if (expected[index] != actual[index])
                    throw new InvalidOperationException($"Title snapshot {phase} frame {frame}: pixel ({index % FrontendFrame.Width},{index / FrontendFrame.Width}) expected {expected[index]}, got {actual[index]}.");
        }
    }

    /// <summary>Runs title snapshot and sequence-data checks.</summary>
    [VerificationCommand("--title-fixtures")]
    internal static int TitleFixturesCommand()
    {
     VerifyTitleRenderSnapshots(); VerifyTitleSequenceRomData(); return 0; 
    }
}
