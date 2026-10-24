using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks ending planet VRAM transfers and final-scene rendering against cartridge and native PPU data.</summary>
internal static class EndingPlanetBoundaryVerification
{
    /// <summary>
    /// Compares sampled finale frames with the pinned upstream PPU (csharp/test-fixtures/ending-native-ppu).
    /// The planet-boundary pass writes each requested frame's PPU memory dump (<c>.ppu</c>) into this call's scratch
    /// directory and keeps its capture; the probe rasterizes the dump with retail <c>$8B:F2FA</c> (or <c>$8B:F2B7</c>
    /// burst) registers into a <c>.bgra</c> beside it. The scratch directory is deleted when the comparison ends.
    /// </summary>
    internal static void VerifyEndingNativePpu(IEnumerable<int> frames, bool burst)
    {
        using var scratch = new TestScratchDirectory("ending-native-ppu-frames");
        Directory.CreateDirectory(scratch.Root);
        var capture = new PlanetFrameCapture(scratch.Root, frames.ToHashSet(), []);
        Suite(nameof(VerifyEndingPlanetBoundary), () => VerifyEndingPlanetBoundary(capture));
        string probe = EnsureEndingNativeProbe();
        foreach (int frame in capture.Frames.Order())
        {
            string prefix = Path.Combine(scratch.Root, $"later-{frame:D4}-ZebesExplosionAnimation");
            RunNativeTool(probe, burst
                ? [prefix + ".ppu", prefix + ".bgra", "burst"]
                : [prefix + ".ppu", prefix + ".bgra"]);
            byte[] bgra = File.ReadAllBytes(prefix + ".bgra");
            AssertEqual(256 * 224 * 4, bgra.Length, "native PPU raster length");
            var native = new Rgba32[256 * 224];
            for (int i = 0; i < native.Length; i++) native[i] = new(bgra[i * 4 + 2], bgra[i * 4 + 1], bgra[i * 4]);
            var actual = DisplayRendering.Render(capture.Snapshots[frame]);
            int differences = actual.Zip(native, (first, second) => first != second).Count(differs => differs);
            Console.WriteLine($"Native PPU finale frame {frame}: {differences} pixels differ.");
            AssertEqual(0, differences, $"finale frame {frame} matches pinned native PPU with cartridge register setup");
        }
    }

    /// <summary>
    /// Builds the checked-in ending PPU probe into the ignored <see cref="RepositoryInstallation.CacheRoot"/>, again only when its source changes.
    /// This is a build cache of one executable reused by every run, not a per-run artifact.
    /// </summary>
    private static string EnsureEndingNativeProbe()
    {
        string probe = Path.GetFullPath(Path.Combine(RepositoryInstallation.CacheRoot, "ending-native-ppu", "probe.exe"));
        // build.cmd compiles with cl directly; the C# Directory.Build settings would otherwise apply. The probe is
        // rebuilt whenever its source is newer than the cached build.
        string source = Path.GetFullPath("csharp/test-fixtures/ending-native-ppu/probe.c");
        if (!File.Exists(probe) || File.GetLastWriteTimeUtc(source) > File.GetLastWriteTimeUtc(probe))
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

    /// <summary>
    /// Planet-explosion frames to dump for the native PPU comparison, into a caller-owned scratch directory, with the
    /// capture each dump was written from.
    /// </summary>
    internal sealed record PlanetFrameCapture(string Directory, IReadOnlySet<int> Frames, Dictionary<int, LayeredScene> Snapshots);

    /// <summary>
    /// The probe's input (csharp/test-fixtures/ending-native-ppu/probe.c): VRAM, CGRAM as little-endian words, the
    /// screen's 544-byte OAM upload, then OBSEL and brightness.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: dumps native VRAM, CGRAM and OAM tables for the pinned native PPU")]
    private static byte[] NativeProbeInput(LayeredScene snapshot, byte[] snapshotVram, OamBuffer oam, byte objectSelection)
    {
        var input = new List<byte>(SnesPpuLayout.VramByteCount + SnesPpuLayout.CgramColorCount * 2 + SnesPpuLayoutConstants.OamUploadByteCount + 2);
        input.AddRange(snapshotVram);
        foreach (Bgr555 color in snapshot.Paint.Colors)
        {
            input.Add(SnesLittleEndian.LowByte(color.ToWord()));
            input.Add(SnesLittleEndian.HighByte(color.ToWord()));
        }
        input.AddRange(oam.CreateUploadPayload());
        input.Add(objectSelection);
        input.Add(checked((byte)snapshot.Brightness));
        return [.. input];
    }

    /// <summary>Reconstructs native explosion DMA ranges and compares crossfade and flyaway pixels frame by frame.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the ending planet-explosion VRAM reconstruction")]
    internal static void VerifyEndingPlanetBoundary(PlanetFrameCapture? capture)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.FadeInZebesExplosion; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, ending.Stage.Phase, "planet crossfade reached");
        // Rebuild the upper VRAM image from D8C1/D8E1/D901/D921/D941/D961.
        // In particular D961 transfers only $1000 font bytes at byte $A000.
        byte[] nativeVram = ending.ShownVram().ToArray();
        RomDataReader.Decompress(bus, 0x988304, 0x8000).AsSpan(0, 0x6000).CopyTo(nativeVram.AsSpan(0x8000));
        int[] fragments = [0x98b5c1, 0x98b857, 0x98baed, 0x98bccd];
        for (int index = 0; index < fragments.Length; index++)
            RomDataReader.Decompress(bus, fragments[index], 0x8000).AsSpan(0, 0x800).CopyTo(nativeVram.AsSpan(0xe000 + index * 0x800));
        RomDataReader.Decompress(bus, 0x97e7de, 0x8000).AsSpan(0, 0x1000).CopyTo(nativeVram.AsSpan(0xa000));
        int memoryDifferences = nativeVram.Zip(ending.ShownVram().ToArray(), (first, second) => first != second).Count(differs => differs);
        Console.WriteLine($"Explosion setup: {memoryDifferences} VRAM bytes differ from literal DMA reconstruction.");
        int differences = 0, checkedFrames = 0;
        while (ending.Stage.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
        {
            var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
            var projection = snapshot.LayersOf<Mode7RenderLayer>().Single();
            // D837 retains M7SEL=0 from atmospheric setup. Its main is BG1,
            // subscreen is OBJ, and CGADSUB=$21 adds OBJ to BG1/backdrop.
            RenderLayer[] nativeLayers = [new Mode7RenderLayer(projection.Transform with { Outside = Mode7OverflowMode.Wrap },
                    AffineFloorFixtures.Floor(nativeVram, snapshot.Paint.Colors)),
                new ObjRenderLayer(AddToScreen: true)];
            var nativeMemory = new FramePaint(snapshot.Paint.Colors, snapshot.Paint.SpriteDraws);
            var expected = DisplayRendering.Render(new LayeredScene(nativeMemory, nativeLayers, snapshot.Brightness));
            var actual = ending.Render();
            int changed = actual.Zip(expected, (first, second) => first != second).Count(differs => differs);
            differences += changed;
            checkedFrames++;
            ending.Step();
        }
        Console.WriteLine($"Planet crossfade: {checkedFrames} frames, {differences} pixels differ from native wrapping/composition.");
        AssertEqual(0, differences, "planet crossfade preserves native map wrap and main/subscreen composition");
        for (int frame = 0; frame < 2048 && ending.Stage.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            var live = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
            byte[] reconstructed = ending.ShownVram();
            nativeVram.AsSpan(0x8000).CopyTo(reconstructed.AsSpan(0x8000));
            // The frame carries no VRAM (#1403): the floor is drawn again from the native reconstruction.
            RenderLayer[] reconstructedLayers = [.. live.Layers.ToArray().Select(layer => layer is Mode7RenderLayer floor
                ? (floor with { Floor = AffineFloorFixtures.Floor(reconstructed, live.Paint.Colors) }) : layer)];
            var nativePixels = DisplayRendering.Render(new LayeredScene(live.Paint, reconstructedLayers, live.Brightness));
            differences += ending.Render().Zip(nativePixels, (first, second) => first != second).Count(differs => differs);
            if (capture is not null && capture.Frames.Contains(frame))
            {
                // The native comparison replays exactly this sample's graphics and OAM under the retail registers.
                LayeredScene sample = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
                capture.Snapshots[frame] = sample;
                File.WriteAllBytes(Path.Combine(capture.Directory, $"later-{frame:D4}-{ending.Stage.Phase}.ppu"),
                    NativeProbeInput(sample, ending.ShownVram(), ScreenSpritesFixtures.ToOam(EndingCreditsPresentation.PrepareSprites(ending, EndingCreditsPresentation.CurrentEscapeObjectSheet(ending))),
                        EndingCreditsPresentation.ObjectSelection(EndingCreditsPresentation.CurrentEscapeObjectSheet(ending))));
            }
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Stage.Phase, "planet explosion reaches ship flyaway");
        Console.WriteLine($"Complete explosion: {differences} pixel mismatches against exact native upper-VRAM transfers.");
        AssertEqual(0, differences, "explosion retains graphics outside the native font DMA extent");
        AssertEqual(0, memoryDifferences, "explosion setup executes exactly the native DMA byte ranges");
    }

    /// <summary>Runs the planet VRAM and rendered-boundary comparison without frame capture.</summary>
    [VerificationCommand("--ending-planet-boundary")]
    internal static int EndingPlanetBoundaryCommand()
    {
        Suite(nameof(VerifyEndingPlanetBoundary), () => VerifyEndingPlanetBoundary(capture: null));
        return 0;
    }

    /// <summary>Compares explosion frame 512 with the pinned native PPU raster.</summary>
    [VerificationCommand("--ending-native-ppu")]
    internal static int EndingNativePpuCommand()
    {
        VerifyEndingNativePpu([512], burst: false);
        return 0;
    }

    /// <summary>Compares nineteen sampled finale frames with the pinned native PPU raster.</summary>
    [VerificationCommand("--ending-native-finale")]
    internal static int EndingNativeFinaleCommand()
    {
        VerifyEndingNativePpu(Enumerable.Range(0, 19).Select(step => 512 + step * 16), burst: false);
        return 0;
    }

    /// <summary>Compares five sampled explosion burst frames with native burst register setup.</summary>
    [VerificationCommand("--ending-native-burst")]
    internal static int EndingNativeBurstCommand()
    {
        VerifyEndingNativePpu(Enumerable.Range(0, 5).Select(step => 400 + step * 16), burst: true);
        return 0;
    }
}
