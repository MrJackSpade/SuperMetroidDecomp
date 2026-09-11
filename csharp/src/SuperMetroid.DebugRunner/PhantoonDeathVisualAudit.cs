using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Focused real-room death rendering fixture; it starts at the production death-dispatch boundary.</summary>
internal static class PhantoonDeathVisualAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
        for (int i = 0; i < 1000; i++) runtime.StepFrame(0);
        var boss = runtime.Enemies.Phantoon!;
        // Construct the lethal-hit result, then use the same setup called by the
        // actual projectile dispatcher. This isolates death visuals; it does not
        // claim an end-to-end controller battle or projectile-damage validation.
        boss.Body.Health = 0;
        typeof(RoomEnemySystem).GetMethod("BeginPhantoonDeathSequence", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [boss.Body, boss]);
        using var trace = new StreamWriter(Path.Combine(directory, "death.csv"));
        trace.WriteLine("frame,phase,mosaic,waveActive,amplitude");
        for (int frame = 0; frame < 2000; frame++)
        {
            byte priorMosaic = boss.MosaicRegister;
            runtime.StepFrame(0);
            trace.WriteLine($"{frame},{boss.Body.VariableF:X4},{boss.MosaicRegister:X2},{boss.Wave.Active},{boss.Mouth!.VariableD}");
            // Avoid judging the post-AI register on its change frame: NMI still owns
            // the preceding completed value. A stable setting makes this assertion
            // independent of the not-yet-translated mosaic display latch.
            if (boss.Body.VariableF != (ushort)PhantoonAiFunction.DyingFadeOut || boss.MosaicRegister < 0x42 || boss.MosaicRegister != priorMosaic) continue;
            var full = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            var basis = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
            var ordinary = (OrdinaryGameplayRenderLayer)basis.Layers[0];
            // Isolate the actual production BG2 draw, without OBJ/BG1 color changes
            // obscuring the mosaic's repeated horizontal samples. Mosaic is BG2-only.
            var body = new OrdinaryGameplayRenderLayer(ordinary.Registers with
            {
                MainScreenLayers = SnesMainScreenLayers.Bg2,
            }, ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
            var isolated = new LayeredRenderSnapshot(basis.Memory, [body], basis.ObjectSelection, basis.Brightness);
            var pixels = SoftwareLayeredSnapshotRenderer.Render(isolated);
            if (pixels.Skip(256 * 32).Count(pixel => pixel.R != 0 || pixel.G != 0 || pixel.B != 0) < 100)
                throw new InvalidDataException("Mosaic fixture has no substantial visible body to test.");
            int size = (boss.MosaicRegister >> 4) + 1, mismatches = 0;
            for (int y = 32; y < 224; y++)
            for (int x = 0; x < 256; x++)
                if (pixels[y * 256 + x] != pixels[y * 256 + x / size * size]) mismatches++;
            PngWriter.WriteRgba(Path.Combine(directory, "full-death.png"), 256, 224, SoftwareLayeredSnapshotRenderer.Render(full));
            PngWriter.WriteRgba(Path.Combine(directory, "isolated-body.png"), 256, 224, pixels);
            Console.WriteLine($"Phantoon death frame {frame}: mosaic={boss.MosaicRegister:X2}, size={size}, {mismatches} pixels violate horizontal mosaic repetition.");
            if (mismatches != 0) throw new InvalidDataException("Final-death BG2 mosaic is not applied to rendered body pixels.");
            return 0;
        }
        throw new InvalidDataException("Death fixture never reached a nontrivial mosaic phase.");
    }
}
