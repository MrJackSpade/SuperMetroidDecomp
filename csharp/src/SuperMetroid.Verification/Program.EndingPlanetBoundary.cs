using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingPlanetBoundary()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = new EndingCreditsState(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.FadeInZebesExplosion; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, ending.Phase, "planet crossfade reached");
        Directory.CreateDirectory("csharp/test-temp/ending-506");
        int differences = 0, checkedFrames = 0;
        while (ending.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
        {
            var snapshot = ending.CaptureRenderSnapshot();
            var projection = snapshot.Layers.ToArray().OfType<Mode7RenderLayer>().Single();
            // D837 retains M7SEL=0 from atmospheric setup. Its main is BG1,
            // subscreen is OBJ, and CGADSUB=$21 adds OBJ to BG1/backdrop.
            RenderLayer[] nativeLayers = [new Mode7RenderLayer(projection.Registers with { WrapOutsideMap = true }),
                new ObjRenderLayer(AddToScreen: true)];
            var expected = SoftwareLayeredSnapshotRenderer.Render(new(snapshot.Memory, nativeLayers,
                snapshot.ObjectSelection, snapshot.Brightness));
            var actual = ending.Render();
            int changed = actual.Zip(expected).Count(pair => pair.First != pair.Second);
            differences += changed;
            if (checkedFrames % 8 == 0 || changed != 0)
            {
                PngWriter.WriteRgba($"csharp/test-temp/ending-506/actual-{checkedFrames:D2}.png", 256, 224, actual);
                PngWriter.WriteRgba($"csharp/test-temp/ending-506/native-{checkedFrames:D2}.png", 256, 224, expected);
            }
            checkedFrames++;
            ending.Step();
        }
        Console.WriteLine($"Planet crossfade: {checkedFrames} frames, {differences} pixels differ from native wrapping/composition.");
        AssertEqual(0, differences, "planet crossfade preserves native map wrap and main/subscreen composition");
        for (int frame = 0; frame < 2048 && ending.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            if (frame % 16 == 0)
            {
                PngWriter.WriteRgba($"csharp/test-temp/ending-506/later-{frame:D4}-{ending.Phase}.png", 256, 224, ending.Render());
                // Preserve raw graphics/register/layer evidence alongside each image.
                // A later renderer comparison can replay exactly this sample without
                // re-running the cinematic or depending on a mutable debugger slot.
                File.WriteAllBytes($"csharp/test-temp/ending-506/later-{frame:D4}-{ending.Phase}.smframe",
                    RenderFrameSnapshotCodec.Serialize(new(new(frame + 1, 1, 0), ending.CaptureRenderSnapshot())));
            }
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Phase, "planet explosion reaches ship flyaway");
    }
}
