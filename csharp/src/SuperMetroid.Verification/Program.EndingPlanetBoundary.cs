using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingNativePpu(bool offsetCheck = false, int frame = 512)
    {
        string prefix = $"csharp/test-temp/ending-506/later-{frame:D4}-ZebesExplosionAnimation";
        byte[] bgra = File.ReadAllBytes(prefix + (offsetCheck ? ".offset-check.bgra" : ".bgra"));
        AssertEqual(256 * 224 * 4, bgra.Length, "native PPU raster length");
        var native = new Rgba32[256 * 224];
        for (int i = 0; i < native.Length; i++) native[i] = new(bgra[i * 4 + 2], bgra[i * 4 + 1], bgra[i * 4]);
        var actual = SoftwareFrameSnapshotRenderer.Render(RenderFrameSnapshotCodec.Deserialize(File.ReadAllBytes(prefix + ".smframe")));
        PngWriter.WriteRgba(prefix + (offsetCheck ? ".offset-check.png" : ".native-ppu.png"), 256, 224, native);
        int differences = actual.Zip(native).Count(pair => pair.First != pair.Second);
        Console.WriteLine($"Native PPU finale frame {frame}: {differences} pixels differ.");
        AssertEqual(0, differences, "finale matches pinned native PPU with cartridge register setup");
    }

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
            var nativeMemory = new PpuMemorySnapshot(nativeVram, snapshot.Memory.Cgram, snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
            var expected = SoftwareLayeredSnapshotRenderer.Render(new(nativeMemory, nativeLayers,
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
            var live = ending.CaptureRenderSnapshot();
            byte[] reconstructed = live.Memory.Vram.ToArray();
            nativeVram.AsSpan(0x8000).CopyTo(reconstructed.AsSpan(0x8000));
            var nativeMemory = new PpuMemorySnapshot(reconstructed, live.Memory.Cgram, live.Memory.Oam, live.Memory.ModeledSpriteCount);
            var nativePixels = SoftwareLayeredSnapshotRenderer.Render(new(nativeMemory, live.Layers, live.ObjectSelection, live.Brightness));
            differences += ending.Render().Zip(nativePixels).Count(pair => pair.First != pair.Second);
            if (frame % 16 == 0)
            {
                PngWriter.WriteRgba($"csharp/test-temp/ending-506/later-{frame:D4}-{ending.Phase}.png", 256, 224, ending.Render());
                PngWriter.WriteRgba($"csharp/test-temp/ending-506/later-native-{frame:D4}-{ending.Phase}.png", 256, 224, nativePixels);
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
        Console.WriteLine($"Complete explosion: {differences} pixel mismatches against exact native upper-VRAM transfers.");
        AssertEqual(0, differences, "explosion retains graphics outside the native font DMA extent");
        AssertEqual(0, memoryDifferences, "explosion setup executes exactly the native DMA byte ranges");
    }
}
