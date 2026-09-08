using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyTitleRenderSnapshots()
    {
        Verify(CreateConstructedTitleRom(), "constructed");
        string path = Path.GetFullPath("Super Metroid.smc");
        if (File.Exists(path)) Verify(File.ReadAllBytes(path), "retail");
        else Console.WriteLine("  Title snapshot retail parity not run: ROM absent.");

        static void Verify(byte[] rom, string source)
        {
            var scene = new TitleSequenceState(new SuperMetroidAddressSpace(rom));
            var control = new TitleSequenceState(new SuperMetroidAddressSpace(rom));
            var phases = new HashSet<TitleSequencePhase>();
            int comparisons = 0;
            var reusable = new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
            for (int frame = 0; frame < 2500; frame++)
            {
                TitleSequencePhase phase = scene.Phase;
                bool newPhase = phases.Add(phase);
                if (newPhase || frame % 17 == 0 || phase == TitleSequencePhase.TitleScreenFadeOut)
                {
                    // The legacy producer still independently rasterizes live memory.
                    // Compare at phase boundaries and throughout motion, not just black
                    // endpoints. It remains the reference until extraction is qualified.
                    Rgba32[] expected = scene.Render();
                    Mode7ObjRenderSnapshot packet = scene.CaptureRenderSnapshot();
                    Array.Fill(reusable, new Rgba32(255, 0, 255));
                    var reused = SoftwareFrameSnapshotRenderer.Render(
                        new RenderFrameSnapshot(new(frame + 1, 1, (ushort)frame), packet), reusable);
                    AssertTrue(ReferenceEquals(reusable, reused), "title caller-owned output retained");
                    Match(expected, reused, frame, phase);
                    Match(expected, SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(
                        new RenderFrameSnapshot(new(frame + 1, 1, (ushort)frame), packet))), frame, phase);
                    Match(expected, SoftwareMode7ObjSnapshotRenderer.Render(packet), frame, phase);
                    Match(expected, SoftwareMode7ObjSnapshotRenderer.Render(packet), frame, phase);
                    Mode7ObjRenderSnapshot repeat = scene.CaptureRenderSnapshot();
                    AssertTrue(packet.Memory.Oam.SequenceEqual(repeat.Memory.Oam), "repeat title OAM capture");
                    Match(control.Render(), expected, frame, phase);
                    comparisons++;

                    // Hold the packet across real scene/palette/graphics advancement.
                    ushort input = phase == TitleSequencePhase.TitleScreen ? (ushort)SnesButton.Start : (ushort)0;
                    scene.Step(input);
                    control.Step(input);
                    Match(expected, SoftwareMode7ObjSnapshotRenderer.Render(packet), frame, phase);
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

        static void Match(Rgba32[] expected, Rgba32[] actual, int frame, TitleSequencePhase phase)
        {
            AssertEqual(expected.Length, actual.Length, "title snapshot frame size");
            for (int index = 0; index < expected.Length; index++)
                if (expected[index] != actual[index])
                    throw new InvalidOperationException($"Title snapshot {phase} frame {frame}: pixel ({index % FrontendFrame.Width},{index / FrontendFrame.Width}) expected {expected[index]}, got {actual[index]}.");
        }
    }
}
