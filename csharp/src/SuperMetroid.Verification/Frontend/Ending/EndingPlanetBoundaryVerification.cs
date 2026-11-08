using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
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
            var native = new RgbaPixel[256 * 224];
            for (int i = 0; i < native.Length; i++) native[i] = new(bgra[i * 4 + 2], bgra[i * 4 + 1], bgra[i * 4]);
            var actual = DisplayRendering.Render(capture.Snapshots[frame]);
            int differences = actual.Zip(native, (first, second) => first != second).Count(differs => differs);
            byte[] dump = File.ReadAllBytes(prefix + ".ppu");
            int oamAt = SnesVram.ByteCount + SnesCgram.ColorCount * 2;
            bool[] overflowing = NativeSpriteLineLimits.Overflowing(dump.AsSpan(oamAt, SnesPpuLayoutConstants.OamUploadByteCount),
                dump[oamAt + SnesPpuLayoutConstants.OamUploadByteCount]);
            int onOverflowing = Enumerable.Range(0, actual.Length).Count(index => actual[index] != native[index] && overflowing[index / 256]);
            string rows = string.Join(",", Enumerable.Range(0, 224).Where(row => Enumerable.Range(0, 256).Any(x => actual[row * 256 + x] != native[row * 256 + x])));
            foreach (int index in Enumerable.Range(0, actual.Length).Where(index => actual[index] != native[index]).Where((_, at) => at % 70 == 0))
                Console.WriteLine($"  differs at ({index % 256},{index / 256}): live {actual[index]}, native {native[index]}; {SpriteInkTrace(capture.Snapshots[frame], index % 256, index / 256)}; native {NativeSpritePixelTrace.Trace(dump.AsSpan(0, SnesVram.ByteCount), dump.AsSpan(oamAt, SnesPpuLayoutConstants.OamUploadByteCount), dump[oamAt + SnesPpuLayoutConstants.OamUploadByteCount], index % 256, index / 256)}");
            Console.WriteLine($"Native PPU finale frame {frame}: {onOverflowing} of the differing pixels lie on lines past the sprite line limit; differing rows {rows}; overflowing rows {string.Join(",", Enumerable.Range(0, 224).Where(row => overflowing[row]))}.");
            Console.WriteLine($"Native PPU finale frame {frame}: {differences} pixels differ.");
            int first = differences == 0 ? -1 : Enumerable.Range(0, actual.Length).First(index => actual[index] != native[index]);
            AssertEqual(0, differences, $"finale frame {frame} matches pinned native PPU with cartridge register setup" + (first < 0 ? ""
                : $" (first at ({first % 256},{first / 256}): live {actual[first]}, native {native[first]}; sprites {SpriteInkTrace(capture.Snapshots[frame], first % 256, first / 256)};"
                    + $" screen shades {capture.Shown[frame].Shades}; dumped sprite group 7 {string.Join(" ", capture.Shown[frame].Colours[240..])})"));
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
    internal sealed record PlanetFrameCapture(string Directory, IReadOnlySet<int> Frames, Dictionary<int, LayeredScene> Snapshots)
    {
        /// <summary>Each captured frame's screen shades, in the order shown, and the colours the dump holds, for a failure's message.</summary>
        internal Dictionary<int, CapturedColours> Shown { get; } = [];
    }

    /// <summary>A captured frame's screen shades, in the order shown, and the colours its dump holds.</summary>
    /// <param name="Shades">The screen shades' keys, in the order shown.</param>
    /// <param name="Colours">The 256 colours the frame's dump holds.</param>
    internal sealed record CapturedColours(string Shades, RgbColor[] Colours);

    /// <summary>
    /// The probe's input (csharp/test-fixtures/ending-native-ppu/probe.c): VRAM, CGRAM as little-endian words, the
    /// screen's 544-byte OAM upload, then OBSEL and brightness.
    /// </summary>
    private static byte[] NativeProbeInput(LayeredScene snapshot, IReadOnlyList<RgbColor> snapshotColours, byte[] snapshotVram, OamBuffer oam, byte objectSelection)
    {
        var input = new List<byte>(SnesVram.ByteCount + SnesCgram.ColorCount * 2 + SnesPpuLayoutConstants.OamUploadByteCount + 2);
        input.AddRange(snapshotVram);
        foreach (RgbColor color in snapshotColours)
        {
            input.Add(LittleEndianBytes.LowByte(color.ToWord()));
            input.Add(LittleEndianBytes.HighByte(color.ToWord()));
        }
        input.AddRange(oam.CreateUploadPayload());
        input.Add(objectSelection);
        input.Add(checked((byte)snapshot.Brightness));
        return [.. input];
    }

    /// <summary>Reconstructs native explosion DMA ranges and compares crossfade and flyaway pixels frame by frame.</summary>
    internal static void VerifyEndingPlanetBoundary(PlanetFrameCapture? capture)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = new AudioQueues();
        var ending = CreateRetailEndingFixture(audio, 0, 0);
        for (int frame = 0; frame < 20000 && ending.Stage.Phase != EndingCreditsPhase.FadeInZebesExplosion; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.FadeInZebesExplosion, ending.Stage.Phase, "planet crossfade reached");
        // The ending's installed shades, which every compared frame's shown colours are read through, are built once.
        var shownShades = EndingShownColours.InstalledShades(ending);
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
        string firstDifference = "none";
        ShownFloorPairing? crossfadeFloors = null;
        int composedFloorFrames = 0;
        while (ending.Stage.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade)
        {
            var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
            RgbColor[] snapshotColours = EndingShownColours.Of(ending, shownShades);
            var projection = snapshot.LayersOf<TurningFloorLayer>().Single();
            // The native floor shows what the reconstruction's floor shows in the frame's colours. Where the live floor shows
            // exactly that at every floor point it stands in for it, drawing the same pixels without composing and laying
            // the reconstruction's floor again every frame (#1568).
            ShownFloorPairing floors = crossfadeFloors is { } paired && paired.Pairs(projection.Floor, nativeVram) ? paired
                : new ShownFloorPairing(projection.Floor, nativeVram, snapshotColours);
            crossfadeFloors = floors;
            bool floorsAlike = floors.ShowAlike(projection.Floor, snapshotColours);
            AffineFloor nativeFloor = floorsAlike ? projection.Floor : floors.MemoryFloor(snapshotColours);
            if (!floorsAlike)
                composedFloorFrames++;
            // The crossfade keeps the atmosphere's wrapping floor, and the scene's sprites glow over the planet and the
            // sky (#1514); the native reconstruction draws the scene's sprites in the native colours.
            FramePaint nativeSprites = new FramePaint(snapshot.Paint.Backdrop, snapshot.SpriteDraws).WithNativeColours(snapshotColours);
            DisplayLayer[] nativeLayers = [new TurningFloorLayer(projection.Transform with { Outside = FloorOverflowMode.Wrap },
                    nativeFloor),
                new SpritesLayer(nativeSprites.SpriteDraws, SpriteBlend.Glows)];
            var nativeMemory = new FramePaint(nativeSprites.Backdrop);
            var expected = DisplayRendering.Render(new LayeredScene(nativeMemory, nativeLayers, snapshot.Brightness));
            var actual = ending.Render();
            int changed = Differences(actual, expected);
            if (changed > 0 && differences == 0)
            {
                int pixel = Enumerable.Range(0, actual.Length).First(index => actual[index] != expected[index]);
                // Each layer alone on both sides says whether the floor's colours or the sprites' combination differ.
                string inkTrace = SpriteInkTrace(snapshot, pixel % DisplayScreen.Width, pixel / DisplayScreen.Width);
                string screenShades = string.Join(" ", ending.screen.Colours.Shading.Shades.Select(shade => shade.Key));
                string spriteShades = string.Join(" | ", snapshot.SpriteDraws.ToArray().OfType<ImageSpriteDraw>()
                    .Select(draw => draw.Shading is { } shading ? string.Join(" ", shading.Shades.Select(shade => shade.Key)) : "none").Distinct());
                int FloorOnly() => DisplayRendering.Render(new LayeredScene(snapshot.Paint, [projection], snapshot.Brightness))
                    .Zip(DisplayRendering.Render(new LayeredScene(nativeMemory, [nativeLayers[0]], snapshot.Brightness)), (first, second) => first != second)
                    .Count(differs => differs);
                int SpritesOnly() => DisplayRendering.Render(new LayeredScene(snapshot.Paint, [snapshot.LayersOf<SpritesLayer>().Single()], snapshot.Brightness))
                    .Zip(DisplayRendering.Render(new LayeredScene(nativeMemory, [nativeLayers[1]], snapshot.Brightness)), (first, second) => first != second)
                    .Count(differs => differs);
                firstDifference = $"frame {checkedFrames} ({ending.Stage.Phase}) pixel ({pixel % DisplayScreen.Width},{pixel / DisplayScreen.Width}): " +
                    $"live {actual[pixel]}, native {expected[pixel]}, {changed} pixels that frame; floor alone differs at {FloorOnly()} pixels, " +
                    $"sprites alone at {SpritesOnly()}; live floor subtracts sprites {projection.SubtractsSprites}, adds foreground {projection.AddsForeground}; " +
                    $"live layers {string.Join(", ", snapshot.Layers.ToArray().Select(layer => layer.GetType().Name))}; screen shades {screenShades}; sprite shadings {spriteShades}; ink trace {inkTrace}; native colour 10 of each sprite row {string.Join(" ", Enumerable.Range(0, 8).Select(row => snapshotColours[128 + row * 16 + 10]))}";
            }
            differences += changed;
            checkedFrames++;
            ending.Step();
        }
        Console.WriteLine($"Planet crossfade: {checkedFrames} frames, {differences} pixels differ from native wrapping/composition; "
            + $"{composedFloorFrames} frames drew the reconstruction's own floor, the rest the live floor that shows alike.");
        AssertEqual(0, differences, $"planet crossfade preserves native map wrap and main/subscreen composition (first: {firstDifference})");
        ShownFloorPairing? explosionFloors = null;
        int floorFrames = 0, drawnFrames = 0;
        for (int frame = 0; frame < 2048 && ending.Stage.Phase != EndingCreditsPhase.PlanetEscapeFast; frame++)
        {
            var live = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
            TurningFloorLayer[] liveFloors = live.LayersOf<TurningFloorLayer>();
            // The reconstruction changes only the turning floor: a frame that shows none is the same scene either way.
            if (liveFloors.Length > 0)
            {
                floorFrames++;
                RgbColor[] liveColours = EndingShownColours.Of(ending, shownShades);
                byte[] reconstructed = ending.ShownVram();
                nativeVram.AsSpan(0x8000).CopyTo(reconstructed.AsSpan(0x8000));
                // The frame carries no VRAM (#1403): the floor is drawn again from the native reconstruction. Where every
                // floor the frame shows shows at every floor point what the reconstruction's floor shows, the frames are
                // the same scene and draw the same pixels (#1568); only a frame whose floors differ is drawn both ways.
                ShownFloorPairing floors = explosionFloors is { } paired && paired.Pairs(liveFloors[0].Floor, reconstructed) ? paired
                    : new ShownFloorPairing(liveFloors[0].Floor, reconstructed, liveColours);
                explosionFloors = floors;
                if (!liveFloors.All(layer => floors.ShowAlike(layer.Floor, liveColours)))
                {
                    AffineFloor reconstructedFloor = floors.MemoryFloor(liveColours);
                    DisplayLayer[] reconstructedLayers = [.. live.Layers.ToArray().Select(layer => layer is TurningFloorLayer floor
                        ? (floor with { Floor = reconstructedFloor }) : layer)];
                    var nativePixels = DisplayRendering.Render(new LayeredScene(live.Paint, reconstructedLayers, live.Brightness));
                    differences += Differences(ending.Render(), nativePixels);
                    drawnFrames++;
                }
            }
            if (capture is not null && capture.Frames.Contains(frame))
            {
                // The native comparison replays exactly this sample's graphics and OAM under the retail registers.
                LayeredScene sample = EndingCreditsPresentation.CaptureRenderSnapshot(ending);
                capture.Snapshots[frame] = sample;
                var shownEnding = ending.ShownVram();
                capture.Shown[frame] = new CapturedColours(string.Join(" ", ending.screen.Colours.Shading.Shades.Select(shade => shade.Key)), EndingShownColours.Of(ending));
                File.WriteAllBytes(Path.Combine(capture.Directory, $"later-{frame:D4}-{ending.Stage.Phase}.ppu"),
                    NativeProbeInput(sample, EndingShownColours.Of(ending), shownEnding, ScreenSpritesFixtures.ToOam(EndingCreditsPresentation.PrepareSprites(ending, EndingCreditsPresentation.CurrentEscapeObjectSheet(ending))),
                        SuperMetroid.AssetExtraction.Frontend.Ending.EndingSheetObjectSelectionDefinitions.Of(EndingCreditsPresentation.CurrentEscapeObjectSheet(ending))));
            }
            ending.Step();
            audio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.PlanetEscapeFast, ending.Stage.Phase, "planet explosion reaches ship flyaway");
        Console.WriteLine($"Complete explosion: {differences} pixel mismatches against exact native upper-VRAM transfers; "
            + $"{floorFrames} frames showed a turning floor, {drawnFrames} of them drawn both ways where the floors did not show alike.");
        AssertEqual(0, differences, "explosion retains graphics outside the native font DMA extent");
        AssertEqual(0, memoryDifferences, "explosion setup executes exactly the native DMA byte ranges");
    }

    /// <summary>The number of pixels at which <paramref name="first"/> and <paramref name="second"/>, two frames, differ.</summary>
    private static int Differences(ReadOnlySpan<RgbaPixel> first, ReadOnlySpan<RgbaPixel> second)
    {
        AssertEqual(first.Length, second.Length, "compared frames' pixel counts");
        int count = 0;
        for (int pixel = 0; pixel < first.Length; pixel++)
        {
            if (first[pixel] != second[pixel])
                count++;
        }
        return count;
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

    /// <summary>
    /// The topmost sprite drawn at screen pixel (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="scene"/>: its
    /// sheet ink there, the ink's standard colour, and each shade of the draw that recolours the ink with the colour it gives,
    /// so a failed colour check names the shade that decides the pixel.
    /// </summary>
    internal static string SpriteInkTrace(LayeredScene scene, int x, int y)
    {
        var covering = new List<string>();
        ImageSpriteDraw[] draws = [.. scene.SpriteDraws.ToArray().OfType<ImageSpriteDraw>()];
        for (int index = 0; index < draws.Length; index++)
        {
            ImageSpriteDraw draw = draws[index];
            int localX = x - draw.Corner.X, localY = y - draw.Corner.Y;
            if (localX < 0 || localY < 0 || localX >= draw.Image.Size.X || localY >= draw.Image.Size.Y)
                continue;
            if ((draw.Mirror & SpriteMirror.Horizontal) != 0)
                localX = draw.Image.Size.X - 1 - localX;
            if ((draw.Mirror & SpriteMirror.Vertical) != 0)
                localY = draw.Image.Size.Y - 1 - localY;
            SpriteImage sheet = draw.Image.Sheet;
            SpriteImageInk pixelInk = sheet.Inks[(draw.Image.Corner.Y + localY) * sheet.Size.X + draw.Image.Corner.X + localX];
            if (pixelInk.Colour is not int ink)
            {
                covering.Add($"#{index} {sheet.Name} clear");
                continue;
            }
            RgbColor shown = sheet.StandardColours[ink];
            foreach (PictureShade shade in draw.Shading?.Shades ?? [])
            {
                if (sheet.Shades.TryGetValue(shade, out ShadeInks? inks) && inks.Inks.IndexOf(ink) is var at and >= 0)
                    shown = inks.Colours[at];
            }
            covering.Add($"#{index} {sheet.Name} colour {ink} {(pixelInk.Clear ? "clear" : shown.ToString())}"
                + $" (frame at {draw.Image.Corner.X},{draw.Image.Corner.Y} size {draw.Image.Size.X}x{draw.Image.Size.Y}, drawn at {draw.Corner.X},{draw.Corner.Y},"
                + $" mirror {draw.Mirror}, {(pixelInk.Blends ? "blending" : "opaque")}, shades [{string.Join(" ", (draw.Shading?.Shades ?? []).Select(shade => shade.Key))}])");
        }
        return covering.Count == 0 ? "no sprite" : string.Join("; ", covering);
    }
}
