using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Core.Game.SamusPaletteRomData;

internal static partial class Program
{
    private static (bool Body, bool Window) VerifyCrystalFlashVisualFrame(SuperMetroidRuntime runtime, int elapsed)
    {
        var samus = runtime.Samus!;
        var bus = runtime.AddressSpace;
        var packet = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        if (samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash)
        {
            // Independent timeline lookup: expand authored durations into a cycle
            // and index elapsed calls, rather than reading the production timers.
            int cycleLength = Enumerable.Range(0, CrystalFlash.BodyRecordCount)
                .Sum(i => Word(CrystalFlash.BodyRecords + i * CrystalFlash.BodyRecordByteCount + 2));
            int remaining = elapsed % cycleLength, record = 0;
            while (remaining >= Word(CrystalFlash.BodyRecords + record * CrystalFlash.BodyRecordByteCount + 2))
                remaining -= Word(CrystalFlash.BodyRecords + record++ * CrystalFlash.BodyRecordByteCount + 2);
            int body = Banks.Palette | Word(CrystalFlash.BodyRecords + record * CrystalFlash.BodyRecordByteCount);
            int bubble = Banks.Palette | Word(CrystalFlash.BubblePointers + elapsed / 5 % CrystalFlash.BubblePaletteCount * 2);
            for (int color = 0; color < CrystalFlash.BodyColorCount; color++)
                AssertEqual(Word(body + color * 2), packet.Memory.Cgram[CrystalFlash.BodyCgramStart + color],
                    $"runtime Crystal Flash body palette at frame {elapsed}");
            for (int color = 0; color < CrystalFlash.BubbleColorCount; color++)
                AssertEqual(Word(bubble + color * 2), packet.Memory.Cgram[CrystalFlash.BubbleCgramStart + color],
                    $"runtime Crystal Flash bubble palette at frame {elapsed}");
        }

        var pixels = SoftwareLayeredSnapshotRenderer.Render(packet);
        if (elapsed is 10 or 30 or 100 && Environment.GetEnvironmentVariable("SM_CRYSTAL_FLASH_CAPTURE") is { Length: > 0 } directory)
        {
            Directory.CreateDirectory(directory);
            PngWriter.WriteRgba(Path.Combine(directory, $"crystal-{elapsed:D3}.png"), 256, 224, pixels);
        }
        var colors = packet.Memory.Cgram.ToArray();
        Array.Clear(colors, CrystalFlash.BodyCgramStart, Common.ColorsPerObjPalette);
        var mutedMemory = new PpuMemorySnapshot(packet.Memory.Vram, colors, packet.Memory.Oam, packet.Memory.ModeledSpriteCount);
        var mutedBody = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(mutedMemory,
            packet.Layers, packet.ObjectSelection, packet.Brightness));
        int centerX = samus.XPosition - runtime.DisplayedGameplayPpu.Layer1XPosition;
        int centerY = samus.YPosition - runtime.DisplayedGameplayPpu.Layer1YPosition;
        AssertTrue(centerX is >= 32 and <= 223 && centerY is >= 64 and <= 191,
            "visual fixture keeps the entire Crystal Flash sprite in the viewport");
        bool bodyVisible = Enumerable.Range(centerY - 32, 64)
            .Any(y => Enumerable.Range(centerX - 32, 64)
                .Any(x => pixels[y * 256 + x] != mutedBody[y * 256 + x]));
        bool windowVisible = false;
        if (runtime.BombProjectiles.PowerBombExplosion.Phase is PowerBombExplosionPhase.CrystalFlashExplosion or PowerBombExplosionPhase.CrystalFlashAfterglow)
        {
            // Remove only the captured color window, retaining identical OAM,
            // palette, terrain and camera to isolate its actual visible contribution.
            var withoutWindow = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(packet.Memory,
                packet.Layers.ToArray().Where(layer => layer is not ScanlineColorAddRenderLayer).ToArray(),
                packet.ObjectSelection, packet.Brightness));
            AssertTrue(pixels.AsSpan(0, 32 * 256).SequenceEqual(withoutWindow.AsSpan(0, 32 * 256)),
                "Crystal Flash window preserves HUD pixels");
            windowVisible = !pixels.AsSpan().SequenceEqual(withoutWindow);
        }
        return (bodyVisible, windowVisible);

        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }
}
