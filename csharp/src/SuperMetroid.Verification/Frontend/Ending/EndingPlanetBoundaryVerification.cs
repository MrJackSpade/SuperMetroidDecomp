using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks ending planet VRAM transfers and final-scene rendering against cartridge data.</summary>
internal static class EndingPlanetBoundaryVerification
{
    /// <summary>Reconstructs native explosion DMA ranges and compares crossfade and flyaway pixels frame by frame.</summary>
    internal static void VerifyEndingPlanetBoundary()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var audio = AudioQueues.Standalone();
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
        while (ending.Stage.Phase is EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionCrossfade)
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
                string inkTrace = SpritePixelTrace(snapshot, pixel % DisplayScreen.Width, pixel / DisplayScreen.Width);
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

    /// <summary>Runs the planet VRAM and rendered-boundary comparison.</summary>
    [VerificationCommand("--ending-planet-boundary")]
    internal static int EndingPlanetBoundaryCommand()
    {
        Suite(nameof(VerifyEndingPlanetBoundary), () => VerifyEndingPlanetBoundary());
        return 0;
    }

    /// <summary>
    /// The topmost sprite drawn at screen pixel (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="scene"/>: its
    /// sheet pixel's colour key there and the colour the draw's shades show it in, and whether it blends,
    /// so a failed colour check names what decides the pixel.
    /// </summary>
    internal static string SpritePixelTrace(LayeredScene scene, int x, int y)
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
            int pixel = (draw.Image.Corner.Y + localY) * sheet.Size.X + draw.Image.Corner.X + localX;
            if (sheet.Pixels[pixel].A == 0)
            {
                covering.Add($"#{index} {sheet.Name} clear");
                continue;
            }
            ColourKey key = sheet.KeyAt(pixel);
            RgbColor shown = key.Colour;
            foreach (PictureShade shade in draw.Shading?.Shades ?? [])
            {
                if (!sheet.Shades.TryGetValue(shade, out PictureRecolouring? recolouring))
                    continue;
                foreach (KeyedSubstitution substitution in recolouring.Substitutions)
                {
                    if (substitution.Replaced == key)
                        shown = substitution.Shown;
                }
            }
            bool blends = draw.Image.Blends || (!sheet.Blending.IsEmpty && sheet.Blending[pixel]);
            covering.Add($"#{index} {sheet.Name} colour key {key} {shown}"
                + $" (frame at {draw.Image.Corner.X},{draw.Image.Corner.Y} size {draw.Image.Size.X}x{draw.Image.Size.Y}, drawn at {draw.Corner.X},{draw.Corner.Y},"
                + $" mirror {draw.Mirror}, {(blends ? "blending" : "opaque")}, shades [{string.Join(" ", (draw.Shading?.Shades ?? []).Select(shade => shade.Key))}])");
        }
        return covering.Count == 0 ? "no sprite" : string.Join("; ", covering);
    }
}
