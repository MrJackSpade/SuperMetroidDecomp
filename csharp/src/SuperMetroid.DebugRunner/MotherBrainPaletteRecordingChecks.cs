using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Proves live palette changes reach actor pixels, rather than merely changing CGRAM words.</summary>
internal sealed class MotherBrainPaletteRecordingChecks
{
    private readonly HashSet<string> samusPalettes = [];
    private readonly HashSet<string> motherPalettes = [];

    public void Observe(SuperMetroidRuntime runtime, int frame)
    {
        if (runtime.Enemies.MotherBrain is not { } brain) return;
        if (runtime.Samus?.Drained.RainbowPaletteEnabled == true)
            Check("samus", 192, 16, runtime.Samus.XPosition, runtime.Samus.YPosition, samusPalettes);
        if (brain.RainbowBeamPaletteRequested && brain.RainbowBeamHdmaActive)
            Check("mother", 145, 15, brain.Head!.XPosition, brain.Head.YPosition, motherPalettes);

        void Check(string actor, int start, int colorCount, int worldX, int worldY, HashSet<string> palettes)
        {
            string key = string.Join(',', runtime.Cgram.Colors.Slice(start, colorCount).ToArray());
            if (!palettes.Add(key)) return;
            var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            var actual = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            ushort[] colors = snapshot.Memory.Cgram.ToArray();
            Array.Clear(colors, start, colorCount);
            var blankedMemory = new PpuMemorySnapshot(snapshot.Memory.Vram, colors,
                snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
            var blanked = SoftwareLayeredSnapshotRenderer.Render(new(blankedMemory,
                snapshot.Layers, snapshot.ObjectSelection, snapshot.Brightness));
            int changed = 0;
            // Restrict the witness to the actor's screen neighborhood. A floor tile or
            // another actor reusing a palette elsewhere cannot satisfy this assertion.
            int centerX = worldX - runtime.Camera!.XPosition, centerY = worldY - runtime.Camera.YPosition;
            int radius = actor == "samus" ? 24 : 40;
            for (int y = Math.Max(32, centerY - radius); y < Math.Min(224, centerY + radius); y++)
                for (int x = Math.Max(0, centerX - radius); x < Math.Min(256, centerX + radius); x++)
                    if (actual[y * 256 + x] != blanked[y * 256 + x]) changed++;
            if (changed < 20) throw new InvalidDataException($"{actor} rainbow palette did not affect visible actor pixels at frame {frame}: {changed} pixels.");
            Console.WriteLine($"Visible {actor} palette {palettes.Count} at {frame}: {changed} actor-palette pixels.");
            if (palettes.Count == 2)
            {
                Directory.CreateDirectory("csharp/test-temp/mother-brain-recording");
                File.WriteAllBytes($"csharp/test-temp/mother-brain-recording/{actor}-rainbow.smframe",
                    RenderFrameSnapshotCodec.Serialize(new(new(frame, 1, runtime.NmiFrameCounter), snapshot)));
            }
        }
    }

    public void Verify()
    {
        if (samusPalettes.Count < 10 || motherPalettes.Count < 6)
            throw new InvalidDataException($"Incomplete visible palette coverage: Samus={samusPalettes.Count}, Mother Brain={motherPalettes.Count}.");
        Console.WriteLine($"Rainbow palettes verified in live rendering: Samus={samusPalettes.Count}, Mother Brain={motherPalettes.Count}.");
    }
}
