using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Verifies the 180-frame post-credits wave's native scroll, additive artwork, and snapshot rendering.</summary>
internal static class EndingWavySamusTests
{
    /// <summary>Checks the wave lifecycle and compares its CPU composition with serialized GPU readback.</summary>
    /// <param name="device">Graphics device whose kind is reported with the verification result.</param>
    /// <param name="renderer">Renderer used to compare captured frame packets with the native composition.</param>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var installed = RepositoryInstallation.Installation;
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var maps = installed.LoadMaps();
        var scene = new EndingCreditsState(installed.OpenRuntimeAddressSpace(), new CartridgeAudioState(), 2, 0);
        scene.BindObjectArtwork(installed.LoadEndingObjectArt());
        scene.BindPaletteArtwork(installed.LoadEndingPalettes());
        scene.BindPaletteFxColors(maps.RoomPaletteFx);
        scene.BindEndingFont(maps.EndingFont);
        scene.BindEndingText(maps.EndingText);
        typeof(EndingCreditsState).GetMethod("LoadCreditsAndPostCreditsAssets", flags)!.Invoke(scene, null);
        typeof(EndingCreditsState).GetMethod("SetupPostCreditsBlank", flags)!.Invoke(scene, null);
        typeof(EndingCreditsState).GetProperty(nameof(scene.Phase))!.SetValue(scene, EndingCreditsPhase.PostCreditsShootingStars);
        typeof(EndingCreditsState).GetField("phaseTimer", flags)!.SetValue(scene, 1);
        typeof(EndingCreditsState).GetField("brightness", flags)!.SetValue(scene, (byte)15);
        Require(!scene.CaptureRenderSnapshot().Layers.ToArray().OfType<BgSubscreenAddRenderLayer>().Any(), "Wave must not start during the palette fade");
        scene.Step();
        Rgba32[]? previous = null;
        for (int age = 0; age < 180; age++)
        {
            Require(scene.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop, "Native wave lasts exactly the 180-frame hold");
            var snapshot = scene.CaptureRenderSnapshot();
            var wave = snapshot.Layers.ToArray().OfType<BgSubscreenAddRenderLayer>().Single();
            for (int line = 0; line < 224; line++)
            {
                Require(wave.Scrolls[line].X == NativeX(age, line), $"Native HDMA X age {age}, line {line}");
                Require(wave.Scrolls[line].Y == age * 2, $"Native vertical scroll age {age}");
            }
            if (age is 0 or 1 or 2 or 3 or 179)
            {
                var vram = new SnesVram(); vram.LoadBytes(0, snapshot.Memory.Vram);
                var cgram = new SnesCgram();
                for (int i = 0; i < 256; i++) cgram.SetColor(i, snapshot.Memory.Cgram[i]);
                var main = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, 0x4c00, 0x5000, 0, 0, 256, 224, 32, 32);
                var sub = SnesBgTilemapRenderer.Render2Bpp(vram, cgram, 0x2400, 0x2000, rowCount: 32, transparentColorZero: true);
                var expected = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
                var oam = new OamBuffer();
                oam.LoadUploadPayload(snapshot.Memory.Oam, snapshot.Memory.ModeledSpriteCount);
                SnesLayerCompositor.Composite(expected, SnesObjRenderer.Render(oam, vram, cgram, 0));
                int changed = 0;
                for (int y = 0; y < 224; y++)
                for (int x = 0; x < 256; x++)
                {
                    int index = y * 256 + x;
                    if (main[index].A == 0) continue; // CGADSUB enables BG2, not backdrop.
                    Rgba32 first = main[index], second = sub[((y + age * 2) & 255) * 256 + ((x + NativeX(age, y)) & 255)];
                    expected[index] = second.A == 0 ? first : new(Add(first.R, second.R), Add(first.G, second.G), Add(first.B, second.B));
                    if (expected[index] != first) changed++;
                }
                Require(changed > 0, "Native artwork must visibly add the blue transformation effect");
                var packet = new RenderFrameSnapshot(new(age + 1, 1, (ushort)age), snapshot);
                PixelComparison.Verify(packet, expected, scene.Render(), "Ending direct render matches native BG3 addition");
                var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                PixelComparison.Verify(restored, expected, renderer.RenderForReadback(restored), "Ending wave packet and GPU match native BG3 addition");
                PixelComparison.Verify(restored, expected, renderer.RenderForReadback(restored), "Repeated wave rendering does not advance simulation");
                if (age == 0)
                {
                    // Version 28 ends this uniform subscreen descriptor after its two
                    // object flags; it has no line-scroll-presence byte.
                    var uniform = new LayeredRenderSnapshot(snapshot.Memory,
                        [wave.MainCoverage!, new BgSubscreenAddRenderLayer(0x2400, 0x2000, wave.MainCoverage)], 0, 15);
                    byte[] oldBytes = RenderFrameSnapshotCodec.Serialize(new RenderFrameSnapshot(new(1, 1, 0), uniform));
                    Array.Resize(ref oldBytes, oldBytes.Length - 1);
                    oldBytes[8] = 28; oldBytes[9] = 0;
                    var oldPacket = RenderFrameSnapshotCodec.Deserialize(oldBytes);
                    PixelComparison.Verify(oldPacket, expected, renderer.RenderForReadback(oldPacket), "Version-28 uniform subscreen coverage remains compatible");
                    var supplied = wave.Scrolls.ToArray();
                    var owned = wave.WithScrolls(supplied);
                    supplied[0] = new(123, 123);
                    Require(owned.Scrolls[0] == new BackgroundLineScroll(0, 0), "Captured line scrolls own their input memory");
                }
                if (age == 3) Require(previous is not null && !expected.AsSpan().SequenceEqual(previous), "Wave changes visible pixels between consecutive frames");
                previous = expected;
            }
            scene.Step();
        }
        Require(scene.Phase == EndingCreditsPhase.PostCreditsWaitingSamus, "Result panel replaces the wave after its wait");
        Require(!scene.CaptureRenderSnapshot().Layers.ToArray().OfType<BgSubscreenAddRenderLayer>().Any(), "Result panel disables BG3 addition");
        Console.WriteLine($"{device.Kind}: ending wave native scrolls, moving blue pixels, BG2-only addition, 180-frame lifecycle and packet round trip passed.");

        ushort NativeX(int age, int line)
        {
            if (age < 2) return 0;
            int offset = (-2 + (age - 1) * 16 + (line % 64) * 4) & 0x1ff;
            short sample = unchecked((short)(rom.ReadCartridgeByte(0xa0b443 + offset) | rom.ReadCartridgeByte(0xa0b444 + offset) << 8));
            int displacement = Math.Abs((int)sample) * 0x4000 >> 16;
            if (sample < 0) displacement = -displacement;
            if ((line / 64 & 1) != 0) displacement = -displacement;
            return unchecked((ushort)displacement);
        }
        static byte Add(byte first, byte second) { int value = Math.Min(31, (first >> 3) + (second >> 3)); return (byte)((value << 3) | (value >> 2)); }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
    }
}
