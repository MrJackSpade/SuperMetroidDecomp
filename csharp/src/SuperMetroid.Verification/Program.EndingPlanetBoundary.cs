using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Compares sampled finale frames with the pinned upstream PPU (csharp/test-fixtures/ending-native-ppu).
    /// The planet-boundary pass writes each requested frame's <c>.smframe</c> into this call's scratch
    /// directory; the probe rasterizes it with retail <c>$8B:F2FA</c> (or <c>$8B:F2B7</c> burst) registers
    /// into a <c>.bgra</c> beside it. The scratch directory is deleted when the comparison ends.
    /// </summary>
    private static void VerifyEndingNativePpu(IEnumerable<int> frames, bool burst)
    {
        using var scratch = new TestScratchDirectory("ending-native-ppu-frames");
        var capture = new PlanetFrameCapture(scratch.Root, frames.ToHashSet());
        Suite(nameof(VerifyEndingPlanetBoundary), () => VerifyEndingPlanetBoundary(capture));
        string probe = EnsureEndingNativeProbe();
        foreach (int frame in capture.Frames.Order())
        {
            string prefix = Path.Combine(scratch.Root, $"later-{frame:D4}-ZebesExplosionAnimation");
            RunNativeTool(probe, burst
                ? [prefix + ".smframe", prefix + ".bgra", "burst"]
                : [prefix + ".smframe", prefix + ".bgra"]);
            byte[] bgra = File.ReadAllBytes(prefix + ".bgra");
            AssertEqual(256 * 224 * 4, bgra.Length, "native PPU raster length");
            var native = new Rgba32[256 * 224];
            for (int i = 0; i < native.Length; i++) native[i] = new(bgra[i * 4 + 2], bgra[i * 4 + 1], bgra[i * 4]);
            var actual = SoftwareFrameSnapshotRenderer.Render(RenderFrameSnapshotCodec.Deserialize(File.ReadAllBytes(prefix + ".smframe")));
            int differences = actual.Zip(native).Count(pair => pair.First != pair.Second);
            Console.WriteLine($"Native PPU finale frame {frame}: {differences} pixels differ.");
            AssertEqual(0, differences, $"finale frame {frame} matches pinned native PPU with cartridge register setup");
        }
    }

    /// <summary>
    /// Builds the checked-in ending PPU probe once into ignored csharp/test-temp. This is a build cache of
    /// one executable reused by every run, not a per-run artifact.
    /// </summary>
    private static string EnsureEndingNativeProbe()
    {
        string probe = Path.GetFullPath("csharp/test-temp/ending-native-ppu/probe.exe");
        // build.cmd compiles with cl directly; the C# Directory.Build settings would otherwise apply.
        if (!File.Exists(probe))
            RunNativeTool(Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                ["/c", Path.GetFullPath("csharp/test-fixtures/ending-native-ppu/build.cmd")]);
        AssertTrue(File.Exists(probe), "ending native PPU probe was built");
        return probe;
    }

    /// <summary>Runs a native tool to completion, failing with its output on a nonzero exit.</summary>
    private static string RunNativeTool(string fileName, IReadOnlyList<string> arguments)
    {
        var start = new System.Diagnostics.ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(start)
            ?? throw new InvalidOperationException($"Could not start {fileName}.");
        var error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{Path.GetFileName(fileName)} exited {process.ExitCode}:{Environment.NewLine}{output}{error.Result}");
        return output;
    }

    /// <summary>Planet-explosion frames to serialize for the native PPU comparison, into a caller-owned scratch directory.</summary>
    private sealed record PlanetFrameCapture(string Directory, IReadOnlySet<int> Frames);

    [CpuBusBoundary("exact bus test: retail ROM oracle for the ending planet-explosion VRAM reconstruction")]
    private static void VerifyEndingPlanetBoundary(PlanetFrameCapture? capture)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = CreateRetailEndingFixture(bus, audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.FadeInZebesExplosion; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, ending.Phase, "planet crossfade reached");
        // Rebuild the upper VRAM image from D8C1/D8E1/D901/D921/D941/D961.
        // In particular D961 transfers only $1000 font bytes at byte $A000.
        byte[] nativeVram = ending.CaptureRenderSnapshot().Memory.Vram.ToArray();
        RomDataReader.Decompress(bus, 0x988304, 0x8000).AsSpan(0, 0x6000).CopyTo(nativeVram.AsSpan(0x8000));
        int[] fragments = [0x98b5c1, 0x98b857, 0x98baed, 0x98bccd];
        for (int index = 0; index < fragments.Length; index++)
            RomDataReader.Decompress(bus, fragments[index], 0x8000).AsSpan(0, 0x800).CopyTo(nativeVram.AsSpan(0xe000 + index * 0x800));
        RomDataReader.Decompress(bus, 0x97e7de, 0x8000).AsSpan(0, 0x1000).CopyTo(nativeVram.AsSpan(0xa000));
        int memoryDifferences = nativeVram.Zip(ending.CaptureRenderSnapshot().Memory.Vram.ToArray()).Count(pair => pair.First != pair.Second);
        Console.WriteLine($"Explosion setup: {memoryDifferences} VRAM bytes differ from literal DMA reconstruction.");
        int differences = 0, checkedFrames = 0;
        while (ending.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
        {
            var snapshot = ending.CaptureRenderSnapshot();
            var projection = snapshot.Layers.ToArray().OfType<Mode7RenderLayer>().Single();
            // D837 retains M7SEL=0 from atmospheric setup. Its main is BG1,
            // subscreen is OBJ, and CGADSUB=$21 adds OBJ to BG1/backdrop.
            RenderLayer[] nativeLayers = [new Mode7RenderLayer(projection.Registers with { WrapOutsideMap = true }),
                new ObjRenderLayer(AddToScreen: true)];
            var nativeMemory = new PpuMemorySnapshot(nativeVram, snapshot.Memory.Cgram, snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
            var expected = SoftwareLayeredSnapshotRenderer.Render(new(nativeMemory, nativeLayers,
                snapshot.ObjectSelection, snapshot.Brightness));
            var actual = ending.Render();
            int changed = actual.Zip(expected).Count(pair => pair.First != pair.Second);
            differences += changed;
            checkedFrames++;
            ending.Step();
        }
        Console.WriteLine($"Planet crossfade: {checkedFrames} frames, {differences} pixels differ from native wrapping/composition.");
        AssertEqual(0, differences, "planet crossfade preserves native map wrap and main/subscreen composition");
        for (int frame = 0; frame < 2048 && ending.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            var live = ending.CaptureRenderSnapshot();
            byte[] reconstructed = live.Memory.Vram.ToArray();
            nativeVram.AsSpan(0x8000).CopyTo(reconstructed.AsSpan(0x8000));
            var nativeMemory = new PpuMemorySnapshot(reconstructed, live.Memory.Cgram, live.Memory.Oam, live.Memory.ModeledSpriteCount);
            var nativePixels = SoftwareLayeredSnapshotRenderer.Render(new(nativeMemory, live.Layers, live.ObjectSelection, live.Brightness));
            differences += ending.Render().Zip(nativePixels).Count(pair => pair.First != pair.Second);
            if (capture is not null && capture.Frames.Contains(frame))
            {
                // The native comparison replays exactly this sample's graphics, registers and layers.
                File.WriteAllBytes(Path.Combine(capture.Directory, $"later-{frame:D4}-{ending.Phase}.smframe"),
                    RenderFrameSnapshotWriter.Serialize(new(new(frame + 1, 1, 0), ending.CaptureRenderSnapshot())));
            }
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Phase, "planet explosion reaches ship flyaway");
        Console.WriteLine($"Complete explosion: {differences} pixel mismatches against exact native upper-VRAM transfers.");
        AssertEqual(0, differences, "explosion retains graphics outside the native font DMA extent");
        AssertEqual(0, memoryDifferences, "explosion setup executes exactly the native DMA byte ranges");
    }
}
