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
        int opaqueControlWorst = 0;
        int additiveFrames = 0, contributingFrames = 0;
        using var trace = new StreamWriter(Path.Combine(directory, "transparency.csv"));
        trace.WriteLine("frame,phase,erasedPixels,firstX,firstY");
        for (int frame = 0; frame < 2400; frame++)
        {
            runtime.StepFrame(0);
            // The final unused word must retain InitAI_PhantoonBody's blank character,
            // not tile zero from a stale bootstrap upload (which is visible on addition).
            if (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(runtime.Vram.Bytes.Slice(0x9ffe, 2)) != 0x0338)
                throw new InvalidDataException("Phantoon fixture's blank BG2 page was overwritten.");
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
            var opaqueLayers = full.Layers.ToArray();
            opaqueLayers[0] = ordinary;
            var opaque = SoftwareLayeredSnapshotRenderer.Render(new(full.Memory, opaqueLayers, full.ObjectSelection, full.Brightness));
            int opaqueErased = 0;
            for (int i = 256 * 32; i < opaque.Length; i++)
                if (opaque[i].R == 0 && opaque[i].G == 0 && opaque[i].B == 0 &&
                    (background[i].R != 0 || background[i].G != 0 || background[i].B != 0)) opaqueErased++;
            opaqueControlWorst = Math.Max(opaqueControlWorst, opaqueErased);
            if (full.Layers[0] is XrayGameplayRenderLayer { SubscreenUsesBg2: true })
            {
                additiveFrames++;
                bool contributes = false;
                for (int i = 256 * 32; i < actual.Length; i++)
                {
                    if (actual[i].R < background[i].R || actual[i].G < background[i].G || actual[i].B < background[i].B)
                        throw new InvalidDataException("Additive Phantoon darkened a background channel.");
                    contributes |= actual[i] != background[i];
                }
                if (contributes) contributingFrames++;
                if (!actual.AsSpan(0, 256 * 32).SequenceEqual(background.AsSpan(0, 256 * 32)))
                    throw new InvalidDataException("Phantoon blending altered the HUD.");
            }
            if (frame is 620 or 1936)
            {
                PngWriter.WriteRgba(Path.Combine(directory, $"full-frame-{frame:D4}.png"), 256, 224, actual);
                File.WriteAllBytes(Path.Combine(directory, $"full-frame-{frame:D4}.smframe"),
                    RenderFrameSnapshotCodec.Serialize(new(new(frame, 1, runtime.NmiFrameCounter), full)));
            }
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
        Console.WriteLine($"Additive capture: {additiveFrames} frames, {contributingFrames} with positive body-color contribution.");
        Console.WriteLine($"Opaque control still reproduces {opaqueControlWorst} erased scenery pixels with the corrected bootstrap fixture.");
        // $88:E449 selects $1A, whose $88:80D9 setup places BG2 on the additive
        // subscreen with no subtraction/halving. Adding black cannot erase color.
        if (checkedFrames != 1363 || worst != 0 || additiveFrames != 1353 || contributingFrames != 1094 || opaqueControlWorst == 0)
            throw new InvalidDataException("Phantoon semi-transparency erases visible scenery with black body pixels.");
        return 0;
    }
}
