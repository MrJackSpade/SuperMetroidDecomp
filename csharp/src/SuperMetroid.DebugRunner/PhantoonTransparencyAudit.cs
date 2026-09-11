using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Real-room #555 check: additive BG2 must not erase the scenery with black pixels.</summary>
internal static class PhantoonTransparencyAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
        int worst = 0, worstFrame = -1, checkedFrames = 0;
        using var trace = new StreamWriter(Path.Combine(directory, "transparency.csv"));
        trace.WriteLine("frame,phase,erasedPixels,firstX,firstY");
        for (int frame = 0; frame < 2400; frame++)
        {
            runtime.StepFrame(0);
            var boss = runtime.Enemies.Phantoon!;
            if ((boss.SemiTransparencyLayerFlags & 0x4000) == 0) continue;
            checkedFrames++;
            var full = GameplayDisplayCapture.TryCaptureFrame(runtime)
                ?? throw new InvalidDataException("No full gameplay render packet.");
            var basis = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
            var ordinary = (OrdinaryGameplayRenderLayer)basis.Layers[0];
            var withoutBody = new OrdinaryGameplayRenderLayer(ordinary.Registers with
            {
                MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2,
            }, ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
            // Keep every remaining full-frame effect. Only remove the body BG2 from the
            // ordinary scene to reveal the background it would contribute to additively.
            var referenceLayers = full.Layers.ToArray();
            referenceLayers[0] = withoutBody;
            var reference = new LayeredRenderSnapshot(full.Memory, referenceLayers,
                full.ObjectSelection, full.Brightness);
            var actual = SoftwareLayeredSnapshotRenderer.Render(full);
            var background = SoftwareLayeredSnapshotRenderer.Render(reference);
            int erased = 0, first = -1;
            for (int i = 256 * 32; i < actual.Length; i++)
            {
                if (actual[i].R != 0 || actual[i].G != 0 || actual[i].B != 0) continue;
                if (background[i].R == 0 && background[i].G == 0 && background[i].B == 0) continue;
                if (first < 0) first = i;
                erased++;
            }
            trace.WriteLine($"{frame},{boss.Body.VariableF:X4},{erased},{first % 256},{first / 256}");
            if (erased > worst)
            {
                worst = erased;
                worstFrame = frame;
                PngWriter.WriteRgba(Path.Combine(directory, "worst-full-frame.png"), 256, 224, actual);
                PngWriter.WriteRgba(Path.Combine(directory, "worst-without-body.png"), 256, 224, background);
            }
        }
        Console.WriteLine($"Phantoon full-frame transparency: {checkedFrames} checked frames; maximum {worst} colored scenery pixels erased to black at frame {worstFrame}.");
        // $88:E449 selects $1A, whose $88:80D9 setup places BG2 on the additive
        // subscreen with no subtraction/halving. Adding black cannot erase color.
        if (checkedFrames == 0 || worst != 0)
            throw new InvalidDataException("Phantoon semi-transparency erases visible scenery with black body pixels.");
        return 0;
    }
}
